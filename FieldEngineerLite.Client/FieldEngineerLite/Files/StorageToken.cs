using System;
using System.Collections.Generic;
using System.Text;

namespace FieldEngineerLite.Files
{
    public class StorageToken
    {
        public string RawToken { get; set; }

        public StoragePermissions Permissions { get; set; }

        public StorageTokenScope Scope { get; set; }
    }
}
