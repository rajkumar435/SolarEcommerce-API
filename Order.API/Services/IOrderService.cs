using Order.API.DTOs;
using Order.API.Models;

namespace Order.API.Services
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateOrder(CreateOrderDto dto);

        Task<List<Orders>> GetOrders();
        Task<bool> MarkPaymentSuccessful(
    int orderId);
    }
}