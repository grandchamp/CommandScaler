using CommandScaler.Bus.Contracts;
using CommandScaler.Sample.Core.Commands;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CommandScaler.Sample.API.Controllers
{
    public class TestController : Controller
    {
        private readonly IBus _bus;

        public TestController(IBus bus)
        {
            _bus = bus;
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            var result = await _bus.Send(new DelayedCommand { Message = "message" });

            return Ok(result);
        }
    }
}
