using AgileRap_Process_Software_ModelV2.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgileRap_Process_Software_ModelV2.Models
{
    public class WorkMetadata
    {
        public string? Project { get; set; }
        public string? Name { get; set; }

        [Required]
        [DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [DisplayName("Due Date")]
        public DateTime? DueDate { get; set; }

        [Required]
        [DisplayName("Status")]
        public int StatusID { get; set; }
        public string? Remark { get; set; }

        [DisplayName("Assign By/ Requester")]
        public int? CreateBy { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [DisplayName("Create Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CreateDate { get; set; }
        public int? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }

    }
    [ModelMetadataType(typeof(WorkMetadata))]
    public partial class Work
    {
        [NotMapped]
        public string? ProviderIDs { get; set; }
        [NotMapped]
        public bool IsSelectAll { get; set; }

        [NotMapped]
        [DisplayName("Assign To/Provider")]
        public string? ProviderList { get; set; }
        public void Insert()
        {
            this.CreateDate = DateTime.Now;
            this.UpdateDate = DateTime.Now;
            this.IsDelete = false;
            this.IsSelectAll = false;
            this.CreateBy = GlobalVariable.GetUserLogin();
            this.StatusID = 1;
        }
        public void Update(AgileRap_Process_Software_Context db)
        {
            this.UpdateDate = DateTime.Now;
            this.UpdateBy = GlobalVariable.GetUserLogin();
            db.Entry(this).State = EntityState.Modified;
        }

    }
}
