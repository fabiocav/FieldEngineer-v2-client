using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FieldEngineerLite.Files
{
    public class MobileServiceFile
    {
        private readonly Dictionary<string, string> metadata;
        private string localFilePath;
        private string name;
        private string id;

        private MobileServiceFile()
        {
            this.metadata = new Dictionary<string, string>();
        }

        private MobileServiceFile(string localFilePath, string id, string name)
            : this()
        {
            this.localFilePath = localFilePath;
            this.name = name;
            this.id = id;
        }

        public string Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public IDictionary<string, string> Metadata
        {
            get { return this.metadata; }
        }

        public async static Task<MobileServiceFile> FromFileAsync(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            if (string.IsNullOrWhiteSpace(path) || path.IndexOfAny(Path.GetInvalidPathChars()) > 0)
            {
                throw new ArgumentException("Invalid file name.", "fileName");
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException();
            }

            string fileName = Path.GetFileName(path);
            return new MobileServiceFile(path, fileName, fileName);
        }

        //public async static Task<MobileServiceFile> FromByteArrayAsync(byte[] bytes, string fileName)
        //{
        //    if (bytes == null)
        //    {
        //        throw new ArgumentNullException("stream");
        //    }

        //    if (string.IsNullOrWhiteSpace(fileName) || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) > 0)
        //    {
        //        throw new ArgumentException("Invalid file name.", "fileName");
        //    }


        //    string filesDirectory = GetTempFilesDirectoryAsync();
        //    using (Stream fileStream = File.Create()

        //    string fileId = Guid.NewGuid().ToString();
        //    StorageFile file = await folder.CreateFileAsync(fileId, CreationCollisionOption.ReplaceExisting);

        //    using (var backingFileStream = await file.OpenStreamForWriteAsync())
        //    {
        //        await backingFileStream.WriteAsync(bytes, 0, bytes.Length);
        //    }

        //    return new MobileServiceFile(file, fileId, fileName);
        //}

        internal Stream GetStreamAsync()
        {
            
            if (this.localFilePath == null)
            {
                return null;
            }

            return File.Open(this.localFilePath, FileMode.Open, FileAccess.ReadWrite);
        }

        //private async Task<StorageFile> GetOrCreateBackingFile()
        //{
        //    if (this.backingFile == null)
        //    {
        //        StorageFolder folder = await GetTempFilesDirectoryAsync();

        //        try
        //        {
        //            this.backingFile = await folder.GetFileAsync(this.id);
        //        }
        //        catch (FileNotFoundException)
        //        {
        //            // need to define what we want this behavior to be.
        //            // Auto download? throw an exception? return a null stream?
        //            return null;
        //        }
        //    }

        //    return this.backingFile;
        //}

        //internal async Task<string> GetTempFilePath()
        //{
        //    StorageFile file = await GetOrCreateBackingFile();

        //    if (file == null)
        //    {
        //        return null;
        //    }

        //    return file.Path;
        //}

        private static string GetTempFilesDirectoryAsync()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MobileServicesFiles");
        }
    }

}
