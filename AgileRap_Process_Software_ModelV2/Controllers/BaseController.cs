using AgileRap_Process_Software_ModelV2.Data;
using Microsoft.AspNetCore.Mvc;

namespace AgileRap_Process_Software_ModelV2.Controllers
{
    public class BaseController : Controller
    {
        public AgileRap_Process_Software_Context db = new AgileRap_Process_Software_Context();
    }
}
