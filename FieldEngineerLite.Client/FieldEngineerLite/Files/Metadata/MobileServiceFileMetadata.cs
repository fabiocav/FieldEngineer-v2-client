using System;
using System.Collections.Generic;
using System.Text;

namespace FieldEngineerLite.Files.Metadata
{
    public class MobileServiceFileMetadata
    {
        public string Id
        {
            get { return this.FileId; }
            set { this.FileId = value; }
        }
        public string FileId { get; set; }

        public string FileName { get; set; }

        public long Length { get; set; }

        public string ContentMD5 { get; set; }

        public string LocalPath { get; set; }

        public FileLocation Location { get; set; }

        public DateTime? LastSynchronized { get; set; }

        public string ParentDataItemType { get; set; }

        public string ParentDataItemId { get; set; }

        public bool PendingDeletion { get; set; }

        public static MobileServiceFileMetadata FromFile(MobileServiceFile file)
        {
            return new MobileServiceFileMetadata
            {
                FileId = file.Id,
                FileName = file.Name,
                ContentMD5 = file.ContentMD5,
                LastSynchronized = DateTime.UtcNow,
                Length = file.Length,
                ParentDataItemType = file.TableName,
                PendingDeletion = false
            };
        }
    }
}
