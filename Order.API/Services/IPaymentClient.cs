
using System.Threading.Tasks;
using Order.API.DTOs;


namespace Order.API.Services
{
    public interface IPaymentClient
    {
        //Task<PaymentResponse> ProcessPayment(
        //    PaymentRequest request);

        Task<PaymentResponse> ProcessPayment(
            PaymentRequest request);


    }
}