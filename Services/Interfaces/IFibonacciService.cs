using Fibonacci_API.Types;

namespace Fibonacci_API.Services.Interfaces
{
    public interface IFibonacciService
    {
        int MemoryLimit { get; }
        TimeSpan TimeoutLimit { get; }
        Task<ReturnType> Initialize(int startIndex, int endindex, bool cachEnabled, TimeSpan timelimit, int memorylimit, ICachingService _cachingService);
        string GetTotalProcessMemory();
    }
}