﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgileRap_Process_Software_ModelV2.Models
{
    public class WorkLogMetadata
    {
    }
    [MetadataType(typeof(WorkLogMetadata))]
    public partial class WorkLog
    {
        [NotMapped]
        public string? Description { get; set; }
    }
}