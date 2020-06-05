using System;
using System.Text;
using System.Collections.Generic;

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
        public JassVariable this[string name] { get => Find(item => item.Name == name); }
    }

    public sealed class JassFunction
    {
        public string Name;
        public JassVariableList TakesVariable = new JassVariableList();
        public string ReturnType;
        public JassVariableList LocalVariable = new JassVariableList();
        public List<string> Lines = new List<string>();

        public JassFunction(string Name, string ReturnType)
        {
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
            StringBuilder Builder = new StringBuilder("function ");
            Builder.AppendFormat("{0} takes ", Name);
            if (TakesVariable.Count > 0)
                for (int i = 0; i < TakesVariable.Count; i++)
                    Builder.AppendFormat("{0} {1}{2}", TakesVariable[i].Type,
                        TakesVariable[i].Name, i + 1 == TakesVariable.Count ? ' ' : ',');
            else Builder.Append("nothing ");
            Builder.AppendFormat("returns {0}", ReturnType);
            Lines.Add(Builder.ToString());
            foreach (var item in LocalVariable)
                Lines.Add(string.Format("local {0}", item.ToString()));
            Lines.AddRange(this.Lines);
            Lines.Add("endfunction");
            return Lines.ToArray();
        }

        public static JassFunction Parse(string script)
        {
            return Parse(script.Replace("\r", "\n").Replace("\n\n", "\n").Split('\n'));
        }

        public static JassFunction Parse(string[] script)
        {
            string[] initLine = script[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            JassFunction function = new JassFunction(initLine[1], initLine[initLine.Length - 1]);
            int takesIndex = script[0].IndexOf("takes") + 6;
            initLine = script[0].Substring(takesIndex, script[0].LastIndexOf("return") - takesIndex).Replace(',', ' ').Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (string.Compare(initLine[0], "nothing", true) != 0)
            {
                for (int i = 0; i < initLine.Length; i += 2)
                    function.TakesVariable.Add(new JassVariable(initLine[i], initLine[i + 1]));
            }

            int CurrentLine = 1;
            for (; CurrentLine < script.Length; CurrentLine++)
            {
                if (script[CurrentLine].Length < 5 || string.Compare(script[CurrentLine].Substring(0, 5), "local", true) != 0) break;
                function.LocalVariable.Add(JassVariable.Parse(script[CurrentLine].Substring(6, script[CurrentLine].Length - 6)));
            }
            while (string.Compare(script[CurrentLine], "endfunction", true) != 0)
                function.Lines.Add(script[CurrentLine++]);
            return function;
        }
    }

    public sealed class JassFunctionList : List<JassFunction>
    {
        public JassFunction this[string name] { get => Find(item => item.Name == name); }
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
                    if (string.Compare(Lines[CurrentLine], "globals", true) == 0)
                        IsGlobalFound = true;
                    continue;
                }
                if (string.Compare(Lines[CurrentLine], "endglobals", true) == 0)
                {
                    CurrentLine++;
                    break;
                }
                document.GlobalVariable.Add(JassVariable.Parse(Lines[CurrentLine]));
            }
            List<string> FunctionLines = new List<string>();
            for (; CurrentLine < Lines.Count; CurrentLine++)
            {
                if (Lines[CurrentLine].Length < 8 || string.Compare(Lines[CurrentLine].Substring(0, 8), "function", true) != 0) continue;
                do FunctionLines.Add(Lines[CurrentLine]);
                while (string.Compare(Lines[CurrentLine++], "endfunction", true) != 0);
                CurrentLine--;
                document.Functions.Add(JassFunction.Parse(FunctionLines.ToArray()));
                FunctionLines.Clear();
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
            foreach (var item in ToStringArray())
            {
                switch (item)
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
                Builder.AppendLine(item);
                if (Indent && item == "endfunction") Builder.AppendLine();
                if (item == "globals"
                 || item == "loop"
                 || item.IndexOf("if") == 0
                 || item.IndexOf("function") == 0
                 || item.IndexOf("library") == 0
                 || item.IndexOf("scope") == 0)
                    IndentLevel++;
            }
            return Builder.ToString();
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
