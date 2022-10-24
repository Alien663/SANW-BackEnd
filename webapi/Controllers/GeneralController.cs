using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Lib;
using Dapper;
using WebAPI.Model;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeneralController : ControllerBase
    {
        [HttpGet("dropdown/category")]
        public IActionResult GetCateGories()
        {
            using (var db = new AppDb())
            {
                string sql = "select CategoryID as [Value], CategoryName as [Name] from Categories";
                List<DropdownList> data = db.Connection.Query<DropdownList>(sql).ToList();
                return Ok(data);
            }
        }
        [HttpGet("dropdown/supplier")]
        public IActionResult GetSuppliers()
        {
            using (var db = new AppDb())
            {
                string sql = "select SupplierID as [Value], CompanyName as [Name] from Suppliers";
                List<DropdownList> data = db.Connection.Query<DropdownList>(sql).ToList();
                return Ok(data);
            }
        }

        [HttpGet("dropdown/shipper")]
        public IActionResult GetShippers()
        {
            using (var db = new AppDb())
            {
                string sql = "select ShipperID as [Value], CompanyName as [Name] from Shippers";
                List<DropdownList> data = db.Connection.Query<DropdownList>(sql).ToList();
                return Ok(data);
            }
        }

    }
}
