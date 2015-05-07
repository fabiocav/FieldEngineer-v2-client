using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.MobileServices;
using FieldEngineerLite.Files;
using Newtonsoft.Json;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FieldEngineerLite.Models
{
    public class Job : INotifyPropertyChanged
    {
        public const string CompleteStatus = "Completed";
        public const string InProgressStatus = "On Site";
        public const string PendingStatus = "Not Started";

        private IEnumerable<JobImageViewModel> photos;
        private string customerName;
        private string customerAddress;

        public event PropertyChangedEventHandler PropertyChanged;


        public Job()
        {
            Equipments = new List<Equipment>{
                new Equipment{ 
                    Description = "Set top box", 
                    Id = "1231", 
                    Name = "Set top box",
                    ThumbImage = "Data/EquipementImages/Dish_1_Thumb.jpg"
                },
                 new Equipment{ 
                    Description = "RCA cable", 
                    Id = "1110012", 
                    Name = "RCA cable",
                    ThumbImage = "Data/EquipementImages/RCA_2_Thumb.jpg"
                }
            };
        }

        public string Id { get; set; }
        public string AgentId { get; set; }
        public string JobNumber { get; set; }
        public string EtaTime { get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
        public string CustomerPhoneNumber { get; set; }

        public string CustomerName
        {
            get { return this.customerName; }
            set
            {
                this.customerName = value;
                OnPropertyChanged();
                OnPropertyChanged("Customer");
            }
        }

        public string CustomerAddress
        {
            get { return this.customerAddress; }
            set
            {
                this.customerAddress = value;
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


        public List<Equipment> Equipments { get; set; }

        [Version]
        public string Version { get; set; }

        [JsonIgnore]
        public IEnumerable<JobImageViewModel> Photos
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var temp = PropertyChanged;
            if (temp != null)
            {
                temp(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private async void RequestFiles()
        {
            IEnumerable<MobileServiceFile> files = await App.JobService.GetFilesAsync(this);

            Photos = files.Select(f => new JobImageViewModel(f)).ToList();
        }

    }
}
