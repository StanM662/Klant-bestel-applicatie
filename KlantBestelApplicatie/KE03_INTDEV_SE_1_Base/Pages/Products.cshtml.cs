using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using System.Text.Json;

namespace KE03_INTDEV_SE_1_Base.Pages;

public class ProductsModel : PageModel
{
    private readonly ILogger<ProductsModel> _logger;
    private readonly IProductRepository _productRepository;
    private readonly IPartRepository _partRepository;
    private readonly IOrderRepository _orderRepository;
    public List<Product> Products { get; set; } = new List<Product>();
    public List<Product> Cart { get; set; } = new List<Product>();

    public ProductsModel(ILogger<ProductsModel> logger, IProductRepository productRepository, IPartRepository partRepository, IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
        _logger = logger;
        _productRepository = productRepository;
        _partRepository = partRepository;
        Products = _productRepository.GetAllProducts().ToList();

    }

    public void OnGet()
    {
        LoadCart();
        Products = _productRepository.GetAllProducts().ToList();
    }

    public IActionResult OnPost(string action)
    {
        LoadCart();
        switch (action)
        {
            case "AddToCart":
                var productId = Request.Form["productId"];
                if (int.TryParse(productId, out int id))
                {
                    var product = _productRepository.GetProductById(id);
                    if (product != null)
                    {
                        AddProductToCart(product);
                    }
                }
                break;

            case "GoToCart":
                return RedirectToPage("Cart");

            default:
                ModelState.AddModelError(string.Empty, "Ongeldige actie.");
                break;
        }
        
        SaveCart();
        Products = _productRepository.GetAllProducts().ToList();
        return Page();
    }

    private void AddProductToCart(Product product)
    {
        Cart.Add(product);
        SaveCart();
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
}