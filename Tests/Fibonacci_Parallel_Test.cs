using Fibonacci_API.Services;
using Fibonacci_API.Types;

namespace Fibonacci_API_Tests
{
    //Run at the same time with Fibonacci_Tests to test concurrency
    public class Fibonacci_Parallel_Tests
    {
        private readonly FibonacciService _sut;
        private readonly CachingService _cachingService;
        private List<long> _expectedSequeunce;

        public static IEnumerable<object[]> TestParameters()
        {
            yield return new object[] { new List<long>() { 0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55 }, 0, 10, 60000, 900000 };
            yield return new object[] { new List<long>() { 0, 1, 1, 2, 3, 5, 8, 13, 21 }, 0, 8, 60000, 900000 };
            yield return new object[] { new List<long>() { 0, 1, 1, 2, 3, 5, 8, 13 }, 0, 7, 60000, 900000 };
            yield return new object[] { new List<long>() { 0, 1 }, 0, 1, 60000, 900000 };
        }

        public static IEnumerable<object[]> TestParameters_cached()
        {
            yield return new object[] { new List<long>() { 0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55 }, 0, 10, 60000, 900000 };
            yield return new object[] { new List<long>() { 0, 1, 1, 2, 3, 5, 8, 13, 21 }, 0, 8, 500, 100 };
            yield return new object[] { new List<long>() { 0, 1, 1, 2, 3, 5, 8, 13 }, 0, 7, 5, 0 };
            yield return new object[] { new List<long>() { 0, 1 }, 0, 1, 5, 0 };
        }

        public Fibonacci_Parallel_Tests()
        {
            _sut = new FibonacciService();
            _cachingService = new CachingService();
            _expectedSequeunce = new() { 0 };
        }

        [Theory]
        [MemberData(nameof(TestParameters))]
        public async void TestWithSmallRange_NotCached_DefaultTimings(List<long> expected, int start, int end, int timelimit, int memorylimit)
        {
            var _returnType = await _sut.Initialize(start, end, false, TimeSpan.FromMilliseconds(timelimit), memorylimit, _cachingService);
            Assert.Equal(expected, _returnType.FibonacciSequence);
        }

        [Theory]
        [MemberData(nameof(TestParameters_cached))]
        public async void TestWithSmallRange_Cached_strictTimings(List<long> expected, int start, int end, int timelimit, int memorylimit)
        {
            var _returnType = await _sut.Initialize(start, end, true, TimeSpan.FromMilliseconds(timelimit), memorylimit, _cachingService);
            Assert.Equal(expected, _returnType.FibonacciSequence);
        }

    }
}