﻿using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using FieldEngineerLite.Helpers;
using FieldEngineerLite.Models;
using FieldEngineerLite.ViewModels;

namespace FieldEngineerLite.Views
{
    public class JobCell : ViewCell
    {
        public JobCell()
        {
            var jobHeader = new JobHeaderView();

            var title = new Label();
            title.Font = AppStyle.DefaultFont;
            title.SetBinding<JobViewModel>(Label.TextProperty, job => job.Title);

            var customer = new Label();
            customer.Font = AppStyle.DefaultFont;
            customer.SetBinding<JobViewModel>(Label.TextProperty, job => job.Customer.FullName);            

            var jobDetails = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Padding = 5,
                Children =
                {
                    new StackLayout 
                    {
                        Orientation = StackOrientation.Horizontal,
                        Children = {
                            new Label { Text = "Customer:", Font = AppStyle.DefaultFont.WithAttributes(FontAttributes.Bold) },
                            customer
                        }
                    },
                    new Label { Text = "Description:", Font = AppStyle.DefaultFont.WithAttributes(FontAttributes.Bold) },
                    title
                }
            };
            jobDetails.SetBinding<JobViewModel>(StackLayout.BackgroundColorProperty, job => job.Status, converter: new JobStatusToColorConverter(useLightTheme: true));

            var rootLayout = new StackLayout()
            {
                Orientation = StackOrientation.Vertical,
                Spacing = 0,
                Children =
                {
                    jobHeader,
                    jobDetails
                }
            };

            this.Height = 130;
            this.View = rootLayout;
        }
    }
}
