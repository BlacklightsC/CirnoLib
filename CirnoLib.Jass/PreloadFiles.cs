using System;
using System.IO;

namespace CirnoLib.Jass
{
    public sealed class PreloadFiles : IDisposable
    {
        private const string PreloadGenStart = "function PreloadFiles takes nothing returns nothing\n";
        private const string PreloadGenEnd = "\tcall PreloadEnd( 0.0 )\r\n\nendfunction\n\n";
        private const string PreloadLine = "\tcall Preload( \"{0}\" )";
        private readonly FileStream stream;
        private readonly TextWriter writer;
        private bool EndOfLine = false;

        public PreloadFiles(string path)
        {
            stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            writer = new StreamWriter(stream);
            writer.WriteLine(PreloadGenStart);
            writer.Flush();
        }

        public void WriteLine(params string[] value)
        {
            if (EndOfLine) return;
            foreach (var item in value)
                writer.WriteLine(PreloadLine, item);
            writer.Flush();
        }

        public void Clear()
        {
            if (EndOfLine) return;
            stream.Seek(0, SeekOrigin.Begin);
            stream.SetLength(0);
            writer.WriteLine(PreloadGenStart);
        }

        public void EndWrite()
        {
            using (stream)
            using (writer)
            {
                writer.WriteLine(PreloadGenEnd);
            }
            EndOfLine = true;
        }

        public static string[] Preloader(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(stream))
            {
                string value = reader.ReadToEnd();
                return Preload.Preloader(value);
            }
        }

        public static void PreloadGen(string path, params string[] value)
        {
            using (PreloadFiles p = new PreloadFiles(path))
            {
                p.WriteLine(value);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // 중복 호출을 검색하려면

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 관리되는 상태(관리되는 개체)를 삭제합니다.
                    if (!EndOfLine) EndWrite();
                }

                // TODO: 관리되지 않는 리소스(관리되지 않는 개체)를 해제하고 아래의 종료자를 재정의합니다.
                // TODO: 큰 필드를 null로 설정합니다.

                disposedValue = true;
            }
        }

        // TODO: 위의 Dispose(bool disposing)에 관리되지 않는 리소스를 해제하는 코드가 포함되어 있는 경우에만 종료자를 재정의합니다.
        // ~PreloadFiles() {
        //   // 이 코드를 변경하지 마세요. 위의 Dispose(bool disposing)에 정리 코드를 입력하세요.
        //   Dispose(false);
        // }

        // 삭제 가능한 패턴을 올바르게 구현하기 위해 추가된 코드입니다.
        public void Dispose()
        {
            // 이 코드를 변경하지 마세요. 위의 Dispose(bool disposing)에 정리 코드를 입력하세요.
            Dispose(true);
            // TODO: 위의 종료자가 재정의된 경우 다음 코드 줄의 주석 처리를 제거합니다.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
