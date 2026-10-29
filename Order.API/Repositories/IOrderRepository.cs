using Order.API.Models;

namespace Order.API.Repositories
{
    public interface IOrderRepository
    {  
        Task<Orders?> GetById(int orderId);

        Task Update(Orders order);

        Task SaveChanges();
    }
}