using System.Collections.Generic;
using System;

namespace KobGamesSDKSlim
{
    // 
    // No need anymore since Firebase v6.0.0 added ContinueWithOnMainThread
    //

    //public class DoOnMainThread : Singleton<DoOnMainThread>
    //{
    //    private static readonly Queue<Action> tasks = new Queue<Action>();

    //    public void Init()
    //    {
    //    }

    //    void Update()
    //    {
    //        this.HandleTasks();
    //    }

    //    void HandleTasks()
    //    {
    //        while (tasks.Count > 0)
    //        {
    //            Action task = null;

    //            lock (tasks)
    //            {
    //                if (tasks.Count > 0)
    //                {
    //                    task = tasks.Dequeue();
    //                }
    //            }

    //            task.InvokeSafe();
    //        }
    //    }

    //    public static void QueueOnMainThread(Action task)
    //    {
    //        lock (tasks)
    //        {
    //            tasks.Enqueue(task);
    //        }
    //    }
    //}
}
