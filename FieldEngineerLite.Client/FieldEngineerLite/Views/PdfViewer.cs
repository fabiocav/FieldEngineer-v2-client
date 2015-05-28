using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace FieldEngineerLite.Views
{
    public class PdfViewer : View
    {
        public PdfViewer(string filePath)
        {
            this.FilePath = filePath;
        }

        public string FilePath { get; set; }

    }
}
