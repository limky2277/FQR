using System;
using System.Collections.Generic;

namespace TicketBOT.Services.Interfaces
{
    public interface IGenericService<T>
    {
        List<T> Get();
        T GetById(Guid id);
        T Get(string id);
        T Create(T obj);
        void Update(Guid id, T obj);
        void Remove(T obj);
        void Remove(Guid id);
    }
}
