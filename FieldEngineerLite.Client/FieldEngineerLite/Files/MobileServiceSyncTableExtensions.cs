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
using System.Linq;
using FieldEngineerLite.Files.Operations;

namespace FieldEngineerLite.Files
{
    public static class MobileServiceSyncTableExtensions
    {
        // TMP
        internal static IFileMetadataStore metadataStore = new InMemoryFileMetadataStore();
        private static IFileSyncContext syncContext;

        public async static Task<IEnumerable<MobileServiceFile>> GetFilesAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem)
        {
            string route = string.Format("/tables/{0}/{1}/MobileServiceFiles", table.TableName, GetDataItemId(dataItem));

            if (!table.MobileServiceClient.SerializerSettings.Converters.Any(p => p is MobileServiceFileJsonConverter))
            {
                table.MobileServiceClient.SerializerSettings.Converters.Add(new MobileServiceFileJsonConverter(table.MobileServiceClient));
            }

            if (syncContext == null)
            {
                syncContext = new MobileServiceFileSyncContext(table.MobileServiceClient, metadataStore);
            }

            IEnumerable<MobileServiceFile> files = await table.MobileServiceClient.InvokeApiAsync<IEnumerable<MobileServiceFile>>(route, HttpMethod.Get, null);

            foreach (var file in files)
            {
                var metadata = new MobileServiceFileMetadata
                {
                    FileId = file.Id,
                    FileName = file.Name,
                    ContentMD5 = file.ContentMD5,
                    LastSynchronized = DateTime.UtcNow,
                    Length = file.Length,
                    LocalPath = file.LocalFilePath,
                    ParentDataItemType = table.TableName,
                    ParentDataItemId = file.ParentDataItemId,
                    Location = file.LocalFileExists ? FileLocation.LocalAndServer : FileLocation.Server,
                };

                await metadataStore.CreateOrUpdateAsync(metadata);
            }

            return files;
        }

        public async static Task<MobileServiceFile> CreateFileFromPath<T>(this IMobileServiceSyncTable<T> table, T dataItem, string filePath)
        {
            return await MobileServiceFile.FromFile(table.MobileServiceClient, table.TableName, GetDataItemId(dataItem), filePath);
        }

        public async static Task AddFileAsync<T>(this IMobileServiceSyncTable<T> table, MobileServiceFile file)
        {


            var metadata = new MobileServiceFileMetadata
            {
                FileId = file.Id,
                FileName = file.Name,
                Length = file.Length,
                Location = FileLocation.Local,
                ContentMD5 = file.ContentMD5,
                LocalPath = file.LocalFilePath
            };

            await metadataStore.CreateOrUpdateAsync(metadata);

            
            // add a "create" operation to the queue
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
            // Add a delete operation to the queue 
            //  - Check currrent file status and, depending on it, remove previous operations on the file

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

    public interface IFileMetadataStore
    {
        Task CreateOrUpdateAsync(MobileServiceFileMetadata metadata);

        MobileServiceFileMetadata GetFileMetadataAsync(string fileId);

        Task DeleteAsync(MobileServiceFileMetadata metadata);
    }


    public class InMemoryFileMetadataStore : IFileMetadataStore
    {
        private List<MobileServiceFileMetadata> metadataCollection = new List<MobileServiceFileMetadata>();

        public Task CreateOrUpdateAsync(MobileServiceFileMetadata metadata)
        {

            if (this.metadataCollection.Any(m => string.Compare(m.FileId, metadata.FileId) == 0))
            {
                throw new InvalidOperationException(string.Format("Metadata for file id {0} already exists.", metadata.FileId));
            }

            this.metadataCollection.Add(metadata);

            return Task.FromResult(0);
        }

        
        public MobileServiceFileMetadata GetFileMetadataAsync(string fileId)
        {
            return this.metadataCollection.FirstOrDefault(m => string.Compare(m.FileId, fileId) == 0);
        }


        public Task DeleteAsync(MobileServiceFileMetadata metadata)
        {
            this.metadataCollection.Remove(metadata);

            return Task.FromResult(0);
        }
    }
}
