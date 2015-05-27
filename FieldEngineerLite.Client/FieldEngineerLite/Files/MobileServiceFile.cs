using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using FieldEngineerLite.Files.Metadata;

namespace FieldEngineerLite.Files
{
    public sealed class MobileServiceFile
    {
        private string id;
        private string name;
        private string parentId;
        private string tableName;
        private IDictionary<string, string> metadata;

        public MobileServiceFile(string name, string tableName, string parentId)
            : this(name, name, tableName, parentId)
        { }

        public MobileServiceFile(string id, string name, string tableName, string parentId)
        {
            this.id = id;
            this.name = name;
            this.tableName = tableName;
            this.parentId = parentId;
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

        public string TableName
        {
            get { return this.tableName; }
            set { this.tableName = value; }
        }

        public string ParentId
        {
            get { return parentId; }
            set { parentId = value; }
        }

        public long Length { get; set; }

        public string ContentMD5 { get; set; }

        public IDictionary<string, string> Metadata
        {
            get { return this.metadata; }
            private set { this.metadata = value; }
        }

        internal static MobileServiceFile FromMobileServiceFileInfo(IMobileServiceClient client, MobileServiceFileInfo fileInfo)
        {
            var file = new MobileServiceFile(fileInfo.Name, fileInfo.ParentDataItemType, fileInfo.ParentDataItemId);

            file.ContentMD5 = fileInfo.ContentMD5;
            file.Metadata = fileInfo.Metadata;
            file.Length = fileInfo.Length;

            return file;
        }

        internal static MobileServiceFile FromMetadata(IMobileServiceClient client, MobileServiceFileMetadata metadata)
        {
            var file = new MobileServiceFile(metadata.FileId, metadata.FileName, metadata.ParentDataItemId);

            file.ContentMD5 = metadata.ContentMD5;
            //file.Metadata = fileInfo.;
            file.Length = metadata.Length;

            return file;
        }

        private string GetMD5Hash(string filePath)
        {
            using (MD5 md5 = MD5.Create())
            {
                using (Stream fileStream = File.OpenRead(filePath))
                {
                    byte[] hash = md5.ComputeHash(fileStream);

                    return Convert.ToBase64String(hash);
                }
            }
        }
    }
}
