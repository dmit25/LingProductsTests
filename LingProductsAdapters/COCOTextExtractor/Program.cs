using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace COCOTextExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            var infiles = Directory.EnumerateFiles("texts", "*", SearchOption.AllDirectories).ToArray();
            var outfiles = infiles.Select(f => Path.Combine("result", Path.GetDirectoryName(f), Path.GetFileName(f))).ToArray();

            Parallel.For(0, infiles.Length,
                (i, state) =>
                {
                    ExtractClearedText(infiles[i], outfiles[i]);
                });
        }

        private static void ExtractClearedText(string input, string output)
        {
            var watch = Stopwatch.StartNew();
            var text = new StringBuilder(File.ReadAllText(input));
            text = text.Replace("<p>", "\n");
            text = text.Replace(" 's", "'s");
            text = text.Replace(" 'd", "'d");
            text = text.Replace(" 'm", "'m");
            text = text.Replace(" 'm", "'m");
            text = text.Replace(" ' ", "'");
            text = text.Replace("\r\n", "\n");
            text = text.Replace("\r", "\n");
            text = text.Replace(" n't", "n't");
            text = text.Replace("&;", "&");
            var matches = Regex.Matches(text.ToString(), @" \. (?<letter>[A-Z""])");
            var matchValues = new Dictionary<string, string>();
            foreach (Match match in matches)
            {
                matchValues[match.Value] = " .\n" + match.Groups["letter"].Value;
            }
            foreach (var matchValue in matchValues)
            {
                text.Replace(matchValue.Key, matchValue.Value);
            }
            var str = Regex.Replace(text.ToString(), @"##\d+", "\n");
            str = Regex.Replace(str, @"\n+", "\n");
            var lines = str.Split('\n');
            var finalText = new StringBuilder(text.Length);
            foreach (var line in lines)
            {
                var lineStr = line.Trim();
                if (lineStr.Length > 10)
                {
                    if (Regex.IsMatch(lineStr, @"(@ ){3,}|&\w+;"))
                    {
                        continue;
                    }
                    if (lineStr[0] == '"' && lineStr[lineStr.Length - 1] == '"')
                    {
                        finalText.AppendLine(lineStr.Substring(1, lineStr.Length - 2).Trim());
                    }
                    else if (Regex.Matches(lineStr, @"""").Count == 1)
                    {
                        finalText.AppendLine(lineStr.Replace("\"", string.Empty).Trim());
                    }
                    else
                    {
                        finalText.AppendLine(lineStr);
                    }
                }
            }
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.WriteAllText(output, finalText.ToString());
            watch.Stop();
            Console.WriteLine("{0} processed in {1} ms", input, watch.ElapsedMilliseconds);
        }
    }
}
