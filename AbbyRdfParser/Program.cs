using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Linguistics.Core.Enums;
using Linguistics.Core.Interfaces.CRF.Training;

namespace AbbyRdfParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var model = new EntitiesCorpus();
            var doc =
                XDocument.Parse(
                    File.ReadAllText(@"rdf_IE_2.0_ru.xml"));
            var root = doc.Root;
            var rdfnamespace = XNamespace.Get("http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            var auxnamespace = XNamespace.Get("http://www.abbyy.com/ns/Aux#");
            var basicentitynamespace = XNamespace.Get("http://www.abbyy.com/ns/BasicEntity#");
            var nodeId = rdfnamespace.GetName("nodeID");
            var resource = rdfnamespace.GetName("resource");
            var annotationsName = auxnamespace.GetName("TextAnnotations");
            var annotation = auxnamespace.GetName("InstanceAnnotation");
            var start = auxnamespace.GetName("annotation_start");
            var end = auxnamespace.GetName("annotation_end");
            var instance = auxnamespace.GetName("instance");
            var annotationsText = auxnamespace.GetName("document_text");
            var annotations = root.Element(annotationsName);
            var text = annotations.Element(annotationsText).Value;
            File.WriteAllText("testmodeleng_text_abby.txt", text);
            var objects = new Dictionary<string, Tuple<int, int>>();
            var allEntities = new Dictionary<KeyValuePair<int, int>, NerTextEntity>();
            var basicEntityTypes = new HashSet<string>();
            foreach (var e in root.Elements())
            {
                if (e.Name.Namespace == basicentitynamespace)
                {
                    basicEntityTypes.Add(e.Name.LocalName);
                }
            }
            foreach (var basicEntityType in basicEntityTypes)
            {
                Console.WriteLine(basicEntityType);
            }
            foreach (var ann in annotations
                .Elements(auxnamespace.GetName("instance_annotation"))
                .Select(a => a.Element(annotation))
                .Where(a => a != null))
            {
                var ints = ann.Element(instance);
                var idattr = ints.Attribute(nodeId);
                var begin = int.Parse(ann.Element(start).Value);
                var endi = int.Parse(ann.Element(end).Value);
                if (idattr != null)
                {
                    var id = idattr.Value;
                    if (!objects.ContainsKey(id))
                    {

                        if (begin != endi)
                        {
                            objects.Add(id, Tuple.Create(begin, endi));
                        }
                    }
                }
                else
                {
                    var resAttr = ints.Attribute(resource);
                    if (resAttr != null)
                    {
                        NerType type = GetTypeFromUriAttr(resAttr.Value, string.Empty);
                        if (type != NerType.Undefined)
                        {
                            AddEntityToModel(model, begin, endi, type, allEntities);
                        }
                    }
                }
            }
            model.ClearedText = text;

            foreach (var xElement in root.Elements())
            {
                NerType type = GetTypeFromUriAttr(xElement.Name.Namespace.NamespaceName, xElement.Name.LocalName);
                if (type != NerType.Undefined)
                {
                    var idattr = xElement.Attribute(nodeId);
                    if (idattr != null)
                    {
                        var val = idattr.Value;
                        Tuple<int, int> pos;
                        if (objects.TryGetValue(val, out pos))
                        {
                            AddEntityToModel(model, pos.Item1, pos.Item2, type, allEntities);
                        }
                    }
                }
            }
            File.WriteAllText("modelsall_abby.txt", model.Render());
            Console.ReadLine();
        }

        private static void AddEntityToModel(EntitiesCorpus model, int begin, int endi, NerType type, Dictionary<KeyValuePair<int, int>, NerTextEntity> allEntities)
        {
            NerTextEntity existing;
            if (allEntities.TryGetValue(new KeyValuePair<int, int>(begin, endi), out existing) && existing.Type != type)
            {
                model.Entities.Remove(existing);
            }
            if (existing != null)
            {
                return;
            }
            var ent = new NerTextEntity(begin, endi, type);
            model.Entities.Add(ent);
            allEntities.Add(new KeyValuePair<int, int>(begin, endi), ent);
        }

        private static NerType GetTypeFromUriAttr(string namespaceName, string localName)
        {
            if (namespaceName.Contains("Geo#"))
            {
                return NerType.Location;
            }

            if (namespaceName.Contains("Org#"))
            {
                return NerType.Org;
            }
            if (localName == "Person")
            {
                return NerType.Person;
            }
            return NerType.Undefined;
        }
    }
}
