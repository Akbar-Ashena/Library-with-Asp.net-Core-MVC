using LibraryData;
using LibraryData.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace LibraryServices
{
    public class LibraryAssetService : ILibraryAsset
    {
        private readonly LibraryContext _Context;
        public LibraryAssetService(LibraryContext context)
        {
            _Context = context;
        }
        public void Add(LibraryAsset newAsset)
        {
            _Context.Add(newAsset);
            _Context.SaveChanges();
        }

        public IEnumerable<LibraryAsset> GetAll()
        {            
            return _Context.LibraryAssets.ToList();
            //.Include(asset => asset.Status);
            //.Include(asset => asset.Location);           
        }



        public LibraryAsset GetById(int id)
        {
            return _Context.LibraryAssets
                .Include(asset => asset.Status)
                .Include(asset => asset.Location)
                .FirstOrDefault(asset => asset.Id == id);
        }

        public LibraryBranch GetCurrentLocation(int id)
        {
            return GetById(id).Location;
            //return _Context.LibraryAssets.FirstOrDefault(asset => asset.Id == id).Location;
        }

        public string GetDewayIndex(int id)
        {
            if (_Context.Books.Any(book => book.Id == id))
            {
                return _Context.Books.FirstOrDefault(book => book.Id == id).DeweyIndex;
            }
            //var isBook = _Context.LibraryAssets.OfType<Book>().Where(c => c.Id == id).Any();
            else return "";

        }

        public string GetIsbn(int id)
        {
            if (_Context.Books.Any(c => c.Id == id))
            {
                return _Context.Books.FirstOrDefault(c => c.Id == id).ISBN;
            }
            else return "";

        }

        public string GetTitle(int id)
        {
            if (_Context.Books.Any(c => c.Id == id))
            {
                return _Context.Books.FirstOrDefault(c => c.Id == id).Title;
            }
            else return "";
        }
        public string GetAuthorOrDirector(int id)
        {
            var isBook = _Context.LibraryAssets.OfType<Book>()
                .Where(c => c.Id == id).Any();
            var isVideo = _Context.LibraryAssets.OfType<Book>()
                .Where(c => c.Id == id).Any();
            return isBook ? _Context.Books.FirstOrDefault(book => book.Id == id).Author :
                _Context.Videos.FirstOrDefault(book => book.Id == id).Director
            ?? "Unknown";
        }
        public string GetType(int id)
        {
            var book = _Context.LibraryAssets.OfType<Book>().Where(b => b.Id == id);
            return book.Any() ? "Book" : "Video";
        }
    }
}
