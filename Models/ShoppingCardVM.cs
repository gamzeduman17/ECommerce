using System.Collections;
using System.Collections.Generic;

namespace ECommerce.Models
{
    public class ShoppingCardVM
    {
        public IEnumerable<ShoppingCard> ListCard { get; set; }
        public OrderHeader OrderHeader { get; set; }    
    }
}
