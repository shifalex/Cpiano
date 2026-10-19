using GestureSample.Maui.Models;

int checks = 0;
void Check(bool condition, string message)
{
    checks++;
    if (!condition) throw new Exception(message);
}

foreach (bool stageImmediate in new[] { false, true })
{
    Check(!GripAnswerTiming.AllowsImmediate(true, true, stageImmediate), "Hold option overrides stage immediate acceptance");
    Check(GripAnswerTiming.AllowsImmediate(true, false, stageImmediate), "Disabling hold permits immediate child answers");
    Check(GripAnswerTiming.AllowsImmediate(false, true, stageImmediate) == stageImmediate, "Other stages preserve their timing");
}
Check(!GripAnswerTiming.HasHeldLongEnough(null, 2), "An unstarted hold cannot submit");
Check(!GripAnswerTiming.HasHeldLongEnough(1.999, 2), "Correct grip must wait for full hold duration");
Check(GripAnswerTiming.HasHeldLongEnough(2, 2), "Full hold permits submission");
Check(!GripAnswerTiming.HasHeldLongEnough(0, 0), "Zero timer cannot bypass required hold");
Check(GripAnswerTiming.HasHeldLongEnough(1, 0), "Zero timer uses one second when hold required");

var gate = new GripInputGate();
int nudges = 0;
gate.PrematurePress += () => nudges++;
Check(gate.Phase == GripInputPhase.Watching, "Starts in demonstration phase");
Check(!gate.Press(1) && !gate.Press(2), "Early touches are rejected");
Check(nudges == 2, "Early touches request a visual nudge");
gate.SetBlocked(false);
Check(gate.Phase == GripInputPhase.Lift && !gate.CanPlay, "Held fingers prevent green light");
Check(!gate.Release(1) && !gate.CanPlay, "One release is not enough");
Check(!gate.Press(3), "New touches while lifting cannot start an answer");
Check(!gate.Release(2) && !gate.CanPlay, "All contacts, including new ones, must lift");
Check(!gate.Release(3) && gate.CanPlay, "Final release enables input without submitting a key-up");
Check(gate.Press(1) && gate.Press(2), "Fresh contacts can start an answer");
gate.SetBlocked(false);
Check(gate.CanPlay, "Repeated ready notification does not invalidate a legitimate hold");
gate.SetBlocked(true);
Check(!gate.CanPlay, "Next question blocks an existing hold");
gate.SetBlocked(false);
Check(gate.Phase == GripInputPhase.Lift, "Answer reset cannot erase physical hold");
gate.Release(1); gate.Release(2);
Check(gate.CanPlay, "Releasing previous answer enables next attempt");
Check(gate.Press(4) && gate.Release(4), "Normal release reaches answer handling");
gate.SetBlocked(true);
gate.Press(5); gate.Release(5);
Check(gate.Phase == GripInputPhase.Watching, "Releasing during demonstration does not prematurely show green");
gate.SetBlocked(false);
Check(gate.CanPlay, "No held contacts means immediate green after demonstration");
gate.Press(1); gate.SetBlocked(true); gate.ResetContacts();
Check(!gate.CanPlay, "Leaving the page clears contacts but does not end demonstration");
gate.SetBlocked(false);
Check(gate.CanPlay, "Returning does not wait for releases lost while navigating away");
// Recover when a native recognizer consumed the previous Up callback.
gate.SetBlocked(true);
gate.Press(7);
gate.SetBlocked(false);
Check(gate.Phase == GripInputPhase.Lift, "Lost Up leaves the gate waiting for release");
Check(!gate.Press(8), "Another finger joining the existing hold remains blocked");
Check(gate.Press(9, startsNewContactSequence: true), "A fresh physical touch sequence recovers a lost Up");
Check(gate.CanPlay && gate.Release(9), "Recovered sequence accepts a complete new answer");
Check(!gate.Release(7), "Late release from the rejected hold cannot submit an answer");
gate.SetBlocked(true);
gate.Press(1);
Check(!gate.Press(2, startsNewContactSequence: true), "Fresh sequence cannot bypass demonstration");
gate.SetBlocked(false);
Check(!gate.Release(2) && gate.CanPlay, "Pan completion releases the rejected hold");
Check(!gate.Release(2), "Up after pan completion cannot submit a phantom answer");

// Per-key callbacks may report a fresh sequence while other keys are active.
// Every accepted press must retain its release, in either release order.
foreach (bool reverseReleaseOrder in new[] { false, true })
{
    var playableGate = new GripInputGate();
    playableGate.SetBlocked(false);
    for (int attempt = 0; attempt < 3; attempt++)
    {
        Check(playableGate.Press(1, startsNewContactSequence: true), "First key is accepted");
        Check(playableGate.Press(2, startsNewContactSequence: true), "Additional key is accepted");
        Check(playableGate.Release(reverseReleaseOrder ? 2 : 1), "First released key must not stick");
        Check(playableGate.Release(reverseReleaseOrder ? 1 : 2), "Other released key must not stick");
    }
}

