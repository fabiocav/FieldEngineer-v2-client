﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using FieldEngineerLite.Helpers;
using FieldEngineerLite.Models;
using Microsoft.WindowsAzure.MobileServices.Files;
using FieldEngineerLite.ViewModels;
using System.Windows.Input;
using System.IO;

namespace FieldEngineerLite.Views
{
    public class JobDetailsPage : ContentPage
    {
        public JobDetailsPage()
        {
            TableSection mainSection = new TableSection();


            mainSection.Add(new DataElementCell("Title", "Description"));
            mainSection.Add(new DataElementCell("Customer.FullName", "Customer"));
            mainSection.Add(new DataElementCell("Customer.Address", "Address") { Height = 60 });
            mainSection.Add(new DataElementCell("Customer.PrimaryContactNumber", "Telephone"));

            var statusCell = new DataElementCell("Status");
            statusCell.ValueLabel.SetBinding<JobViewModel>(Label.TextColorProperty, job => job.Status, converter: new JobStatusToColorConverter());
            mainSection.Add(statusCell);

            var photosSection = new TableSection("Photos");

            var photosRowTemplate = new DataTemplate(typeof(JobImageCell));

            var photosListView = new ListView
             {
                 RowHeight = 70,
                 ItemTemplate = photosRowTemplate,
                 SeparatorVisibility = SeparatorVisibility.None
             };

            photosListView.SetBinding<JobViewModel>(ListView.ItemsSourceProperty, job => job.Photos);

            var photosCell = new ViewCell { View = photosListView };
            photosSection.Add(photosCell);
            photosCell.Height = 150;



            TextCell addPhoto = new TextCell
            {
                Text = "Add Job Photo",
                TextColor = AppStyle.DefaultActionColor
            };

            addPhoto.Tapped += async delegate
            {
                await this.GetImageAsync();
            };

            photosSection.Add(addPhoto);

            var equipmentSection = new TableSection("Equipment");
            var equipmentRowTemplate = new DataTemplate(typeof(ImageCell));
            equipmentRowTemplate.SetBinding(ImageCell.TextProperty, "Name");
            equipmentRowTemplate.SetBinding(ImageCell.DetailProperty, "Description");

            // I don't have images working on Android yet
            if (Device.OS == TargetPlatform.iOS)
                equipmentRowTemplate.SetBinding(ImageCell.ImageSourceProperty, "ThumbImage");

            var equipmentListView = new ListView
            {
                RowHeight = 50,
                ItemTemplate = equipmentRowTemplate
            };
            equipmentListView.SetBinding<JobViewModel>(ListView.ItemsSourceProperty, job => job.Equipments);

            var equipmentCell = new ViewCell { View = equipmentListView };

            equipmentSection.Add(equipmentCell);

            var actionsSection = new TableSection("Actions");

            TextCell viewServiceContract = new TextCell
            {
                Text = "View Service Contract",
                TextColor = AppStyle.DefaultActionColor
            };

            viewServiceContract.Tapped += async delegate
            {
                await this.ViewServiceContract();
            };

            actionsSection.Add(viewServiceContract);

            TextCell completeJob = new TextCell
            {
                Text = "Mark Job as Complete",
                TextColor = AppStyle.DefaultActionColor
            };

            completeJob.Tapped += async delegate
            {
                await this.CompleteJobAsync();
            };

            actionsSection.Add(completeJob);

            var table = new TableView
            {
                Intent = TableIntent.Form,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HasUnevenRows = true,
                Root = new TableRoot("Root")
                {
                    mainSection, photosSection, actionsSection, equipmentSection
                }
            };

            table.SetBinding<JobViewModel>(TableView.BackgroundColorProperty, job => job.Status, converter: new JobStatusToColorConverter(useLightTheme: true));

            this.Title = "Job Details";
            this.Content = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Children = { new JobHeaderView(), table }
            };

            this.BindingContextChanged += delegate
            {
                if (SelectedJob != null && SelectedJob.Equipments != null)
                    equipmentCell.Height = SelectedJob.Equipments.Count * equipmentListView.RowHeight;
            };

            DeleteCommand = new Command<JobImageViewModel>(async m => await DeleteImageAsync(m));
        }

        private async Task ViewServiceContract()
        {
            MobileServiceFile serviceContract = await this.SelectedJob.GetServiceContract();

            if (serviceContract != null)
            {
                var documentPage = new DocumentPage(serviceContract);

                await this.Navigation.PushAsync(documentPage);
            }
            else
            {
                this.DisplayAlert("No service contract", "There is no service contract available for this record.", "OK");
            }
        }

        public static ICommand DeleteCommand
        {
            get;
            private set;
        }

        private JobViewModel SelectedJob
        {
            get { return this.BindingContext as JobViewModel; }
        }

        private async Task CompleteJobAsync()
        {
            var job = this.SelectedJob;
            await job.CompleteJobAsync();

            // Force a refresh
            this.BindingContext = null;
            this.BindingContext = job;
        }

        private async Task GetImageAsync()
        {
            JobViewModel job = this.SelectedJob;
            if (job != null)
            {
                IMediaPicker mediaProvider = DependencyService.Get<IMediaPicker>();
                string sourceImagePath = await mediaProvider.GetPhotoAsync(App.UIContext);

                string imagePath = FileHelper.CopyJobFile(sourceImagePath);
                await job.AddPhotoAsync(imagePath);

                // Force a refresh
                this.BindingContext = null;
                this.BindingContext = job;
            }
        }

        private async Task<object> DeleteImageAsync(JobImageViewModel imageViewModel)
        {
            await this.SelectedJob.DeletePhotoAsync(imageViewModel);
            return null;
        }

        private class DataElementCell : ViewCell
        {
            public Label DescriptionLabel { get; set; }
            public Label ValueLabel { get; set; }

            public DataElementCell(string property, string propertyDescription = null)
            {
                DescriptionLabel = new Label
                {
                    Text = propertyDescription ?? property,
                    Font = AppStyle.DefaultFont.WithAttributes(FontAttributes.Bold),
                    WidthRequest = 150,
                    VerticalOptions = LayoutOptions.CenterAndExpand
                };

                ValueLabel = new Label
                {
                    Font = AppStyle.DefaultFont,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    XAlign = TextAlignment.End
                };
                ValueLabel.SetBinding(Label.TextProperty, property);

                this.View = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Padding = 10,
                    Children = { DescriptionLabel, ValueLabel }
                };
            }
        }
    }
}
