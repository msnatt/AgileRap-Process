using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AgileRap_Process_Software_ModelV2.Data;
using AgileRap_Process_Software_ModelV2.Models;
using AgileRap_Process_Software_ModelV2.Common;

namespace AgileRap_Process_Software_ModelV2.Controllers
{
    public class WorksController : BaseController
    {
        public void AllBag(int id)
        {
            ViewBag.StatusBag = db.Status.Select(i => new SelectListItem() { Value = i.ID.ToString(), Text = i.StatusName });
            ViewBag.UserBag = db.User.Select(i => new SelectListItem() { Value = i.ID.ToString(), Text = i.Name });
            List<SelectListItem> ListWorkTemp = db.Work.Select(i => new SelectListItem() { Value = i.Project, Text = i.Project }).ToList();
            ViewBag.WorkBag = ListWorkTemp.DistinctBy(b => b.Value);
            ViewBag.UserLogin = db.User.Where(b => b.ID == id);
            ViewBag.UserFilterBag = SelectedSelectListItem(HttpContext.Session.GetString("AssignTo"));
        }
        private void FuncChangeMode(string? changeMode)
        {
            if (changeMode == "Operator")
            {
                HttpContext.Session.SetString("Default", "Operator");
            }
            else if (changeMode == "Controller")
            {
                HttpContext.Session.SetString("Default", "Controller");
            }
        }
        public ActionResult Index(string? AssignBy, string? AssignTo, string? Project, string? Status, bool? IsChangePage, string? ChangeMode)
        {
            List<Work> Works = GetWork();
            Works = FilterWorks(Works, AssignBy, AssignTo, Project, Status, IsChangePage);
            AllBag(GlobalVariable.GetUserLogin());

            //ChangeMode Operator or Controller
            FuncChangeMode(ChangeMode);

            return View(Works);
        }
        public ActionResult Create(string? AssignBy, string? AssignTo, string? Project, string? Status, bool? IsChangePage, string? ChangeMode)
        {
            //ดึงข้อมูล Work จาก Database
            List<Work> Works = GetWork();
            Works = FilterWorks(Works, AssignBy, AssignTo, Project, Status, IsChangePage);

            Work work = new Work();
            work.Insert();

            //เพิ่มข้อมูลที่พึ่งสร้างขึ้นมาเข้าไปที่ Work ที่ดึงมาจาก database
            Works.Add(work);

            //ChangeMode Operator or Controller
            FuncChangeMode(ChangeMode);

            AllBag(GlobalVariable.GetUserLogin());
            return View(Works);
        }
        [HttpPost]
        public ActionResult Create(Work Work)
        {
            //ตัวจัดการ dropdown multi checkbox
            if (Work.IsSelectAll)
            {
                foreach (var item in db.User.ToList())
                {
                    Provider provider = new Provider();
                    provider.Insert(db, Work, item.ID);
                    Work.Provider.Add(provider);
                }
            }
            else
            {
                foreach (var item in Work.ProviderIDs.Split(','))
                {
                    Provider provider = new Provider();
                    provider.Insert(db, Work, int.Parse(item));
                    Work.Provider.Add(provider);
                }
            }
            //*************************************************//
            db.Work.Add(Work);
            db.SaveChanges();

            //บันทึก Work ปัจจุบันลง WorkLog
            WorkLog workLog = new WorkLog();
            workLog.Insert(db, Work);
            db.SaveChanges();

            //วนลูปเพื่อบันทึก provider
            foreach (var ProviderItem in Work.Provider)
            {
                //บันทึก Provider ปัจจุบันลง ProviderLog
                ProviderLog providerLog = new ProviderLog();
                providerLog.Insert(db, workLog, ProviderItem.ID);
            }
            db.SaveChanges();
            //ส่งแจ้งเตือนผ่านเมล์
            SentEmailToProvider(Work);
            //กลับสู่หน้าหลัก
            return RedirectToAction("Index");
        }
        public ActionResult Edit(int id, string? AssignBy, string? AssignTo, string? Project, string? Status, bool? IsChangePage, string? ChangeMode)
        {
            //หาตัวที่ต้องการแก้ไขจาก id 
            List<Work> Works = GetWork();
            Works = FilterWorks(Works, AssignBy, AssignTo, Project, Status, IsChangePage);
            var work = Works.Where(b => b.ID == id).FirstOrDefault();

            //เช็คว่าที่กรองมา มี id ที่ตรงกับที่ผู้ใช้ต้องการ Edit ไหม 
            if (Works.Where(b => b.ID == id).Count() == 0)
            {
                //ถ้าไม่ย้อนกลับไปหน้า Index
                return RedirectToAction("Index", Works);
            }

            //************************************************//

            //ChangeMode Operator or Controller
            FuncChangeMode(ChangeMode);

            ViewBag.Model = Works;
            AllBag(GlobalVariable.GetUserLogin());
            ViewBag.UserBag = SelectedSelectListItem(work.ProviderIDs);
            return View(work);
        }
        [HttpPost]
        public ActionResult Edit(Work work)
        {
            //1. ตรวจสอบรายการว่ามีการเปลี่ยนแปลงค่าหรือไม่
            bool check = FuncCheckDulicateData(work);
            if (check)
            {
                db.ChangeTracker.Clear();
                //UPDATE work
                work.Update(db);
            }

            //ส่งไปทำ Action Index ต่อไป
            return RedirectToAction("Index");
        }
        public ActionResult History(int id, string? AssignBy, string? AssignTo, string? Project, string? Status, bool? IsChangePage, string? ChangeMode)
        {
            List<Work> Works = db.Work.Where(b => b.ID == id).ToList();
            
            List<Work> works = GetWork();
            works = FilterWorks(works, AssignBy, AssignTo, Project, Status, IsChangePage);

            works = works.Where(b => b.ID != Works.First().ID).ToList();
            Works.AddRange(works);

            //วนหา ID ที่ ผู้ใช้งานต้องการดู History
            foreach (var work in Works)
            {
                //เช็ค ID ที่ผู้ใช้งานต้องการดู History
                if (work.ID == id)
                {
                    //เคลีย WorkLog แล้วดึงมาใหม่
                    work.WorkLog.Clear();
                    var wLog = db.WorkLog.Where(b => b.WorkID == id).Include(m => m.Status).ToList();

                    //เทียบระหว่าง Log 2 ตัว และนำตัวที่ต่างกันเขียนลงใน Description
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
                            if (wLog[i].ProviderLogList != wLog[i + 1].ProviderLogList) { wLog[i].Description += "Provider : " + wLog[i].ProviderLogList + " > " + wLog[i + 1].ProviderLogList + "\n"; }
                            work.WorkLog.Add(wLog[i]);
                        }
                    }
                }
            }
            //ChangeMode Operator or Controller
            FuncChangeMode(ChangeMode);

