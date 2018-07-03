namespace FeedTime.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FeedTime.Common.DataModel;
    using FeedTime.ViewModels;

    public static class LinqExtensions
    {
        /// <summary>
        /// Extrapolates the given trend of ActivityTrend data points 
        /// to a full trend of ActivityTrendViewModel data points, 
        /// one for each hour over the given period. If no ActivityTrend data 
        /// point exists for the hour then the IsActive value of the 
        /// ActivityTrendViewModel is set to 0
        /// </summary>
        public static IEnumerable<ActivityTrendViewModel> GenerateRangeFromValues(this IEnumerable<ActivityTrend> trend, DateTimeOffset startTime, int numberOfHours)
        {
            var endTime = startTime.AddHours(numberOfHours);
            var trendRange = new List<ActivityTrendViewModel>();

            if (!trend.Any())
                return trendRange;

            var firstTrendItem = trend.First();
            if (!firstTrendItem.StartTime.Equals(startTime))
                trendRange.Add(new ActivityTrendViewModel { Time = startTime.DateTime, IsActive = 0 });

            ActivityTrend lastItem = null;
            foreach (var trendItem in trend)
            {
                lastItem = trendItem;

                trendRange.Add(new ActivityTrendViewModel { Time = trendItem.StartTime.AddMilliseconds(-1).DateTime, IsActive = 0 });
                trendRange.Add(new ActivityTrendViewModel { Time = trendItem.StartTime.DateTime, IsActive = 1 });

                if (trendItem.EndTime.HasValue)
                {
                    trendRange.Add(new ActivityTrendViewModel { Time = trendItem.EndTime.Value.DateTime, IsActive = 1 });
                    trendRange.Add(new ActivityTrendViewModel { Time = trendItem.EndTime.Value.AddMilliseconds(1).DateTime, IsActive = 0 });
                }
            }

            if (lastItem != null && lastItem.EndTime.HasValue && !endTime.Equals(lastItem.EndTime.Value))
                trendRange.Add(new ActivityTrendViewModel { Time = endTime.DateTime, IsActive = 0 });

            return trendRange;
        }

        /// <summary>
        /// Extrapolates the given trend of MoodTrend data points 
        /// to a full trend of MoodTrendViewModel data points, 
        /// one for each day over the given period. If no MoodTrend data 
        /// point exists for the hour then the Feeling value of the 
        /// MoodTrendViewModel is set to null
        /// </summary>
        public static IEnumerable<MoodTrendViewModel> GenerateRangeFromValues(this IEnumerable<MoodTrend> trend, DateTimeOffset startTime, int numberOfDays)
        {
            return trend.Select(trendItem => new MoodTrendViewModel { Date = trendItem.Date.Date, Feeling = trendItem.Feeling - 1 });
        }

        /// <summary>
        /// Extrapolates the given trend of MeasurementTrend data points 
        /// to a full trend of MeasurementTrendViewModel data points, 
        /// one for each day over the given period. If no MeasurementTrend data 
        /// point exists for the hour then the Feeling value of the 
        /// MeasurementTrendViewModel is set to null
        /// </summary>
        public static IEnumerable<MeasurementTrendViewModel> GenerateRangeFromValues(this IEnumerable<MeasurementTrend> trend, DateTimeOffset startTime, int numberOfWeeks, Func<MeasurementTrend, double?> measurementValueSelector)
        {
            return trend.Where(trendItem => measurementValueSelector(trendItem).HasValue)
                        .Select(trendtItem => new MeasurementTrendViewModel { Date = trendtItem.Date.DateTime, Week = Convert.ToInt32(Math.Floor((trendtItem.Date - startTime).TotalDays / 7)), Measurement = measurementValueSelector(trendtItem) });
        }

        /// <summary>
        /// Returns the maximal element of the given sequence, based on
        /// the given projection.
        /// </summary>
        /// <remarks>
        /// If more than one element has the maximal projected value, the first
        /// one encountered will be returned. This overload uses the default comparer
        /// for the projected type. This operator uses immediate execution, but
        /// only buffers a single result (the current maximal element).
        /// </remarks>
        /// <typeparam name="TSource">Type of the source sequence</typeparam>
        /// <typeparam name="TKey">Type of the projected element</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="selector">Selector to use to pick the results to compare</param>
        /// <returns>The maximal element, according to the projection.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null</exception>
        /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty</exception>
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> selector)
        {
            return source.MinBy(selector, Comparer<TKey>.Default);
        }

        /// <summary>
        /// Returns the maximal element of the given sequence, based on
        /// the given projection and the specified comparer for projected values. 
        /// </summary>
        /// <remarks>
        /// If more than one element has the maximal projected value, the first
        /// one encountered will be returned. This overload uses the default comparer
        /// for the projected type. This operator uses immediate execution, but
        /// only buffers a single result (the current maximal element).
        /// </remarks>
        /// <typeparam name="TSource">Type of the source sequence</typeparam>
        /// <typeparam name="TKey">Type of the projected element</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="selector">Selector to use to pick the results to compare</param>
        /// <param name="comparer">Comparer to use to compare projected values</param>
        /// <returns>The maximal element, according to the projection.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="selector"/> 
        /// or <paramref name="comparer"/> is null</exception>
        /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty</exception>
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");
            if (comparer == null) throw new ArgumentNullException("comparer");

            using (var sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                {
                    throw new InvalidOperationException("Sequence contains no elements");
                }

                var min = sourceIterator.Current;
                var minKey = selector(min);

                while (sourceIterator.MoveNext())
                {
                    var candidate = sourceIterator.Current;
                    var candidateProjected = selector(candidate);

                    if (comparer.Compare(candidateProjected, minKey) < 0)
                    {
                        min = candidate;
                        minKey = candidateProjected;
                    }
                }

                return min;
            }
        }
    }
}