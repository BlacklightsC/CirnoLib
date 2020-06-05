using System;
using System.Collections.Generic;

namespace CirnoLib.Format
{
    [Serializable]
    public sealed class Camera : List<Camera.Data>, IArrayable
    {
        public sealed class Data : IArrayable
        {
            public float TargetX = 0;
            public float TargetY = 0;
            public float ZOffset = 0;
            public float Rotation = 0;
            public float AngleOfAttack = 0;
            public float Distance = 0;
            public float Roll = 0;
            public float FieldOfView = 0;
            public float FarClipping = 0;
            public float Unknown = 100;
            public string CinematicName = string.Empty;

            public byte[] ToArray()
            {
                using (ByteStream bs = new ByteStream())
                {
                    bs.Write(TargetX);
                    bs.Write(TargetY);
                    bs.Write(ZOffset);
                    bs.Write(Rotation);
                    bs.Write(AngleOfAttack);
                    bs.Write(Distance);
                    bs.Write(Roll);
                    bs.Write(FieldOfView);
                    bs.Write(FarClipping);
                    bs.Write(Unknown);
                    bs.Write(CinematicName);
                    return bs.ToArray();
                }
            }
        }

        public static Camera Parse(byte[] data)
        {
            Camera cam = new Camera();
            using (ByteStream bs = new ByteStream(data))
            {
                bs.Skip(4);
                int Count = bs.ReadInt32();
                for (int i = 0; i < Count; i++)
                    cam.Add(new Data
                    {
                        TargetX = bs.ReadSingle(),
                        TargetY = bs.ReadSingle(),
                        ZOffset = bs.ReadSingle(),
                        Rotation = bs.ReadSingle(),
                        AngleOfAttack = bs.ReadSingle(),
                        Distance = bs.ReadSingle(),
                        Roll = bs.ReadSingle(),
                        FieldOfView = bs.ReadSingle(),
                        FarClipping = bs.ReadSingle(),
                        Unknown = bs.ReadSingle(),
                        CinematicName = bs.ReadString()
                    });
            }
            return cam;
        }

        public new byte[] ToArray()
        {
            using (ByteStream bs = new ByteStream())
            {
                bs.Write(0);
                bs.Write(Count);
                foreach (var item in this)
                    bs.Write(item);
                return bs.ToArray();
            }
        }
    }
}
