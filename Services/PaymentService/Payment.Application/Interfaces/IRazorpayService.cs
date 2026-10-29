using Payment.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment.Application.Interfaces
{
    public interface IRazorpayService
    {
        Task<RazorpayOrderResponse> CreateOrder(
            CreateRazorpayOrderRequest request);

        bool VerifyPayment(
            VerifyPaymentRequest request);
    }
}
