using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace KE03_INTDEV_SE_1_Base.Pages
{
    public class IndexModel : PageModel
    {
        // Logger voor debuggen
        private readonly ILogger<IndexModel> _logger;

        // Bools
        public bool showOrderHistory = false;
        public bool confirmOrder = false;
        public bool confirmOrderbtn = false;

        // Repositories
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        
        // Lijsten
        public List<string> orderHistory { get; set; } = new List<string>();

        public List<Product> Products = new List<Product>();

        // Add the missing repository field for _productRepository  
        private readonly IProductRepository _productRepository;
        public IList<Customer> Customers { get; set; }

        // Constructor voor IndexModel
        public IndexModel(ILogger<IndexModel> logger, ICustomerRepository customerRepository, IOrderRepository orderRepository, IProductRepository productRepository)
        {
            _logger = logger;
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            Customers = new List<Customer>();
        }

        // OnGet methode (dus wanneer de pagina geladen wordt)
        public void OnGet()
        {
            // Haalt de producten lijst op uit de sessie
            var productsJson = HttpContext.Session.GetString("Products");
            Products = productsJson != null
                ? JsonSerializer.Deserialize<List<Product>>(productsJson) ?? new List<Product>()
                : new List<Product>();


            Customers = _customerRepository.GetAllCustomers().ToList();
            _logger.LogInformation($"getting all {Customers.Count} customers");
        }

        // OnPost methode (dus wanneer er een actie uitgevoerd wordt)
        public IActionResult OnPost()
        {
            // Haalt de producten lijst op uit de sessie
            var productsJson = HttpContext.Session.GetString("Products");
            Products = productsJson != null
                ? JsonSerializer.Deserialize<List<Product>>(productsJson) ?? new List<Product>()
                : new List<Product>();


            Product product = new Product();
            var action = Request.Form["action"];
            product.Name = Request.Form["productId"];
            try
            {
                product.Price = float.Parse(Request.Form["prijs"]);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fout bij het ophalen van de prijs: {ex.Message}");
                product.Price = 0; // Standaardwaarde
            }

                switch (action)
            {
                case "addProduct":
                    Products.Add(product);
                    break;
                case "removeProduct":
                    Products.Remove(product);
                    break;
                case "orderHistory":
                    OrderHistory();
                    break;
                case "checkout":
                    confirmOrderbtn = true;
                    break;
                case "checkoutConfirm":
                    Checkout();
                    confirmOrderbtn = false;
                    return RedirectToPage();
                default:
                    Console.WriteLine("Ongeldige actie.");
                    break;
            }

            HttpContext.Session.SetString("Products", JsonSerializer.Serialize(Products));
            return Page();
        }

        private void Checkout()
        {
            try
            {
                var customer = _customerRepository.GetAllCustomers().FirstOrDefault();
                if (customer == null)
                {
                    _logger.LogError("Geen customer gevonden voor order.");
                    return;
                }

                var newOrder = new Order
                {
                    OrderDate = DateTime.Now,
                    Customer = customer
                };

                foreach (var product in Products)
                {
                    // Assuming you have a method to get product from DB by Id
                    var existingProduct = _productRepository.GetProductById(product.Id);
                    if (existingProduct != null)
                    {
                        newOrder.Products.Add(existingProduct);
                    }
                    else
                    {
                        _logger.LogWarning($"Product with Id {product.Id} not found in DB.");
                    }
                }

                _orderRepository.AddOrder(newOrder);

                Products.Clear();
                HttpContext.Session.SetString("Products", JsonSerializer.Serialize(Products));
                confirmOrder = false;
                _logger.LogInformation("Bestelling succesvol verwerkt en opgeslagen.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fout bij het verwerken van de bestelling: {ex.Message}");
            }
        }



        private void OrderHistory()
        {
            showOrderHistory = !showOrderHistory;
            if (showOrderHistory)
            {
                LoadOrderHistory();
            }
        }

        private void LoadOrderHistory()
        {
            try
            {
                var orders = _orderRepository.GetAllOrders();
                orderHistory = new List<string>();
          
                foreach (var order in orders)
                {
                    orderHistory.Add($"Bestelling #{order.Id}");
                    foreach (var item in order.Products)
                    {
                        orderHistory.Add($" - {item.Name}");
                    }
                    orderHistory.Add("Order compleet");
                    orderHistory.Add("");
                }
                _logger.LogInformation("Orderhistorie succesvol opgehaald.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fout bij het ophalen van de orderhistorie: {ex.Message}");
            }
        }
    }
}
