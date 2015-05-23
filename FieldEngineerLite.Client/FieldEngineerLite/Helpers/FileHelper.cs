using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FieldEngineerLite.Helpers
{
    public class FileHelper
    {
        public static string CopyJobFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);

            string targetPath = GetLocalFilePath(fileName);

            File.Copy(filePath, targetPath);

            return targetPath;
        }

        public static string GetLocalFilePath(string fileName)
        {
            return Path.Combine(GetFilesDirectory(), fileName);
        }

        private static string GetFilesDirectory()
        {
            string filesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MobileServicesFiles");

            if (!Directory.Exists(filesPath))
            {
                Directory.CreateDirectory(filesPath);
            }

            return filesPath;
        }
    }
}
