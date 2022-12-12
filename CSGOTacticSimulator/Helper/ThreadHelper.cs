using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CS0618

namespace CSGOTacticSimulator.Helper
{
    static public class ThreadHelper
    {
        static public CancellationTokenSource cts;
        static public ManualResetEvent manualEvent = new ManualResetEvent(true);
        static private List<Task> listTask = new List<Task>();

        static public void StopAllThread()
        {
            if (cts != null && !cts.IsCancellationRequested)
            {
                cts.Cancel();
            }
        }

        static public void PauseAllThread()
        {
            manualEvent.Reset();
        }

        static public void RestartAllThread()
        {
            manualEvent.Set();
        }

        static public void AddThread(Task task)
        {
            listTask.Add(task);
        }

    }
}
