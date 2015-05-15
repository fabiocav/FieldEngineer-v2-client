using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using FieldEngineerLite.Files.Operations;
using Microsoft.WindowsAzure.MobileServices;

namespace FieldEngineerLite.Files
{
    public class MobileServiceFileSyncContext : IFileSyncContext
    {
        private Queue<IMobileServiceFileOperation> operations = new Queue<IMobileServiceFileOperation>();
        private BlobStorageManager storageManager;
        private IMobileServiceClient mobileServiceClient;
        private IFileMetadataStore metadataStore;

        public MobileServiceFileSyncContext(IMobileServiceClient client, IFileMetadataStore metadataStore)
        {
            this.mobileServiceClient = client;
            this.metadataStore = metadataStore;

            this.storageManager = new BlobStorageManager(client);
        }

        public async Task AddFileAsync(MobileServiceFile file)
        {
            var operation = new CreateMobileServiceFileOperation(this.mobileServiceClient, file.Id, metadataStore, storageManager);
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
    }

}
