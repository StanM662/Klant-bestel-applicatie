using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace KE03_INTDEV_SE_1_Base.Pages
{
    public class IndexModel : PageModel
    {
        private const string CartSessionKey = "Cart";
        private static bool _firstVisit = true;
        private readonly ILogger<IndexModel> _logger;
        public bool showOrderHistory = false;
        public bool confirmOrder = false;
        public bool confirmOrderbtn = false;
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;

        public IList<Customer> Customers { get; set; }
        // Lijsten voor de winkelmand en orderhistorie
        public List<string> Cart { get; set; } = new List<string>();
        public List<string> orderHistory { get; set; } = new List<string>();
        public List<Product> Products = new List<Product>();

        public IndexModel(ILogger<IndexModel> logger, ICustomerRepository customerRepository)
        {
            _logger = logger;
            _customerRepository = customerRepository;
            Customers = new List<Customer>();
        }

        // OnGet methode (dus wanneer de pagina geladen wordt)
        public void OnGet()
        {
            Customers = _customerRepository.GetAllCustomers().ToList();
            _logger.LogInformation($"getting all {Customers.Count} customers");

            // Winkelwagen ophalen uit session
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            Cart = cartJson != null ? JsonSerializer.Deserialize<List<string>>(cartJson) ?? new List<string>() : new List<string>();

            // Orderhistorie ophalen via repository
            if (showOrderHistory)
            {
                LoadOrderHistory();
            }



            // if (_firstVisit)
            //{
            //    if (System.IO.File.Exists("Cart.txt"))
            //        System.IO.File.Delete("Cart.txt");
            //    if (System.IO.File.Exists("orderHistory.txt"))
            //        System.IO.File.Delete("orderHistory.txt");
            //    _firstVisit = false;
            //}

            //if (System.IO.File.Exists("Cart.txt"))
            //    Cart = new List<string>(System.IO.File.ReadAllLines("Cart.txt"));

            //if (showOrderHistory && System.IO.File.Exists("orderHistory.txt"))
            //    orderHistory = new List<string>(System.IO.File.ReadAllLines("orderHistory.txt"));

            //if (!System.IO.File.Exists("orderHistory.txt"))
            //    System.IO.File.Create("orderHistory.txt"); 
        }

        // OnPost methode (dus wanneer er een actie uitgevoerd wordt)
        public IActionResult OnPost()
        {
            // Winkelwagen ophalen uit session
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            Cart = cartJson != null ? JsonSerializer.Deserialize<List<string>>(cartJson) ?? new List<string>() : new List<string>();

            var action = Request.Form["action"];
            var productId = Request.Form["productId"];
            var product = $"{productId}";

            switch (action)
            {
                case "addProduct":
                    Cart.Add(product);
                    break;
                case "removeProduct":
                    Cart.Remove(product);
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
                    break;
                default:
                    Console.WriteLine("Ongeldige actie.");
                    break;
            }

            // Winkelwagen opslaan in session
            HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(Cart));

            return Page();
        }

        private void Checkout()
        {
            try
            {
                var newOrder = new Order
                {
                    OrderDate = DateTime.Now
                    // Products niet hier initialiseren!
                };

                // Zorg dat Products in de Order-constructor wordt geïnitialiseerd!
                foreach (var productId in Cart)
                {
                    newOrder.Products.Add(new Product
                    {
                        Id = int.Parse(productId)
                    });
                }
                _orderRepository.AddOrder(newOrder);

                Cart.Clear();
                confirmOrder = false;
                _logger.LogInformation("Bestelling succesvol verwerkt.");
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
                    orderHistory.Add($"Bestelling #{order.Id} op {order.OrderDate:g}");
                    foreach (var item in order.Products)
                    {
                        orderHistory.Add($" - {item.Id}");
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
