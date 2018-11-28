using System;
using Newtonsoft.Json;

namespace RestApiApp.Services.Json
{
    public interface IJsonService
    {
        string SerializeObject(object value);

        T DeserializeObject<T>(string value);

        T DeserializeStream<T>(JsonTextReader jsonTextReader);
    }
}
