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

        [DataMember]
        public XElement XmlDocument { get; set; }

        [DataMember]
        public XElement XsltTransformation { get; set; }
    }
}
