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
        public bool IsFound { get; set; }

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

            //นำ Work ปัจจุบันไปเก็บไว้เป็น Log
            WorkLog workLog = new WorkLog();
            workLog.Insert(db, this);

            //ProviderIDs ที่ join กันมาเป็น string มาแบ่งออกเก็บไว้ใน List strings
            List<string> strings = new List<string>();
                if (this.ProviderIDs != null) { strings = this.ProviderIDs.Split(',').ToList(); }

                //กรณีไม่ได้เลือก provider เข้ามาเลย
                if (strings.Count == 0 && this.IsSelectAll == false)
                {
                    foreach (var item in this.Provider)
                    {
                        db.Provider.Remove(item);
                    }
                }
                //กรณีเลือก provider ทั้งหมด
                else if (this.IsSelectAll)
                {
                    foreach (var item in this.Provider)
                    {
                        db.Provider.Remove(item);
                    }
                    foreach (var item in db.User.ToList())
                    {
                        Provider provider = new Provider();
                        provider.Insert(db, this, item.ID);

                        ProviderLog providerLog = new ProviderLog();
                        providerLog.Insert(db, workLog, item.ID);
                        db.SaveChanges();
                    }

                    // ทำ List strings ให้ว่าง
                    strings = new List<string>();
                }

                //กรณีเลือก providerมาบางส่วน
                foreach (string s in strings)
                {
                    bool isvalid = false;
                    foreach (var item in this.Provider)
                    {
                        if (s == item.UserID.ToString())
                        {
                            isvalid = true;
                        }
                        if (!strings.Contains(item.UserID.ToString()))
                        {
                            db.Provider.Remove(item);
                        }
                    }
                    if (!isvalid)
                    {
                        Provider provider = new Provider();
                        provider.Insert(db, this, int.Parse(s));
                    }
                    ProviderLog providerLog = new ProviderLog();
                    providerLog.Insert(db, workLog, int.Parse(s));
                    db.SaveChanges();
                }
                //บันทึกการเปลี่ยนแปลงลง Database
                db.SaveChanges();
        }

    }
}
