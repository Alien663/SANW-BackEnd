using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Model
{
    public class PorductModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string QuantityPerUnit { get; set; }
        public double UnitPrice { get; set; }
        public int UnitsInStock { get; set; }
        public int UnitsOnOrder { get; set; }
        public int ReorderLevel { get; set; }
        public Boolean Discontinued { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string Supplier { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
    }
    public class ProductCondition
    {
        public int Category { get; set; }
        public int Supplier { get; set; }
        public double Price { get; set; }
        public string ProductName { get; set; }
        public bool Discontinued { get; set; }
        public int page { get; set; }
        public int pagesize { get; set; }
    }
}
