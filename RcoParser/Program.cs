using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Linguistics.Core.Enums;
using Linguistics.Core.Extensions;
using Linguistics.Core.Interfaces.CRF.Training;

namespace RcoParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var corpus = new EntitiesCorpus();
            var cp1251 = Encoding.GetEncoding("windows-1251");
            var xml = File.ReadAllText(@"E:\SystemsTests\rco\bin\result.xml", cp1251);
            var text = File.ReadAllText(@"E:\SystemsTests\rco\bin\samples\modelsall_text_cp1251.txt",cp1251);
            corpus.ClearedText = text;
            var doc = XDocument.Parse(xml);
            var document = doc.Element("output").Element("document");
            var entities = document.Elements("sentence").SelectMany(s => s.Elements("entities").SelectMany(e => e.Elements("entity"))).ToList();
            foreach (var entity in entities)
            {
                var t = GetType(entity.GetAttributeValue("type"));
                if (t != NerType.Undefined)
                {
                    var begin = entity.GetAttributeValue("offset").ParseInt(-1);
                    var end = entity.GetAttributeValue("length").ParseInt(-1) + begin;
                    corpus.Entities.Add(new NerTextEntity(begin, end, t));
                }
            }
            File.WriteAllText("modelsall_rco_text.txt", corpus.ClearedText);
            File.WriteAllText("modelsall_rco.txt", corpus.Render());
            Console.ReadLine();
        }

        static NerType GetType(string t)
        {
            switch (t)
            {
                case "Event:Event":
                    return NerType.Event;
                case "Geoplace:KeyPost":
                case "Geoplace:Metro":
                case "Geoplace:Name":
                    return NerType.Location;
                case "Organization:Key":
                case "Organization:KeyAbbr":
                case "Organization:KeyToName":
                case "Organization:Name":
                case "Organization:Noname":
                case "Organization:Part":
                    return NerType.Org;
                case "Person:Name":
                case "Person:Position":
                case "Person:Vocative":
                    return NerType.Person;
                default:
                    return NerType.Undefined;
            }
        }
    }
}
