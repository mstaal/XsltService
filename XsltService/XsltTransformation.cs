using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace XsltService
{
    /// <summary>
    /// Class that performs transformations on xml documents using xslt.
    /// </summary>
    public static class XsltTransformation
    {
        /// <summary>
        /// Returns the Xml namespace for Xslt.
        /// </summary>
        private const string XsltNamespace = "http://www.w3.org/1999/XSL/Transform";

        public static XDocument Transform(XNode document, string xsltFile)
        {
            Console.WriteLine(xsltFile);
            return Transform(document, XElement.Load(xsltFile));
        }

        public static XDocument Transform(XNode document, XElement xsltDocument)
        {
            var args = new XsltArgumentList();
            var extensionFunctions = new XsltFunctions();
            args.AddExtensionObject("xalan://diadem.dirigent.plugin.helpers.XsltFunctions", extensionFunctions);
            var transformedDocument = Transform(document, args, xsltDocument);
            var debugMessage = new XElement("DebugMessage", extensionFunctions.DebugMessage.ToString());
            return transformedDocument;
        }

        /// <summary>
        /// Transforms given xml document using xslt schema to other xml document.
        /// </summary>
        /// <param name="document"> Xml document to be transformed. </param>
        /// <param name="xsltArgumentList"> Arguments for the transformation. </param>
        /// <param name="xsltDocument"> Xslt transformation schema file info. </param>
        /// <returns> Transformed xml document from given xml document using xslt schema. </returns>
        public static XDocument Transform(XNode document, XsltArgumentList xsltArgumentList, XElement xsltDocument)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            if (xsltDocument == null)
            {
                throw new ArgumentNullException("xsltDocument");
            }
            
            var importElements = xsltDocument.XPathSelectElements(
                "//xslt:import", CreateXsltNamespaceManager(new XDocument(xsltDocument)));

            var query = from importElement in importElements
                        let hrefAttribute = importElement.Attribute("href")
                        where hrefAttribute != null
                        select hrefAttribute;

            foreach (var hrefAttribute in query)
            {
                if (File.Exists(hrefAttribute.Value))
                {
                    continue;
                }

                var newLocation = Path.Combine(PathResolver.ExecutablePath, hrefAttribute.Value);
                if (File.Exists(newLocation))
                {
                    hrefAttribute.Value = newLocation;
                }
            }

            var xsltTransformation = new XslCompiledTransform(false); // <- Will cause memory leak if debug = true, or at least it will create tons of .dlls
            xsltTransformation.Load(xsltDocument.CreateReader(), new XsltSettings(false, true), new XmlUrlResolver());
                
            var reader = document.CreateReader();
            var transformedDocument = new XDocument();
            using (var xmlWriter = transformedDocument.CreateWriter())
            {
                xsltTransformation.Transform(reader, xsltArgumentList, xmlWriter);
                return transformedDocument;
            }
        }

        /// <summary>
        /// Creates a <see cref="XmlNamespaceManager"/> for DMP.
        /// </summary>
        /// <param name="document">
        /// The xml document.
        /// </param>
        /// <returns>
        /// The namespace manager.
        /// </returns>
        private static XmlNamespaceManager CreateXsltNamespaceManager(XDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            using (var reader = document.CreateReader())
            {
                if (reader.NameTable == null)
                {
                    throw new InvalidOperationException("Could not get a name table from input");
                }

                var requestNamespaceManager = new XmlNamespaceManager(reader.NameTable);
                requestNamespaceManager.AddNamespace("xslt", XsltNamespace);
                return requestNamespaceManager;
            }
        }
    }
}
