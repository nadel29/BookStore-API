using Blazored.LocalStorage;
using BookStore_UI.Contracts;
using BookStore_UI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BookStore_UI.Services
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        //This will handle all the connections to the API and any url requests
        private readonly IHttpClientFactory _client;
        private readonly ILocalStorageService _localStorage;

        public BaseRepository(IHttpClientFactory client,
            ILocalStorageService localStorage)
        {
            _client = client;
            _localStorage = localStorage;
        }

        public async Task<bool> Create(string url, T entity)
        {
            //Use try and catch because localstorage might give you an error
            //---------------------------------------------------------------

            var request = new HttpRequestMessage(HttpMethod.Post, url);

            if(entity == null)
            {
                return false;
            }

            request.Content = new StringContent(JsonConvert.SerializeObject(entity), Encoding.UTF8, "application/json");
            var client = _client.CreateClient();
            client.DefaultRequestHeaders.Authorization =
           new AuthenticationHeaderValue("bearer", await GetBearerToken());

            HttpResponseMessage response = await client.SendAsync(request);
            if(response.StatusCode == HttpStatusCode.Created)
            {
                return true;
            }
            return false;             
        }

        public async Task<bool> Delete(string url, int id)
        {
            if(id<1)
            {
                return false;
            }
            var request = new HttpRequestMessage(HttpMethod.Delete, url + id);
            var client = _client.CreateClient();
            client.DefaultRequestHeaders.Authorization =
           new AuthenticationHeaderValue("bearer", await GetBearerToken());

            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return true;
            }
            return false;

        }

        public async Task<T> Get(string url, int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url + id);                     
          
            var client = _client.CreateClient();
            client.DefaultRequestHeaders.Authorization =
           new AuthenticationHeaderValue("bearer", await GetBearerToken());

            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(content);

            }
            return null;
        }

        public async Task<IList<T>> Get(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var client = _client.CreateClient();
            client.DefaultRequestHeaders.Authorization =
             new AuthenticationHeaderValue("bearer", await GetBearerToken());

            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();                
                return JsonConvert.DeserializeObject<IList<T>>(content);

            }
            return null;
        }

        public async Task<bool> Update(string url, T entity, int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, url+id);

            if (entity == null)
            {
                return false;
            }

            request.Content = new StringContent(JsonConvert.SerializeObject(entity), Encoding.UTF8, "application/json");
            var client = _client.CreateClient();
            client.DefaultRequestHeaders.Authorization =
           new AuthenticationHeaderValue("bearer", await GetBearerToken());

            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return true;
            }
            return false;
        }

        private async Task<string> GetBearerToken()
        {
            return await _localStorage.GetItemAsync<String>("authToken");
        }

    }
}
