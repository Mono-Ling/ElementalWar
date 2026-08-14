using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using Google.Protobuf;
using Server.Event;
using Server.GamePlay.StateTransfer;
using Space;

namespace Server.GamePlay
{
    public class PlayerStateTransfer : IDisposable
    {
        private const int UPDATE_DELTA_TIMR = 10;// ms

        private Dictionary<Type, StateSynReceiveEvent> _stateSynEventDic = new();
        private List<BaseTransfer> _stateTransferList = new();
        private CancellationTokenSource _cancel = new();
        public PlayerStateTransfer(List<int> playerIdList)
        {
            _stateTransferList.Add(new PlayerPositionStateTransfer());
            _stateTransferList.Add(new SpaceStateTransfer());

            _stateTransferList.Add(new PlayerStateTransmitTransfer());

            _stateTransferList.Add(new DynamicSceneItemTransfer());
            _stateTransferList.Add(new GrenadePositionTransfer());

            _stateTransferList.Add(new DynamicTextUITransfer());

            foreach (var stateTransfer in _stateTransferList)
                stateTransfer.Start(this,playerIdList);

            Task.Run(Update);
            EventBus.Instance.AddListener<ClientPackage>(EventType.OnReceive, OnStateSynMessageReceive);
        }
        private async Task Update()
        {
            while (!_cancel.IsCancellationRequested)
            {
                await Task.Delay(UPDATE_DELTA_TIMR);
                try
                {
                    foreach (var stateTransfer in _stateTransferList)
                        stateTransfer.Update();
                }
                catch (Exception e)
                { Console.WriteLine(e.ToString()); }
            }
        }
        public void Dispose()
        {
            foreach (var stateTransfer in _stateTransferList)
                stateTransfer.Stop();

            EventBus.Instance.RemoveListener<ClientPackage>(EventType.OnReceive, OnStateSynMessageReceive);
            _cancel.Cancel();
        }
        public void AddListener<T>(Action<ClientPackage> action) where T : IMessage
            => AddListener(typeof(T), action);
        public void AddListener(Type type, Action<ClientPackage> action)
        {
            if (_stateSynEventDic.TryGetValue(type, out var synEvent))
                synEvent.action += action;
            else
            {
                StateSynReceiveEvent newSynEvent = new();
                newSynEvent.action += action;
                _stateSynEventDic.Add(type, newSynEvent);
            }
        }
        public void RemoveListener<T>(Action<ClientPackage> action) where T : IMessage
            => RemoveListener(typeof(T), action);
        public void RemoveListener(Type type, Action<ClientPackage> action)
        {
            if (_stateSynEventDic.TryGetValue(type, out var synEvent))
                synEvent.action -= action;
            else
                Console.WriteLine($"【网络玩家中转】不存在{type}消息的监听");
        }
        private void OnStateSynMessageReceive(ClientPackage package)
        {
            if (package.sendType != SendType.Udp || package.message == null)
                return;
            IMessage message = package.message;
            if (_stateSynEventDic.TryGetValue(message.GetType(), out var baseEvent))
            {
                baseEvent.Trigger(package);
            }
            //else
            //    Console.WriteLine($"【网络玩家中转】不存在{message.GetType()}消息的监听");
        }
    }
    public class StateSynReceiveEvent
    {
        public event Action<ClientPackage>? action;

        public void Trigger(ClientPackage message)
        {
            action?.Invoke(message);
        }
    }
}
