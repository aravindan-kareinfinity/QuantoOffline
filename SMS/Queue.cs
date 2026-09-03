using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Quanto.SMS
{
    public class MessageQueue<T>
    {
        readonly int _Size = 0;
        readonly Queue<T> _Queue = new Queue<T>();
        readonly object _Key = new object();
        bool _Quit = false;
        private static MessageQueue<T> tradeQueue = null;

        public MessageQueue(int size)
        {
            _Size = size;
        }

        public static MessageQueue<T> Current
        {
            get
            {
                if (tradeQueue != null) return tradeQueue;
                int maxqueuecount = maxqueuecount = 500000;
                tradeQueue = new MessageQueue<T>(maxqueuecount);
                return tradeQueue;
            }
        }

        public T[] GetAll()
        {
            return _Queue.ToArray();
        }
        public void Quit()
        {
            lock (_Key)
            {
                _Quit = true;

                Monitor.PulseAll(_Key);
            }
        }

        public bool Enqueue(T t)
        {
            lock (_Key)
            {
                while (!_Quit && _Queue.Count >= _Size)
                {
                    Monitor.Wait(_Key);
                }

                if (_Quit) return false;

                _Queue.Enqueue(t);

                Monitor.PulseAll(_Key);
            }

            return true;
        }

        public bool Dequeue(out T t)
        {
            t = default(T);

            lock (_Key)
            {
                while (!_Quit && _Queue.Count == 0) Monitor.Wait(_Key);

                if (_Queue.Count == 0) return false;

                t = _Queue.Dequeue();

                Monitor.PulseAll(_Key);
            }

            return true;
        }
    }
}
