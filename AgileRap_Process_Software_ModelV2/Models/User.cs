namespace AgileRap_Process_Software_ModelV2.Models
{
    public partial class User
    {
        public User()
        {
            this.Provider = new HashSet<Provider>();
            this.ProviderLog = new HashSet<ProviderLog>();
        }

        public int ID { get; set; }
        public string? Name { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? LineID { get; set; }
        public string? Role { get; set; }
        public int? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool IsDelete { get; set; }


        //Childen
        public virtual ICollection<Provider> Provider { get; set; }
        public virtual ICollection<ProviderLog> ProviderLog { get; set; }

    }
}
