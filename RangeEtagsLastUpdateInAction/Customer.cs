using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RangeEtagsLastUpdateInAction
{
    public class Customer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public byte[] Avatar { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
