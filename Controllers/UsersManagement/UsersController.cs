using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BHP.Helpers;
using BHP.HelpersClass;
using BHP.Models;
using BHP.Models.DB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace BHP.Controllers.UsersManagement
{
    [Authorize]
    public class UsersController : Controller
    {
        RestSharpServices restSharpServices = new RestSharpServices();

        private readonly BHP_DBContext _context;
        GeneralClass generalClass = new GeneralClass();
        HelpersController helpers;
        IHttpContextAccessor _httpContextAccessor;
        IConfiguration _configuration;

        private readonly IHostingEnvironment _env;

        

        public UsersController(BHP_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IHostingEnvironment env)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _env = env;
            helpers = new HelpersController(_context, _configuration, _httpContextAccessor);
        }


        //[Authorize(Roles = "DIRECTOR, SUPPORT, IT ADMIN, SUPER ADMIN, HEAD GAS, AD")]
        public IActionResult Index()
        {
            return View();
        }


        //[Authorize(Roles = "DIRECTOR, SUPPORT, IT ADMIN, SUPER ADMIN, HEAD GAS, AD")]
        public IActionResult Staff()
        {
            var response = restSharpServices.Response("api/Accounts/Staff/{email}/{apiHash}");
            var staffList = JsonConvert.DeserializeObject<List<LpgLicense.Models.Staff>>(response.Content);
            ViewBag.StaffList = staffList.ToList();

            return View();
        }


        /*
         * Getting all staff on elps
         */
        public JsonResult GetAllElpsStaff()
        {
            var response = restSharpServices.Response("api/Accounts/Staff/{email}/{apiHash}");

            if (response.ErrorException != null)
            {
                return Json(restSharpServices.ErrorResponse(response));
            }
            else
            {
                var staffList = JsonConvert.DeserializeObject<List<LpgLicense.Models.Staff>>(response.Content);
                //ViewBag.StaffList = staffList.ToList();

                return Json(JsonConvert.DeserializeObject(response.Content));
            }
        }



        /*
         * Getting aall staff on elps by email
         */
        public JsonResult GetElpsStaff(string staffemail)
        {
            var paramData = new List<ParameterData>();

            paramData.Add(new ParameterData
            {
                ParamKey = "staffEmail",
                ParamValue = staffemail.Trim()
            });

            var response = restSharpServices.Response("api/Accounts/Staff/{staffEmail}/{email}/{apiHash}", paramData);
            if (response.ErrorException != null)
            {
                return Json(restSharpServices.ErrorResponse(response));
            }
            else
            {
                return Json(JsonConvert.DeserializeObject<LpgLicense.Models.Staff>(response.Content));
            }
        }


        /*
         * Creating staff on local system
         */
        //[Authorize(Roles = "DIRECTOR, SUPPORT, IT ADMIN, SUPER ADMIN, HEAD GAS, AD")]
        public JsonResult CreateStaff(string ElpsHashID, string Email, string FirstName, string LastName, int RoleID, int FieldOfficeID, int LocationID, IFormFile StaffSignature)
        {
            string response = "";
            var newFileName = "";
            string db_path = "";


            var _staff = (from s in _context.Staff where s.StaffEmail == Email select s);

            if (_staff.Any())
            {
                response = "This staff already exits.";
            }
            else
            {
                if (StaffSignature != null)
                {
                    if (StaffSignature.Length > 0)
                    {
                        var randoneGuid = generalClass.Generate_Receipt_Number();
                        string extention = Path.GetFileName(StaffSignature.FileName);
                        newFileName = randoneGuid + "_" + extention;
                        string uploadsFolder = Path.Combine(_env.WebRootPath, "images\\Signature");
                        string filePath = Path.Combine(uploadsFolder, newFileName);

                        db_path = "~/images/Signature/" + newFileName;

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            StaffSignature.CopyTo(fileStream);
                        }
                    }
                }

                Staff staff = new Staff()
                {
                    StaffElpsId = ElpsHashID.Trim(),
                    FieldOfficeId = FieldOfficeID,
                    RoleId = RoleID,
                    StaffEmail = Email,
                    FirstName = FirstName.ToUpper(),
                    LastName = LastName.ToUpper(),
                    CreatedAt = DateTime.Now,
                    ActiveStatus = true,
                    DeleteStatus = false,
                    LocationId = LocationID,
                    SignaturePath = db_path,
                    SignatureName = newFileName,
                    Theme = "Light",
                    CreatedBy = helpers.getSessionUserID()
                };

                _context.Staff.Add(staff);
                int saved = _context.SaveChanges();

                if (saved > 0)
                {
                    response = "Staff Created";
                }
                else
                {
                    response = "Staff not created. Try again later.";
                }
            }

            helpers.LogMessages("Creating new staff. Status : " + response+ " Staff Email : "+ Email, helpers.getSessionEmail());
            return Json(response);
        }



        /*
         * Getting list of staff record on local system
         */
        //[Authorize(Roles = "DIRECTOR, SUPPORT, IT ADMIN, SUPER ADMIN, HEAD GAS, AD")]
        public JsonResult GetStaffRecord()
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

            var getStaff = from s in _context.Staff
                            join cs in _context.Staff on s.CreatedBy equals cs.StaffId into cstaff
                            from cst in cstaff.DefaultIfEmpty()
                            join r in _context.UserRoles on s.RoleId equals r.RoleId into role
                            from ro in role.DefaultIfEmpty()
                            join f in _context.FieldOffices on s.FieldOfficeId equals f.FieldOfficeId into field
                            from fd in field.DefaultIfEmpty()
                            join l in _context.Location on s.LocationId equals l.LocationId into location
                            from lo in location.DefaultIfEmpty()
                            where s.DeleteStatus == false
                            select new
                            {
                                StaffID = s.StaffId,
                                FirstName = s.FirstName,
                                LastName = s.LastName,
                                StaffEmail = s.StaffEmail,
                                CreatedBy = cst == null ? "" : cst.FirstName + " " + cst.LastName,
                                FieldOffice = fd == null ? "" : fd.OfficeName,
                                FieldOfficeId = fd == null ? 0 : fd.FieldOfficeId,
                                Role = ro == null ? "" : ro.RoleName,
                                RoleId = ro == null ? 0 : ro.RoleId,
                                ActiveStatus = s.ActiveStatus == true ? "Activated" : "Deactivated",
                                CreatedAt = s.CreatedAt.ToString(),
                                UpdatedAt = s.UpdatedAt.ToString(),
                                LocationName = lo == null ? "" : lo.LocationName,
                                LocationId = lo == null ? 0 : lo.LocationId,
                                Signature = s.SignatureName == null || s.SignatureName == "" ? "No Signature" : "Signature present",
                                SignaturePath = s.SignaturePath == null || s.SignaturePath == "" ? "~/images/Signature/TestSignature.jpg" : s.SignaturePath

                            };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    getStaff = sortColumn == "firstName" ? getStaff.OrderByDescending(c => c.FirstName) :
                               sortColumn == "lastName" ? getStaff.OrderByDescending(c => c.LastName) :
                               sortColumn == "staffEmail" ? getStaff.OrderByDescending(c => c.StaffEmail) :
                               sortColumn == "fieldOffice" ? getStaff.OrderByDescending(c => c.FieldOffice) :
                               sortColumn == "role" ? getStaff.OrderByDescending(c => c.Role) :
                               
                               sortColumn == "createdAt" ? getStaff.OrderByDescending(c => c.CreatedAt) :
                               
                               getStaff.OrderByDescending(c => c.StaffID + " " + sortColumnDir);
                }
                else
                {
                    getStaff = sortColumn == "firstName" ? getStaff.OrderBy(c => c.FirstName) :
                               sortColumn == "lastName" ? getStaff.OrderBy(c => c.LastName) :
                               sortColumn == "staffEmail" ? getStaff.OrderBy(c => c.StaffEmail) :
                               sortColumn == "fieldOffice" ? getStaff.OrderBy(c => c.FieldOffice) :
                               sortColumn == "role" ? getStaff.OrderBy(c => c.Role) :
                               
                               sortColumn == "createdAt" ? getStaff.OrderBy(c => c.CreatedAt) :
                               
                               getStaff.OrderBy(c => c.StaffID + " " + sortColumnDir);
                }
            }

            if (!string.IsNullOrWhiteSpace(txtSearch) || txtSearch != "")
            {
                getStaff = getStaff.Where(c =>  c.StaffEmail.Contains(txtSearch.ToUpper()) || c.FirstName.Contains(txtSearch.ToUpper()) || c.LastName.Contains(txtSearch.ToUpper()) || c.Role.Contains(txtSearch.ToUpper()));
            }

            totalRecords = getStaff.Count();
            var data = getStaff.Skip(skip).Take(pageSize).ToList();

            helpers.LogMessages("Displaying all staff records...", helpers.getSessionEmail());

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });
        }


        /*
         * Editing local staff information
         */
        [HttpPost]
        //[Authorize(Roles = "DIRECTOR, SUPPORT, IT ADMIN, SUPER ADMIN, HEAD GAS, AD")]
        public JsonResult Editstaff(int StaffID, int RoleID, int OfficeID, string FirstName, string LastName, int LocationID, IFormFile StaffSignature)
        {
            string response = "";
            var newFileName = "";
            string db_path = "";

            var _staff = (from s in _context.Staff where s.StaffId == StaffID && s.DeleteStatus == false select s);

            if (_staff.Any())
            {
                string rootFolder = Path.Combine(_env.WebRootPath, "images\\Signature");
                var signatureName = _staff.FirstOrDefault().SignatureName == null ? "xxx" : _staff.FirstOrDefault().SignatureName;
                string deletePath = Path.Combine(rootFolder, signatureName);

                if (System.IO.File.Exists(deletePath))
                {
                    System.IO.File.Delete(deletePath);
                }

                if (StaffSignature != null)
                {
                    if (StaffSignature.Length > 0)
                    {
                        var randoneGuid = generalClass.Generate_Receipt_Number();
                        string extention = Path.GetFileName(StaffSignature.FileName);
                        newFileName = randoneGuid + "_" + extention;
                        string uploadsFolder = Path.Combine(_env.WebRootPath, "images\\Signature");
                        string filePath = Path.Combine(uploadsFolder, newFileName);

                        db_path = "~/images/Signature/" + newFileName;

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            StaffSignature.CopyTo(fileStream);
                        }
                    }
                }

                _staff.FirstOrDefault().RoleId = RoleID;
                _staff.FirstOrDefault().FieldOfficeId = OfficeID;
                _staff.FirstOrDefault().FirstName = FirstName.ToUpper();
                _staff.FirstOrDefault().LastName = LastName.ToUpper();
                _staff.FirstOrDefault().UpdatedAt = DateTime.Now;
                _staff.FirstOrDefault().DeleteStatus = false;
                _staff.FirstOrDefault().LocationId = LocationID;
                _staff.FirstOrDefault().SignatureName = newFileName;
                _staff.FirstOrDefault().SignaturePath = db_path;

                _staff.FirstOrDefault().UpdatedBy = helpers.getSessionUserID();

                int updated = _context.SaveChanges();

                if (updated > 0)
                {
                    response = "Staff Updated";
                }
                else
                {
                    response = "Nothing was updated. Try again!";
                }
            }
            else
            {
                response = "The selected staff was not found.";
            }

            helpers.LogMessages("Updating staff details. Status : " + response + " Staff ID : " + StaffID, helpers.getSessionEmail());

            return Json(response);
        }



        /*
         * Deactivating a staff
         */
        //[Authorize(Roles = "DIRECTOR, SUPPORT, IT ADMIN, SUPER ADMIN, HEAD GAS, AD")]
        public JsonResult DeactivateStaff(int StaffID, string Status)
        {
            bool status = Status.Trim() == "Activated" ? true : false;

            string response = "";

            var _staff = from s in _context.Staff where s.StaffId == StaffID && s.DeleteStatus == false select s;

            if (_staff.Any())
            {
                _staff.FirstOrDefault().ActiveStatus = status;
                _staff.FirstOrDefault().UpdatedAt = DateTime.Now;
                _staff.FirstOrDefault().UpdatedBy = helpers.getSessionUserID();


                int done = _context.SaveChanges();

                if (done > 0)
                {
                    response = "Done";
                }
                else
                {
                    response = "Something went wron trying to " + Status + " this staff. Try again.";
                }
            }
            else
            {
                response = "This satff was not found.";
            }

            helpers.LogMessages("Deactivating Staff. Status : " + response + " Staff ID : "+ StaffID, helpers.getSessionEmail());

            return Json(response);
        }


        /*
         * Removing Staff
         */
        //[Authorize(Roles = "DIRECTOR, SUPPORT, IT ADMIN, SUPER ADMIN, HEAD GAS, AD")]
        public JsonResult RemoveStaff(int StaffID)
        {
            string response = "";
            var _staff = from s in _context.Staff where s.StaffId == StaffID && s.DeleteStatus == false select s;

            if (_staff.Any())
            {
                _staff.FirstOrDefault().ActiveStatus = false;
                _staff.FirstOrDefault().DeleteStatus = true;
                _staff.FirstOrDefault().DeletedBy = helpers.getSessionUserID();
                _staff.FirstOrDefault().DeletedAt = DateTime.Now;

                int done = _context.SaveChanges();

                if (done > 0)
                {
                    response = "Staff Removed";
                }
                else
                {
                    response = "Something went wron trying to remove this staff. Try again.";
                }
            }
            else
            {
                response = "This satff was not found.";
            }

            helpers.LogMessages("Removing Staff. Status : " + response + " Staff ID : " + StaffID, helpers.getSessionEmail());

            return Json(response);
        }




        /*
         * Getting the list of all companies.
         * 
         */
        //[Authorize(Roles = "DIRECTOR, SUPPORT, IT ADMIN, SUPER ADMIN, HEAD GAS, AD")]
        public IActionResult Company()
        {
            var company = _context.Companies.ToList();
            return View(company);
        }




        /*
         * An action to perform company activation, remove, restore or deactivation
         */
        //[Authorize(Roles = "DIRECTOR, SUPPORT, IT ADMIN, SUPER ADMIN, HEAD GAS, AD")]
        public JsonResult CompanyAction(string CompID, string option, string response)
        {
            string result = "";

            if (string.IsNullOrWhiteSpace(CompID))
            {
                result = "Error, Company link is broken or not in correct format.";
            }

            int compid = 0;
            var comp_id = generalClass.Decrypt(CompID);

            if (comp_id == "Error")
            {
                result = "Error, Company link is broken or not in correct format.";
            }
            else
            {
                compid = Convert.ToInt32(comp_id);

                var company = _context.Companies.Where(x => x.CompanyId == compid);

                if (company.Any())
                {
                    if (option == "activate")
                    {
                        company.FirstOrDefault().ActiveStatus = true;
                        company.FirstOrDefault().UpdatedAt = DateTime.Now;
                    }
                    else if (option == "deactivate")
                    {
                        company.FirstOrDefault().ActiveStatus = false;
                        company.FirstOrDefault().UpdatedAt = DateTime.Now;
                    }
                    else if (option == "delete")
                    {
                        company.FirstOrDefault().DeleteStatus = true;
                        company.FirstOrDefault().DeletedBy = helpers.getSessionUserID();
                        company.FirstOrDefault().DeletedAt = DateTime.Now;
                        company.FirstOrDefault().UpdatedAt = DateTime.Now;
                    }
                    else if (option == "restore")
                    {
                        company.FirstOrDefault().DeleteStatus = false;
                        company.FirstOrDefault().UpdatedAt = DateTime.Now;
                    }
                    else
                    {
                        result = "The option entered does not match the current operation. Please contact support for this operation.";
                    }

                    if (_context.SaveChanges() > 0)
                    {
                        result = response;
                    }
                    else
                    {
                        result = "Something went wrong tying to perform current operation, please try again later.";
                    }
                }
                else
                {
                    result = "Something went wrong. Company cannot be found.";
                }
            }

            helpers.LogMessages("Company Actiion : " + option + " Status : " + result + " Company ID : " + compid, helpers.getSessionEmail());
            return Json(result);
        }





       // [Authorize(Roles = "DIRECTOR, SUPPORT, IT ADMIN, SUPER ADMIN, HEAD GAS, AD")]
        public IActionResult CompanyLogins(int id)
        {
            var login = from l in _context.Logins
                        join r in _context.UserRoles on l.RoleId equals r.RoleId
                        join c in _context.Companies on l.UserId equals c.CompanyId
                        where r.RoleName.Contains("COMPANY")
                        select new UserLogins
                        {
                            ID = c.CompanyId,
                            Name = c.CompanyName,
                            Email = c.CompanyEmail,
                            Role = r.RoleName,
                            HostName = l.HostName,
                            MacAddress = l.MacAddress,
                            LocalIp = l.LocalIp,
                            RemoteIp = l.RemoteIp,
                            UserAgent = l.UserAgent,
                            Status = l.LoginStatus,
                            LogInTime = l.LoginTime,
                            LogOutTime = (DateTime)l.LogoutTime
                        };

            ViewData["LoginTitle"] = "All Company's Logins";

            if (login.Any())
            {
                if (id != 0)
                {
                    login = login.Where(x => x.ID == id);
                    ViewData["LoginTitle"] = "All Logins for " + login.FirstOrDefault().Name;
                }
                helpers.LogMessages("Displaying " + ViewData["LoginTitle"], helpers.getSessionEmail());
                return View(login.ToList());
            }
            else
            {
                return View();
            }
        }



       // [Authorize(Roles = "DIRECTOR, SUPPORT, IT ADMIN, SUPER ADMIN, HEAD GAS, AD")]
        public IActionResult StaffLogins(int id)
        {
            var login = from l in _context.Logins
                        join r in _context.UserRoles on l.RoleId equals r.RoleId
                        join s in _context.Staff on l.UserId equals s.StaffId
                        join f in _context.FieldOffices on s.FieldOfficeId equals f.FieldOfficeId
                        where !r.RoleName.Contains("COMPANY")
                        select new UserLogins
                        {
                            ID = s.StaffId,
                            Name = s.LastName + " " + s.FirstName,
                            Email = s.StaffEmail,
                            Role = r.RoleName,
                            HostName = l.HostName,
                            FieldOffice = f.OfficeName,
                            MacAddress = l.MacAddress,
                            LocalIp = l.LocalIp,
                            RemoteIp = l.RemoteIp,
                            UserAgent = l.UserAgent,
                            Status = l.LoginStatus,
                            LogInTime = l.LoginTime,
                            LogOutTime = (DateTime)l.LogoutTime
                        };

            ViewData["LoginTitle"] = "All Staff's Logins";

            if (login.Any())
            {
                if (id != 0)
                {
                    login = login.Where(x => x.ID == id);
                    ViewData["LoginTitle"] = "All Logins for " + login.FirstOrDefault()?.Name;
                }
                helpers.LogMessages("Displaying " + ViewData["LoginTitle"], helpers.getSessionEmail());
                return View(login.ToList());
            }
            else
            {
                return View();
            }
        }


        /*
         * Displaying all acctivities done by all or specific user.
         */ 
       // [Authorize(Roles = "DIRECTOR, SUPPORT, IT ADMIN, SUPER ADMIN, HEAD GAS, AD")]
        public IActionResult Activities(string id)
        {
            var act = _context.AuditTrail.OrderByDescending(x => x.CreatedAt).ToList();
            ViewData["ActivityTitle"] = "Activities for all users";

            if (id != null)
            {
                act = act.Where(x => x.UserId == id).ToList();
                ViewData["ActivityTitle"] = "Activities for " + id;
            }

            return View(act);
        }


        /*
         * To check if user is still logged in
         */
        public JsonResult CheckSession()
        {
            try
            {
                string result = "";

                if (string.IsNullOrWhiteSpace(helpers.getSessionEmail()) || helpers.getSessionEmail() == "")
                {
                    result = "true";
                }
                return Json(result);
            }
            catch(Exception ex)
            {
                return Json("true");
            }
        }

    }
}