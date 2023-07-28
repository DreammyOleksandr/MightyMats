using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MightyMatsData.Models;
using MightyMatsData.UnitOfWork;
using NToastNotify;

namespace MightyMats.Controllers;

[Area("Customer")]
public sealed class HomeController : Controller
{
    private readonly IToastNotification _toastNotification;
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, IToastNotification toastNotification)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _toastNotification = toastNotification;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _unitOfWork._productRepository.GetAll();
        return View(products);
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> IndexPOST(int productId)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        ShoppingCartItem cart = new()
        {
            Product = _unitOfWork._productRepository.Get(_ => _.Id == productId).Result,
            ProductId = productId,
            UserId = userId,
            Count = 1,
        };

        var shoppingCartItemFromDb =
            await _unitOfWork._shoppingCartItemRepository.Get(_ =>
                _.UserId == userId && _.ProductId == cart.ProductId);

        if (!ReferenceEquals(shoppingCartItemFromDb, null))
        {
            shoppingCartItemFromDb.Count += cart.Count;
            _unitOfWork._shoppingCartItemRepository.Update(shoppingCartItemFromDb);
            _toastNotification.AddSuccessToastMessage("Mat was successfully added to the cart!");
        }
        else
        {
            await _unitOfWork._shoppingCartItemRepository.Add(cart);
            _toastNotification.AddSuccessToastMessage("Mat was successfully added to the cart!");
        }

        await _unitOfWork._shoppingCartItemRepository.Save();

        return RedirectToAction(nameof(Index));
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

    public IActionResult Details(int productId)
    {
        ShoppingCartItem cart = new()
        {
            Product = _unitOfWork._productRepository.Get(_ => _.Id == productId).Result,
            ProductId = productId
        };
        return cart.Product.Id == 0 || productId == 0 ? NotFound() : View(cart);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Details(ShoppingCartItem shoppingCartItem)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        shoppingCartItem.UserId = userId;
        shoppingCartItem.Count = 1;

        var shoppingCartItemFromDb =
            await _unitOfWork._shoppingCartItemRepository.Get(_ =>
                _.UserId == userId && _.ProductId == shoppingCartItem.ProductId);

        if (!ReferenceEquals(shoppingCartItemFromDb, null))
        {
            shoppingCartItemFromDb.Count += shoppingCartItem.Count;
            _unitOfWork._shoppingCartItemRepository.Update(shoppingCartItemFromDb);
        }
        else
        {
            await _unitOfWork._shoppingCartItemRepository.Add(shoppingCartItem);
        }

        await _unitOfWork._shoppingCartItemRepository.Save();

        return RedirectToAction(nameof(Index));
    }
}