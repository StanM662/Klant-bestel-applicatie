using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace KE03_INTDEV_SE_1_Base.Pages
{
    public class OrderHistoryModel : PageModel
    {
        private readonly ICustomerRepository _customerRepository;
        public Customer customer { get; set; } = null!;
        public List<Order> Orders { get; set; } = new List<Order>();

        public OrderHistoryModel(ICustomerRepository customerRepository)
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

    }
}
