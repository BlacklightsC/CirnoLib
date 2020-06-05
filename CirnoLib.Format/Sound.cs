using System;
using System.Collections.Generic;

namespace CirnoLib.Format
{
    [Serializable]
    public sealed class Sound : List<Sound.Data>, IArrayable
    {
        public const int Version = 1;

        public struct EAXEffects : IArrayable
        {
            private readonly enums value;

            private EAXEffects(string value)
            {
                switch(value.ToUpper())
                {
                    case "DEFAULTEAXON": this.value = enums.DefaultEAXON; break;
                    case "COMBATSOUNDSEAX": this.value = enums.CombatSoundsEAX; break;
                    case "KOTODRUMSEAX": this.value = enums.KotoDrumsEAX; break;
                    case "SPELLSEAX": this.value = enums.SpellsEAX; break;
                    case "MISSILESEAX": this.value = enums.MissilesEAX; break;
                    case "HEROACKSEAX": this.value = enums.HeroAcksEAX; break;
                    case "DOODADSEAX": this.value = enums.DoodadsEAX; break;
                    default: this.value = enums.None; break;
                }
            }

            public enum enums
            {
                DefaultEAXON,
                CombatSoundsEAX,
                KotoDrumsEAX,
                SpellsEAX,
                MissilesEAX,
                HeroAcksEAX,
                DoodadsEAX,
                None
            }

            public static implicit operator string(EAXEffects from)
            {
                return from.value.ToString();
            }

            public static implicit operator EAXEffects(string from)
            {
                return new EAXEffects(from);
            }

            public byte[] ToArray()
            {
                switch (value)
                {
                    case enums.None: return new byte[1];
                    default:
                        using (ByteStream bs = new ByteStream())
                        {
                            bs.Write(value.ToString());
                            return bs.ToArray();
                        }
                }
            }
        }

        [Flags]
        public enum Flag : int
        {
            Looping = 1,
            Sound3D = 2,
            StopWhenOutOfRange = 4,
            Music = 8,
            Default = 0x10
        }

        public enum Channels : int
        {
            General = 0,
            UnitSelection = 1,
            UnitAcknowledgement = 2,
            UnitMovement = 3,
            UnitReady = 4,
            Combat = 5,
            Error = 6,
            Music = 7,
            UserInterface = 8,
            LoopingMovement = 9,
            LoopingAmbient = 10,
            Animations = 11,
            Constructions = 12,
            Birth = 13,
            Fire = 14
        }

        public sealed class Data
        {
            public string VariableName = string.Empty;
            public string FilePath = string.Empty;
            public EAXEffects EAXEffect = "DefaultEAXON";
            public Flag Flags = Flag.Default;
            public int FadeInRate = 10;
            public int FadeOutRate = 10;
            public int Volume = 127;
            public float Pitch = 1;
            public float Unknown1 = 0;
            public int Unknown2 = 8;
            public Channels Channel = Channels.General;
            public float MinDistance = 0;
            public float MaxDistance = 10000;
            public float DistanceCutOff = 3000;
            public float ConeAnglesInside = 0;
            public float ConeAnglesOutside = 0;
            public int ConeAnglesOutsizeVolume = 0x7F;
            public float ConeOrientationX = 0;
            public float ConeOrientationY = 0;
            public float ConeOrientationZ = 0;

            public void SetMusic()
            {
                EAXEffect = "None";
                Flags = Flag.Default | Flag.Music;
                FadeInRate = 10;
                FadeOutRate = 10;
                Volume = 0;
                Unknown2 = 0;
                Channel = Channels.General;
                MinDistance = 0;
                MaxDistance = 0;
                DistanceCutOff = 0;
                ConeAnglesInside = 0;
                ConeAnglesOutside = 0;
                ConeAnglesOutsizeVolume = 0;
                ConeOrientationX = 0;
                ConeOrientationY = 0;
                ConeOrientationZ = 0;
            }
        }

        public static Sound Parse(byte[] data)
        {
            Sound snd = new Sound();
            using (ByteStream bs = new ByteStream(data))
            {
                bs.Skip(4);
                int Count = bs.ReadInt32();
                for (int i = 0; i < Count; i++)
                    snd.Add(new Data
                    {
                        VariableName = bs.ReadString(),
                        FilePath = bs.ReadString(),
                        EAXEffect = bs.ReadString(),
                        Flags = (Flag)bs.ReadInt32(),
                        FadeInRate = bs.ReadInt32(),
                        FadeOutRate = bs.ReadInt32(),
                        Volume = bs.ReadInt32(),
                        Pitch = bs.ReadSingle(),
                        Unknown1 = bs.ReadSingle(),
                        Unknown2 = bs.ReadInt32(),
                        Channel = (Channels)bs.ReadInt32(),
                        MinDistance = bs.ReadSingle(),
                        MaxDistance = bs.ReadSingle(),
                        DistanceCutOff = bs.ReadSingle(),
                        ConeAnglesInside = bs.ReadSingle(),
                        ConeAnglesOutside = bs.ReadSingle(),
                        ConeAnglesOutsizeVolume = bs.ReadInt32(),
                        ConeOrientationX = bs.ReadSingle(),
                        ConeOrientationY = bs.ReadSingle(),
                        ConeOrientationZ = bs.ReadSingle()
                    });
            }
            return snd;
        }

        public new byte[] ToArray()
        {
            using (ByteStream bs = new ByteStream())
            {
                bs.Write(Version);
                bs.Write(Count);
                foreach (var item in this)
                {
                    bs.Write(item.VariableName);
                    bs.Write(item.FilePath);
                    bs.Write(item.EAXEffect);
                    bs.Write((int)item.Flags);
                    bs.Write(item.FadeInRate);
                    bs.Write(item.FadeOutRate);
                    bs.Write(item.Volume);
                    bs.Write(item.Pitch);
                    bs.Write(item.Unknown1);
                    bs.Write(item.Unknown2);
                    bs.Write((int)item.Channel);
                    bs.Write(item.MinDistance);
                    bs.Write(item.MaxDistance);
                    bs.Write(item.DistanceCutOff);
                    bs.Write(item.ConeAnglesInside);
                    bs.Write(item.ConeAnglesOutside);
                    bs.Write(item.ConeAnglesOutsizeVolume);
                    bs.Write(item.ConeOrientationX);
                    bs.Write(item.ConeOrientationY);
                    bs.Write(item.ConeOrientationZ);
                }
                return bs.ToArray();
            }
        }
    }
}
