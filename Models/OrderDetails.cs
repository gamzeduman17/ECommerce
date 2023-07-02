using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models
{
    public class OrderDetails
    {
        [Key]
        public int Id { get; set; }
        [Required] 
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public OrderHeader OrderHeader { get; set; }    
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
       public int Count { get; set; }
        public double Price { get; set; }

    }
}
