using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace CirnoLib.MPQ.CompressLib
{
    public static class ZLibHelper
    {
        private static Inflater inflater = new Inflater();
        private static Deflater deflater = new Deflater(Deflater.BEST_COMPRESSION);

        public static byte[] Deflate(byte[] input)
        {
            deflater.SetInput(input);
            deflater.Finish();

            using (MemoryStream ms = new MemoryStream())
            {
                byte[] readbyte = new byte[input.Length];
                while (!deflater.IsNeedingInput)
                {
                    int read = deflater.Deflate(readbyte);
                    ms.Write(readbyte, 0, read);

                    if (deflater.IsFinished) break;
                }
                deflater.Reset();
                return ms.ToArray();
            }
        }

        public static byte[] Inflate(byte[] input)
        {
            inflater.Reset();
            inflater.SetInput(input, 0, input.Length - 2);

            using (MemoryStream ms = new MemoryStream())
            {
                byte[] outputBuffer = new byte[1024];
                int read;

                while ((read = inflater.Inflate(outputBuffer)) > 0)
                {
                    ms.Write(outputBuffer, 0, read);

                    if (inflater.IsFinished) break;
                }

                return ms.ToArray();
            }
        }

        /*public static byte[] Deflate(byte[] input, int skip)
        {
            using (var inputStream = new MemoryStream(input, 2 + skip, input.Length - 2 - skip))
            using (var deflate = new DeflateStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                var buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = deflate.Read(buffer, 0, buffer.Length)) > 0)
                {
                    outputStream.Write(buffer, 0, bytesRead);
                }

                return outputStream.ToArray();
            }
        }

        public static byte[] Inflate(byte[] input)
        {
            using (MemoryStream comrMem = new MemoryStream())
            {
                DeflateStream defalte = new DeflateStream(comrMem, CompressionMode.Compress, true);
                defalte.Write(input, 0, input.Length); // 일단 Byte를 CompressStream에 쓴다 -> 자동으로 압축됨
                defalte.Close(); // 닫음 -> MemoryStream에 입력됨
                return comrMem.ToArray(); // 기존에 있던건 너무 더러워서 ToArray()로 깔끔하게 설정
            }
        }*/
    }
}