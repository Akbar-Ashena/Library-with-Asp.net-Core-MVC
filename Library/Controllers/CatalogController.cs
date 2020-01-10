using Library.Models.Catalog;
using Library.Models.CheckoutModels;
using LibraryData;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Library.Controllers
{

    public class CatalogController : Controller
    {
        private ILibraryAsset _assets;
        private ICheckout _checkout;

        public CatalogController(ILibraryAsset assets, ICheckout checkout)
        {
            _assets = assets;
            _checkout = checkout;
        }

        public IActionResult Index()
        {
            var assetModels = _assets.GetAll();
            AssetIndexListingModel dd = new AssetIndexListingModel();

            var listingResult = assetModels.Select(result => new AssetIndexListingModel
            {
                Id = result.Id,
                ImageUrl = "img/13.jpg",
                AuthorOrDirector = _assets.GetAuthorOrDirector(result.Id),
                Title = _assets.GetTitle(result.Id),
                Type = _assets.GetType(result.Id),
                DeweyCallNumber = _assets.GetDewayIndex(result.Id)

            });


            var model = new AssetIndexModel()
            {
                Assets = listingResult
            };
            return View(model);
        }

        public IActionResult Detail(int id)
        {
            var asset = _assets.GetById(id);
            var currenthold = _checkout.GetCurrentHolds(id)
                .Select(c => new AssetHoldModel
                {
                    HoldPlaced = _checkout.GetCurrentHoldPlaced(c.Id).ToString("d"),
                    PatronName = _checkout.GetCurrentHoldPatronName(c.Id)
                });

            var model = new AssetDetailModel()

            {
                AssetId = id,
                Title = asset.Title,
                Year = asset.Year,
                Status = asset.Status.Name,
                ImageUrl = "img/13.jpg", //asset.ImageUrl,
                AuthorOrDirector = _assets.GetAuthorOrDirector(id),
                CurrentLocation = _assets.GetCurrentLocation(id).Name,
                DeweyCallNumber = _assets.GetDewayIndex(id),
                CheckOutHistory = _checkout.GetCheckoutHistory(id),
                ISBN = _assets.GetIsbn(id),
                LatestCheckout = _checkout.GetLatestCheckout(id),
                PatronName = _checkout.GetCurrentCheckoutPatron(id),
                CurrentHolds = currenthold,
                Cost = asset.Cost


            };
            return View(model);
        }

        public IActionResult Checkout(int id)
        {
            var asset = _assets.GetById(id);
            var model = new CheckoutModel
            {
                AssetId = id,
                ImageUrl = asset.ImageUrl,
                Title = asset.Title,
                LibraryCardId = "",
                IsCheckedOut = _checkout.IsCheckedOut(id)
            };
            return View(model);
        }

        public IActionResult Hold(int id)
        {
            var asset = _assets.GetById(id);
            var model = new CheckoutModel
            {
                AssetId = id,
                ImageUrl = asset.ImageUrl,
                Title = asset.Title,
                LibraryCardId = "",
                IsCheckedOut = _checkout.IsCheckedOut(id),
                HoldCount = _checkout.GetCurrentHolds(id).Count()
            };
            return View(model);
        }
        public IActionResult MarkLost(int assetId)
        {
            _checkout.MarkLost(assetId);
            return RedirectToAction("Detail", new { id = assetId });
        }
        public IActionResult MarkFound(int assetId)
        {
            _checkout.MarkFound(assetId);
            return RedirectToAction("Detail", new { id = assetId });
        }


        [HttpPost]
        public IActionResult PlaceCheckout(int assetId, int libraryCardId)
        {
            _checkout.CheckOutItem(assetId, libraryCardId);
            return RedirectToAction("Detail", new { id = assetId });
        }

        [HttpPost]
        public IActionResult PlaceHold(int assetId, int libraryCardId)
        {
            _checkout.PlaceHold(assetId, libraryCardId);
            return RedirectToAction("Detail", new { id = assetId });
        }
    }
}