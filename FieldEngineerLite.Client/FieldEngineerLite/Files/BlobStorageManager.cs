using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FieldEngineerLite.Files;
using FieldEngineerLite.Files.Metadata;
using FieldEngineerLite.Files.Sync;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FieldEngineerLite
{
    public class BlobStorageProvider
    {
        private readonly IMobileServiceClient mobileServiceClient;

        public BlobStorageProvider(IMobileServiceClient client)
        {
            this.mobileServiceClient = client;
        }

        public async Task UploadFileAsync(MobileServiceFileMetadata metadata, IMobileServiceFileDataSource dataSource)
        {
            StorageToken token = await GetStorageToken(MobileServiceFile.FromMetadata(metadata), StoragePermissions.Write);

            CloudBlockBlob blob = GetBlobReference(token, metadata.FileName);

            using (var stream = await dataSource.GetStream())
            {
                await blob.UploadFromStreamAsync(stream).ContinueWith(t => Console.Write(t.IsFaulted));

                metadata.LastModified = blob.Properties.LastModified;
                metadata.FileStoreUri = blob.Uri.LocalPath;

                stream.Position = 0;
                metadata.ContentMD5 = GetMD5Hash(stream);
            }
        }

        public async Task DownloadFileToStreamAsync<T>(MobileServiceFile file, Stream stream)
        {
            StorageToken token = await GetStorageToken(file, StoragePermissions.Read);

            CloudBlockBlob blob = GetBlobReference(token, file.Name);

            try
            {
                await blob.DownloadToStreamAsync(stream);
            }
            catch (Exception exc)
            {
                Console.WriteLine("Error downloading blob:" + exc.GetType().FullName);

                throw;
            }
        }

        private CloudBlockBlob GetBlobReference(StorageToken token, string fileName)
        {
            CloudBlockBlob blob = null;

            if (token.Scope == StorageTokenScope.File)
            {
                blob = new CloudBlockBlob(new Uri(token.RawToken));
            }
            else if (token.Scope == StorageTokenScope.Record)
            {
                var container = new CloudBlobContainer(new Uri(token.RawToken));

                blob = container.GetBlockBlobReference(fileName);
            }

            return blob;
        }

        private async Task<StorageToken> GetStorageToken(MobileServiceFile file, StoragePermissions permissions)
        {
            
            var tokenRequest = new StorageTokenRequest();
            tokenRequest.Permissions = permissions;
            tokenRequest.TargetFile = file;

            string route = string.Format("/tables/{0}/{1}/StorageToken", file.TableName, file.ParentId);

            return await this.mobileServiceClient.InvokeApiAsync<StorageTokenRequest, StorageToken>(route, tokenRequest);
        }

        private string GetMD5Hash(Stream stream)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(stream);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
