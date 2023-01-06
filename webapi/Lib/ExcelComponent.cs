using System;
using System.IO;
using System.Data;
using System.Reflection;
using System.Collections.Generic;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System.Linq;

namespace WebAPI.Lib
{
    public class ExcelComponent
    {
        private IWorkbook workbook = null;
        public byte[] export<T>(List<T> items)
        {
            if (this.workbook is null)
            {
                this.workbook = new XSSFWorkbook();
            }
            ISheet sheet = this.workbook.CreateSheet(typeof(T).Name);
            IRow header = sheet.CreateRow(0);
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            int i = 0;
            foreach (PropertyInfo prop in Props)
            {
                ICell cell = header.CreateCell(i++);
                cell.SetCellValue(prop.Name);
            }
            i = 0;
            foreach (var item in items)
            {
                IRow rows = sheet.CreateRow(++i);
                for (int j = 0; j < Props.Length; j++)
                {
                    ICell cell = rows.CreateCell(j);
                    var the_value = Props[j].GetValue(item, null);
                    cell.SetCellValue(the_value is null ? "" : the_value.ToString());
                }
            }
            MemoryStream stream = new MemoryStream();
            this.workbook.Write(stream, false);
            stream.Flush();
            byte[] result = stream.ToArray();
            stream.Close();
            return result;
        }
    }
}
