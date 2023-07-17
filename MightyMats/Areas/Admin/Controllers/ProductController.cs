using Microsoft.AspNetCore.Mvc;
using MightyMatsData.Models;
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

    [HttpPost]
    public IActionResult Create(Product product)
    {
        if (ModelState.IsValid)
        {
            _unitOfWork._productRepository.Add(product);
            _unitOfWork._productRepository.Save();

            return RedirectToAction(nameof(Index));
        }
        return View();
    }
}