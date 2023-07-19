using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MightyMatsData.Models;
using MightyMatsData.Models.ViewModels;
using MightyMatsData.UnitOfWork;

namespace MightyMats.Controllers;

[Area("Customer")]
[Authorize]
public class ShoppingCartItemController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    public ShoppingCartVM ShoppingCartVm { get; set; }

    public ShoppingCartItemController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        ShoppingCartVm = new()
        {
            ShoppingCartItems =
                await _unitOfWork._shoppingCartItemRepository.GetAll(_ => _.UserId == userId, includeProps: "Product"),
        };

        foreach (var item in ShoppingCartVm.ShoppingCartItems)
        {
            item.Price = GetOrderTotal(item);
            ShoppingCartVm.OrderTotal += (double)(item.Price * item.Count);
        }

        return View(ShoppingCartVm);
    }

    private decimal GetOrderTotal(ShoppingCartItem shoppingCartItem)
    {
        return shoppingCartItem.Product.Price;
    }

    public async Task<IActionResult> Plus(int cartId)
    {
        ShoppingCartItem shoppingCartItem = await _unitOfWork._shoppingCartItemRepository.Get(_ => _.Id == cartId);
        shoppingCartItem.Count += 1;
        _unitOfWork._shoppingCartItemRepository.Update(shoppingCartItem);
        await _unitOfWork._shoppingCartItemRepository.Save();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Minus(int cartId)
    {
        ShoppingCartItem shoppingCartItem = await _unitOfWork._shoppingCartItemRepository.Get(_ => _.Id == cartId);
        if (shoppingCartItem.Count <= 1)
        {
            _unitOfWork._shoppingCartItemRepository.Remove(shoppingCartItem);
        }
        else
        {
            shoppingCartItem.Count -= 1;
            _unitOfWork._shoppingCartItemRepository.Update(shoppingCartItem);
        }

        await _unitOfWork._shoppingCartItemRepository.Save();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Remove(int cartId)
    {
        ShoppingCartItem shoppingCartItem = await _unitOfWork._shoppingCartItemRepository.Get(_ => _.Id == cartId);
        _unitOfWork._shoppingCartItemRepository.Remove(shoppingCartItem);
        await _unitOfWork._shoppingCartItemRepository.Save();
        return RedirectToAction(nameof(Index));
    }
}