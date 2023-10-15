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
        internal static Queue<Action> Actions = new Queue<Action>();

        internal static void QueueAction(Action action)
        {
            lock (Actions)
                Actions.Enqueue(action);
        }

        internal static void InvokeActions(int numberOfActions)
        {
            for (int i = 0; i < numberOfActions; i++)
            {
                lock (Actions)
                    Actions.Dequeue().Invoke();
            }
        }

        internal static void InvokeActions()
        {
            for (int i = 0; i < Actions.Count; i++)
            {
                lock (Actions)
                    Actions.Dequeue().Invoke();
            }
        }
    }
}
