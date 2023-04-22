using BHP.Helpers;
using BHP.HelpersClass;
using BHP.Models.DB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BHP.Controllers.Configurations
{
    [Authorize]
    public class ApplicationProccessesController : Controller
    {
        private readonly BHP_DBContext _context;
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        HelpersController _helpersController;
        GeneralClass generalClass = new GeneralClass();

        public ApplicationProccessesController(BHP_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }

        // GET: ApplicationProccesses
        public async Task<IActionResult> Index()
        {
            return View(await _context.ApplicationProccess.ToListAsync());
        }



        /*
         * Application process list
         */
        public JsonResult GetApplicationProcess()
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

            var get = from ap in _context.ApplicationProccess.AsEnumerable()
                      join sf in _context.Staff.AsEnumerable() on ap.CreatedBy equals sf.StaffId into Staff
                      join sfu in _context.Staff.AsEnumerable() on ap.UpdatedBy equals sfu.StaffId into StaffU
                    
                      join r in _context.UserRoles.AsEnumerable() on ap.RoleId equals r.RoleId into Role
                      join ar in _context.UserRoles.AsEnumerable() on ap.OnAcceptRoleId equals ar.RoleId into AcceptRole
                      join rr in _context.UserRoles.AsEnumerable() on ap.OnRejectRoleId equals rr.RoleId into RejectRole
                      join l in _context.Location.AsEnumerable() on ap.LocationId equals l.LocationId into Location
                      where ((ap.DeleteStatus == false) && (Role.FirstOrDefault()?.DeleteStatus == false) && (Location.FirstOrDefault()?.DeleteStatus == false) && (Staff.FirstOrDefault()?.DeleteStatus == false))
                      select new
                      {
                          ProcessID = ap.ProccessId,
                         
                          RoleID = Role.FirstOrDefault()?.RoleId,
                          RoleName = Role.FirstOrDefault()?.RoleName,
                          LocationID = Location.FirstOrDefault()?.LocationId,
                          LocationName = Location.FirstOrDefault()?.LocationName,
                          Sort = ap.Sort,
                          AcceptRole = AcceptRole.FirstOrDefault()?.RoleName,
                          RejectRole = RejectRole.FirstOrDefault()?.RoleName,
                          Process = ap.Process,
                          CanPush = ap.CanPush == true ? "YES" : "NO",
                          CanWork = ap.CanWork == true ? "YES" : "NO",
                          CanAccept = ap.CanAccept == true ? "YES" : "NO",
                          CanReject = ap.CanReject == true ? "YES" : "NO",
                          CanReport = ap.CanReport == true ? "YES" : "NO",
                          CanInspect = ap.CanInspect == true ? "YES" : "NO",
                          CanSchdule = ap.CanSchdule == true ? "YES" : "NO",
                          CanRequest = ap.CanRequest == true ? "YES" : "NO",
                          CreatedAt = ap.CreatedAt.ToString(),
                      };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    get = sortColumn == "roleName" ? get.OrderByDescending(c => c.RoleName) :
                               sortColumn == "locationName" ? get.OrderByDescending(c => c.LocationName) :
                               sortColumn == "sort" ? get.OrderByDescending(c => c.Sort) :
                               sortColumn == "createdAt" ? get.OrderByDescending(c => c.CreatedAt) :

                               get.OrderByDescending(c => c.ProcessID + " " + sortColumnDir);
                }
                else
                {
                    get = sortColumn == "roleName" ? get.OrderBy(c => c.RoleName) :
                               sortColumn == "locationName" ? get.OrderBy(c => c.LocationName) :
                               sortColumn == "sort" ? get.OrderBy(c => c.Sort) :
                               sortColumn == "createdAt" ? get.OrderBy(c => c.CreatedAt) :

                               get.OrderBy(c => c.ProcessID + " " + sortColumnDir);
                }
            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                get = get.Where(c => c.LocationName.Contains(txtSearch.ToUpper()) || c.RoleName.Contains(txtSearch.ToUpper()) || c.CreatedAt.Contains(txtSearch));
            }

            totalRecords = get.Count();
            var data = get.Skip(skip).Take(pageSize).ToList();

            _helpersController.LogMessages("Displaying application processes",  _helpersController.getSessionEmail());

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }



        /*
         * Getting process for editing
         */
        // [Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public JsonResult GetProcess(int processID)
        {
            var get = _context.ApplicationProccess.Where(x => x.ProccessId == processID && x.DeleteStatus == false);

            _helpersController.LogMessages("Displaying single application process. Application Process ID : " + processID,  _helpersController.getSessionEmail());

            if (get.Any())
            {
                return Json(get.ToList());
            }
            else
            {
                return Json("The process was not found or have been deleted.");
            }
        }



        /*
         * Creating application process...
         */
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]

        public JsonResult CreateProcess(ApplicationProccess Proccess)
        {
            string result = "";

            var check = _context.ApplicationProccess.Where(x => x.RoleId == Proccess.RoleId && x.LocationId == Proccess.LocationId && x.Sort == Proccess.Sort && x.DeleteStatus == false);

            if (check.Any())
            {
                result = "This Application process already exits. Try a different process.";
            }
            else
            {
                Proccess.CreatedAt = DateTime.Now; 
                Proccess.CreatedBy = _helpersController.getSessionUserID();

                _context.ApplicationProccess.Add(Proccess);
                int done = _context.SaveChanges();

                if (done > 0)
                {
                    result = "Process Created";
                }
                else
                {
                    result = "Process not created. Something went wrong trying to create this process.";
                }
            }

            _helpersController.LogMessages("Creating application process. Status : " + result,  _helpersController.getSessionEmail());

            return Json(result);
        }



        /*
         * Editing Application process
         * 
         */
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public IActionResult EditProcess(int processID, ApplicationProccess Proccess)
        {
            string result = "";
            var check = _context.ApplicationProccess.Where(x => x.ProccessId == processID && x.DeleteStatus == false);

            if (check.Any())
            {
                check.FirstOrDefault().UpdatedAt = DateTime.Now;
                check.FirstOrDefault().UpdatedBy = _helpersController.getSessionUserID();
                check.FirstOrDefault().Sort = Proccess.Sort;
                check.FirstOrDefault().RoleId = Proccess.RoleId;
                check.FirstOrDefault().LocationId = Proccess.LocationId;
                check.FirstOrDefault().CanWork = Proccess.CanWork;
                check.FirstOrDefault().CanInspect = Proccess.CanInspect;
                check.FirstOrDefault().CanPush = Proccess.CanPush;
                check.FirstOrDefault().CanReject = Proccess.CanReject;
                check.FirstOrDefault().CanReport = Proccess.CanReport;
                check.FirstOrDefault().Process = Proccess.Process;
                check.FirstOrDefault().CanSchdule = Proccess.CanSchdule;
                check.FirstOrDefault().CanRequest = Proccess.CanRequest;
                check.FirstOrDefault().CanAccept = Proccess.CanAccept;
                check.FirstOrDefault().OnAcceptRoleId = Proccess.OnAcceptRoleId;
                check.FirstOrDefault().OnRejectRoleId = Proccess.OnRejectRoleId;
                check.FirstOrDefault().Process = Proccess.Process;

                int done = _context.SaveChanges();

                if (done > 0)
                {
                    result = "Process Updated";
                }
                else
                {
                    result = "Nothing was updated.";
                }
            }
            else
            {
                result = "This application process was not found or have been deleted.";
            }

            _helpersController.LogMessages("Updating application process. Status : " + result + " Application Process ID : " + processID,  _helpersController.getSessionEmail());

            return Json(result);
        }


        /*
         * Removing application process
         */
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public IActionResult DeleteProcess(int processID)
        {
            string response = "";

            var check = from s in _context.ApplicationProccess where s.ProccessId == processID && s.DeleteStatus == false select s;

            if (check.Any())
            {
                check.FirstOrDefault().DeleteStatus = true;
                check.FirstOrDefault().DeletedBy = _helpersController.getSessionUserID();
                check.FirstOrDefault().DeletedAt = DateTime.Now;

                int done = _context.SaveChanges();

                if (done > 0)
                {
                    response = "Process Removed";
                }
                else
                {
                    response = "Something went wrong trying to remove this process. Try again.";
                }
            }
            else
            {
                response = "This Application process was not found.";
            }

            _helpersController.LogMessages("Deleting application process. Status : " + response + " Application Process ID : " + processID,  _helpersController.getSessionEmail());

            return Json(response);
        }

    }
}
