using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AgileRap_Process_Software_ModelV2.Data;
using AgileRap_Process_Software_ModelV2.Models;

namespace AgileRap_Process_Software_ModelV2.Controllers
{
    public class WorksController : Controller
    {
        private AgileRap_Process_Software_Context db = new AgileRap_Process_Software_Context();

        public ActionResult Index()
        {
            var Works = GetWork();
            return View(Works);
        }

        public ActionResult Create()
        {
            var Works = GetWork();
            Work work = new Work();
            work.CreateDate = DateTime.Now;
            work.UpdateDate = DateTime.Now;
            work.IsDelete = false;
            work.IsSelectAll = false;
            Works.Add(work);

            ViewBag.StatusBag = db.Status.Select(i => new SelectListItem() { Value = i.ID.ToString(), Text = i.StatusName });
            ViewBag.UserBag = db.User.Select(i => new SelectListItem() { Value = i.ID.ToString(), Text = i.Name });

            return View(Works);
        }

        [HttpPost]
        public ActionResult Create(Work Works)
        {
            if ((bool)Works.IsSelectAll)
            {
                foreach (var item in db.User.ToList())
                {
                    Provider provider = new Provider();
                    provider.CreateDate = DateTime.Now;
                    provider.UpdateDate = DateTime.Now;
                    provider.User = item;
                    provider.UserID = item.ID;
                    provider.IsDelete = false;
                    provider.Work = Works;
                    provider.WorkID = Works.ID;
                    provider.CreateBy = Works.CreateBy;
                    provider.UpdateBy = Works.UpdateBy;
                    Works.Provider.Add(provider);
                }

            }
            else
            {
                foreach (var item in Works.ProviderIDs.Split(','))
                {
                    Provider provider = new Provider();
                    provider.CreateDate = DateTime.Now;
                    provider.UpdateDate = DateTime.Now;
                    provider.User = db.User.Find(item);
                    provider.UserID = db.User.Find(item).ID;
                    provider.IsDelete = false;
                    provider.Work = Works;
                    provider.WorkID = Works.ID;
                    provider.CreateBy = Works.CreateBy;
                    provider.UpdateBy = Works.UpdateBy;
                    Works.Provider.Add(provider);
                }
            }




            return View(Works);
        }

        public List<Work> GetWork()
        {
            var Works = db.Work.Include(m => m.Status).Include(b => b.Provider).Include(h => h.WorkLog).ToList();
            foreach (var item in Works)
            {
                foreach (var item2 in item.Provider)
                {
                    item2.User = db.User.Find(item2.UserID);
                }
            }
            return Works;
        }
    }
}
