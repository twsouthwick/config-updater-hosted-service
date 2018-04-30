using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace BackgroundQuerySample.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly SomeOptions _options;

        public ValuesController(SomeOptions options)
        {
            _options = options;
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<bool> Get()
        {
            return new bool[] { _options.ShouldLog };
        }
    }
}
