using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
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
            if (file.Metadata != null && 
                file.Metadata.ContainsKey("isServiceContract") && 
                string.Compare(file.Metadata["isServiceContract"], "true", true) == 0)

            {
                return;
            }

            await this.jobService.DownloadFileAsync(file);
        }


        public Task ProcessFileSynchronizationAction(MobileServiceFile file, FileSynchronizationAction action)
        {
            throw new NotImplementedException();
        }

        public Task ProcessDeletedFileAsync(MobileServiceFile file)
        {
            throw new NotImplementedException();
        }
    }
}
