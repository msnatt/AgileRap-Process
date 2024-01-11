namespace AgileRap_Process_Software_ModelV2.Models
{
    public partial class Status
    {
        public Status()
        {

        }
        public int? ID { get; set; }
        public string? StatusName { get; set; }
        public int? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool IsDelete { get; set; }

        //Childen
        public virtual ICollection<Work>? Work { get; set; }
        public virtual ICollection<WorkLog>? WorkLog { get; set; }

    }
}
