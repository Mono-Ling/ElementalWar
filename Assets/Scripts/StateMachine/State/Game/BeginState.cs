using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBeginState", menuName = "StateMachine/State/Game/BeginState")]
public class BeginState : State
{
    public override void OnEnter(Blackboard blackboard)
    {
        blackboard.SetValue("IsMatch", false);
        if (!UIManager.Instance.TryGetCurrentPanel<BeginPanel>(out var panel))
            panel = UIManager.Instance.ShowPanel<BeginPanel>(isAnimation: false);
        panel.SetBlackboard(blackboard);
    }
}
