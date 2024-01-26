using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgileRap_Process_Software_ModelV2.Models
{
    public class UserMetadata
    {
        
        [DisplayName(displayName:"Nick Name")]
        public string? Name { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [DisplayName(displayName:"Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [DisplayName(displayName:"Password")]
        public string? Password { get; set; }
    }

    [ModelMetadataType(typeof(UserMetadata))]
    public partial class User
    {
        [Required, NotMapped, DataType(DataType.Password)]
        [DisplayName(displayName:"Confirm Password")]
        public string ConfirmPassword { get; set; }
        
    }
}
