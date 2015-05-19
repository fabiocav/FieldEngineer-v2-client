using FieldEngineerLite.Files;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FieldEngineerLite.Models
{
    public class JobImageViewModel : INotifyPropertyChanged
    {
        private MobileServiceFile file;
        private string filePath;

        public event PropertyChangedEventHandler PropertyChanged;
        private Job job;

        public JobImageViewModel(Job job, MobileServiceFile file)
        {
            this.file = file;
            this.job = job;
        }


        public string Name
        {
            get { return this.file.Name; }
        }

        public string FilePath
        {
            get
            {
                if (this.filePath == null)
                {
                    GetLocalFilePath();
                }

                return this.filePath;
            }
            private set
            {
                this.filePath = value;
                OnPropertyChanged();
            }
        }

        public Job Job
        {
            get { return this.job; }
        }

        public MobileServiceFile File
        {
            get { return this.file; }
        }

        private async void GetLocalFilePath()
        {
            // Disabling auto-download.... 
            //if (!file.IsLocalFileCurrent)
            //{
            //    await file.DownloadAsync();
            //}
            
            FilePath = file.LocalFilePath;
        }

        internal async Task DeleteFileAsync()
        {
            await App.JobService.DeleteFileAsync(this.job, this.file);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var temp = PropertyChanged;
            if (temp != null)
            {
                temp(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
