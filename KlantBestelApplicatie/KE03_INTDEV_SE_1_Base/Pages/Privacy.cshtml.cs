using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

public class PrivacyModel : PageModel
{
    private readonly ILogger<PrivacyModel> _logger;

    public PrivacyModel(ILogger<PrivacyModel> logger)
    {
        _logger = logger;
    }

    public List<Customer> Customers = new()
    {
        new Customer { Id = 1, Name = "Neo", Address = "123 Elm St", Active = true },
        new Customer { Id = 2, Name = "Morpheus", Address = "456 Oak St", Active = true },
        new Customer { Id = 3, Name = "Trinity", Address = "789 Pine St", Active = true }
    };

    [BindProperty]
    public int SelectedCustomerId { get; set; }

    public Customer? SelectedCustomer { get; set; }

    public void OnGet()
    {
        var storedId = HttpContext.Session.GetInt32("SelectedCustomerId");
        if (storedId.HasValue)
        {
            SelectedCustomer = Customers.FirstOrDefault(c => c.Id == storedId.Value);
        }
    }

    public void OnPost()
    {
        HttpContext.Session.SetInt32("SelectedCustomerId", SelectedCustomerId);

        SelectedCustomer = Customers.FirstOrDefault(c => c.Id == SelectedCustomerId);
    }

}
