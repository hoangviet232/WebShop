using Microsoft.AspNetCore.Mvc;
using System.Linq;
using WebShop.Models;

namespace WebShop.Controllers
{
    public class GetFilteredProductsController : Controller
    {
        private readonly dbMarketsContext _context;
        private const int PageSize = 10; // Adjust the value as needed

        public GetFilteredProductsController(dbMarketsContext context)
        {
            _context = context;
        }

        [Route("GetFilteredProducts/loadproducts")]
        [HttpPost]
        public IActionResult LoadProducts(string priceRange, int page)
        {
            // Parse the price range values from the string (lower and upper bounds)
            var priceBounds = priceRange.Split(':');
            var lowerBound = float.Parse(priceBounds[0]);
            var upperBound = float.Parse(priceBounds[1]);

            // Fetch products based on price range and page using your data context
            var products = _context.Products
                .Where(p => p.Price >= lowerBound && p.Price <= upperBound)
                .Skip((page - 1) * PageSize) // Define PageSize based on your requirements
                .Take(PageSize)
                .ToList();

            return PartialView("_ListProductPartialView", products);
        }
    }
}