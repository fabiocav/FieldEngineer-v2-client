﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FieldEngineerLite.Files;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FieldEngineerLite
{
    public class BlobStorageManager
    {
        private readonly IMobileServiceClient mobileServiceClient;

        public BlobStorageManager(IMobileServiceClient client)
        {
            this.mobileServiceClient = client;
        }

        public async Task UploadFile(MobileServiceFileMetadata metadata)
        {
            StorageToken token = await GetStorageToken(metadata.ParentDataItemType, metadata.ParentDataItemId, StoragePermissions.Write);

            var container = new CloudBlobContainer(new Uri(token.RawToken));

            CloudBlockBlob blob = container.GetBlockBlobReference(metadata.FileName);

            using (var stream = File.OpenRead(metadata.LocalPath))
            {
                await blob.UploadFromStreamAsync(stream).ContinueWith(t => Console.Write(t.IsFaulted));
            }
        }

        private async Task<StorageToken> GetStorageToken(string tableName, string dataItemId, StoragePermissions permissions)
        {
            var tokenRequest = new StorageTokenRequest();
            tokenRequest.Permissions = permissions;

            string route = string.Format("/tables/{0}/{1}/StorageToken", tableName, dataItemId);

            return await this.mobileServiceClient.InvokeApiAsync<StorageTokenRequest, StorageToken>(route, tokenRequest);
        }
    }
}
