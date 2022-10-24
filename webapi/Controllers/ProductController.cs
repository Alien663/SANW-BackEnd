using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using WebAPI.Lib;
using WebAPI.Model;
namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        [HttpPost("gridview")]
        public IActionResult GetGridView([FromBody] ProductCondition condition)
        {
            using (var db = new AppDb())
            {
                string sqlcount = @"
                    declare @_category int = @Category
                    declare @_supplier int = @Supplier
                    declare @_price float = @Price
                    declare @_product varchar(40)= @ProductName
                    select count(*) as counts from vd_Product where Discontinued = @Discontinued
                    ";
                sqlcount += condition.Category == 0 ? "" : " and CategoryID = @_category ";
                sqlcount += condition.Supplier == 0 ? "" : " and SupplierID = @_supplier ";
                sqlcount += condition.Price == 0 ? "" : " and UnitPrice >= @_price ";
                sqlcount += string.IsNullOrEmpty(condition.ProductName) ? "" : " and ProductName = @_product ";
                var counts = db.Connection.QuerySingleOrDefault(sqlcount, new { condition.Discontinued, condition.Category, condition.Supplier, condition.Price, condition.ProductName});

                string sql = @"
                    declare @_category int = @Category
                    declare @_supplier int = @Supplier
                    declare @_price float = @Price
                    declare @_product varchar(40)= @ProductName
                    select ProductID, ProductName, QuantityPerUnit, UnitPrice, UnitsInStock, UnitsOnOrder, ReorderLevel, Discontinued, CategoryID, CategoryName, [Description], SupplierID, Supplier, ContactName, ContactTitle
                    from vd_Product
                    where Discontinued = @Discontinued
                    ";
                sql += condition.Category == 0 ? "" : " and CategoryID = @_category ";
                sql += condition.Supplier == 0 ? "" : " and SupplierID = @_supplier ";
                sql += condition.Price == 0 ? "" : " and UnitPrice >= @_price ";
                sql += string.IsNullOrEmpty(condition.ProductName)? "" : " and ProductName = @_product ";
                sql += @"
                    order by ProductID
                    offset(@page)*@pagesize ROWS
                    fetch next @pagesize rows only";
                List<PorductModel> data = db.Connection.Query<PorductModel>(sql, new { condition.Discontinued, condition.Category, condition.Supplier, condition.Price, condition.ProductName, condition.page, condition.pagesize }).ToList();
                return Ok(new { data, counts.counts });
            }
        }
    }
}
