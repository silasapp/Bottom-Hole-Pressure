using BHP.Helpers;
using BHP.HelpersClass;
using BHP.Models.DB;
using LpgLicense.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Security.Claims;
using Staff = BHP.Models.DB.Staff;

namespace BHP.Controllers.Authentication
{
    public class AuthController : Controller
    {
        private readonly BHP_DBContext _context;

        RestSharpServices _restService = new RestSharpServices();

        public IConfiguration _configuration;

        ElpsResponse elpsResponse = new ElpsResponse();
        Authentications auth = new Authentications();
        GeneralClass generalClass = new GeneralClass();
        IHttpContextAccessor _httpContextAccessor;
        HelpersController _helpersController;

        ElpsServices elpsServices;

        // session
        public const string sessionEmail = "_sessionEmail";
        public const string sessionUserID = "_sessionUserID";
        public const string sessionRoleID = "_sessionRoleID";
        public const string sessionLogin = "_sessionLogin";
        public const string sessionElpsID = "_sessionElpsID";
        public const string sessionRoleName = "_sessionRoleName";
        public const string sessionUserName = "_sessionUserName";
        public const string sessionTheme = "_sessionTheme";


        public AuthController(BHP_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            elpsServices = new ElpsServices();
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }




        public IActionResult  UserAuth(string email, string code)
        {
           var isSuccess = elpsServices.CodeCheck(email, code);

            if (isSuccess == true || isSuccess == false)
            {
                var log = HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);


                if (string.IsNullOrWhiteSpace(email))
                {
                    elpsResponse.message = "Oops... No result gotten from ELPS.";
                }
                else
                {
                    // Loginig from elps
                    var paramData = _restService.parameterData("staffEmail", email);
                    var response = _restService.Response("/api/Accounts/Staff/{staffEmail}/{email}/{apiHash}", paramData); // GET

                    if (response.ErrorException != null)
                    {
                        elpsResponse.message = _restService.ErrorResponse(response);
                    }
                    else
                    {
                        var res = JsonConvert.DeserializeObject<LpgLicense.Models.Staff>(response.Content);
                        // checking local staff database for staff

                        if (res == null)
                        {
                            // no staff check for company
                            var paramData2 = _restService.parameterData("compemail", email);
                            var response2 = _restService.Response("/api/company/{compemail}/{email}/{apiHash}", paramData2); // GET

                            if (response2.ErrorException != null)
                            {
                                elpsResponse.message = _restService.ErrorResponse(response2);
                            }
                            else
                            {
                                // checck company
                                var res2 = JsonConvert.DeserializeObject<CompanyDetail>(response2.Content);

                                if (res2 != null)
                                {

                                    string address = "";

                                    if (res2.registered_Address_Id != null || res2.registered_Address_Id != "0")
                                    {
                                        address = res2.registered_Address_Id;
                                    }
                                    else if (res2.operational_Address_Id != null || res2.operational_Address_Id != "0")
                                    {
                                        address = res2.operational_Address_Id;
                                    }

                                    // check address
                                    var paramDatas2 = _restService.parameterData("Id", address);
                                    var responses = _restService.Response("/api/Address/ById/{Id}/{email}/{apiHash}", paramDatas2); // GET


                                    var _company = (from s in _context.Companies where s.CompanyElpsId == res2.id select s);

                                    if (_company.Any())
                                    {
                                        if (_company.FirstOrDefault().DeleteStatus == true)
                                        {
                                            elpsResponse.message = email + " company has been deleted on this portal, please contact support.";
                                        }
                                        else if (_company.FirstOrDefault().ActiveStatus == false)
                                        {
                                            elpsResponse.message = email + " company has been deactivated on this portal, please contact support.";

                                        }
                                        else if (_company.FirstOrDefault().IsFirstTime == true)
                                        {
                                            _helpersController.LogMessages(email + " Needs to accept legal condictions", _company.FirstOrDefault().CompanyEmail.ToString());
                                            return RedirectToAction("LegalStuff", "Company", new { id = generalClass.Encrypt(_company.FirstOrDefault().CompanyId.ToString()) });
                                        }
                                        else
                                        {
                                            elpsResponse.message = email + " Company Found On Local DB.";
                                            elpsResponse.value = _company;

                                            if (responses != null)
                                            {
                                                var com = JsonConvert.DeserializeObject<Address>(responses.Content);

                                                if (com != null)
                                                {
                                                    //save address
                                                    var codes = generalClass.GetStateShortName(com.stateName.ToUpper(), "00" + _company.FirstOrDefault().CompanyId);

                                                    _company.FirstOrDefault().IdentificationCode = codes;
                                                    _company.FirstOrDefault().CompanyName = res2.name;
                                                    _company.FirstOrDefault().CompanyEmail = res2.user_Id;
                                                    _company.FirstOrDefault().Address = com.address_1;
                                                    _company.FirstOrDefault().City = com.city;
                                                    _company.FirstOrDefault().StateName = com.stateName.ToUpper();
                                                    _context.SaveChanges();
                                                }
                                                else
                                                {
                                                    // address not found
                                                    _company.FirstOrDefault().CompanyName = res2.name;
                                                    _company.FirstOrDefault().CompanyEmail = res2.user_Id;
                                                    _context.SaveChanges();
                                                }
                                            }
                                            else
                                            {
                                                _company.FirstOrDefault().CompanyName = res2.name.ToUpper();
                                                _company.FirstOrDefault().CompanyEmail = res2.user_Id;
                                                _context.SaveChanges();
                                            }

                                            Logins logins = new Logins()
                                            {
                                                UserId = _company.FirstOrDefault().CompanyId,
                                                RoleId = _company.FirstOrDefault().RoleId,
                                                HostName = auth.GetHostName(),
                                                MacAddress = auth.GetMACAddress(),
                                                LocalIp = auth.GetLocalIPAddress(),
                                                RemoteIp = HttpContext.GetRemoteIPAddress().ToString(),
                                                UserAgent = Request.Headers["User-Agent"].ToString(),
                                                LoginTime = DateTime.Now,
                                                LoginStatus = "Logged in",
                                            };

                                            _context.Logins.Add(logins);
                                            _context.SaveChanges();

                                            int lastLoginID = logins.LoginId;

                                            // get role name
                                            var roleName = _context.UserRoles.Where(x => x.RoleId == _company.FirstOrDefault().RoleId);

                                            var identity = new ClaimsIdentity(new[]
                                            {
                                            new Claim(ClaimTypes.Role, roleName.FirstOrDefault().RoleName),

                                        }, CookieAuthenticationDefaults.AuthenticationScheme);

                                            var principal = new ClaimsPrincipal(identity);
                                            var getin = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);


                                            HttpContext.Session.SetString(sessionEmail, generalClass.Encrypt(res2.user_Id));
                                            HttpContext.Session.SetString(sessionUserID, generalClass.Encrypt(_company.FirstOrDefault().CompanyId.ToString()));
                                            HttpContext.Session.SetString(sessionRoleID, generalClass.Encrypt(_company.FirstOrDefault().RoleId.ToString()));
                                            HttpContext.Session.SetString(sessionElpsID, generalClass.Encrypt(_company.FirstOrDefault().CompanyElpsId.ToString()));
                                            HttpContext.Session.SetString(sessionLogin, generalClass.Encrypt(lastLoginID.ToString()));
                                            HttpContext.Session.SetString(sessionRoleName, generalClass.Encrypt(roleName.FirstOrDefault().RoleName));
                                            HttpContext.Session.SetString(sessionUserName, generalClass.Encrypt(_company.FirstOrDefault().CompanyName));

                                            _helpersController.LogMessages(email + " Company Successfully logged in", _helpersController.getSessionEmail());

                                            return RedirectToAction("Index", "Company");
                                        }
                                    }
                                    else
                                    {
                                        elpsResponse.message = email + " This company was not found on  BHP PORTAL.... Creating company now.";

                                        Companies companies = new Companies()
                                        {
                                            CompanyElpsId = res2.id,
                                            CompanyEmail = res2.user_Id.Trim(),
                                            CompanyName = res2.name.ToUpper(),
                                            LocationId = _context.Location.Where(x => x.LocationName.Contains("CUS")).Select(x => x.LocationId).FirstOrDefault(),
                                            RoleId = _context.UserRoles.Where(x => x.RoleName.Contains("COMPANY")).Select(x => x.RoleId).FirstOrDefault(),
                                            ActiveStatus = true,
                                            CreatedAt = DateTime.Now,
                                            DeleteStatus = false,
                                            IsFirstTime = true
                                        };

                                        _context.Companies.Add(companies);

                                        int done = _context.SaveChanges();

                                        int lastEmail = companies.CompanyElpsId;

                                        if (done > 0)
                                        {
                                            _helpersController.LogMessages(email + " Company Successfully created. Logging user in", companies.CompanyEmail.ToString());

                                            return RedirectToAction("UserAuth", "Auth", new { email = companies.CompanyEmail });
                                        }
                                        else
                                        {
                                            elpsResponse.message = email + " Something went wrong trying to create your company. please try again.";
                                        }
                                    }
                                }
                                else
                                {
                                    elpsResponse.message = email + " company was not found on ELPS and BHP PORTAL....";
                                }
                            }
                        }
                        else
                        {
                            var _staff = (from s in _context.Staff where s.StaffElpsId.Trim() == res.userId.Trim() select s);

                            if (_staff.Any())
                            {
                                elpsResponse.message = email + " Staff Found On Local DB.";
                                elpsResponse.value = _staff;

                                if ((!string.IsNullOrWhiteSpace(_staff.FirstOrDefault().StaffEmail)) && _staff.FirstOrDefault().DeleteStatus == true)
                                {
                                    elpsResponse.message = email + " You have been deleted from this portal. Please contact administrator.";
                                    // redirect to deleted page page
                                }
                                else if ((!string.IsNullOrWhiteSpace(_staff.FirstOrDefault().StaffEmail)) && _staff.FirstOrDefault().ActiveStatus == false)
                                {
                                    elpsResponse.message = email + " You have been deactivated on this portal. Please contact administrator.";
                                    //redirect to deactivated page
                                }
                                else
                                {
                                    _staff.FirstOrDefault().StaffEmail = res.email;
                                    _context.SaveChanges();

                                    Logins logins = new Logins()
                                    {
                                        UserId = _staff.FirstOrDefault().StaffId,
                                        RoleId = _staff.FirstOrDefault().RoleId,
                                        HostName = auth.GetHostName(),
                                         MacAddress = auth.GetMACAddress(),
                                        LocalIp = auth.GetLocalIPAddress(),
                                        RemoteIp = HttpContext.GetRemoteIPAddress().ToString(),
                                        UserAgent = Request.Headers["User-Agent"].ToString(),
                                        LoginTime = DateTime.Now,
                                        LoginStatus = "Logged in",
                                    };

                                    _context.Logins.Add(logins);
                                    _context.SaveChanges();


                                    int lastLoginID = logins.LoginId;

                                    // get role name
                                    var roleName = _context.UserRoles.Where(x => x.RoleId == _staff.FirstOrDefault().RoleId);


                                    ClaimsPrincipal principal = new ClaimsPrincipal();

                                    var identity = new ClaimsIdentity(new[]
                                               {
                                            new Claim(ClaimTypes.Role, roleName.FirstOrDefault().RoleName.ToString()),

                                        }, CookieAuthenticationDefaults.AuthenticationScheme);

                                    var authProperties = new AuthenticationProperties
                                    {
                                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
                                    };


                                    principal = new ClaimsPrincipal(identity);
                                    principal.AddIdentity(identity);

                                    var getin = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

                                    HttpContext.Session.SetString(sessionEmail, generalClass.Encrypt(res.email));
                                    HttpContext.Session.SetString(sessionUserID, generalClass.Encrypt(_staff.FirstOrDefault().StaffId.ToString()));
                                    HttpContext.Session.SetString(sessionRoleID, generalClass.Encrypt(_staff.FirstOrDefault().RoleId.ToString()));
                                    HttpContext.Session.SetString(sessionElpsID, generalClass.Encrypt(_staff.FirstOrDefault().StaffElpsId.ToString()));
                                    HttpContext.Session.SetString(sessionLogin, generalClass.Encrypt(lastLoginID.ToString()));
                                    HttpContext.Session.SetString(sessionRoleName, generalClass.Encrypt(roleName.FirstOrDefault().RoleName));
                                    HttpContext.Session.SetString(sessionUserName, generalClass.Encrypt(_staff.FirstOrDefault().FirstName));
                                    HttpContext.Session.SetString(sessionTheme, generalClass.Encrypt(_staff.FirstOrDefault().Theme));

                                    if (principal.IsInRole(roleName.FirstOrDefault().RoleName.ToString()))
                                    {
                                        _helpersController.LogMessages(email + " Staff Successfully logged in", _helpersController.getSessionEmail());

                                        return RedirectToAction("Dashboard", "Staffs");
                                    }
                                }
                            }
                            else
                            {
                                Staff staff = new Staff()
                                {
                                    StaffElpsId = res.userId.Trim(),
                                    FieldOfficeId = 0,
                                    RoleId = 0,
                                    LocationId = 0,
                                    UpdatedBy = 0,
                                    StaffEmail = res.email.Trim(),
                                    FirstName = res.firstName.ToUpper(),
                                    LastName = res.lastName.ToUpper(),
                                    CreatedAt = DateTime.Now,
                                    ActiveStatus = false,
                                    DeleteStatus = false,
                                    Theme = "Light"
                                };

                                _context.Staff.Add(staff);
                                int saved = _context.SaveChanges();

                                int lastStaffID = staff.StaffId;

                                var updateStaff = from s in _context.Staff where s.StaffId == lastStaffID select s;
                                updateStaff.FirstOrDefault().CreatedBy = lastStaffID;
                                int save2 = _context.SaveChanges(); // updating self creating staff.

                                if (saved > 0)
                                {
                                    elpsResponse.message = email + " Staff successfully created from elps but not active";
                                }
                                else
                                {
                                    elpsResponse.message = email + " Staff not created. Try again later.";
                                }
                            }
                        }
                    }
                }

            }
            else
            {
                elpsResponse.message = email + " does not have authorization to access this portal";
            }

