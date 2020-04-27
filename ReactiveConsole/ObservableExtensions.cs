using System;
using System.Reactive.Linq;

namespace ReactiveConsole
{
    public static class ObservableExtensions
    {
        public static IObservable<(TSource previous, TSource current)> PairWithPrevious<TSource>(this IObservable<TSource> source)
        {
            return source.Scan(
                new ValueTuple<TSource, TSource>(default, default),
                (acc, curr) => (acc.Item2, curr));
        }
    }
}
