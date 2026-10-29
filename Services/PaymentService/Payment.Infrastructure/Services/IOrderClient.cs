using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment.Infrastructure.Services
{
    public interface IOrderClient
    {
        Task<bool> MarkPaymentSuccessful(int orderId);
    }
}
