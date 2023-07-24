using Microsoft.AspNetCore.Mvc;
using MightyMatsData.Models;
using MightyMatsData.UnitOfWork;
using NToastNotify;

namespace MightyMats.Controllers;

[Area("Admin")]
public sealed class ProductController : Controller
{
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly IToastNotification _toastNotification;
    private readonly IUnitOfWork _unitOfWork;

    public ProductController(IUnitOfWork unitOfWork, IToastNotification toastNotification,
        IWebHostEnvironment hostEnvironment)
    {
        _unitOfWork = unitOfWork;
        _toastNotification = toastNotification;
        _hostEnvironment = hostEnvironment;
    }


    public async Task<IActionResult> Index()
    {
        var productsList = await _unitOfWork._productRepository.GetAll();
        return View(productsList);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product product, IFormFile? file)
    {
        var webRootPath = _hostEnvironment.WebRootPath;
        if (!ReferenceEquals(file, null))
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var productsPath = Path.Combine(webRootPath, @"images/products");

            using (var fileStream = new FileStream(Path.Combine(productsPath, fileName), FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            product.Image = @"/images/products/" + fileName;
        }

        if (ModelState.IsValid)
        {
            await _unitOfWork._productRepository.Add(product);
            await _unitOfWork._productRepository.Save();

            _toastNotification.AddSuccessToastMessage("Successful creation");
            return RedirectToAction(nameof(Index));
        }

        return View(product);
    }


    public async Task<IActionResult> Edit(int? id)
    {
        var product = await _unitOfWork._productRepository.Get(_ => _.Id == id);
        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Product product, IFormFile? file)
    {
        var webRootPath = _hostEnvironment.WebRootPath;
        if (ModelState.IsValid)
        {
            if (!ReferenceEquals(file, null))
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var productsPath = Path.Combine(webRootPath, @"images/products");

                if (!string.IsNullOrEmpty(product.Image))
                {
                    var oldImagePath = $"{webRootPath}{product.Image.TrimStart('\\')}";

                    if (System.IO.File.Exists(oldImagePath)) System.IO.File.Delete(oldImagePath);
                }

                using (var fileStream = new FileStream(Path.Combine(productsPath, fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                product.Image = @"/images/products/" + fileName;
            }

            _unitOfWork._productRepository.Update(product);
            await _unitOfWork._productRepository.Save();

            _toastNotification.AddInfoToastMessage("Successful update");
            return RedirectToAction(nameof(Index));
        }

        return NotFound();
    }

    public async Task<IActionResult> Delete(int? id)
    {
        var webRootPath = _hostEnvironment.WebRootPath;
        var product = await _unitOfWork._productRepository.Get(_ => _.Id == id);

        if (ReferenceEquals(product, null) || ReferenceEquals(product.Id, null) || product.Id == 0)
            return NotFound();

        if (!string.IsNullOrEmpty(product.Image))
        {
            var oldImagePath = $"{webRootPath}{product.Image.TrimStart('\\')}";

            if (System.IO.File.Exists(oldImagePath)) System.IO.File.Delete(oldImagePath);
        }

        if (ModelState.IsValid)
        {
            _unitOfWork._productRepository.Remove(product);
            await _unitOfWork._productRepository.Save();

            _toastNotification.AddWarningToastMessage($"Product {product.Title} has been deleted");
        }

        return RedirectToAction(nameof(Index));
    }
}