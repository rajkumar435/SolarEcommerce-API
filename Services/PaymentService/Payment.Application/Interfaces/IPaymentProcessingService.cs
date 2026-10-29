using Payment.Application.DTOs;

namespace Payment.Application.Interfaces
{
    public interface IPaymentProcessingService
    {
        Task<bool> ProcessSuccessfulRazorpayPayment(
            VerifyPaymentRequest request);
    }
}