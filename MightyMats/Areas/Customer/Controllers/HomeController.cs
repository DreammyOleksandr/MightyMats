using System.Diagnostics;
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
        Product product = _unitOfWork._productRepository.Get(_ => _.Id == id).Result;
        return product == null || product.Id == 0 || id == 0 ? NotFound() : View(product);
    }
}