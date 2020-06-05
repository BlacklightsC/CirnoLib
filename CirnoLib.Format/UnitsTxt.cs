using System.Collections.Generic;

using CirnoLib.Jass;

namespace CirnoLib.Format
{
    public sealed class UnitsTxt : List<UnitsTxt.Element>, IArrayable
    {
        public Element this[string ID] { get => Find(item => item.RawCode == RawCode.Numberize(ID)); }
        public Element this[byte[] ID] { get => Find(item => item.RawCode.GetRawCode().Compare(ID)); }

        public sealed class Element : ListDictionary<string, object>
        {
            public int RawCode = 0;

            public byte[] ToArray()
            {
                using (ByteStream bs = new ByteStream())
                {
                    bs.Write('[');
                    bs.Write(RawCode.GetRawCode());
                    bs.Write("]\r\n", false);
                    foreach (var item in this)
                    {
                        bs.Write(item.Key, false);
                        bs.Write('=');
                        switch (item.Value.GetType().ToString())
                        {
                            case "System.Int32":
                                bs.Write(item.Value.ToString(), false);
                                break;
                            case "System.String":
                                bs.Write(item.Value as string, false);
                                break;
                            case "System.byte[]":
                                bs.Write(item.Value as byte[]);
                                break;
                            case "System.Int32[]":
                                bs.Write((item.Value as int[]).GetRawCodeList());
                                break;
                        }
                        bs.Write("\r\n", false);
                    }
                    return bs.ToArray();
                }
            }
        }

        public static UnitsTxt Parse(byte[] data)
        {
            UnitsTxt txt = new UnitsTxt();
            Element part = null;
            foreach (var item in data.Replace(new byte[] { 0xD }, new byte[] { 0xA }).Replace(new byte[] { 0xA, 0xA }, new byte[] { 0xA }).Split(0xA))
            {
                int index;
                if (item.Length == 0) continue;
                else if (item[0] == '[')
                {
                    if (part != null) txt.Add(part);
                    part = new Element { RawCode = item.SubArray(1, 4).Numberize() };
                }
                else if ((index = item.SearchPattern(true, 61 /* '=' */)) != -1)
                {
                    string key = item.SubArray(0, index).GetString();
                    byte[] bytes = item.SubArray(index + 1);
                    int[] rawCodes = bytes.GetNumberizeList();
                    if (rawCodes != null)
                    {
                        part.Add(key, rawCodes);
                        continue;
                    }
                    string chars = bytes.GetString();
                    if (chars.IndexOf('�') != -1)
                        part.Add(key, bytes);
                    else part.Add(key, chars);
                }
                else continue;
            }
            txt.Add(part);
            return txt;
        }

        public new byte[] ToArray()
        {
            using (ByteStream bs = new ByteStream())
            {
                foreach (var item in this)
                    bs.Write(item.ToArray());
                return bs.ToArray();
            }
        }
    }
}
