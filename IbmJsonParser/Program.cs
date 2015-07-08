using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Linguistics.Core.Enums;
using Linguistics.Core.Interfaces.CRF.Training;
using Newtonsoft.Json;

namespace IbmJsonParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var corpus = new EntitiesCorpus();
            var json = File.ReadAllText(@"C:\code\ibm\data\output1.json");
            dynamic s = JsonConvert.DeserializeObject(json);
            var doc = s.doc;
            corpus.ClearedText = doc.text;
            File.WriteAllText("testmodeleng_ibm_text.txt", s.text);
            var entities = doc.entities.entity;
            var etypes = new Dictionary<string, string>();
            foreach (dynamic entity in entities)
            {
                string id = entity.eid;
                if (entity.level == "NAM")
                {
                    string type = entity.type;
                    etypes.Add(id, type);
                }
            }
            var types = new HashSet<string>();
            var mentions = doc.mentions.mention;
            foreach (dynamic mention in mentions)
            {
                int begin = mention.begin;
                int end = mention.end;
                string id = mention.eid;
                string type;
                if (etypes.TryGetValue(id, out type))
                {
                    types.Add(type);
                    var t = GetType(type);
                    if (t != NerType.Undefined)
                    {
                        corpus.Entities.Add(new NerTextEntity(begin, end + 1, t));
                    }
                }
            }

            Console.WriteLine("Types {0}", types.Count);
            foreach (var type in types)
            {
                Console.WriteLine(type);
            }
            File.WriteAllText("testmodeleng_ibm_corpus.txt", corpus.Render());
            Console.ReadLine();
        }

        static NerType GetType(string t)
        {
            switch (t)
            {
                case "GPE":
                case "LOCATION":
                case "GEOLOGICALOBJ":
                    return NerType.Location;
                case "PEOPLE":
                case "PERSON":
                    return NerType.Person;
                case "PRODUCT":
                case "VEHICLE":
                    return NerType.Product;
                case "EVENT_VIOLENCE":
                case "EVENT_SPORTS":
                case "EVENT_PERFORMANCE":
                case "AWARD":
                    return NerType.Event;
                case "FACILITY":
                case "ORGANIZATION":
                    return NerType.Org;
                default:
                    return NerType.Undefined;
            }
        }
    }
}
