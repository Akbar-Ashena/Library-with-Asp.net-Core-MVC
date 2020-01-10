using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LibraryData.Models
{
   public class CheckOutHistory
    {
        public int Id { get; set; }
        [Required]
        // for this item a Foreign key in table will be crated
        public LibraryAsset LibraryAsset { get; set; }

        // for this item a Foreign key in table will be crated
        [Required]
        public LibraryCard LibraryCard { get; set; }
        [Required]
        public DateTime CheckedOut { get; set; }
        public DateTime? CheckedIn { get; set; }
    }
}
