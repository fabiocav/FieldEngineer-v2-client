using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;

namespace FieldEngineerLite.Files
{
    public abstract class MobileServiceFileOperation : IMobileServiceFileOperation
    {
        private IMobileServiceClient client;
        private BlobStorageManager storageManager;
        private string fileId;

        public MobileServiceFileOperation(IMobileServiceClient client, string fileId, IFileMetadataStore metadataStore, BlobStorageManager storageManager)
        {
            this.fileId = fileId;
            this.client = client;
            this.storageManager = storageManager;
        }

        public string FileId
        {
            get { return this.fileId; }
        }

        public IMobileServiceClient MobileClient
        {
            get { return this.client; }
            set { this.client = value; }
        }

        protected IFileMetadataStore MetadataStore
        {
            get { return MetadataStore; }
        }

        protected BlobStorageManager StorageManager
        {
            get { return this.storageManager; }
        }

        public abstract FileOperationKind Kind { get; }

        public FileOperationState State { get; protected set; }

        public async Task Execute()
        {
            try
            {
                this.State = FileOperationState.InProcess;

                await ExecuteOperation();
            }
            catch
            {
                this.State = FileOperationState.Failed;
                throw;
            }

            this.State = FileOperationState.Succeeded;
        }

        protected abstract Task ExecuteOperation();


        public abstract void OnQueueingNewOperation(IMobileServiceFileOperation operation);
    }
}
