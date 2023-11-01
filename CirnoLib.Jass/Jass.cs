using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CirnoLib.Jass
{
    public sealed class JassVariable
    {
        public string Type;
        public string Name;
        public string Value;
        public bool IsArray = false;
        public bool IsConstant = false;
        public JassVariable(string Type = null, string Name = null, string Value = null, bool IsArray = false, bool IsConstant = false)
        {
            this.Type = Type;
            this.Name = Name;
            this.Value = Value;
            this.IsArray = IsArray;
            this.IsConstant = IsConstant;
        }

        public override string ToString()
        {
            StringBuilder Builder = new StringBuilder();
            if (IsConstant) Builder.Append("constant ");
            Builder.AppendFormat("{0} ", Type);
            if (IsArray) Builder.Append("array ");
            Builder.Append(Name);
            if (!string.IsNullOrEmpty(Value)) Builder.AppendFormat("={0}", Value);
            return Builder.ToString();
        }

        public static JassVariable Parse(string Line)
        {
            string[] Element = Line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            JassVariable Variable = new JassVariable();
            StringBuilder Builder = new StringBuilder();
            foreach (var item in Element)
            {
                switch (item)
                {
                    case "constant":
                        Variable.IsConstant = true;
                        continue;
                    case "array":
                        Variable.IsArray = true;
                        continue;
                }
                if (Variable.Type == null)
                {
                    Variable.Type = item;
                    continue;
                }
                Builder.Append(item);
            }
            string value = Builder.ToString();
            int index = value.IndexOf('=');
            if (index != -1)
            {
                Variable.Name = value.Substring(0, index);
                Variable.Value = value.Substring(++index, value.Length - index);
            }
            else Variable.Name = value;
            return Variable;
        }
    }

    public sealed class JassVariableList : List<JassVariable>
    {
        public JassVariable this[string name] => Find(item => item.Name == name);
    }

    public sealed class JassNative
    {

    }

    public sealed class JassFunction
    {
        public bool Native = false;
        public string Name;
        public JassVariableList TakesVariable = new JassVariableList();
        public string ReturnType;
        public JassVariableList LocalVariable = new JassVariableList();
        public List<string> Lines = new List<string>();

        public JassFunction(string Name, string ReturnType, bool Native = false)
        {
            this.Native = Native;
            this.Name = Name;
            this.ReturnType = ReturnType;
        }

        public override string ToString()
        {
            StringBuilder Builder = new StringBuilder();
            foreach (var item in ToStringArray())
                Builder.AppendFormat("{0}\r\n", item);
            return Builder.ToString();
        }
        public string[] ToStringArray()
        {
            List<string> Lines = new List<string>();
            StringBuilder Builder = new StringBuilder();
            Builder.Append(Native ? "native" : "function");
            Builder.AppendFormat(" {0} takes ", Name);
            if (TakesVariable.Count > 0)
                for (int i = 0; i < TakesVariable.Count; i++)
                    Builder.AppendFormat("{0} {1}{2}", TakesVariable[i].Type,
                        TakesVariable[i].Name, i + 1 == TakesVariable.Count ? ' ' : ',');
            else Builder.Append("nothing ");
            Builder.AppendFormat("returns {0}", ReturnType);
            Lines.Add(Builder.ToString());
            if (!Native)
            {
                foreach (var item in LocalVariable)
                    Lines.Add($"local {item}");
                Lines.AddRange(this.Lines);
                Lines.Add("endfunction");
            }
            return Lines.ToArray();
        }

        public static JassFunction Parse(string script)
        {
            return Parse(script.Replace("\r", "\n").Replace("\n\n", "\n").Split('\n'));
        }

        public static JassFunction Parse(string[] script)
        {
            string[] initLine = script[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            JassFunction function = new JassFunction(initLine[1], initLine[initLine.Length - 1], initLine[0] == "native");
            int takesIndex = script[0].IndexOf("takes") + 6;
            initLine = script[0].Substring(takesIndex, script[0].LastIndexOf("return") - takesIndex).Replace(',', ' ').Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (!initLine[0].Equals("nothing"))
            {
                for (int i = 0; i < initLine.Length; i += 2)
                    function.TakesVariable.Add(new JassVariable(initLine[i], initLine[i + 1]));
            }
            if (!function.Native)
            {
                int CurrentLine = 1;
                for (; CurrentLine < script.Length; CurrentLine++)
                {
                    if (script[CurrentLine].Length < 5 || !script[CurrentLine].Substring(0, 5).Equals("local")) break;
                    function.LocalVariable.Add(JassVariable.Parse(script[CurrentLine].Substring(6, script[CurrentLine].Length - 6)));
                }
                while (!script[CurrentLine].Trim().Equals("endfunction"))
                    function.Lines.Add(script[CurrentLine++]);
            }
            return function;
        }
    }

    public sealed class JassFunctionList : List<JassFunction>
    {
        public JassFunction this[string name] => Find(item => item.Name == name);
    }

    public sealed class JassDocument
    {
        public JassVariableList GlobalVariable = new JassVariableList();
        public JassFunctionList Functions = new JassFunctionList();

        public static JassDocument Parse(byte[] Data)
            => Parse(Encoding.UTF8.GetString(Data));

        public static JassDocument Parse(string script)
            => Parse(script.Replace("\r", "\n").Replace("\n\n", "\n").Split('\n'));

        public static JassDocument Parse(string[] script)
        {
            JassDocument document = new JassDocument();
            List<string> Lines = new List<string>(script);

            int CurrentLine = 0;

            for (bool IsComment = false; CurrentLine < Lines.Count; CurrentLine++)
            {
                int index = 0;
                if (!IsComment && (index = Lines[CurrentLine].IndexOf("/*")) == -1) continue;
                int end = Lines[CurrentLine].IndexOf("*/");
                if (end == -1)
                {
                    Lines[CurrentLine] = Lines[CurrentLine].Substring(0, index);
                    continue;
                }
                string text = string.Empty;
                if (index > 0)
                    text = Lines[CurrentLine].Substring(0, index);
                Lines[CurrentLine] = text + Lines[CurrentLine].Substring(end, Lines[CurrentLine].Length - end);
                IsComment = false;
            }

            for (int i = Lines.Count - 1; i >= 0; i--)
                if (string.IsNullOrEmpty(Lines[i])) Lines.RemoveAt(i);

            CurrentLine = 0;
            for (bool IsGlobalFound = false; CurrentLine < Lines.Count; CurrentLine++)
            {
                if (!IsGlobalFound)
                {
                    if (Lines[CurrentLine].Equals("globals"))
                        IsGlobalFound = true;
                    continue;
                }
                if (Lines[CurrentLine].Equals("endglobals"))
                {
                    CurrentLine++;
                    break;
                }
                document.GlobalVariable.Add(JassVariable.Parse(Lines[CurrentLine]));
            }
            List<string> FunctionLines = new List<string>();
            for (; CurrentLine < Lines.Count; CurrentLine++)
            {
                string prefix = Lines[CurrentLine].Trim().Split(' ')[0];
                if (prefix.Length < 6) continue;
                switch (prefix)
                {
                    case "native":
                        document.Functions.Add(JassFunction.Parse(Lines[CurrentLine].Trim()));
                        break;
                    case "function":
                        do FunctionLines.Add(Lines[CurrentLine].Trim());
                        while (!Lines[CurrentLine++].Trim().Equals("endfunction"));
                        CurrentLine--;
                        document.Functions.Add(JassFunction.Parse(FunctionLines.ToArray()));
                        FunctionLines.Clear();
                        break;
                }
                if (CurrentLine >= Lines.Count) break;
            }
            return document;
        }

        public override string ToString() => ToString(true);

        public string ToString(bool Indent, int Interval = 4)
        {
            int IndentLevel = 0;
            bool stringMode = false;
            string IndentText = string.Empty.PadLeft(Interval);
            StringBuilder Builder = new StringBuilder();
            string[] lines = ToStringArray();
            for (int i = 0; i < lines.Length; i++)
            {
                var item = lines[i].Trim();
                switch (item.Split(' ')[0].Split('/')[0])
                {
                    case "endglobals":
                    case "endloop":
                    case "endif":
                    case "endfunction":
                    case "endlibrary":
                    case "endscope":
                        IndentLevel--;
                        break;
                }

                int Indented = item.IndexOf("else") == 0 ? 1 : 0;
                while (Indent && !stringMode && Indented < IndentLevel)
                {
                    Builder.Append(IndentText);
                    Indented++;
                }
                if ((Regex.Matches(item, "\"").Count - (Regex.Matches(item, "\\\\\"").Count - Regex.Matches(item, "\\\\\\\\\"").Count)) % 2 == 1)stringMode = !stringMode;
                if (stringMode) Builder.Append(item + "\\n");
                else Builder.AppendLine(item);
                if (Indent && item == "endfunction") Builder.AppendLine();
                if (item.IndexOf("globals") == 0
                 || item.IndexOf("loop") == 0
                 || item.IndexOf("if") == 0
                 || item.IndexOf("function") == 0
                 || item.IndexOf("library") == 0
                 || item.IndexOf("scope") == 0
                 || item.IndexOf("static if") == 0)
                    IndentLevel++;
            }
            string ret = Builder.ToString();
            if (Indent)
            {
                ret = Regex.Replace(ret, "(\r\n +)else\\1endif", item => $"{item.Groups[1].Value}endif");
                ret = Regex.Replace(ret, " ?== ?true", string.Empty);
                ret = Regex.Replace(ret, "([^0-9])(\\.[0-9])", item => $"{item.Groups[1].Value}0{item.Groups[2].Value}");
                ret = Regex.Replace(ret, "([0-9]\\.)([^0-9a-zA-Z])", item => $"{item.Groups[1].Value}0{item.Groups[2].Value}");
                ret = Regex.Replace(ret, "\\$([0-9a-fA-F]+)", item =>
                {
                    int value = 0;
                    string rawcode = null;
                    try
                    {
                        value = Convert.ToInt32(item.Groups[1].Value, 16);
                        try
                        {
                            rawcode = value.GetRawString();
                        }
                        catch
                        {
                            goto Error;
                        }
                    }
                    catch
                    {
                        Console.WriteLine(item.Groups[1].Value);
                        return item.Value;
                    }


                    if (Regex.IsMatch(rawcode, "[0-9A-Za-z]{4}"))
                        return $"'{rawcode}'";
                    Error: return item.Groups[1].Value.Length >= 8 ? $"0x{item.Groups[1].Value}" : value.ToString();
                });
                ret = Regex.Replace(ret, "'([^+])'", item => ((int)item.Groups[1].Value[0]).ToString());
            }
            return ret;
        }

        public string[] ToStringArray()
        {
            List<string> Lines = new List<string> { "globals" };
            foreach (var item in GlobalVariable)
                Lines.Add(item.ToString());
            Lines.Add("endglobals");
            foreach (var item in Functions)
                Lines.AddRange(item.ToStringArray());
            return Lines.ToArray();
        }
    }
}
