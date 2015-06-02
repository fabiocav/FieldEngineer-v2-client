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
using System.Threading;
using FieldEngineerLite.Files.Metadata;
using FieldEngineerLite.Files.Sync;

namespace FieldEngineerLite.Files
{
    public static class MobileServiceSyncTableExtensions
    {
        private static IFileSyncHandler fileSyncHandler;
        private static BlobStorageProvider storageProvider;
        internal static IFileMetadataStore metadataStore;

        public async static Task<IEnumerable<MobileServiceFile>> GetFilesAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem)
        {
            IFileMetadataStore metadataStore = GetMetadataStore(table.MobileServiceClient.SyncContext.Store as MobileServiceLocalStore);

            var fileMetadata = await metadataStore.GetMetadataAsync(table.TableName, GetDataItemId(dataItem));

            return fileMetadata.Where(m => !m.PendingDeletion).Select(m => MobileServiceFile.FromMetadata(m));
        }

        internal static void InitializeFileSync(IFileSyncHandler handler)
        {
            fileSyncHandler = handler;
        }

        private static string GetDataItemId(object dataItem)
        {
            var job = dataItem as Job;
            if (job != null)
            {
                return job.Id;
            }

            return null;
        }

        private static IFileMetadataStore GetMetadataStore(MobileServiceLocalStore localStore)
        {
            if (metadataStore == null)
            {
                metadataStore = new DelegatedFileMetadataStore(localStore);
            }

            return metadataStore;
        }

        private static IFileSyncContext GetFileSyncContext(IMobileServiceClient client)
        {
            IFileMetadataStore metadataStore = GetMetadataStore(client.SyncContext.Store as MobileServiceLocalStore);

            return MobileServiceFileSyncContext.GetContext(client, metadataStore, fileSyncHandler);
        }

        private static BlobStorageProvider GetStorageProvider(IMobileServiceClient client)
        {
            if (storageProvider == null)
            {
                storageProvider = new BlobStorageProvider(client);
            }

            return storageProvider;
        }

        public static MobileServiceFile CreateFile<T>(this IMobileServiceSyncTable<T> table, T dataItem, string fileName)
        {
            return new MobileServiceFile(fileName, table.TableName, GetDataItemId(dataItem));
        }

        public async static Task PurgeFilesAsync<T>(this IMobileServiceSyncTable<T> table)
        {
            IFileMetadataStore metadataStore = GetMetadataStore(table.MobileServiceClient.SyncContext.Store as MobileServiceLocalStore);

            await metadataStore.PurgeAsync(table.TableName);
        }

        public async static Task PurgeFilesAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem)
        {
            IFileMetadataStore metadataStore = GetMetadataStore(table.MobileServiceClient.SyncContext.Store as MobileServiceLocalStore);

            await metadataStore.PurgeAsync(table.TableName, GetDataItemId(dataItem));

            // Application logic: delete job's files:
            // TODO: Add a hook to the sync handler
        }

        public async static Task PushFileChangesAsync<T>(this IMobileServiceSyncTable<T> table)
        {
            IFileSyncContext context = GetFileSyncContext(table.MobileServiceClient);
            
            await context.PushChangesAsync(CancellationToken.None);
        }

        public async static Task PullFilesAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem)
        {
            IFileSyncContext context = GetFileSyncContext(table.MobileServiceClient);
            
            await context.PullFilesAsync(table.TableName, GetDataItemId(dataItem));
        }

        public async static Task AddFileAsync<T>(this IMobileServiceSyncTable<T> table, MobileServiceFile file)
        {
            IFileSyncContext context = GetFileSyncContext(table.MobileServiceClient);
            
            await context.AddFileAsync(file);
        }

        public async static Task UploadFileAsync<T>(this IMobileServiceSyncTable<T> table, MobileServiceFile file, string filePath)
        {
            StorageToken token = await GetStorageToken(table, file, StoragePermissions.Write);

            var container = new CloudBlobContainer(new Uri(token.RawToken));

            CloudBlockBlob blob = container.GetBlockBlobReference(file.Name);

            using (var stream = File.OpenRead(filePath))
            {
                await blob.UploadFromStreamAsync(stream).ContinueWith(t => Console.Write(t.IsFaulted));
            }
        }

        public async static Task DownloadFileAsync<T>(this IMobileServiceSyncTable<T> table, MobileServiceFile file, string targetPath)
        {
            using (Stream stream = File.Create(targetPath))
            {
                BlobStorageProvider provider = GetStorageProvider(table.MobileServiceClient);

                await provider.DownloadFileToStreamAsync<T>(file, stream);
            }
        }

        public async static Task DeleteFileAsync<T>(this IMobileServiceSyncTable<T> table, MobileServiceFile file)
        {
            IFileSyncContext context = GetFileSyncContext(table.MobileServiceClient);
            
            await context.DeleteFileAsync(file);

            MobileServiceFileMetadata metadata = await metadataStore.GetFileMetadataAsync(file.Id);
            metadata.PendingDeletion = true;

            await metadataStore.CreateOrUpdateAsync(metadata);
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

        private async static Task<StorageToken> GetStorageToken<T>(this IMobileServiceSyncTable<T> table, MobileServiceFile file, StoragePermissions permissions)
        {
            var tokenRequest = new StorageTokenRequest();
            tokenRequest.Permissions = permissions;

            return await table.MobileServiceClient.InvokeApiAsync<StorageTokenRequest, StorageToken>("StorageToken", tokenRequest);
        }
    }
}
