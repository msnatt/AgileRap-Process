using AgileRap_Process_Software_ModelV2.Data;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgileRap_Process_Software_ModelV2.Models
{
    public class WorkLogMetadata
    {
    }
    [ModelMetadataType(typeof(WorkLogMetadata))]
    public partial class WorkLog
    {
        [NotMapped]
        public string? Description { get; set; }
        [NotMapped]
        public string? ProviderLogList { get; set; }
        public void Insert(AgileRap_Process_Software_Context db, Work work)
        {
            this.WorkID = work.ID;
            this.Work = work;
            this.CreateDate = work.CreateDate;
            this.UpdateDate = work.UpdateDate;
            this.CreateBy = work.CreateBy;
            this.UpdateBy = work.UpdateBy;
            this.Remark = work.Remark;
            this.DueDate = work.DueDate;
            this.Name = work.Name;
            this.Project = work.Project;
            this.StatusID = work.StatusID;
            this.Status = work.Status;
            WorkLog rawLog = new WorkLog();
            try
            {
                rawLog = db.WorkLog.Where(b => b.WorkID == work.ID).LastOrDefault();
            }
            catch
            {
                rawLog = null;
            }
            if (rawLog == null)
            {
                this.No = 1;
            }
            else
            {
                this.No = rawLog.No + 1;
            }
            db.WorkLog.Add(this);
        }
    }
}
