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
            var files = Directory.EnumerateFiles("texts", "*", SearchOption.AllDirectories).ToArray();
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
