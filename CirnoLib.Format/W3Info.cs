using System;
using System.Collections.Generic;

namespace CirnoLib.Format
{
    [Serializable]
    public sealed class W3Info : IArrayable
    {
        #region [    Constant    ]
        /// <summary>
        /// <see cref="Flags"/>에 사용되는 속성에 대한 값입니다.
        /// </summary>
        public const int
            W3I_HIDE_MINIMAP_IN_PREVIEW_SCREENS = 0x1,
            W3I_MODIFY_ALLY_PRIORITIES = 0x2,
            W3I_MELEE_MAP = 0x4,
            W3I_PLAYABLE_MAP_SIZE_WAS_LARGE_AND_HAS_NEVER_BEEN_REDUCED_TO_MEDIUM = 0x8,
            W3I_MASKED_AREA_ARE_PARTIALLY_VISIBLE = 0x10,
            W3I_FIXED_PLAYER_SETTING_FOR_CUSTOM_FORCES = 0x20,
            W3I_USE_CUSTOM_FORCES = 0x40,
            W3I_USE_CUSTOM_TECHTREE = 0x80,
            W3I_USE_CUSTOM_ABILITIES = 0x100,
            W3I_USE_CUSTOM_UPGRADES = 0x200,
            W3I_MAP_PROPERTIES_MENU_OPENED_AT_LEAST_ONCE_SINCE_MAP_CREATION = 0x400,
            W3I_SHOW_WATER_WAVES_ON_CLIFF_SHORES = 0x800,
            W3I_SHOW_WATER_WAVES_ON_ROLLING_SHORES = 0x1000;
        #endregion

        public static readonly int FileFormatVersion = 25;
        public int SaveCount;
        public int EditorVersion;
        public string MapName = string.Empty;
        public string MapAuthor = string.Empty;
        public string MapDescription = string.Empty;
        public string PlayersRecommended = string.Empty;
        public float[] CameraBounds = new float[8];
        public int[] CameraBoundsComplements = new int[4];
        public int MapPlayableAreaWidth;
        public int MapPlayableAreaHeight;
        public int Flags;
        public byte MapMainGroundType;
        public int LoadingScreenPresetIndex;
        public string LoadingScreenModelPath = string.Empty;
        public string MapLoadingScreenText = string.Empty;
        public string MapLoadingScreenTitle = string.Empty;
        public string MapLoadingScreenSubtitle = string.Empty;
        public int UsedGameDataSet;
        public string PrologueScreenPath = string.Empty;
        public string PrologueScreenText = string.Empty;
        public string PrologueScreenTitle = string.Empty;
        public string PrologueScreenSubtitle = string.Empty;
        public int UsesTerrainFog;
        public float FogStartZHeight;
        public float FogEndZHeight;
        public float FogDensity;
        public byte FogRed;
        public byte FogGreen;
        public byte FogBlue;
        public byte FogAlpha;
        public int GlobalWeatherID;
        public string CustomSoundEnvironment;
        public byte TilesetID;
        public byte CustomWaterTintingRed;
        public byte CustomWaterTintingGreen;
        public byte CustomWaterTintingBlue;
        public byte CustomWaterTintingAlpha;
        public List<Player> PlayerList = new List<Player>();
        public sealed class Player
        {
            public int InternalNumber;
            public int Type;
            public int Race;
            public string Name;
            public float StartCoordX;
            public float StartCoordY;
            public int AllyLowPrioritiesFlags;
            public int AllyHighPrioritiesFlags;
        }
        public List<Force> ForceList = new List<Force>();
        public sealed class Force
        {
            public const int
                ALLIED                      = 0x01,
                ALLIED_VICTORY              = 0x02,
                SHARE_VISION                = 0x04,
                SHARE_UNIT_CONTROL          = 0x10,
                SHARE_ADVANCED_UNIT_CONTROL = 0x20;
            public int Flags;
            public int PlayerMasks; // BIt Value
            public string Name;
        }
        public List<UpgradeChange> UpgradeChangeList = new List<UpgradeChange>();
        public sealed class UpgradeChange
        {
            public int PlayerFlags; // Bit Value
            public int ID;
            public int LimitLevel;
            public int Availability;
        }
        public List<TechChange> TechChangeList = new List<TechChange>();
        public sealed class TechChange
        {
            public int PlayerFlags; // Bit Value
            public int ID;
        }

