using System;
using System.Linq;
using System.Net;
using Common;

namespace WebASM
{
    public class RequestEssentials
    {
        public WebHeaderCollection Headers { get; set; }

        public SeacTokenBare GetSeacTokenAndClass()
        {
            if (Headers == null)
                return null;

            var seacAuthHeader =
                (Headers.GetValues("Authorization") ?? new string[0]).FirstOrDefault(
                    _ => _.StartsWith("SeacToken ", StringComparison.InvariantCulture));
            if (seacAuthHeader == null)
                return null;

            var pairs = seacAuthHeader.Split(new[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
            var keyValues =
                pairs.Select(pair => pair.Split(new[] {'='}, 2)).Where(arree => arree.Length == 2).ToDictionary(
                    _ => _[0], _ => _[1]);

            string tokenId, tokenClass;
            if (!keyValues.TryGetValue("token", out tokenId))
                return null;

            if (!keyValues.TryGetValue("class", out tokenClass))
                return null;

            return new SeacTokenBare
                       {
                           Token = tokenId.Substring(1, tokenId.Length - 2),
                           Class = tokenClass.Substring(1, tokenClass.Length - 2)
                       };
        }
    }
}