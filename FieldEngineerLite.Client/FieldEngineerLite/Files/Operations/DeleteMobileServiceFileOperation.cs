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

        protected async override Task ExecuteOperation()
        {
            MobileServiceFileMetadata metadata = await MetadataStore.GetFileMetadataAsync(FileId);

            if (metadata != null)
            {
                if ((metadata.Location & FileLocation.Local) == FileLocation.Local)
                {
                    await MetadataStore.DeleteAsync(metadata);
                }

                if ((metadata.Location & FileLocation.Server) == FileLocation.Server)
                {
                    string route = string.Format("/tables/{0}/{1}/MobileServiceFiles/{2}", metadata.ParentDataItemType, metadata.ParentDataItemId, metadata.FileName);

                    await MobileClient.InvokeApiAsync(route, HttpMethod.Delete, null);
                }
            }
        }

        public override void OnQueueingNewOperation(IMobileServiceFileOperation operation)
        {
            //
        }
    }
}
