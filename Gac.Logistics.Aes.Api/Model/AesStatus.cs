using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gac.Logistics.Aes.Api.Model
{
    public static class AesStatus
    {
        public const string PENDING = "PENDING";
        public const string DRAFT = "DRAFT";
        public const string GETSAPPROVED = "GETS APPROVED";
        public const string GETSREJECTED = "GETS REJECTED";
        public const string CUSTOMSAPPROVED = "CUSTOMS APPROVED";
        public const string CUSTOMSREJECTED = "CUSTOMS REJECTED";
        public const string SUBMITTED = "SUBMITTED";
        public const string CANCELLED = "CANCELLED";
    }

    public static class GetsStatus
    {
        public const string SUCCESS = "SUCCESS";
        public const string FAIL = "FAIL";
    }
}
