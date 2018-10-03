using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gac.Logistics.Aes.Api.Model
{
    public class AesTransaction
    {
        public string TransactionId { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public Aes AesDetailEntity { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
