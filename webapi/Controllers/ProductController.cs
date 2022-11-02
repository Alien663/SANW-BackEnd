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
                List<ProductModel> data = db.Connection.Query<ProductModel>(sql, new { condition.Discontinued, condition.Category, condition.Supplier, condition.Price, condition.ProductName, condition.page, condition.pagesize }).ToList();
                return Ok(new { data, counts.counts });
            }
        }

        [HttpPost("download")]
        public IActionResult DownloadGridView([FromBody] ProductCondition condition)
        {
            using (var db = new AppDb())
            {
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
                sql += string.IsNullOrEmpty(condition.ProductName) ? "" : " and ProductName = @_product ";

                List<ProductModel> data = db.Connection.Query<ProductModel>(sql, new { condition.Discontinued, condition.Category, condition.Supplier, condition.Price, condition.ProductName }).ToList();
                ExcelComponent myexcel = new ExcelComponent();
                byte[] excelFile = myexcel.export(data);

                HttpContext.Response.ContentType = "application/octet-stream";
                HttpContext.Response.Headers.Add("Content-Disposition", "attachment;filename=Products.xlsx");
                return File(excelFile, "application/octet-stream");
            }
        }

        [HttpPost]
        public IActionResult UpdateOrder([FromBody] ProductModel payload)
        {
            try
            {
                using (var db = new AppDb())
                {
                    string sql = @"xp_ProductUpdate";
                    var p = new DynamicParameters();
                    p.Add("@ProductID", payload.ProductID);
                    p.Add("@ProductName", payload.ProductName);
                    p.Add("@QuantityPerUnit", payload.QuantityPerUnit);
                    p.Add("@UnitPrice", payload.UnitPrice);
                    p.Add("@UnitsInStock", payload.UnitsInStock);
                    p.Add("@UnitsOnOrder", payload.UnitsOnOrder);
                    p.Add("@ReorderLevel", payload.ReorderLevel);
                    p.Add("@Discontinued", payload.Discontinued);
                    db.Connection.Execute(sql, p, commandType: System.Data.CommandType.StoredProcedure);
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }

        [HttpPut]
        public IActionResult CreateProduct([FromBody] ProductModel payload)
        {
            try
            {
                using (var db = new AppDb())
                {
                    string sql = @"xp_ProductCreate";
                    var p = new DynamicParameters();
                    db.Connection.Execute(sql, p, commandType: System.Data.CommandType.StoredProcedure);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        public IActionResult DeleteProduct([FromBody] int ProductID)
        {
            try
            {
                using (var db = new AppDb())
                {
                    string sql = @"xp_ProductDelete";
                    var p = new DynamicParameters();
                    db.Connection.Execute(sql, p, commandType: System.Data.CommandType.StoredProcedure);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
