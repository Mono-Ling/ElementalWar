using System;
using System.Collections.Generic;
using System.Text;

namespace Server.GamePlay.StateTransfer.SpaceTransfer
{
    public static class Tools
    {
        public static bool TryGet<T,W>(this PriorityQueue<T,W> queue,out T? item,object lockObj)
        {
            lock (lockObj)
                return queue.TryDequeue(out item, out _);
        }
    }
}
