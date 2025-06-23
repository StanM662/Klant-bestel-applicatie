using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KE03_INTDEV_SE_1_Base.Pages
{
    public class HomePageModel : PageModel
    {
        private readonly ICustomerRepository _customerRepository;
        public Customer customer = new Customer();

        public HomePageModel(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }
        public void OnGet()
        {
            var customerIdString = HttpContext.Session.GetString("customerId");
            if (int.TryParse(customerIdString, out int customerId))
            {
                customer = _customerRepository.GetCustomerById(customerId);
            }
        }
    }
}
