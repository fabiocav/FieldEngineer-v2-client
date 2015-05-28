using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FieldEngineerLite.Views;
using MonoTouch.Foundation;
using MonoTouch.QuickLook;
using MonoTouch.UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(PdfViewer), typeof(FieldEngineerLite.iOS.DocumentViewRenderer))]

namespace FieldEngineerLite.iOS
{

    public class DocumentViewRenderer : ViewRenderer<PdfViewer, UIView>
    {
        private QLPreviewController controller;

        protected override void OnElementChanged(ElementChangedEventArgs<PdfViewer> e)
        {
            base.OnElementChanged(e);

            this.controller = new QLPreviewController();
            this.controller.DataSource = new DocumentQLPreviewControllerDataSource(e.NewElement.FilePath);

            SetNativeControl(this.controller.View);
        }

        private class DocumentQLPreviewControllerDataSource : QLPreviewControllerDataSource
        {
            private string fileName;
            public DocumentQLPreviewControllerDataSource(string fileName)
            {
                this.fileName = fileName;
            }

            public override int PreviewItemCount(QLPreviewController controller)
            {
                return 1;
            }
            
            public override QLPreviewItem GetPreviewItem(QLPreviewController controller, int index)
            {
                var documents = NSBundle.MainBundle.BundlePath;
                var library = Path.Combine(documents, this.fileName);
                NSUrl url = NSUrl.FromFilename(library);

                return new PdfItem(string.Empty, url);
            }

            private class PdfItem : QLPreviewItem
            {
                private string itemTitle;
                private NSUrl itemUrl;

                public PdfItem(string title, NSUrl uri)
                {
                    this.itemTitle = title;
                    this.itemUrl = uri;
                }

                public override string ItemTitle
                {
                    get { return this.itemTitle; }
                }

                public override NSUrl ItemUrl
                {
                    get { return this.itemUrl; }
                }
            }
        }
    }

}
