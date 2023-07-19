namespace MightyMatsData.Models.ViewModels;

public class ShoppingCartVM
{
    public IEnumerable<ShoppingCartItem> ShoppingCartItems { get; set; }
    
    public double OrderTotal { get; set; }
}