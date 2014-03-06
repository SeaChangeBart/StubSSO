using System;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace WebASM
{
    static class XmlHelpers
    {
        public static string ToStringWithDeclaration(this XDocument doc, SaveOptions saveOptions)
        {
            if (doc == null)
                throw new ArgumentNullException("doc");

            using (TextWriter writer = new Utf8StringWriter())
            {
                doc.Save(writer, saveOptions);
                return writer.ToString();
            }
        }

        private class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }

        static public string ElementValueOrNull(this XElement element, XName childname)
        {
            var childElem = element.Element(childname);
            return childElem == null ? null : childElem.Value;
        }
    }
}