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
        public string? ProviderList
        {
            get
            {
                if (Provider.Count > 0)
                {
                    if (Provider.First().User != null)
                    {
                        var temp = Provider.First().User.Name;
                        foreach (var item in Provider.Where(m => m.ID != Provider.First().ID))
                        {
                            temp += temp + ", " + item.User.Name;
                        }
                        return temp;
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {

                    return "";
                }
            }
            set => ProviderList = value;
        }

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
