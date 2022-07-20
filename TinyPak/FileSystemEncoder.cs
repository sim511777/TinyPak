﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TinyPak {
    // 파일 인코딩 : 파일 정보 제외하고 파일 데이터 그대로 쓴다.
    // 폴더 인코딩 :
    // 폴더 정보 제외하고 폴더 내용을 쓴다.
    // - 폴더내 항목 개수
    // - 항목 정보 리스트
    //  - 파일 정보들
    //  - 이름 정보(폴더 인코딩으로 부터 옵셋, 길이)
    //  - 데이터 정보(폴더 인코딩으로 부터 옵셋, 옵셋, 길이)
    // - 이름 버퍼, 데이터 버퍼 리스트
    public class FileSystemEncoder {

        // e디렉토리 인코딩
        // >= 0 : 스트림에 기록된 바이트 수
        // -1 : 실패
        public static long EncodeDirectory(Stream sr, FileSystemInfo[] sinfos) {
            var oldPos = sr.Position;
            
            // 항목 개수 기록
            EncodeLong(sr, sinfos.Length);
            
            // 파일 정보 기록할 공간 확보
            var tablePos = sr.Position;
            for (int i = 0; i < sinfos.Length; i++) {
                sr.Write(dummyBuffer, 0, (int)FileSystemInfoTableSize);
            }

            // 이름, 파일시스템 데이터 기록
            var tableDic = new Dictionary<FileSystemInfo, FileSystemInfoTable>();
            foreach (var sinfo in sinfos) {
                // 파일시스템 이름, 파일시스템 데이터 기록
                var nameOffset = sr.Position - oldPos;
                var nameSize = EncodeString(sr, sinfo.Name);
                var dataOffset = sr.Position - oldPos;
                long dataSize;
                var bFile = !sinfo.Attributes.HasFlag(FileAttributes.Directory);
                if (bFile) {
                    var finfo = sinfo as FileInfo;
                    dataSize = EncodeFile(sr, finfo);
                } else {
                    var dinfo = sinfo as DirectoryInfo;
                    dataSize = EncodeDirectory(sr, dinfo.GetFileSystemInfos());
                }
                tableDic[sinfo] = new FileSystemInfoTable(sinfo.Attributes, sinfo.CreationTimeUtc, sinfo.LastAccessTimeUtc, sinfo.LastAccessTimeUtc, nameOffset, nameSize, dataOffset, dataSize);
            }
            
            // 파일 정보 기록
            sr.Position = tablePos;     // 스트림 테이블 영역으로 이동
            foreach (var sinfo in sinfos) {
                var size = EncodeFileSystemInfoTable(sr, tableDic[sinfo]);
            }
            sr.Seek(0, SeekOrigin.End); // 스트림 끝으로 이동

            return sr.Position - oldPos;
        }

        public const long FileSystemInfoTableSize = 60;
        public static readonly byte[] dummyBuffer = new byte[FileSystemInfoTableSize];
        public static long EncodeFileSystemInfoTable(Stream sr, FileSystemInfoTable table) {
            EncodeInt(sr, (int)table.attributes);               // ( 0, 4) 파일 속성
            EncodeLong(sr, table.creationTimeUtc.Ticks);        // ( 4, 8) 생성 시간
            EncodeLong(sr, table.lastAccessTimeUtc.Ticks);      // (12, 8) 접속 시간
            EncodeLong(sr, table.lastWriteTimeUtc.Ticks);       // (20, 8) 기록 시간
            EncodeLong(sr, table.nameOffset);                // (28, 8) 이름 데이터 옵셋
            EncodeLong(sr, table.nameSize);                // (36, 8) 이름 데이터 사이즈
            EncodeLong(sr, table.dataOffset);                // (44, 8) 파일 데이터 옵셋
            EncodeLong(sr, table.dataSize);                // (52, 8) 파일 데이터 사이즈
            return FileSystemInfoTableSize;
        }

        // 파일 데이터 인코딩
        // >= 0 : 스트림에 기록된 바이트 수
        // -1 : 실패
        // todo: 파일 압축시 이 코드를 수정한다. Chunk 분할 필요
        public static long EncodeFile(Stream sr, FileInfo finfo) {
            var buf = File.ReadAllBytes(finfo.FullName);
            sr.Write(buf, 0, buf.Length);
            return buf.Length;
        }

        // 문자열 데이터 인코딩
        // >= 0 : 스트림에 기록된 바이트 수
        // -1 : 실패
        // todo: 문자열 압축시 이 코드를 수정한다.
        public static long EncodeString(Stream sr, String str) {
            var buf = Encoding.UTF8.GetBytes(str);
            sr.Write(buf, 0, buf.Length);
            return buf.Length;
        }

        public static long EncodeInt(Stream sr, int v) {
            var buf = BitConverter.GetBytes(v);
            sr.Write(buf, 0, buf.Length);
            return Marshal.SizeOf(v.GetType());
        }

        public static long EncodeLong(Stream sr, long v) {
            var buf = BitConverter.GetBytes(v);
            sr.Write(buf, 0, buf.Length);
            return Marshal.SizeOf(v.GetType());
        }

        // ==== decoding
        public static void DecodeDirectory(Stream sr, DirectoryInfo dinfo) {
            var oldPos = sr.Position;
            var childNum = DecodeLong(sr);
            List<FileSystemInfoTable> tables = new List<FileSystemInfoTable>();
            for (int i = 0; i < childNum; i++) {
                var table = DecodeFileSystemInfoTable(sr);
                tables.Add(table);
            }
            
            foreach (var table in tables) {
                sr.Position = oldPos + table.nameOffset;
                var name = DecodeString(sr, table.nameSize);
                bool bFile = !table.attributes.HasFlag(FileAttributes.Directory);
                if (bFile) {
                    var filePath = Util.MakeNewFilePath(dinfo.FullName + "\\" + name);
                    sr.Position = oldPos + table.dataOffset;
                    var finfo = new FileInfo(filePath);
                    DecodeFile(sr, table.dataSize, finfo);
                    finfo.CreationTimeUtc = table.creationTimeUtc;
                    finfo.LastAccessTimeUtc = table.lastAccessTimeUtc;
                    finfo.LastWriteTimeUtc = table.lastWriteTimeUtc;
                } else {
                    var dirPath = Util.MakeNewDirectoryPath(dinfo.FullName + "\\" + name);
                    Directory.CreateDirectory(dirPath);
                    var childDirInfo = new DirectoryInfo(dirPath);
                    childDirInfo.CreationTimeUtc = table.creationTimeUtc;
                    childDirInfo.LastAccessTimeUtc = table.lastAccessTimeUtc;
                    childDirInfo.LastWriteTimeUtc = table.lastWriteTimeUtc;
                    sr.Position = oldPos + table.dataOffset;
                    DecodeDirectory(sr, childDirInfo);
                }
            }
        }

        public static FileSystemInfoTable DecodeFileSystemInfoTable(Stream sr) {
            FileSystemInfoTable table = new FileSystemInfoTable();
            table.attributes = (FileAttributes)DecodeInt(sr);
            table.creationTimeUtc = new DateTime(DecodeLong(sr));
            table.lastAccessTimeUtc = new DateTime(DecodeLong(sr));
            table.lastWriteTimeUtc = new DateTime(DecodeLong(sr));
            table.nameOffset = DecodeLong(sr);
            table.nameSize = DecodeLong(sr);
            table.dataOffset = DecodeLong(sr);
            table.dataSize = DecodeLong(sr);
            return table;
        }


        public static void DecodeFile(Stream sr, long size, FileInfo finfo) {
            var buf = new byte[size];
            sr.Read(buf, 0, buf.Length);
            File.WriteAllBytes(finfo.FullName, buf);
        }

        public static string DecodeString(Stream sr, long size) {
            var buf = new byte[size];
            sr.Read(buf, 0, buf.Length);
            return Encoding.UTF8.GetString(buf);
        }

        public static int DecodeInt(Stream sr) {
            var buf = new byte[4];
            sr.Read(buf, 0, buf.Length);
            return BitConverter.ToInt32(buf, 0);
        }

        public static long DecodeLong(Stream sr) {
            var buf = new byte[8];
            sr.Read(buf, 0, buf.Length);
            return BitConverter.ToInt64(buf, 0);
        }
    }

    public class FileSystemInfoTable {
        public FileAttributes attributes;
        public DateTime creationTimeUtc;
        public DateTime lastAccessTimeUtc;
        public DateTime lastWriteTimeUtc;
        public long nameOffset;
        public long nameSize;
        public long dataOffset;
        public long dataSize;

        public FileSystemInfoTable() {

        }

        public FileSystemInfoTable(FileAttributes attributes, DateTime creationTimeUtc, DateTime lastAccessTimeUtc, DateTime lastWriteTimeUtc, long nameOffset, long nameSize, long dataOffset, long dataSize) {
            this.attributes = attributes;
            this.creationTimeUtc = creationTimeUtc;
            this.lastAccessTimeUtc = lastAccessTimeUtc;
            this.lastWriteTimeUtc = lastWriteTimeUtc;
            this.nameOffset = nameOffset;
            this.nameSize = nameSize;
            this.dataOffset = dataOffset;
            this.dataSize = dataSize;
        }
    }
}