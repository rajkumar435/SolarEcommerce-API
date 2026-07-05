using Order.API.DTOs;
using Order.API.Models;

namespace Order.API.Services
{
    public interface IOrderService
    {
        Task<Orders> CreateOrder(CreateOrderDto dto);

        Task<List<Orders>> GetOrders();
    }
}