using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FieldEngineerLite.Files;
using FieldEngineerLite.Files.Metadata;
using FieldEngineerLite.Files.Sync;
using Microsoft.WindowsAzure.MobileServices;
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
            Debug.WriteLine("FILE MANAGEMENT: Uploading file.");

            StorageToken token = await GetStorageToken(metadata.ParentDataItemType, metadata.ParentDataItemId, StoragePermissions.Write);

            var container = new CloudBlobContainer(new Uri(token.RawToken));

            CloudBlockBlob blob = container.GetBlockBlobReference(metadata.FileName);

            using (var stream = await dataSource.GetStream())
            {
                await blob.UploadFromStreamAsync(stream).ContinueWith(t => Console.Write(t.IsFaulted));
            }
        }

        public async Task DownloadFileToStreamAsync<T>(MobileServiceFile file, Stream stream)
        {
            StorageToken token = await GetStorageToken(file.TableName, file.ParentId, StoragePermissions.Read);

            var container = new CloudBlobContainer(new Uri(token.RawToken));

            CloudBlob blob = container.GetBlobReference(file.Name);

            await blob.DownloadToStreamAsync(stream);
        }

        private async Task<StorageToken> GetStorageToken(string tableName, string dataItemId, StoragePermissions permissions)
        {
            Debug.WriteLine("FILE MANAGEMENT: Retrieving SAS.");

            var tokenRequest = new StorageTokenRequest();
            tokenRequest.Permissions = permissions;

            string route = string.Format("/tables/{0}/{1}/StorageToken", tableName, dataItemId);

            return await this.mobileServiceClient.InvokeApiAsync<StorageTokenRequest, StorageToken>(route, tokenRequest);
        }
    }
}
