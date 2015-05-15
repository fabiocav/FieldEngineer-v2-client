using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FieldEngineerLite.Files
{
    public class MobileServiceFileMetadata
    {
        public string FileId { get; set; }

        public string FileName { get; set; }

        public long Length { get; set; }

        public string ContentMD5 { get; set; }

        public string LocalPath { get; set; }

        public FileLocation Location { get; set; }

        public DateTime? LastSynchronized { get; set; }

        public string ParentDataItemType { get; set; }

        public string ParentDataItemId { get; set; }

        public static MobileServiceFileMetadata FromFile(MobileServiceFile file)
        {
            return new MobileServiceFileMetadata
            {
                FileId = file.Id,
                FileName = file.Name,
                ContentMD5 = file.ContentMD5,
                LastSynchronized = DateTime.UtcNow,
                Length = file.Length,
                LocalPath = file.LocalFilePath,
                ParentDataItemType = file.TableName,
                ParentDataItemId = file.ParentDataItemId,
                Location = file.LocalFileExists ? FileLocation.LocalAndServer : FileLocation.Server,
            };
        }
    }


    public class MobileServiceFileInfo
    {

        public string Id { get; set; }

        public string Name { get; set; }

        public string ParentDataItemType { get; set; }

        public string ParentDataItemId { get; set; }

        public string ContentMD5 { get; set; }

        public long Length { get; set; }

        public IDictionary<string, string> Metadata { get; set; }
    }
}
