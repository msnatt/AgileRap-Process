using AgileRap_Process_Software_ModelV2.Data;
using Microsoft.AspNetCore.Mvc;

namespace AgileRap_Process_Software_ModelV2.Models
{
    public class ProviderLogMetadata
    {
    }
    [ModelMetadataType(typeof(ProviderLogMetadata))]
    public partial class ProviderLog
    {

        public void Insert(AgileRap_Process_Software_Context db)
        {
            this.CreateDate = DateTime.Now;
            this.UpdateDate = DateTime.Now;
            this.CreateBy = GlobalVariable.GetUserLogin();
            this.UpdateBy = GlobalVariable.GetUserLogin();
            db.ProviderLog.Add(this);
        }
    }
}
