using Payment.Application.DTOs;
using Payment.Domain.Entities;

namespace Payment.Application.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payments?> GetById(int id);

        Task<Payments?> GetByOrderId(int orderId);

        Task Add(Payments payment);

        Task<Payments> RecordSuccessfulRazorpayPayment(
    VerifyPaymentRequest request);

        Task SaveChanges();
    }
}