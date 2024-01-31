using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AgileRap_Process_Software_ModelV2.Data;
using AgileRap_Process_Software_ModelV2.Models;
using System.Net.Mail;
using MailKit.Net.Smtp;
using MimeKit;
using AgileRap_Process_Software_ModelV2.Common;
using Microsoft.Extensions.Options;

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
            AllBag(GlobalVariable.GetUserLogin());
            return View(Works);
        }
        public ActionResult Create()
        {
            //ดึงข้อมูล Work จาก Database
            var Works = GetWork();

            Work work = new Work();
            work.Insert();

            //เพิ่มข้อมูลที่พึ่งสร้างขึ้นมาเข้าไปที่ Work ที่ดึงมาจาก database
            Works.Add(work);

            AllBag(GlobalVariable.GetUserLogin());
            return View(Works);
        }
        [HttpPost]
        public ActionResult Create(Work Work)
        {
            //ตัวจัดการ dropdown multi checkbox
            if ((bool)Work.IsSelectAll)
            {
                foreach (var item in db.User.ToList())
                {
                    Provider provider = new Provider();
                    provider.Insert(db, Work, item.ID);
                }
            }
            else
            {
                foreach (var item in Work.ProviderIDs.Split(','))
                {
                    Provider provider = new Provider();
                    provider.Insert(db, Work, int.Parse(item));
                }
            }
            //*************************************************//
            db.Work.Add(Work);
            //db.SaveChanges();

            //บันทึก Work ปัจจุบันลง WorkLog
            WorkLog workLog = new WorkLog();
            workLog.Insert(db, Work);
            //db.SaveChanges();

            //วนลูปเพื่อบันทึก provider
            foreach (var ProviderItem in Work.Provider)
            {
                //บันทึก Provider ปัจจุบันลง ProviderLog
                ProviderLog providerLog = new ProviderLog();
                providerLog.Insert(db, workLog, ProviderItem.ID);
                //db.SaveChanges();
            }
            //ส่งแจ้งเตือนผ่านเมล์
            SentEmailToProvider(Work);
            //กลับสู่หน้าหลัก
            return RedirectToAction("Index");
        }
        public ActionResult Edit(int id)
        {
            //หาตัวที่ต้องการแก้ไขจาก id 
            var Works = GetWork();
            var work = Works.Where(b => b.ID == id).FirstOrDefault();
            //************************************************//

            //ทำ field ProviderIDs ให้มีค่าตาม Provider ที่ดึงมาจาก Database
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
            //==================================================//

            //ทำ SelectListItem ให้กับ Dropdown multi checkbox เพื่อให้มีค่าตรงกับ Provider ใน Database
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
            //---------------------------------------------------//

            ViewBag.Model = Works;
            AllBag(GlobalVariable.GetUserLogin());
            ViewBag.UserBag = list;
            return View(work);
        }
        [HttpPost]
        public ActionResult Edit(Work work)
        {
            //เช็คว่ามีการเปลี่ยนแปลงค่าไหม  ไม่มีค่าที่เปลี่ยนแปลง ไม่ต้องบันทึก Log
            var Works = GetWork();
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
                if (!IsNotEqual)
                {
                    return RedirectToAction("Index");
                }
            }
            //*************************************************//

            db.ChangeTracker.Clear();
            //UPDATE work
            work.Update(db);

            //นำ Work ปัจจุบันไปเก็บไว้เป็น Log
            WorkLog workLog = new WorkLog();
            workLog.Insert(db, work);

            //ProviderIDs ที่ join กันมาเป็น string มาแบ่งออกเก็บไว้ใน List strings
            List<string> strings = new List<string>();
            if (work.ProviderIDs != null) { strings = work.ProviderIDs.Split(',').ToList(); }


            //กรณีไม่ได้เลือก provider เข้ามาเลย
            if (strings.Count == 0 && work.IsSelectAll == false)
            {
                foreach (var item in work.Provider)
                {
                    db.Provider.Remove(item);
                }
            }
            //กรณีเลือก provider ทั้งหมด
            else if (work.IsSelectAll)
            {
                foreach (var item in work.Provider)
                {
                    db.Provider.Remove(item);
                }
                foreach (var item in db.User.ToList())
                {
                    Provider provider = new Provider();
                    provider.Insert(db, work, item.ID);

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
                foreach (var item in work.Provider)
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
                    provider.Insert(db, work, int.Parse(s));

                }
                ProviderLog providerLog = new ProviderLog();
                providerLog.Insert(db, workLog, int.Parse(s));
                db.SaveChanges();
            }
            //บันทึกการเปลี่ยนแปลงลง Database
            db.SaveChanges();

            //ส่งไปทำ Action Index ต่อไป
            return RedirectToAction("Index");
        }
        public ActionResult History(int id)
        {
            var Works = GetWork();

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
            var Works = GetWork();
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
                $"<h2>Subject {work.Project}</h2> " +
                $"<h3>Name {work.Name}</h3>" +
                $"<p>Form {HttpContext.RequestServices.GetService<EmailConfiguration>().From}</p>" +
                $"<p>Status : {work.Status.StatusName}</p>" +
                $"<p>Due Date : {work.DueDate}</p>" +
                $"<p>Remark : {work.Remark}</p>" +
                $"" +
                $"<p>Assign by {db.User.Where(b => b.ID == GlobalVariable.GetUserLogin()).First().Name}</p>" +
                $"";

            //รวมข้อมูลที่เตรียมไว้ข้างต้น
            Message message = new Message(EmailList, work.Project, contentText);

            //ทำการส่ง Email ไปตามใน EmailList
            _emailSender.SendEmail(message);
        }


    }
}
