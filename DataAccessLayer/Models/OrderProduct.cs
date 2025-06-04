using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Models
{
    public class OrderProduct
    {        public int OrdersId { get; set; }
        public Order Order { get; set; } = null!;

        public int ProductsId { get; set; }
        public Product Product { get; set; } = null!;

        public int Aantal { get; set; }
    }
}
