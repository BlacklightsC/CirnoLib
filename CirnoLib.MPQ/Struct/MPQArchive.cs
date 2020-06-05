using System;
using System.IO;
using System.Collections.Generic;

using static CirnoLib.MPQ.Constant;

namespace CirnoLib.MPQ.Struct
{
    public class MPQArchive : IDisposable
    {
        internal int MPQHeaderPos { get; set; }
        public MPQHeader Header { get; internal set; }
        public MPQFiles Files { get; internal set; }
        public MPQHashTable HashTable { get; internal set; }
        public MPQBlockTable BlockTable { get; internal set; }
        public bool ReadOnly { get; protected set; } = false;
        public bool StreamMode { get; protected set; } = false;
        internal bool Initialized { get; set; } = false;
        internal byte[] Data { get; set; }
        internal Stream Stream { get; set; }

        public MPQArchive()
        {
            Header = new MPQHeader(this);
            HashTable = new MPQHashTable(this);
            BlockTable = new MPQBlockTable(this);
            Files = new MPQFiles(this);
        }

        /// <summary>
        /// MPQ 파일을 데이터에 맞게 읽어서 파싱합니다.
        /// </summary>
        /// <param name="data">읽어낼 MPQ 파일</param>
        /// <param name="readOnly">읽기 전용? true일 경우, 파일 데이터를 분리하지 않고 참조값으로 저장합니다. (메모리 사용량 감소 및 일부 프로텍트 무시)</param>
        public MPQArchive(byte[] data, bool readOnly = false, bool tryFindAllKey = false)
        {
            if (ReadOnly = readOnly) Data = data;
            Header = new MPQHeader(this, data);
            HashTable = new MPQHashTable(this, data);
            BlockTable = new MPQBlockTable(this, data, tryFindAllKey);
            Files = new MPQFiles(this, data, tryFindAllKey);
            Initialized = true;
        }

        public MPQArchive(Stream stream, bool tryFindAllKey = false)
        {
            StreamMode = ReadOnly = true;
            Stream = stream;
            Header = new MPQHeader(this, stream);
            HashTable = new MPQHashTable(this, stream);
            BlockTable = new MPQBlockTable(this, stream, tryFindAllKey);
            Files = new MPQFiles(this, stream, tryFindAllKey);
            Initialized = true;
        }

        /// <summary>
        /// 경로상에 있는 MPQ 파일을 데이터에 맞게 읽어서 파싱합니다.
        /// </summary>
        /// <param name="path">읽어낼 MPQ 파일의 경로</param>
        /// <param name="streamMode">읽기 전용? true일 경우, 파일 데이터를 분리하지 않고 참조값으로 저장합니다. (메모리 사용량 감소 및 일부 프로텍트 무시)</param>
        public MPQArchive(string path, bool streamMode = false, bool tryFindAllKey = false)
        {
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (StreamMode = ReadOnly = streamMode)
            {
                Stream = stream;
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

                    Header = new MPQHeader(this, data);
                    HashTable = new MPQHashTable(this, data);
                    BlockTable = new MPQBlockTable(this, data, tryFindAllKey);
                    Files = new MPQFiles(this, data, tryFindAllKey);
                }
            }
            Initialized = true;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (Stream != null) using (Stream) Stream.Close();
                    // TODO: 관리되는 상태(관리되는 개체)를 삭제합니다.
                }

                // TODO: 관리되지 않는 리소스(관리되지 않는 개체)를 해제하고 아래의 종료자를 재정의합니다.
                // TODO: 큰 필드를 null로 설정합니다.

