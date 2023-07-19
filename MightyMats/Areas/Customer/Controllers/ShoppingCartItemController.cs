using Microsoft.AspNetCore.Mvc;

namespace MightyMats.Controllers;

[Area("Customer")]
public class ShoppingCartItemController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}