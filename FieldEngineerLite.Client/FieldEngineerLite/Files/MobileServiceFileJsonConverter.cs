using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FieldEngineerLite.Files
{
    public class MobileServiceFileJsonConverter : JsonConverter
    {
        private Type mobileServiceFileType;
        private IMobileServiceClient mobileServiceClient;
        private IFileMetadataManager fileMetadataManager;

        public MobileServiceFileJsonConverter(IMobileServiceClient client)
        {
            this.mobileServiceFileType = typeof(MobileServiceFile);
            this.mobileServiceClient = client;
            this.fileMetadataManager = new FileSystemMetadataManager();
        }

        public override bool CanConvert(Type objectType)
        {
            return mobileServiceFileType.IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            MobileServiceFileInfo fileInfo = serializer.Deserialize<MobileServiceFileInfo>(reader);

            return new MobileServiceFile(this.mobileServiceClient, this.fileMetadataManager, 
                fileInfo.ParentDataItemType, fileInfo.ParentDataItemId, fileInfo.Name);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException("MobileServiceFileJsonConverter does not support serialization");
        }

        public override bool CanWrite { get { return false; } }
    }
}
