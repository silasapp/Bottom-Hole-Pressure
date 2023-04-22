using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using BHP.Models.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using BHP.Helpers;
using BHP.HelpersClass;
using Microsoft.AspNetCore.Authorization;

namespace BHP.Controllers.OutOfOffice
{
    [Authorize]
    public class OutOfOfficesController : Controller
    {
        private readonly BHP_DBContext _context;
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        HelpersController _helpersController;
        GeneralClass generalClass = new GeneralClass();

        public OutOfOfficesController(BHP_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }


        // [Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD GAS, AD, PRINTER, DOC MANAGER, SUPERVISOR, INSPECTOR, REVIEWER, TEAM LEAD, SUPER ADMIN")]
        public IActionResult OutOfOffice()
        {
            return View();
        }



        /*
         * Creating out of office module
         */
        //[Authorize(Roles = "DIRECTOR, AD, SUPERVISOR, HEAD GAS, INSPECTOR, TEAM LEAD, AD OPS, REVIEWER, SUPPORT")]
        public IActionResult CreateOutOfOffice(BHP.Models.DB.OutOfOffice outOfOffice)
        {
            string result = "";

            var staffID = _helpersController.getSessionUserID();

            var office = _context.OutOfOffice.Where(x => x.StaffId == staffID && x.Status != GeneralClass._FINISHED && x.DeletedStatus == false);

            if (office.Any())
            {
                result = "Sorry, you already have an active out of office schedule.";
            }
            else
            {
                BHP.Models.DB.OutOfOffice outOf = new BHP.Models.DB.OutOfOffice()
                {
                    StaffId = staffID,
                    ReliverId = outOfOffice.ReliverId,
                    Comment = outOfOffice.Comment,
                    DateFrom = outOfOffice.DateFrom,
                    DateTo = outOfOffice.DateTo,
                    CreatedAt = DateTime.Now,
                    Status = GeneralClass._WAITING,
                    DeletedStatus = false
                };

                _context.OutOfOffice.Add(outOf);

                if (_context.SaveChanges() > 0)
                {
                    result = "Out Created";
                }
                else
                {
                    result = "Sorry! Something went wrong trying to create your out of office schedule.";
                }

            }

            _helpersController.LogMessages("Creating an out of office schedule see output => " + result,  _helpersController.getSessionEmail());

            return Json(result);
        }




        /*
        * Creating out of office module
        */
        // [Authorize(Roles = "DIRECTOR, AD, SUPERVISOR, HEAD GAS, INSPECTOR, TEAM LEAD, AD OPS, REVIEWER, SUPPORT, SUPER ADMIN")]
        public IActionResult EditOutOfOffice(BHP.Models.DB.OutOfOffice outOfOffice)
        {
            string result = "";

            var staffID =_helpersController.getSessionUserID();

            var office = _context.OutOfOffice.Where(x => x.OutId == outOfOffice.OutId && x.DeletedStatus == false);

            if (office.Count() <= 0)
            {
                result = "Sorry, no out of office schedule was found.";
            }
            else
            {
                office.FirstOrDefault().ReliverId = outOfOffice.ReliverId;
                office.FirstOrDefault().DateFrom = outOfOffice.DateFrom;
                office.FirstOrDefault().DateTo = outOfOffice.DateTo;
                office.FirstOrDefault().Comment = outOfOffice.Comment;
                office.FirstOrDefault().UpdatedAt = DateTime.Now;

                if (_context.SaveChanges() > 0)
                {
                    result = "Out Edited";
                }
                else
                {
                    result = "Sorry! Something went wrong trying to edit your out of office schedule.";
                }

            }

            _helpersController.LogMessages("Editing an out of office schedule see output => " + result,  _helpersController.getSessionEmail());

            return Json(result);
        }



