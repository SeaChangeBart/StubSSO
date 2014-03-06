using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using Common;

namespace WebASM
{
    static internal class Serializers
    {
        private static readonly XNamespace XmlNs = "urn:schange:userauth:1.0";

        internal static string AuthenticationToXmlString(Authentication authentication)
        {
            var doc = new XDocument(new XDeclaration("1.0", "utf-8", null));
            var elemList = new List<object>();
            if (authentication.CustomerId != null)
                elemList.Add(new XElement(XmlNs + "CustomerId", authentication.CustomerId));
            if (authentication.CpeId != null)
                elemList.Add(new XElement(XmlNs + "CpeId", authentication.CpeId));
            if (authentication.ProfileId != null)
                elemList.Add(new XElement(XmlNs + "ProfileId", authentication.ProfileId));
            var tokenElem = new XElement(XmlNs + "Authentication", elemList.ToArray());
            doc.Add(tokenElem);
            return doc.ToStringWithDeclaration(SaveOptions.DisableFormatting);
        }

        internal static string SeacTokenToXmlString(SeacToken seacToken)
        {
            var doc = new XDocument {Declaration = new XDeclaration("1.0", "utf-8", null)};
            var tokenElem = new XElement(XmlNs + "SeacToken", new object[]
                                                                  {
                                                                      new XAttribute("class", seacToken.Class),
                                                                      new XAttribute("expiration",
                                                                                     seacToken.Expiration.ToString("yyyy-MM-ddTHH:mm:ssK"))
                                                                      ,
                                                                      new XAttribute("singleUse",
                                                                                     seacToken.SingleUse
                                                                                         ? "true"
                                                                                         : "false"),
                                                                      seacToken.Token
                                                                  });
            doc.Add(tokenElem);
            return doc.ToStringWithDeclaration(SaveOptions.DisableFormatting);
        }

        internal static Session SessionFromXmlStringSession(string sessionXml)
        {
            var doc = XDocument.Parse(sessionXml);
            var sessionElement = doc.Element(XmlNs + "Session") ?? doc.Element("Session");
            if (sessionElement == null)
                throw new Exception("No <Session> root element");
            var xmlNs = sessionElement.Name.Namespace;
            var expElement = sessionElement.Element(xmlNs + "Expiration");
            if (expElement == null)
                throw new Exception("No Expiration Element");
            var expiration = DateTime.Parse(expElement.Value, null, DateTimeStyles.RoundtripKind);

            var authElem = sessionElement.Element(xmlNs + "Authentication");
            var auth = new Authentication
                           {
                               CustomerId = authElem.ElementValueOrNull(xmlNs + "CustomerId"),
                               CpeId = authElem.ElementValueOrNull(xmlNs + "CpeId"),
                               ProfileId = authElem.ElementValueOrNull(xmlNs + "ProfileId"),
                           };
            var session = new Session {Authentication = auth, Expiration = expiration};
            return session;
        }
    }
}