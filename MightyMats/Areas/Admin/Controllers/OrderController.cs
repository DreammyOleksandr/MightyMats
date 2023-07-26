using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MightyMatsData;
using MightyMatsData.Models;
using MightyMatsData.Models.ViewModels;
using MightyMatsData.UnitOfWork;
using NToastNotify;
using Stripe;

namespace MightyMats.Controllers;

[Area("Admin")]
[Authorize]
public sealed class OrderController : Controller
{
    private readonly IToastNotification _toastNotification;
    private readonly IUnitOfWork _unitOfWork;


    public OrderController(IUnitOfWork unitOfWork, IToastNotification toastNotification)
    {
        _unitOfWork = unitOfWork;
        _toastNotification = toastNotification;
    }

    [BindProperty] public OrderVM OrderVm { get; set; }


    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Details(int orderId)
    {
        OrderVm = new OrderVM
        {
            OrderHeader = await _unitOfWork._orderHeaderRepository
                .Get(_ => _.Id == orderId, "User"),
            OrderDetails =
                await _unitOfWork._orderDetailRepository.GetAll(_ => _.OrderHeaderId == orderId,
                    "Product")
        };
        return View(OrderVm);
    }

    [Authorize(Roles = $"{StaticDetails.AdminRole}")]
    [HttpPost]
    public async Task<IActionResult> UpdateOrderDetail()
    {
        var orderHeaderFromDb = await _unitOfWork._orderHeaderRepository.Get(u => u.Id == OrderVm.OrderHeader.Id);
        orderHeaderFromDb.Name = OrderVm.OrderHeader.Name;
        orderHeaderFromDb.PhoneNumber = OrderVm.OrderHeader.PhoneNumber;
        orderHeaderFromDb.StreetAddress = OrderVm.OrderHeader.StreetAddress;
        orderHeaderFromDb.City = OrderVm.OrderHeader.City;
        orderHeaderFromDb.State = OrderVm.OrderHeader.State;
        orderHeaderFromDb.PostalCode = OrderVm.OrderHeader.PostalCode;

        if (!string.IsNullOrEmpty(OrderVm.OrderHeader.Carrier))
            orderHeaderFromDb.Carrier = OrderVm.OrderHeader.Carrier;


        if (!string.IsNullOrEmpty(OrderVm.OrderHeader.TrackingNumber))
            orderHeaderFromDb.Carrier = OrderVm.OrderHeader.TrackingNumber;

        _unitOfWork._orderHeaderRepository.Update(orderHeaderFromDb);
        await _unitOfWork._orderHeaderRepository.Save();

        _toastNotification.AddInfoToastMessage("Order has been updated successfully");

        return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
    }

    [Authorize(Roles = $"{StaticDetails.AdminRole}")]
    [HttpPost]
    public IActionResult StartProcessing()
    {
        _unitOfWork._orderHeaderRepository.UpdateStatus(OrderVm.OrderHeader.Id, StaticDetails.StatusInProcess);
        _unitOfWork._orderHeaderRepository.Save();

        _toastNotification.AddInfoToastMessage($"Started processing the order with id: {OrderVm.OrderHeader.Id}");

        return RedirectToAction(nameof(Details), new { orderId = OrderVm.OrderHeader.Id });
    }

    [Authorize(Roles = $"{StaticDetails.AdminRole}")]
    [HttpPost]
    public async Task<IActionResult> ShipOrder()
    {
        var orderHeader = await _unitOfWork._orderHeaderRepository.Get(_ => _.Id == OrderVm.OrderHeader.Id);
        orderHeader.Carrier = OrderVm.OrderHeader.Carrier;
        orderHeader.TrackingNumber = OrderVm.OrderHeader.TrackingNumber;
        orderHeader.OrderStatus = StaticDetails.StatusShipped;
        orderHeader.ShippingDate = DateTime.Now;

        _unitOfWork._orderHeaderRepository.Update(orderHeader);
        _unitOfWork._orderHeaderRepository.UpdateStatus(OrderVm.OrderHeader.Id, StaticDetails.StatusShipped);
        await _unitOfWork._orderHeaderRepository.Save();

        _toastNotification.AddInfoToastMessage($"Order with id: {OrderVm.OrderHeader.Id} is shipped");

        return RedirectToAction(nameof(Details), new { orderId = OrderVm.OrderHeader.Id });
    }

    [Authorize(Roles = $"{StaticDetails.AdminRole}")]
    [HttpPost]
    public async Task<IActionResult> CancelOrder()
    {
        var orderHeader = await _unitOfWork._orderHeaderRepository.Get(_ => _.Id == OrderVm.OrderHeader.Id);

        if (orderHeader.PaymentStatus == StaticDetails.PaymentStatusApproved)
        {
            var options = new RefundCreateOptions
            {
                Reason = RefundReasons.RequestedByCustomer,
                PaymentIntent = orderHeader.PaymentIntentId,
            };

            var service = new RefundService();
            Refund refund = service.Create(options);

            _unitOfWork._orderHeaderRepository.UpdateStatus(orderHeader.Id, StaticDetails.StatusCancelled,
                StaticDetails.StatusRefunded);
        }
        else
        {
            _unitOfWork._orderHeaderRepository.UpdateStatus(orderHeader.Id, StaticDetails.StatusCancelled,
                StaticDetails.StatusCancelled);
        }

        await _unitOfWork._orderHeaderRepository.Save();

        _toastNotification.AddInfoToastMessage($"Order with id: {OrderVm.OrderHeader.Id} was cancelled");

        return RedirectToAction(nameof(Details), new { orderId = OrderVm.OrderHeader.Id });
    }

    #region API CALLS

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        IEnumerable<OrderHeader> objOrderHeaders;

        if (User.IsInRole(StaticDetails.AdminRole))
            objOrderHeaders = await _unitOfWork._orderHeaderRepository.GetAll(includeProps: "User");

        else
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            objOrderHeaders =
                await _unitOfWork._orderHeaderRepository.GetAll(_ => _.UserId == userId, includeProps: "User");
        }


        return Json(new { data = objOrderHeaders });
    }

    #endregion
}