            _helpersController.LogMessages(elpsResponse.message);
            return RedirectToAction("Error", "Home", new { message = generalClass.Encrypt(elpsResponse.message) });
        }




        public ContentResult Logout()
        {
            string publicKey = _configuration.GetSection("ElpsKeys").GetSection("PK").Value.ToString();
            var elpsLogoff = ElpsServices._elpsBaseUrl + "Account/RemoteLogOff";
            var returnUrl = Url.Action("Index", "Home", null, Request.Scheme);

            var frm = "<form action='" + elpsLogoff + "' id='frmTest' method='post'>" +
                    "<input type='hidden' name='returnUrl' value='" + returnUrl + "' />" +
                    "<input type='hidden' name='appId' value='" + publicKey + "' />" +
                    "</form>" +
                    "<script>document.getElementById('frmTest').submit();</script>";

            if (string.IsNullOrWhiteSpace(_helpersController.getSessionLogin().ToString()))
            {
                return Content(frm, "text/html");
            }
            else
            {
                var logins = _context.Logins.Where(x => x.LoginId == _helpersController.getSessionLogin());

                logins.FirstOrDefault().LoginStatus = "Logout";
                logins.FirstOrDefault().LogoutTime = DateTime.Now;
                _context.SaveChanges();

                _helpersController.LogMessages(_helpersController.getSessionEmail() + " logged out successfully...", _helpersController.getSessionEmail());

                HttpContext.Session.Remove(sessionEmail);
                HttpContext.Session.Remove(sessionUserID);
                HttpContext.Session.Remove(sessionRoleID);
                HttpContext.Session.Remove(sessionElpsID);
                HttpContext.Session.Remove(sessionLogin);
                HttpContext.Session.Remove(sessionRoleName);
                HttpContext.Session.Remove(sessionUserName);
                HttpContext.Session.Remove(sessionTheme);

                var log = HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return Content(frm, "text/html");
            }
        }




        public IActionResult ChangePassword()
        {
            return View();
        }





        public JsonResult ChangePasswordAction(string OldPassword, string NewPassword, string ConfirmPassword)
        {
            string result = "";

            LpgLicense.Models.ChangePassword changePassword = new ChangePassword()
            {
                oldPassword = OldPassword,
                newPassword = NewPassword,
                confirmPassword = ConfirmPassword
            };

            var paramData = _restService.parameterData("useremail",  _helpersController.getSessionEmail());
            var response = _restService.Response("/api/Accounts/ChangePassword/{useremail}/{email}/{apiHash}", paramData, "POST", changePassword); // GET

            if (response.ErrorException != null)
            {
                result = _restService.ErrorResponse(response);
            }
            else
            {
                var res = JsonConvert.DeserializeObject<LpgLicense.Models.ChangePasswordResponse>(response.Content);

                if (res.code == 1)
                {
                    result = "Password Changed";
                }
                else
                {
                    result = "Password not change. " + res.msg;
                }
            }

            _helpersController.LogMessages("Result for user changing password " + result, _helpersController.getSessionEmail());

            return Json(result);

        }

    }
}