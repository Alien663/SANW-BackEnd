using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using WebAPI.Lib;
using WebAPI.Model;
using NPOI.HPSF;
using NPOI.HSSF.Record.PivotTable;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        [HttpPost("gridview")]
        public IActionResult GetGridView([FromBody] OrderSearchCondition condition)
        {
            using (var db = new AppDb())
            {
                string sqlcount = @"
                    declare @_shipper int = @Shipper
                    select count(*) as counts from vd_OrderGridView
                    where OrderDate >= @_OrderDate ";
                sqlcount += condition.Shipper == 0 ? "" : " and ShipperID = @_shipper ";
                var counts = db.Connection.QuerySingleOrDefault(sqlcount, new { condition._OrderDate , condition.Shipper});

                string sql = @"
                    declare @_shipper int = @Shipper
                    select OrderID, CustomerID, OrderDate, ShipName, Shipper, ShipRegion, ShipCountry, ShipCity, ShipPostalCode, ShipAddress, ShippedDate, Employee, JobTitle, Region, Country, City, PostalCode, [Address]
                    from vd_OrderGridView
                    where OrderDate >= @_OrderDate ";
                sql += condition.Shipper == 0 ? "" : " and ShipperID = @_shipper ";
                sql += @" order by OrderID
                    offset(@page)*@pagesize ROWS
                    fetch next @pagesize rows only";
                List<OrdersModel> data = db.Connection.Query<OrdersModel>(sql, new { condition._OrderDate, condition.Shipper, condition.page, condition.pagesize }).ToList();
                return Ok(new { data, counts.counts });
            }
        }


        [HttpPost("download")]
        public IActionResult DownloadGridView([FromBody] OrderSearchCondition condition)
        {
            using (var db = new AppDb())
            {
                string sql = @"
                    declare @_shipper int = @Shipper
                    select OrderID, CustomerID, OrderDate, ShipName, Shipper, ShipRegion, ShipCountry, ShipCity, ShipPostalCode, ShipAddress, ShippedDate, Employee, JobTitle, Region, Country, City, PostalCode, [Address]
                    from vd_OrderGridView
                    where OrderDate >= @_OrderDate ";
                sql += condition.Shipper == 0 ? "" : " and ShipperID = @_shipper ";
                List<OrdersModel> data = db.Connection.Query<OrdersModel>(sql, new { condition._OrderDate, condition.Shipper }).ToList();
                ExcelComponent myexcel = new ExcelComponent();
                byte[] excelFile = myexcel.export(data);

                HttpContext.Response.ContentType = "application/octet-stream";
                HttpContext.Response.Headers.Add("Content-Disposition", "attachment;filename=Orders.xlsx");
                return File(excelFile, "application/octet-stream");
            }
        }

        [HttpGet("{OID}")]
        public IActionResult GetOrderDetail(int OID)
        {
            using (var db = new AppDb())
            {
                string sql = @"select OrderID, CustomerID, OrderDate, ShipName, Shipper, ShipRegion, ShipCountry, ShipCity, ShipPostalCode, ShipAddress, ShippedDate, Employee, JobTitle, Region, Country, City, PostalCode, [Address]
                    from vd_OrderGridView wehre OrderID = @OID";
                OrdersModel data = db.Connection.QueryFirstOrDefault(sql, new { OID });
                return Ok(data);
            }
        }
    }
}
