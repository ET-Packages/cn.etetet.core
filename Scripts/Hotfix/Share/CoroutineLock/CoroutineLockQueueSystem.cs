﻿namespace ET
{
    [EntitySystemOf(typeof(CoroutineLockQueue))]
    public static partial class CoroutineLockQueueSystem
    {
        [EntitySystem]
        private static void Awake(this CoroutineLockQueue self, long type)
        {
            self.type = type;
        }
        
        [EntitySystem]
        private static void Destroy(this CoroutineLockQueue self)
        {
            self.queue.Clear();
            self.type = 0;
            self.CurrentCoroutineLock = null;
        }
        
        public static async ETTask<CoroutineLock> Wait(this CoroutineLockQueue self, int time)
        {
            if (self.CurrentCoroutineLock == null)
            {
                self.CurrentCoroutineLock = self.AddChild<CoroutineLock, long, long, int>(self.type, self.Id, 1, true);
                return self.CurrentCoroutineLock;
            }

            WaitCoroutineLock waitCoroutineLock = WaitCoroutineLock.Create();
            self.queue.Enqueue(waitCoroutineLock);
            if (time > 0)
            {
                long tillTime = TimeInfo.Instance.ClientFrameTime() + time;
                self.Root().GetComponent<TimerComponent>().NewOnceTimer(tillTime, TimerCoreInvokeType.CoroutineTimeout, waitCoroutineLock);
            }
            self.CurrentCoroutineLock = await waitCoroutineLock.Wait();
            return self.CurrentCoroutineLock;
        }

        // 返回值，是否找到了一个有效的协程锁
        public static bool Notify(this CoroutineLockQueue self, int level)
        {
            // 有可能WaitCoroutineLock已经超时抛出异常，所以要找到一个未处理的WaitCoroutineLock
            while (self.queue.Count > 0)
            {
                WaitCoroutineLock waitCoroutineLock = self.queue.Dequeue();

                if (waitCoroutineLock.IsDisposed())
                {
                    continue;
                }

                CoroutineLock coroutineLock = self.AddChild<CoroutineLock, long, long, int>(self.type, self.Id, level, true);

                waitCoroutineLock.SetResult(coroutineLock);
                return true;
            }
            return false;
        }
    }
}