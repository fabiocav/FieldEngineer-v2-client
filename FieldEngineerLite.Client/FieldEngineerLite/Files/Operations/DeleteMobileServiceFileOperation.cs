using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;

namespace FieldEngineerLite.Files.Operations
{
    public sealed class DeleteMobileServiceFileOperation : MobileServiceFileOperation
    {
        public DeleteMobileServiceFileOperation(IMobileServiceClient client, string fileId, IFileMetadataStore metadataStore, BlobStorageManager storageManager)
            : base(client, fileId, metadataStore, storageManager)
        {
        }

        public override FileOperationKind Kind
        {
            get { return FileOperationKind.Delete; }
        }

        protected async override Task ExecuteOperation()
        {
            MobileServiceFileMetadata metadata = MetadataStore.GetFileMetadataAsync(FileId);

            if (metadata != null)
            {
                if ((metadata.Location & FileLocation.Local) == FileLocation.Local)
                {
                    File.Delete(metadata.LocalPath);

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
            throw new NotImplementedException();
        }
    }
}
