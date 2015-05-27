using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FieldEngineerLite.Files.Sync
{
    public interface IMobileServiceFileDataSource
    {
        Task<Stream> GetStream();
    }
}
