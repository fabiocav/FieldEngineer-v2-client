﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FieldEngineerLite.Files.Metadata
{
    public class InMemoryFileMetadataStore : IFileMetadataStore
    {
        private List<MobileServiceFileMetadata> metadataCollection = new List<MobileServiceFileMetadata>();

        public Task CreateOrUpdateAsync(MobileServiceFileMetadata metadata)
        {


            if (this.metadataCollection.Any(m => string.Compare(m.FileId, metadata.FileId) == 0))
            {
                // Ignore this for now.... 
                this.metadataCollection.Remove(metadata);
            }

            this.metadataCollection.Add(metadata);

            return Task.FromResult(0);
        }


        public Task<MobileServiceFileMetadata> GetFileMetadataAsync(string fileId)
        {
            MobileServiceFileMetadata metadata =  this.metadataCollection.FirstOrDefault(m => string.Compare(m.FileId, fileId) == 0);

            return Task.FromResult(metadata);
        }


        public Task DeleteAsync(MobileServiceFileMetadata metadata)
        {
            this.metadataCollection.Remove(metadata);

            return Task.FromResult(0);
        }


        public Task<IEnumerable<MobileServiceFileMetadata>> GetMetadataAsync(string tableName, string objectId)
        {
            var result = this.metadataCollection.Where(m => string.Compare(m.ParentDataItemType, tableName) == 0
                && string.Compare(m.ParentDataItemId, objectId) == 0);

            return Task.FromResult(result);
        }
    }
}
