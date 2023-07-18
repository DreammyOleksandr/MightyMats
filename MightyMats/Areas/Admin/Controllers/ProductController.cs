using Microsoft.AspNetCore.Mvc;
using MightyMatsData.Models;
using MightyMatsData.UnitOfWork;
using NToastNotify;

namespace MightyMats.Controllers;

[Area("Admin")]
public class ProductController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IToastNotification _toastNotification;

    public ProductController(IUnitOfWork unitOfWork, IToastNotification toastNotification)
    {
        _unitOfWork = unitOfWork;
        _toastNotification = toastNotification;
    }


    public async Task<IActionResult> Index()
    {
        IEnumerable<Product> productsList = await _unitOfWork._productRepository.GetAll(null);
        return View(productsList);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product product)
    {
        if (ModelState.IsValid)
        {
            _unitOfWork._productRepository.Add(product);
            await _unitOfWork._productRepository.Save();

            _toastNotification.AddSuccessToastMessage("Successful creation");
            return RedirectToAction(nameof(Index));
        }

        return View();
    }

    public async Task<IActionResult> Edit(int? id)
    {
        Product product = await _unitOfWork._productRepository.Get(_ => _.Id == id);
        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Product? product)
    {
        if (product == null || product.Id == null || product.Id == 0)
            return NotFound();

        if (ModelState.IsValid)
        {
            _unitOfWork._productRepository.Update(product);
            await _unitOfWork._productRepository.Save();

            _toastNotification.AddInfoToastMessage("Successful update");

            return RedirectToAction(nameof(Index));
        }

        return View();
    }

    public async Task<IActionResult> Delete(int? id)
    {
        Product? product = await _unitOfWork._productRepository.Get(_ => _.Id == id);

        if (product == null || product.Id == null || product.Id == 0)
            return NotFound();

        if (ModelState.IsValid)
        {
            _unitOfWork._productRepository.Remove(product);
            await _unitOfWork._productRepository.Save();
            
            _toastNotification.AddWarningToastMessage($"Product {product.Title} has been deleted");
        }

        return RedirectToAction(nameof(Index));
    }
}