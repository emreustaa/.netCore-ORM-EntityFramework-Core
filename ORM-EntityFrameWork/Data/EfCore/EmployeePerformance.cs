using System;
using System.Collections.Generic;

namespace ORM_EntityFrameWork.Data.EfCore
{
    public partial class EmployeePerformance
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        public string EMail { get; set; }
        public int? SatisAdeti { get; set; }
    }
}
