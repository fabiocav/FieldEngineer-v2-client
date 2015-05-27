﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using FieldEngineerLite.Helpers;
using FieldEngineerLite.Models;
using System.Threading;
using FieldEngineerLite.Files;
using FieldEngineerLite.Files.Metadata;
using System.IO;
using System.Net.Http;
using FieldEngineerLite.Files.Sync;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FieldEngineerLite
{
    public class JobService
    {
        private MobileServiceClient MobileService = new MobileServiceClient(
           "https://fieldengineerfiles-code.azurewebsites.net",
           "https://default-sql-westus73e0c3fd2d6645ec9f32852f08e1f38f.azurewebsites.net",
           "tTWOhHeXaEKuNaONJQPkHVEKxUWzcP58",
           new LoggingHandler(true)
       );

        private IMobileServiceSyncTable<Job> jobTable;

        public async Task InitializeAsync()
        {
            var store = new MobileServiceSQLiteStoreWithLogging("localdata.db");
            store.ItemChanged += StoreItemChanged;

            store.DefineTable<Job>();
            DelegatedFileMetadataStore.DefineTable(store);

            //IFileSyncContext fileSyncContext = MobileServiceFileSyncContext.GetContext(this.MobileService)
            await this.MobileService.SyncContext.InitializeAsync(store);

            jobTable = this.MobileService.GetSyncTable<Job>();

            FieldEngineerLite.Files.MobileServiceSyncTableExtensions.InitializeFileSync(new FieldEngieerFileSyncHandler(this));
        }

        private async void StoreItemChanged(object sender, ItemChangedEventArgs e)
        {
            if (string.Compare(e.TableName, "Job") == 0)
            {
                Job job = await this.jobTable.LookupAsync(e.ItemId);

                if (e.ChangeType == ItemChangeType.AddedOrUpdated)
                {
                    // Retrieve files
                    await this.jobTable.PullFilesAsync(job);
                }
                else if (e.ChangeType == ItemChangeType.Deleted)
                {
                    // Purge all files
                    await this.jobTable.PurgeFilesAsync(job);
                }
            }
        }

        public async Task SyncAsync()
        {
            // Comment this back in if you want auth
            //if (!await IsAuthenticated())
            //    return;

            await this.MobileService.SyncContext.PushAsync();

            await this.jobTable.PushFileChangesAsync();

            var query = jobTable.CreateQuery()
                .Where(job => job.AgentId == "37e865e8-38f1-4e6b-a8ee-b404a188676e");

            await jobTable.PullAsync("myjobs", query);
        }

        public async Task<IEnumerable<Job>> SearchJobs(string searchInput)
        {
            var query = jobTable.CreateQuery();

            if (!string.IsNullOrWhiteSpace(searchInput))
            {
                query = query.Where(job =>
                    job.JobNumber == searchInput
                    || searchInput.ToUpper().Contains(job.Title.ToUpper()) // workaround bug: these are backwards
                    || searchInput.ToUpper().Contains(job.Status.ToUpper())
                );
            }

            return await query.ToEnumerableAsync();
        }

        public async Task CompleteJobAsync(Job job)
        {
            job.Status = Job.CompleteStatus;
            await jobTable.UpdateAsync(job);

            var inprogress = await jobTable
                .Where(j => j.Status == Job.InProgressStatus)
                .Take(1)
                .ToListAsync();

            if (inprogress.Count == 0)
            {
                var nextJob = (await jobTable
                    .Where(j => j.Status == Job.PendingStatus)
                    .Take(1)
                    .ToListAsync()
                ).FirstOrDefault();

                if (nextJob != null)
                {
                    nextJob.Status = Job.InProgressStatus;
                    await jobTable.UpdateAsync(nextJob);
                }
            }
        }

        public async Task ClearAllJobs()
        {
            await jobTable.PurgeAsync(true);
            await jobTable.PurgeFilesAsync();
            //await jobTable.PurgeAsync ("myjobs", jobTable.CreateQuery(), true, CancellationToken.None);
            await InitializeAsync();
        }

        private async Task<bool> IsAuthenticated()
        {
            if (this.MobileService.CurrentUser == null)
            {
                await this.MobileService.LoginAsync(App.UIContext, MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory);
            }

            return this.MobileService.CurrentUser != null;
        }

        internal async Task<IEnumerable<MobileServiceFile>> GetFilesAsync(Job job)
        {
            return await jobTable.GetFilesAsync(job);
        }

        internal async Task<MobileServiceFile> AddFileFromPath(Job job, string imagePath)
        {
            string fileName = Path.GetFileName(imagePath);
            MobileServiceFile file = this.jobTable.CreateFile(job, fileName);

            await this.jobTable.AddFileAsync(file);

            // "Touching" the file to force it to sync.
            // This is a simple approach for this demo
            await this.jobTable.UpdateAsync(job);

            return file;
        }

        internal async Task DeleteFileAsync(Job job, MobileServiceFile file)
        {
            await this.jobTable.DeleteFileAsync(file);

            // "Touching" the file to force it to sync.
            // This is a simple approach for this demo
            await this.jobTable.UpdateAsync(job);
        }

        internal async Task DownloadFileAsync(MobileServiceFile file)
        {
            string filePath = FileHelper.GetLocalFilePath(file.Name);

            await this.jobTable.DownloadFileAsync(file, filePath);
        }
    }

}

