using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MightyMatsData;
using MightyMatsData.Models;
using MightyMatsData.Models.ViewModels;
using MightyMatsData.UnitOfWork;
using NToastNotify;
using Stripe.Checkout;

namespace MightyMats.Controllers;

[Area("Customer")]
[Authorize]
public sealed class ShoppingCartItemController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IToastNotification _toastNotification;

    public ShoppingCartItemController(IUnitOfWork unitOfWork, IToastNotification toastNotification)
    {
        _unitOfWork = unitOfWork;
        _toastNotification = toastNotification;
    }

    [BindProperty] public ShoppingCartVM ShoppingCartVm { get; set; }

    public async Task<IActionResult> Index()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        ShoppingCartVm = new ShoppingCartVM
        {
            ShoppingCartItems =
                await _unitOfWork._shoppingCartItemRepository.GetAll(_ => _.UserId == userId, "Product"),
            OrderHeader = new OrderHeader()
        };

        foreach (var item in ShoppingCartVm.ShoppingCartItems)
        {
            item.Price = GetOrderTotal(item);
            ShoppingCartVm.OrderHeader.OrderTotal += (double)(item.Price * item.Count);
        }

        return View(ShoppingCartVm);
    }

    public async Task<IActionResult> Summary()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        ShoppingCartVm = new ShoppingCartVM
        {
            ShoppingCartItems =
                await _unitOfWork._shoppingCartItemRepository.GetAll(_ => _.UserId == userId, "Product"),
            OrderHeader = new OrderHeader()
        };

        ShoppingCartVm.OrderHeader.User = await _unitOfWork._identityUserRepository.Get(_ => _.Id == userId);

        foreach (var item in ShoppingCartVm.ShoppingCartItems)
        {
            item.Price = GetOrderTotal(item);
            ShoppingCartVm.OrderHeader.OrderTotal += (double)(item.Price * item.Count);
        }

        return View(ShoppingCartVm);
    }

    [HttpPost]
    [ActionName("Summary")]
    public async Task<IActionResult> SummaryPOST(ShoppingCartVM shoppingCartVm)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        ShoppingCartVm.ShoppingCartItems =
            await _unitOfWork._shoppingCartItemRepository.GetAll(_ => _.UserId == userId, "Product");

        ShoppingCartVm.OrderHeader.UserId = userId;
        ShoppingCartVm.OrderHeader.OrderDate = DateTime.Now;

        foreach (var item in ShoppingCartVm.ShoppingCartItems)
        {
            item.Price = GetOrderTotal(item);
            ShoppingCartVm.OrderHeader.OrderTotal += (double)(item.Price * item.Count);
        }

        ShoppingCartVm.OrderHeader.OrderStatus = StaticDetails.StatusPending;
        ShoppingCartVm.OrderHeader.PaymentStatus = StaticDetails.PaymentStatusPending;

        await _unitOfWork._orderHeaderRepository.Add(ShoppingCartVm.OrderHeader);
        await _unitOfWork._orderHeaderRepository.Save();

        foreach (var item in ShoppingCartVm.ShoppingCartItems)
        {
            OrderDetail orderDetail = new()
            {
                ProductId = item.ProductId,
                OrderHeaderId = ShoppingCartVm.OrderHeader.Id,
                Price = item.Price,
                Count = item.Count
            };
            await _unitOfWork._orderDetailRepository.Add(orderDetail);
            await _unitOfWork._orderDetailRepository.Save();
        }

        var domain = "https://localhost:7094/";

        var options = new SessionCreateOptions
        {
            SuccessUrl = domain + $"Customer/ShoppingCartItem/OrderConfirmation?id={ShoppingCartVm.OrderHeader.Id}",
            CancelUrl = domain + "Customer/ShoppingCartItem/Index",
            LineItems = new List<SessionLineItemOptions>(),
            Mode = "payment"
        };

        foreach (var item in ShoppingCartVm.ShoppingCartItems)
        {
            var sessionLineItem = new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(item.Price * 100),
                    Currency = "uah",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Product.Title
                    }
                },
                Quantity = item.Count
            };
            options.LineItems.Add(sessionLineItem);
        }

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        _unitOfWork._orderHeaderRepository.UpdateStripePaymentId(ShoppingCartVm.OrderHeader.Id, session.Id,
            session.PaymentIntentId);
        await _unitOfWork._orderHeaderRepository.Save();

        Response.Headers.Add("Location", session.Url);
        return new StatusCodeResult(303);
    }

    public async Task<IActionResult> OrderConfirmation(int id)
    {
        var orderHeader = await _unitOfWork._orderHeaderRepository.Get(_ => _.Id == id, "User");

        var service = new SessionService();
        var session = await service.GetAsync(orderHeader.SessionId);

        if (session.PaymentStatus.ToLower() == "paid")
        {
            _unitOfWork._orderHeaderRepository.UpdateStripePaymentId(orderHeader.Id, session.Id,
                session.PaymentIntentId);
            _unitOfWork._orderHeaderRepository.UpdateStatus(id, StaticDetails.StatusApproved,
                StaticDetails.PaymentStatusApproved);
            await _unitOfWork._orderHeaderRepository.Save();
        }

        var shoppingCartItems =
            _unitOfWork._shoppingCartItemRepository.GetAll(_ => _.UserId == orderHeader.UserId).Result;

        _unitOfWork._shoppingCartItemRepository.RemoveRange(shoppingCartItems);
        await _unitOfWork._shoppingCartItemRepository.Save();

        return View(id);
    }

    private decimal GetOrderTotal(ShoppingCartItem shoppingCartItem)
    {
        return shoppingCartItem.Product.Price;
    }

    public async Task<IActionResult> Plus(int cartId)
    {
        var shoppingCartItem = await _unitOfWork._shoppingCartItemRepository.Get(_ => _.Id == cartId);
        shoppingCartItem.Count += 1;
        _unitOfWork._shoppingCartItemRepository.Update(shoppingCartItem);
        await _unitOfWork._shoppingCartItemRepository.Save();

        _toastNotification.AddSuccessToastMessage("Mat was successfully added to the cart!");

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Minus(int cartId)
    {
        var shoppingCartItem = await _unitOfWork._shoppingCartItemRepository.Get(_ => _.Id == cartId);
        if (shoppingCartItem.Count <= 1)
        {
            _unitOfWork._shoppingCartItemRepository.Remove(shoppingCartItem);
            _toastNotification.AddInfoToastMessage("Mat was removed from the cart.");
        }
        else
        {
            shoppingCartItem.Count -= 1;
            _unitOfWork._shoppingCartItemRepository.Update(shoppingCartItem);
            _toastNotification.AddInfoToastMessage("Mat was removed from the cart.");
        }

        await _unitOfWork._shoppingCartItemRepository.Save();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Remove(int cartId)
    {
        var shoppingCartItem = await _unitOfWork._shoppingCartItemRepository.Get(_ => _.Id == cartId);
        _unitOfWork._shoppingCartItemRepository.Remove(shoppingCartItem);

        await _unitOfWork._shoppingCartItemRepository.Save();


        return RedirectToAction(nameof(Index));
    }
}