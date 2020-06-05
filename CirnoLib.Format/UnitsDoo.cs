using CirnoLib.Jass;

using System;
using System.Collections.Generic;

namespace CirnoLib.Format
{
    [Serializable]
    public sealed class UnitsDoo : List<UnitsDoo.Data>, IArrayable
    {
        public const uint FileID = 0x6F643357;    // W3do
        public const int FileVersion = 8;
        public const int SubVersion = 0xB;
        
        public sealed class Data
        {
            /// <summary>
            /// iDNR = random item, uDNR = random unit
            /// </summary>
            public int TypeID;              // RawCode
            public int variation = 0;
            public float CoordX = 0;
            public float CoordY = 0;
            public float CoordZ = 0;
            public float RotateAngle = 0;
            public float ScaleX = 1;
            public float ScaleY = 1;
            public float ScaleZ = 1;
            public byte flags = 2;
            /// <summary>
            /// owner (player1 = 0, 15 = neutral passive)
            /// </summary>
            public int PlayerNumber = 0;
            /// <summary>
            /// -1 = use default
            /// </summary>
            public int HitPoint = -1;
            /// <summary>
            /// -1 = use default, 0 = unit doesn't have mana
            /// </summary>
            public int ManaPoint = -1;
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
            /// <summary>
            /// gold amount (default = 12500)
            /// </summary>
            public int Gold = 12500;
            /// <summary>
            /// target acquisition (-1 = normal, -2 = camp)
            /// </summary>
            public float Target = -1;
            /// <summary>
            /// set to1 for non hero units and items
            /// </summary>
            public int Level = 1;
            /// <summary>
            /// 0 = use default
            /// </summary>
            public int STR = 0, AGI = 0, INT = 0;
            public List<InvItem> InvItems;
            public sealed class InvItem
            {
                public int Slot;
                public int ID;      // RawCode
            }
            public List<ModAbil> ModAbils;
            public sealed class ModAbil
            {
                public int ID;      // RawCode
                public int IsAutoCast;
                public int Level;
            }
            public int RandomFlag = 0;

            // 0
            public byte[] LevRandom = new byte[] { 1, 0, 0 };
            public byte ItemClass = 0;
            // 1
            public int UnitGroup = 0;
            public int PosNumber = 0;
            // 2
            public List<RandomUnit> RandomUnits;
            public sealed class RandomUnit
            {
                public int ID;      // RawCode
                public int Chance;
            }

            public int CustomColor = -1;
            public int WayGate = -1;
            public int CreationNumber = -1;
        }

        public Data CreateUnit(int player, string unitid, float x, float y, float face, int number = -1)
            => CreateUnit(player, unitid.Numberize(), x, y, face, number);
        public Data CreateUnit(int player, int unitid, float x, float y, float face, int number = -1)
        {
            Data unit = new Data
            {
                PlayerNumber = player,
                TypeID = unitid,
                CoordX = x,
                CoordY = y,
                RotateAngle = face,
                CreationNumber = number
            };

            Add(unit);
            return unit;
        }
        public Data CreateItem(string itemid, float x, float y)
           => CreateItem(itemid.Numberize(), x, y);
        public Data CreateItem(int itemid, float x, float y)
        {
            Data item = new Data
            {
                TypeID = itemid,
                CoordX = x,
                CoordY = y
            };

            Add(item);
            return item;
        }

        public void PurgeNumber()
        {
            int seq = 0;
            foreach (var item in this)
            {
                if (item.CreationNumber != -1) continue;
                int index;
                while ((index = FindIndex(data => data.CreationNumber == seq)) != -1)
                    seq++;
                item.CreationNumber = seq++;
            }
            Sort((a, b) => a.CreationNumber - b.CreationNumber);
        }

