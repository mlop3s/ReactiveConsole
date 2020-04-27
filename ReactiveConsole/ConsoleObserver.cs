using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ReactiveConsole
{
    class ConsoleObserver<T> : IObserver<T>
    {
        string _name;

        public ConsoleObserver(string name = "")
        {
            _name = name;
        }

        public void OnCompleted()
        {
            Console.WriteLine("[{0}]{1} - OnCompleted()", Thread.CurrentThread.ManagedThreadId, _name);
        }

        public void OnError(Exception error)
        {
            Console.WriteLine("[{0}]{1} - OnError:", Thread.CurrentThread.ManagedThreadId, _name);
            Console.WriteLine("\t [{0}]{1}", Thread.CurrentThread.ManagedThreadId, error);
        }

        public void OnNext(T value)
        {
            Console.WriteLine("[{0}]{1} - OnNext({2})", Thread.CurrentThread.ManagedThreadId,  _name, value);
        }
    }
}
