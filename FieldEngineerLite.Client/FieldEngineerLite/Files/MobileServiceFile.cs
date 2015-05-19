﻿using Microsoft.WindowsAzure.MobileServices;
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
        private IDictionary<string, string> metadata;
        private string localFilePath;
        private string name;
        private string id;
        private string tableName;
        private string parentDataItemId;
        private IMobileServiceClient client;

        //internal MobileServiceFile()
        //{
        //    this.metadata = new Dictionary<string, string>();

        //    // Here for this POC only
        //    this.tableName = "Job";
        //}                                                                                                            

        internal MobileServiceFile(IMobileServiceClient client, string tableName,
            string parentDataItemId, string name)
            : this(client, tableName, parentDataItemId, null, name)
        {
        }

        internal MobileServiceFile(IMobileServiceClient client, string tableName, string parentDataItemId,
            string localFilePath, string name)
        {
            this.tableName = tableName;
            this.localFilePath = localFilePath;
            this.name = name;
            this.id = name;
            this.parentDataItemId = parentDataItemId;
            this.metadata = new Dictionary<string, string>();
            this.client = client;
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
        }

        public string ParentDataItemId
        {
            get { return this.parentDataItemId; }
            set { this.parentDataItemId = value; }
        }

        public long Length { get; set; }

        public string ContentMD5 { get; set; }

        public IDictionary<string, string> Metadata
        {
            get { return this.metadata; }
            private set { this.metadata = value; }
        }

        public bool LocalFileExists
        {
            get { return File.Exists(this.LocalFilePath); }
        }

        public bool IsLocalFileCurrent
        {
            get
            {
                bool result = false;

                if (LocalFileExists)
                {
                    string localContentMD5 = this.GetMD5Hash(LocalFilePath);

                    if (string.Compare(localContentMD5, this.ContentMD5, StringComparison.InvariantCulture) == 0)
                    {
                        result = true;
                    }
                }

                return result;
            }
        }

        public string LocalFilePath
        {
            get
            {
                if (this.localFilePath == null)
                {
                    this.localFilePath = CreateLocaFilePath(this.name);
                }

                return this.localFilePath;
            }
        }

        private static string CreateLocaFilePath(string fileName)
        {
            return Path.Combine(GetFilesDirectory(), fileName);
        }

        private async Task<StorageToken> GetStorageToken(StoragePermissions permissions)
        {
            var tokenRequest = new StorageTokenRequest();
            tokenRequest.Permissions = permissions;

            string route = string.Format("/tables/{0}/{1}/StorageToken", this.tableName, this.parentDataItemId);

            return await this.client.InvokeApiAsync<StorageTokenRequest, StorageToken>(route, tokenRequest);
        }

        internal static string GetFilesDirectory()
        {
            string filesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MobileServicesFiles");

            if (!Directory.Exists(filesPath))
            {
                Directory.CreateDirectory(filesPath);
            }

            return filesPath;
        }

        internal static MobileServiceFile FromFilePath(IMobileServiceClient client, string tableName, string dataItemId, string path)
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

            var file = new MobileServiceFile(client, tableName, dataItemId, targetPath, fileName);

            return file;
        }

        internal static MobileServiceFile FromMobileServiceFileInfo(IMobileServiceClient client, MobileServiceFileInfo fileInfo)
        {
            var file = new MobileServiceFile(client, fileInfo.ParentDataItemType, fileInfo.ParentDataItemId, fileInfo.Name);

            file.ContentMD5 = fileInfo.ContentMD5;
            file.Metadata = fileInfo.Metadata;
            file.Length = fileInfo.Length;

            return file;
        }

        internal static MobileServiceFile FromMetadata(IMobileServiceClient client, MobileServiceFileMetadata metadata)
        {
            var file = new MobileServiceFile(client, metadata.ParentDataItemType, metadata.ParentDataItemId, metadata.FileName);

            file.ContentMD5 = metadata.ContentMD5;
            //file.Metadata = fileInfo.;
            file.Length = metadata.Length;

            return file;
        }

        /// <summary>
        /// Gets an array containing the file bytes.
        /// </summary>
        /// <param name="forceDownload">Bypasses local cache checks and Forces a file download.</param>
        /// <returns>
        /// A task that completes when the delete operation has finished.
        /// </returns>
        public Task<byte[]> GetBytes(bool forceDownload)
        {
            
            if (this.LocalFileExists)
            {
                return Task.FromResult(File.ReadAllBytes(this.LocalFilePath));
            }

            return Task.FromResult<byte[]>(null);
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