double now = 0;
var clock = new GripPresentationClock(() => now);
now = 300;
Check(clock.ElapsedMilliseconds == 300, "Presentation advances normally");
clock.Pause(1000);
now = 800;
Check(clock.ElapsedMilliseconds == 300, "A violation freezes presentation progress");
clock.Pause(1000);
now = 1600;
Check(clock.ElapsedMilliseconds == 300, "Repeated violations extend the same pause");
now = 1900;
Check(clock.ElapsedMilliseconds == 400, "Presentation resumes from the frozen position");
clock.Pause(1000);
now = 3000;
Check(clock.ElapsedMilliseconds == 500, "Later pauses exclude only their own duration");
for (int index = 0; index < PrecisionSignLearningIntro.Count; index++)
{
    var step = PrecisionSignLearningIntro.GetStep(index);
    int lowerTarget = step.LowerRow + (step.IsShift || step.BaseAtTop ? step.Delta : 0);
    int upperTarget = step.UpperRow + (step.IsShift || !step.BaseAtTop ? step.Delta : 0);
    Check(lowerTarget >= 0 && upperTarget < 5 && lowerTarget < upperTarget,
        $"Arrow lesson {index + 1} stays legal on the smallest supported keyboard");
    Check(step.IsShift == (index < 5), "Exactly the first five lessons move the whole grip");
    if (index < 5)
        Check(upperTarget - lowerTarget == step.UpperRow - step.LowerRow, "SHIFT preserves grip size");
    else
    {
        Check(step.BaseAtTop == (index >= 7), "Upper-only lessons precede lower-only lessons");
        bool expands = upperTarget - lowerTarget > step.UpperRow - step.LowerRow;
        Check(expands == (index is 5 or 7), "Each endpoint is taught expansion before contraction");
    }
}

for (int question = 0; question < 6; question++)
    Check(EasyGripIntro.Applies(true, question) == (question < 4) && !EasyGripIntro.Applies(false, question),
        "Exactly the first four easy questions use the introduction");
var introRandom = new Random(51);
for (int rows = 7; rows <= 12; rows++)
    for (int previous = 2; previous <= rows; previous++)
        foreach (bool grow in new[] { false, true })
            for (int sample = 0; sample < 20; sample++)
            {
                var pair = EasyGripIntro.ChooseSizes(introRandom, rows, grow, previous);
                Check(pair.Initial >= 2 && pair.Initial <= rows && pair.Target >= 2 && pair.Target <= rows,
                    "Intro grips fit the keyboard");
                Check(pair.Target - pair.Initial == (grow ? 1 : -1), "Transformation is exactly one row");
                Check(Math.Abs(pair.Initial - previous) <= 1, "Left-hand height changes at most one row between exercises");
            }
var motion = new PrecisionGlideMotion();
double graceNow = 1000;
var graceGate = new GripInputGate(() => graceNow);
graceGate.SetBlocked(false);
graceGate.Press(1);
graceGate.Press(2);
graceGate.RememberCorrectAnswerHold();
graceNow += 5000;
Check(graceGate.IgnorePreviousAnswerHold && !graceGate.ShowPreviousAnswerEye && graceGate.PreviousAnswerGraceRemaining == 0,
    "Feedback time does not consume grace or show an eye before the next combination");
