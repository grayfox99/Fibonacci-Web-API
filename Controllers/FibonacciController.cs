using Fibonacci_API.Services.Interfaces;
using Fibonacci_API.Types;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Fibonacci_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FibonacciController : ControllerBase
    {
        readonly IFibonacciService _fibservice;
        readonly ICachingService _cachingService;

        public FibonacciController(IFibonacciService fibonacciService, ICachingService cachingService)
        {
            _fibservice = fibonacciService;
            _cachingService = cachingService;
        }

        /// <summary>
        /// Generates the Fibonacci sequence based on the inputs provided.
        /// Please note that larger ranges may cause the integer to overflow.
        /// </summary>
        /// <param name="inputType">The value !!</param>
        //POST 
        [HttpPost]
        public async Task<ActionResult> GenerateFibonacciSequence([FromBody] InputType inputType)
        {
            ReturnType returnClass;

            returnClass = await _fibservice
                .Initialize(inputType.StartIndex
                            , inputType.EndIndex
                            , inputType.Cached
                            , TimeSpan.FromMilliseconds(inputType.TimeLimit)
                            , inputType.MemoryLimit
                            , _cachingService);

            var serializedResult = JsonSerializer.Serialize(returnClass);

            if (!returnClass.FibonacciSequence.Any())
            {
                return BadRequest(JsonSerializer.Serialize(returnClass.Exceptions));
            }
          
            else if (returnClass.Exceptions != null)
            {
                return new JsonResult(returnClass)
                {
                    StatusCode = StatusCodes.Status206PartialContent
                };
            }
            
            return Ok(serializedResult);
        }
    }
}
