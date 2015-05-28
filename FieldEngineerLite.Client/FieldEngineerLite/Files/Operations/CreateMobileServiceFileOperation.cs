using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FieldEngineerLite.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices;

namespace FieldEngineerLite.Files.Operations
{
    public class CreateMobileServiceFileOperation : MobileServiceFileOperation
    {
        public CreateMobileServiceFileOperation(IMobileServiceClient client, string fileId, IFileMetadataStore metadataStore, BlobStorageProvider storageProvider)
            : base(client, fileId, metadataStore, storageProvider)
        {
        }

        protected async override Task ExecuteOperation(IFileSyncContext context)
        {
            MobileServiceFileMetadata metadata = await MetadataStore.GetFileMetadataAsync(FileId);

            if (metadata != null)
            {
                var dataSource = await context.SyncHandler.GetDataSource(metadata);
                await StorageProvider.UploadFileAsync(metadata, dataSource);

                await MetadataStore.CreateOrUpdateAsync(metadata);
            }
        }

        public override void OnQueueingNewOperation(IMobileServiceFileOperation operation)
        {
        }
    }


}
