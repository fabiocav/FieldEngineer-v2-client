using FieldEngineerLite.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Media;

[assembly: Xamarin.Forms.Dependency(typeof(FieldEngineerLite.iOS.MediaProvider))]

namespace FieldEngineerLite.iOS
{
    internal class MediaProvider : IMediaPicker
    {

        public async Task<string> GetPhotoAsync(object context)
        {
            var mediaPicker = new MediaPicker();
            var mediaFile = await mediaPicker.PickPhotoAsync();

            return mediaFile.Path;
        }
    }
}
