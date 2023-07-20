namespace MightyMatsData.Models.ViewModels;

public class ShoppingCartVM
{
    public IEnumerable<ShoppingCartItem> ShoppingCartItems { get; set; }

    public OrderHeader OrderHeader { get; set; }
}