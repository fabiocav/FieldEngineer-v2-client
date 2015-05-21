using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using FieldEngineerLite.Helpers;
using Xamarin.Media;

[assembly: Xamarin.Forms.Dependency(typeof(FieldEngineerLite.Droid.MediaProvider))]

namespace FieldEngineerLite.Droid
{
    public class MediaProvider : IMediaPicker
    {
        public async System.Threading.Tasks.Task<string> GetPhotoAsync(object context)
        {
            var uiContext = context as Context;
            if (uiContext != null)
            {
                var mediaPicker = new MediaPicker(uiContext);
                var photo = await mediaPicker.PickPhotoAsync();

                return photo.Path;
            }

            return null;
        }
    }
}