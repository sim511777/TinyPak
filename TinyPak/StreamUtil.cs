using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TinyPak {
    internal static class StreamUtil {
        // 파일 데이터 인코딩
        // >= 0 : 스트림에 기록된 바이트 수
        // -1 : 실패
        // todo: 파일 압축시 이 코드를 수정한다. Chunk 분할 필요
        internal static long EncodeFile(this Stream sr, FileInfo finfo) {
            var buf = File.ReadAllBytes(finfo.FullName);
            sr.Write(buf, 0, buf.Length);
            return buf.Length;
        }

        // 문자열 데이터 인코딩
        // >= 0 : 스트림에 기록된 바이트 수
        // -1 : 실패
        // todo: 문자열 압축시 이 코드를 수정한다.
        internal static long EncodeString(this Stream sr, String str) {
            var buf = Encoding.UTF8.GetBytes(str);
            sr.Write(buf, 0, buf.Length);
            return buf.Length;
        }

        internal static void EncodeInt(this Stream sr, int v) {
            var buf = BitConverter.GetBytes(v);
            sr.Write(buf, 0, buf.Length);
        }

        internal static void EncodeLong(this Stream sr, long v) {
            var buf = BitConverter.GetBytes(v);
            sr.Write(buf, 0, buf.Length);
        }

        internal static void DecodeFile(this Stream sr, long size, FileInfo finfo) {
            var buf = new byte[size];
            sr.Read(buf, 0, buf.Length);
            File.WriteAllBytes(finfo.FullName, buf);
        }

        internal static string DecodeString(this Stream sr, long size) {
            var buf = new byte[size];
            sr.Read(buf, 0, buf.Length);
            return Encoding.UTF8.GetString(buf);
        }

        internal static int DecodeInt(this Stream sr) {
            var buf = new byte[4];
            sr.Read(buf, 0, buf.Length);
            return BitConverter.ToInt32(buf, 0);
        }

        internal static long DecodeLong(this Stream sr) {
            var buf = new byte[8];
            sr.Read(buf, 0, buf.Length);
            return BitConverter.ToInt64(buf, 0);
        }
    }
}
