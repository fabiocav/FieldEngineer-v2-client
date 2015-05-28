using FieldEngineerLite.Files;
using FieldEngineerLite.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System.Collections.ObjectModel;
using FieldEngineerLite.Views;

namespace FieldEngineerLite.ViewModels
{
    public class JobViewModel : INotifyPropertyChanged
    {
        private Job job;
        private ObservableCollection<JobImageViewModel> photos;

        public event PropertyChangedEventHandler PropertyChanged;

        public JobViewModel(Job job)
        {
            this.job = job;

            Equipments = new List<Equipment>{
                new Equipment{ 
                    Description = "Set top box", 
                    Id = "1231", 
                    Name = "Set top box",
                    ThumbImage = "Data/EquipmentImages/Dish_1_Thumb.jpg"
                },
                 new Equipment{ 
                    Description = "RCA cable", 
                    Id = "1110012", 
                    Name = "RCA cable",
                    ThumbImage = "Data/EquipmentImages/RCA_2_Thumb.jpg"
                }
            };
        }

        public string Id
        {
            get { return this.job.Id; }
            set { this.job.Id = value; }
        }

        public string AgentId
        {
            get { return this.job.AgentId; }
            set { this.job.AgentId = value; }
        }
        public string JobNumber
        {
            get { return this.job.JobNumber; }
            set { this.job.JobNumber = value; }
        }
        public string EtaTime
        {
            get { return this.job.EtaTime; }
            set { this.job.EtaTime = value; }
        }
        public string Status
        {
            get { return this.job.Status; }
            set { this.job.Status = value; }
        }

        public string Title
        {
            get { return this.job.Title; }
            set { this.job.Title = value; }
        }

        public string CustomerPhoneNumber
        {
            get { return this.job.CustomerPhoneNumber; }
            set { this.job.CustomerPhoneNumber = value; }
        }

        public string CustomerName
        {
            get { return this.job.CustomerName; }
            set
            {
                this.job.CustomerName = value;
                OnPropertyChanged();
                OnPropertyChanged("Customer");
            }
        }

        public string CustomerAddress
        {
            get { return this.job.CustomerAddress; }
            set
            {
                this.job.CustomerAddress = value;
                OnPropertyChanged();
                OnPropertyChanged("Customer");
            }
        }

        public Customer Customer
        {
            get
            {
                return new Customer
                {
                    FullName = this.CustomerName,
                    PrimaryContactNumber = this.CustomerPhoneNumber
                };
            }
        }

        public List<Equipment> Equipments
        {
            get { return this.job.Equipments; }
            set { this.job.Equipments = value; }
        }

        public ObservableCollection<JobImageViewModel> Photos
        {
            get
            {
                if (this.photos == null)
                {
                    RequestFiles();
                }

                return this.photos;
            }
            private set
            {
                this.photos = value;

                OnPropertyChanged();
            }
        }

        private async void RequestFiles()
        {
            IEnumerable<MobileServiceFile> files = await App.JobService.GetFilesAsync(this.job);

            Photos = new ObservableCollection<JobImageViewModel>(
                files.Where(f => !f.Name.EndsWith(".pdf"))
                .Select(f => new JobImageViewModel(this.job, f)));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var temp = PropertyChanged;
            if (temp != null)
            {
                temp(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        internal async Task CompleteJobAsync()
        {
            await App.JobService.CompleteJobAsync(job);
        }

        internal async Task AddPhotoAsync(string imagePath)
        {
            MobileServiceFile file = await App.JobService.AddFileFromPath(this.job, imagePath);
            
            this.photos.Add(new JobImageViewModel(this.job, file));
            OnPropertyChanged("Photos");

            //await file.UploadAsync();
        }

        internal async Task DeletePhotoAsync(JobImageViewModel imageViewModel)
        {
            await imageViewModel.DeleteFileAsync();
            
            this.Photos.Remove(imageViewModel);
        }

        internal async Task<MobileServiceFile> GetServiceContract()
        {
            IEnumerable<MobileServiceFile> files = await App.JobService.GetFilesAsync(this.job);

            return files.FirstOrDefault(f => f.Name.EndsWith(".pdf"));
        }
    }
}
