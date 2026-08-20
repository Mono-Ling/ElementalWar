using System;
using System.Collections.Generic;
using System.Text;
using Message;

namespace Server.GamePlay.StateTransfer.Hit.TriggerItem
{
    public class ElementCrystalTriggerItem : TriggerItem
    {
        public int elementType;
        public override bool InitTriggerItem(DynamicSceneItem item, ITriggerItemInitMessage message)
        {
            if(!base.InitTriggerItem(item, message))
                return false;
            isEnable = false;
            if (message is not ElementCrystalInitMessage eciMes)
                return false;
            elementType = eciMes.ElementType;
            isEnable = true;
            return true;
        }
        public override void OnTrigger()
        {
            base.OnTrigger();
            isEnable = false;
        }
    }
}