            ViewBag.IDBag = id;
            AllBag(GlobalVariable.GetUserLogin());
            return View(Works);
        }
        public List<Work> GetWork()
        {
            //ดึงข้อมูลมาจาก Database ทั้งหมดที่เชื่อมโยงกับ Work
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
            //นำ IDและName ของ Provider ที่ดึงมาจาก Database ไปเปลี่ยนเป็น Text เก็บไว้ที่ ProviderList และ ProviderIDs
            ConvertProviderToAssignText(Works);



            return Works;
        }
        [HttpPost]
        public ActionResult UpdateStatus(Work work)
        {
            List<Work> Works = GetWork();
            if (work.DueDate == null) { work.StatusID = 1; } else { work.StatusID = 2; }
            AllBag(GlobalVariable.GetUserLogin());
            Works.Add(work);
            ModelState.Clear();
            return View("Create", Works);
        }
        private void ConvertProviderToAssignText(List<Work> works)
        {
            //ทำของ Work
            foreach (var item in works)
            {
                for (int i = 0; i < item.Provider.Count; i++)
                {
                    if (i == 0)
                    {
                        item.ProviderList = item.Provider.First().User.Name;
                        item.ProviderIDs = item.Provider.First().User.ID.ToString();
                    }
                    else
                    {
                        item.ProviderList += ", " + item.Provider.ElementAt(i).User.Name;
                        item.ProviderIDs += ", " + item.Provider.ElementAt(i).User.ID;
                    }

                }
                //ทำของ WorkLog
                foreach (var item2 in item.WorkLog)
                {
                    for (int j = 0; j < item2.ProviderLog.Count; j++)
                    {
                        if (j == 0) { item2.ProviderLogList = item2.ProviderLog.First().User.Name; }
                        else
                        {
                            item2.ProviderLogList += ", " + item2.ProviderLog.ElementAt(j).User.Name;
                        }
                    }
                }
            }
        }
        private void SentEmailToProvider(Work work)
        {
            //Send Email
            //เก็บ Email ผู้รับไว้ใน EmailList
            work.Status = db.Status.Find(work.StatusID);
            List<string> EmailList = new List<string>();
            foreach (var item in work.Provider)
            {
                EmailList.Add(item.User.Email);
            }

            //ดึงเมล์คนส่งจาก appsetting.json
            EmailSender _emailSender = new EmailSender(HttpContext.RequestServices.GetService<EmailConfiguration>());

            //กำหนดข้อความที่จะส่งไปในเมล์
            string contentText = $"" +
                $"<h1>Hello</h1>" +
                $"<h2>Subject {work.Project} : {work.Name}</h2> " +
                $"<p>Form {HttpContext.RequestServices.GetService<EmailConfiguration>().From}</p>" +
                $"<p>Status : {work.Status.StatusName}</p>" +
                $"<p>Due Date : {work.DueDate}</p>" +
                $"<p>Remark : {work.Remark}</p>" +
                $"<p>Assign by {db.User.Where(b => b.ID == GlobalVariable.GetUserLogin()).First().Name}</p>" +
                $"";

            //รวมข้อมูลที่เตรียมไว้ข้างต้น
            Message message = new Message(EmailList, work.Project, contentText);

            //ทำการส่ง Email ไปตามใน EmailList
            _emailSender.SendEmail(message);
        }
        private List<Work> FilterWorks(List<Work> Works, string? AssignBy, string? AssignTo, string? Project, string? Status, bool? IsChangePage)
        {
            if (IsChangePage == null && AssignBy == null) { HttpContext.Session.Remove("AssignBy"); }
            if (IsChangePage == null && AssignTo == null) { HttpContext.Session.Remove("AssignTo"); }
            if (IsChangePage == null && Project == null) { HttpContext.Session.Remove("Project"); }
            if (IsChangePage == null && Status == null) { HttpContext.Session.Remove("Status"); }

            //filter All Works
            if (AssignBy != null) { Works = Works.Where(b => b.CreateBy == int.Parse(AssignBy)).ToList(); HttpContext.Session.SetString("AssignBy", AssignBy); }
            if (Project != null) { Works = Works.Where(b => b.Project == Project).ToList(); HttpContext.Session.SetString("Project", Project); }
            if (Status != null) { Works = Works.Where(b => b.StatusID == int.Parse(Status)).ToList(); HttpContext.Session.SetString("Status", Status); }
            if (AssignTo != null)
            {
                //Credit P Terng
                string[] strings = AssignTo.Split(',');
                foreach (var work in Works)
                {
                    if (work.Provider.Where(w => strings.Contains(w.UserID.ToString()) == true).Count() > 0) { work.IsFound = true; }
                }
                Works = Works.Where(b => b.IsFound == true).ToList();
                HttpContext.Session.SetString("AssignTo", AssignTo);
            }
            //=========================================================//
            return Works;
        }
        public List<SelectListItem> SelectedSelectListItem(string Liststring)
        {
            //ทำ SelectListItem ให้กับ Dropdown multi checkbox เพื่อให้มีค่าตรงกับ Provider ใน Database
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var item in db.User)
            {
                SelectListItem selectListItem = new SelectListItem();
                selectListItem.Value = item.ID.ToString();
                selectListItem.Text = item.Name;
                if (Liststring != null)
                {
                    foreach (var item2 in Liststring.Split(','))
                    {
                        if (item2 == item.ID.ToString())
                        {
                            selectListItem.Selected = true;
                            break;
                        }
                        else
                        {
                            selectListItem.Selected = false;
                        }
                    }
                }
                list.Add(selectListItem);
            }
            //---------------------------------------------------//
            return list;

        }
        private bool FuncCheckDulicateData(Work work)
        {
            List<Work> Works = GetWork();
            var temp = Works.Where(b => b.ID == work.ID).ToList().First();
            if (work.Project == temp.Project &&
                work.Name == temp.Name &&
                work.DueDate == temp.DueDate &&
                work.StatusID == temp.StatusID &&
                work.Remark == temp.Remark &&
                work.IsSelectAll != true &&
                work.ProviderIDs == temp.ProviderIDs &&
                work.ProviderList == temp.ProviderList &&
                work.Provider.Count == temp.Provider.Count)
            {
                bool IsNotEqual = false;
                for (int i = 0; i < work.Provider.Count; i++)
                {
                    if (work.Provider.ElementAt(i).ID != temp.Provider.ElementAt(i).ID)
                    {
                        IsNotEqual = true;
                    }
                }
                return IsNotEqual;
            }
            else { return true; }
        }

    }
}
