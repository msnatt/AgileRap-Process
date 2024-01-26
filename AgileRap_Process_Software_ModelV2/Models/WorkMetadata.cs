using AgileRap_Process_Software_ModelV2.Data;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgileRap_Process_Software_ModelV2.Models
{
    public class WorkMetadata
    {
        [Required]
        public int StatusID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CreateDate { get; set; }

        [DataType(DataType.Date)]
        [Required]
        public DateTime? DueDate { get; set; }
    }
    [ModelMetadataType(typeof(WorkMetadata))]
    public partial class Work
    {
        [NotMapped]
        public int UserLoginID { get; set; }
        public string? CreateDateText { get { return CreateDate.Value.ToString("d"); } }
        public string? DueDateText { get { return DueDate.Value.ToString("d"); } }
        [NotMapped]
        public string? ProviderIDs { get; set; }
        [NotMapped]
        public bool IsSelectAll { get; set; }

        [NotMapped]
        [DisplayName("Assign To/Provider")]
        public string? ProviderList { get; set; }
        public void Insert(AgileRap_Process_Software_Context db)
        {
            this.CreateDate = DateTime.Now;
            this.UpdateDate = DateTime.Now;
            this.IsDelete = false;
            this.IsSelectAll = false;
            db.Work.Add(this);
        }
        public void Update(AgileRap_Process_Software_Context db)
        {
            this.CreateDate = DateTime.Now;
            this.UpdateDate = DateTime.Now;
            this.IsDelete = false;
            db.Work.Add(this);
        }

    }
}
