using System.Text.Json.Serialization;

namespace Fibonacci_API.Types
{
    public class ReturnType
    { 
        public IEnumerable<long> FibonacciSequence { get; set; } = new List<long>();
        public TimeSpan ElapsedTime { get; set; }
        public long MemoryUsed_KB { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Exceptions { get; set; }
    }
}
