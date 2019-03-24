using System;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace XsltService.Models
{
    [DataContract]
    public class TransformationRequest
    {
        public TransformationRequest()
        {
        }

        [DataMember(IsRequired = true)]
        public XElement XmlDocument { get; set; }

        [DataMember(IsRequired = true)]
        public XElement XslTransformation { get; set; }
    }
}
