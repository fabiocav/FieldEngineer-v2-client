using FieldEngineerLite.Models;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FieldEngineerLite.Files
{
    public static class MobileServiceTableExtensions
    {
        public async static Task<IEnumerable<MobileServiceFile>> GetFilesAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem)
        {
            string route = string.Format("/tables/{0}/{1}/MobileServiceFiles", table.TableName, GetDataItemId(dataItem));

            return await table.MobileServiceClient.InvokeApiAsync<IEnumerable<MobileServiceFile>>(route, HttpMethod.Get, null);
        }

        public static MobileServiceFile CreateFileFromPath<T>(this IMobileServiceSyncTable<T> table, T dataItem, string filePath)
        {
            return MobileServiceFile.FromFile(table.TableName, GetDataItemId(dataItem), filePath);
        }

        public async static Task UploadFileAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem, MobileServiceFile file)
        {
            StorageToken token = await GetStorageToken(table, GetDataItemId(dataItem), StoragePermissions.Write);

            var container = new CloudBlobContainer(new Uri(token.RawToken));

            CloudBlockBlob blob = container.GetBlockBlobReference(file.Name);

            string filePath = Path.Combine(file.LocalFilePath);
            using (var stream = File.OpenRead(filePath))
            {
                await blob.UploadFromStreamAsync(stream).ContinueWith(t => Console.Write(t.IsFaulted));
            }
        }

        public async static Task DownloadFileAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem, MobileServiceFile file)
        {
            using (var stream = File.Create(file.LocalFilePath))
            {
                await DownloadFileToStreamAsync(table, dataItem, file, stream);
            }
        }

        public async static Task DownloadFileToStreamAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem, MobileServiceFile file, Stream stream)
        {

            StorageToken token = await GetStorageToken(table, GetDataItemId(dataItem), StoragePermissions.Read);

            var container = new CloudBlobContainer(new Uri(token.RawToken));

            CloudBlob blob = container.GetBlobReference(file.Name);

            await blob.DownloadToStreamAsync(stream);
        }

        public async static Task DeleteFileAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem, MobileServiceFile file)
        {
            string route = string.Format("/tables/{0}/{1}/MobileServiceFiles/{2}", table.TableName, GetDataItemId(dataItem), file.Name);

            await table.MobileServiceClient.InvokeApiAsync(route, HttpMethod.Delete, null);
        }

        private static string GetFilesDirectoryAsync()
        {
            string filesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MobileServicesFiles");

            if (!Directory.Exists(filesPath))
            {
                Directory.CreateDirectory(filesPath);
            }

            return filesPath;
        }

        private static string GetDataItemId(object dataItem)
        {
            // This would be replaced with the logic used to resolve object IDs
            return ((Job)dataItem).Id;
        }

        private async static Task<StorageToken> GetStorageToken(IMobileServiceSyncTable table, string dataItemId, StoragePermissions permissions)
        {
            var tokenRequest = new StorageTokenRequest();
            tokenRequest.Permissions = permissions;

            string route = string.Format("/tables/{0}/{1}/StorageToken", table.TableName, dataItemId);

            return await table.MobileServiceClient.InvokeApiAsync<StorageTokenRequest, StorageToken>(route, tokenRequest);
        }
    }
}
