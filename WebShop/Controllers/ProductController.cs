using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using WebShop.Models;

namespace WebShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly textdbMarketsContext _context;

        public ProductController(textdbMarketsContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return View(new List<Product>());
        }

        [Route("shop.html", Name = ("ShopProduct"))]
        public IActionResult Index(int? page)
        {
            try
            {
                var pageNumber = page == null || page <= 0 ? 1 : page.Value;
                var pageSize = 10;
                var lsTinDangs = _context.Products
                    .AsNoTracking()
                    .OrderBy(x => x.DateCreated);
                PagedList<Product> models = new PagedList<Product>(lsTinDangs, pageNumber, pageSize);

                ViewBag.CurrentPage = pageNumber;
                return View(models);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [Route("danhmuc/{Alias}", Name = ("ListProduct"))]
        public IActionResult List(string Alias, int page = 1)
        {
            try
            {
                var pageSize = 10;
                var danhmuc = _context.Categories.AsNoTracking().SingleOrDefault(x => x.Alias == Alias);

                var lsTinDangs = _context.Products
                    .AsNoTracking()
                    .Where(x => x.CatId == danhmuc.CatId)
                    .OrderByDescending(x => x.DateCreated);
                PagedList<Product> models = new PagedList<Product>(lsTinDangs, page, pageSize);
                ViewBag.CurrentPage = page;
                ViewBag.CurrentCat = danhmuc;
                return View(models);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [Route("/{Alias}-{id}.html", Name = ("ProductDetails"))]
        public IActionResult Details(int id)
        {
            try
            {
                var product = _context.Products.Include(x => x.Cat).FirstOrDefault(x => x.ProductId == id);
                if (product == null)
                {
                    return RedirectToAction("Index");
                }
                var lsProduct = _context.Products
                    .AsNoTracking()
                    .Where(x => x.CatId == product.CatId && x.ProductId != id && x.Active == true)
                    .Take(4)
                    .OrderByDescending(x => x.DateCreated)
                    .ToList();
                ViewBag.SanPham = lsProduct;
                return View(product);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }

  
        [Route("/Product/FilterProducts")]
        [HttpGet]
        public IActionResult FilterProducts(int minPrice, int maxPrice, int sortOption)
        {
            try
            {
                var filteredProductsQuery = _context.Products
                    .AsNoTracking()
                    .Where(x => x.Price >= minPrice && x.Price <= maxPrice);

                // Sắp xếp theo tiêu chí đã chọn
                switch (sortOption)
                {
                    case 1:
                        filteredProductsQuery = filteredProductsQuery.OrderBy(x => x.Price);
                        break;

                    case 2:
                        filteredProductsQuery = filteredProductsQuery.OrderByDescending(x => x.Price);
                        break;

                    case 3:
                        filteredProductsQuery = filteredProductsQuery.OrderByDescending(x => x.DateCreated);
                        break;

                    case 4:
                        filteredProductsQuery = filteredProductsQuery.OrderBy(x => x.ProductName);
                        break;
                }

                var filteredProducts = filteredProductsQuery.ToList();
                return PartialView("_FilteredProductsPartial", filteredProducts);
            }
            catch
            {
                // Handle any exceptions that might occur during filtering
                return BadRequest("An error occurred while filtering products.");
            }
        }
    }
}