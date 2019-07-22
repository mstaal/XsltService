using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XsltService.Models;

namespace XsltService.Controllers
{
    //[Route("api/[controller]")]
    [Route("api")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        // POST: api/Api
        //[HttpPost("{id}", Name = "post")]
        [Route("transform")]
        [HttpPost]
        public IActionResult Post([FromBody] TransformationRequest request)
        {
            var document = request.XmlDocument;
            var transformationSpec = request.XslTransformation;
            var xsltFile = transformationSpec.Name == "File" ? transformationSpec.Value : null;
            var content = "";
            if (!string.IsNullOrEmpty(xsltFile))
            {
                content = new XDocument(XsltTransformation.Transform(document, xsltFile)).ToString();
            }
            else
            {
                content = new XDocument(XsltTransformation.Transform(document, transformationSpec)).ToString();
            }

            return new ContentResult
            {
                Content = content,
                ContentType = "text/xml",
                StatusCode = 200
            };
        }

    }
}
