using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.WindowsAzure.MobileServices.Files;
using Xamarin.Forms;

namespace FieldEngineerLite.Views
{
    public class DocumentPage : ContentPage
    {
        private MobileServiceFile file;
        string tempFilePath;

        public DocumentPage(MobileServiceFile file)
        {
            this.file = file;
            this.Content = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Children = 
                { 
                    new ActivityIndicator { IsEnabled = true, IsRunning= true, IsVisible = true}, 
                    new Label { Text = "Downloading document", TextColor = Color.Gray } 
                }
            };
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            DownloadFile();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }

        private async void DownloadFile()
        {
            this.tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".pdf");
            await App.JobService.DownloadFileAsync(file, tempFilePath);

            this.Content = new PdfViewer(tempFilePath);
        }
    }
}
