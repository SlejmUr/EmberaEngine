using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Core
{
    public static class MainThreadDispatcher
    {
        private static readonly ConcurrentQueue<Action> actions = new();

        public static void Queue(Action action) => actions.Enqueue(action);

        public static void ExecutePending()
        {
            while (actions.TryDequeue(out var action))
                action();
        }
    }
}