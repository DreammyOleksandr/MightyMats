using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MightyMatsData.Models;
using MightyMatsData.UnitOfWork;

namespace MightyMats.Controllers;

[Area("Customer")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        IEnumerable<Product> products = await _unitOfWork._productRepository.GetAll(null);
        return View(products);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult Details(int? id)
    {
        ShoppingCartItem cart = new()
        {
            Product = _unitOfWork._productRepository.Get(_ => _.Id == id).Result,
        };
        return cart.Product.Id == 0 || id == 0 ? NotFound() : View(cart);
    }

    [HttpPost]
    [Authorize]
    public IActionResult Details(ShoppingCartItem shoppingCartItem)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        return RedirectToAction(nameof(Index));
    }
}