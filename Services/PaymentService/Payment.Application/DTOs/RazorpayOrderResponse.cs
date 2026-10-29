using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment.Application.DTOs
{
    public class RazorpayOrderResponse
    {
        public string RazorpayOrderId { get; set; }

        public int Amount { get; set; }

        public string Currency { get; set; }

        public string KeyId { get; set; }
    }
}
