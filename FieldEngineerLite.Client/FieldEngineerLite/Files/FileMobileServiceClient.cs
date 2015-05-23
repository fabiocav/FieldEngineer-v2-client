using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FieldEngineerLite.Files.Metadata;
using FieldEngineerLite.Files.Sync;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FieldEngineerLite.Files
{

    //public class FileMobileServiceClient : MobileServiceClient
    //{
    //    private IFileSyncHandler fileSyncHandler;

    //    public FileMobileServiceClient(string mobileAppUri, string gatewayUri, string applicationKey, IFileSyncHandler fileSyncHandler, params HttpMessageHandler[] httpHandlers)
    //        : base(mobileAppUri, gatewayUri, applicationKey, httpHandlers)
    //    {
    //        this.fileSyncHandler = fileSyncHandler;
    //    }


    //    internal static IFileMetadataStore metadataStore;

    //    //public async static Task<IEnumerable<MobileServiceFile>> GetFilesAsync<T>(T dataItem)
    //    //{
    //    //    IFileMetadataStore metadataStore = GetMetadataStore(table.MobileServiceClient.SyncContext.Store as MobileServiceLocalStore);

    //    //    var fileMetadata = await metadataStore.GetMetadataAsync(table.TableName, GetDataItemId(dataItem));

    //    //    return fileMetadata.Select(m => MobileServiceFile.FromMetadata(table.MobileServiceClient, m));
    //    //}

    //    private static IFileMetadataStore GetMetadataStore(MobileServiceLocalStore localStore)
    //    {
    //        if (metadataStore == null)
    //        {
    //            //metadataStore = new DelegatedFileMetadataStore(localStore);
    //            metadataStore = new DelegatedFileMetadataStore(localStore);
    //        }

    //        return metadataStore;
    //    }
    //    //public async static Task<IEnumerable<MobileServiceFile>> GetFilesAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem)
    //    //{
    //    //    string route = string.Format("/tables/{0}/{1}/MobileServiceFiles", table.TableName, GetDataItemId(dataItem));

    //    //    if (!table.MobileServiceClient.SerializerSettings.Converters.Any(p => p is MobileServiceFileJsonConverter))
    //    //    {
    //    //        table.MobileServiceClient.SerializerSettings.Converters.Add(new MobileServiceFileJsonConverter(table.MobileServiceClient));
    //    //    }

    //    //    IFileSyncContext syncContext = MobileServiceFileSyncContext.GetContext(table.MobileServiceClient, metadataStore);

    //    //    IEnumerable<MobileServiceFile> files = await table.MobileServiceClient.InvokeApiAsync<IEnumerable<MobileServiceFile>>(route, HttpMethod.Get, null);

    //    //    foreach (var file in files)
    //    //    {
    //    //        var metadata = new MobileServiceFileMetadata
    //    //        {
    //    //            FileId = file.Id,
    //    //            FileName = file.Name,
    //    //            ContentMD5 = file.ContentMD5,
    //    //            LastSynchronized = DateTime.UtcNow,
    //    //            Length = file.Length,
    //    //            LocalPath = file.LocalFilePath,
    //    //            ParentDataItemType = table.TableName,
    //    //            ParentDataItemId = file.ParentDataItemId,
    //    //            Location = file.LocalFileExists ? FileLocation.LocalAndServer : FileLocation.Server,
    //    //        };

    //    //        await metadataStore.CreateOrUpdateAsync(metadata);
    //    //    }

    //    //    return files;
    //    //}

    //    public async Task PushFileChangesAsync()
    //    {
    //        IFileSyncContext context = MobileServiceFileSyncContext.GetContext(this, metadataStore);
    //        await context.PushChangesAsync(CancellationToken.None);
    //    }

    //    public async Task AddFileAsync(MobileServiceFile file)
    //    {

    //        var metadata = new MobileServiceFileMetadata
    //        {
    //            FileId = file.Id,
    //            FileName = file.Name,
    //            Length = file.Length,
    //            Location = FileLocation.Local,
    //            ContentMD5 = file.ContentMD5,
    //        };

    //        await metadataStore.CreateOrUpdateAsync(metadata);

    //        IFileSyncContext context = MobileServiceFileSyncContext.GetContext(this, metadataStore);
    //        await context.AddFileAsync(file);
    //    }

    //    public async Task UploadFileAsync<T>(MobileServiceFile file, string filePath)
    //    {
    //        StorageToken token = await GetStorageToken(file, StoragePermissions.Write);

    //        var container = new CloudBlobContainer(new Uri(token.RawToken));

    //        CloudBlockBlob blob = container.GetBlockBlobReference(file.Name);

    //        using (var stream = File.OpenRead(filePath))
    //        {
    //            await blob.UploadFromStreamAsync(stream).ContinueWith(t => Console.Write(t.IsFaulted));
    //        }
    //    }

    //    public async Task DownloadFileAsync(MobileServiceFile file, string targetPath)
    //    {
    //        using (Stream stream = File.Create(targetPath))
    //        {
    //            await DownloadFileToStreamAsync(file, stream);
    //        }
    //    }

    //    public async Task DownloadFileToStreamAsync(MobileServiceFile file, Stream stream)
    //    {

    //        StorageToken token = await GetStorageToken(file, StoragePermissions.Read);

    //        var container = new CloudBlobContainer(new Uri(token.RawToken));

    //        CloudBlob blob = container.GetBlobReference(file.Name);

    //        await blob.DownloadToStreamAsync(stream);
    //    }

    //    public async Task DeleteFileAsync(MobileServiceFile file)
    //    {

    //        IFileSyncContext context = MobileServiceFileSyncContext.GetContext(this, metadataStore);
    //        await context.DeleteFileAsync(file);
    //    }

    //    private static string GetFilesDirectoryAsync()
    //    {
    //        string filesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MobileServicesFiles");

    //        if (!Directory.Exists(filesPath))
    //        {
    //            Directory.CreateDirectory(filesPath);
    //        }

    //        return filesPath;
    //    }

       
    //    private async Task<StorageToken> GetStorageToken(MobileServiceFile file, StoragePermissions permissions)
    //    {
    //        var tokenRequest = new StorageTokenRequest();
    //        tokenRequest.Permissions = permissions;

    //        return await InvokeApiAsync<StorageTokenRequest, StorageToken>("StorageToken", tokenRequest);
    //    }
    //}
}
