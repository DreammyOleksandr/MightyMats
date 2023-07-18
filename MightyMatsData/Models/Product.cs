using System.ComponentModel.DataAnnotations;

namespace MightyMatsData.Models;

public class Product
{
    [Key] public int Id { get; set; }
    
    [Required] public string Title { get; set; }
    
    [MaxLength(299, ErrorMessage = "Description must contain less than 300 symbols")]
    public string? Description { get; set; }
    
    [Required] public decimal Price { get; set; }
    
    [MaxLength(399, ErrorMessage = "Details field must contain less than 400 symbols")]
    public string? Details { get; set; }
    
    [Required] public string Image { get; set; }
}