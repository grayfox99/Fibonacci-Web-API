
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fibonacci_API.Types
{

    public class InputType
    {
        [Range(0, 999)]
        [DefaultValue(1)]
        [Required(ErrorMessage = "Start Index is required")]
        public int StartIndex { get; set; }

        [Range(0, 999)]
        [DefaultValue(10)]
        [Required(ErrorMessage = "End Index is required")]
        public int EndIndex { get; set; }

        [Required(ErrorMessage = "Caching status is required")]
        public bool Cached { get; set; }

        [Range(10, 1000000)]
        [DefaultValue(100000)]
        [Required(ErrorMessage = "Memory Limit is required")]
        public int MemoryLimit { get; set; }

        [Range(5, 300000)]
        [Required(ErrorMessage = "Time Limit is required")]
        [DefaultValue(10000)]
        public int TimeLimit { get; set; }
    }
    
}
