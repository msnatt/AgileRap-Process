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
        public ActionResult Create(Work Work)
        {
            if ((bool)Work.IsSelectAll)
            {
                foreach (var item in db.User.ToList())
                {
                    Provider provider = new Provider();
                    provider.CreateDate = DateTime.Now;
                    provider.UpdateDate = DateTime.Now;
                    provider.User = item;
                    provider.UserID = item.ID;
                    provider.IsDelete = false;
                    provider.Work = Work;
                    provider.WorkID = Work.ID;
                    provider.CreateBy = Work.CreateBy;
                    provider.UpdateBy = Work.UpdateBy;
                    Work.Provider.Add(provider);
                }
            }
            else
            {
                foreach (var item in Work.ProviderIDs.Split(','))
                {
                    Provider provider = new Provider();
                    provider.CreateDate = DateTime.Now;
                    provider.UpdateDate = DateTime.Now;
                    provider.User = db.User.Find(int.Parse(item));
                    provider.UserID = db.User.Find(int.Parse(item)).ID;
                    provider.IsDelete = false;
                    provider.Work = Work;
                    provider.WorkID = Work.ID;
                    provider.CreateBy = Work.CreateBy;
                    provider.UpdateBy = Work.UpdateBy;
                    Work.Provider.Add(provider);
                }
            }
            db.Work.Add(Work);
            db.SaveChanges();

            WorkLog workLog = new WorkLog();
            workLog.WorkID = Work.ID;
            workLog.Work = Work;
            workLog.CreateDate = Work.CreateDate;
            workLog.UpdateDate = Work.UpdateDate;
            workLog.CreateBy = Work.CreateBy;
            workLog.UpdateBy = Work.UpdateBy;
            workLog.Remark = Work.Remark;
            workLog.DueDate = Work.DueDate;
            workLog.Name = Work.Name;
            workLog.Project = Work.Project;
            workLog.StatusID = Work.StatusID;
            workLog.Status = Work.Status;
            WorkLog rawLog = new WorkLog();
            try
            {
                rawLog = db.WorkLog.Where(b => b.WorkID == Work.ID).LastOrDefault();
            }
            catch
            {
                rawLog = null;
            }
            if (rawLog == null)
            {
                workLog.No = 1;
            }
            else
            {
                workLog.No = rawLog.No + 1;
            }
            db.WorkLog.Add(workLog);
            db.SaveChanges();
            foreach (var item in Work.Provider)
            {
                ProviderLog providerLog = new ProviderLog();
                providerLog.WorkLog = workLog;
                providerLog.WorkLogID = workLog.ID;
                providerLog.CreateBy = workLog.CreateBy;
                providerLog.UpdateBy = workLog.UpdateBy;
                providerLog.CreateDate = workLog.CreateDate;
                providerLog.UpdateDate = workLog.UpdateDate;
                providerLog.UserID = item.UserID;
                db.ProviderLog.Add(providerLog);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var Works = GetWork();
            ViewBag.WorkID = id;
            ViewBag.StatusBag = db.Status.Select(i => new SelectListItem() { Value = i.ID.ToString(), Text = i.StatusName });
            ViewBag.UserBag = db.User.Select(i => new SelectListItem() { Value = i.ID.ToString(), Text = i.Name });
            ViewBag.Model = Works;
            var work = Works.Where(b => b.ID == id).FirstOrDefault();
            var c = 0;
            work.ProviderIDs = work.Provider.First().UserID.ToString();
            foreach (var item in work.Provider)
            {
                if (c != 0)
                {
                    work.ProviderIDs = work.ProviderIDs + ',' + item.UserID.ToString();
                }
                c++;

            }

            return View(work);
        }
        [HttpPost]
        public ActionResult Edit(Work work)
        {
            db.Entry(work).State = EntityState.Modified;
            //db.Provider.
            WorkLog workLog = new WorkLog();
            workLog.WorkID = work.ID;
            workLog.Work = work;
            workLog.CreateDate = work.CreateDate;
            workLog.UpdateDate = work.UpdateDate;
            workLog.CreateBy = work.CreateBy;
            workLog.UpdateBy = work.UpdateBy;
            workLog.Remark = work.Remark;
            workLog.DueDate = work.DueDate;
            workLog.Name = work.Name;
            workLog.Project = work.Project;
            workLog.StatusID = work.StatusID;
            workLog.Status = work.Status;
            WorkLog rawLog = new WorkLog();
            try
            {
                rawLog = db.WorkLog.Where(b => b.WorkID == work.ID).ToList().Last();
            }
            catch
            {
                rawLog = null;
            }
            if (rawLog == null)
            {
                workLog.No = 1;
            }
            else
            {
                workLog.No = rawLog.No + 1;
            }
            db.WorkLog.Add(workLog);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult History(int id)
        {
            var Works = GetWork();

            foreach (var work in Works)
            {
                if (work.ID == id)
                {
                    work.WorkLog.Clear();
                    var Log = db.WorkLog.Where(b => b.WorkID == id).ToList();
                    if (Log.Count > 1)
                    {
                        for (int i = 0; i < Log.Count() - 1; i++)
                        {
                            Log[i].Description = "";
                            if (Log[i].Project != Log[i + 1].Project) { Log[i].Description += "Remark : " + Log[i].Project + " > " + Log[i + 1].Project; }
                            if (Log[i].Name != Log[i + 1].Name) { Log[i].Description += "Name : " + Log[i].Name + " > " + Log[i + 1].Name; }
                            if (Log[i].CreateBy != Log[i + 1].CreateBy) { Log[i].Description += "Assign By : " + Log[i].CreateBy + " > " + Log[i + 1].CreateBy; }
                            if (Log[i].CreateDate != Log[i + 1].CreateDate) { Log[i].Description += "Created Date : " + Log[i].CreateDate + " > " + Log[i + 1].CreateDate; }
                            if (Log[i].StatusID != Log[i + 1].StatusID) { Log[i].Description += "Status : " + Log[i].Status.StatusName + " > " + Log[i + 1].Status.StatusName; }
                            if (Log[i].Remark != Log[i + 1].Remark) { Log[i].Description += "Remark : " + Log[i].Remark + " > " + Log[i + 1].Remark; }
                            work.WorkLog.Add(Log[i]);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < Log.Count(); i++)
                        {
                            Log[i].Description = "";
                            if (Log[i].Project != work.Project) { Log[i].Description += "Remark : " + Log[i].Project + " > " + Log[i + 1].Project; }
                            if (Log[i].Name != work.Name) { Log[i].Description += "Name : " + Log[i].Name + " > " + work.Name; }
                            if (Log[i].CreateBy != work.CreateBy) { Log[i].Description += "Assign By : " + Log[i].CreateBy + " > " + work.CreateBy; }
                            if (Log[i].CreateDate != work.CreateDate) { Log[i].Description += "Created Date : " + Log[i].CreateDate + " > " + work.CreateDate; }
                            if (Log[i].StatusID != work.StatusID) { Log[i].Description += "Status : " + Log[i].Status.StatusName + " > " + work.Status.StatusName; }
                            if (Log[i].Remark != work.Remark) { Log[i].Description += "Remark : " + Log[i].Remark + " > " + work.Remark; }
                            work.WorkLog.Add(Log[i]);
                        }
                    }
                }

            }

            ViewBag.IDBag = id;
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
