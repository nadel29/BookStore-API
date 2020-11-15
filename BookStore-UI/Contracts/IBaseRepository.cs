using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_UI.Contracts
{
    public interface IBaseRepository<T> where T: class
    {
        Task<T> Get(string url, int id);//url is the endpoint, id is for single record retrieval
        Task<IList<T>> Get(string url);
        Task<bool> Create(string url, T entity);
        Task<bool> Update(string url, T entity);
        Task<bool> Delete(string url, int id);
    }
}
