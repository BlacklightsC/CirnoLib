using System;
using System.Collections.Generic;

namespace CirnoLib.Format
{
    [Serializable]
    public sealed class CustomTextTrigger : List<CustomTextTrigger.CustomText>, IArrayable
    {
        public const int FileVersion = 1;

        public string Comment = string.Empty;

        public CustomText HeadScript = string.Empty;

        [Serializable]
        public struct CustomText : IArrayable
        {
            private readonly string value;

            private CustomText(string value)
                => this.value = value;

            public int Length => value.GetByteCount() + 1;

            public static implicit operator string(CustomText from)
                => from.value;

            public static implicit operator CustomText(string from)
                => new CustomText(from);

            public byte[] ToArray()
            {
                using (ByteStream bs = new ByteStream())
                {
                    if (string.IsNullOrEmpty(value))
                        bs.Write(0);
                    else
                    {
                        bs.Write(Length);
                        bs.Write(value);
                    }
                    return bs.ToArray();
                }
            }

            public override string ToString() => value;
        }

        public static CustomTextTrigger Parse(byte[] data, bool isReforged = false)
        {
            CustomTextTrigger wct = new CustomTextTrigger();
            using (ByteStream bs = new ByteStream(data))
            {
                if (isReforged) bs.Skip(4);
                bs.Skip(4);
                wct.Comment = bs.ReadString();
                if (isReforged)
                {
                    bs.ReadInt32();
                    while (bs.Length - bs.Position > 5)
                    {
                        bs.Skip(4);
                        string str = bs.ReadString();
                        if (string.IsNullOrEmpty(str)) continue;
                        wct.Add(str);
                    }
                }
                else
                {
                    wct.HeadScript = bs.ReadBytes(bs.ReadInt32()).GetString().TrimEnd('\0');
                    int Count = bs.ReadInt32();
                    for (int i = 0; i < Count; i++)
                        wct.Add(bs.ReadBytes(bs.ReadInt32()).GetString().TrimEnd('\0'));
                }
            }
            return wct;
        }

        public new byte[] ToArray()
        {
            using (ByteStream bs = new ByteStream())
            {
                bs.Write(FileVersion);
                bs.Write(Comment);
                bs.Write(HeadScript);
                bs.Write(Count);
                foreach (var item in this)
                    bs.Write(item);
                return bs.ToArray();
            }
        }
    }
}
