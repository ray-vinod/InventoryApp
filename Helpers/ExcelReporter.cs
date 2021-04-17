using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace InventoryApp.Helpers
{
    public static class ExcelReporter<TEntity> where TEntity : class
    {
        private static MemberInfo[] memberInfos;

        public static async Task<byte[]> GetReports(List<TEntity> entities, string wrkSheetName)
        {
            await Task.Delay(0);
            byte[] fileContent = null;

            if (entities.Count > 0)
            {
                Type typeOfItem = entities[0].GetType();
                memberInfos = typeOfItem.GetMembers();
                int memberCount = typeOfItem.GetProperties().Length; //Properties count
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage xlpkg = new ExcelPackage();
                var worksheet = xlpkg.Workbook.Worksheets.Add(wrkSheetName);
                worksheet.Cells[3, 1].LoadFromCollection(entities, true); //[row,cell]

                //Title setup
                using (ExcelRange title = worksheet.Cells[1, 1, 1, memberCount])
                {
                    title.Style.Font.Size = 18;
                    title.Style.Font.Bold = true;
                    title.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    title.Merge = true;
                    title.Value = "CIWEC Hospital OT Inventory";
                    title.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    var fill = title.Style.Fill;
                    fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    fill.BackgroundColor.SetColor(System.Drawing.Color.LightCyan);

                }

                //Date setup
                worksheet.Cells[2, memberCount - 1].Style.Font.Bold = true;
                worksheet.Cells[2, memberCount - 1].Value = "Date : ";
                worksheet.Cells[2, memberCount - 1].Style.Font.Size = 14;
                worksheet.Cells[2, memberCount].Style.Font.Size = 14;
                worksheet.Cells[2, memberCount].Style.Font.Bold = true;
                worksheet.Cells[2, memberCount].Value = DateTime.Now.Date.ToShortDateString();

                //Worksheet header style
                using (ExcelRange header = worksheet.Cells[3, 1, 3, memberCount])
                {
                    header.Style.Font.Bold = true;
                    header.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    var fill = header.Style.Fill;
                    fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                //Cell autofill property & data type formating
                worksheet.Cells.AutoFitColumns();
                int index = 1;
                foreach (var member in memberInfos)
                {
                    if (member.MemberType == MemberTypes.Property)
                    {
                        if (member.Name.Contains("Date"))
                        {
                            worksheet.Column(index).Style.Numberformat.Format = "yyyy-mm-dd";
                        }

                        index++;
                    }
                }

                fileContent = xlpkg.GetAsByteArray();
            }

            return fileContent;
        }

    }
}
