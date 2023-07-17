using Microsoft.AspNetCore.Mvc;
using MightyMatsData.UnitOfWork;

namespace MightyMats.Controllers;

[Area("Admin")]
public class ProductController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    
    public ProductController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    
    public IActionResult Index()
    {
        var productsList = _unitOfWork._productRepository.GetAll(null).Result;
        return View(productsList);
    }
    
    public IActionResult Create()
    {
        return View();
    }
}