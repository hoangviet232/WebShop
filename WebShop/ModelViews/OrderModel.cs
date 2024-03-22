using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace WebShop.ModelViews
{
    public class OrderModel : Controller
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public List<CartItem> CartItems { get; set; }
        public decimal TotalAmount { get; set; }
    }
}