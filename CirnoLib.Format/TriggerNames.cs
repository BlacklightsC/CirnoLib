using System;
using System.Collections.Generic;

namespace CirnoLib.Format
{
    [Serializable]
    public sealed class TriggerNames : IArrayable
    {
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

            internal int _RunToMapInit = 1;
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
            public enum FunctionType : int
            {
                Event = 0,
                Condition = 1,
                Action = 2
            }

            public string Name = string.Empty;

            internal int _Enabled = 1;
            public bool Enabled {
                get => _Enabled == 1;
                set => _Enabled = value ? 1 : 0;
            }

            public List<Parameter> Parameters { get; } = new List<Parameter>();

            public int Unknown = 0;

            public List<Function> Functions = null;

            public byte[] ToArray()
            {
                using (ByteStream bs = new ByteStream())
                {
                    bs.Write((int)Type);
                    bs.Write(Name);
                    bs.Write(_Enabled);
                    foreach (var item in Parameters)
                        bs.Write(item);
                    bs.Write(Unknown);
                    if (Functions == null) bs.Write(0);
                    else
                    {
                        bs.Write(Functions.Count);
                        foreach (var item in Functions)
                            bs.Write(item);
                    }
                    return bs.ToArray();
                }
            }
        }

        public sealed class Parameter : IArrayable
        {
            public ParameterType Type;
            public enum ParameterType : int
            {
                Preset = 0,
                Variable = 1,
                Function = 2,
                String = 3
            }

            public string Value = string.Empty;

            public List<Parameter> Parameters = null;

            public byte[] ToArray()
            {
                using (ByteStream bs = new ByteStream())
                {
                    bs.Write((int)Type);
                    bs.Write(Value);
                    if (Type == ParameterType.Function)
                    {
                        bs.Write(1);
                        bs.Write(3);
                        bs.Write(Value);
                        bs.Write(1);
                        if (Parameters != null)
                            foreach (var item in Parameters)
                                bs.Write(item);
                    }
                    else
                        bs.Write(0);
                    bs.Write(0);
                    return bs.ToArray();
                }
            }

        }

        [Obsolete]
        public static TriggerNames Parse(byte[] data)
        {
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
                    int FunctionLoopCount = bs.ReadInt32();
                    for (int j = 0; j < FunctionLoopCount; j++)
                    {
                        var func = new Function
                        {
                            Type = (Function.FunctionType)bs.ReadInt32(),
                            Name = bs.ReadString(),
                            _Enabled = bs.ReadInt32()
                        };
                        throw new NotImplementedException();
                        // TODO : ParamParse
                        trig.Functions.Add(func);
                    }
                    wtg.Triggers.Add(trig);
                }
            }
            return wtg;
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
    }
}