        public List<RandomUnitTable> RandomUnitTableList = new List<RandomUnitTable>();
        public sealed class RandomUnitTable
        {
            int Number;
            string Name;
            List<Group> GroupList = new List<Group>();
            public sealed class Group : List<UnitData>
            {
                private int _Columns;
                public int Columns
                {
                    get => _Columns;
                    set {
                        if (value > 0)
                        _Columns = value;
                        //TODO
                    }
                }
            }
            public sealed class UnitData : List<int> { int Chance; }
        }

        public List<RandomItemTable> RandomItemTableList = new List<RandomItemTable>();
        public sealed class RandomItemTable
        {
            int Number;
            string Name;
            List<List<ItemData>> ItemSetList = new List<List<ItemData>>();
            public sealed class ItemData
            {
                int Chance;
                int ItemID;
            }
        }

        public static W3Info Parse(byte[] data)
        {
            W3Info w3i = new W3Info();
            if (data == null) return w3i;
            using (ByteStream bs = new ByteStream(data))
            {
                bs.Skip(4);
                w3i.SaveCount = bs.ReadInt32();
                w3i.EditorVersion = bs.ReadInt32();
                w3i.MapName = bs.ReadString();
                w3i.MapAuthor = bs.ReadString();
                w3i.MapDescription = bs.ReadString();
                w3i.PlayersRecommended = bs.ReadString();
                for (int i = 0; i < w3i.CameraBounds.Length; i++)
                    w3i.CameraBounds[i] = bs.ReadSingle();
                for (int i = 0; i < w3i.CameraBoundsComplements.Length; i++)
                    w3i.CameraBoundsComplements[i] = bs.ReadInt32();
                w3i.MapPlayableAreaWidth = bs.ReadInt32();
                w3i.MapPlayableAreaHeight = bs.ReadInt32();
                w3i.Flags = bs.ReadInt32();
                w3i.MapMainGroundType = bs.ReadByte();
                w3i.LoadingScreenPresetIndex = bs.ReadInt32();
                w3i.LoadingScreenModelPath = bs.ReadString();
                w3i.MapLoadingScreenText = bs.ReadString();
                w3i.MapLoadingScreenTitle = bs.ReadString();
                w3i.MapLoadingScreenSubtitle = bs.ReadString();
                w3i.UsedGameDataSet = bs.ReadInt32();
                w3i.PrologueScreenPath = bs.ReadString();
                w3i.PrologueScreenText = bs.ReadString();
                w3i.PrologueScreenTitle = bs.ReadString();
                w3i.PrologueScreenSubtitle = bs.ReadString();
                w3i.UsesTerrainFog = bs.ReadInt32();
                w3i.FogStartZHeight = bs.ReadSingle();
                w3i.FogEndZHeight = bs.ReadSingle();
                w3i.FogDensity = bs.ReadSingle();
                w3i.FogRed = bs.ReadByte();
                w3i.FogGreen = bs.ReadByte();
                w3i.FogBlue = bs.ReadByte();
                w3i.FogAlpha = bs.ReadByte();
                w3i.GlobalWeatherID = bs.ReadInt32();
                w3i.CustomSoundEnvironment = bs.ReadString();
                w3i.TilesetID = bs.ReadByte();
                w3i.CustomWaterTintingRed = bs.ReadByte();
                w3i.CustomWaterTintingGreen = bs.ReadByte();
                w3i.CustomWaterTintingBlue = bs.ReadByte();
                w3i.CustomWaterTintingAlpha = bs.ReadByte();
                if (bs.Byte == 0xFF) return w3i;
                int LoopCount = bs.ReadInt32();
                for (int i = 0; i < LoopCount; i++)
                {
                    Player p = new Player();
                    p.InternalNumber = bs.ReadInt32();
                    p.Type = bs.ReadInt32();
                    p.Race = bs.ReadInt32();
                    bs.Skip(4);
                    p.Name = bs.ReadString();
                    p.StartCoordX = bs.ReadSingle();
                    p.StartCoordY = bs.ReadSingle();
                    p.AllyLowPrioritiesFlags = bs.ReadInt32();
                    p.AllyHighPrioritiesFlags = bs.ReadInt32();
                    w3i.PlayerList.Add(p);
                }
                if (bs.Byte == 0xFF) return w3i;
                LoopCount = bs.ReadInt32();
                for (int i = 0; i < LoopCount; i++)
                {
                    Force f = new Force
                    {
                        Flags = bs.ReadInt32(),
                        PlayerMasks = bs.ReadInt32(),
                        Name = bs.ReadString()
                    };
                    w3i.ForceList.Add(f);
                }
                if (bs.Byte == 0xFF) return w3i;
                LoopCount = bs.ReadInt32();
                for (int i = 0; i < LoopCount; i++)
                {
                    UpgradeChange uc = new UpgradeChange
                    {
                        PlayerFlags = bs.ReadInt32(),
                        ID = bs.ReadInt32().ReverseByte(),
                        LimitLevel = bs.ReadInt32(),
                        Availability = bs.ReadInt32()
                    };
                    w3i.UpgradeChangeList.Add(uc);
                }
                if (bs.Byte == 0xFF) return w3i;
                LoopCount = bs.ReadInt32();
                for (int i = 0; i < LoopCount; i++)
                {
                    TechChange tc = new TechChange
                    {
                        PlayerFlags = bs.ReadInt32(),
                        ID = bs.ReadInt32().ReverseByte()
                    };
                    w3i.TechChangeList.Add(tc);
                }
                //w3i.RandomThingsData = data.SubArray((int)bs.Position);
            }
            return w3i;
        }

