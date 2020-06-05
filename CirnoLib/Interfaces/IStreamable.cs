using System.IO;

namespace CirnoLib
{
    /// <summary>
    /// 해당 클래스가 스트림의 형식으로 직렬화할 수 있음을 나타냅니다.
    /// </summary>
    public interface IStreamable
    {
        Stream GetStream();

        void WriteFile(string fileName);

        void WriteFile(Stream stream);

        void WriteFile(FileInfo file);
    }
}
