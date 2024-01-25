using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgileRap_Process_Software_ModelV2.Models
{
    public class UserMetadata
    {
        [Required, DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string? Password { get; set; }
    }

    [ModelMetadataType(typeof(UserMetadata))]
    public partial class User
    {
        [Required, NotMapped, DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        
    }
}
