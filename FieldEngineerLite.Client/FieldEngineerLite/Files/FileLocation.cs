using System;
using System.Collections.Generic;
using System.Text;

namespace FieldEngineerLite.Files
{
    [Flags]
    public enum FileLocation
    {
        Local,
        Server,
        LocalAndServer = Local | Server
    }
}
