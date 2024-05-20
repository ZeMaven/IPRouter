using Momo.Common.Actions;
using OfficeOpenXml;
using System.ComponentModel;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace Jobs.Actions
{
    public interface IExcel
    {
        Task Write<T>(List<T> ReportCollection, string SheetName, string FileName, string Path);
        byte[] Write<T>(List<T> ReportCollection, string SheetName);
    }

    public class Excel : IExcel
    {
        private readonly ILog Log;
        public Excel(ILog log)
        {
            Log = log;
        }

        public byte[] Filebytes { get; set; }
        public async Task Write<T>(List<T> ReportCollection, string SheetName, string FileName, string Path)
        {
            try
            {
                //  Log.Write("Excel.Write", $" Starting to create excel sheet:{SheetName}");
                var ReportPath = Path;
                if (!Directory.Exists(ReportPath)) Directory.CreateDirectory(ReportPath);

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                var MyFile = new FileInfo($"{ReportPath}/{FileName}.xlsx");
                if (MyFile.Exists) MyFile.Delete();
                var Package = new ExcelPackage(MyFile);
                var WorkSheet = Package.Workbook.Worksheets.Add(SheetName);
                var SheetRange = WorkSheet.Cells["A1"].LoadFromCollection(ReportCollection, true);


                WorkSheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                WorkSheet.Row(1).Style.Font.Bold = true;
                SheetRange.AutoFitColumns();
                await Package.SaveAsync();
                Filebytes = await Package.GetAsByteArrayAsync();
                Log.Write("Excel.Write", $" finished creating excel sheet:{SheetName}");
            }
            catch (Exception Ex)
            {
                Log.Write($"Excel.Write", $"Err: {Ex.Message}");
            }
        }



        public byte[] Write<T>(List<T> ReportCollection, string SheetName)
        {
            try
            {

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var Package = new ExcelPackage())
                {
                    var WorkSheet = Package.Workbook.Worksheets.Add(SheetName);
                    var SheetRange = WorkSheet.Cells["A1"].LoadFromCollection(ReportCollection, true);

                    WorkSheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    WorkSheet.Row(1).Style.Font.Bold = true;
                    SheetRange.AutoFitColumns();
                    Package.Save();
                    Filebytes = Package.GetAsByteArray();
                }

                Log.Write("Excel.Write", $" finished creating excel sheet:{SheetName}");
                return Filebytes;
            }
            catch (Exception Ex)
            {
                Log.Write($"Excel.Write", $"Err: {Ex.Message}");
                return null;
            }
        }


    }
}
