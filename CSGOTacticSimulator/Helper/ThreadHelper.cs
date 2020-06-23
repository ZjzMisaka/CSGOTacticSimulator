using System.Collections.Generic;
using System.Threading;

#pragma warning disable CS0618

namespace CSGOTacticSimulator.Helper
{
    static public class ThreadHelper
    {
        static private List<Thread> listThread = new List<Thread>();

        static public void StopAllThread()
        {
            for (int i = listThread.Count - 1; i >= 0; --i)
            {
                if (!((listThread[i].ThreadState & (ThreadState.Suspended | ThreadState.WaitSleepJoin)) == 0))
                {
                    listThread[i].Resume();
                }
                listThread[i].Abort();
            }
            listThread.Clear();
        }

        static public void PauseAllThread()
        {
            for (int i = listThread.Count - 1; i >= 0; --i)
            {
                if (listThread[i].IsAlive)
                {
                    listThread[i].Suspend();
                }
            }
        }

        static public void RestartAllThread()
        {
            for (int i = listThread.Count - 1; i >= 0; --i)
            {
                if (listThread[i].IsAlive && listThread[i].ThreadState == ThreadState.Suspended)
                {
                    listThread[i].Resume();
                }
            }
        }

        static public void AddThread(Thread thread)
        {
            listThread.Add(thread);
        }

    }
}
