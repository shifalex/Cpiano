namespace GestureSample.Maui.Models;

public enum GripInputPhase { Watching, Lift, Ready }

/// <summary>Tracks physical contacts independently of the answer, which resets between questions.</summary>
public sealed class GripInputGate
{
    private readonly HashSet<int> _contacts = new();
    private bool _blocked = true;
    private bool _awaitingRelease;
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
        if (_contacts.Count == 0 && _awaitingRelease)
        {
            _awaitingRelease = false;
            Changed?.Invoke();
        }
        return accepted;
    }

    public void ResetContacts()
    {
        _contacts.Clear();
        _awaitingRelease = false;
        Changed?.Invoke();
    }
}
