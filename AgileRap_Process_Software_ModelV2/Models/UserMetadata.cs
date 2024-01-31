using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgileRap_Process_Software_ModelV2.Models
{
    public class UserMetadata
    {
        
        [Required(ErrorMessage ="กรุณากรอก Nick name.")]
        [DisplayName(displayName:"Nick Name")]
        public string? Name { get; set; }

        [Required(ErrorMessage ="กรุณากรอก Email.")]
        [DataType(DataType.EmailAddress)]
        [DisplayName(displayName:"Email")]
        public string Email { get; set; }

        [Required(ErrorMessage ="กรุณากรอก Password.")]
        [DataType(DataType.Password)]
        [DisplayName(displayName:"Password")]
        public string? Password { get; set; }
    }

    [ModelMetadataType(typeof(UserMetadata))]
    public partial class User
    {
        [Required(ErrorMessage ="กรุณากรอก Confirm Password.")]
        [NotMapped, DataType(DataType.Password)]
        [DisplayName(displayName:"Confirm Password")]
        public string ConfirmPassword { get; set; }
        
    }
}
