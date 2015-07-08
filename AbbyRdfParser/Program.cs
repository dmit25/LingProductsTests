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

namespace AbbyRdfParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var model = new EntitiesCorpus();
            var doc =
                XDocument.Parse(
                    File.ReadAllText(@"E:\Data\_DOWNLOADS\Results\testmodeleng_abby\testmodeleng_abby_rdf.xml"));
            var root = doc.Root;
            var rdfnamespace = XNamespace.Get("http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            var nodeId = rdfnamespace.GetName("nodeID");
            var resource = rdfnamespace.GetName("resource");
            var auxnamespace = XNamespace.Get("http://www.abbyy.com/ns/Aux#");
            var start = auxnamespace.GetName("AnnotationStart");
            var end = auxnamespace.GetName("AnnotationEnd");
            var annotationsName = auxnamespace.GetName("TextAnnotations");
            var annotationsText = auxnamespace.GetName("DocumentText");
            var annotations = root.Element(annotationsName);
            var text = annotations.Element(annotationsText).Value;
            File.WriteAllText("testmodeleng_text_abby.txt", text);
            var objects = new Dictionary<string, Tuple<int, int>>();
            foreach (var xElement in annotations.Elements(auxnamespace.GetName("ObjectAnnotation")))
            {
                var idattr = xElement.Attribute(nodeId);
                if (idattr != null)
                {
                    var id = idattr.Value;
                    if (!objects.ContainsKey(id))
                    {
                        objects.Add(id,
                            Tuple.Create(int.Parse(xElement.Attribute(start).Value),
                                int.Parse(xElement.Attribute(end).Value)));
                    }
                    else
                    {
                        //var old = objects[id];
                        //objects[id] = Tuple.Create(old.Item1, int.Parse(xElement.Attribute(end).Value));
                    }
                }
                else
                {
                    var resAttr = xElement.Attribute(resource);
                    if (resAttr != null)
                    {
                        var uri = resAttr.Value;
                        if (uri.Contains("Geo#"))
                        {
                            model.Entities.Add(new NerTextEntity(int.Parse(xElement.Attribute(start).Value),
                                int.Parse(xElement.Attribute(end).Value), NerType.Location));
                        }
                    }
                }
            }
            model.ClearedText = File.ReadAllText("testmodeleng_text.txt");

            foreach (var xElement in root.Elements())
            {
                if (xElement.Name.Namespace.NamespaceName.EndsWith("Geo#"))
                {
                    var idattr = xElement.Attribute(nodeId);
                    if (idattr != null)
                    {
                        var val = idattr.Value;
                        Tuple<int, int> pos;
                        //if (objects.TryGetValue(val, out pos))
                        //{
                        //    model.Entities.Add(new NerTextEntity(pos.Item1, pos.Item2, NerType.Location));
                        //}
                    }
                }
            }
            File.WriteAllText("testmodeleng_abby.txt", model.Render());
            model.ClearedText = text;
            File.WriteAllText("testmodeleng_abby_native.txt", model.Render());
        }
    }
}
