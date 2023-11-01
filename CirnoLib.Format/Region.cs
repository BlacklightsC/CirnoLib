using System;
using System.Collections.Generic;

namespace CirnoLib.Format
{
    [Serializable]
    public sealed class Region : List<Region.Data>, IArrayable
    {
        public const int Version = 5;

        public sealed class Data
        {
            public float Left = 0;
            public float Right = 0;
            public float Bottom = 0;
            public float Top = 0;
            public string Name = string.Empty;
            public int Index = 0;
            public int WeatherID = 0;           // RawCode
            public string AmbientSound = string.Empty;
            public byte[] Color = new byte[] { 0xFF, 0x80, 0x80 };
        }

        public Data Rect(string name, float minx, float miny, float maxx, float maxy)
        {
            if (string.IsNullOrEmpty(name)) return null;
            Data data = new Data
            {
                Name = name,
                Left = minx,
                Right = miny,
                Bottom = maxx,
                Top = maxy,
            };
            Add(data);
            return data;
        }

        public static Region Parse(byte[] data)
        {
            Region region = new Region();
            using (ByteStream bs = new ByteStream(data))
            {
                bs.Skip(4);
                int Count = bs.ReadInt32();
                for (int i = 0; i < Count; i++)
                {
                    region.Add(new Data
                    {
                        Left = bs.ReadSingle(),
                        Right = bs.ReadSingle(),
                        Bottom = bs.ReadSingle(),
                        Top = bs.ReadSingle(),
                        Name = bs.ReadString(),
                        Index = bs.ReadInt32(),
                        WeatherID = bs.ReadBytes(4).ReverseCopy(true).ToInt32(),
                        AmbientSound = bs.ReadString(),
                        Color = bs.ReadBytes(3)
                    });
                    bs.Skip(1);
                }
            }
            return region;
        }

        public new byte[] ToArray()
        {
            using (ByteStream bs = new ByteStream())
            {
                bs.Write(Version);
                bs.Write(Count);
                foreach (var item in this)
                {
                    bs.Write(item.Left);
                    bs.Write(item.Right);
                    bs.Write(item.Bottom);
                    bs.Write(item.Top);
                    bs.Write(item.Name);
                    bs.Write(item.Index);
                    bs.Write(item.WeatherID.GetBytes().ReverseCopy(true));
                    bs.Write(item.AmbientSound);
                    bs.Write(item.Color);
                    bs.WriteByte(0xFF);
                }
                return bs.ToArray();
            }
        }
    }
}
