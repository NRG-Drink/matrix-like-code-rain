namespace NRG.Matrix.Models;

public class KeyInputHandler
{
    private Dictionary<Func<ConsoleKeyInfo, bool>, Action<ConsoleKeyInfo>> _keyHandlers = [];

    public KeyInputGroup AddGroup(Func<ConsoleKeyInfo, bool> groupFunc)
    {
        return new(this, groupFunc);
    }

    public KeyInputHandler AddHandler(
        Func<ConsoleKeyInfo, bool> shouldTriggerPredicate,
        Action<ConsoleKeyInfo> execute)
    {
        _keyHandlers.Add(shouldTriggerPredicate, execute);
        return this;
    }

    public bool ExecuteFirstMatchingHandler(ConsoleKeyInfo key)
    {
        foreach (var handler in _keyHandlers)
        {
            if (handler.Key.Invoke(key))
            {
                handler.Value.Invoke(key);
                return true;
            }
        }

        return false;
    }
}

public class KeyInputGroup(
    KeyInputHandler handler,
    Func<ConsoleKeyInfo, bool> groupFunc)
{
    private Action<ConsoleKeyInfo> _startAction = e => { };
    private Action<ConsoleKeyInfo> _finishAction = e => { };

    public KeyInputGroup StartAction(Action<ConsoleKeyInfo> startAction)
    {
        _startAction = startAction;
        return this;
    }

    public KeyInputHandler CloseGroup(Action<ConsoleKeyInfo>? finisAction = null)
    {
        if (finisAction is not null)
        {
            _finishAction = finisAction;
        }

        return handler;
    }

    public KeyInputGroup AddHandler(
        Func<ConsoleKeyInfo, bool> shouldTriggerPredicate,
        Action<ConsoleKeyInfo> execute)
    {
        handler.AddHandler(e => groupFunc(e) && shouldTriggerPredicate(e), e => { _startAction(e); execute(e); _finishAction(e); });
        return this;
    }
}
