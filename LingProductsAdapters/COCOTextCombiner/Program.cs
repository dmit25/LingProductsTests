using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COCOTextCombiner
{
    class Program
    {
        static void Main(string[] args)
        {
            var outDir = @"E:\tmp\ibm\COCA\pieces";
            Directory.CreateDirectory(outDir);
            foreach (var category in Directory.EnumerateDirectories(@"E:\tmp\ibm\COCA\texts"))
            {
                var name = Path.GetFileName(category);
                Directory.CreateDirectory(Path.Combine(outDir, name));
                foreach (var file in Directory.EnumerateFiles(category))
                {
                    foreach (var line in File.ReadAllLines(file))
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            File.WriteAllText(Path.Combine(outDir, name, Guid.NewGuid() + ".txt"), line);
                        }
                    }
                }
            }

        }

        private static void GenerateRandomSample()
        {
            var files = Directory.EnumerateFiles(@"texts\text_newspaper_lsp", "*", SearchOption.AllDirectories).ToArray();
            Directory.CreateDirectory("result");
            var indexRandomizer = new Random();
            var linesRandomizer = new Random();
            var numberOfSentences = 100000;
            var maxLineIndex = 200000;
            Parallel.For(0,
                numberOfSentences,
                (i) =>
                {
                    var fileIndex = indexRandomizer.Next(0, files.Length);
                    var lines = File.ReadLines(files[fileIndex]);
                    while (true)
                    {
                        try
                        {
                            var lineIndex = linesRandomizer.Next(0, 200000);
                            var sent = lines.ElementAt(lineIndex);
                            var filename = Guid.NewGuid().ToString() + ".txt";
                            File.WriteAllText(Path.Combine("result", filename), sent);
                            //Console.WriteLine("Sent saved in {0}", filename);
                            break;
                        }
                        catch (Exception)
                        {
                        }
                    }
                });
        }
    }
}
