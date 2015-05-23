using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Query;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;

namespace FieldEngineerLite.Files.Metadata
{
    public class DelegatedFileMetadataStore : IFileMetadataStore
    {
        public const string FileMetadataTableName = "__filesmetadata";

        private MobileServiceLocalStore store;

        public DelegatedFileMetadataStore(MobileServiceLocalStore store)
        {
            this.store = store;
        }

        internal static void DefineTable(MobileServiceLocalStore store)
        {

            store.DefineTable(FileMetadataTableName, new JObject()
            {
                { MobileServiceSystemColumns.Id, String.Empty },
                { "fileId", string.Empty },
                { "fileName", string.Empty },
                { "length", 0 },
                { "contentMD5", string.Empty },
                { "localPath", string.Empty },
                { "location", FileLocation.Local.ToString() },
                { "lastSyncrhonized", DateTime.Now },
                { "parentDataItemType", string.Empty },
                { "parentDataItemId", string.Empty },
                { "pendingDeletion", false }
            });
        }

        public async Task CreateOrUpdateAsync(MobileServiceFileMetadata metadata)
        {
            JObject jsonObject = JObject.FromObject(metadata);
            await this.store.UpsertAsync(FileMetadataTableName, new[] { jsonObject }, true);
        }

        public async Task<MobileServiceFileMetadata> GetFileMetadataAsync(string fileId)
        {
            JObject metadata = await this.store.LookupAsync(FileMetadataTableName, fileId);

            if (metadata != null)
            {
                return metadata.ToObject<MobileServiceFileMetadata>();
            }

            return null;
        }

        public async Task DeleteAsync(MobileServiceFileMetadata metadata)
        {
            await this.store.DeleteAsync(FileMetadataTableName, new[] { metadata.Id });
        }

        public async Task<IEnumerable<MobileServiceFileMetadata>> GetMetadataAsync(string tableName, string objectId)
        {
            var query = MobileServiceTableQueryDescription.Parse(FileMetadataTableName, string.Format("$filter=parentDataItemType eq '{0}' and parentDataItemId eq '{1}'" , tableName, objectId ));

            var result = await this.store.ReadAsync(query);

            return result.ToObject<List<MobileServiceFileMetadata>>();
            //return null;
        }


        public async Task PurgeAsync(string tableName)
        {
            var query = MobileServiceTableQueryDescription.Parse(FileMetadataTableName, string.Format("$filter=parentDataItemType eq '{0}'" , tableName));
            await this.store.DeleteAsync(query);
        }
    }
}
