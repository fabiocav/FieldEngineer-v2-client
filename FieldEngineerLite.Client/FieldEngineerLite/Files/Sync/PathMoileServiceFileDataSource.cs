using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FieldEngineerLite.Files.Sync
{
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
