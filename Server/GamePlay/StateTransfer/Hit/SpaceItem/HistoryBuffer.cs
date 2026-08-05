using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Server.GamePlay.StateTransfer
{
    /// <summary>
    /// 环形历史缓冲区，支持非连续帧（tick 跳跃）的写入与查询
    /// </summary>
    public class HistoryBuffer<T> where T : struct
    {
        /// <summary>
        /// 哨兵值，标记槽位从未被写入
        /// </summary>
        private const long INVALID_TICK = long.MinValue;

        private struct BufferItem
        {
            public long tick;
            public T info;
        }

        public int WindowSize => _capacity;

        private readonly int _capacity;
        private readonly BufferItem[] _buffer;

        /// <summary>最新写入的 tick</summary>
        private long _newestTick;

        /// <summary>已成功写入的次数（用于判断缓冲区是否已满）。</summary>
        private int _writeCount;

        private readonly object _lock = new();

        public HistoryBuffer(int windowSize)
        {
            _capacity = windowSize;
            _buffer = new BufferItem[windowSize];
            _newestTick = INVALID_TICK;

            // 初始化哨兵值，避免 default(tick=0) 与真实的 tick=0 数据混淆
            for (int i = 0; i < windowSize; i++)
                _buffer[i].tick = INVALID_TICK;
        }

        /// <summary>
        /// 检查 tick 是否在缓冲区的有效时间范围内。
        /// 注意：返回 true 不保证数据一定存在（可能该 tick 未被写入或已被覆盖），
        /// 仅为快速路径的粗略过滤。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEffective(long tick)
        {
            if (_newestTick == INVALID_TICK)
                return false;
            return tick <= _newestTick;
        }

        /// <summary>
        /// 写入一帧数据。非连续帧安全：tick 可以跳跃，按 index = tick % capacity 定位槽位。
        /// </summary>
        public void Add(long tick, T info)
        {
            lock (_lock)
            {
                int index = (int)(tick % _capacity);
                _buffer[index] = new BufferItem { tick = tick, info = info };

                if (tick > _newestTick)
                    _newestTick = tick;
                _writeCount++;
            }
        }

        /// <summary>
        /// 精确查询指定 tick 的数据。非连续帧安全。
        /// </summary>
        public bool TryGet(long tick, out T info)
        {
            lock (_lock)
            {
                info = default;

                if (!IsEffective(tick))
                    return false;

                int index = (int)(tick % _capacity);

                if (_buffer[index].tick != tick)
                    return false;

                info = _buffer[index].info;
                return true;
            }
        }

        /// <summary>
        /// 对指定 tick 进行线性插值查询。
        /// 在已存储的帧数据中找到距离 target tick 最近的两个数据点，按比例插值。
        /// 非连续帧安全。
        /// </summary>
        public bool TryGetLerp(long tick, out T info, Func<T, T, float, T> lerp)
        {
            lock (_lock)
            {
                info = default;

                if (!TryGetNear(tick, out long leftTick, out long rightTick,
                                out T leftInfo, out T rightInfo))
                    return false;

                if (leftTick == rightTick)
                {
                    info = leftInfo;
                    return true;
                }

                float t = (float)(tick - leftTick) / (float)(rightTick - leftTick);
                t = Math.Clamp(t, 0f, 1f);
                info = lerp(leftInfo, rightInfo, t);
                return true;
            }
        }

        /// <summary>
        /// 在缓冲区中找到距离目标 tick 最近的左右两个已存储数据点。
        /// 算法：线性扫描所有有效槽位，记录 ≤tick 的最大 tick（左边界）
        /// 和 ≥tick 的最小 tick（右边界）。O(capacity)，对于帧历史缓冲区的
        /// 边界情况：
        /// - tick 在所有已存储数据之前 → left=right=最旧数据
        /// - tick 在所有已存储数据之后 → left=right=最新数据
        /// - 恰好命中已存储 tick → left=right=该 tick
        /// </summary>
        private bool TryGetNear(long tick, out long leftTick, out long rightTick,
                                out T leftInfo, out T rightInfo)
        {
            leftTick = rightTick = INVALID_TICK;
            leftInfo = rightInfo = default;

            long bestLeftTick = INVALID_TICK;  // ≤ tick 的最大已存储 tick
            long bestRightTick = long.MaxValue; // ≥ tick 的最小已存储 tick
            int leftIdx = -1;
            int rightIdx = -1;

            for (int i = 0; i < _capacity; i++)
            {
                long slotTick = _buffer[i].tick;
                if (slotTick == INVALID_TICK)
                    continue;

                if (slotTick <= tick && slotTick > bestLeftTick)
                {
                    bestLeftTick = slotTick;
                    leftIdx = i;
                }
                if (slotTick >= tick && slotTick < bestRightTick)
                {
                    bestRightTick = slotTick;
                    rightIdx = i;
                }
            }

            if (leftIdx < 0 && rightIdx < 0)
                return false;

            // 边界回退：若一侧缺失，用另一侧补齐（同值插值 = 直接取值）
            if (leftIdx < 0) leftIdx = rightIdx;
            if (rightIdx < 0) rightIdx = leftIdx;

            leftTick = _buffer[leftIdx].tick;
            rightTick = _buffer[rightIdx].tick;
            leftInfo = _buffer[leftIdx].info;
            rightInfo = _buffer[rightIdx].info;
            return true;
        }
    }
}