        /*
         * Get specific out of office for a staff
         */
        //[Authorize(Roles = "DIRECTOR, AD, SUPERVISOR, HEAD GAS, INSPECTOR, TEAM LEAD, AD OPS, REVIEWER, SUPPORT, SUPER ADMIN")]
        public JsonResult GetStaffOutOfOffice()
        {
            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = HttpContext.Request.Form["start"].FirstOrDefault();
            var length = HttpContext.Request.Form["length"].FirstOrDefault();
            var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortColumnDir = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
            var txtSearch = HttpContext.Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var getOut = from o in _context.OutOfOffice
                         join s in _context.Staff on o.ReliverId equals s.StaffId
                         where o.StaffId == _helpersController.getSessionUserID() && o.DeletedStatus == false
                         select new
                         {
                             OutID = o.OutId,
                             Me = "ME",
                             Reliever = s.LastName + " " + s.FirstName + " (" + s.StaffEmail + ")",
                             RelieverID = s.StaffId,
                             DateFrom = o.DateFrom.ToString(),
                             DateTo = o.DateTo.ToString(),
                             CreatedAt = o.CreatedAt.ToString(),
                             UpdatedAt = o.UpdatedAt.ToString(),
                             Status = o.Status,
                             Comment = o.Comment
                         };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    getOut = sortColumn == "dateFrom" ? getOut.OrderByDescending(c => c.DateFrom) :
                               sortColumn == "updatedAt" ? getOut.OrderByDescending(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getOut.OrderByDescending(c => c.CreatedAt) :
                               sortColumn == "dateTo" ? getOut.OrderByDescending(c => c.DateTo) :
                               getOut.OrderByDescending(c => c.OutID + " " + sortColumnDir);
                }
                else
                {
                    getOut = sortColumn == "dateFrom" ? getOut.OrderBy(c => c.DateFrom) :
                               sortColumn == "updatedAt" ? getOut.OrderBy(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getOut.OrderBy(c => c.CreatedAt) :
                               sortColumn == "dateTo" ? getOut.OrderBy(c => c.DateTo) :
                               getOut.OrderBy(c => c.OutID);
                }

            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getOut = getOut.Where(c => c.Reliever.Contains(txtSearch.ToUpper()) || c.DateFrom.Contains(txtSearch) || c.DateTo.Contains(txtSearch) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
            }

            totalRecords = getOut.Count();
            var data = getOut.Skip(skip).Take(pageSize).ToList();

            _helpersController.LogMessages("Displaying my out of office records",  _helpersController.getSessionEmail());

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }



        //[Authorize(Roles = "DIRECTOR, AD, SUPERVISOR, HEAD GAS, INSPECTOR, TEAM LEAD, AD OPS, REVIEWER, SUPPORT, SUPER ADMIN")]
        public IActionResult AllOutOfOffice()
        {
            return View();
        }




        //[Authorize(Roles = "DIRECTOR, IT ADMIN, SUPER ADMIN, HEAD GAS, AD, PRINTER, DOC MANAGER, SUPERVISOR, INSPECTOR, REVIEWER, TEAM LEAD, SUPER ADMIN")]
        public IActionResult RelieveStaff()
        {
            return View();
        }




        /*
        * Get specific out of office for a staff
        */
        //[Authorize(Roles = "DIRECTOR, AD, SUPERVISOR, HEAD GAS, INSPECTOR, TEAM LEAD, AD OPS, REVIEWER, SUPPORT, SUPER ADMIN")]
        public JsonResult GetAllStaffOutOfOffice()
        {
            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = HttpContext.Request.Form["start"].FirstOrDefault();
            var length = HttpContext.Request.Form["length"].FirstOrDefault();
            var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortColumnDir = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
            var txtSearch = HttpContext.Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var getOut = from o in _context.OutOfOffice
                         join s in _context.Staff on o.ReliverId equals s.StaffId
                         join ss in _context.Staff on o.StaffId equals ss.StaffId
                         where o.DeletedStatus == false
                         select new
                         {
                             OutID = o.OutId,
                             Staff = ss.LastName + " " + ss.FirstName + " (" + ss.StaffEmail + ")",
                             Reliever = s.LastName + " " + s.FirstName + " (" + s.StaffEmail + ")",
                             DateFrom = o.DateFrom.ToString(),
                             DateTo = o.DateTo.ToString(),
                             CreatedAt = o.CreatedAt.ToString(),
                             UpdatedAt = o.UpdatedAt.ToString(),
                             Status = o.Status,
                             Comment = o.Comment
                         };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    getOut = sortColumn == "dateFrom" ? getOut.OrderByDescending(c => c.DateFrom) :
                               sortColumn == "updatedAt" ? getOut.OrderByDescending(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getOut.OrderByDescending(c => c.CreatedAt) :
                               sortColumn == "dateTo" ? getOut.OrderByDescending(c => c.DateTo) :
                               getOut.OrderByDescending(c => c.OutID + " " + sortColumnDir);
                }
                else
                {
                    getOut = sortColumn == "dateFrom" ? getOut.OrderBy(c => c.DateFrom) :
                               sortColumn == "updatedAt" ? getOut.OrderBy(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getOut.OrderBy(c => c.CreatedAt) :
                               sortColumn == "dateTo" ? getOut.OrderBy(c => c.DateTo) :
                               getOut.OrderBy(c => c.OutID);
                }

            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getOut = getOut.Where(c => c.Reliever.Contains(txtSearch.ToUpper()) || c.Staff.Contains(txtSearch.ToUpper()) || c.DateFrom.Contains(txtSearch) || c.DateTo.Contains(txtSearch) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
            }

            totalRecords = getOut.Count();
            var data = getOut.Skip(skip).Take(pageSize).ToList();

            _helpersController.LogMessages("Displaying all out of office staff",  _helpersController.getSessionEmail());

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }





        //[Authorize(Roles = "DIRECTOR, AD, SUPERVISOR, HEAD GAS, INSPECTOR, TEAM LEAD, AD OPS, REVIEWER, SUPPORT, SUPER ADMIN")]
        public JsonResult GetRelieveStaff()
        {

            var relieveStaff = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionUserID")).Trim());

            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = HttpContext.Request.Form["start"].FirstOrDefault();
            var length = HttpContext.Request.Form["length"].FirstOrDefault();
            var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortColumnDir = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
            var txtSearch = HttpContext.Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var getOut = from o in _context.OutOfOffice
                         join ss in _context.Staff on o.StaffId equals ss.StaffId
                         where o.DeletedStatus == false && o.ReliverId == relieveStaff && o.Status == GeneralClass._STARTED
                         select new
                         {
                             OutID = o.OutId,
                             Staff = ss.LastName + " " + ss.FirstName + " (" + ss.StaffEmail + ")",
                             DeskCount = _context.MyDesk.Where(c => c.StaffId == ss.StaffId && c.HasWork == false).Count(),
                             DateFrom = o.DateFrom.ToString(),
                             DateTo = o.DateTo.ToString(),
                             CreatedAt = o.CreatedAt.ToString(),
                             UpdatedAt = o.UpdatedAt.ToString(),
                             Status = o.Status,
                             Comment = o.Comment,
                             StaffEmail = ss.StaffEmail
                         };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    getOut = sortColumn == "dateFrom" ? getOut.OrderByDescending(c => c.DateFrom) :
                               sortColumn == "updatedAt" ? getOut.OrderByDescending(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getOut.OrderByDescending(c => c.CreatedAt) :
                               sortColumn == "dateTo" ? getOut.OrderByDescending(c => c.DateTo) :
                               getOut.OrderByDescending(c => c.OutID + " " + sortColumnDir);
                }
                else
                {
                    getOut = sortColumn == "dateFrom" ? getOut.OrderBy(c => c.DateFrom) :
                               sortColumn == "updatedAt" ? getOut.OrderBy(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getOut.OrderBy(c => c.CreatedAt) :
                               sortColumn == "dateTo" ? getOut.OrderBy(c => c.DateTo) :
                               getOut.OrderBy(c => c.OutID);
                }

            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getOut = getOut.Where(c => c.Staff.Contains(txtSearch.ToUpper()) || c.Staff.Contains(txtSearch.ToUpper()) || c.DateFrom.Contains(txtSearch) || c.DateTo.Contains(txtSearch) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
            }

            totalRecords = getOut.Count();
            var data = getOut.Skip(skip).Take(pageSize).ToList();

            _helpersController.LogMessages("Displaying all out of office staff to relieve",  _helpersController.getSessionEmail());

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }




        /*
         * Ending an out of office for a staff by support
         */
        // [Authorize(Roles = "DIRECTOR, AD, SUPERVISOR, HEAD GAS, INSPECTOR, TEAM LEAD, AD OPS, REVIEWER, SUPPORT, SUPER ADMIN")]
        public JsonResult FinishedOut(int OutID)
        {
            var result = "";

            var get = _context.OutOfOffice.Where(x => x.OutId == OutID && x.DeletedStatus == false);

            if (get.Any())
            {
                get.FirstOrDefault().Status = GeneralClass._FINISHED;
                get.FirstOrDefault().UpdatedAt = DateTime.Now;

                if (_context.SaveChanges() > 0)
                {
                    result = "Done";
                }
                else
                {
                    result = "Sorry! Something went wrong trying to end this out of office schedule";
                }
            }
            else
            {
                result = "Sorry! cannot find the selected out of office schedule.";
            }

            _helpersController.LogMessages("Ending an out of office for a staff. see output => " + result,  _helpersController.getSessionEmail());

            return Json(result);
        }



        /*
         * deleting an out of office by staff
         */
        //[Authorize(Roles = "DIRECTOR, AD, SUPERVISOR, HEAD GAS, INSPECTOR, TEAM LEAD, AD OPS, REVIEWER, SUPPORT, SUPER ADMIN")]
        public JsonResult DeleteOut(int OutID)
        {
            var result = "";

            var get = _context.OutOfOffice.Where(x => x.OutId == OutID);

            if (get.Any())
            {
                get.FirstOrDefault().DeletedStatus = true;
                get.FirstOrDefault().DeletedAt = DateTime.Now;
                get.FirstOrDefault().DeletedBy = _helpersController.getSessionUserID();

                if (_context.SaveChanges() > 0)
                {
                    result = "Done";
                }
                else
                {
                    result = "Sorry! Something went wrong trying to delete this out of office schedule";
                }
            }
            else
            {
                result = "Sorry! cannot find the selected out of office schedule.";
            }

            _helpersController.LogMessages("Deleteing an out of office by a staff. see output => " + result,  _helpersController.getSessionEmail());


            return Json(result);
        }




        /*
         * Trigger Star for out of office
         */
        //[Authorize(Roles = "DIRECTOR, AD, SUPERVISOR, HEAD GAS, INSPECTOR, TEAM LEAD, AD OPS, REVIEWER")]
        public JsonResult TriggerOutOfOfficeStart()
        {
            var result = 0;

            var get = from o in _context.OutOfOffice.AsEnumerable()
                      where o.Status == GeneralClass._WAITING && o.DeletedStatus == false
                      select o;


            foreach (var a in get.ToList())
            {
                if (a.DateFrom < DateTime.Now)
                {
                    var update = _context.OutOfOffice.Where(x => x.OutId == a.OutId);
                    update.FirstOrDefault().Status = GeneralClass._STARTED;
                    update.FirstOrDefault().UpdatedAt = DateTime.Now;
                    result += _context.SaveChanges();
                }
            }
            return Json(result);
        }



        /*
         * Trigger end for out of office
         */
        // [Authorize(Roles = "DIRECTOR, AD, SUPERVISOR, HEAD GAS, INSPECTOR, TEAM LEAD, AD OPS, REVIEWER")]
        public JsonResult TriggerOutOfOfficeEnd()
        {
            var result = 0;

            var get = from o in _context.OutOfOffice.AsEnumerable()
                      where o.Status == GeneralClass._STARTED && o.DeletedStatus == false
                      select o;

            foreach (var a in get.ToList())
            {
                if (a.DateTo < DateTime.Now)
                {
                    var update = _context.OutOfOffice.Where(x => x.OutId == a.OutId);
                    update.FirstOrDefault().Status = GeneralClass._FINISHED;
                    update.FirstOrDefault().UpdatedAt = DateTime.Now;
                    result += _context.SaveChanges();
                }
            }
            return Json(result);
        }




        //[Authorize(Roles = "DIRECTOR, AD, SUPERVISOR, HEAD GAS, INSPECTOR, TEAM LEAD, AD OPS, REVIEWER")]
        public JsonResult CountRelieveStaff()
        {
            var relieveStaff = _helpersController.getSessionUserID();
            var countRelieve = _context.OutOfOffice.Where(x => x.ReliverId == relieveStaff && x.DeletedStatus == false && x.Status == GeneralClass._STARTED).AsEnumerable().Count();
            return Json(countRelieve);
        }





        //[Authorize(Roles = "DIRECTOR, AD, SUPERVISOR, HEAD GAS, INSPECTOR, TEAM LEAD, AD OPS, REVIEWER")]
        public JsonResult SwitchAccount(string email)
        {
            var result = "Done";
            _helpersController.LogMessages("Switiching account for out of office for " + email,  _helpersController.getSessionEmail());
            return Json(result);
        }


    }
}
