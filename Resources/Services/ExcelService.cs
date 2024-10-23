using DSManager.Model;
using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace DSManager.Resources.Services
{
    public static class ExcelService
    {
        public static DataTable GetDataTableFromDataGrid<T>(IEnumerable list)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
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
        public static IEnumerable<DataModel> ReadExcelFile(string filePath)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage(filePath);
            var worksheet = package.Workbook.Worksheets[0];
            for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
            {
                yield return new DataModel
                {
                    Id = row,
                    FIO = worksheet.Cells[row, 2].Value.ToString(),
                    Department = worksheet.Cells[row, 3].Value.ToString(),
                    Setup = DateTime.Parse(worksheet.Cells[row, 4].Value.ToString()),
                    Start = DateTime.Parse(worksheet.Cells[row, 5].Value.ToString()),
                    End = DateTime.Parse(worksheet.Cells[row, 6].Value.ToString()),
                    Status = Enum.Parse<Statuses>(worksheet.Cells[row, 7].Value.ToString())
                };
            }
        }
        public static IEnumerable<string> ReadDepartments(string filePath)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage(filePath);
            var worksheet = package.Workbook.Worksheets[1];
            for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
            {
                yield return worksheet.Cells[row, 1].Value.ToString();
            }
        }
        public static int GetEntriesCount(string filePath)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage(filePath);
            var worksheet = package.Workbook.Worksheets[0];
            return worksheet.Dimension.End.Row;
        }
        public static void SaveDataGrid(ObservableCollection<DataModel> data, string filePath, bool backup)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage(filePath);
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
            if (backup)
                package.SaveAs(filePath.Replace(".xlsx", "_temp.xlsx"));
        }
        public static bool ValidateDates(DateTime? startField, DateTime? setupField, DateTime? endField)
        {
            return (
                startField.HasValue && !startField.Value.Equals(DateTime.MinValue) &&
                setupField.HasValue && !setupField.Value.Equals(DateTime.MinValue) &&
                endField.HasValue && !endField.Value.Equals(DateTime.MinValue) &&
                (setupField.Value > startField.Value) && (setupField.Value < endField.Value) && (startField.Value < endField.Value)
            );
        }
        public static void AddRow(string filePath, string fio, string otdel, DateTime? setup, DateTime? start, DateTime? end, object status)
        {
            if (string.IsNullOrEmpty(fio) || string.IsNullOrEmpty(otdel) || (status == null))
            {
                MessageBox.Show("ФИО, Отделение/Подразделение и/или Cтатус пусты!");
                return;
            }
            if (!ValidateDates(start, setup, end))
            {
                MessageBox.Show("Одно или несколько полей дат пусты или имеют невозможные значения");
                return;
            }
            AddData(filePath, fio, otdel, setup, start, end, status);
            MessageBox.Show("Запись создана");
        }
        public static void AddData(string filePath, string fio, string otdel, DateTime? setup, DateTime? start, DateTime? end, object status)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage(filePath);
            var worksheet = package.Workbook.Worksheets[0];
            var lastrow = worksheet.Dimension.Rows + 1;
            worksheet.Cells[lastrow, 1].Value = lastrow;
            worksheet.Cells[lastrow, 2].Value = fio;
            worksheet.Cells[lastrow, 3].Value = otdel;
            worksheet.Cells[lastrow, 4].Value = $"{start:dd.MM.yyyy}";
            worksheet.Cells[lastrow, 5].Value = $"{end:dd.MM.yyyy}";
            worksheet.Cells[lastrow, 6].Value = $"{setup:dd.MM.yyyy}";
            worksheet.Cells[lastrow, 7].Value = status.ToString();
            package.Save();
        }
        public static void DeleteRowFromExcelFile(string filePath, int rowIndex)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage(filePath);
            var worksheet = package.Workbook.Worksheets[0];
            worksheet.DeleteRow(rowIndex);
            package.Save();
        }
        public static void AppendDataFromExcel(string sourceFilePath, string targetFilePath)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var sourcePackage = new ExcelPackage(sourceFilePath);
            using var targetPackage = new ExcelPackage(targetFilePath);
            var sourceWorksheet = sourcePackage.Workbook.Worksheets[0];
            var targetWorksheet = targetPackage.Workbook.Worksheets[0];
            var sourceData = ReadExcelFile(sourceFilePath).ToList();
            int maxRows = targetWorksheet.Dimension.End.Row;
            for (int i = 1; i <= sourceData.Count; i++)
            {
                var rowData = sourceData[i - 1];
                targetWorksheet.Cells[maxRows + i, 1].Value = maxRows + i;
                targetWorksheet.Cells[maxRows + i, 2].Value = rowData.FIO;
                targetWorksheet.Cells[maxRows + i, 3].Value = rowData.Department;
                targetWorksheet.Cells[maxRows + i, 4].Value = rowData.Setup.ToString();
                targetWorksheet.Cells[maxRows + i, 5].Value = rowData.Start.ToString();
                targetWorksheet.Cells[maxRows + i, 6].Value = rowData.End.ToString();
                targetWorksheet.Cells[maxRows + i, 7].Value = rowData.Status.ToString();
            }
            targetPackage.Save();
            sourcePackage.Dispose();
        }
        public static void ExportFile(string path)
        {
            File.Copy(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Files", "data.xlsx"), Path.Combine(path, "export.xlsx"), true);
        }
    }
}
