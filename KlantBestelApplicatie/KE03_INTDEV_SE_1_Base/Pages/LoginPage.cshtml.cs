using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataAccessLayer.Models;
using DataAccessLayer;
using DataAccessLayer.Interfaces;

namespace KE03_INTDEV_SE_1_Base.Pages
{
    public class LoginPageModel : PageModel
    {
        public string Name { get; set; }
        public string Password { get; set; }

        private readonly ILogger<IndexModel> _logger;
        private readonly ICustomerRepository _customerRepository;

        public List<Customer> customers { get; set; }

        public LoginPageModel(ILogger<IndexModel> logger, ICustomerRepository customerRepository)
        {
            _logger = logger;
            _customerRepository = customerRepository;
        }


        public IActionResult OnPost(string Name, string Password)
        {
            customers = _customerRepository.GetAllCustomers().ToList();
            foreach (var customer in customers)
            {
                if (customer.Name == Name && customer.Password == Password)
                {
                    HttpContext.Session.SetString("isLoggedIn", "True");
                    HttpContext.Session.SetString("customerId", customer.Id.ToString());
                    return RedirectToPage("/HomePage");
                }
            }

            ModelState.AddModelError(string.Empty, "Ongeldige gebruikersnaam of wachtwoord.");
            return Page();
        }
    }
}
