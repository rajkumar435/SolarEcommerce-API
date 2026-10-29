using Payment.Application.DTOs;
using Payment.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponse> CreatePayment(
            CreatePaymentRequest request);

        Task<PaymentResponse?> GetPaymentById(
            int id);

        Task<PaymentResponse?> GetPaymentByOrderId(
            int orderId);
        Task<PaymentResponse>
              RecordSuccessfulRazorpayPayment(
                  VerifyPaymentRequest request);

    }
}