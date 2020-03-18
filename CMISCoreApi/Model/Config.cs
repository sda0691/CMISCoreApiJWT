using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMISCoreApi.Model
{
    [Table("tbl_Config")]
    public class Config
    {
        public int ConfigID { get; set; }
        public string Category { get; set; }
        public string ConfigName { get; set; }
        public string ConfigValue { get; set; }
        public string ConfigDesc { get; set; }
    }
}
