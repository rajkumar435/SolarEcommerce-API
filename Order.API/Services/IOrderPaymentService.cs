namespace Order.API.Services
{
    public interface IOrderPaymentService
    {
        Task<bool> MarkPaymentSuccessful(
            int orderId);
    }
}