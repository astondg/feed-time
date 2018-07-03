namespace FeedTime.Common.Extensions
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Windows.Foundation;
    using Windows.Storage;

    public static class SerializationExtensions
    {
        private static JsonSerializerSettings serialisationSettings = new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore };

        public static IAsyncOperation<object> ReadJsonFileAs(this IStorageFile file, Type objectType)
        {
            return ReadJsonFileAsync(file, objectType).AsAsyncOperation();
        }

        public static IAsyncAction WriteJsonToFile(this IStorageFile file, Type objectType, object item)
        {
            return WriteJsonToFileAsync(file, objectType, item).AsAsyncAction();
        }

        private static async Task<object> ReadJsonFileAsync(this IStorageFile file, Type objectType)
        {
            using (var fileStream = await file.OpenReadAsync())
            {
                if (fileStream.Size == 0)
                    return null;

                var serializer = JsonSerializer.Create(serialisationSettings);
                using (var reader = new StreamReader(fileStream.AsStreamForRead()))
                using (var jsonReader = new JsonTextReader(reader))
                    return serializer.Deserialize(jsonReader, objectType);
            }
        }

        private static async Task WriteJsonToFileAsync(this IStorageFile file, Type objectType, object item)
        {
            var serializer = JsonSerializer.Create(serialisationSettings);
            using (var stream = await file.OpenStreamForWriteAsync())
            using (var writer = new StreamWriter(stream))
            using (var jsonWriter = new JsonTextWriter(writer))
                serializer.Serialize(jsonWriter, item, objectType);
        }
    }
}