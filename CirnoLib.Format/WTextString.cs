using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CirnoLib.Format
{
    public class WTextString : SortedSet<WTextString.Element>, IArrayable
    {
        public WTextString() : base(new IndexComparer()) { }

        public Element this[int index] {
            get {
                if (Cache == null) Cache = this.ToArray<Element>();
                return Cache[index];
            }
        }

        public Element this[string strIndex] {
            get {
                if (strIndex == null || !strIndex.Contains("TRIGSTR_")) return null;
                int index = int.Parse(strIndex.Substring(8));
                if (Cache == null) Cache = this.ToArray<Element>();
                if (Cache.Length > index && Cache[index].Index == index) return Cache[index];
                for (int i = (index <= Cache.Length ? index : Cache.Length) - 1; i >= 0; i--)
                    if (Cache[i].Index == index)
                        return Cache[i];
                return null;
            }
        }

        public Element[] Cache { get; set; }

        public new bool Add(Element item)
        {
            Cache = null;
            return base.Add(item);
        }
        public new bool Remove(Element item)
        {
            Cache = null;
            return base.Remove(item);
        }
        public new int RemoveWhere(Predicate<Element> match)
        {
            Cache = null;
            return base.RemoveWhere(match);
        }
        public new void Clear()
        {
            Cache = null;
            base.Clear();
        }

        public string TryConvert(string strIndex) => this[strIndex]?.Text ?? strIndex;

        public static WTextString Parse(byte[] data) => Parse(data.GetString());

        public static WTextString Parse(string Text)
        {
            WTextString wts = new WTextString();
            foreach (Match match in Regex.Matches(Text, @"STRING ([0-9]+)[\r\n]+?(// (.+?)[\r\n]+)?\{[\r\n]+(.+?)[\r\n]+?\}"
                                                , RegexOptions.Multiline | RegexOptions.Singleline))
            {
                Element el = new Element
                {
                    Index = int.Parse(match.Groups[1].Value),
                    Comment = match.Groups[3].Value,
                    Text = match.Groups[4].Value
                };
                wts.Add(el);
            }
            return wts;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var item in this)
                builder.Append(item);
            return builder.ToString();
        }

        public byte[] ToArray() => ToString().GetBytes();

        public class Element : IArrayable
        {
            public int Index { get; set; }
            public string Comment { get; set; }
            public string Text { get; set; }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"STRING {Index}");
                if (!string.IsNullOrEmpty(Comment))
                    builder.AppendLine($"// {Comment}");
                builder.AppendLine("{");
                builder.AppendLine(Text);
                builder.AppendLine("}");
                return builder.ToString();
            }

            public byte[] ToArray() => ToString().GetBytes();
        }

        private class IndexComparer : IComparer<Element>
        {
            public int Compare(Element x, Element y)
            {
                return x.Index - y.Index;
            }
        }
    }
}
