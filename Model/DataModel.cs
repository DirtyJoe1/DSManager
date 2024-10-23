using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager.Model
{
    public class DataModel
    {
        public int Id { get; set; }
        public string FIO { get; set; }
        public string Department { get; set; }
        public DateTime Setup { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public Statuses Status { get; set; }
    }
}
