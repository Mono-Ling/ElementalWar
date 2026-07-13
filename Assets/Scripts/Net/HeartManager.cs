using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Message;
using UnityEngine;

public class HeartManager : SingleMono<HeartManager>
{
    private const float DELAY_TIME = 5f;
    private CancellationTokenSource _cancel = new();
    // Start is called before the first frame update
    void Start()
    {
        Task.Run(HeartLoop);
    }
    private async void HeartLoop()
    {
        var package = new TcpPackage(new HeartMessage());
        while (!_cancel.IsCancellationRequested && NetManager.Instance.IsStart)
        {
            await Task.Delay((int)(1000 * DELAY_TIME));
            NetManager.Instance.Send(package);
        }
    }
}
