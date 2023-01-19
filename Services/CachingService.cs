using System.Linq;
using Fibonacci_API.Services.Interfaces;

namespace Fibonacci_API.Services
{
    //if caching is enabled, we use a shared list with a keyvalue_pair type
    //to enable quick lookups of nth term and the corresponding fibonacci sequence
    public class CachingService : ICachingService
    {
        private static readonly object _locker = new();

        private static List<KeyValuePair<int, long>> FibonacciSequenceCached { get; set; } = new List<KeyValuePair<int, long>>();

        public void CacheWrite(int indexVal, long writeVal)
        {
            KeyValuePair<int, long> newPair = new(indexVal, writeVal);

            //Put a lock here to prevent duplicate writes from concurrent threads
            lock (_locker)
            {
                if (!CacheReadExist(indexVal)) FibonacciSequenceCached.Add(newPair);
            }
        }

        public bool CacheReadExist(int indexVal)
        {
            if (FibonacciSequenceCached.Exists(x => x.Key == indexVal))
            {
                return true;
            }
            return false;
        }

        public List<long> CacheReadReturn(int startIndex, int endIndex)
        {
            try
            {
                //order by key values and select only the range given to the method
                var res = FibonacciSequenceCached
                            .Where(x => x.Key >= startIndex && x.Key <= endIndex);

                return (from kvp in res select kvp.Value).ToList();
            }
            catch (Exception)
            {
                //return emppty string for exception handling
                return new List<long>();
            }
        }

        [Obsolete]
        public List<long> CacheReadReturn(int startIndex)
        {
            try
            {
                var res = FibonacciSequenceCached
                            .OrderBy(x => x.Key)
                            .Skip(startIndex + 1);

                return (from kvp in res select kvp.Value).Distinct().ToList();
            }
            catch (Exception)
            {
                //return emppty string for exception handling
                return new List<long>();
            }
        }

        public long CachReadReturn_Single(int startIndex)
        {
            return FibonacciSequenceCached
                .Where(x => x.Key == startIndex)
                .Select(x => x.Value)
                .FirstOrDefault();
        }
    }
}
