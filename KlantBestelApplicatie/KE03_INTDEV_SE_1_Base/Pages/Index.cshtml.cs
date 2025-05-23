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
        // Andere variabelen
        public float totaalPrijs;
        public Customer? SelectedCustomer { get; set; }

        // Lijsten voor productnamen, prijzen en beschrijvingen
        public List<float> prijsLijst = new List<float>();
        public List<string> beschrijvingLijst = new List<string>();
        public List<string> productList = new List<string>();
        public List<int> productIdList = new List<int>();

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

            // Haalt de geselecteerde gebruiker (customer) op
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
            // Haalt de geselecteerde gebruiker (customer) op
            Customers = _customerRepository.GetAllCustomers().ToList();
            _logger.LogInformation($"getting all {Customers.Count} customers");

            var id = HttpContext.Session.GetInt32("SelectedCustomerId");
            if (id.HasValue)
            {
                SelectedCustomer = _customerRepository.GetCustomerById(id.Value);
            }

            // Voegt producten van database toe aan de productenlijst
            VoegProductenToe();

            // Haalt de producten lijst op uit de sessie
            var productsJson = HttpContext.Session.GetString("Products");
            _logger.LogInformation("Products JSON uit sessie: " + productsJson);

            Products = productsJson != null
                ? JsonSerializer.Deserialize<List<Product>>(productsJson) ?? new List<Product>()
                : new List<Product>();

            // Haalt de productinformatie op van het product dat je hebt geselecteerd
            Product product = new Product(); // Product instantie wordt aangemaakt
            var action = Request.Form["action"]; // actie (toevoegen, verwijderen, etc.)
            int.TryParse(Request.Form["productId"], out int productId); // product id omzetten naar int
            product.Id = productId; // product id
            product.Name = Request.Form["productName"]; // product naam
            product.Description = Request.Form["omschrijving"]; // product omschrijving

            // Checkt voor prijs van product
            if (!string.IsNullOrWhiteSpace(Request.Form["prijs"]) && float.TryParse(Request.Form["prijs"], out float price)) // Probeert prijs op te halen en om te zetten naar een float
            {
                product.Price = price; // Als dat lukt wordt de prijs van het product ingesteld
            }
            else
            {
                product.Price = 0.0f; // Anders wordt de prijs naar 0 gezet
            }

            // Switch (case) statement  voor alle acties
            switch (action)
            {
                // Toevoegen van een product
                case "addProduct":
                    var dbProduct = _productRepository.GetProductById(productId); // Product uit db ophalen
                    if (dbProduct != null) // Als product bestaat gebeurt deze statement
                    {
                        Products.Add(dbProduct); // Product wordt toegevoegd aan de lijst
                    }
                    break;

                // Verwijderen van een product
                case "removeProduct":
                    var toRemove = Products.FirstOrDefault(p => p.Id == product.Id); // Zoekt voor eerst voorkomende product in lijst
                    if (toRemove != null) // Product zit in de lijst
                    {
                        Products.Remove(toRemove); // Eeerste product met het product Id wordt verwijderd uit de lijst
                    }
                    break;
                
                // Orderhistorie tonen
                case "orderHistory":
                    OrderHistory();
                    break;

                // Checkout knop voor bevestigen
                case "checkout":
                    confirmOrderbtn = true; // Bevestigknop voor checkout wordt getoond
                    break;

                // Checkout is bevestigd, gaat naar de checkout functie
                case "checkoutConfirm":
                    Checkout(); // Checkout functie wordt aangeroepen
                    confirmOrderbtn = false; // Bevestigknop voor checkout wordt verborgen
                    return RedirectToPage(); // Pagina wordt opnieuw geladen

                // Default statement voor als iets fout gaat
                default:
                    Console.WriteLine("Ongeldige actie.");
                    break;
            }

            // Sla de productenlijst op in de sessie
            HttpContext.Session.SetString("Products", JsonSerializer.Serialize(Products));

            // Refresh de pagina
            return Page();
        }

        // Functie voor de checkout
        private void Checkout()
        {
            try // try catch statement voor evt. fouten
            {
                // Haal customer id op uit de sessie
                var selectedCustomerId = HttpContext.Session.GetInt32("SelectedCustomerId");
                if (!selectedCustomerId.HasValue) // Als er geen customer id is opgeslagen in de sessie runt dit
                {
                    _logger.LogError("Geen geselecteerde klant gevonden in de sessie.");
                    return;
                    // Functie returnt, waardoor de checkout niet kan gebeuren
                }

                // Customer id is goed opgehaald uit de sessie
                var customer = _customerRepository.GetCustomerById(selectedCustomerId.Value); // Haalt de juiste customer op uit de repository
                if (customer == null) // Als de customer niet gevonden kan worden in de repository
                {
                    _logger.LogError($"Klant met ID {selectedCustomerId.Value} niet gevonden in de database.");
                    return;
                    // Functie returnt, waardoor de checkout niet kan gebeuren
                }

                // Nieuwe order wordt aangemaakt
                var newOrder = new Order
                {
                    OrderDate = DateTime.Now,
                    Customer = customer
                };

                // foreach loop om door de productenlijst te lopen en om deze toe te voegen
                foreach (var product in Products)
                {
                    // Haalt het product op uit de repository
                    var existingProduct = _productRepository.GetProductById(product.Id);

                    // Als het product niet gevonden kan worden in de repository
                    if (existingProduct == null)
                    {
                        _logger.LogError($"Product met ID {product.Id} niet gevonden in repository.");
                    }
                    else // Product is opgehaald uit de repository en wordt toegevoegd aan de order
                    {
                        _logger.LogInformation($"Product gevonden: {existingProduct.Name}");
                        newOrder.Products.Add(existingProduct);
                        existingProduct.Quantity += 1;
                        
                    }
                }

                // Log de producten in de order
                _logger.LogInformation($"Voor het opslaan: producten in order: {newOrder.Products.Count}");
                _logger.LogInformation("Order toegevoegd aan database.");
                _orderRepository.AddOrder(newOrder); // Product wordt toegevoegd aan repository

                Products.Clear(); // Clear productenlijst voor volgende bestelling
                HttpContext.Session.SetString("Products", JsonSerializer.Serialize(Products)); // Sla de nieuwe lege productenlijst op in de sessie
                confirmOrder = false; // Bevestigknop voor checkout wordt verborgen
                _logger.LogInformation($"Bestelling succesvol opgeslagen voor klant: {customer.Name}"); // Log dat de bestelling succesvol is opgeslagen
            }

            // Catch statement voor als er een fout optreedt
            catch (Exception ex)
            {
                _logger.LogError($"Fout bij het verwerken van de bestelling: {ex.Message}"); // Log de foutmelding
            }
        }

        // Functie voor het tonen van de orderhistorie
        private void OrderHistory()
        {
            // Toggle de boolean voor het tonen van de orderhistorie
            showOrderHistory = !showOrderHistory;

            // Als orderHistorie true is
            if (showOrderHistory)
            {
                // Laad orderhistorie
                LoadOrderHistory();
            }
        }

        // Functie voor het ophalen van de orderhistorie
        private void LoadOrderHistory()
        {
            // Try catch statement voor evt. fouten
            try
            {
                // Haalt de customer id op uit de sessie
                var customerId = HttpContext.Session.GetInt32("SelectedCustomerId");
                if (!customerId.HasValue)
                {
                    return; // als er geen customer id is opgeslagen in de sessie, return waardoor het niks toont
                }

                // Haal de orders op uit de repository voor de klant dat deze heeft bestelt
                var orders = _orderRepository.GetAllOrders()
                   .Where(o => o.Customer.Id == customerId.Value).ToList();

                // Log het aantal orders dat is gevonden voor de bepaalde klant
                _logger.LogInformation($"Gevonden {orders.Count} orders voor klant ID {customerId}");
                orderHistory = new List<string>(); // order historie list wordt aangemaakt

                // Log de complete orderhistorie
                foreach (var order in orders)
                {
                    _logger.LogInformation($"Order #{order.Id} bevat {order.Products.Count} producten.");
                    foreach (var p in order.Products)
                    {
                        _logger.LogInformation($" - Product: {p.Name}");
                    }
                }

                // Voeg orderhistorie toe aan de lijst in deze foreach loop
                foreach (var order in orders)
                {
                    // Order ID wordt toegevoegd aan de orderhistorie
                    orderHistory.Add($"Bestelling #{order.Id}");
                    
                    // Foreach loop voor alle producten in de bestelling
                    foreach (var item in order.Products)
                    {
                        // Voeg het aantal producten per product toe aan de bestelling
                        for (int n = 0; n < item.Quantity; n++)
                        {
                            orderHistory.Add($"- {item.Name}");
                        }

                    }

                    // Voeg de order compleet toe aan de orderhistorie om aan te tonen dat de order klaar is
                    orderHistory.Add("Order compleet");
                    orderHistory.Add("");
                }
                // Log dat de orderhistorie succesvol is opgehaald
                _logger.LogInformation("Orderhistorie succesvol opgehaald.");
            }

            // Catch statement voor fouten
            catch (Exception ex)
            {
                // Log dat er een fout op is getreden
                _logger.LogError($"Fout bij het ophalen van de orderhistorie: {ex.Message}");
            }
        }

        // Functie voor het toevoegen van producten aan de productenlijst
        void VoegProductenToe()
        {
            // Foreach loop om alle producten uit de producten repository op te halen
            foreach (var item in _productRepository.GetAllProducts())
            {
                productIdList.Add(item.Id); // Product ID wordt toegevoegd aan de lijst
                productList.Add(item.Name); // Product naam wordt toegevoegd aan de lijst
                prijsLijst.Add(item.Price); // Product prijs wordt toegevoegd aan de lijst
                beschrijvingLijst.Add(item.Description); // Product omschrijving wordt toegevoegd aan de lijst
            }
        }
    }
}
