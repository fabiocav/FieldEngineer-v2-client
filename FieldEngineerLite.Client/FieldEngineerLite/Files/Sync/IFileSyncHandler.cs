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
    }


    public interface IMobileServiceFileDataSource
    {
        Task<Stream> GetStream();
    }

    public class PathMobileServiceFileDataSource : IMobileServiceFileDataSource
    {
        private string filePath;

        public PathMobileServiceFileDataSource(string filePath)
        {
            this.filePath = filePath;
        }

        public Task<Stream> GetStream()
        {
            return Task.FromResult<Stream>(File.OpenRead(filePath));
        }
    }
}
