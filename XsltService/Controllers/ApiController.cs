using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace XsltService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        // POST: api/Api
        [HttpPost("{id}", Name = "post")]
        public IActionResult Post([FromBody] IEnumerable<string> value)
        {
            return new ContentResult
            {
                Content = XsltTransformation.Transform(XDocument.Parse(value.ElementAt(0)), value.ElementAt(1)).ToString(),
                ContentType = "text/xml",
                StatusCode = 200
            };
        }

    }
}
