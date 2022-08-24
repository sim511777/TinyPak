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

        public static string MakeNewFilePath(string filePath, string copyName = "New") {
            var dirName = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var ext = Path.GetExtension(filePath);
            string newPath;
            int i = 0;
            do {
                if (i == 0)
                    newPath = Path.Combine(dirName, fileName + ext);
                else if (i == 1)
                    newPath = Path.Combine(dirName, fileName + $" - {copyName}" + ext);
                else
                    newPath = Path.Combine(dirName, fileName + $" - {copyName} ({i})" + ext);
                i++;
            } while (File.Exists(newPath));
            return newPath;
        }

        public static string MakeNewDirectoryPath(string dirPath, string copyName = "New") {
            var dirName = Path.GetDirectoryName(dirPath);
            var fileName = Path.GetFileName(dirPath);
            string newPath;
            int i = 0;
            do {
                if (i == 0)
                    newPath = Path.Combine(dirName, fileName);
                else if (i == 1)
                    newPath = Path.Combine(dirName, fileName + $" - {copyName}");
                else
                    newPath = Path.Combine(dirName, fileName + $" - {copyName} ({i})");
                i++;
            } while (Directory.Exists(newPath));
            return newPath;
        }
    }
}
