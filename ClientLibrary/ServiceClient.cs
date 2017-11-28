// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Cognitive Services (formerly Project Oxford): https://www.microsoft.com/cognitive-services
// 
// Microsoft Cognitive Services (formerly Project Oxford) GitHub:
// https://github.com/Microsoft/Cognitive-Common-Windows
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.ProjectOxford.Common
{
    public abstract class ServiceClient : IDisposable
    {
        protected class UrlReqeust
        {
            public string url { get; set; }
        }

        public class WrappedClientError
        {
            public ClientError Error { get; set; }
        }

        #region private/protected members

        /// <summary>
        /// The default resolver.
        /// </summary>
        protected static CamelCasePropertyNamesContractResolver s_defaultResolver = new CamelCasePropertyNamesContractResolver();

        protected static JsonSerializerSettings s_settings = new JsonSerializerSettings()
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = s_defaultResolver
        };

        private readonly HttpClient _httpClient;

        private readonly bool _ownHttpClient;

        private bool _disposed;

        /// <summary>
        /// Default constructor
        /// </summary>
        protected ServiceClient() : this(new HttpClient(), true)
        {
        }

        /// <summary>
        /// Test constructor; use to inject mock clients.
        /// </summary>
        /// <param name="httpClient">Custom HttpClient, for testing.</param>
        protected ServiceClient(HttpClient httpClient) : this(httpClient, false)
        {
        }

        /// <summary>
        /// Common constructor for default and test.
        /// </summary>
        /// <param name="httpClient">Custom HttpClient, for testing.</param>
        /// <param name="ownHttpClient">True if this object owns the HttpClient, false if the caller owns it.</param>
        private ServiceClient(HttpClient httpClient, bool ownHttpClient)
        {
            _httpClient = httpClient;
            _ownHttpClient = ownHttpClient;
        }

        /// <summary>
        /// IDisposable.Dispose implementation.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing && _ownHttpClient)
            {
                _httpClient.Dispose();
            }

            _disposed = true;
        }

        ~ServiceClient()
        {
            Dispose(false);
        }

        /// <summary>
        /// Root of the API URL.  Must not end in slash.
        /// </summary>
        protected string ApiRoot { get; set; }

        /// <summary>
        /// Header name for authorization.
        /// </summary>
        protected string AuthKey { get; set; }

        /// <summary>
        /// Header value for authorization.
        /// </summary>
        protected string AuthValue { get; set; }
        #endregion

        #region the JSON client
        /// <summary>
        /// Helper method executing a GET REST request.
        /// </summary>
        /// <typeparam name="TRequest">Type of request.</typeparam>
        /// <typeparam name="TResponse">Type of response.</typeparam>
        /// <param name="apiUrl">API URL relative to the apiRoot</param>
        /// <param name="requestBody">Content of the HTTP request.</param>
        /// <param name="cancellationToken">Async cancellation token</param>
        /// <returns>TResponse</returns>
        /// <exception cref="ClientException">Service exception</exception>
        protected Task<TResponse> PostAsync<TRequest, TResponse>(string apiUrl, TRequest requestBody, CancellationToken cancellationToken)
        {
            return SendAsync<TRequest, TResponse>(HttpMethod.Post, apiUrl, requestBody, cancellationToken);
        }

        /// <summary>
        /// Helper method executing a POST REST request.
        /// </summary>
        /// <typeparam name="TRequest">Type of request.</typeparam>
        /// <typeparam name="TResponse">Type of response.</typeparam>
        /// <param name="apiUrl">API URL relative to the apiRoot</param>
        /// <param name="requestBody">Content of the HTTP request.</param>
        /// <param name="cancellationToken">Async cancellation token</param>
        /// <returns>TResponse</returns>
        /// <exception cref="ClientException">Service exception</exception>
        protected Task<TResponse> GetAsync<TRequest, TResponse>(string apiUrl, TRequest requestBody, CancellationToken cancellationToken)
        {
            return SendAsync<TRequest, TResponse>(HttpMethod.Get, apiUrl, requestBody, cancellationToken);
        }

        /// <summary>
        /// Helper method executing a REST request.
        /// </summary>
        /// <typeparam name="TRequest">Type of request.</typeparam>
        /// <typeparam name="TResponse">Type of response.</typeparam>
        /// <param name="method">HTTP method</param>
        /// <param name="apiUrl">API URL, generally relative to the ApiRoot</param>
        /// <param name="requestBody">Content of the HTTP request</param>
        /// <param name="cancellationToken">Async cancellation token</param>
        /// <returns>TResponse</returns>
        /// <exception cref="ClientException">Service exception</exception>
        protected Task<TResponse> SendAsync<TRequest, TResponse>(HttpMethod method, string apiUrl, TRequest requestBody, CancellationToken cancellationToken)
        {
            bool urlIsRelative = System.Uri.IsWellFormedUriString(apiUrl, System.UriKind.Relative);

            string requestUri = urlIsRelative ? ApiRoot + apiUrl : apiUrl;
            var request = new HttpRequestMessage(method, requestUri);
            request.Headers.Add(AuthKey, AuthValue);

            if (requestBody != null)
            {
                if (requestBody is Stream)
                {
                    request.Content = new StreamContent(requestBody as Stream);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                }
                else
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(requestBody, s_settings), Encoding.UTF8, "application/json");
                }
            }

            var task = _httpClient.SendAsync(request, cancellationToken)
                .ContinueWith(t => GetContent(t.Result)
                    .ContinueWith(u => GetResponse<TResponse>(t.Result, u.Result)));

            return task.Result;
        }

        /// <summary>
        /// Task to get the HTTP response string.
        /// </summary>
        private Task<string> GetContent(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                if (response.Content != null)
                {
                    return response.Content.ReadAsStringAsync();
                }
            }
            else if (response.Content != null && response.Content.Headers.ContentType.MediaType.Contains("application/json"))
            {
                return response.Content.ReadAsStringAsync();
            }
            return Task.FromResult("");
        }

        /// <summary>
        /// Task to construct the JSON object from the HTTP response.
        /// </summary>
        private TResponse GetResponse<TResponse>(HttpResponseMessage response, string responseContent)
        {
            if (response.IsSuccessStatusCode)
            {
                if (!string.IsNullOrWhiteSpace(responseContent))
                {
                    return JsonConvert.DeserializeObject<TResponse>(responseContent, s_settings);
                }
            }
            else
            {
                if (response.Content != null && response.Content.Headers.ContentType.MediaType.Contains("application/json"))
                {
                    var wrappedClientError = JsonConvert.DeserializeObject<WrappedClientError>(responseContent);
                    if (wrappedClientError?.Error != null)
                    {
                        throw new ClientException(wrappedClientError.Error, response.StatusCode);
                    }
                }

                response.EnsureSuccessStatusCode();
            }
            return default(TResponse);
        }
        #endregion
    }
}
