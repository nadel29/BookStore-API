using Blazored.LocalStorage;
using BookStore_UI.Contracts;
using BookStore_UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BookStore_UI.Services
{
    //AuthorRepository inherits from BaseRepository with its generice methods \
    //as well as IAuthorRepository with its strongly typed methods specidfic to the author only
    public class AuthorRepository : BaseRepository<Author> , IAuthorRepository
    {
        private readonly IHttpClientFactory _client;
        private readonly ILocalStorageService _localStorage;

        //you must pass the injected value into the Base class
        public AuthorRepository(IHttpClientFactory client, ILocalStorageService localStorage) : base(client, localStorage)
        {
            _client = client;
            _localStorage = localStorage;
        }
    }
}
