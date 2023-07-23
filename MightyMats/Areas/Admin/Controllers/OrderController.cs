using Microsoft.AspNetCore.Mvc;
using MightyMatsData.UnitOfWork;

namespace MightyMats.Controllers;

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

    #region API CALLS

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var objOrderHeaders = await _unitOfWork._orderHeaderRepository.GetAll(includeProps: "User");
        return Json(new { data = objOrderHeaders });
    }

    #endregion
}