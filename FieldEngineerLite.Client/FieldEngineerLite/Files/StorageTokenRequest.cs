using System;
using System.Collections.Generic;
using System.Text;

namespace FieldEngineerLite.Files
{
    public class StorageTokenRequest
    {
        public StoragePermissions Permissions { get; set; }

        public string ScopedEntityId { get; set; }

        public string ProviderName { get; set; }
    }
}
