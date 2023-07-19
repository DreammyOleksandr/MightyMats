using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            ShoppingCartItems = await _unitOfWork._shoppingCartItemRepository.GetAll(_ => _.UserId == userId, includeProps:"Product"),
        };

        return View(ShoppingCartVm);
    }
}