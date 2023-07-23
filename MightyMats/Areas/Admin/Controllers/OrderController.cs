using Microsoft.AspNetCore.Mvc;
using MightyMatsData.Models.ViewModels;
using MightyMatsData.UnitOfWork;

namespace MightyMats.Controllers;

[Area("Admin")]
public class OrderController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Details(int orderId)
    {
        OrderVM orderVm = new()
        {
            OrderHeader = await _unitOfWork._orderHeaderRepository
                .Get(_ => _.Id == orderId, includeProps: "User"),
            OrderDetails =
                await _unitOfWork._orderDetailRepository.GetAll(_ => _.OrderHeaderId == orderId,
                    includeProps: "Product"),
        };
        return View(orderVm);
    }

    #region API CALLS

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var objOrderHeaders = await _unitOfWork._orderHeaderRepository.GetAll(includeProps: "User");
        return Json(new { data = objOrderHeaders });
    }

    #endregion
}