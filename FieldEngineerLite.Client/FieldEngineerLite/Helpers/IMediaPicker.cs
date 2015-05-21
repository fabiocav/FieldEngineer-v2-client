using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FieldEngineerLite.Helpers
{
    public interface IMediaPicker
    {
        Task<string> GetPhotoAsync(object context);
    }
}
