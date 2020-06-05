using CirnoLib.Jass;
using System;
using System.Collections.Generic;

namespace CirnoLib.Format
{
    [Serializable]
    public sealed class Doodad : List<Doodad.Data>, IArrayable
    {
        public static readonly uint FileID = 0x6F643357;    // W3do
        public static readonly int FileVersion = 8;
        public static readonly int SubVersion = 0xB;
        public List<Special> Specials = new List<Special>();

        public sealed class Data
        {
            public int TypeID;      // RawCode
            public int variation = 0;
            public float CoordX = 0;
            public float CoordY = 0;
            public float CoordZ = 0;
            public float RotateAngle = 0;
            public float ScaleX = 1;
            public float ScaleY = 1;
            public float ScaleZ = 1;
            public byte flags = 2;
            public byte Life = 0x64;
            /// <summary>
            /// map item table pointer (for dropped items on death)
            /// if -1 => no item table used
            /// if >= 0 => the item table with this number will be dropped on death
            /// </summary>
            public int ItemTablePointer = -1;
            public List<List<DropItem>> DropItemSet;
            public sealed class DropItem
            {
                public int ID;      // RawCode
                public int Chance;
            }
            public int CreationNumber;
        }

        public sealed class Special
        {
            public int TypeID;      // RawCode
            public int Z;
            public int X;
            public int Y;
        }

        public Data CreateDestructable(string objectid, float x, float y, float face, float scale, int variation)
            => CreateDestructable(objectid.Numberize(), x, y, face, scale, variation);
        public Data CreateDestructable(int objectid, float x, float y, float face, float scale, int variation)
        {
            Data item = new Data
            {
                TypeID = objectid,
                CoordX = x,
                CoordY = y,
                RotateAngle = face,
                ScaleX = scale,
                ScaleY = scale,
                ScaleZ = scale,
                variation = variation
            };

            Add(item);
            return item;
        }
        public Data CreateDestructableZ(string objectid, float x, float y, float z, float face, float scale, int variation)
           => CreateDestructableZ(objectid.Numberize(), x, y, z, face, scale, variation);
        public Data CreateDestructableZ(int objectid, float x, float y, float z, float face, float scale, int variation)
        {
            Data item = new Data
            {
                TypeID = objectid,
                CoordX = x,
                CoordY = y,
                CoordZ = z,
                RotateAngle = face,
                ScaleX = scale,
                ScaleY = scale,
                ScaleZ = scale,
                variation = variation
            };

            Add(item);
            return item;
        }

        public static Doodad Parse(byte[] data)
        {
            Doodad doo = new Doodad();
            using (ByteStream bs = new ByteStream(data))
            {
                bs.Skip(0xC);
                int Count = bs.ReadInt32();
                for (int i = 0; i < Count; i++)
                {
                    Data d = new Data();
                    d.TypeID = bs.ReadInt32().ReverseByte();
                    d.variation = bs.ReadInt32();
                    d.CoordX = bs.ReadSingle();
                    d.CoordY = bs.ReadSingle();
                    d.CoordZ = bs.ReadSingle();
                    d.RotateAngle = bs.ReadSingle();
                    d.ScaleX = bs.ReadSingle();
                    d.ScaleY = bs.ReadSingle();
                    d.ScaleZ = bs.ReadSingle();
                    d.flags = bs.ReadByte();
                    d.Life = bs.ReadByte();
                    d.ItemTablePointer = bs.ReadInt32();
                    int LoopCount = bs.ReadInt32();
                    for (int j = 0; j < LoopCount; j++)
                    {
                        if (d.DropItemSet == null) d.DropItemSet = new List<List<Data.DropItem>>();
                        int ItemTableCount = bs.ReadInt32();
                        List<Data.DropItem> ItemTable = null;
                        for (int k = 0; k < ItemTableCount; k++)
                        {
                            if (ItemTable == null) ItemTable = new List<Data.DropItem>();
                            ItemTable.Add(new Data.DropItem
                            {
                                ID = bs.ReadInt32().ReverseByte(),
                                Chance = bs.ReadInt32()
                            });
                        }
                        if (ItemTable != null) d.DropItemSet.Add(ItemTable);
                    }
                    d.CreationNumber = bs.ReadInt32();
                    doo.Add(d);
                }
                bs.Skip(4);
                Count = bs.ReadInt32();
                for (int i = 0; i < Count; i++)
                {
                    doo.Specials.Add(new Special
                    {
                        TypeID = bs.ReadInt32().ReverseByte(),
                        Z = bs.ReadInt32(),
                        X = bs.ReadInt32(),
                        Y = bs.ReadInt32()
                    });
                }
            }
            return doo;
        }

        public new byte[] ToArray()
        {
            using (ByteStream bs = new ByteStream())
            {
                bs.Write(FileID);
                bs.Write(FileVersion);
                bs.Write(SubVersion);
                bs.Write(Count);
                foreach (var item in this)
                {
                    bs.Write(item.TypeID.ReverseByte());
                    bs.Write(item.variation);
                    bs.Write(item.CoordX);
                    bs.Write(item.CoordY);
                    bs.Write(item.CoordZ);
                    bs.Write(item.RotateAngle);
                    bs.Write(item.ScaleX);
                    bs.Write(item.ScaleY);
                    bs.Write(item.ScaleZ);
                    bs.Write(item.flags);
                    bs.Write(item.Life);
                    bs.Write(item.ItemTablePointer);
                    if (item.DropItemSet == null) bs.Write(0);
                    else
                    {
                        bs.Write(item.DropItemSet.Count);
                        foreach (var initem in item.DropItemSet)
                        {
                            bs.Write(initem.Count);
                            foreach (var ininitem in initem)
                            {
                                bs.Write(ininitem.ID.ReverseByte());
                                bs.Write(ininitem.Chance);
                            }
                        }
                    }
                    bs.Write(item.CreationNumber);
                }
                bs.WriteEmpty(4);
                bs.Write(Specials.Count);
                foreach (var item in Specials)
                {
                    bs.Write(item.TypeID.ReverseByte());
                    bs.Write(item.Z);
                    bs.Write(item.X);
                    bs.Write(item.Y);
                }
                return bs.ToArray();
            }
        }
    }
}
