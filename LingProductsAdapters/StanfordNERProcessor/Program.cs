using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using edu.stanford.nlp.ie.crf;
using Linguistics.Crf.Corpus;

namespace StanfordNERProcessor
{
    class Program
    {
        static void Main()
        {
            // Path to the folder with classifies models
            var jarRoot = @"E:\SystemsTests\stanford\stanford-ner-2015-04-20";
            var classifiersDirecrory = jarRoot + @"\classifiers";

            // Loading 3 class classifier model
            var classifier = CRFClassifier.getClassifierNoExceptions(
                classifiersDirecrory + @"\english.all.3class.distsim.crf.ser.gz");
            var text =
                System.IO.File.ReadAllText(
                    @"C:\code\github\LingProductsTests\TestResults\strict\eng\2talk\testmodeleng_etalon.txt");
            var parser = new HtmlCorpusParser();
            var model = parser.Parse(text, 350);

            var input = model.ClearedText;
            var timer = Stopwatch.StartNew();
            System.IO.File.WriteAllText("stanford.txt", classifier.classifyWithInlineXML(input));
            timer.Stop();
            Console.WriteLine("Elapsed {0} ms", timer.ElapsedMilliseconds);
            Console.ReadLine();
        }
    }
}
