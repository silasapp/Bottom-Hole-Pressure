using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using BHP.Models.DB;
using BHP.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using BHP.HelpersClass;
using Microsoft.AspNetCore.Authorization;
using BHP.Models;

namespace BHP.Controllers.Schedule
{
    [Authorize]
    public class SchdulesController : Controller
    {
        private readonly BHP_DBContext _context;
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        HelpersController _helpersController;
        GeneralClass generalClass = new GeneralClass();


        public SchdulesController(BHP_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }



        /*
       * Customer approving schedule
       * 
       * ScheduleID => encrypted schedule id
       */
        [Authorize(Roles = "COMPANY")]
        public JsonResult CustomerAcceptSchedule(string ScheduleID, string txtComment)
        {
            string result = "";

            int scheduleID = 0;

            var sID = generalClass.Decrypt(ScheduleID);

            if (sID == "Error")
            {
                result = "Application link error";
            }
            else
            {
                scheduleID = Convert.ToInt32(sID);

                var sch = _context.Schdules.Where(x => x.SchduleId == scheduleID && x.DeletedStatus == false);

                if (sch.Any())
                {
                    int requestid = sch.FirstOrDefault().RequestId;
                    int scheduleBy = sch.FirstOrDefault().SchduleBy;

                    sch.FirstOrDefault().CustomerAccept = 1;
                    sch.FirstOrDefault().CustomerComment = txtComment;
                    sch.FirstOrDefault().UpdatedAt = DateTime.Now;

                    if (_context.SaveChanges() > 0)
                    {
                        var staff = _context.Staff.Where(x => x.StaffId == scheduleBy);

                        var actionFrom = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());
                        var actionTo = _helpersController.getActionHistory(staff.FirstOrDefault().RoleId, staff.FirstOrDefault().StaffId);

                        var getApps = _context.RequestProposal.Where(x => x.RequestId == requestid);

                        _helpersController.SaveHistory(requestid, actionFrom, actionTo, "Schedule Approved", "Customer accepted the Schedule");

                        string subj = "Schedule for application (" + getApps.FirstOrDefault().RequestRefNo + ") Approved by the Marketer.";
                        string cont = "Schedule for application with reference number " + getApps.FirstOrDefault().RequestRefNo + " has been approved by the Marketer.";

                        var send = _helpersController.SendEmailMessageAsync(staff.FirstOrDefault().StaffEmail, staff.FirstOrDefault().LastName + " " + staff.FirstOrDefault().FirstName, subj, cont, GeneralClass.STAFF_NOTIFY, null);

                        _helpersController.SaveMessage(requestid, (int)getApps.FirstOrDefault().CompanyId, subj, cont);

                        result = "Schedule Accepted";
                    }
                    else
                    {
                        result = "Something went wrong trying to approve this schedule. Please try again later.";
                    }
                }
                else
                {
                    result = "Something went wrong. Your schedule was not found.";
                }
            }

            _helpersController.LogMessages("Schedule Status " + result + ". Schedule ID : " + scheduleID,  _helpersController.getSessionEmail());

            return Json(result);
        }



        /*
         * Customer approving schedule
         * 
         * ScheduleID => encrypted schedule id
         */
        [Authorize(Roles = "COMPANY")]
        public JsonResult CustomerRejectSchedule(string ScheduleID, string txtComment)
        {
            string result = "";

            int scheduleID = 0;

            var sID = generalClass.Decrypt(ScheduleID);

            if (sID == "Error")
            {
                result = "Application link error";
            }
            else
            {
                scheduleID = Convert.ToInt32(sID);

                var sch = _context.Schdules.Where(x => x.SchduleId == scheduleID && x.DeletedStatus == false);

                if (sch.Any())
                {
                    sch.FirstOrDefault().CustomerAccept = 2;
                    sch.FirstOrDefault().CustomerComment = txtComment;
                    sch.FirstOrDefault().UpdatedAt = DateTime.Now;

                    if (_context.SaveChanges() > 0)
                    {
                        result = "Schedule Rejected";
                    }
                    else
                    {
                        result = "Something went wrong trying to reject this schedule. Please try again later.";
                    }
                }
                else
                {
                    result = "Something went wrong. Your schedule was not found.";
                }
            }

            _helpersController.LogMessages("Schedule Status " + result + ". Schedule ID : " + scheduleID,  _helpersController.getSessionEmail());

            return Json(result);
        }



