using System;
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
            Dictionary<FileSystemInfo, Tuple<long, long, long, long>> bufOffsetInfoDic = new Dictionary<FileSystemInfo, Tuple<long, long, long, long>>();
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
                bufOffsetInfoDic[sinfo] = Tuple.Create(nameOffset, nameSize, dataOffset, dataSize);
            }
            
            // 파일 정보 기록
            sr.Position = tablePos;     // 스트림 테이블 영역으로 이동
            foreach (var sinfo in sinfos) {
                var size = EncodeFileSystemInfoTable(sr, sinfo, bufOffsetInfoDic[sinfo]);
            }
            sr.Seek(0, SeekOrigin.End); // 스트림 끝으로 이동

            return sr.Position - oldPos;
        }

        public const long FileSystemInfoTableSize = 60;
        public static readonly byte[] dummyBuffer = new byte[FileSystemInfoTableSize];
        public static long EncodeFileSystemInfoTable(Stream sr, FileSystemInfo sinfo, Tuple<long, long, long, long> bufOffsetInfo) {
            EncodeInt(sr, (int)sinfo.Attributes);               // ( 0, 4) 파일 속성
            EncodeLong(sr, sinfo.CreationTimeUtc.Ticks);        // ( 4, 8) 생성 시간
            EncodeLong(sr, sinfo.LastAccessTimeUtc.Ticks);      // (12, 8) 접속 시간
            EncodeLong(sr, sinfo.LastWriteTimeUtc.Ticks);       // (20, 8) 기록 시간
            EncodeLong(sr, bufOffsetInfo.Item1);                // (28, 8) 이름 데이터 옵셋
            EncodeLong(sr, bufOffsetInfo.Item2);                // (36, 8) 이름 데이터 사이즈
            EncodeLong(sr, bufOffsetInfo.Item3);                // (44, 8) 파일 데이터 옵셋
            EncodeLong(sr, bufOffsetInfo.Item4);                // (52, 8) 파일 데이터 사이즈
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
    }
}
