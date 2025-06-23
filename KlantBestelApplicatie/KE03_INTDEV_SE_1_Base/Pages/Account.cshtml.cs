using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KE03_INTDEV_SE_1_Base.Pages
{
    public class AccountModel : PageModel
    {
        private readonly ICustomerRepository _customerRepository;
        public Customer customer { get; set; } = null!;
        public List<Order> Orders { get; set; } = new List<Order>();

        public AccountModel(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }
        public void OnGet()
        {
            var customerIdString = HttpContext.Session.GetString("customerId");
            if (int.TryParse(customerIdString, out int customerId))
            {
                customer = _customerRepository.GetCustomerById(customerId);
                if (customer != null && customer.Orders != null)
                {
                    Orders = new List<Order>(customer.Orders);
                }
                else
                {
                    Orders = new List<Order>();
                }
            }
            else
            {
                Orders = new List<Order>();
            }
        }


        public IActionResult OnPost(string action)
        {
            switch (action)
            {
                case "ChangeName":
                    var newName = Request.Form["newName"];
                    if (!string.IsNullOrEmpty(newName))
                    {
                        var customerIdString = HttpContext.Session.GetString("customerId");
                        if (int.TryParse(customerIdString, out int customerId))
                        {
                            var customer = _customerRepository.GetCustomerById(customerId);
                            if (customer != null)
                            {
                                customer.Name = newName;
                                _customerRepository.UpdateCustomer(customer);
                                HttpContext.Session.SetString("customerName", newName);
                            }
                        }
                    }
                    break;

                case "ChangePassword":
                    var newPassword = Request.Form["newPassword"];
                    if (!string.IsNullOrEmpty(newPassword))
                    {
                        var customerIdString = HttpContext.Session.GetString("customerId");
                        if (int.TryParse(customerIdString, out int customerId))
                        {
                            var customer = _customerRepository.GetCustomerById(customerId);
                            if (customer != null)
                            {
                                customer.Password = newPassword;
                                _customerRepository.UpdateCustomer(customer);
                            }
                        }
                    }
                    break;

                case "ChangeAddress":
                    var newAddress = Request.Form["newAddress"];
                    if (!string.IsNullOrEmpty(newAddress))
                    {
                        var customerIdString = HttpContext.Session.GetString("customerId");
                        if (int.TryParse(customerIdString, out int customerId))
                        {
                            var customer = _customerRepository.GetCustomerById(customerId);
                            if (customer != null)
                            {
                                customer.Address = newAddress;
                                _customerRepository.UpdateCustomer(customer);
                            }
                        }
                    }
                    break;

                case "Logout":
                    HttpContext.Session.SetString("isLoggedIn", "False");
                    HttpContext.Session.Remove("customerId");
                    return RedirectToPage("/LoginPage");

                default:
                    ModelState.AddModelError(string.Empty, "Ongeldige actie.");
                    return Page();
            }
            return Page();
        }
    }
}
