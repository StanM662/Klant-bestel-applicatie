using System.Text.Json;
using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KE03_INTDEV_SE_1_Base.Pages
{
    public class CartModel : PageModel
    {
        private readonly IProductRepository _productRepository;
        public List<Product> Cart { get; set; } = new List<Product>();

        public CartModel(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }


        public void OnGet()
        {
            var jsonCart = HttpContext.Session.GetString("Cart");
            if (!string.IsNullOrEmpty(jsonCart))
            {
                Console.WriteLine("Cart JSON: " + jsonCart);
                Cart = JsonSerializer.Deserialize<List<Product>>(jsonCart) ?? new List<Product>();
            }
            else
            {
                Console.WriteLine("Cart not found in session.");
            }
        }

        public IActionResult OnPost(string action)
        {
            LoadCart();
            switch (action)
            {
                case "RemoveFromCart":
                    var productId = Request.Form["productId"];
                    if (int.TryParse(productId, out int id))
                    {
                        var product = _productRepository.GetProductById(id);
                        if (product != null)
                        {
                            RemoveProductFromCart(product);
                        }
                    }
                    break;

                default:
                    ModelState.AddModelError(string.Empty, "Ongeldige actie.");
                    break;
            }

            SaveCart();
            return Page();
        }

        private void RemoveProductFromCart(Product product)
        {
            var itemToRemove = Cart.FirstOrDefault(p => p.Id == product.Id);
            if (itemToRemove != null)
                Cart.Remove(itemToRemove);

            SaveCart();
        }

        private void LoadCart()
        {
            var jsonCart = HttpContext.Session.GetString("Cart");
            if (!string.IsNullOrEmpty(jsonCart))
            {
                Cart = JsonSerializer.Deserialize<List<Product>>(jsonCart) ?? new List<Product>();
            }
            else
            {
                Cart = new List<Product>();
            }
        }

        private void SaveCart()
        {
            var jsonCart = JsonSerializer.Serialize(Cart);
            HttpContext.Session.SetString("Cart", jsonCart);
        }
    }

}
