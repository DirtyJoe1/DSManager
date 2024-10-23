using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager.Model
{
    public static class Repository
    {
        public static string ExcelFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Files", "data.xlsx");
    }
}
