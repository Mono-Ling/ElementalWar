using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class ThrowStateSynSend : BaseSynSend
{
    private BlackboardArg<bool> _isThrowArg;
    private BlackboardArg<bool> _isThrowFireArg;
    private ThrowStateMessage _message = new();
    public override void Init(Blackboard blackboard)
    {
        base.Init(blackboard);
        SetHeader(true);
        blackboard.GetBlackboardArg<bool>("IsThrow", out _isThrowArg);
        blackboard.GetBlackboardArg<bool>("IsThrowFire", out _isThrowFireArg);
        _isThrowArg.OnValueChange += OnIsThrowChanged;
        _isThrowFireArg.OnValueChange += OnIsThrowFireChanged;
        OnThrowChanged(_isThrowArg.value, _isThrowFireArg.value);
    }
    private void OnIsThrowChanged(bool isThrow)
    => OnThrowChanged(isThrow, _isThrowFireArg.value);
    private void OnIsThrowFireChanged(bool isThrowFire)
    => OnThrowChanged(_isThrowArg.value, isThrowFire);
    private void OnThrowChanged(bool isThrow, bool isThrowFire)
    {
        _message.IsThrow = isThrow;
        _message.IsThrowFire = isThrowFire;
        Send(_message);
    }
    public override void OnRemove()
    {
        _isThrowArg.OnValueChange -= OnIsThrowChanged;
        _isThrowFireArg.OnValueChange -= OnIsThrowFireChanged;
    }
}
