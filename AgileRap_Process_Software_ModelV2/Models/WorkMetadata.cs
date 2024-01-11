using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgileRap_Process_Software_ModelV2.Models
{
    public class WorkMetadata
    {
        [Required]
        public int StatusID { get; set; }


    }
    [MetadataType(typeof(WorkMetadata))]
    public partial class Work
    {
        [NotMapped]
        public string? ProviderIDs { get; set; }
        [NotMapped]
        public bool? IsSelectAll { get; set; }

        [DisplayName("Assign To/Provider")]
        public string? ProviderList
        {
            get
            {
                if (Provider.Count > 0)
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
                    return null;
                }

            }
        }

    }
}
