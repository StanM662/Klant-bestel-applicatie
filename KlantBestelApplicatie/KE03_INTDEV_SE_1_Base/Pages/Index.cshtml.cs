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

        public Customer? SelectedCustomer { get; set; }

        // Lijsten voor productnamen, prijzen en beschrijvingen
        public List<float> prijsLijst = new List<float>();
        public List<string> beschrijvingLijst = new List<string>();
        public List<string> productList = new List<string>();

        // Logger voor debuggen
        private readonly ILogger<IndexModel> _logger;

        // Bools
        public bool showOrderHistory = false;
        public bool confirmOrder = false;
        public bool confirmOrderbtn = false;

        // Repositories
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IPartRepository _partRepository;
        private readonly IProductRepository _productRepository;

        // Lijsten
        public List<string> orderHistory { get; set; } = new List<string>();

        public List<Product> Products = new List<Product>();
        public IList<Customer> Customers { get; set; }

        // Constructor voor IndexModel
        public IndexModel(ILogger<IndexModel> logger, ICustomerRepository customerRepository, IOrderRepository orderRepository, IProductRepository productRepository, IPartRepository partRepository)
        {
            _logger = logger;
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _partRepository = partRepository;
            Customers = new List<Customer>();

        }

        // OnGet methode (dus wanneer de pagina geladen wordt)
        public void OnGet()
        {
            VoegProductenToe();

            // Haalt de producten lijst op uit de sessie
            var productsJson = HttpContext.Session.GetString("Products");
            Products = productsJson != null
                ? JsonSerializer.Deserialize<List<Product>>(productsJson) ?? new List<Product>()
                : new List<Product>();


            Customers = _customerRepository.GetAllCustomers().ToList();
            _logger.LogInformation($"getting all {Customers.Count} customers");

            var id = HttpContext.Session.GetInt32("SelectedCustomerId");
            if (id.HasValue)
            {
                SelectedCustomer = _customerRepository.GetCustomerById(id.Value);
            }

        }

        // OnPost methode (dus wanneer er een actie uitgevoerd wordt)
        public IActionResult OnPost()
        {
            VoegProductenToe();

            // Haalt de producten lijst op uit de sessie
            var productsJson = HttpContext.Session.GetString("Products");
            Products = productsJson != null
                ? JsonSerializer.Deserialize<List<Product>>(productsJson) ?? new List<Product>()
                : new List<Product>();


            Product product = new Product();
            var action = Request.Form["action"];
            int.TryParse(Request.Form["productId"], out int productId);
            product.Id = productId;

            product.Name = Request.Form["productName"];
            product.Description = Request.Form["omschrijving"];

            if (!string.IsNullOrWhiteSpace(Request.Form["prijs"]) && float.TryParse(Request.Form["prijs"], out float price))
            {
                product.Price = price;
            }
            else
            {
                product.Price = 0.0f;
            }


            switch (action)
            {
                case "addProduct":
                    Products.Add(product);
                    break;

                case "removeProduct":
                    var toRemove = Products.FirstOrDefault(p => p.Id == product.Id);
                    if (toRemove != null)
                    {
                        Products.Remove(toRemove);
                    }
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
                // Get customer ID from session
                var selectedCustomerId = HttpContext.Session.GetInt32("SelectedCustomerId");
                if (!selectedCustomerId.HasValue)
                {
                    _logger.LogError("Geen geselecteerde klant gevonden in de sessie.");
                    return;
                }

                // Get the correct customer from the repository
                var customer = _customerRepository.GetCustomerById(selectedCustomerId.Value);
                if (customer == null)
                {
                    _logger.LogError($"Klant met ID {selectedCustomerId.Value} niet gevonden in de database.");
                    return;
                }

                var newOrder = new Order
                {
                    OrderDate = DateTime.Now,
                    Customer = customer
                };

                foreach (var product in Products)
                {
                    var existingProduct = _productRepository.GetProductById(product.Id);
                    if (existingProduct != null)
                    {
                        newOrder.Products.Add(existingProduct);
                    }
                    else
                    {
                        _logger.LogWarning($"Product met ID {product.Id} niet gevonden in de database.");
                    }
                }

                _orderRepository.AddOrder(newOrder);

                Products.Clear();
                HttpContext.Session.SetString("Products", JsonSerializer.Serialize(Products));
                confirmOrder = false;
                _logger.LogInformation($"Bestelling succesvol opgeslagen voor klant: {customer.Name}");
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
                var customerId = HttpContext.Session.GetInt32("SelectedCustomerId");
                if (!customerId.HasValue) return;

                var orders = _orderRepository.GetAllOrders()
                    .Where(o => o.Customer.Id == customerId.Value);

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

        void VoegProductenToe()
        {
            Random rand = new Random();

            // Voeg parts toe
            foreach (var item in _partRepository.GetAllParts())
            {
                productList.Add(item.Name);
                prijsLijst.Add(rand.Next(4, 21));
                beschrijvingLijst.Add(item.Description);
            }

            // Voeg producten toe
            foreach (var item in _productRepository.GetAllProducts())
            {
                productList.Add(item.Name);
                prijsLijst.Add(item.Price);
                beschrijvingLijst.Add(item.Description);
            }

        }


    }
}