graceGate.BeginPreviousAnswerGrace();
graceGate.SetBlocked(true);
Check(graceGate.PreviousAnswerGraceRemaining == 750, "Previous correct hold gets 750 ms grace");
graceNow += 749;
Check(graceGate.PreviousAnswerGraceRemaining == 1 && !graceGate.CanPlay, "Grace hides cues without accepting input");
graceNow += 1;
Check(graceGate.PreviousAnswerGraceRemaining == 0 && graceGate.ShowPreviousAnswerEye, "Eye appears at 750 ms only for a still-held old answer");
graceGate.BeginPreviousAnswerGrace();
graceGate.Release(1); graceGate.Release(2);
Check(graceGate.PreviousAnswerGraceRemaining == 0, "Releasing the old answer cancels its grace");
graceGate.SetBlocked(false);
graceGate.Press(1);
graceGate.RememberCorrectAnswerHold();
graceGate.BeginPreviousAnswerGrace();
graceGate.SetBlocked(true);
graceGate.Press(3);
Check(graceGate.PreviousAnswerGraceRemaining == 0, "A new premature touch does not inherit old-answer grace");
graceGate.ResetContacts();
graceGate.SetBlocked(false);
graceGate.Press(1);
graceGate.RememberCorrectAnswerHold();
graceGate.SetBlocked(true);
graceGate.BeginPreviousAnswerGrace();
graceNow += 749;
graceGate.BeginPreviousAnswerGrace();
Check(graceGate.PreviousAnswerGraceRemaining == 1, "Repeated visibility notifications cannot restart the grace period");
graceGate.Release(1);
graceNow += 1;
Check(!graceGate.ShowPreviousAnswerEye, "Releasing just before the deadline prevents the eye");
graceGate.BeginPreviousAnswerGrace();
graceNow += 1000;
Check(!graceGate.ShowPreviousAnswerEye, "A new combination without an old held answer never schedules an eye");
foreach (bool horizontal in new[] { false, true })
    for (int moving = 0; moving < 4; moving++)
        foreach (int direction in new[] { -1, 1 })
        {
            motion.Clear();
            for (int finger = 0; finger < 4; finger++) motion.Begin(finger, finger * 100, 80);
            for (int combination = 0; combination < 3; combination++)
            {
                for (int frame = 0; frame < 8; frame++)
                    Check(motion.Move(moving, horizontal ? direction * 5 : 0, horizontal ? 0 : direction * 5, out _),
                        "The same finger keeps gliding through successive combinations");
                double distance = direction * 40 * (combination + 1);
                Check(motion.Move(moving, 0, 0, out var actual) &&
                    actual == (moving * 100 + (horizontal ? distance : 0), 80 + (horizontal ? 0 : distance)),
                    "Restarted pan totals or stationary samples cannot snap the finger back");
                for (int finger = 0; finger < 4; finger++)
                    if (finger != moving)
                        Check(motion.Move(finger, 0, 0, out var stationary) && stationary == (finger * 100d, 80d),
                            "A moving finger cannot animate another finger's keys");
            }
            int lifted = (moving + 1) % 4;
            motion.End(lifted);
            Check(!motion.Move(lifted, 0, 20, out _), "A released finger cannot glide");
            Check(motion.Move(moving, 0, 5, out _), "Lifting another finger does not reset the moving finger");
            Check(!motion.Move(moving, double.NaN, 0, out _), "Invalid native movement is ignored");
            motion.Clear();
            Check(!motion.Move(moving, 0, 5, out _), "A new question discards old motion");
        }
foreach (bool vertical in new[] { false, true })
    foreach (bool rightHand in new[] { false, true })
        foreach (int direction in new[] { -1, 1 })
            foreach (bool lowerFirst in new[] { false, true })
            {
                int step = vertical ? 2 : 1;
                int handOffset = rightHand ? (vertical ? 1 : 10) : 0;
                int otherOffset = rightHand ? (vertical ? -1 : -10) : (vertical ? 1 : 10);
                int low = (vertical ? 4 : 3) + handOffset, high = low + step;
                var positions = new Dictionary<int, int> { [1] = low, [2] = high, [3] = low + otherOffset, [4] = high + otherOffset };
                var targets = new Dictionary<int, int>();
                var occupied = positions.Values.ToHashSet();
                bool SameHand(int from, int to) => vertical ? from % 2 == to % 2 : from / 10 == to / 10;
                foreach (int finger in lowerFirst ? new[] { 1, 2 } : new[] { 2, 1 })
                {
                    targets[finger] = (finger == 1 ? low : high) + direction * step;
                    foreach (var move in PrecisionGlideMoves.Plan(positions, targets, occupied, SameHand))
                    {
                        Check(!occupied.Contains(move.To), "Paired glides never overwrite another pressed finger");
                        occupied.Remove(move.From); occupied.Add(move.To);
                        positions[move.Origin] = move.To;
                    }
                }
                Check(positions[1] == low + direction * step && positions[2] == high + direction * step,
                    "Both adjacent fingers glide together regardless of callback order");
                Check(positions[3] == low + otherOffset && positions[4] == high + otherOffset,
                    "The two stationary fingers on the opposite hand remain unchanged");
                Check(occupied.Count == 4, "Paired motion preserves all four held contacts");
            }
var blockedPositions = new Dictionary<int, int> { [1] = 2, [2] = 4 };
Check(PrecisionGlideMoves.Plan(blockedPositions, new Dictionary<int, int> { [1] = 4 }, new[] { 2, 4 }, (_, _) => true).Count == 0,
    "A finger waiting for its partner cannot overwrite it");
Check(PrecisionGlideMoves.Plan(blockedPositions, new Dictionary<int, int> { [1] = 3 }, new[] { 2, 4 }, (a, b) => a % 2 == b % 2).Count == 0,
    "Deferred targets cannot cross to the other hand");
Check(PrecisionGlideMoves.Plan(blockedPositions, new Dictionary<int, int> { [9] = 6 }, new[] { 2, 4 }, (_, _) => true).Count == 0,
    "Released contacts cannot replay deferred moves");
Console.WriteLine($"Passed {checks} grip input and presentation checks.");
