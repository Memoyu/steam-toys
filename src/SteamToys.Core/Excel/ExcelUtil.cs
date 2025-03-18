using SteamToys.Core.Excel.Attributes;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace SteamToys.Core.Excel;

public class ExcelUtil
{
    public static void ExportReport<T>(IEnumerable<T> datas, string path, string fileName = "")
    {
        // 将 Excel 工作簿保存到文件
        if (string.IsNullOrEmpty(path)) throw new Exception("path is not null or empty");
        if (string.IsNullOrEmpty(fileName)) 
        {
            fileName = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
        }
        path = Path.Combine(path, $"{fileName}.xlsx");

        // 创建一个新的 Excel 工作簿
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        ExcelPackage package = new ExcelPackage();

        // 添加一个工作表
        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");

        // 获取实体表头
        var headerOptions = new List<ExcelHeaderOption>();
        var type = datas.FirstOrDefault()?.GetType() ?? throw new Exception("datas is not null");
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

        foreach (PropertyInfo property in properties)
        {
            var attr = property.GetCustomAttributes(typeof(HeaderCellAttribute), true)?.FirstOrDefault();
            if (attr == null) continue;
            var headerCellAttr = (HeaderCellAttribute)attr;
            var order = headerCellAttr.Order;
            if (order == 0)
            {
                var max = headerOptions.Any() ? headerOptions.Max(x => x.Order) : 0;
                order = max + 1;
            }
            var title = string.IsNullOrWhiteSpace(headerCellAttr.Title) ? property.Name : headerCellAttr.Title;
            headerOptions.Add(new ExcelHeaderOption { Order = order, Title = title, PropertyInfo = property });
        }

        // 添加表头
        headerOptions = headerOptions.OrderBy(h => h.Order).ToList();
        for (int i = 1; i <= headerOptions.Count; i++)
        {
            worksheet.Cells[1, i].Value = headerOptions[i - 1].Title;
        }

        // 添加数据
        int row = 2;
        foreach (var item in datas)
        {
            for (int i = 1; i <= headerOptions.Count; i++)
            {
                worksheet.Cells[row, i].Value = headerOptions[i - 1].PropertyInfo.GetValue(item);
            }
            row++;
        }

        FileInfo file = new FileInfo(path);
        package.SaveAs(file);
    }

    public static void AddToReport<T>(IEnumerable<T> datas, string filePath = "")
    {
        //// 将 Excel 工作簿保存到文件
        //if (string.IsNullOrEmpty(filePath))
        //    filePath = Path.Combine(Environment.CurrentDirectory, "Succeed.xlsx");
        //else
        //    filePath = Path.Combine(filePath, "Succeed.xlsx");

        //// 创建一个新的 Excel 工作簿
        //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //FileInfo existingFile = new FileInfo(filePath);

        //using (ExcelPackage package = new ExcelPackage(existingFile))
        //{
        //    // 获取 Excel 文件中的工作表
        //    ExcelWorksheet worksheet = package.Workbook.Worksheets["Sheet1"];

        //    // 获取工作表中最后一行的行数
        //    int lastRowNumber = worksheet.Dimension.End.Row;

        //    // 添加新行的数据
        //    int newRowNumber = lastRowNumber + 1;
        //    foreach (var item in datas)
        //    {
        //        worksheet.Cells[newRowNumber, 1].Value = item.Id;
        //        worksheet.Cells[newRowNumber, 2].Value = item.Steam;
        //        worksheet.Cells[newRowNumber, 3].Value = item.SteamPassword;
        //        worksheet.Cells[newRowNumber, 4].Value = item.Email;
        //        worksheet.Cells[newRowNumber, 5].Value = item.EmailPassword;
        //        worksheet.Cells[newRowNumber, 6].Value = item.SmsPlatform;
        //        worksheet.Cells[newRowNumber, 7].Value = item.PhoneNumber;
        //        worksheet.Cells[newRowNumber, 8].Value = item.Captcha;
        //        worksheet.Cells[newRowNumber, 9].Value = item.RecoverCode ?? "";
        //        worksheet.Cells[newRowNumber, 10].Value = item.QuoteUrl;
        //        worksheet.Cells[newRowNumber, 11].Value = item.PrivacyInventory ?? "默认";
        //        worksheet.Cells[newRowNumber, 12].Value = item.BindStatus;
        //        worksheet.Cells[newRowNumber, 13].Value = item.BindDateTime;
        //        worksheet.Cells[newRowNumber, 14].Value = item.ErrMessage;

        //        newRowNumber++;
        //    }

        //    // 保存 Excel 文件
        //    package.Save();
        //}
    }
}

class ExcelHeaderOption
{
    public int Order { get; set; }

    public string Title { get; set; }

    public PropertyInfo PropertyInfo { get; set; }
}

