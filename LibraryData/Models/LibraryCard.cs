using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryData.Models
{
   public class LibraryCard
    {
        public int Id { get; set; }
        public decimal Fees { get; set; }
        public DateTime Created { get; set; }
        public virtual IEnumerable<CheckOut> Checkouts { get; set; }

    }
}
