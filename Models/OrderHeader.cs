using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace ECommerce.Models
{
    public class OrderHeader
    {
        [Key]
        public int Id { get; set; }
       
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }
        public DateTime OrderDate { get; set; }
        public double OrderTotal { get; set; }
        public string OrderStatus { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Surname { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string Region { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string PostalCode { get; set; }
        [Required]
        public string CardName { get; set; }
        [Required]
        public string CardNumber { get; set; }
        [Required]
        public string ExpirationYear { get; set; }
        [Required]
        public string ExpirationMonth { get; set; }
        [Required]
        public string Cvc { get; set; }
    }
}
