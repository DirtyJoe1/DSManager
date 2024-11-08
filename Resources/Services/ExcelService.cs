using DSManager.Model;
using OfficeOpenXml;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Windows;

namespace DSManager.Resources.Services
{
    public static class ExcelService
    {
        public static readonly string ExcelFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Files", "data.xlsx");
        //Не спрашивай, как этот метод работает, одному богу известно
        //Он преобразует содержимое DataGrid в DataTable, чтобы потом его записать в файл
        public static DataTable GetDataTableFromDataGrid<T>(IEnumerable list)
        {
            //Важная строчка, без неё прога всегда крашится
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial; 
            //Поэтому, если захочешь сделать новый метод, который работает с Excel файлом, обязательно впиши её
            
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                Type columnType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                if (columnType == typeof(DateTime))
                {
                    table.Columns.Add(prop.Name, typeof(string));
                }
                else if (columnType == typeof(Statuses))
                {
                    table.Columns.Add(prop.Name, typeof(string));
                }
                else
                {
                    table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(columnType) ?? columnType);
                }
            }
            object[] values = new object[props.Count];
            foreach (T item in list)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item) ?? DBNull.Value;
                }
                table.Rows.Add(values);
            }
            return table;
        }
        //Метод, чтобы читать пользователей, которые в первом листе файла
        public static async IAsyncEnumerable<DataModel> ReadExcelFile(string filePath)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage(filePath);
            var worksheet = package.Workbook.Worksheets[0];
            if (worksheet.Dimension != null)
            {
                for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
                {
                    await Task.Yield();
                    string endValue = worksheet.Cells[row, 5].Value?.ToString();
                    DateTime? endDate = DateTime.TryParse(endValue, out DateTime parsedEndDate) ? parsedEndDate : null;
                    string startValue = worksheet.Cells[row, 4].Value?.ToString();
                    DateTime? startDate = DateTime.TryParse(startValue, out DateTime parsedStartDate) ? parsedStartDate : null;
                    string setupValue = worksheet.Cells[row, 6].Value?.ToString();
                    DateTime? setupDate = DateTime.TryParse(setupValue, out DateTime parsedSetupDate) ? parsedSetupDate : null;
                    Statuses status;
                    if (endDate.HasValue && endDate <= DateTime.Now)
                    {
                        status = Statuses.Закончился;//Если получили null или дата прошла при чтении даты окончания, то считаем, что статус Закончился
                    }
                    else
                    {
                        if (Enum.TryParse<Statuses>(worksheet.Cells[row, 7].Value?.ToString(), out var parsedStatus))
                        {
                            status = parsedStatus;
                        }
                        else
                        {
                            status = Statuses.Подан; //Если получили null при чтении статуса, то считаем, что статус Подан
                        }
                    }

                    yield return new DataModel
                    {
                        Id = row,
                        FIO = worksheet.Cells[row, 2].Value?.ToString(),
                        Department = worksheet.Cells[row, 3].Value?.ToString(),
                        Start = startDate,
                        End = endDate,
                        Setup = setupDate,
                        Status = status
                    };
                }
            }
            else
            {
                yield break;
            }
            
        }
        //Метод чтобы читать отделения/подразделения, которые во втором листе файла
        public static IEnumerable<string> ReadDepartments() 
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage(ExcelFilePath);
            var worksheet = package.Workbook.Worksheets[1];
            if (worksheet.Dimension != null)
            {
                for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
                {
                    yield return worksheet.Cells[row, 1].Value.ToString();
                }
            }
            else
            {
                yield break;
            }
        }
        //Тут добавляются отделения/подразделения
        public static void AddDepartment(string department)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage(ExcelFilePath);
            var worksheet = package.Workbook.Worksheets[1];
            if (worksheet.Dimension != null)
            {
                worksheet.Cells[worksheet.Dimension.End.Row + 1, 1].Value = department;
            }
            else
            { 
                worksheet.Cells[1, 1].Value = department;
            }
            package.Save();
        }
        //Думал, что может где-то пригодится, пусть будет
        //Считает количество строк в файле, но есть удобный worksheet.Dimension.End.Row
        //Так что оказался без надобности
        public static int GetRowCount()
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage(ExcelFilePath);
            var worksheet = package.Workbook.Worksheets[0];
            return worksheet.Dimension.End.Row;
        }
        //Метод сохранения DataGrid в файл 
        public static void SaveDataGrid(ObservableCollection<DataModel> data, bool backup)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage(ExcelFilePath);
            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = 1;
            foreach (var item in data)
            {
                worksheet.Cells[rowCount, 1].Value = item.Id;
                worksheet.Cells[rowCount, 2].Value = item.FIO;
                worksheet.Cells[rowCount, 3].Value = item.Department;
                worksheet.Cells[rowCount, 4].Value = $"{item.Start:dd.MM.yyyy}";
                worksheet.Cells[rowCount, 5].Value = $"{item.End:dd.MM.yyyy}";
                worksheet.Cells[rowCount, 6].Value = $"{item.Setup:dd.MM.yyyy}";
                worksheet.Cells[rowCount, 7].Value = item.Status.ToString();
                rowCount++;
            }
            package.Save();
            //Штука чтобы сделать резервную копию, она в bin находится, название data_temp.xlsx
            //Создается только при выходе из программы, когда пользователь выбирает "Да" на предложение сохранить изменения
            if (backup)
                package.SaveAs(ExcelFilePath.Replace(".xlsx", "_temp.xlsx"));
        }
        //Метод, который вызывается из ViewModel, тут есть проверки 
        public static void AddRow(string fio, string otdel, DateTime? setup, DateTime? start, DateTime? end, object status)
        {
            if (string.IsNullOrEmpty(fio) || string.IsNullOrEmpty(otdel) || (status == null))
            {
                MessageBox.Show("ФИО, Отделение/Подразделение и/или Cтатус пусты!");
                return;
            }
            if (start == null || setup == null || end == null || setup <= start || setup >= end)
            {
                MessageBox.Show("Одно или несколько полей дат пусты или имеют невозможные значения");
                return;
            }
            AddData(fio, otdel, setup, start, end, status);
        }
        //Простое добавление строки в конец
        public static void AddData(string fio, string otdel, DateTime? setup, DateTime? start, DateTime? end, object status)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage(ExcelFilePath);
            var worksheet = package.Workbook.Worksheets[0];
            int lastrow = 1;
            if (worksheet.Dimension != null)
            {
                lastrow = worksheet.Dimension.Rows + 1;
            }
            worksheet.Cells[lastrow, 1].Value = lastrow;
            worksheet.Cells[lastrow, 2].Value = fio;
            worksheet.Cells[lastrow, 3].Value = otdel;
            worksheet.Cells[lastrow, 4].Value = $"{start:dd.MM.yyyy}";
            worksheet.Cells[lastrow, 5].Value = $"{end:dd.MM.yyyy}";
            worksheet.Cells[lastrow, 6].Value = $"{setup:dd.MM.yyyy}";
            worksheet.Cells[lastrow, 7].Value = status.ToString();
            package.Save();
        }
        //Вставка строки в определенную позицию
        public static void InsertRow(int rowIndex, string fio, string otdel, DateTime? setup, DateTime? start, DateTime? end, object status)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage(ExcelFilePath);
            var worksheet = package.Workbook.Worksheets[0];
            worksheet.InsertRow(rowIndex + 1, 1);
            worksheet.Cells[rowIndex + 1, 1].Value = rowIndex;
            worksheet.Cells[rowIndex + 1, 2].Value = fio;
            worksheet.Cells[rowIndex + 1, 3].Value = otdel;
            worksheet.Cells[rowIndex + 1, 4].Value = $"{start:dd.MM.yyyy}";
            worksheet.Cells[rowIndex + 1, 5].Value = $"{end:dd.MM.yyyy}";
            worksheet.Cells[rowIndex + 1, 6].Value = $"{setup:dd.MM.yyyy}";
            worksheet.Cells[rowIndex + 1, 7].Value = status.ToString();
            package.Save();
        }
        public static void DeleteRowFromExcelFile(int rowIndex, int windex)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage(ExcelFilePath);
            var worksheet = package.Workbook.Worksheets[windex];
            worksheet.DeleteRow(rowIndex);
            package.Save();
        }
        //При импорте нет проверок на адекватность полей :)
        public static async Task AppendDataFromExcel(string sourceFilePath, string targetFilePath)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var sourcePackage = new ExcelPackage(sourceFilePath);
            using var targetPackage = new ExcelPackage(targetFilePath);
            var sourceWorksheet = sourcePackage.Workbook.Worksheets[0];
            var targetWorksheet = targetPackage.Workbook.Worksheets[0];
            var sourceData = await ReadExcelFile(sourceFilePath).ToListAsync();
            int maxRows = 0;
            if (targetWorksheet.Dimension != null)
            {
                maxRows = targetWorksheet.Dimension.End.Row;
            }
            for (int i = 1; i <= sourceData.Count; i++)
            {
                var rowData = sourceData[i - 1];
                targetWorksheet.Cells[maxRows + i, 1].Value = maxRows + i;
                targetWorksheet.Cells[maxRows + i, 2].Value = rowData.FIO;
                targetWorksheet.Cells[maxRows + i, 3].Value = rowData.Department;
                targetWorksheet.Cells[maxRows + i, 4].Value = rowData.Start.ToString();
                targetWorksheet.Cells[maxRows + i, 5].Value = rowData.End.ToString();
                targetWorksheet.Cells[maxRows + i, 6].Value = rowData.Setup.ToString();
                targetWorksheet.Cells[maxRows + i, 7].Value = rowData.Status.ToString();
            }
            await targetPackage.SaveAsync();
            sourcePackage.Dispose();
        }
        public static void ExportFile(string path)
        {
            File.Copy(ExcelFilePath, Path.Combine(path, "export.xlsx"), true);
        }
    }
}
