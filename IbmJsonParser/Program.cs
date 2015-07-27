using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var types = new ConcurrentBag<string>();
            var jsonPath = @"E:\tmp\ibm\ibmresultjson";
            var files = Directory.EnumerateFiles(jsonPath).ToArray();
            var ouputDir = @"E:\tmp\ibm\ibmcorpus";
            Parallel.For(0, files.Length,
                (int i) =>
                {
                    ConvertJsonToCorpus(files[i], Path.Combine(ouputDir, Path.GetFileName(files[i])), types);
                });

            Console.WriteLine("Types {0}", types.Count);
            foreach (var type in types.Distinct())
            {
                Console.WriteLine(type);
            }
            Console.ReadLine();
        }

        private static void ConvertJsonToCorpus(string inputPath, string outputPath, ConcurrentBag<string> types)
        {
            var json = File.ReadAllText(inputPath);
            var corpus = new EntitiesCorpus();
            dynamic s = JsonConvert.DeserializeObject(json);
            var doc = s.doc;
            corpus.ClearedText = doc.text;
            var entities = doc.entities.entity;
            var etypes = new Dictionary<string, string>();
            if (entities != null)
            {
                foreach (dynamic entity in entities)
                {
                    string id = entity.eid;
                    if (entity.level == "NAM")
                    {
                        string type = entity.type;
                        etypes.Add(id, type);
                    }
                }

                var mentions = doc.mentions.mention;
                if (mentions != null)
                {
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
                }
            }

            File.WriteAllText(outputPath, corpus.Render());
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
                case "EVENT_DISASTER":
                case "EVENT_MEETING":
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
