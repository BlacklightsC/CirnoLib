using System.Collections.Generic;

namespace CirnoLib.Format
{
    public sealed class ImportedFile : List<ImportedFile.Data>, IArrayable
    {
        public int Version { get; private set; } = 1;
        public sealed class Data
        {
            public bool UseCustomPath = false;
            public string Path;
        }

        public new void Sort()
        {
            Sort((a, b) => a.Path.CompareTo(b.Path));
        }

        public static ImportedFile Parse(byte[] data)
        {
            ImportedFile list = new ImportedFile();
            using (ByteStream bs = new ByteStream(data))
            {
                bs.ReadInt32();
                int LoopCount = bs.ReadInt32();
                for (int i = 0; i < LoopCount; i++)
                {
                    Data item = new Data();
                    byte UseCustomPath = bs.ReadByte();
                    item.UseCustomPath = UseCustomPath == 0xA || UseCustomPath == 0xD;
                    item.Path = bs.ReadString();
                    list.Add(item);
                }
            }
            return list;
        }

        public static ImportedFile Convert(string[] items)
        {
            ImportedFile list = new ImportedFile();
            foreach (var item in items)
            {
                if (item.IndexOf("war3map.") != -1) continue;
                list.Add(new Data
                {
                    UseCustomPath = item.IndexOf("war3mapImported\\") != 0,
                    Path = item
                });
            }
            return list;
        }

        public new byte[] ToArray()
        {
            using (ByteStream bs = new ByteStream())
            {
                bs.Write(Version);
                bs.Write(Count);
                foreach (var item in this)
                {
                    bs.WriteByte((byte)(item.UseCustomPath ? 0xD : 0x8));
                    bs.Write(item.Path);
                }
                return bs.ToArray();
            }
        }
    }
}
