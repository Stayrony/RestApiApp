using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestApiApp.Models.Rest;
using RestApiApp.Services.Json;

namespace RestApiApp.Services.Rest
{
    public class RestService : IRestService
    {
        private readonly IJsonService _jsonService;

        public RestService(IJsonService jsonService)
        {
            _jsonService = jsonService;

            BaseUrl = Constants.BASE_URL;
        }

        public string BaseUrl { get; private set; }

        #region -- IRestService implementation --

        public async Task<RestResult<T200>> GetAsync_1<T200, T400, T401, T404, T403>(string resource, Dictionary<string, string> additioalHeaders = null)
        {
            using (var client = CreateHttpClient(DefaultHeaders(additioalHeaders)))
            {
                using (var response = await client.GetAsync(GetRequestUrl(BaseUrl, resource)).ConfigureAwait(false))
                {

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(stream))
                    using (var json = new JsonTextReader(reader))
                    {
                        if (response.StatusCode.Equals(HttpStatusCode.OK))
                        {
                            return RestResult<T200>.Ok(_jsonService.DeserializeStream<T200>(json));
                        }
                        else if (response.StatusCode.Equals(HttpStatusCode.BadRequest))
                        {
                            // return RestResult<T400>.Fail(HttpStatusCode.BadRequest, _jsonService.DeserializeStream<T400>(json));
                        }
                        else if(response.StatusCode.Equals(HttpStatusCode.NotFound))
                        {
                            // return RestResult<T404>.Fail(HttpStatusCode.NotFound, _jsonService.DeserializeStream<T404>(json));
                        }
                    }

                    return RestResult<T200>.Fail(response.StatusCode);
                }
            }
        }

