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
        internal static IFileMetadataStore metadataStore;

        public async static Task<IEnumerable<MobileServiceFile>> GetFilesAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem)
        {
            IFileMetadataStore metadataStore = GetMetadataStore(table.MobileServiceClient.SyncContext.Store as MobileServiceLocalStore);

            var fileMetadata = await metadataStore.GetMetadataAsync(table.TableName, GetDataItemId(dataItem));

            return fileMetadata.Where(m => !m.PendingDeletion).Select(m => MobileServiceFile.FromMetadata(table.MobileServiceClient, m));
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
                //metadataStore = new DelegatedFileMetadataStore(localStore);
                metadataStore = new DelegatedFileMetadataStore(localStore);
            }

            return metadataStore;
        }

        public static MobileServiceFile CreateFile<T>(this IMobileServiceSyncTable<T> table, T dataItem, string fileName)
        {
            return new MobileServiceFile(fileName, table.TableName, GetDataItemId(dataItem));
        }

        //public async static Task<IEnumerable<MobileServiceFile>> GetFilesAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem)
        //{
        //    string route = string.Format("/tables/{0}/{1}/MobileServiceFiles", table.TableName, GetDataItemId(dataItem));

        //    if (!table.MobileServiceClient.SerializerSettings.Converters.Any(p => p is MobileServiceFileJsonConverter))
        //    {
        //        table.MobileServiceClient.SerializerSettings.Converters.Add(new MobileServiceFileJsonConverter(table.MobileServiceClient));
        //    }

        //    IFileSyncContext syncContext = MobileServiceFileSyncContext.GetContext(table.MobileServiceClient, metadataStore);

        //    IEnumerable<MobileServiceFile> files = await table.MobileServiceClient.InvokeApiAsync<IEnumerable<MobileServiceFile>>(route, HttpMethod.Get, null);

        //    foreach (var file in files)
        //    {
        //        var metadata = new MobileServiceFileMetadata
        //        {
        //            FileId = file.Id,
        //            FileName = file.Name,
        //            ContentMD5 = file.ContentMD5,
        //            LastSynchronized = DateTime.UtcNow,
        //            Length = file.Length,
        //            LocalPath = file.LocalFilePath,
        //            ParentDataItemType = table.TableName,
        //            ParentDataItemId = file.ParentDataItemId,
        //            Location = file.LocalFileExists ? FileLocation.LocalAndServer : FileLocation.Server,
        //        };

        //        await metadataStore.CreateOrUpdateAsync(metadata);
        //    }

        //    return files;
        //}

        public async static Task PurgeFilesAsync<T>(this IMobileServiceSyncTable<T> table)
        {
            IFileMetadataStore metadataStore = GetMetadataStore(table.MobileServiceClient.SyncContext.Store as MobileServiceLocalStore);

            await metadataStore.PurgeAsync(table.TableName);
        }
        

        public async static Task PushFileChangesAsync<T>(this IMobileServiceSyncTable<T> table)
        {
            IFileSyncContext context = MobileServiceFileSyncContext.GetContext(table.MobileServiceClient, metadataStore, fileSyncHandler);
            await context.PushChangesAsync(CancellationToken.None);
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
                ParentDataItemType = file.TableName,
                ParentDataItemId = file.ParentId
            };

            await metadataStore.CreateOrUpdateAsync(metadata);

            IFileSyncContext context = MobileServiceFileSyncContext.GetContext(table.MobileServiceClient, metadataStore, fileSyncHandler);
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
                await DownloadFileToStreamAsync<T>(table, file, stream);
            }
        }

        public async static Task DownloadFileToStreamAsync<T>(this IMobileServiceSyncTable<T> table, MobileServiceFile file, Stream stream)
        {

            StorageToken token = await GetStorageToken(table, file, StoragePermissions.Read);

            var container = new CloudBlobContainer(new Uri(token.RawToken));

            CloudBlob blob = container.GetBlobReference(file.Name);

            await blob.DownloadToStreamAsync(stream);
        }

        public async static Task DeleteFileAsync<T>(this IMobileServiceSyncTable<T> table, MobileServiceFile file)
        {
            IFileSyncContext context = MobileServiceFileSyncContext.GetContext(table.MobileServiceClient, metadataStore, fileSyncHandler);
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

        private static async Task<StorageToken> GetStorageToken<T>(this IMobileServiceSyncTable<T> table, MobileServiceFile file, StoragePermissions permissions)
        {
            var tokenRequest = new StorageTokenRequest();
            tokenRequest.Permissions = permissions;

            return await table.MobileServiceClient.InvokeApiAsync<StorageTokenRequest, StorageToken>("StorageToken", tokenRequest);
        }

    }
}
