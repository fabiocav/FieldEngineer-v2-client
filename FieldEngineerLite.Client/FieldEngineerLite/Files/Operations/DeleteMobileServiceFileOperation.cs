using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FieldEngineerLite.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;

namespace FieldEngineerLite.Files.Operations
{
    public sealed class DeleteMobileServiceFileOperation : MobileServiceFileOperation
    {
        public DeleteMobileServiceFileOperation(IMobileServiceClient client, string fileId, IFileMetadataStore metadataStore, BlobStorageProvider storageProvider)
            : base(client, fileId, metadataStore, storageProvider)
        {
        }

        protected async override Task ExecuteOperation(IFileSyncContext context)
        {
            MobileServiceFileMetadata metadata = await MetadataStore.GetFileMetadataAsync(FileId);

            if (metadata != null)
            {
                await MetadataStore.DeleteAsync(metadata);

                string route = string.Format("/tables/{0}/{1}/MobileServiceFiles/{2}", metadata.ParentDataItemType, metadata.ParentDataItemId, metadata.FileName);

                var parameters = new Dictionary<string, string>();
                if (metadata.FileStoreUri != null)
                {
                    parameters.Add("x-zumo-filestoreuri", metadata.FileStoreUri);
                }

                await MobileClient.InvokeApiAsync(route, HttpMethod.Delete, parameters);
            }
        }

        public override void OnQueueingNewOperation(IMobileServiceFileOperation operation)
        {
            //
        }
    }
}