        public static UnitsDoo Parse(byte[] data)
        {
            UnitsDoo doo = new UnitsDoo();
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
                    d.PlayerNumber = bs.ReadInt32();
                    bs.Skip(2);
                    d.HitPoint = bs.ReadInt32();
                    d.ManaPoint = bs.ReadInt32();
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
                    d.Gold = bs.ReadInt32();
                    d.Target = bs.ReadInt32();
                    d.Level = bs.ReadInt32();
                    d.STR = bs.ReadInt32();
                    d.AGI = bs.ReadInt32();
                    d.INT = bs.ReadInt32();
                    LoopCount = bs.ReadInt32();
                    for (int j = 0; j < LoopCount; j++)
                    {
                        if (d.InvItems == null) d.InvItems = new List<Data.InvItem>();
                        d.InvItems.Add(new Data.InvItem
                        {
                            Slot = bs.ReadInt32(),
                            ID = bs.ReadInt32().ReverseByte()
                        });
                    }
                    LoopCount = bs.ReadInt32();
                    for (int j = 0; j < LoopCount; j++)
                    {
                        if (d.ModAbils == null) d.ModAbils = new List<Data.ModAbil>();
                        d.ModAbils.Add(new Data.ModAbil
                        {
                            ID = bs.ReadInt32().ReverseByte(),
                            IsAutoCast = bs.ReadInt32(),
                            Level = bs.ReadInt32()
                        });
                    }
                    d.RandomFlag = bs.ReadInt32();
                    switch (d.RandomFlag)
                    {
                        case 0:
                            d.LevRandom = bs.ReadBytes(3);
                            d.ItemClass = bs.ReadByte();
                            break;
                        case 1:
                            d.UnitGroup = bs.ReadInt32();
                            d.PosNumber = bs.ReadInt32();
                            break;
                        case 2:
                            LoopCount = bs.ReadInt32();
                            for (int j = 0; j < LoopCount; j++)
                            {
                                if (d.RandomUnits == null) d.RandomUnits = new List<Data.RandomUnit>();
                                d.RandomUnits.Add(new Data.RandomUnit
                                {
                                    ID = bs.ReadInt32().ReverseByte(),
                                    Chance = bs.ReadInt32()
                                });
                            }
                            break;
                    }
                    d.CustomColor = bs.ReadInt32();
                    d.WayGate = bs.ReadInt32();
                    d.CreationNumber = bs.ReadInt32();
                    doo.Add(d);
                }
            }
            return doo;
        }

        public new byte[] ToArray()
        {
            PurgeNumber();
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
                    bs.Write(item.PlayerNumber);
                    bs.WriteEmpty(2);
                    bs.Write(item.HitPoint);
                    bs.Write(item.ManaPoint);
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
                    bs.Write(item.Gold);
                    bs.Write(item.Target);
                    bs.Write(item.Level);
                    bs.Write(item.STR);
                    bs.Write(item.AGI);
                    bs.Write(item.INT);
                    if (item.InvItems == null) bs.Write(0);
                    else
                    {
                        bs.Write(item.InvItems.Count);
                        foreach (var initem in item.InvItems)
                        {
                            bs.Write(initem.Slot);
                            bs.Write(initem.ID.ReverseByte());
                        }
                    }
                    if (item.ModAbils == null) bs.Write(0);
                    else
                    {
                        bs.Write(item.ModAbils.Count);
                        foreach (var initem in item.ModAbils)
                        {
                            bs.Write(initem.ID.ReverseByte());
                            bs.Write(initem.IsAutoCast);
                            bs.Write(initem.Level);
                        }
                    }
                    bs.Write(item.RandomFlag);
                    switch (item.RandomFlag)
                    {
                        case 0:
                            bs.Write(item.LevRandom);
                            bs.Write(item.ItemClass);
                            break;
                        case 1:
                            bs.Write(item.UnitGroup);
                            bs.Write(item.PosNumber);
                            break;
                        case 2:
                            if (item.RandomUnits == null) bs.Write(0);
                            else
                            {
                                bs.Write(item.RandomUnits.Count);
                                foreach (var initem in item.RandomUnits)
                                {
                                    bs.Write(initem.ID.ReverseByte());
                                    bs.Write(initem.Chance);
                                }
                            }
                            break;
                    }
                    bs.Write(item.CustomColor);
                    bs.Write(item.WayGate);
                    bs.Write(item.CreationNumber);
                }
                return bs.ToArray();
            }
        }
    }
}