                disposedValue = true;
            }
        }

        public virtual void Dispose() => Dispose(true);
        #endregion

        public MPQFile this[int index] => Files.Find(BlockTable[index]);
        public MPQFile this[byte[] FileName] => Files.Find(FileName);
        public MPQFile this[string FileName] => Files.Find(FileName);

        public void Insert(byte[] FileName, byte[] data) => Files.Insert(FileName, data);
        public void Insert(string FileName, byte[] data) => Files.Insert(FileName, data);

        public MPQFile Find(MPQHash Hash) => Files.Find(Hash);
        public MPQFile Find(MPQBlock Block) => Files.Find(Block);
        public MPQFile Find(byte[] FileName) => Files.Find(FileName);
        public MPQFile Find(string FileName) => Files.Find(FileName);

        public void Remove(MPQHash Hash) => Files.Remove(Hash);
        public void Remove(MPQBlock Block) => Files.Remove(Block);
        public void Remove(byte[] FileName) => Files.Remove(FileName);
        public void Remove(string FileName) => Files.Remove(FileName);

        public void TryReadListfile(params string[] list) => Files.TryReadListfile(list);
        public void SyncHeader()
        {
            Header.HashTableSize = (uint)HashTable.Count;
            Header.BlockTableSize = (uint)BlockTable.Count;
        }
        public void Sort() => HashTable.Sort();
        public void Purge(bool RemoveHash = false, bool ReplaceHash = false)
        {
            // 쓸모없는 블럭 전체 삭제 (블럭 청소 완료)
            BlockTable.RemoveAll(item => (item.Flags & MPQ_FILE_EXISTS) == 0);
            BlockTable.ForEach(item => item.Flags = (item.Flags / 0x100) * 0x100);
            // 해시들 전부 맞춰줌 (FFFFFFFF같은건 남아있음)
            Files.ForEach(File => File.Hash.BlockIndex = (uint)BlockTable.FindIndex(Block => Block == File.Block));

            if (RemoveHash)
            {
                // 블럭 없는 해시 전부 삭제
                List<uint> ExistIndexes = new List<uint>();

                int BlockTableCount = BlockTable.Count;
                for (int i = HashTable.Count - 1; i >= 0; i--)
                {
                    MPQHash Hash = HashTable[i];
                    foreach (var item in ExistIndexes)
                        if (item == Hash.BlockIndex)
                            goto RemoveHash;

                    ExistIndexes.Add(Hash.BlockIndex);

                    if ((Hash.Name1 == 0xFFFFFFFF
                      && Hash.Name2 == 0xFFFFFFFF)
                      || !Hash.BlockIndex.IsSafeIndex(out uint SafeIndex))
                        goto RemoveHash;
                    else
                    {
                        Hash.BlockIndex = SafeIndex;
                        if (Hash.BlockIndex >= BlockTableCount)
                            goto RemoveHash;
                    }

                    Hash.Locale = 0;
                    Hash.Platform = 0;
                    continue;

                    RemoveHash: HashTable.Remove(Hash);
                }
            }
            
            if (ReplaceHash)
            {
                // 해시 위치 재정렬

            }
        }
        public void Shuffle()
        {
            HashTable.Shuffle();
            BlockTable.Shuffle();
            Files.Shuffle();
        }

        public virtual byte[] ToArray(bool TryHashTableNormalize = false)
        {
            using (ByteStream ms = new ByteStream())
            {
                if (TryHashTableNormalize)
                    HashTable.Fill(BlockTable.Count.BitUpper());
                Header.HashTableSize = (uint)HashTable.Count;
                Header.BlockTableSize = (uint)BlockTable.Count;
                ms.WriteEmpty(0x20);
                foreach (var item in Files)
                {
                    if ((item.Block.Flags & MPQ_FILE_FIX_KEY) != 0)
                    {
                        item.Decrypt();
                        item.Block.FilePos = (int)ms.Length;
                        item.Encrypt(true);
                    }
                    else item.Block.FilePos = (int)ms.Length;
                    ms.Write(item.RawFile);
                }
                Header.HashTablePos = (int)ms.Length;
                byte[] Buffer = HashTable.ToArray();
                if (Buffer != null)
                {
                    Buffer.EncryptBlock(MPQ_HASH_KEY);
                    ms.Write(Buffer);
                }
                Header.BlockTablePos = (int)ms.Length;
                Buffer = BlockTable.ToArray();
                if (Buffer != null)
                {
                    Buffer.EncryptBlock(MPQ_BLOCK_KEY);
                    ms.Write(Buffer);
                }
                Header.ArchiveSize = (uint)ms.Length;
                ms.Seek(0, SeekOrigin.Begin);
                ms.Write(Header.ToArray());
                return ms.ToArray();
            }
        }
    }
}
