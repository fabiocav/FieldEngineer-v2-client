using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FieldEngineerLite.Files;
using FieldEngineerLite.Files.Metadata;
using FieldEngineerLite.Files.Sync;
using FieldEngineerLite.Helpers;

namespace FieldEngineerLite
{
    public class FieldEngieerFileSyncHandler : IFileSyncHandler
    {
        private JobService jobService;

        public FieldEngieerFileSyncHandler(JobService jobService)
        {
            if (jobService == null)
            {
                throw new ArgumentNullException("jobService");
            }

            this.jobService = jobService;
        }

        public Task<IMobileServiceFileDataSource> GetDataSource(MobileServiceFileMetadata metadata)
        {
            IMobileServiceFileDataSource source = new PathMobileServiceFileDataSource(FileHelper.GetLocalFilePath(metadata.FileName));

            return Task.FromResult(source);
        }


        public async Task ProcessNewFileAsync(MobileServiceFile file)
        {
            // TODO: Decide what files to download based on user defined metadata
            await this.jobService.DownloadFileAsync(file);
        }
    }
}
