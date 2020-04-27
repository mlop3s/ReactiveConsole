using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace ReactiveConsole
{
//    The Observer pattern was introduced by the Gang of Four(GoF) in Design Patterns:
//Elements of Reusable Object-Oriented Software(Addison-Wesley Professional, 1994).
//The pattern defines two components: subject and observer(not to be confused with
//IObserver of Rx). The observer is the participant that’s interested in an event and
//subscribes itself to the subject that raises the events.

    class Program
    {
        static void Main(string[] args)
        {
            //OwnImplementation();
            //UsualImplementation();
            //FromEnumerable();
            //ReadFile();
            //Explicit();
            //OnlyOdds();
            //MultiThreaded();
            Synchronization();

            Console.ReadKey();
        }

        private static void OwnImplementation()
        {
            var numbers = new NumbersObservable(5);
            var subscription =
            numbers.Subscribe(new ConsoleObserver<int>("numbers"));
        }

        private static void OnlyOdds()
        {
            Enumerable.Range(0, 10)
                .ToObservable()
                .Where(x => x % 2 > 0)
                .PairWithPrevious()
                .Skip(1)
                .Select(pair => pair.previous + pair.current)
                .Subscribe(new ConsoleObserver<int>("odds"));
        }

        private static void UsualImplementation()
        {
            IObservable<int> observable =
             Observable.Generate(
             0, //Initial state
             i => i < 10, //Condition (false means terminate)
             i => i + 1, //Next iteration step
             i => i * 2); //The value in each iteration

            observable.Subscribe(new ConsoleObserver<int>("enumerable"));
        }

        private static void FromEnumerable()
        {
            NumbersAndThrow()
             .ToObservable()
             .Subscribe(new ConsoleObserver<int>("throws"));
        }

        static IEnumerable<int> NumbersAndThrow()
        {
            yield return 1;
            yield return 2;
            yield return 3;
            throw new ApplicationException("Something Bad Happened");
        }

        static void Explicit()
        {
            var observable =
             Observable.Create<string>(o =>
             {
                 o.OnNext("Observable");
                 o.OnNext("To");
                 o.OnNext("Enumerable");
                 o.OnCompleted();
                 return Disposable.Empty;
             });

            observable.Subscribe(new ConsoleObserver<string>("explicit"));
        }

        static void ReadFile()
        {
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}Test.txt";
            IObservable<string> lines =
             Observable.Using(
             () => File.OpenText(path),
                 stream =>
                     Observable.Generate(
                         stream,
                         s => { Console.WriteLine($"Testing On {Thread.CurrentThread.ManagedThreadId}"); return !s.EndOfStream; },
                         s => s,
                         s => s.ReadLine())
             );
                //.SubscribeOn(Scheduler.Default)
                //.ObserveOn(Scheduler.Default);

            lines.Subscribe(new ConsoleObserver<string>("lines"));
        }

        static void MultiThreaded()
        {
            var observable1 = ProduceInt(0, 10).ToObservable();
            var observable2 = ProduceInt(10, 10).ToObservable();

            observable1
                .Merge(observable2)
                //.Timeout(TimeSpan.FromMilliseconds(100))
                .SelectMany(x => x.ToList())
                .SubscribeOn(Scheduler.Default)
                .Synchronize()
                .Subscribe(new ConsoleObserver<int>("multi"));
        }

        static void Synchronization()
        {
            var messages =
                Observable.FromEventPattern<(int, double)>(
                                h => IntReceived += h,
                                h => IntReceived -= h)
                .Synchronize()
                .Select(k => k.EventArgs);

            var sw = Stopwatch.StartNew();
            var observable1 = RangeAsync(0, 10)
                    .ToObservable()
                    .Subscribe(i => IntReceived?.Invoke(null, (i, sw.Elapsed.TotalMilliseconds)));

            var observable2 = RangeAsync(10, 10)
                    .ToObservable()
                    .Subscribe(i => IntReceived?.Invoke(null, (i, sw.Elapsed.TotalMilliseconds)));

            messages.Subscribe(new ConsoleObserver<(int, double)>("sync"));
        }

        static async IAsyncEnumerable<int> RangeAsync(int start, int count)
        {
            for (int i = 0; i < count; i++)
            {
                await Task.Delay(i);
                yield return start + i;
            }
        }

        static async Task<IEnumerable<int>> ProduceInt(int start, int count)
        {
            await Task.Delay(100);
            return Enumerable.Range(start, count);
        }

        public static event EventHandler<(int, double)> IntReceived;
    }
}