        public async Task<T> GetAsync<T>(string resource, Dictionary<string, string> additioalHeaders = null)
        {
            using (var client = CreateHttpClient(DefaultHeaders(additioalHeaders)))
            {
                using (var response = await client.GetAsync(GetRequestUrl(BaseUrl, resource)).ConfigureAwait(false))
                {
                    // ThrowIfNotSuccess(response);
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(stream))
                    using (var json = new JsonTextReader(reader))
                    {
                        return _jsonService.DeserializeStream<T>(json);
                    }
                }
            }
        }

        public Task<T> PutAsync<T>(string resource, object requestBody, Dictionary<string, string> additioalHeaders = null)
        {
            var jsonString = _jsonService.SerializeObject(requestBody);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            return PutAsync<T>(resource, content, CancellationToken.None, additioalHeaders);
        }

        public async Task<T> DeleteAsync<T>(string resource, Dictionary<string, string> additioalHeaders = null)
        {
            using (var client = CreateHttpClient(DefaultHeaders(additioalHeaders)))
            {
                using (var response = await client.DeleteAsync(GetRequestUrl(BaseUrl, resource)).ConfigureAwait(false))
                {
                    //ThrowIfNotSuccess(response);
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(stream))
                    using (var json = new JsonTextReader(reader))
                    {
                        return _jsonService.DeserializeStream<T>(json);
                    }
                }
            }
        }

        public async Task<T> DeleteAsync<T>(string resource, object requestBody, Dictionary<string, string> additioalHeaders = null)
        {
            using (var client = CreateHttpClient(DefaultHeaders(additioalHeaders)))
            {
                var jsonString = _jsonService.SerializeObject(requestBody);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                using (var request = new HttpRequestMessage
                {
                    Content = content,
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(GetRequestUrl(BaseUrl, resource))
                })
                {
                    using (var response = await client.SendAsync(request).ConfigureAwait(false))
                    {
                        // ThrowIfNotSuccess(response)
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var reader = new StreamReader(stream))
                        using (var json = new JsonTextReader(reader))
                        {
                            return _jsonService.DeserializeStream<T>(json);
                        }
                    }
                }
            }
        }

        public Task<T> PostAsync<T>(string resource, object requestBody, Dictionary<string, string> additioalHeaders = null)
        {
            var jsonString = _jsonService.SerializeObject(requestBody);

            HttpContent content = requestBody as HttpContent;
            if (requestBody is IEnumerable<KeyValuePair<string, string>>)
            {
                content = new FormUrlEncodedContent(requestBody as IEnumerable<KeyValuePair<string, string>>);
            }

            if (content == null)
            {
                content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            }

            return PostAsync<T>(resource, content, CancellationToken.None, additioalHeaders);
        }

        #endregion

        #region -- Private helpers --
        private static void ThrowIfNotSuccess(HttpResponseMessage response)
        {
            HttpStatusCode status = response.StatusCode;
            //response.StatusCode = HttpStatusCode.OK;
            //int httpCodeType = ((int)response.StatusCode) / 100;

            //if (httpCodeType == 4 || httpCodeType == 5)
            //{
            //    result.SetFailure(EServerError.ServerBroken);
            //    return result;
            //}
            //else if (httpCodeType == 0)
            //{
            //    result.SetFailure(EServerError.NoInternent);
            //    return result;
            //}
            if (!response.IsSuccessStatusCode)
            {
                response.EnsureSuccessStatusCode();
            }
        }

        private async Task<T> PostAsync<T>(string resource, HttpContent content, CancellationToken cancellationToken, Dictionary<string, string> additioalHeaders = null)
        {
            using (var client = CreateHttpClient(DefaultHeaders(additioalHeaders)))
            {
                using (var response = await client.PostAsync(GetRequestUrl(BaseUrl, resource), content, cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    //ThrowIfNotSuccess(response);
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(stream))
                    using (var json = new JsonTextReader(reader))
                    {
                        var str = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var data = _jsonService.DeserializeStream<T>(json);
                        return data;
                    }
                }
            }
        }

        private async Task<T> PutAsync<T>(string resource, HttpContent content, CancellationToken cancellationToken, Dictionary<string, string> additioalHeaders = null)
        {
            using (var client = CreateHttpClient(DefaultHeaders(additioalHeaders)))
            {
                using (var response = await client.PutAsync(GetRequestUrl(BaseUrl, resource), content, cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    //ThrowIfNotSuccess(response);
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(stream))
                    using (var json = new JsonTextReader(reader))
                    {
                        var str = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        return _jsonService.DeserializeStream<T>(json);
                    }
                }
            }
        }

        internal string BuildParametersString(Dictionary<string, string> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder("?");
            bool needAddDivider = false;

            foreach (var item in parameters)
            {
                if (needAddDivider)
                {
                    sb.Append('&');
                }

                var encodedKey = WebUtility.UrlEncode(item.Key);
                var encodedVal = WebUtility.UrlEncode(item.Value);
                sb.Append($"{encodedKey}={encodedVal}");

                needAddDivider = true;
            }

            return sb.ToString();
        }

        internal string GetRequestUrl(string host, string resource, Dictionary<string, string> parameters = null)
        {
            string paramsStr = BuildParametersString(parameters);
            var ret = $"{host}{resource}{paramsStr}";
            return ret;
        }

        private Dictionary<string, string> DefaultHeaders(Dictionary<string, string> additioalHeaders = null)
        {
            var defheaders = new Dictionary<string, string>();
            defheaders["User-Agent"] = "Mobile";
            defheaders["Accept"] = "application/json";

            if (additioalHeaders != null)
            {
                foreach (var kv in additioalHeaders)
                {
                    defheaders[kv.Key] = kv.Value;
                }
            }

            return defheaders;
        }

        private HttpClient CreateHttpClient(Dictionary<string, string> headerParams = null)
        {
            var httpClient = new HttpClient();

            if (headerParams != null)
            {
                foreach (var headerParam in headerParams)
                {
                    httpClient.DefaultRequestHeaders.Add(headerParam.Key, headerParam.Value);
                }
            }

            return httpClient;
        }

        #endregion
    }
}
