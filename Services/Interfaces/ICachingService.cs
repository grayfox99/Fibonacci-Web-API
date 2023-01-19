namespace Fibonacci_API.Services.Interfaces
{
    public interface ICachingService
    {
        bool CacheReadExist(int indexVal);
        List<long> CacheReadReturn(int startIndex, int endIndex);
        List<long> CacheReadReturn(int startIndex);
        void CacheWrite(int indexVal, long writeVal);
        long CachReadReturn_Single(int startIndex);
    }
}