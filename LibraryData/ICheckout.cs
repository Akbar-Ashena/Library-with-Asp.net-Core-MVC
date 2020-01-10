using LibraryData.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryData
{
    public interface ICheckout
    {
        void Add(CheckOut newCheckout);

        IEnumerable<CheckOut> GetAll();
        IEnumerable<CheckOutHistory> GetCheckoutHistory(int id);
        IEnumerable<Hold> GetCurrentHolds(int id);

        CheckOut GetById(int CheckoutId);
        CheckOut GetLatestCheckout(int assetId);
        string GetCurrentCheckoutPatron(int assetId);
        string GetCurrentHoldPatronName(int id);
        DateTime GetCurrentHoldPlaced(int id);
        bool IsCheckedOut(int id);
        void PlaceHold(int assetId, int LibraryCard);

        void MarkLost(int assetId);
        void MarkFound(int assetId);
        void CheckOutItem(int assetId, int LibraryCardId);
        void CheckInItem(int assetId, int LibraryCardId);
    }

}
