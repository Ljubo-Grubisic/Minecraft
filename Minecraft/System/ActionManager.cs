using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft.System
{
    /// <summary>
    /// The manager calls functions on the main thread
    /// which is needed for OpenGL functions which have to be called on the main thread
    /// </summary>
    internal static class ActionManager
    {
        private static List<Queue<Action>> ActionsQueues = new List<Queue<Action>>(); 

        static ActionManager() 
        {
            for (int i = 0; i < (int)Thread.NumThreads; i++)
            {
                ActionsQueues.Add(new Queue<Action>());
            }
        }

        internal static void QueueAction(Thread thread, Action action)
        {
            lock (ActionsQueues[(int)thread])
                ActionsQueues[(int)thread].Enqueue(action);
        }

        internal static void InvokeActions(Thread thread, int numberOfActions)
        {
            for (int i = 0; i < numberOfActions; i++)
            {
                lock (ActionsQueues[(int)thread])
                    ActionsQueues[(int)thread].Dequeue().Invoke();
            }
        }

        internal static void InvokeActions(Thread thread)
        {
            for (int i = 0; i < ActionsQueues[(int)thread].Count; i++)
            {
                lock (ActionsQueues[(int)thread])
                    ActionsQueues[(int)thread].Dequeue().Invoke();
            }
        }

        internal enum Thread
        {
            Main,
            ChunkManager,
            NumThreads
        }
    }
}
