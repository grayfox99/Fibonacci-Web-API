using System.Linq;
using Fibonacci_API.Services.Interfaces;

namespace Fibonacci_API.Services
{
    //if caching is enabled, we use a shared list with a keyvalue_pair type
    //to enable quick lookups of nth term and the corresponding fibonacci sequence
    public class CachingService : ICachingService
    {
        private int executionCount = 0;
        private readonly ILogger<CachingService>? _logger;
        private Timer? _timer;
        public static TimeSpan CacheExpirationTime_Minutes { get; private set; }
        private static readonly object _locker = new();
        public CachingService(IConfiguration configuration, ILogger<CachingService> logger) 
        {
            _logger = logger;

            if (int.TryParse(configuration["CachedClearPeriod_Minutes"], out int configtime))
            {
                CacheExpirationTime_Minutes = TimeSpan.FromMinutes(configtime);
            }

            //default expiration is 1-min
            else
            {
                _logger.LogInformation("Could not parse Time Span provided in config file. Using default value of 1 minute.");
                CacheExpirationTime_Minutes = TimeSpan.FromMinutes(1);
            }

            StartAsync();
        }

        //seperate constructer for unit-tests
        public CachingService()
        {
            CacheExpirationTime_Minutes = TimeSpan.FromMinutes(1);
            //StartAsync();
        }

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

        public Task StartAsync()
        {
            _logger.LogInformation("Timed Hosted Service running.");

            _timer = new Timer(ClearCache, null, TimeSpan.Zero,
                CacheExpirationTime_Minutes);

            return Task.CompletedTask;
        }

        public void ClearCache(object state)
        {
            FibonacciSequenceCached.Clear();
            var count = Interlocked.Increment(ref executionCount);
            _logger.LogInformation(
                "Cache cleared. Count: {Count}", count);
        }

    }
}
