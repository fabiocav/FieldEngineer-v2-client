using FieldEngineerLite.Models;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace FieldEngineerLite.Files
{
    public static class MobileServiceTableExtensions
    {
        public async static Task<IEnumerable<MobileServiceFile>> GetFilesAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem)
        {
            string route = string.Format("/tables/{0}/{1}/MobileServiceFiles", table.TableName, GetDataItemId(dataItem));

            if (!table.MobileServiceClient.SerializerSettings.Converters.Any(p => p is MobileServiceFileJsonConverter))
            {
                table.MobileServiceClient.SerializerSettings.Converters.Add(new MobileServiceFileJsonConverter(table.MobileServiceClient));
            }

            return await table.MobileServiceClient.InvokeApiAsync<IEnumerable<MobileServiceFile>>(route, HttpMethod.Get, null);
        }

        public async static Task<MobileServiceFile> CreateFileFromPath<T>(this IMobileServiceSyncTable<T> table, T dataItem, string filePath)
        {
            return await MobileServiceFile.FromFile(table.MobileServiceClient, table.TableName, GetDataItemId(dataItem), filePath);
        }

        private static string GetDataItemId(object dataItem)
        {
            // This would be replaced with the logic used to resolve object IDs
            return ((Job)dataItem).Id;
        }
    }
}
