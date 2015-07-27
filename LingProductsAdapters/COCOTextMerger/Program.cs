using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COCOTextMerger
{
    class Program
    {
        static void Main(string[] args)
        {
            var resultFile = "corpus.txt";
            using (var writer = File.CreateText(resultFile))
            {
                foreach (var file in Directory.EnumerateFiles(@"E:\tmp\ibm\ibmcorpus"))
                {
                    writer.WriteLine(File.ReadAllText(file));
                }
            }
        }
    }
}
