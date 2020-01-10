using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LibraryData.Models
{
   public class Hold
    {
        // رزرو کردن کتاب. یعنی فلان کتاب توسط فلان شخص  در فلان تاریخ رزرو شده که هر موقع برگردونده شد به کتابخانه به آن شخص داده شود
        public int Id { get; set; }
        public virtual LibraryAsset LibraryAsset { get; set; }
        public virtual LibraryCard LibraryCard { get; set; }
        public DateTime HoldPlaced { get; set; }
    }
}
