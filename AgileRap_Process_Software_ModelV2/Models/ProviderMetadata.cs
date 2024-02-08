using AgileRap_Process_Software_ModelV2.Data;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AgileRap_Process_Software_ModelV2.Models
{
    public class ProviderMetadata
    {
    }
    [ModelMetadataType(typeof(ProviderMetadata))]
    public partial class Provider
    {
        public void Insert(AgileRap_Process_Software_Context db)
        {
            this.CreateDate = DateTime.Now;
            this.UpdateDate = DateTime.Now;
            this.CreateBy = GlobalVariable.GetUserLogin();
            this.UpdateBy = GlobalVariable.GetUserLogin();
            this.IsDelete = false;

            db.Provider.Add(this);
        }
    }
}
