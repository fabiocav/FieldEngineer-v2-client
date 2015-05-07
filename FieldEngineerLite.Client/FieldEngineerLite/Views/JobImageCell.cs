using FieldEngineerLite.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FieldEngineerLite.Views
{
    public class JobImageCell : ViewCell
    {
        public JobImageCell()
        {
            this.Height = 50;

            var grid = new Grid();
            grid.ColumnDefinitions = new ColumnDefinitionCollection  
            {
                new ColumnDefinition { Width = new GridLength(100, GridUnitType.Absolute) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            };

            var image = new Image();
            image.HorizontalOptions = LayoutOptions.Start;
            image.WidthRequest = 100;
            grid.Children.Add(image, 0,0);


            //if (Device.OS == TargetPlatform.iOS)
                image.SetBinding(Image.SourceProperty, "FilePath");


            var label = new Label();
            grid.Children.Add(label, 1,0);

            label.Font = AppStyle.DefaultFont;
            label.SetBinding<JobImageViewModel>(Label.TextProperty, job => job.Name);

            var deletePhotoAction = new MenuItem { Text = "Delete", IsDestructive = true };
            deletePhotoAction.SetBinding(MenuItem.CommandParameterProperty, new Binding("."));
            deletePhotoAction.Command = JobDetailsPage.DeleteCommand;

            ContextActions.Add(deletePhotoAction);

            this.View = grid;
        }
    }
}
