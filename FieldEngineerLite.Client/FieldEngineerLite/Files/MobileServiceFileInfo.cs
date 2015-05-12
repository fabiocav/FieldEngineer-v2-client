using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FieldEngineerLite.Files
{
    public class MobileServiceFileMetadata
    {
        public string FileName { get; set; }

        public long Length { get; set; }

        public string ContentMD5 { get; set; }
    }

    public class MobileServiceFileInfo
    {

        public string Id { get; set; }

        public string Name { get; set; }

        public string ParentDataItemType { get; set; }

        public string ParentDataItemId { get; set; }

        public IDictionary<string, string> Metadata { get; set; }
    }
}
