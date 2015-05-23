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


        public Task<bool> QueueOperationAsync(IMobileServiceFileOperation operation)
        {
            bool operationEnqueued = false;

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

            return Task.FromResult(operationEnqueued);
        }


        public IFileSyncHandler SyncHandler
        {
            get { return this.syncHandler; }
        }
    }

}
