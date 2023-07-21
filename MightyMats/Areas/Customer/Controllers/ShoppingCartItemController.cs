using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MightyMatsData;
using MightyMatsData.Models;
using MightyMatsData.Models.ViewModels;
using MightyMatsData.UnitOfWork;
using Stripe;
using Stripe.Checkout;

namespace MightyMats.Controllers;

[Area("Customer")]
[Authorize]
public class ShoppingCartItemController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    [BindProperty] public ShoppingCartVM ShoppingCartVm { get; set; }

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
            OrderHeader = new(),
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

        ShoppingCartVm = new()
        {
            ShoppingCartItems =
                await _unitOfWork._shoppingCartItemRepository.GetAll(_ => _.UserId == userId, includeProps: "Product"),
            OrderHeader = new(),
        };

        ShoppingCartVm.OrderHeader.User = await _unitOfWork._identityUserRepository.Get(_ => _.Id == userId);

        foreach (var item in ShoppingCartVm.ShoppingCartItems)
        {
            item.Price = GetOrderTotal(item);
            ShoppingCartVm.OrderHeader.OrderTotal += (double)(item.Price * item.Count);
        }

        return View(ShoppingCartVm);
    }

    [HttpPost, ActionName("Summary")]
    public async Task<IActionResult> SummaryPOST(ShoppingCartVM shoppingCartVm)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        ShoppingCartVm.ShoppingCartItems =
            await _unitOfWork._shoppingCartItemRepository.GetAll(_ => _.UserId == userId, includeProps: "Product");

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
                Count = item.Count,
            };
            await _unitOfWork._orderDetailRepository.Add(orderDetail);
            await _unitOfWork._orderDetailRepository.Save();
        }

        var domain = "https://localhost:7094/";

        var options = new SessionCreateOptions
        {
            SuccessUrl = domain + $"Customer/ShoppingCartItem/OrderConfirmation?id={shoppingCartVm.OrderHeader.Id}",
            CancelUrl = domain + $"Customer/ShoppingCartItem/Index",
            LineItems = new List<SessionLineItemOptions>(),
            Mode = "payment",
        };

        foreach (var item in ShoppingCartVm.ShoppingCartItems)
        {
            var sessionLineItem = new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions()
                {
                    UnitAmount = (long)(item.Price * 100),
                    Currency = "uah",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Product.Title
                    }
                },
                Quantity = item.Count,
            };
            options.LineItems.Add(sessionLineItem);
        }

        var service = new SessionService();
        Session session = await service.CreateAsync(options);

        _unitOfWork._orderHeaderRepository.UpdateStripePaymentId(ShoppingCartVm.OrderHeader.Id, session.Id,
            session.PaymentIntentId);
        await _unitOfWork._orderHeaderRepository.Save();
        
        Response.Headers.Add("Location", session.Url);
        return new StatusCodeResult(303);
    }

    public IActionResult OrderConfirmation(int id)
    {
        return View(id);
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