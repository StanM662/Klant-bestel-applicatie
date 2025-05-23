using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class Product
    {        
        public Product()
        {
            Id = 1;
            Name = "Null";
            Description = "Null";
            Price = 0;
            Quantity = 0;

        }
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public float Price { get; set; }
        public int Quantity { get; set; }
        public List<Order> Orders { get; } = new List<Order>();

        public ICollection<Part> Parts { get; } = new List<Part>();
    }
}