        public byte[] ToArray()
        {
            using (ByteStream bs = new ByteStream())
            {
                bs.Write(25);
                bs.Write(SaveCount);
                bs.Write(EditorVersion);
                bs.Write(MapName);
                bs.Write(MapAuthor);
                bs.Write(MapDescription);
                bs.Write(PlayersRecommended);
                foreach (var item in CameraBounds)
                    bs.Write(item);
                foreach (var item in CameraBoundsComplements)
                    bs.Write(item);
                bs.Write(MapPlayableAreaWidth);
                bs.Write(MapPlayableAreaHeight);
                bs.Write(Flags);
                bs.Write(MapMainGroundType);
                bs.Write(LoadingScreenPresetIndex);
                bs.Write(LoadingScreenModelPath);
                bs.Write(MapLoadingScreenText);
                bs.Write(MapLoadingScreenTitle);
                bs.Write(MapLoadingScreenSubtitle);
                bs.Write(UsedGameDataSet);
                bs.Write(PrologueScreenPath);
                bs.Write(PrologueScreenText);
                bs.Write(PrologueScreenTitle);
                bs.Write(PrologueScreenSubtitle);
                bs.Write(UsesTerrainFog);
                bs.Write(FogStartZHeight);
                bs.Write(FogEndZHeight);
                bs.Write(FogDensity);
                bs.Write(FogRed);
                bs.Write(FogGreen);
                bs.Write(FogBlue);
                bs.Write(FogAlpha);
                bs.Write(GlobalWeatherID);
                bs.Write(CustomSoundEnvironment);
                bs.Write(TilesetID);
                bs.Write(CustomWaterTintingRed);
                bs.Write(CustomWaterTintingGreen);
                bs.Write(CustomWaterTintingBlue);
                bs.Write(CustomWaterTintingAlpha);
                bs.Write(PlayerList.Count);
                foreach (var item in PlayerList)
                {
                    bs.Write(item.InternalNumber);
                    bs.Write(item.Type);
                    bs.Write(item.Race);
                    bs.Write(1);
                    bs.Write(item.Name);
                    bs.Write(item.StartCoordX);
                    bs.Write(item.StartCoordY);
                    bs.Write(item.AllyLowPrioritiesFlags);
                    bs.Write(item.AllyHighPrioritiesFlags);
                }
                bs.Write(ForceList.Count);
                foreach (var item in ForceList)
                {
                    bs.Write(item.Flags);
                    bs.Write(item.PlayerMasks);
                    bs.Write(item.Name);
                }
                bs.Write(UpgradeChangeList.Count);
                foreach (var item in UpgradeChangeList)
                {
                    bs.Write(item.PlayerFlags);
                    bs.Write(item.ID.ReverseByte());
                    bs.Write(item.LimitLevel);
                    bs.Write(item.Availability);
                }
                bs.Write(TechChangeList.Count);
                foreach (var item in TechChangeList)
                {
                    bs.Write(item.PlayerFlags);
                    bs.Write(item.ID.ReverseByte());
                }
                bs.Write(RandomUnitTableList.Count);
                foreach (var item in RandomUnitTableList)
                {
                    //bs.Write(item.PlayerFlags);
                    //bs.Write(item.ID.ReverseByte());
                }
                bs.Write(RandomItemTableList.Count);
                foreach (var item in RandomItemTableList)
                {
                    //bs.Write(item.PlayerFlags);
                    //bs.Write(item.ID.ReverseByte());
                }

                return bs.ToArray();
            }
        }
    }
}
