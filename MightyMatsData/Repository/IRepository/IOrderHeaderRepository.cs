using MightyMatsData.Models;

namespace MightyMatsData.Repository.IRepository;

public interface IOrderHeaderRepository : IRepository<OrderHeader>
{
    void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);
    void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId);
}