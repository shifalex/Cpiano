using GestureSample.Maui.Models;

int checks = 0;
void Check(bool condition, string message)
{
    checks++;
    if (!condition) throw new Exception(message);
}

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
Console.WriteLine($"Passed {checks} grip input and presentation checks.");
