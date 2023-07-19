using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MightyMatsData.Models;

public class ShoppingCartItem
{
    [Key] public int Id { get; set; }

    public int ProductId { get; set; }

    [ValidateNever, ForeignKey("ProductId")]
    public Product Product { get; set; }

    public string UserId { get; set; }

    [ValidateNever, ForeignKey("UserId")]
    public IdentityUser User { get; set; }

    [Range(1, 100, ErrorMessage = "You can't buy more than 100 mats, lad")]
    public int Count { get; set; }

    [NotMapped]
    public decimal Price { get; set; }
}