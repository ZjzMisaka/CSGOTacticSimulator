using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

#pragma warning disable CS0618

namespace CSGOTacticSimulator.Helper
{
    static public class ThreadHelper
    {
        static public CancellationTokenSource heigherKillPriorityCts;
        static public CancellationTokenSource lowerKillPriorityCts;
        static public CancellationTokenSource cts;
        static public ManualResetEvent manualEvent = new ManualResetEvent(true);
        static private List<Task> heigherKillPriorityTaskList = new List<Task>();
        static private List<Task> lowerKillPriorityTaskList = new List<Task>();
        static private List<Task> taskList = new List<Task>();

        public enum KillPriority { Default, Heigher, Lower };

        static public async Task<bool> StopAllThread()
        {
            if (heigherKillPriorityCts != null && !heigherKillPriorityCts.IsCancellationRequested)
            {
                heigherKillPriorityCts.Cancel();

                // Task.WaitAll(heigherKillPriorityTaskList.ToArray());
                await Task.WhenAll(heigherKillPriorityTaskList);

                heigherKillPriorityTaskList.Clear();
            }

            if (cts != null && !cts.IsCancellationRequested)
            {
                cts.Cancel();

                // Task.WaitAll(taskList.ToArray());
                await Task.WhenAll(taskList);

                taskList.Clear();
            }
            if (lowerKillPriorityCts != null && !lowerKillPriorityCts.IsCancellationRequested)
            {
                lowerKillPriorityCts.Cancel();

                // Task.WaitAll(lowerKillPriorityTaskList.ToArray());
                await Task.WhenAll(lowerKillPriorityTaskList);

                lowerKillPriorityTaskList.Clear();
            }

            return true;
        }

        static public void PauseAllThread()
        {
            manualEvent.Reset();
        }

        static public void RestartAllThread()
        {
            manualEvent.Set();
        }

        static public void AddThread(Task task, KillPriority priority = KillPriority.Default)
        {
            if (priority == KillPriority.Default)
            {
                taskList.Add(task);
            }
            else if (priority == KillPriority.Heigher)
            {
                heigherKillPriorityTaskList.Add(task);
            }
            else if (priority == KillPriority.Lower)
            {
                lowerKillPriorityTaskList.Add(task);
            }
        }

        static public void ReNewCancellationTokenSource()
        {
            cts = new CancellationTokenSource();
            heigherKillPriorityCts = new CancellationTokenSource();
            lowerKillPriorityCts = new CancellationTokenSource();
        }

        static public bool CheckIsCancellationRequested(KillPriority priority = KillPriority.Default)
        {
            if (priority == KillPriority.Heigher)
            {
                return heigherKillPriorityCts.IsCancellationRequested;
            }
            else if (priority == KillPriority.Lower)
            {
                return lowerKillPriorityCts.IsCancellationRequested;
            }

            return cts.IsCancellationRequested;
        }

        static public CancellationToken GetToken(KillPriority priority = KillPriority.Default)
        {
            if (priority == KillPriority.Heigher)
            {
                return heigherKillPriorityCts.Token;
            }
            else if (priority == KillPriority.Lower)
            {
                return lowerKillPriorityCts.Token;
            }

            return cts.Token;
        }
    }
}
