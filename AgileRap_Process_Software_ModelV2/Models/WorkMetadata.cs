using AgileRap_Process_Software_ModelV2.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared;
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
        public string? DueDateText
        {
            get
            {
                if (DueDate != null)
                {
                    return DueDate.Value.Date.ToString("dd/MM/yyyy");
                }
                else { return null; }
            }
        }

        [NotMapped]
        [DisplayName("Assign To/Provider")]
        public string? ProviderList { get; set; }


        public void Insert(AgileRap_Process_Software_Context db)
        {
            this.Provider = new List<Provider>();

            //ตัวจัดการ dropdown multi checkbox
            if (this.IsSelectAll)
            {
                foreach (var item in db.User.ToList())
                {
                    Provider provider = new Provider();
                    provider.WorkID = this.ID;
                    provider.UserID = item.ID;
                    provider.Insert(db);
                    this.Provider.Add(provider);
                }
            }
            else
            {
                if (this.ProviderIDs != null)
                {
                    foreach (var item in this.ProviderIDs.Split(','))
                    {
                        Provider provider = new Provider();
                        provider.WorkID = this.ID;
                        provider.UserID = int.Parse(item);
                        provider.Insert(db);
                        this.Provider.Add(provider);
                    }

                }
            }
            db.Work.Add(this);
            //*************************************************//
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
                    provider.UserID = item.ID;
                    provider.WorkID = this.ID;
                    provider.Insert(db);

                    ProviderLog providerLog = new ProviderLog();
                    providerLog.UserID = item.ID;
                    providerLog.WorkLogID = workLog.ID;
                    providerLog.Insert(db);
                    db.SaveChanges();
                }
                // ทำ List strings ให้ว่าง
                strings = new List<string>();
            }

            //กรณีเลือก provider มาบางส่วน
            foreach (string b in strings)
            {
                bool isvalid = false;
                foreach (var item in this.Provider)
                {
                    if (b == item.UserID.ToString())
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
                    provider.UserID = int.Parse(b);
                    provider.WorkID = this.ID;
                    provider.Insert(db);

                }
                ProviderLog providerLog = new ProviderLog();
                providerLog.UserID = int.Parse(b);
                providerLog.WorkLogID = workLog.ID;
                providerLog.Insert(db);
                db.SaveChanges();
            }
            //บันทึกการเปลี่ยนแปลงลง Database
            db.SaveChanges();
        }

    }
}
