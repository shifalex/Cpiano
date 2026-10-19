namespace GestureSample.Maui.Models;

public enum GripInputPhase { Watching, Lift, Ready }

/// <summary>Tracks physical contacts independently of the answer, which resets between questions.</summary>
public sealed class GripInputGate
{
    private readonly HashSet<int> _contacts = new();
    private bool _blocked = true;
    private bool _awaitingRelease;
    private readonly Func<double> _now;
    private double _previousAnswerGraceUntil;
    private readonly HashSet<int> _previousAnswerContacts = new();
    private bool _previousAnswerGraceStarted;
    public GripInputGate(Func<double>? now = null) => _now = now ??
        (() => System.Diagnostics.Stopwatch.GetTimestamp() * 1000d / System.Diagnostics.Stopwatch.Frequency);
    public bool HasPreviousAnswerHold => _previousAnswerContacts.Count > 0;
    public bool IgnorePreviousAnswerHold => HasPreviousAnswerHold &&
        (!_previousAnswerGraceStarted || PreviousAnswerGraceRemaining > 0);
    public bool ShowPreviousAnswerEye => HasPreviousAnswerHold &&
        _previousAnswerGraceStarted && PreviousAnswerGraceRemaining <= 0;
    public double PreviousAnswerGraceRemaining => !HasPreviousAnswerHold || !_previousAnswerGraceStarted ? 0 :
        Math.Max(0, _previousAnswerGraceUntil - _now());
    public void RememberCorrectAnswerHold()
    {
        _previousAnswerContacts.Clear();
        _previousAnswerContacts.UnionWith(_contacts);
        _previousAnswerGraceStarted = false;
    }
    public void BeginPreviousAnswerGrace()
    {
        if (_previousAnswerGraceStarted || !HasPreviousAnswerHold) return;
        _previousAnswerGraceStarted = true;
        _previousAnswerGraceUntil = _now() + 750;
    }
    public GripInputPhase Phase => _blocked ? GripInputPhase.Watching
        : _awaitingRelease ? GripInputPhase.Lift : GripInputPhase.Ready;
    public bool CanPlay => Phase == GripInputPhase.Ready;
    public event Action? Changed;
    public event Action? PrematurePress;

    public void SetBlocked(bool blocked)
    {
        if (blocked || _blocked) _awaitingRelease = _contacts.Count > 0;
        _blocked = blocked;
        Changed?.Invoke();
    }

    public bool Press(int key, bool startsNewContactSequence = false)
    {
        if (!_contacts.Contains(key))
        {
            _previousAnswerContacts.Clear();
            _previousAnswerGraceUntil = 0;
        }
        // Native gesture cancellation can lose a key's Up event. A Down whose
        // complete touch set is new proves the previous physical hold has ended.
        // Recovery is only for rejected holds. Never discard playable contacts:
        // a later key's Down must not make an earlier key's Up disappear.
        if (startsNewContactSequence && !CanPlay && _contacts.Count > 0) ResetContacts();
        _contacts.Add(key);
        if (CanPlay) return true;
        _awaitingRelease = true;
        PrematurePress?.Invoke();
        return false;
    }

    public bool Release(int key)
    {
        bool accepted = CanPlay && _contacts.Contains(key);
        _contacts.Remove(key);
        bool previousReleased = _previousAnswerContacts.Remove(key) && !HasPreviousAnswerHold;
        if (_contacts.Count == 0 && _awaitingRelease)
        {
            _awaitingRelease = false;
            Changed?.Invoke();
        }
        else if (previousReleased) Changed?.Invoke();
        return accepted;
    }

    public void ResetContacts()
    {
        _previousAnswerGraceUntil = 0;
        _previousAnswerContacts.Clear();
        _previousAnswerGraceStarted = false;
        _contacts.Clear();
        _awaitingRelease = false;
        Changed?.Invoke();
    }
}
