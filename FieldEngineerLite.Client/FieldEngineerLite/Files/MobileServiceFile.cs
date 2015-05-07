using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FieldEngineerLite.Files
{
    public class MobileServiceFile
    {
        private readonly Dictionary<string, string> metadata;
        private string localFilePath;
        private string name;
        private string id;
        private string tableName;
        private string parentDataItemId;

        private MobileServiceFile()
        {
            this.metadata = new Dictionary<string, string>();

            // Here for this POC only
            this.tableName = "Job";
        }

        private MobileServiceFile(string tableName, string parentDataItemId, string localFilePath, string name)
        {
            this.tableName = tableName;
            this.localFilePath = localFilePath;
            this.name = name;
            this.id = name;
            this.parentDataItemId = parentDataItemId;
            this.metadata = new Dictionary<string, string>();
        }

        public string Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public string ParentDataItemId
        {
            get { return parentDataItemId; }
            set { parentDataItemId = value; }
        }

        public IDictionary<string, string> Metadata
        {
            get { return this.metadata; }
        }

        internal static MobileServiceFile FromFile(string tableName, string dataItemId, string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            if (string.IsNullOrWhiteSpace(path) || path.IndexOfAny(Path.GetInvalidPathChars()) > 0)
            {
                throw new ArgumentException("Invalid file name.", "fileName");
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException();
            }

            string fileName = Path.GetFileName(path);

            string targetPath = CreateLocaFilePath(fileName);

            File.Copy(path, targetPath);

            return new MobileServiceFile(tableName, dataItemId, targetPath, fileName);
        }

        private static string CreateLocaFilePath(string fileName)
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

        public bool LocalFileExists
        {
            get
            {
                return File.Exists(localFilePath);
            }
        }

        public string LocalFilePath
        {
            get
            {
                if (localFilePath == null)
                {
                    localFilePath = CreateLocaFilePath(this.name);
                }

                return localFilePath;
            }
        }
    }

}
