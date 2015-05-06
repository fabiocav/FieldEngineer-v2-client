using FieldEngineerLite.Files;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace FieldEngineerLite.Models
{
    public class JobImageViewModel : INotifyPropertyChanged
    {
        private MobileServiceFile file;

        public event PropertyChangedEventHandler PropertyChanged;
        private string filePath;

        public JobImageViewModel(MobileServiceFile file)
        {
            this.file = file;
        }


        public string Name
        {
            get { return this.file.Name; }
        }

        public string Description { get; set; }


        public ImageSource ImageSource
        {
            get
            {
                if (this.filePath == null)
                {
                    GetLocalFilePath();
                }

                return this.filePath == null ? null : ImageSource.FromFile(filePath);
            }
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
                OnPropertyChanged("ImageSource");
            }
        }

        private async void GetLocalFilePath()
        {
            string path = await file.GetLocalFilePathAsync();

            FilePath = path;
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
