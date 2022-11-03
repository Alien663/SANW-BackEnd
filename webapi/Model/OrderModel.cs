using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Model
{
    public class OrderModel
    {
        public int OrderID { get; set; }
        public string CustomerID { get; set; }
        public DateTime OrderDate { get; set; }
        public string ShipName { get; set; }
        public int Shipper { get; set; }
        public string ShipRegion { get; set; }
        public string ShipCountry { get; set; }
        public string ShipCity { get; set; }
        public string ShipPostalCode { get; set; }
        public string ShipAddress { get; set; }
        public DateTime ShippedDate { get; set; }
        public string Employee { get; set; }
        public string JobTitle { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Address { get; set; }
    }

    public class OrderSearchCondition
    {
        public DateTime _OrderDate {
            get {
                return string.IsNullOrEmpty(this.OrderDate) ? new DateTime(1901, 1, 1) : DateTime.Parse(OrderDate);
            }
            set { }
        }
        public int Shipper { get; set; }
        public int page { get; set; }
        public int pagesize { get; set; }
        public string OrderDate { get; set; }
    }
}
