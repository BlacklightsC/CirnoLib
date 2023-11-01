using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace CirnoLib.Format
{
    [Serializable]
    public sealed class TriggerNames : IArrayable
    {
        public enum FunctionType : int
        {
            Event = 0,
            Condition = 1,
            Action = 2,
            Call = 3
        }

        public const uint FileID = 0x21475457;   // WTG!
        public const int FileVersion = 7;

        public List<Category> Categories { get; } = new List<Category>();
        public sealed class Category : IArrayable
        {
            public int Index = 0;
            public string Name = string.Empty;

            internal int _Type = 0;
            public bool IsComment {
                get => _Type == 1;
                set => _Type = value ? 1 : 0;
            }

            public byte[] ToArray()
            {
                using (ByteStream bs = new ByteStream())
                {
                    bs.Write(Index);
                    bs.Write(Name);
                    bs.Write(_Type);
                    return bs.ToArray();
                }
            }
        }
        public Category CreateCategory(string name)
        {
            Category category = new Category();
            category.Name = name;
            category.Index = Categories.Max(i => i.Index) + 1;
            Categories.Add(category);
            return category;
        }

        public int Unknown = 2;

        public List<Variable> Variables { get; } = new List<Variable>();
        public sealed class Variable : IArrayable
        {
            public string Name = string.Empty;
            public string Type = string.Empty;
            public int Unknown = 1;

            internal int _IsArray = 0;
            public bool IsArray {
                get => _IsArray == 1;
                set => _IsArray = value ? 1 : 0;
            }

            internal int _ArraySize = 1;
            public int ArraySize {
                get => _ArraySize;
                set => _ArraySize = value > 0 ? value : 1;
            }

            internal int _Initialization = 0;
            public bool Initialization {
                get => _Initialization == 1;
                set {
                    _Initialization = value ? 1 : 0;
                    if (!value)
                        _InitialValue = string.Empty;
                }
            }

            internal string _InitialValue = string.Empty;
            public string InitialValue {
                get => _InitialValue;
                set {
                    if (Initialization)
                        _InitialValue = value;
                }
            }

            public byte[] ToArray()
            {
                using (ByteStream bs = new ByteStream())
                {
                    bs.Write(Name);
                    bs.Write(Type);
                    bs.Write(Unknown);
                    bs.Write(_IsArray);
                    bs.Write(_ArraySize);
                    bs.Write(_Initialization);
                    bs.Write(InitialValue);
                    return bs.ToArray();
                }
            }
        }

        public List<Trigger> Triggers { get; } = new List<Trigger>();
        public sealed class Trigger : IArrayable
        {
            public string Name = string.Empty;
            public string Description = string.Empty;

            internal int _IsComment = 0;
            public bool IsComment {
                get => _IsComment == 1;
                set {
                    _IsComment = value ? 1 : 0;
                    if (value)
                    {
                        _Enabled = 1;
                        _IsCustomText = 0;
                        _InitialState = 0;
                        _RunToMapInit = 0;
                    }
                }
            }

            internal int _Enabled = 1;
            public bool Enabled {
                get => _Enabled == 1;
                set {
                    if (_IsComment == 0)
                        _Enabled = value ? 1 : 0;
                }
            }

            internal int _IsCustomText = 0;
            public bool IsCustomText {
                get => _IsCustomText == 1;
                set {
                    if (_IsComment == 0)
                        _IsCustomText = value ? 1 : 0;
                }
            }

            internal int _InitialState = 0;
            public bool InitialState {
                get => _InitialState == 0;
                set {
                    if (_IsComment == 0)
                        _InitialState = value ? 0 : 1;
                }
            }

            internal int _RunToMapInit = 0;
            public bool RunToMapInit {
                get => _RunToMapInit == 1;
                set {
                    if (_IsComment == 0)
                        _RunToMapInit = value ? 1 : 0;
                }
            }

            public int CategoryIndex = 0;

            public List<Function> Functions { get; } = new List<Function>();

            public byte[] ToArray()
            {
                using (ByteStream bs = new ByteStream())
                {
                    bs.Write(Name);
                    bs.Write(Description);
                    bs.Write(_IsComment);
                    bs.Write(_Enabled);
                    bs.Write(_IsCustomText);
                    bs.Write(_InitialState);
                    bs.Write(_RunToMapInit);
                    bs.Write(CategoryIndex);
                    bs.Write(Functions.Count);
                    foreach (var item in Functions)
                        bs.Write(item);
                    return bs.ToArray();
                }
            }
        }

        public sealed class Function : IArrayable
        {
            public FunctionType Type;

            public int Category = 0;

            public string Name = string.Empty;

            internal int _Enabled = 1;
            public bool Enabled {
                get => _Enabled == 1;
                set => _Enabled = value ? 1 : 0;
            }

            public List<Parameter> Parameters { get; } = new List<Parameter>();

            public List<Function> Functions = null;

            internal static void Parse(ByteStream bs, List<Function> parent, bool inner)
            {
                int LoopCount = bs.ReadInt32();
                for (int j = 0; j < LoopCount; j++)
                {
                    var func = new Function { Type = (FunctionType)bs.ReadInt32() };
                    if (inner) func.Category = bs.ReadInt32();
                    func.Name = bs.ReadString();
                    func._Enabled = bs.ReadInt32();
                    if (func._Enabled != 0 && func._Enabled != 1) throw new Exception();
                    Parameter.Parse(bs, func.Parameters, ECAParamCount.TryGetValue(func.Name, out int paramCount) ? paramCount : 20);
                    if (bs.Int32 > 0) Parse(bs, func.Functions = new List<Function>(), true);
                    else bs.Skip(4);
                    parent.Add(func);
                }
            }

            public byte[] ToArray(bool inner)
            {
                using (ByteStream bs = new ByteStream())
                {
                    bs.Write((int)Type);
                    if (inner) bs.Write(Category);
                    bs.Write(Name);
                    bs.Write(_Enabled);
                    foreach (var item in Parameters)
                        bs.Write(item);
                    if (Functions == null) bs.Write(0);
                    else
                    {
                        bs.Write(Functions.Count);
                        foreach (var item in Functions)
                            bs.Write(item.ToArray(true));
                    }
                    return bs.ToArray();
                }
            }

            public byte[] ToArray() => ToArray(false);
        }

        public sealed class Parameter : IArrayable
        {
            public ParameterType Type;
            public enum ParameterType : int
            {
                None = -1,
                Preset = 0,
                Variable = 1,
                Function = 2,
                String = 3
            }

            public FunctionType FuncType;

            public string Value = string.Empty;
            public string CallValue = string.Empty;
            public Parameter ArrayIndex = null;
            public List<Parameter> Parameters = null;

            internal static Parameter Parse(ByteStream bs, List<Parameter> parent, int count)
            {
                long Start;
                for (int i = 0; i < count; i++)
                {
                    Start = bs.Position;
                    try
                    {
                        int Type = bs.ReadInt32();
                        if (Type < -1 || 3 < Type) goto Error;
                        var param = new Parameter
                        {
                            Type = (ParameterType)Type,
                            Value = bs.ReadString()
                        };
                        if (((Type == 0 || Type == 1) && param.Value.Length == 0)
                         || (param.Value.Length == 1 && param.Value[0] < 0x20)
                         || Type == 0  && param.Value.IndexOf(' ') != -1)
                            goto Error;
                        int flag = bs.ReadInt32();
                        if (flag == 1)
                        {
                            if (param.Type != ParameterType.Function) goto Error;
                            Type = bs.ReadInt32();
                            if (Type < 0 || 3 < Type) goto Error;
                            switch (Type)
                            {
                                default: goto Error;
                                case 0:
                                case 1:
                                case 2:
                                    param.CallValue = bs.ReadString();
                                    break;
                                case 3:
                                    if (param.Value != bs.ReadString()) goto Error;
                                    param.CallValue = param.Value;
                                    break;
                            }
                            param.FuncType = (FunctionType)Type;
                            if (bs.ReadInt32() != 1) goto Error;
                            Parse(bs, param.Parameters = new List<Parameter>(), CALLParamCount.TryGetValue(param.Value, out int paramCount) ? paramCount : 20);
                            if (bs.ReadInt32() != 0) goto Error;
                        }
                        else if (flag != 0) goto Error;
                        flag = bs.ReadInt32();
                        if (flag == 1) param.ArrayIndex = Parse(bs, null, 1);
                        else if (flag != 0) goto Error;
                        if (parent == null) return param;
                        else parent.Add(param);
                    }
                    catch
                    {
                        goto Error;
                    }
                }
                return null;
            Error:
                bs.Position = Start;
                return null;
            }

            public byte[] ToArray()
            {
                using (ByteStream bs = new ByteStream())
                {
                    bs.Write((int)Type);
                    bs.Write(Value);
                    if (Type == ParameterType.Function)
                    {
                        bs.Write(1);
                        bs.Write((int)FuncType);
                        bs.Write(FuncType == FunctionType.Call ? Value : CallValue);
                        bs.Write(1);
                        if (Parameters != null)
                            foreach (var item in Parameters)
                                bs.Write(item);
                    }
                    bs.Write(0);
                    if (Type == ParameterType.Variable && ArrayIndex != null)
                    {
                        bs.Write(1);
                        bs.Write(ArrayIndex);
                    }
                    else bs.Write(0);
                    return bs.ToArray();
                }
            }
        }

        private static Dictionary<string, int> ECAParamCount;
        private static Dictionary<string, int> CALLParamCount;
        private static TriggerNames ret;
        private static void InternalParse(byte[] data, string uiDataPath)
        {
            if (uiDataPath != null)
            {
                try
                {
                    ECAParamCount = new Dictionary<string, int>();
                    CALLParamCount = new Dictionary<string, int>();
                    string[] lines = File.ReadAllLines(uiDataPath);
                    int flag = 0;
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        if (line[0] == '[')
                            switch (line)
                            {
                                default: flag = 0; break;
                                case "[TriggerEvents]":
                                case "[TriggerConditions]":
                                case "[TriggerActions]": flag = 1; break;
                                case "[TriggerCalls]": flag = 2; break;
                            }
                        else
                            switch (flag)
                            {
                                case 1:
                                    if (line[0] != '_')
                                    {
                                        int length = line.IndexOf('=');
                                        string[] param = line.Substring(length + 1).Split(',');
                                        ECAParamCount.Add(line.Substring(0, length), param.Length <= 1 || param[1] == "nothing" ? 0 : param.Length - 1);
                                    }
                                    break;
                                case 2:
                                    if (line[0] != '_')
                                    {
                                        int length = line.IndexOf('=');
                                        string[] param = line.Substring(length + 1).Split(',');
                                        CALLParamCount.Add(line.Substring(0, length), param.Length <= 3 || param[1] == "nothing" ? 0 : param.Length - 3);
                                    }
                                    break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    ECAParamCount = null;
                    CALLParamCount = null;
                    Console.WriteLine(ex.Message);
                }
            }

            TriggerNames wtg = new TriggerNames();
            using (ByteStream bs = new ByteStream(data))
            {
                bs.Skip(8);
                int LoopCount = bs.ReadInt32();
                for (int i = 0; i < LoopCount; i++)
                    wtg.Categories.Add(new Category
                    {
                        Index = bs.ReadInt32(),
                        Name = bs.ReadString(),
                        _Type = bs.ReadInt32()
                    });
                wtg.Unknown = bs.ReadInt32();
                LoopCount = bs.ReadInt32();
                for (int i = 0; i < LoopCount; i++)
                    wtg.Variables.Add(new Variable
                    {
                        Name = bs.ReadString(),
                        Type = bs.ReadString(),
                        Unknown = bs.ReadInt32(),
                        _IsArray = bs.ReadInt32(),
                        _ArraySize = bs.ReadInt32(),
                        _Initialization = bs.ReadInt32(),
                        InitialValue = bs.ReadString()
                    });

                LoopCount = bs.ReadInt32();
                for (int i = 0; i < LoopCount; i++)
                {
                    Trigger trig = new Trigger
                    {
                        Name = bs.ReadString(),
                        Description = bs.ReadString(),
                        _IsComment = bs.ReadInt32(),
                        _Enabled = bs.ReadInt32(),
                        _IsCustomText = bs.ReadInt32(),
                        _InitialState = bs.ReadInt32(),
                        _RunToMapInit = bs.ReadInt32(),
                        CategoryIndex = bs.ReadInt32()
                    };
                    if (trig.Name.Length == 0)
                        throw new InvalidDataException("trig.Name.Length can't 0");
                    Function.Parse(bs, trig.Functions, false);
                    wtg.Triggers.Add(trig);
                }
            }
            ECAParamCount = null;
            CALLParamCount = null;
            ret = wtg;
        }


        public static TriggerNames Parse(byte[] data, string uiDataPath)
        {
            Thread thread = new Thread(() => InternalParse(data, uiDataPath), 0x4000000);
            thread.Start();
            thread.Join();
            return ret;
        }

        public byte[] ToArray()
        {
            using (ByteStream bs = new ByteStream())
            {
                bs.Write(FileID);
                bs.Write(FileVersion);
                bs.Write(Categories.Count);
                foreach (var item in Categories)
                    bs.Write(item);
                bs.Write(Unknown);
                bs.Write(Variables.Count);
                foreach (var item in Variables)
                    bs.Write(item);
                bs.Write(Triggers.Count);
                foreach (var item in Triggers)
                    bs.Write(item);
                return bs.ToArray();
            }
        }

        private void DumpParameter(List<Parameter> p, StringBuilder buffer, ref int indent)
        {
            if (p == null) return;
            for (int i = 0; i < p.Count; i++)
            {
                buffer.AppendLine($"{string.Empty.PadLeft(indent * 2)}[P] {(p[i].Type == Parameter.ParameterType.Function ? p[i].CallValue : p[i].Value)}");
                indent++;
                if (p[i].Parameters != null) DumpParameter(p[i].Parameters, buffer, ref indent);
                if (p[i].ArrayIndex != null)
                {
                    buffer.AppendLine($"{string.Empty.PadLeft(indent * 2)}[A] {p[i].ArrayIndex.Value}");
                    indent++;
                    DumpParameter(p[i].ArrayIndex.Parameters, buffer, ref indent);
                    indent--;
                }
                indent--;
            }
        }
        private void DumpFunction(List<Function> p, bool inner, StringBuilder buffer, ref int indent)
        {
            if (p == null) return;
            for (int i = 0; i < p.Count; i++)
            {
                buffer.AppendLine($"{string.Empty.PadLeft(indent * 2)}[F{(inner ? p[i].Category : (int)p[i].Type)}] {p[i].Name}");
                indent++;
                DumpParameter(p[i].Parameters, buffer, ref indent);
                DumpFunction(p[i].Functions, true, buffer, ref indent);
                indent--;
            }
        }
        public string DumpToText()
        {
            StringBuilder buffer = new StringBuilder();
            int indent = 0;
            foreach (var variable in Variables)
            {
                buffer.Append(variable.Type);
                if (variable.IsArray) buffer.Append($"[{variable.ArraySize}]");
                buffer.Append($" {variable.Name}");
                if (variable.Initialization) buffer.Append($"= {variable.InitialValue}");
                buffer.AppendLine();
            }
            buffer.AppendLine();
            foreach (var category in Categories)
            {
                buffer.AppendLine($"[C] {category.Name}");
                indent++;
                foreach (var trigger in Triggers)
                {
                    if (category.Index == trigger.CategoryIndex)
                    {
                        buffer.AppendLine($"{string.Empty.PadLeft(indent * 2)}[T] {trigger.Name}");
                        indent++;
                        DumpFunction(trigger.Functions, false, buffer, ref indent);
                        indent--;
                    }
                }
                indent--;
            }
            return buffer.ToString();
        }
    }
}