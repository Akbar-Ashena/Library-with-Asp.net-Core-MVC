using LibraryData;
using LibraryData.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryServices
{
    public class CheckoutService : ICheckout
    {
        private readonly LibraryContext _Context;
        DateTime now = DateTime.Now;
        public CheckoutService(LibraryContext context)
        {
            _Context = context;
        }
        public void Add(CheckOut newCheckout)
        {
            _Context.Add(newCheckout);
            _Context.SaveChanges();
        }

        public IEnumerable<CheckOut> GetAll()
        {
            return _Context.CheckOuts;
        }

        public CheckOut GetById(int CheckoutId)
        {
            return _Context.CheckOuts.FirstOrDefault(c => c.Id == CheckoutId);
        }

        public IEnumerable<CheckOutHistory> GetCheckoutHistory(int id)
        {
            return _Context.CheckOutHistories
                .Include(h => h.LibraryAsset)
                .Include(c => c.LibraryCard)
                .Where(chk => chk.LibraryAsset.Id == id);
        }

      
        public IEnumerable<Hold> GetCurrentHolds(int id)
        {
            return _Context.Holds
                .Include(h => h.LibraryAsset)
                .Where(c => c.LibraryAsset.Id == id);
        }

        public void MarkFound(int assetId)
        {
            UpdateAssetStatus(assetId, "Available");
            RemoveExistingCheckouts(assetId);
            CloseExistingCheckoutHistory(assetId);
            _Context.SaveChanges();
        }
        void UpdateAssetStatus(int assetId, string newStatus)
        {
            var item = _Context.LibraryAssets.FirstOrDefault(c => c.Id == assetId);
            _Context.Update(item);
            item.Status = _Context.Statuses.FirstOrDefault(c => c.Name == newStatus);
        }
        void RemoveExistingCheckouts(int assetId)
        {
            var checkout = _Context.CheckOuts.FirstOrDefault(c => c.LibraryAsset.Id == assetId);
            if (checkout != null)
            {
                _Context.Remove(checkout);
            }
        }
        void CloseExistingCheckoutHistory(int assetId)
        {
            var checkOutHistory = _Context.CheckOutHistories.FirstOrDefault(c => c.LibraryAsset.Id == assetId);
            if (checkOutHistory != null)
            {
                _Context.Update(checkOutHistory);
                checkOutHistory.CheckedIn = now;
            }
        }
        public void MarkLost(int assetId)
        {
            var item = _Context.LibraryAssets.FirstOrDefault(c => c.Id == assetId);
            _Context.Update(item);
            item.Status = _Context.Statuses.FirstOrDefault(c => c.Name == "Lost");
            _Context.SaveChanges();
        }

        public void PlaceHold(int assetId, int LibraryCard)
        {
            var asset = _Context.LibraryAssets
                .Include(a=>a.Status)
                .FirstOrDefault(a => a.Id == assetId);
            var card = _Context.LibraryCards
                .FirstOrDefault(l => l.Id == LibraryCard);
            if (asset.Status.Name == "Available")
            {
                UpdateAssetStatus(assetId, "On Hold");
            }
            var hold = new Hold
            {
                HoldPlaced = now,
                LibraryAsset = asset,
                LibraryCard = card
            };
            _Context.Add(hold);
            _Context.SaveChanges();
        }
        public void CheckInItem(int assetId, int LibraryCardId)
        {
            var item = _Context.LibraryAssets
                 .FirstOrDefault(a => a.Id == assetId);
            _Context.Update(item);
            //remove any existing checkouts on the item
            RemoveExistingCheckouts(assetId);
            //Close any existing checkout history
            CloseExistingCheckoutHistory(assetId);
            //look for existing holds on the item
            var currentHolds = _Context.Holds
                .Include(c => c.LibraryAsset)
                .Include(c => c.LibraryCard)
                .Where(h => h.LibraryAsset.Id == assetId);


            //if there are holds, checkout the item to the librarycard with the earliest hold.
            if (currentHolds.Any())
            {
                CheckoutToEarliestHold(assetId, currentHolds);
            }
            //otherwise update the item status to available
            UpdateAssetStatus(assetId, "Available");
            _Context.SaveChanges();
        }

        private void CheckoutToEarliestHold(int assetId, IQueryable<Hold> currentHolds)
        {
            var earliestHold = currentHolds
                .OrderBy(holds => holds.HoldPlaced)
                .FirstOrDefault();

            var card = earliestHold.LibraryCard;
            _Context.Remove(earliestHold);
            _Context.SaveChanges();
            CheckOutItem(assetId, card.Id);
        }

        public void CheckOutItem(int assetId, int LibraryCardId)
        {
            if (IsCheckedOut(assetId))
            {
                return;
                //Add logic here to handle feedback to the user!
            }
            var item = _Context.LibraryAssets
                .FirstOrDefault(a => a.Id == assetId);
            UpdateAssetStatus(assetId, "Checked Out");
            var libraryCard = _Context.LibraryCards
                .Include(card => card.Checkouts)
                .FirstOrDefault(card => card.Id == LibraryCardId);
            var checkout = new CheckOut
            {
                LibraryAsset = item,
                LibraryCard = libraryCard,
                Since = now,
                Until = GetDefaultCheckoutTime(now)
            };
            _Context.Add(checkout);
            var checkoutHistory = new CheckOutHistory
            {
                CheckedOut = now,
                LibraryAsset = item,
                LibraryCard = libraryCard
            };
            _Context.Add(checkoutHistory);
            _Context.SaveChanges();
        }

        private DateTime GetDefaultCheckoutTime(object now)
        {
            return DateTime.Now.AddDays(30);
        }

        public bool IsCheckedOut(int assetId)
        {
            return _Context.CheckOuts
                .Where(c => c.LibraryAsset.Id == assetId)
                .Any();

        }

      

        public DateTime GetCurrentHoldPlaced(int id)
        {
            return _Context.Holds.Include(c => c.LibraryAsset)
                .Include(c => c.LibraryCard)
                .FirstOrDefault(c => c.Id == id).HoldPlaced;
        }

        public CheckOut GetLatestCheckout(int assetId)
        {
            return _Context.CheckOuts
                .Where(c => c.LibraryAsset.Id == assetId)
                .OrderByDescending(c => c.Since)
                .FirstOrDefault();
        }

        public string GetCurrentCheckoutPatron(int assetId)
        {
            var checkout = GetCheckoutByAssetId(assetId);
            if (checkout==null)
            {
                return "";
            }
            var cardId = checkout.LibraryCard.Id;
            var patron = _Context.Patrons
                .Include(c => c.LibraryCard)
                .Where(c => c.LibraryCard.Id == assetId).FirstOrDefault();
            return patron.FirstName + " " + patron.LastName;
                
        }

        CheckOut GetCheckoutByAssetId(int assetId)
        {
            return _Context.CheckOuts
                .Include(c => c.LibraryAsset)
                .Include(c => c.LibraryCard)
                .FirstOrDefault(c => c.LibraryAsset.Id == assetId);
        }

        string ICheckout.GetCurrentHoldPatronName(int id)
        {
            var hold = _Context.Holds
                .Include(c => c.LibraryCard)
                .Include(c=>c.LibraryAsset)
                .FirstOrDefault(c=>c.Id==id);

            var cardId = hold?.LibraryCard.Id;
            var patron = _Context.Patrons.FirstOrDefault(c => c.LibraryCard.Id == cardId);
            return patron?.FirstName + " " + patron?.LastName;
        }

       
    }
}
