using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CMISCoreApi.Model
{
    [Table("ASPSessionState")]
    public class ASPSessionState
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public Guid GUID { get; set; }
        public string SessionKey { get; set; }
        public string SessionValue { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
