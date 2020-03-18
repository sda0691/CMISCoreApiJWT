using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMISCoreApi.Model
{
    [Table("COMPANY_PROFILE")]
    public class Company
    {
        public int CompanyID { get; set; }
        public string CompanyName { get; set; }
        public string ClientOf { get; set; }

    }
}
