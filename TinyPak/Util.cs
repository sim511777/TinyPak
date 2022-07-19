using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyPak {
    public class Util {
        public static double GetTime() {
            return (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency;
        }

        public static double GetTimeMs() {
            return GetTime() * 1000.0;
        }

        public static string MakeNewFilePath(string filePath) {
            var dirName = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var ext = Path.GetExtension(filePath);
            string newPath;
            int i = 0;
            do {
                if (i == 0)
                    newPath = Path.Combine(dirName, fileName + ext);
                else if (i == 1)
                    newPath = Path.Combine(dirName, fileName + " - 복사본" + ext);
                else
                    newPath = Path.Combine(dirName, fileName + $" - 복사본 ({i})" + ext);
                i++;
            } while (File.Exists(newPath));
            return newPath;
        }

        public static string MakeNewDirectoryPath(string dirPath) {
            var dirName = Path.GetDirectoryName(dirPath);
            var fileName = Path.GetFileNameWithoutExtension(dirPath);
            string newPath;
            int i = 0;
            do {
                if (i == 0)
                    newPath = Path.Combine(dirName, fileName);
                else if (i == 1)
                    newPath = Path.Combine(dirName, fileName + " - 복사본");
                else
                    newPath = Path.Combine(dirName, fileName + $" - 복사본 ({i})");
                i++;
            } while (Directory.Exists(newPath));
            return newPath;
        }
    }
}
