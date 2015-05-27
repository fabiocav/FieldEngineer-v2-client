using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FieldEngineerLite.Files.Metadata;

namespace FieldEngineerLite.Files.Sync
{
    public interface IFileSyncHandler
    {
        Task<IMobileServiceFileDataSource> GetDataSource(MobileServiceFileMetadata metadata);

        Task ProcessNewFileAsync(MobileServiceFile metadata);
    }
}
