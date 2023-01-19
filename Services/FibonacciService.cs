using Fibonacci_API.Services.Interfaces;
using Fibonacci_API.Types;
using System.Diagnostics;
using System.Text.Json;

namespace Fibonacci_API.Services
{
    public sealed class FibonacciService : IFibonacciService
    {
        #region Properties
        private List<long> FibonacciSequence { get; } = new();
        private bool CacheEnabled { get; set; }
        public TimeSpan TimeoutLimit { get; private set; }
        public int MemoryLimit { get; private set; }
        public List<long> MemoryRecord { get; private set; } = new List<long> { 0 };
        public ICachingService CachingService { get; set; } = new CachingService();
        #endregion

        #region Methods
        public async Task<ReturnType> Initialize(int startIndex, int endIndex, bool cacheEnabled, TimeSpan timelimit, int memorylimit,ICachingService _cachingService)
        {
            TimeoutLimit = timelimit;
            MemoryLimit = memorylimit;
            CacheEnabled = cacheEnabled;
            CachingService = _cachingService;


            //Pass index arguments to async task
            var fibsequence = await ExecuteFibonacciOperation(startIndex, endIndex);
            return fibsequence; 
                
        }

        private async Task<ReturnType> ExecuteFibonacciOperation(int startIndex, int endIndex)
        {
            var StartTimeStamp = Stopwatch.GetTimestamp();
            ReturnType returnType = new ReturnType();

            if (!CacheEnabled)
            {
                try
                {
                    await Task.Run(() => FibonacciIterative(endIndex, StartTimeStamp));

                    var result = FibonacciSequence.GetRange(startIndex, endIndex - startIndex + 1);

                    returnType.FibonacciSequence = result;
                    returnType.ElapsedTime = GetCurrentElapsedTime(StartTimeStamp);
                    returnType.MemoryUsed_KB = MemoryRecord.Last();

                    return returnType;
                }

                catch (Exception ex)
                {
                    if (FibonacciSequence.Skip(startIndex + 1).Any())
                    {
                        returnType.FibonacciSequence = FibonacciSequence.Skip(startIndex);
                    }

                    returnType.ElapsedTime = GetCurrentElapsedTime(StartTimeStamp);
                    returnType.MemoryUsed_KB = MemoryRecord.Last();
                    returnType.Exceptions = ex.Message;
                        
                    return returnType;
                }
            }

            else
            {
                try
                {
                    await Task.Run(() => FibonacciIterative_cached(endIndex, StartTimeStamp));

                    var result = CachingService.CacheReadReturn(startIndex, endIndex);

                    returnType.FibonacciSequence = result;
                    returnType.ElapsedTime = GetCurrentElapsedTime(StartTimeStamp);
                    returnType.MemoryUsed_KB = MemoryRecord.Last();

                    return returnType;
                }

                catch (Exception ex)
                {
                    returnType.FibonacciSequence = CachingService.CacheReadReturn(startIndex, endIndex);
                    returnType.ElapsedTime = GetCurrentElapsedTime(StartTimeStamp);
                    returnType.MemoryUsed_KB = MemoryRecord.Last();
                    returnType.Exceptions = ex.Message;

                    return returnType;
                }
            }
        }

        private TimeSpan GetCurrentElapsedTime(long startTime)
        {
            var currTime = Stopwatch.GetTimestamp();
            return Stopwatch.GetElapsedTime(startTime, currTime);
        }

        private void RecordCurrentMemoryUsage()
        {
            Process currentProcess = Process.GetCurrentProcess();
            var sizeInMB = currentProcess.PeakWorkingSet64 / 1024 ;
            MemoryRecord.Add(sizeInMB);
            if (sizeInMB > MemoryLimit)
            {
                throw new Exception($"Memory Limit of {MemoryLimit} Exceeded by {sizeInMB - MemoryLimit} KB");
            }
        }
      
        private void RecordCurrentTimeUsage(Int64 startTime)
        {
            if (GetCurrentElapsedTime(startTime) >= TimeoutLimit)
            {
                throw new TimeoutException($"Time Limit of {TimeoutLimit.TotalMilliseconds} ms exceeded.");
            }
        }

        public string GetTotalProcessMemory()
        {
            var memResult = "Memory Consumptions for Every Number Generated (MB): "+
            string.Join(",", MemoryRecord);
            return memResult;
        }

        private void FibonacciIterative(int endIndex, Int64 startTime)
        {
            int firstnumber = 0, secondnumber = 1, result = 0;
           
            for (int i = 0; i <= endIndex; i++)  
            {
                Thread.Sleep(500); //500ms thread stop for simulating intensive CPU operation
                RecordCurrentMemoryUsage();
                RecordCurrentTimeUsage(startTime);

                if (i == 0)
                {
                    FibonacciSequence.Add(0);
                    continue;
                }
                else if (i == 1)
                {
                    FibonacciSequence.Add(1);
                    continue;
                }

                result = firstnumber + secondnumber;
                FibonacciSequence.Add(result);
                
                firstnumber = secondnumber;
                secondnumber = result;             
            }
        }
 
        private void FibonacciIterative_cached(int endIndex, Int64 startTime)
        {
            long firstnumber = 0;
            long secondnumber = 1;
            long result = 0;

            for (int i = 0; i <= endIndex + 1; i++)
            {
                //check if cached key-value pair is present in the List<T>
                if (CachingService.CacheReadExist(i))
                {
                    result = CachingService.CachReadReturn_Single(i);
                    firstnumber = secondnumber;
                    secondnumber = result;
                    continue;
                }

                Thread.Sleep(500); //500ms thread stop for simulating intensive CPU operation
                RecordCurrentMemoryUsage();
                RecordCurrentTimeUsage(startTime);

                if (i == 0)
                {
                    CachingService.CacheWrite(i, 0);
                    continue;
                }
                else if (i == 1)
                {
                    CachingService.CacheWrite(i, 1);
                    continue;
                }

                result = firstnumber + secondnumber;
                CachingService.CacheWrite(i, result);

                firstnumber = secondnumber;
                secondnumber = result;
            }
        }

        //[Obsolete("Recursive Methods are not stack friendly")]
        //private long FibonacciRecursiveCached(long endIndex)
        //{
        //    //check if cached key-value pair is present in the List<T>
        //    if (FibonacciSequenceCached.Exists(x => x.Key == endIndex))
        //    {
        //        var res = FibonacciSequenceCached
        //            .Where(x => x.Key == endIndex)
        //            .Select(kvp => kvp.Value);
        //        return res.First();
        //    }

        //    else if (endIndex == 1)
        //    {
        //        KeyValuePair<long, long> basepair = new(endIndex, 1);
        //        FibonacciSequenceCached.Add(basepair);
        //        return 1;
        //    }

        //    else if (endIndex == 0)
        //    {
        //        KeyValuePair<long, long> basepair = new(endIndex, 0);
        //        FibonacciSequenceCached.Add(basepair);
        //        return 0;
        //    }

        //    long currResult = FibonacciRecursiveCached(endIndex - 1) + FibonacciRecursiveCached(endIndex - 2);

        //    KeyValuePair<long, long> currpair = new KeyValuePair<long, long>(endIndex, currResult);
        //    FibonacciSequenceCached.Add(currpair);

        //    RecordCurrentMemoryUsage();
        //    Thread.Sleep(50); //50ms thread stop for simulating intensive CPU operation
        //    return currResult;
        //}

        #endregion
    }
}
