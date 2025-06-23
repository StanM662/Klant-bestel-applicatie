using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KE03_INTDEV_SE_1_Base.Pages
{
    public class CheckoutModel : PageModel
    {
        private readonly IProductRepository _productRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IOrderRepository _orderRepository;
        public List<Product> Cart { get; set; } = new List<Product>();
        public Customer customer = new Customer();
        public CheckoutModel(IProductRepository productRepository, ICustomerRepository customerRepository, IOrderRepository orderRepository)
        {
            _productRepository = productRepository;
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
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
            
            var customerIdString = HttpContext.Session.GetString("customerId");
            if (int.TryParse(customerIdString, out int customerId))
            {
                customer = _customerRepository.GetCustomerById(customerId);
            }
        }

        public IActionResult OnPost(string action)
        {
            LoadCart();
            switch (action)
            {
                case "Checkout":
                    var order = LoadOrder();
                    _orderRepository.AddOrder(order);
                    Cart.Clear();
                    SaveCart();
                    return RedirectToPage("Products");

                default:
                    ModelState.AddModelError(string.Empty, "Ongeldige actie.");
                    break;
            }
            return Page();
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

        private Order LoadOrder()
        {
            var order = new Order();
            LoadCart();

            foreach (var product in Cart)
            {
                var _product = _productRepository.GetProductById(product.Id);
                if (_product != null)
                {

                    if (order.Products.Contains(_product))
                    {
                        _product.Quantity += 1;
                        order.Products.Remove(_product);
                    }
                    else { _product.Quantity = 1; }
                    order.Products.Add(_product);
                }
            }

            order.OrderDate = DateTime.Now;

            var customerIdString = HttpContext.Session.GetString("customerId");
            if (int.TryParse(customerIdString, out int customerId))
            {
                order.CustomerId = customerId;
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Ongeldig klant-ID.");
                order.CustomerId = 0;
            }

            order.Customer = _customerRepository.GetCustomerById(order.CustomerId);
            return order;
        }


    }
}