        public JsonResult ScheduleCalendar(string id)
        {
            var sch = from s in _context.Schdules
                      join a in _context.RequestProposal on s.RequestId equals a.RequestId
                      join sf in _context.Staff on s.SchduleBy equals sf.StaffId
                      join f in _context.RequestDuration on a.DurationId equals f.DurationId
                      join c in _context.Companies on a.CompanyId equals c.CompanyId
                      select new MySchdule
                      {
                          ScheduleID = s.SchduleId,
                          ScheduleDate = s.SchduleDate.ToString(),
                          ScheduleBy = sf.LastName + " " + sf.FirstName,
                          CompanyName = c.CompanyName,
                          CompanyID = c.CompanyId,
                          staffID = s.SchduleBy,
                          CustomerResponse = s.CustomerAccept,
                          StaffComment = s.Comment,
                          SupervisorComment = s.SupervisorComment,
                          CustomerComment = s.CustomerComment,
                          ScheduleType = s.SchduleType,
                          ScheduleLocation = s.SchduleLocation,
                          CreatedAt = s.CreatedAt,
                          UpdatedAt = s.UpdatedAt,
                          Supervisor = s.Supervisor,
                          SupervisorApproved = s.SupervisorApprove,
                          ProposedYear = f.ProposalYear
                      };

            var calendar = from s in sch
                           select new
                           {
                               id = s.ScheduleID,
                               title = s.ScheduleType.ToUpper(),
                               start = Convert.ToDateTime(s.ScheduleDate),
                               company = s.CompanyName.ToUpper(),
                               location = s.ScheduleLocation,
                               customerResponse = s.CustomerResponse == 1 ? "Accepted" : s.CustomerResponse == 2 ? "Rejected" : "Awaiting Response",
                               customerComment = s.CustomerComment,
                               schedule = s.ScheduleBy,
                               staffComment = s.StaffComment,
                               supervisorResponse = s.SupervisorApproved == 1 ? "Accepted" : s.SupervisorApproved == 2 ? "Rejected" : "Awaiting Response",
                               supervisorComment = s.SupervisorComment,
                               allDay = false,
                               supervisor = s.Supervisor,
                               staff_id = s.staffID,
                               proposeYear =  s.ProposedYear
                           };

            if (!string.IsNullOrEmpty(id))
            {
                calendar = calendar.Where(x => x.supervisor == Convert.ToInt32(id) || x.staff_id == Convert.ToInt32(id));
            }

            return Json(calendar.ToList());
        }




        public JsonResult GetCompanyScheduleCount()
        {
            var getSch = from sh in _context.Schdules
                         join a in _context.RequestProposal on sh.RequestId equals a.RequestId
                         where a.CompanyId == _helpersController.getSessionUserID() && sh.CustomerAccept == 0 && a.DeletedStatus == false && sh.DeletedStatus == false && sh.SupervisorApprove == 1
                         select new
                         {
                             sh
                         };
            return Json(getSch.Count());
        }




        public IActionResult Index(string id)
        {
            int staff_id = 0;
            var staffID = generalClass.Decrypt(id);

            var sch = from d in _context.Schdules
                      join a in _context.RequestProposal.AsEnumerable() on d.RequestId equals a.RequestId
                      join sf in _context.Staff.AsEnumerable() on d.SchduleBy equals sf.StaffId
                      join c in _context.Companies.AsEnumerable() on a.CompanyId equals c.CompanyId
                      join s in _context.RequestDuration.AsEnumerable() on a.DurationId equals s.DurationId
                      where d.DeletedStatus == false && a.DeletedStatus == false
                      select new MyApps
                      {
                          RequestId = a.RequestId,
                          RefNo = a.RequestRefNo,
                          Status = a.Status,
                          isDone = d.IsDone == true ? "YES" : "NO",
                          CompanyName = c.CompanyName,
                          CompanyID = c.CompanyId,
                          CompanyEmail = c.CompanyEmail,
                          CompanyAddress = c.Address + ", " + c.City + ", " + c.StateName,
                          DateApplied = a.DateApplied.ToString(),
                          ScheduleType = d.SchduleType,
                          ScheduleLocation = d.SchduleLocation,
                          ScheduleDate = d.SchduleDate,
                          Comment = d.Comment,
                          CustomerAccept = (int)d.CustomerAccept,
                          CustomerComment = d.CustomerComment,
                          StaffName = sf.LastName + " " + sf.FirstName,
                          StaffID = sf.StaffId,
                          CreatedAt = d.CreatedAt,
                          UpdateAt = d.UpdatedAt,
                          Year = s.ProposalYear
                      };

            ViewData["ScheduleTitle"] = "All Schedules";
            ViewData["ScheduleStaffID"] = "";

            if (sch.Any())
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    if (staffID == "Error")
                    {
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Schedule not found or not in correct format. Kindly contact support.") });
                    }
                    else
                    {
                        staff_id = Convert.ToInt32(staffID);
                        sch = sch.Where(x => x.StaffID == staff_id);
                        ViewData["ScheduleTitle"] = "My Schedule";
                        ViewData["ScheduleStaffID"] = staffID;

                        _helpersController.LogMessages("Displaying schedule for " + ViewData["ScheduleTitle"],  _helpersController.getSessionEmail());

                        return View(sch.ToList());
                    }
                }
                else
                {
                    return View(sch.ToList());
                }
            }
            else
            {
                return View();
            }
        }


    }
}
