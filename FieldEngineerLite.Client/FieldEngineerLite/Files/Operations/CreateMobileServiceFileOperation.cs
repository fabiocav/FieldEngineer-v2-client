using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;

namespace FieldEngineerLite.Files.Operations
{
    public class CreateMobileServiceFileOperation : MobileServiceFileOperation
    {
        public CreateMobileServiceFileOperation(IMobileServiceClient client, string fileId, IFileMetadataStore metadataStore, BlobStorageManager storageManager)
            : base(client, fileId, metadataStore, storageManager)
        {
        }

        public override FileOperationKind Kind
        {
            get { return FileOperationKind.Create; }
        }

        protected async override Task ExecuteOperation()
        {
            MobileServiceFileMetadata metadata = MetadataStore.GetFileMetadataAsync(FileId);

            if (metadata != null)
            {
                await StorageManager.UploadFile(metadata);
            }
        }

        public override void OnQueueingNewOperation(IMobileServiceFileOperation operation)
        {
        }
    }


}
