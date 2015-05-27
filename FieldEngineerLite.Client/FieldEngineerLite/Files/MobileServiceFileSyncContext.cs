using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using FieldEngineerLite.Files.Operations;
using Microsoft.WindowsAzure.MobileServices;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;
using FieldEngineerLite.Files.Metadata;
using FieldEngineerLite.Files.Sync;
using System.Net.Http;
using System.Diagnostics;

namespace FieldEngineerLite.Files
{
    public class MobileServiceFileSyncContext : IFileSyncContext
    {
        private Queue<IMobileServiceFileOperation> operations = new Queue<IMobileServiceFileOperation>();
        private BlobStorageProvider storageProvider;
        private IMobileServiceClient mobileServiceClient;
        private IFileMetadataStore metadataStore;
        private static ConcurrentDictionary<IMobileServiceClient, IFileSyncContext> contexts = new ConcurrentDictionary<IMobileServiceClient, IFileSyncContext>();
        private SemaphoreSlim processingSemaphore = new SemaphoreSlim(1);
        private IFileSyncHandler syncHandler;

        public MobileServiceFileSyncContext(IMobileServiceClient client, IFileMetadataStore metadataStore, IFileSyncHandler syncHandler)
        {
            this.mobileServiceClient = client;
            this.metadataStore = metadataStore;
            this.syncHandler = syncHandler;

            this.storageProvider = new BlobStorageProvider(client);
        }

        public static IFileSyncContext GetContext(IMobileServiceClient client, IFileMetadataStore metadataStore, IFileSyncHandler syncHandler)
        {
            return contexts.GetOrAdd(client, c => new MobileServiceFileSyncContext(c, metadataStore, syncHandler));
        }

        public async Task AddFileAsync(MobileServiceFile file)
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

            var operation = new CreateMobileServiceFileOperation(this.mobileServiceClient, file.Id, metadataStore, storageProvider);

            await QueueOperationAsync(operation);
        }

        public async Task DeleteFileAsync(MobileServiceFile file)
        {
            var operation = new DeleteMobileServiceFileOperation(this.mobileServiceClient, file.Id, metadataStore, storageProvider);

            await QueueOperationAsync(operation);
        }

        public async Task PushChangesAsync(CancellationToken cancellationToken)
        {
            await processingSemaphore.WaitAsync(cancellationToken);
            try
            {
                while (this.operations.Count > 0)
                {
                    var operation = operations.Dequeue();

                    // This would also take the cancellation token
                    await operation.Execute(this);
                }
            }
            finally
            {
                processingSemaphore.Release();
            }
        }

        public async Task PullFilesAsync(string tableName, string itemId)
        {
            string route = string.Format("/tables/{0}/{1}/MobileServiceFiles", tableName, itemId);

            if (!this.mobileServiceClient.SerializerSettings.Converters.Any(p => p is MobileServiceFileJsonConverter))
            {
                this.mobileServiceClient.SerializerSettings.Converters.Add(new MobileServiceFileJsonConverter(this.mobileServiceClient));
            }

            IEnumerable<MobileServiceFile> files = await this.mobileServiceClient.InvokeApiAsync<IEnumerable<MobileServiceFile>>(route, HttpMethod.Get, null);

            foreach (var file in files)
            {
                Debug.WriteLine("PROCESSING FILE: " + file.Name);

                MobileServiceFileMetadata metadata = await this.metadataStore.GetFileMetadataAsync(file.Id);

                if (metadata == null)
                {
                    metadata = new MobileServiceFileMetadata
                    {
                        FileId = file.Id,
                        FileName = file.Name,
                        Length = file.Length,
                        ParentDataItemType = tableName,
                        ParentDataItemId = itemId,
                        PendingDeletion = false
                    };
                }

                if (string.Compare(metadata.ContentMD5, file.ContentMD5, StringComparison.InvariantCulture) != 0)
                {
                    metadata.LastSynchronized = DateTime.UtcNow;
                    metadata.ContentMD5 = file.ContentMD5;

                    await this.metadataStore.CreateOrUpdateAsync(metadata);
                    await this.syncHandler.ProcessNewFileAsync(file);
                }
            }

            // This is an example of how this would be handled. VERY simple logic right now... 
            var fileMetadata = await this.metadataStore.GetMetadataAsync(tableName, itemId);
            var deletedItemsMetadata = fileMetadata.Where(m => !files.Any(f => string.Compare(f.Id, m.FileId) == 0));

            foreach (var metadata in deletedItemsMetadata)
            {
                var pendingOperation = this.operations.FirstOrDefault(o=>string.Compare(o.FileId, metadata.FileId) == 0);
                
                // TODO: Need to call into the sync handler for conflict resolution here...
                if (pendingOperation == null || pendingOperation is DeleteMobileServiceFileOperation)
                {
                    await metadataStore.DeleteAsync(metadata);
                }
            }
        }

        public async Task<bool> QueueOperationAsync(IMobileServiceFileOperation operation)
        {
            bool operationEnqueued = false;

            await processingSemaphore.WaitAsync();
            try
            {
                var pendingItemOperations = this.operations.Where(o => string.Compare(o.FileId, operation.FileId) == 0);

                foreach (var item in operations)
                {
                    item.OnQueueingNewOperation(operation);
                }

                if (operation.State != FileOperationState.Cancelled)
                {
                    this.operations.Enqueue(operation);
                    operationEnqueued = true;
                }
            }
            finally
            {
                processingSemaphore.Release();
            }

            return operationEnqueued;
        }


        public IFileSyncHandler SyncHandler
        {
            get { return this.syncHandler; }
        }
    }

}
