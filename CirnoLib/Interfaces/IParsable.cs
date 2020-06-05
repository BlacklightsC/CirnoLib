using System.IO;

namespace CirnoLib
{
    public interface IParsable
    {
        IParsable Parse(byte[] data);
    }
}
