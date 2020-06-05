namespace CirnoLib.Format.MDXLib
{
    public class Float3 :IArrayable
    {
        public float X;
        public float Y;
        public float Z;

        public byte[] ToArray()
        {
            byte[] value = new byte[12];
            value.Write(0, X);
            value.Write(4, Y);
            value.Write(8, Z);
            return value;
        }
    }
}