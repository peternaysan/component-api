using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gac.Logistics.Aes.Api.Model
{
    public class AesTransaction
    {
        public string Id { get; set; }
        public long BookingId { get; set; }
        public string InstanceCode { get; set; }

        public string TransactionId { get; set; }

        public Aes AesDetailEntity { get; set; }
    }
}
