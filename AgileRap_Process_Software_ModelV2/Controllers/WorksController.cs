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

        public void AllBag(int id)
        {
            ViewBag.StatusBag = db.Status.Select(i => new SelectListItem() { Value = i.ID.ToString(), Text = i.StatusName });
            ViewBag.UserBag = db.User.Select(i => new SelectListItem() { Value = i.ID.ToString(), Text = i.Name });
            ViewBag.UserLogin = db.User.Where(b => b.ID == id);

        }
        public ActionResult Index()
        {
            var Works = GetWork();
            AllBag((int)HttpContext.Session.GetInt32("UserID"));
            return View(Works);
        }

        public ActionResult Create()
        {
            var Works = GetWork();
            Work work = new Work();
          
            work.Insert(db);
            work.CreateBy = (int)HttpContext.Session.GetInt32("UserID");
            Works.Add(work);


            AllBag((int)HttpContext.Session.GetInt32("UserID"));
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

            var work = Works.Where(b => b.ID == id).FirstOrDefault();
            var c = 0;
            if (work.Provider.Count > 0)
            {
                work.ProviderIDs = work.Provider.First().UserID.ToString();
            }
            foreach (var item in work.Provider)
            {
                if (c != 0)
                {
                    work.ProviderIDs = work.ProviderIDs + ',' + item.UserID.ToString();
                }
                c++;

            }
            List<SelectListItem> list = new List<SelectListItem>();

            foreach (var item in db.User)
            {
                SelectListItem selectListItem = new SelectListItem();
                selectListItem.Value = item.ID.ToString();
                selectListItem.Text = item.Name;
                foreach (var item2 in work.Provider)
                {
                    if (item2.UserID == item.ID)
                    {
                        selectListItem.Selected = true;
                        break;
                    }
                    else
                    {
                        selectListItem.Selected = false;
                    }
                }
                list.Add(selectListItem);

            }
            ViewBag.WorkID = id;
            ViewBag.Model = Works;
            AllBag((int)HttpContext.Session.GetInt32("UserID"));
            ViewBag.UserBag = list;

            return View(work);
        }
        [HttpPost]
        public ActionResult Edit(Work work)
        {
            if (work.DueDate == null)
            {
                work.StatusID = 1;
            }
            work.UpdateDate = DateTime.Now;
            db.Entry(work).State = EntityState.Modified;
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
            WorkLog rawWorkLog = new WorkLog();
            try
            {
                rawWorkLog = db.WorkLog.Where(b => b.WorkID == work.ID).ToList().Last();
            }
            catch
            {
                rawWorkLog = null;
            }
            if (rawWorkLog == null)
            {
                workLog.No = 1;
            }
            else
            {
                workLog.No = rawWorkLog.No + 1;
            }
            db.WorkLog.Add(workLog);
            List<string> strings = new List<string>();
            if (work.IsSelectAll)
            {
                var c = 0;
                work.ProviderIDs = db.User.First().ID.ToString();
                foreach (var item in db.User.ToList())
                {
                    if (c != 0)
                    {
                        work.ProviderIDs = work.ProviderIDs + ',' + item.ID.ToString();
                    }
                    c++;
                }

            }
            else
            {
                if (work.ProviderIDs != null || work.ProviderIDs == "")
                {
                    strings = work.ProviderIDs.Split(',').ToList();
                }
                else
                {
                    foreach (var itemPro in work.Provider)
                    {
                        db.Provider.Remove(itemPro);
                    }
                    work.Provider = new List<Provider>();
                }
            }

            List<Provider> providers = new List<Provider>();
            foreach (Provider i in work.Provider)
            {
                providers.Add(i);
            }


            foreach (var item in strings)
            {
                ProviderLog providerLog = new ProviderLog();
                providerLog.CreateDate = DateTime.Now;
                providerLog.UpdateDate = DateTime.Now;
                providerLog.User = db.User.Find(int.Parse(item));
                providerLog.UserID = int.Parse(item);
                providerLog.IsDelete = false;
                providerLog.WorkLog = workLog;
                providerLog.WorkLogID = work.ID;
                providerLog.CreateBy = work.CreateBy;
                providerLog.UpdateBy = work.UpdateBy;
                db.ProviderLog.Add(providerLog);

                if (work.Provider.Count > 0)
                {
                    foreach (var item2 in providers)
                    {
                        if (item == item2.UserID.ToString())
                        {
                            db.Entry(item2).State = EntityState.Modified;
                        }
                        else
                        {
                            var itHaveValue = false;
                            foreach (var item3 in strings)
                            {
                                if (item2.UserID.ToString() == item3)
                                {
                                    itHaveValue = true;
                                }
                            }
                            if (!itHaveValue)
                            {
                                db.Provider.Remove(item2);
                            }

                            var itHasValue2 = false;
                            foreach (var item4 in providers)
                            {
                                if (item == item4.UserID.ToString())
                                {
                                    itHasValue2 = true;
                                }
                            }
                            if (!itHasValue2)
                            {
                                Provider provider = new Provider();
                                provider.CreateDate = DateTime.Now;
                                provider.UpdateDate = DateTime.Now;
                                provider.User = db.User.Find(int.Parse(item));
                                provider.UserID = db.User.Find(int.Parse(item)).ID;
                                provider.IsDelete = false;
                                provider.Work = work;
                                provider.WorkID = work.ID;
                                provider.CreateBy = work.CreateBy;
                                provider.UpdateBy = work.UpdateBy;
                                db.Provider.Add(provider);

                            }
                        }
                    }
                }
                else
                {
                    foreach (var idprovider in strings)
                    {
                        Provider provider = new Provider();
                        provider.CreateDate = DateTime.Now;
                        provider.UpdateDate = DateTime.Now;
                        provider.User = db.User.Find(int.Parse(idprovider));
                        provider.UserID = int.Parse(idprovider);
                        provider.IsDelete = false;
                        provider.Work = work;
                        provider.WorkID = work.ID;
                        provider.CreateBy = work.CreateBy;
                        provider.UpdateBy = work.UpdateBy;
                        db.Provider.Add(provider);
                    }
                }
            }

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
                    var wLog = db.WorkLog.Where(b => b.WorkID == id).Include(m => m.Status).ToList();
                    if (wLog.Count > 1)
                    {
                        for (int i = 0; i < wLog.Count() - 1; i++)
                        {
                            wLog[i].Description = "";
                            if (wLog[i].Project != wLog[i + 1].Project) { wLog[i].Description += "Project : " + wLog[i].Project + " > " + wLog[i + 1].Project + "\n"; }
                            if (wLog[i].Name != wLog[i + 1].Name) { wLog[i].Description += "Name : " + wLog[i].Name + " > " + wLog[i + 1].Name + "\n"; }
                            if (wLog[i].CreateBy != wLog[i + 1].CreateBy) { wLog[i].Description += "Assign By : " + wLog[i].CreateBy + " > " + wLog[i + 1].CreateBy + "\n"; }
                            if (wLog[i].CreateDate != wLog[i + 1].CreateDate) { wLog[i].Description += "Created Date : " + wLog[i].CreateDate + " > " + wLog[i + 1].CreateDate + "\n"; }
                            if (wLog[i].StatusID != wLog[i + 1].StatusID) { wLog[i].Description += "Status : " + wLog[i].Status.StatusName + " > " + wLog[i + 1].Status.StatusName + "\n"; }
                            if (wLog[i].Remark != wLog[i + 1].Remark) { wLog[i].Description += "Remark : " + wLog[i].Remark + " > " + wLog[i + 1].Remark + "\n"; }

                            //หาค่าที่ไม่เท่ากันของ provider โดยเทียบจาก WorkLog[i] และ WorkLog[i+1]
                            if (wLog[i].ProviderLog != wLog[i + 1].ProviderLog)
                            {
                                List<string> stringlist = new List<string>();

                                //เช็ค provider ทั้ง 2 ตั้วว่าเท่ากันหรือไม่
                                if (wLog[i].ProviderLog.Count == wLog[i + 1].ProviderLog.Count)
                                {
                                    for (int j = 0; j < wLog[i].ProviderLog.Count; j++)
                                    {
                                        if (wLog[i].ProviderLog.ElementAt(j).UserID != wLog[i + 1].ProviderLog.ElementAt(j).UserID)
                                        {
                                            stringlist.Add($"{wLog[i].ProviderLog.ElementAt(j).User.Name} > {wLog[i + 1].ProviderLog.ElementAt(j).User.Name}\n");
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var itemProLog in wLog[i].ProviderLog)
                                    {
                                        foreach (var itemProLog2 in wLog[i + 1].ProviderLog)
                                        {
                                            if (itemProLog.UserID != itemProLog2.UserID)
                                            {
                                                stringlist.Add($"{itemProLog.User.Name} > {itemProLog2.User.Name}\n");
                                            }
                                        }
                                    }
                                }
                                stringlist.Distinct();
                                var TextUserName = "";
                                foreach (var item in stringlist) { TextUserName += item; }
                                if (TextUserName != "")
                                {
                                    wLog[i].Description += "Provider : " + TextUserName;
                                }
                            }
                            work.WorkLog.Add(wLog[i]);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < wLog.Count(); i++)
                        {
                            wLog[i].Description = "";
                            //if (wLog[i].Project != work.Project) { wLog[i].Description += "Project : " + wLog[i].Project + " > " + work.Project + "\n"; }
                            //if (wLog[i].Name != work.Name) { wLog[i].Description += "Name : " + wLog[i].Name + " > " + work.Name + "\n"; }
                            //if (wLog[i].CreateBy != work.CreateBy) { wLog[i].Description += "Assign By : " + wLog[i].CreateBy + " > " + work.CreateBy + "\n"; }
                            //if (wLog[i].CreateDate != work.CreateDate) { wLog[i].Description += "Created Date : " + wLog[i].CreateDate + " > " + work.CreateDate + "\n"; }
                            //if (wLog[i].StatusID != work.StatusID) { wLog[i].Description += "Status : " + wLog[i].Status.StatusName + " > " + work.Status.StatusName + "\n"; }
                            //if (wLog[i].Remark != work.Remark) { wLog[i].Description += "Remark : " + wLog[i].Remark + " > " + work.Remark + "\n"; }
                            //if (wLog[i].ProviderLog != work.Provider)
                            //{
                            //    foreach (var itemProLog in wLog[i].ProviderLog)
                            //    {
                            //        foreach (var itemProLog2 in wLog[i + 1].ProviderLog)
                            //        {
                            //            if (itemProLog.Id != itemProLog2.Id)
                            //            {
                            //                wLog[i].Description += "Provider : " + itemProLog.User.Name + " > " + itemProLog2.User.Name + "\n";
                            //            }
                            //        }
                            //    }
                            //}

                            work.WorkLog.Add(wLog[i]);
                        }
                    }
                }

            }

            ViewBag.IDBag = id;
            AllBag((int)HttpContext.Session.GetInt32("UserID"));
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
                if (item.WorkLog.Count > 0)
                {
                    foreach (var item3 in item.WorkLog)
                    {
                        if (item3.ProviderLog == null)
                        {
                            item3.ProviderLog = db.ProviderLog.Where(b => b.WorkLogID == item3.ID).ToList();
                        }
                        foreach (var item5 in item3.ProviderLog)
                        {
                            item5.User = db.User.Find(item5.UserID);
                        }

                    }
                }
            }
            return Works;
        }
    }
}
