using System;
using System.IO;

namespace CirnoLib.MPQ.Struct
{
    public sealed class W3MArchive : MPQArchive
    {
        public W3MHeader MapHeader { get; internal set; }

        public W3MArchive() : base() { MapHeader = new W3MHeader(); }

        /// <summary>
        /// W3M 파일을 데이터에 맞게 읽어서 파싱합니다.
        /// </summary>
        /// <param name="data">읽어낼 W3M 파일</param>
        /// <param name="readOnly">읽기 전용? true일 경우, 파일 데이터를 분리하지 않고 참조값으로 저장합니다. (메모리 사용량 감소 및 일부 프로텍트 무시)</param>
        public W3MArchive(byte[] data, bool readOnly = false, bool tryFindAllKey = false) : base(data, readOnly, tryFindAllKey) { MapHeader = new W3MHeader(data); }

        public W3MArchive(Stream stream, bool tryFindAllKey = false)
        {
            StreamMode = ReadOnly = true;
            Stream = stream;
            MapHeader = new W3MHeader(stream);
            Header = new MPQHeader(this, stream);
            HashTable = new MPQHashTable(this, stream);
            BlockTable = new MPQBlockTable(this, stream, tryFindAllKey);
            Files = new MPQFiles(this, stream, tryFindAllKey);
            Initialized = true;
        }

        /// <summary>
        /// 경로상에 있는 W3M 파일을 데이터에 맞게 읽어서 파싱합니다.
        /// </summary>
        /// <param name="path">읽어낼 W3M 파일의 경로</param>
        /// <param name="streamMode">읽기 전용? true일 경우, 파일 데이터를 분리하지 않고 참조값으로 저장합니다. (메모리 사용량 감소 및 일부 프로텍트 무시)</param>
        public W3MArchive(string path, bool streamMode = false, bool tryFindAllKey = false)
        {
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (StreamMode = ReadOnly = streamMode)
            {
                Stream = stream;
                MapHeader = new W3MHeader(stream);
                Header = new MPQHeader(this, stream);
                HashTable = new MPQHashTable(this, stream);
                BlockTable = new MPQBlockTable(this, stream, tryFindAllKey);
                Files = new MPQFiles(this, stream, tryFindAllKey);
            }
            else
            {
                using (stream)
                {
                    byte[] data = new byte[stream.Length];
                    stream.Read(data, 0, (int)stream.Length);
                    stream.Close();

                    MapHeader = new W3MHeader(data);
                    Header = new MPQHeader(this, data);
                    HashTable = new MPQHashTable(this, data);
                    BlockTable = new MPQBlockTable(this, data, tryFindAllKey);
                    Files = new MPQFiles(this, data, tryFindAllKey);
                }
            }
            Initialized = true;
        }

        public override byte[] ToArray(bool TryHashTableNormalize = true)
        {
            using (ByteStream ms = new ByteStream())
            {
                Header.UpdateSize();
                byte[] buffer;
                ms.Write(buffer = MapHeader.ToArray());
                ms.SetLength((int)Math.Ceiling(buffer.Length / (double)0x200) * 0x200);
                ms.Seek(0, SeekOrigin.End);
                ms.Write(buffer = base.ToArray(TryHashTableNormalize));
                return ms.ToArray();
            }
        }
    }
}
