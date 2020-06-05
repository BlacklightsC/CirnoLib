using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CirnoLib.Jass
{
    public sealed class Preload
    {
        private const string PreloadGenStart = "function PreloadFiles takes nothing returns nothing\n";
        private const string PreloadGenEnd = "\tcall PreloadEnd( 0.0 )\r\n\nendfunction\n\n\r\n";
        private const string PreloadLine = "\tcall Preload( \"{0}\" )\r\n";
        private readonly StringBuilder builder;

        public Preload() => builder = new StringBuilder(PreloadGenStart);

        public void WriteLine(params string[] value)
        {
            foreach (var item in value)
                builder.AppendFormat(PreloadLine, item);
        }

        public void Clear()
        {
            builder.Clear();
            builder.Append(PreloadGenStart);
        }

        public override string ToString()
        {
            return builder + PreloadGenEnd;
        }

        public static string[] Preloader(string value)
        {
            Regex pattern = new Regex("\tcall Preload\\( \"(.+?)\" \\)\r\n", RegexOptions.Singleline);
            List<string> lines = new List<string>();
            foreach (Match item in pattern.Matches(value))
                lines.Add(item.Groups[1].Value);
            return lines.ToArray();
        }

        public static string PreloadGen(params string[] value)
        {
            Preload p = new Preload();
            p.WriteLine(value);
            return p.ToString();
        }
    }
}
