using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace SimcraftGearOptimizer
{
    public class GearItemLoader
    {
        private readonly string cacheDir;

        public GearItemLoader(string cacheDir)
        {
            this.cacheDir = cacheDir;
        }

        public GearItem LoadGearItem(string itemid)
        {
            if (!Directory.Exists(cacheDir))
                Directory.CreateDirectory(cacheDir);

            var cacheName = Path.Combine(cacheDir, itemid);
            if (!File.Exists(cacheName))
            {
                using (var client = new WebClient())
                {
                    client.Headers.Add("user-agent", "MSIE 7.0");
                    client.QueryString.Add("i", itemid);
                    client.DownloadFile("http://www.wowarmory.com/item-tooltip.xml", cacheName);
                }
            }

            using (var reader = new StreamReader(cacheName))
            {
                var rawXML = reader.ReadToEnd();
                var doc = new XmlDocument();
                doc.LoadXml(rawXML);
                var docElement = doc.DocumentElement;

                if (!GearItem.IsValidItem(docElement))
                    return null;

                return GearItem.LoadFrom(docElement, rawXML);
            }
        }
    }
}
