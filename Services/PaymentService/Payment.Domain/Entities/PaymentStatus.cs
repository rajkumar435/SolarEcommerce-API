using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment.Domain.Entities
{
    public enum PaymentStatus
    {
        Pending = 1,
        Processing = 2,
        Successful = 3,
        Failed = 4,
        Cancelled = 5,
        Refunded = 6
    }
}