using System;
using System.IO;

namespace Search4Match
{
  
        public class FileHelper
        {
            public static string prefix = AppDomain.CurrentDomain.BaseDirectory + "img";
            public static void CleanFolder()
            {
                foreach (FileInfo file in new DirectoryInfo(prefix).GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in new DirectoryInfo(prefix).GetDirectories())
                {
                    dir.Delete(true);
                }
            }
            public static void RemoveFile()
            {
                foreach (FileInfo file in new DirectoryInfo(prefix).GetFiles())
                {
                    file.Delete();
                }
            }
            public static void Move(string src)
            {
            foreach (DirectoryInfo dir in new DirectoryInfo(src).GetDirectories())
            {
                Directory.Move(dir.FullName, prefix + dir.Name);
            }
            }
    }
}
