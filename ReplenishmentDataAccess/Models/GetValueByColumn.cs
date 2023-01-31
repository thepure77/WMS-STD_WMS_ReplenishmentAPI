using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models.Utils
{
    public partial class GetValueByColumn
    {
        [Key]
        [StringLength(2000)]
        public string Dataincolumn1 { get; set; }
        [StringLength(2000)]
        public string Dataincolumn2 { get; set; }
        [StringLength(2000)]
        public string Dataincolumn3 { get; set; }
        [StringLength(2000)]
        public string Dataincolumn4 { get; set; }
        [StringLength(2000)]
        public string Dataincolumn5 { get; set; }

    }
}
