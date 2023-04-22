using BHP.Controllers.Authentication;
using BHP.Helpers;
using BHP.HelpersClass;
using BHP.Models;
using BHP.Models.DB;
using BHP.Models.MyModel;
using LpgLicense.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BHP.Controllers.Company
{
    [Authorize]
    public class CompanyController : Controller
    {
        private readonly BHP_DBContext _context;
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        HelpersController _helpersController;
        GeneralClass generalClass = new GeneralClass();
        RestSharpServices _restService = new RestSharpServices();
        private IHostingEnvironment _hostingEnvironment;

        public CompanyController(BHP_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IHostingEnvironment environment)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
            _hostingEnvironment = environment;
        }



        [Authorize(Roles = "COMPANY")]
        public IActionResult Index()
        {
            var paramData = _restService.parameterData("compemail", _helpersController.getSessionEmail());
            var response = _restService.Response("/api/company/{compemail}/{email}/{apiHash}", paramData); // GET

            if (response.ErrorException != null || string.IsNullOrWhiteSpace(response.Content))
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, this can be a netwrok related or an error. Please try agin later") });
            }
            else
            {
                // checck company
                var res = JsonConvert.DeserializeObject<CompanyDetail>(response.Content);

                if (string.IsNullOrWhiteSpace(res.contact_FirstName) || string.IsNullOrWhiteSpace(res.contact_LastName.ToString()) || (res.registered_Address_Id == null && res.operational_Address_Id == null))
                {
                    return RedirectToAction("CompanyInformation", "Company", new { message = generalClass.Encrypt("Please complete company's profile, addresses before proceeding.") });
                }

                else
                {
                    // checking for directors

                    var paramData2 = _restService.parameterData("CompId", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionElpsID")));
                    var response2 = _restService.Response("/api/Directors/{CompId}/{email}/{apiHash}", paramData2);

                    if (response2.ErrorException != null || string.IsNullOrWhiteSpace(response2.Content))
                    {
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, this can be a netwrok related or an error. Please try agin later") });
                    }
                    else
                    {
                        var res2 = JsonConvert.DeserializeObject<List<Directors>>(response2.Content);

                        if (res2.Any())
                        {
                            // saving company code 

                            string address = "";

                            if (res.registered_Address_Id != null || res.registered_Address_Id != "0")
                            {
                                address = res.registered_Address_Id;
                            }
                            else if (res.operational_Address_Id != null || res.operational_Address_Id != "0")
                            {
                                address = res.operational_Address_Id;
                            }

                            var paramDatas = _restService.parameterData("Id", address);
                            var responses = _restService.Response("/api/Address/ById/{Id}/{email}/{apiHash}", paramDatas); // GET

                            if (responses != null)
                            {
                                var getCom = _context.Companies.Where(x => x.CompanyId == _helpersController.getSessionUserID());

                                if (getCom.Any())
                                {
                                    var CODE = (getCom.FirstOrDefault().CompanyId);
                                    var com = JsonConvert.DeserializeObject<Address>(responses.Content);
                                    var code = generalClass.GetStateShortName(com.stateName.ToUpper(), "00" + CODE);

                                    getCom.FirstOrDefault().IdentificationCode = code;
                                    getCom.FirstOrDefault().Address = com.address_1;
                                    getCom.FirstOrDefault().City = com.city;
                                    getCom.FirstOrDefault().StateName = com.stateName.ToUpper();
                                    getCom.FirstOrDefault().UpdatedAt = DateTime.Now;

                                    if (_context.SaveChanges() > 0)
                                    {
                                        return RedirectToAction("Dashboard", "Company");
                                    }
                                    else
                                    {
                                        return RedirectToAction("CompanyInformation", "Company", new { message = generalClass.Encrypt("Please complete company's profile, addresses before proceeding.") });
                                    }
                                }

                                return RedirectToAction("Dashboard", "Company");
                            }
                            else
                            {
                                return RedirectToAction("CompanyInformation", "Company", new { message = generalClass.Encrypt("Company address not found, please add a registered address or operational address.") });
                            }
                        }
                        else
                        {
                            return RedirectToAction("CompanyInformation", "Company", new { message = generalClass.Encrypt("Please add or update director's information before proceeding.") });
                        }
                    }
                }
            }
        }





        [AllowAnonymous]
        [Authorize(Roles = "COMPANY")]
        public IActionResult LegalStuff(string id)
        {
            if (id == null || string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Error", "Home", new { message = generalClass.Encrypt("Opps... No company was found or passed") });
            }
            else
            {
                ViewData["cid"] = id;
                return View();
            }
        }





        [AllowAnonymous]
        [Authorize(Roles = "COMPANY")]
        public IActionResult AcceptLegal(string id)
        {
            if (id == null || string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Error", "Home", new { message = generalClass.Encrypt("Opps... This company was not found and cannot accept tearms and condictions. Please try again.") });
            }
            else
            {
                int cmid = Convert.ToInt32(generalClass.Decrypt(id));

                var company = (from c in _context.Companies where c.CompanyId == cmid select c);

                if (company.Any())
                {
                    company.FirstOrDefault().IsFirstTime = false;
                    company.FirstOrDefault().UpdatedAt = DateTime.Now;

                    string content = "Hello " + company.FirstOrDefault().CompanyName + ", Welcome to Bottom Hole Pressure Portal.";

                    if (_context.SaveChanges() > 0)
                    {
                        _helpersController.LogMessages("Company accepted legal condictions", company.FirstOrDefault().CompanyEmail);
                        return RedirectToAction("UserAuth", "Auth", new { email = company.FirstOrDefault().CompanyEmail });
                    }
                    else
                    {
                        return RedirectToAction("Error", "Home", new { message = generalClass.Encrypt("Opps... Something went wrong trying to accept your tearms and condictions. Please try again.") });
                    }
                }
                else
                {
                    return RedirectToAction("Error", "Home", new { message = generalClass.Encrypt("Opps... This company was not found and cannot accept tearms and condictions. Please try again.") });
                }
            }
        }





        [Authorize(Roles = "COMPANY")]
        public IActionResult CompanyInformation(string message = null)
        {
            var msg = "";

            if (message != null)
            {
                msg = generalClass.Decrypt(message);
            }

            ViewData["Message"] = msg;

            return View();
        }



        [HttpPost]
        public IActionResult GetApplications()
        {
            var reqid = Convert.ToInt32(Request.Form["RequestId"]); //Request.Form.Get("min1");

            //reqid = 1;
            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = HttpContext.Request.Form["start"].FirstOrDefault();
            var length = HttpContext.Request.Form["length"].FirstOrDefault();

            var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();

            var searchTxt = HttpContext.Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var today = DateTime.Now.Date;
            var Apps = (from a in _context.Application
                        where a.RequestId == reqid
                        select new
                        {
                            a.RequestId,
                            a.Qrt,
                            a.Fields,
                            a.Reservoir,
                            InitialRpressure = a.InitialRpressure.ToString(),
                            RbubblePointPressure = a.RbubblePointPressure.ToString(),
                            a.WellName,
                            LastSurveyDate = a.LastSurveyDate.ToString(),
                            MeasuredRpressure = a.MeasuredRpressure.ToString(),
                            a.TimeShut,
                            a.UsedInstrument,
                            OperaionWellCost = a.OperaionWellCost.ToString(),
                            ReservoirCreatedAt = a.ReservoirCreatedAt.ToString()
                        });

            if (!string.IsNullOrEmpty(searchTxt))
            {
                Apps = Apps.Where(u => u.Qrt.Contains(searchTxt) || u.Fields.Contains(searchTxt) || u.WellName.Contains(searchTxt) || u.LastSurveyDate.Contains(searchTxt)
               || u.Fields.Contains(searchTxt) || u.Reservoir.Contains(searchTxt) || u.InitialRpressure.Contains(searchTxt) || u.RbubblePointPressure.Contains(searchTxt)
                || u.MeasuredRpressure.Contains(searchTxt) || u.TimeShut.Contains(searchTxt) || u.UsedInstrument.Contains(searchTxt) || u.OperaionWellCost.Contains(searchTxt) || u.ReservoirCreatedAt.Contains(searchTxt));
            }
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                Apps = Apps.OrderBy(s => s.RequestId + " " + sortColumnDir);
            }


            totalRecords = Apps.Count();
            var data = Apps.Skip(skip).Take(pageSize).ToList();

            _helpersController.LogMessages("Getting all Company's Applications", _helpersController.getSessionEmail());


            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }



        [HttpPost]
        public IActionResult GetSurveyResult()
        {
            var reqid = Convert.ToInt32(Request.Form["RequestId"]); //Request.Form.Get("min1");

            //reqid = 1;
            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = HttpContext.Request.Form["start"].FirstOrDefault();
            var length = HttpContext.Request.Form["length"].FirstOrDefault();

            var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();

            var searchTxt = HttpContext.Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var today = DateTime.Now.Date;
            var Apps = (from a in _context.SurveyReport
                        where a.RequestId == reqid
                        select new
                        {
                            a.RequestId,
                            a.Field,
                            a.Reservior,
                            RemainingReserve = a.RemainingReserve.ToString(),
                            a.WellName,
                            DateOfSurvey = a.DateOfSurvey.ToString(),
                            Oil = a.Oil.ToString(),
                            Water = a.Water.ToString(),
                            Gas = a.Gas.ToString(),
                            Bsw = a.Bsw.ToString(),
                            Gor = a.Gor.ToString(),
                            Bean = a.Bean.ToString(),
                            DatumDepth = a.DatumDepth.ToString(),
                            InitialPressure = a.InitialPressure.ToString(),
                            Thp = a.Thp.ToString(),
                            Chp = a.Chp.ToString(),
                            FbhpMp = a.FbhpMp.ToString(),
                            SbhpMp = a.SbhpMp.ToString(),
                            RpDatum = a.RpDatum.ToString(),
                            ProductivityIndex = a.ProductivityIndex.ToString(),
                            DateOfLastSurvey = a.DateOfLastSurvey.ToString(),
                            a.Remark
                        });

            if (!string.IsNullOrEmpty(searchTxt))
            {
                Apps = Apps.Where(u => u.Field.Contains(searchTxt) || u.Reservior.Contains(searchTxt) || u.WellName.Contains(searchTxt) || u.RemainingReserve.Contains(searchTxt)
               || u.DateOfSurvey.Contains(searchTxt) || u.Oil.Contains(searchTxt) || u.Water.Contains(searchTxt) || u.Gas.Contains(searchTxt)
                || u.Bsw.Contains(searchTxt) || u.Gor.Contains(searchTxt) || u.Bean.Contains(searchTxt) || u.DatumDepth.Contains(searchTxt) || u.InitialPressure.Contains(searchTxt)
                || u.Thp.Contains(searchTxt) || u.Chp.Contains(searchTxt) || u.FbhpMp.Contains(searchTxt) || u.SbhpMp.Contains(searchTxt) || u.RpDatum.Contains(searchTxt)
                || u.ProductivityIndex.Contains(searchTxt) || u.DateOfLastSurvey.Contains(searchTxt) || u.Remark.Contains(searchTxt));
            }
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                Apps = Apps.OrderBy(s => s.RequestId + " " + sortColumnDir);
            }


            totalRecords = Apps.Count();
            var data = Apps.Skip(skip).Take(pageSize).ToList();



            _helpersController.LogMessages("Getting Company's Survey Result", _helpersController.getSessionEmail());


            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }





        private string fileName { get; set; }
        public FileResult downloadFile(string filePath)
        {
            IFileProvider provider = new PhysicalFileProvider(filePath);
            IFileInfo fileInfo = provider.GetFileInfo(fileName);
            var readStream = fileInfo.CreateReadStream();
            var mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(readStream, mimeType, fileName);
        }



        [HttpGet]
        public IActionResult TemplateADownload()
        {
            string wwwrootPath = _hostingEnvironment.WebRootPath;
            fileName = @"Template\Template_A.xlsx";
            FileInfo file = new FileInfo(Path.Combine(wwwrootPath, fileName));

            _helpersController.LogMessages("Downloading Template A by company", _helpersController.getSessionEmail());

            return downloadFile(wwwrootPath);
        }



        [HttpGet]
        public IActionResult TemplateBDownload()
        {
            string wwwrootPath = _hostingEnvironment.WebRootPath;
            fileName = @"Template\Template_B.xlsx";
            FileInfo file = new FileInfo(Path.Combine(wwwrootPath, fileName));

            _helpersController.LogMessages("Downloading Template B by company", _helpersController.getSessionEmail());

            return downloadFile(wwwrootPath);
        }




        public JsonResult SubmitApplication(string id) // application id
        {
            string response = "";
            string message = "";
            string txtComment = "Application submitted";
            var myrequestid = generalClass.DecryptIDs(id);

            if (myrequestid == 0)
            {
                response = "Wrong RequestID";
                message = "Something went wrong, request ID is incorrect. Please try agin later.";
            }
            else
            {
                int AppDropStaffID = _helpersController.ApplicationDropStaff(1);
                List<ApplicationProccess> process = _helpersController.GetAppProcess(0, 1);

                if (AppDropStaffID > 0 && process.Any())
                {
                    var checkdesk = _context.MyDesk.Where(x => x.RequestId == myrequestid && x.ProcessId == process.FirstOrDefault().ProccessId && x.Sort == process.FirstOrDefault().Sort && x.HasWork == false);

                    if (checkdesk.Any())
                    {
                        response = "Already Exist";
                        message = "Sorry, this application is already on a staff desk.";
                    }
                    else
                    {
                        MyDesk desk = new MyDesk()
                        {
                            ProcessId = process.FirstOrDefault().ProccessId,
                            RequestId = myrequestid,
                            StaffId = AppDropStaffID,
                            HasWork = false,
                            CreatedAt = DateTime.Now,
                            HasPushed = false,
                            Sort = process.FirstOrDefault().Sort,
                        };

                        _context.MyDesk.Add(desk);

                        if (_context.SaveChanges() > 0)
                        {
                            var apps = _context.RequestProposal.Where(x => x.RequestId == myrequestid && x.DeletedStatus == false);

                            if (apps.Any())
                            {
                                apps.FirstOrDefault().UpdatedAt = DateTime.Now;
                                apps.FirstOrDefault().CurrentDeskId = AppDropStaffID;
                                apps.FirstOrDefault().Status = GeneralClass.Processing;
                                apps.FirstOrDefault().IsSubmitted = true;

                                if (_context.SaveChanges() > 0)
                                {
                                    var app = _context.RequestProposal.Where(x => x.RequestId == myrequestid);

                                    string subject = "Application Submitted with Ref : " + app.FirstOrDefault().RequestRefNo;
                                    string content = "You have submitted your application with Refrence Number " + app.FirstOrDefault().RequestRefNo + " for processing on NUPRC Bottom Hole Pressure (BHP) portal. Kindly find other details below.";

                                    _helpersController.SaveMessage(myrequestid, app.FirstOrDefault().CompanyId, subject, content);

                                    var user = _context.Staff.Where(x => x.StaffId == AppDropStaffID).FirstOrDefault();

                                    var actionFrom = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());
                                    var actionTo = _helpersController.getActionHistory(user.RoleId, user.StaffId);

                                    _helpersController.SaveHistory(myrequestid, actionFrom, actionTo, "Moved", "Application landed on staff desk =>" + txtComment);

                                    response = "saved";
                                    message = "Your application was submitted successfully";
                                }
                                else
                                {
                                    response = "update failed";
                                    message = "Something went wrong trying to update your application status";
                                }
                            }
                            else
                            {
                                response = "deleted";
                                message = "This application has been deleted";
                            }
                        }
                        else
                        {
                            message = "Something went wrong trying to submit your application.";
                        }
                    }
                }
                else
                {
                    response = "update failed";
                    message = "Something went wrong trying to submit your application - Staff not found. Please try again later.";
                }
            }

            _helpersController.LogMessages("Result from submitting application process :::: Ressponse => " + response + "; Message => " + message, _helpersController.getSessionEmail());
            return Json(new { response, message });

        }



        /*
         * Upload company's document first time
         * 
         * id => encrypted application id
         */
        [Authorize(Roles = "COMPANY")]
        public IActionResult UploadDocument(string id) // application id
        {
            int app_id = generalClass.DecryptIDs(id);

            if (string.IsNullOrWhiteSpace(id) && app_id == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Request Proposal not found or not in correct format . Kindly contact support.") });
            }
            else
            {
                var doc = _context.ApplicationDocuments.Where(x => x.DocType == "Company" && x.DocName.Trim().Contains(GeneralClass.company_acknowledgement_doc)).FirstOrDefault();

                var appDetails = from app in _context.RequestProposal
                                 join c in _context.Companies on app.CompanyId equals c.CompanyId
                                 where (app.DeletedStatus == false && app.RequestId == app_id && c.DeleteStatus == false)
                                 select new
                                 {
                                     RequestID = app.RequestId,
                                     AppRef = app.RequestRefNo,
                                     LocalCompanyID = c.CompanyId,
                                     ElpsCompanyID = c.CompanyElpsId,
                                     AppDocID = doc.AppDocId,
                                     EplsDocTypeID = doc.ElpsDocTypeId,
                                     DocName = doc.DocName,
                                     docType = doc.DocType,
                                 };

                List<PresentDocuments> presentDocuments = new List<PresentDocuments>();
                List<MissingDocument> missingDocuments = new List<MissingDocument>();
                List<BothDocuments> bothDocuments = new List<BothDocuments>();

                if (appDetails.Any())
                {
                    ViewData["RequestID"] = appDetails.FirstOrDefault().RequestID;
                    ViewData["CompanyElpsID"] = appDetails.FirstOrDefault().ElpsCompanyID;
                    ViewData["AppReference"] = appDetails.FirstOrDefault().AppRef;

                    List<LpgLicense.Models.Document> companyDoc = generalClass.getCompanyDocuments(appDetails.FirstOrDefault().ElpsCompanyID.ToString());

                    if (companyDoc == null)
                    {
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("A network related problem. Please connect to a network.") });
                    }
                    else
                    {
                        foreach (var appDoc in appDetails.ToList())
                        {
                            if (appDoc.docType == "Company")
                            {
                                foreach (var cDoc in companyDoc)
                                {
                                    if (cDoc.document_type_id == appDoc.EplsDocTypeID.ToString())
                                    {
                                        presentDocuments.Add(new PresentDocuments
                                        {
                                            Present = true,
                                            FileName = cDoc.fileName,
                                            Source = cDoc.source,
                                            CompElpsDocID = cDoc.id,
                                            DocTypeID = Convert.ToInt32(cDoc.document_type_id),
                                            LocalDocID = appDoc.AppDocID,
                                            DocType = appDoc.docType,
                                            TypeName = cDoc.documentTypeName
                                        });
                                    }
                                }
                            }
                        }

                        var result = appDetails.Where(x => !presentDocuments.Any(x2 => x2.LocalDocID == x.AppDocID));

                        foreach (var r in result)
                        {
                            missingDocuments.Add(new MissingDocument
                            {
                                Present = false,
                                DocTypeID = r.EplsDocTypeID,
                                LocalDocID = r.AppDocID,
                                DocType = r.docType,
                                TypeName = r.DocName
                            });
                        }

                        presentDocuments = presentDocuments.GroupBy(x => x.TypeName).Select(c => c.FirstOrDefault()).ToList();

                        bothDocuments.Add(new BothDocuments
                        {
                            missingDocuments = missingDocuments,
                            presentDocuments = presentDocuments,
                        });
                    }

                }
                else
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Trying to find Application(Type, Stage or documents). Kindly contact support.") });
                }

                _helpersController.LogMessages("Displaying company upload documents", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

                return View(bothDocuments.ToList());
            }
        }




        /*
        * Submitting application documents for the first time, and also submitting for processing
        * 
        * AppID => encrypted application ID 
        * SubmittedDocuments => a list of documents to be submitted
        */

        public IActionResult SubmitDocuments(string RequestID, List<SubmitDoc> SubmittedDocuments)
        {
            var result = "";
            int appID = generalClass.DecryptIDs(RequestID);

            if (appID == 0)
            {
                result = "Opps! Application link not in correct format. Please try again later.";
            }
            else
            {
                var getApps = _context.RequestProposal.Where(x => x.RequestId == appID && x.DeletedStatus == false);
                // getting application process for single

                if (getApps.Any())
                {
                    var company = _context.Companies.Where(x => x.CompanyId == getApps.FirstOrDefault().CompanyId && x.ActiveStatus == true && x.DeleteStatus == false);

                    foreach (var item in SubmittedDocuments)
                    {
                        var check_doc = _context.SubmittedDocuments.Where(x => x.RequestId == appID && x.AppDocId == item.LocalDocID);

                        if (check_doc.Count() <= 0)
                        {
                            SubmittedDocuments submitDocs = new SubmittedDocuments()
                            {
                                RequestId = appID,
                                AppDocId = item.LocalDocID,
                                CompElpsDocId = item.CompElpsDocID,
                                CreatedAt = DateTime.Now,
                                DeletedStatus = false,
                                DocSource = item.DocSource
                            };
                            _context.SubmittedDocuments.Add(submitDocs);
                        }
                        else
                        {
                            check_doc.FirstOrDefault().RequestId = appID;
                            check_doc.FirstOrDefault().AppDocId = item.LocalDocID;
                            check_doc.FirstOrDefault().CompElpsDocId = item.CompElpsDocID;
                            check_doc.FirstOrDefault().DocSource = item.DocSource;
                            check_doc.FirstOrDefault().UpdatedAt = DateTime.Now;
                        }
                    }

                    int done = _context.SaveChanges();

                    if (done > 0)
                    {
                        getApps.FirstOrDefault().Status = GeneralClass.DocumentsUploaded;
                        getApps.FirstOrDefault().UpdatedAt = DateTime.Now;

                        if (_context.SaveChanges() > 0)
                        {
                            var apps = _context.RequestProposal.Where(x => x.RequestId == appID && x.DeletedStatus == false);

                            var actionFrom = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());
                            var actionTo = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());

                            _helpersController.SaveHistory(apps.FirstOrDefault().RequestId, actionFrom, actionTo, "Upload", "Request proposal documents uploaded successfully");

                            _helpersController.UpdateElpsApplication(apps.ToList());

                            _helpersController.LogMessages("Request proposal documents uploaded successfully with reference : " + apps.FirstOrDefault().RequestRefNo, _helpersController.getSessionEmail());

                            result = "All Done";
                        }
                        else
                        {
                            result = "Something went wrong trying to change your application status. Please try again later.";
                        }
                    }
                    else
                    {
                        result = "Something went wrong trying to save the uploaded document for this request proposal.";
                    }
                }
                else
                {
                    result = "Something went wrong trying to save or update some of your documents, please try again later";
                }
            }

            _helpersController.LogMessages("Result for submitting company's document : " + result, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionEmail)));

            return Json(result);
        }






        public IActionResult ApplicationPayment(string id) // application id
        {
            try
            {
                var myid = generalClass.DecryptIDs(id);

                if (myid == 0)
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, request ID is incorrect. Please try agin later") });
                }

                ViewData["RequestID"] = id;

                var requestrefnbr = (from r in _context.RequestProposal where r.RequestId == myid select r).FirstOrDefault();

                var company = _context.Companies.Where(x => x.CompanyId == requestrefnbr.CompanyId);

                var wellcount = (from p in _context.Application
                                      where p.RequestId == myid
                                      group new { p.WellName } by new { p.WellName } into g
                                      orderby (g.Key.WellName)
                                      select new { g.Key.WellName }).ToList().Count();

                var amount = wellcount * GeneralClass.ProposalAmount;
                // saving transactions
                var checktranexist = (from t in _context.Transactions where t.RequestId == myid select t).FirstOrDefault();

                string description = "SubSurface Pressure Survey Fee : N250,000 per well; No of Well : " + wellcount + "; Total Amount : " + string.Format("{0:N}", amount).ToString();

                if (checktranexist == null)
                {
                    Transactions transactions = new Transactions()
                    {
                        RequestId = myid,
                        TransactionType = "Await",
                        TransactionStatus = GeneralClass.PaymentPending,
                        TransactionDate = DateTime.Now,
                        AmtPaid = amount,
                        TotalAmt = amount,
                        ServiceCharge = 0,
                        TransRef = generalClass.Generate_Receipt_Number(),
                        Description = description
                    };

                    _context.Transactions.Add(transactions);
                    _context.SaveChanges();
                }

                var paramData = _restService.parameterData("compemail", _helpersController.getSessionEmail());
                var response = _restService.Response("/api/company/{compemail}/{email}/{apiHash}", paramData); // GET

                if (response.ErrorException != null || string.IsNullOrWhiteSpace(response.Content))
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, this can be a netwrok related or an error. Please try agin later") });
                }
                else
                {
                    var res = JsonConvert.DeserializeObject<CompanyDetail>(response.Content);

                    //helpers.LogMessage("Generating payment for application : " + paymentDetails.FirstOrDefault().RefNo, _helpersController.getSessionEmail());

                    var values = new JObject();
                    values.Add("serviceTypeId", _configuration.GetSection("RemitaSplit").GetSection("ServicTypeID").Value.ToString());
                    values.Add("categoryName", _configuration.GetSection("RemitaSplit").GetSection("CategoryName").Value.ToString());
                    values.Add("totalAmount", amount.ToString());//amount.ToString()
                    values.Add("payerName", res.contact_FirstName + " " + res.contact_LastName);
                    values.Add("payerEmail", res.user_Id);
                    values.Add("serviceCharge", "0"); //application.ServiceCharge.ToString("#"));
                    values.Add("amountDue", amount.ToString());//
                    values.Add("payerPhone", res.contact_Phone);
                    values.Add("orderId", requestrefnbr.RequestRefNo);
                    values.Add("returnSuccessUrl", Url.Action("PaymentSuccess", "Company", new { id = requestrefnbr.RequestRefNo }, Request.Scheme));
                    values.Add("returnFailureUrl", Url.Action("PaymentFail", "Company", new { id = requestrefnbr.RequestRefNo }, Request.Scheme));
                    //values.Add("returnBankPaymentUrl", Url.Action("PaymentBank", "Applications"));

                    JArray lineItems = new JArray();
                    JObject lineItem1 = new JObject();
                    lineItem1.Add("lineItemsId", "1");
                    lineItem1.Add("beneficiaryName", _configuration.GetSection("RemitaSplit").GetSection("BeneficiaryName").Value.ToString());
                    lineItem1.Add("beneficiaryAccount", _configuration.GetSection("RemitaSplit").GetSection("BeneficiaryAccount").Value.ToString());
                    lineItem1.Add("bankCode", _configuration.GetSection("RemitaSplit").GetSection("BankCode").Value.ToString());
                    lineItem1.Add("beneficiaryAmount", amount.ToString());//amount.ToString()
                    lineItem1.Add("deductFeeFrom", "1");

                    lineItems.Add(lineItem1);
                    values.Add("lineItems", lineItems);

                    JArray appItems = new JArray();
                    JObject appItem1 = new JObject();

                    appItem1.Add("name", _configuration.GetSection("RemitaSplit").GetSection("CategoryName").Value.ToString());
                    appItem1.Add("description", _configuration.GetSection("RemitaSplit").GetSection("Description").Value.ToString() + " " + description);
                    appItem1.Add("group", _configuration.GetSection("RemitaSplit").GetSection("Group").Value.ToString());
                    appItems.Add(appItem1);
                    values.Add("applicationItems", appItems);

                    JArray customFields = new JArray();
                    JObject customFields1 = new JObject
                                {
                                    { "name", "STATE" },
                                    { "value", company.FirstOrDefault().StateName + " State" },
                                    { "type", "ALL" }
                                };

                    JObject customFields2 = new JObject
                                {
                                    { "name", "COMPANY BRANCH" },
                                    { "value", company.FirstOrDefault().CompanyName },
                                    { "type", "ALL" }
                                };

                    JObject customFields3 = new JObject
                                {
                                    { "name", "FACILITY ADDRESS" },
                                    { "value", company.FirstOrDefault().Address },
                                    { "type", "ALL" }
                                };

                    JObject customFields4 = new JObject
                                {
                                    { "name", "Field/Zonal Office" },
                                    { "value", "HEAD OFFICE" },
                                    { "type", "ALL" }
                                };

                    customFields.Add(customFields1);
                    customFields.Add(customFields2);
                    customFields.Add(customFields3);
                    customFields.Add(customFields4);

                    values.Add("customFields", customFields);

                    _helpersController.LogMessages("Done Generating payment for application : " + id + " Posting to remita", _helpersController.getSessionEmail());

                    var paramDatas = _restService.parameterData("CompId", res.id.ToString());
                    var resp = _restService.Response("/api/Payments/{CompId}/{email}/{apiHash}", paramDatas, "POST", values);

                    var resz = JsonConvert.DeserializeObject<JObject>(resp.Content);

                    if (resz != null)
                    {
                        var trans = _context.Transactions.Where(x => x.RequestId == myid);

                        if (trans.Any())
                        {
                            // helpers.LogMessage("Payment RRR for application : " + paymentDetails.FirstOrDefault().RefNo + " generated successfully. RRR => " + resz.GetValue("rrr").ToString(), _helpersController.getSessionEmail());

                            trans.FirstOrDefault().Rrr = resz.GetValue("rrr").ToString();
                            trans.FirstOrDefault().ElpsTransId = Convert.ToInt32(resz.GetValue("transactionId").ToString());
                            trans.FirstOrDefault().UpdatedAt = DateTime.Now;
                            _context.SaveChanges();

                            var paramDatas2 = _restService.parameterData("CompId", res.id.ToString());

                            _helpersController.LogMessages("Checking if generated RRR has already been paid. RRR => " + resz.GetValue("rrr").ToString(), _helpersController.getSessionEmail());

                            var rrrCheck = _restService.Response("/api/Payments/BankPaymentInfo/{CompId}/{email}/{apiHash}/" + resz.GetValue("rrr").ToString(), paramDatas, "GET");

                            var rrrResp = JsonConvert.DeserializeObject<JObject>(rrrCheck.Content);

                            if (rrrResp != null && (rrrResp.GetValue("statusMessage").ToString() == "Approved" || rrrResp.GetValue("status")?.ToString() == "01" || rrrResp.GetValue("status")?.ToString() == "00"))
                            {
                                var transs = _context.Transactions.Where(x => x.RequestId == myid);
                                transs.FirstOrDefault().TransactionStatus = GeneralClass.PaymentCompleted;
                                transs.FirstOrDefault().TransactionType = "Online";
                                transs.FirstOrDefault().TransactionDate = Convert.ToDateTime(rrrResp.GetValue("paymentDate").ToString());
                                _context.SaveChanges();

                                ViewData["PaymentResponse"] = "true";
                                _helpersController.LogMessages("Paid RRR => " + resz.GetValue("rrr").ToString(), _helpersController.getSessionEmail());

                            }
                            else if (rrrResp != null && (rrrResp.GetValue("statusMessage").ToString() == "Transaction Pending"))
                            {
                                ViewData["PaymentResponse"] = "false";
                                _helpersController.LogMessages("Transaction Pending RRR => " + resz.GetValue("rrr").ToString(), _helpersController.getSessionEmail());
                            }
                            else
                            {
                                ViewData["PaymentResponse"] = "false";
                                _helpersController.LogMessages("Not Paid RRR => " + resz.GetValue("rrr").ToString(), _helpersController.getSessionEmail());

                            }

                            var pay = (from r in _context.RequestProposal
                                       join com in _context.Companies on r.CompanyId equals com.CompanyId
                                       join tran in _context.Transactions on r.RequestId equals tran.RequestId
                                       where (r.RequestId == myid)
                                       select new PaymentDetailsSubmit()
                                       {
                                           RefNo = r.RequestRefNo,
                                           Status = tran.TransactionStatus,
                                           AppType = "BHP",
                                           ShortName = "SubSurface Pressure SurveyStatus",
                                           CompanyName = com.CompanyName,
                                           Amount = amount,
                                           TotalAmount = amount,
                                           ServiceCharge = 0,
                                           RequestID = r.RequestId,
                                           rrr = tran.Rrr
                                       });

                            ViewData["PaymentDescriptioin"] = description;
                            List<PaymentDetailsSubmit> paymentDetails = new List<PaymentDetailsSubmit>();

                            paymentDetails.Add(new PaymentDetailsSubmit
                            {
                                RequestID = pay.FirstOrDefault().RequestID,
                                RefNo = pay.FirstOrDefault().RefNo,
                                Status = pay.FirstOrDefault().Status,
                                AppType = pay.FirstOrDefault().AppType,
                                ShortName = pay.FirstOrDefault().ShortName,
                                CompanyName = pay.FirstOrDefault().CompanyName,
                                Amount = pay.FirstOrDefault().Amount,
                                TotalAmount = pay.FirstOrDefault().TotalAmount,
                                ServiceCharge = pay.FirstOrDefault().ServiceCharge,
                                Description = description,
                                rrr = pay.FirstOrDefault().rrr
                            });

                            _helpersController.LogMessages("Displaying company's payment details for " + paymentDetails.FirstOrDefault().RefNo + "; Payment Status : "+ paymentDetails.FirstOrDefault().Status, _helpersController.getSessionEmail());

                            return View(paymentDetails);
                        }
                        else
                        {
                            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Oops...! Something went wrong, Application not found or have been deleted.") });
                        }
                    }
                    else
                    {
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Oops...! Something went wrong, Remita RRR not generated, please try again later.") });
                    }
                }

            }
            catch (Exception ex)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Oops...! Something went wrong, Remita RRR not generated, please try again later. " + ex.Message) });
            }


            //return View();
        }



        public IActionResult PaymentSuccess(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found. Kindly contact support.") });
            }

            var apps = _context.RequestProposal.Where(x => x.RequestRefNo == id);

            if (apps.Any())
            {
                apps.FirstOrDefault().Status = GeneralClass.PaymentCompleted;
                apps.FirstOrDefault().UpdatedAt = DateTime.Now;
                _context.SaveChanges();
                var req = (from t in _context.RequestProposal where t.RequestRefNo == id select t.RequestId).FirstOrDefault();
                var trans = _context.Transactions.Where(x => x.RequestId == req);

                trans.FirstOrDefault().TransactionStatus = GeneralClass.PaymentCompleted;
                trans.FirstOrDefault().TransactionType = "Online";
                trans.FirstOrDefault().TransactionDate = DateTime.Now;
                _context.SaveChanges();
                var myreqid = generalClass.Encrypt(req.ToString());

                var actionFrom = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());
                var actionTo = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());

                _helpersController.SaveHistory(apps.FirstOrDefault().RequestId, actionFrom, actionTo, "Payment", "Result from Payment application for " + id + " Payment Status : " + GeneralClass.PaymentCompleted);


                _helpersController.LogMessages("Result from Payment application for " + id + " Payment Status : " + GeneralClass.PaymentCompleted, _helpersController.getSessionEmail());


                return RedirectToAction("ApplicationPayment", "Company", new { id = myreqid });
            }
            else
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found or have been deleted. Kindly contact support.") });
            }
        }





        public IActionResult PaymentFail(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found. Kindly contact support.") });
            }
            var myid = Convert.ToInt32(id);
            var apps = _context.RequestProposal.Where(x => x.RequestId == myid);

            if (apps.Any())
            {
                //helpers.LogMessage("Application payment failed. RRR => " + apps.FirstOrDefault().Rrr, _helpersController.getSessionEmail());

                return View(apps.ToList());
            }
            else
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found or have been deleted. Kindly contact support.") });
            }
        }




        public IActionResult CreateApplicationForm(string id)
        {
            var myid = generalClass.DecryptIDs(id);

            if (myid == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application reference not found or not in correct format. Kindly contact support.") });
            }
            else
            {
                var ack = _context.RequestProposal.Where(x => x.RequestId == myid && x.HasAcknowledge == false && x.DeletedStatus == false);

                if (ack.Any())
                {
                    ack.FirstOrDefault().HasAcknowledge = true;
                    ack.FirstOrDefault().AcknowledgeAt = DateTime.Now;
                    _helpersController.SaveMessage(myid, ack.FirstOrDefault().CompanyId, "Acknowledgment of " + ack.FirstOrDefault().Comment, "Thanks for your acknowledgement.");
                    _context.SaveChanges();
                }


                ViewData["ReqId"] = myid;
                var longinemail = _helpersController.getSessionEmail();
                var update = (from r in _context.RequestProposal
                              join d in _context.RequestDuration on r.DurationId equals d.DurationId
                              join c in _context.Companies on r.CompanyId equals c.CompanyId
                              where r.RequestId == myid && c.CompanyEmail == longinemail && r.Status == GeneralClass.Rejected && r.IsProposalApproved == false
                              select new { r, d }).ToList().LastOrDefault();

                if (update != null)
                {
                    ViewData["Rejected"] = update.r.Status;
                    ViewData["Year"] = update.d.ProposalYear;
                }

                _helpersController.LogMessages("Opening Proposal Form", _helpersController.getSessionEmail());


                return View();
            }
        }



        public IActionResult CreateResultForm(string id)
        {
            var myid = generalClass.DecryptIDs(id);

            if (myid == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application reference not found or not in correct format. Kindly contact support.") });
            }
            else
            {
                ViewData["ReqId"] = myid;
                var longinemail = _helpersController.getSessionEmail();

                var update = (from r in _context.RequestProposal
                              join d in _context.RequestDuration on r.DurationId equals d.DurationId
                              join c in _context.Companies on r.CompanyId equals c.CompanyId
                              where r.RequestId == myid && c.CompanyEmail == longinemail && r.Status == GeneralClass.Rejected && r.IsProposalApproved == true
                              select new { r, d }).ToList().LastOrDefault();


                var update2 = (from r in _context.RequestProposal
                              join d in _context.RequestDuration on r.DurationId equals d.DurationId
                              join c in _context.Companies on r.CompanyId equals c.CompanyId
                              where r.RequestId == myid && c.CompanyEmail == longinemail && r.IsProposalApproved == true
                              select new { r, d }).ToList().LastOrDefault();

                ViewData["Year"] = "";

                if (update2 != null)
                {
                    ViewData["Year"] = update2.d.ProposalYear;
                }

                if (update != null)
                {
                    ViewData["Rejected"] = update.r.Status;
                    ViewData["Year"] = update.d.ProposalYear;
                }

            }

            _helpersController.LogMessages("Opening result creation form", _helpersController.getSessionEmail());


            return View();
        }



        public async Task<JsonResult> SaveResultForm(string ReqId, IFormFile Files)
        {
            string response = "";
            string message = "";
            string req_id = "";
            var reqid = generalClass.Encrypt(ReqId);
            try
            {
                var myid = Convert.ToInt32(ReqId);
                var longinemail = _helpersController.getSessionEmail();

                CancellationToken cancellationToken;


                if (Files == null || Files.Length <= 0)
                {
                    message = "file is empty please select file";
                    response = "No file";
                }

                else if (!Path.GetExtension(Files.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    message = "Not Support file extension";
                    response = "Wrong extension";
                }
                else
                {
                    var loginemail = _helpersController.getSessionEmail();
                    var requestid = Convert.ToInt32(ReqId);
                    var checkupdate = (from a in _context.SurveyReport
                                       join r in _context.RequestProposal on a.RequestId equals r.RequestId
                                       join c in _context.Companies on r.CompanyId equals c.CompanyId
                                       where a.RequestId == requestid && c.CompanyEmail == loginemail
                                       select a).ToList();

                    if (checkupdate.Count > 0)
                    {
                        foreach (var item in checkupdate)
                        {
                            _context.SurveyReport.Remove(item);
                            _context.SaveChanges();
                        }
                    }

                    using (var stream = new MemoryStream())
                    {
                        List<SurveyReport> survey = new  List<SurveyReport>();

                        await Files.CopyToAsync(stream, cancellationToken);

                        using (var package = new ExcelPackage(stream))
                        {

                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            var rowCount = workSheet.Dimension.End.Row;

                            if (workSheet.Cells[2, 1].Value.ToString() == "Field" && workSheet.Cells[2, 22].Value.ToString() == "Remarks")
                            {
                                for (int row = 3; row <= rowCount; row++)
                                {
                                    var cell1 = workSheet.Cells[row, 1].Value;
                                    var cell2 = workSheet.Cells[row, 2].Value;
                                    var cell3 = workSheet.Cells[row, 3].Value;
                                    var cell4 = workSheet.Cells[row, 4].Value;
                                    var cell5 = workSheet.Cells[row, 5].Value;
                                    var cell6 = workSheet.Cells[row, 6].Value;
                                    var cell7 = workSheet.Cells[row, 7].Value;
                                    var cell8 = workSheet.Cells[row, 8].Value;
                                    var cell9 = workSheet.Cells[row, 9].Value;
                                    var cell10 = workSheet.Cells[row, 10].Value;
                                    var cell11 = workSheet.Cells[row, 11].Value;
                                    var cell12 = workSheet.Cells[row, 12].Value;
                                    var cell13 = workSheet.Cells[row, 13].Value;
                                    var cell14 = workSheet.Cells[row, 14].Value;
                                    var cell15 = workSheet.Cells[row, 15].Value;
                                    var cell16 = workSheet.Cells[row, 16].Value;
                                    var cell17 = workSheet.Cells[row, 17].Value;
                                    var cell18 = workSheet.Cells[row, 18].Value;
                                    var cell19 = workSheet.Cells[row, 19].Value;
                                    var cell20 = workSheet.Cells[row, 20].Value;
                                    var cell21 = workSheet.Cells[row, 21].Value;
                                    var cell22 = workSheet.Cells[row, 22].Value;

                                    if (cell1 == null || cell2 == null || cell3 == null || cell4 == null || cell5 == null || cell6 == null || cell7 == null || cell8 == null || cell9 == null || cell10 == null || cell11 == null || cell12 == null
                                        || cell13 == null || cell14 == null || cell15 == null || cell16 == null || cell17 == null || cell18 == null || cell19 == null || cell20 == null || cell21 == null || cell22 == null)
                                    {
                                        message = "One or more excel field(s) was not filled. Please ensure to fill all the fields before uploading the excel sheet.";
                                        response = "Empty field";
                                    }
                                    else
                                    {
                                        survey.Add(new SurveyReport
                                        {
                                            Field = workSheet.Cells[row, 1].Value.ToString(),
                                            Reservior = workSheet.Cells[row, 2].Value.ToString(),
                                            RemainingReserve = Convert.ToDecimal(workSheet.Cells[row, 3].Value),
                                            WellName = workSheet.Cells[row, 4].Value.ToString(),
                                            DateOfSurvey = Convert.ToDateTime(workSheet.Cells[row, 5].Value),
                                            Oil = Convert.ToDecimal(workSheet.Cells[row, 6].Value),
                                            Water = Convert.ToDecimal(workSheet.Cells[row, 7].Value),
                                            Gas = Convert.ToDecimal(workSheet.Cells[row, 8].Value),
                                            Bsw = Convert.ToDecimal(workSheet.Cells[row, 9].Value),
                                            Gor = Convert.ToDecimal(workSheet.Cells[row, 10].Value),
                                            Bean = Convert.ToDecimal(workSheet.Cells[row, 11].Value),
                                            DatumDepth = Convert.ToDecimal(workSheet.Cells[row, 12].Value),
                                            InitialPressure = Convert.ToDecimal(workSheet.Cells[row, 13].Value),
                                            Thp = Convert.ToDecimal(workSheet.Cells[row, 14].Value),
                                            Chp = Convert.ToDecimal(workSheet.Cells[row, 15].Value),
                                            FbhpMp = Convert.ToDecimal(workSheet.Cells[row, 16].Value),
                                            SbhpMp = Convert.ToDecimal(workSheet.Cells[row, 17].Value),
                                            RpDatum = Convert.ToDecimal(workSheet.Cells[row, 18].Value),
                                            ProductivityIndex = Convert.ToDecimal(workSheet.Cells[row, 19].Value),
                                            DateOfLastSurvey = Convert.ToDateTime(workSheet.Cells[row, 20].Value),
                                            TypeOfSurvey = workSheet.Cells[row, 21].Value.ToString(),
                                            Remark = (workSheet.Cells[row, 22].Value.ToString()),
                                            CreatedAt = DateTime.Now,
                                            RequestId = myid

                                        });
                                    }
                                }

                                _context.SurveyReport.AddRange(survey.ToList());

                                if (_context.SaveChanges() > 0)
                                {
                                    var checkrejected = (from a in _context.SurveyReport
                                                         join r in _context.RequestProposal on a.RequestId equals r.RequestId
                                                         join c in _context.Companies on r.CompanyId equals c.CompanyId
                                                         where a.RequestId == requestid && c.CompanyEmail == loginemail && r.Status == GeneralClass.Rejected && r.IsProposalApproved == true && r.IsReportApproved == false
                                                         select a).ToList();

                                    // for rejected application
                                    if (checkrejected.Count > 0)
                                    {
                                        var Req = _context.RequestProposal.Where(x => x.RequestId == myid && x.DeletedStatus == false);
                                        var desk = _context.MyDesk.Where(x => x.RequestId == myid && x.DeskId == Req.FirstOrDefault().DeskId);
                                        var company = _context.Companies.Where(x => x.CompanyId == Req.FirstOrDefault().CompanyId && x.DeleteStatus == false);

                                        int staffID = desk.FirstOrDefault().StaffId;

                                        Req.FirstOrDefault().Status = GeneralClass.Processing;
                                        Req.FirstOrDefault().UpdatedAt = DateTime.Now;

                                        desk.FirstOrDefault().HasWork = false;
                                        desk.FirstOrDefault().UpdatedAt = DateTime.Now;

                                        int done = _context.SaveChanges();

                                        if (done > 0)
                                        {
                                            var user = _context.Staff.Where(x => x.StaffId == staffID).FirstOrDefault();

                                            var actionFrom = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());
                                            var actionTo = _helpersController.getActionHistory(user.RoleId, user.StaffId);

                                            _helpersController.SaveHistory(Req.FirstOrDefault().RequestId, actionFrom, actionTo, "Moved", "Application re-submitted and landed on staff desk.");

                                            string subject = "Application Survey Report Resubmitted with Ref : " + Req.FirstOrDefault().RequestRefNo;
                                            string content = "You have resubmitted your application with Refrence Number " + Req.FirstOrDefault().RequestRefNo + " for processing on BHP Portal. Kindly find other details below.";
                                            var emailMsg = _helpersController.SaveMessage(Req.FirstOrDefault().RequestId, _helpersController.getSessionUserID(), subject, content);
                                            var sendEmail = _helpersController.SendEmailMessageAsync(company.FirstOrDefault().CompanyEmail, company.FirstOrDefault().CompanyName, subject, content, GeneralClass.COMPANY_NOTIFY, emailMsg);


                                            string subject2 = "Application Survey Report Resubmitted with Ref : " + Req.FirstOrDefault().RequestRefNo;
                                            string content2 = "An application survey report has been resubmitted with Refrence Number " + Req.FirstOrDefault().RequestRefNo + " to you for processing on BHP Portal. Kindly find other details below.";
                                             var sendEmail2 = _helpersController.SendEmailMessageAsync(company.FirstOrDefault().CompanyEmail, company.FirstOrDefault().CompanyName, subject, content, GeneralClass.STAFF_NOTIFY, emailMsg);

                                            response = "Resubmitted";
                                            message = "Your survey report was successfully resubmitted";

                                            //var getApps = _context.RequestProposal.Where(x => x.RequestId == Req.FirstOrDefault().RequestId);

                                            //string subj = "Application (" + getApps.FirstOrDefault().RequestRefNo + ") Resubmitted and Awaiting your response.";
                                            //string cont = "Application with reference number " + getApps.FirstOrDefault().RequestRefNo + " has been resubmitted for processing.";

                                            //_helpersController.SendStaffEmailMessage(staffID, subj, cont);

                                        }
                                        else
                                        {
                                            response = "Exception error";
                                            message = "Something went wrong trying to resubmit your application";
                                        }
                                    }
                                    else
                                    {
                                        int AppDropStaffID = _helpersController.ApplicationDropStaff(0, GeneralClass.BEGIN);
                                        List<ApplicationProccess> process = _helpersController.GetAppProcess(0, 0, GeneralClass.BEGIN);

                                        if (AppDropStaffID > 0 && process.Any())
                                        {
                                            var checkdesk = _context.MyDesk.Where(x => x.RequestId == myid && x.ProcessId == process.FirstOrDefault().ProccessId && x.Sort == process.FirstOrDefault().Sort && x.HasWork == false);

                                            if (checkdesk.Any())
                                            {
                                                response = "Already Exist";
                                                message = "Sorry, this application is already on a staff desk.";
                                            }
                                            else
                                            {
                                                MyDesk desk = new MyDesk()
                                                {
                                                    ProcessId = process.FirstOrDefault().ProccessId,
                                                    RequestId = myid,
                                                    StaffId = AppDropStaffID,
                                                    HasWork = false,
                                                    CreatedAt = DateTime.Now,
                                                    HasPushed = false,
                                                    Sort = process.FirstOrDefault().Sort,
                                                };

                                                _context.MyDesk.Add(desk);

                                                if (_context.SaveChanges() > 0)
                                                {
                                                    var user = _context.Staff.Where(x => x.StaffId == AppDropStaffID).FirstOrDefault();

                                                    var actionFrom = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());
                                                    var actionTo = _helpersController.getActionHistory(user.RoleId, user.StaffId);

                                                    _helpersController.SaveHistory(myid, actionFrom, actionTo, "Moved", "Application landed on staff desk");

                                                    var apps = _context.RequestProposal.Where(x => x.RequestId == myid && x.DeletedStatus == false);
                                                    
                                                    if (apps.Any())
                                                    {
                                                        apps.FirstOrDefault().UpdatedAt = DateTime.Now;
                                                        apps.FirstOrDefault().CurrentDeskId = AppDropStaffID;
                                                        apps.FirstOrDefault().Status = GeneralClass.Processing;
                                                        apps.FirstOrDefault().IsSubmitted = true;
                                                        apps.FirstOrDefault().IsReportSubmitted = true;

                                                        if (_context.SaveChanges() > 0)
                                                        {
                                                            var Req = _context.RequestProposal.Where(x => x.RequestId == myid && x.DeletedStatus == false);
                                                            var company = _context.Companies.Where(x => x.CompanyId == Req.FirstOrDefault().CompanyId && x.DeleteStatus == false);
                                                            string subject = "Application Survey Report submitted with Ref : " + Req.FirstOrDefault().RequestRefNo;
                                                            string content = "You have submitted your application survey report with Refrence Number " + Req.FirstOrDefault().RequestRefNo + " for processing on BHP Portal. Kindly find other details below.";
                                                            var emailMsg = _helpersController.SaveMessage(Req.FirstOrDefault().RequestId, _helpersController.getSessionUserID(), subject, content);
                                                            var sendEmail = _helpersController.SendEmailMessageAsync(company.FirstOrDefault().CompanyEmail, company.FirstOrDefault().CompanyName, subject, content, GeneralClass.COMPANY_NOTIFY, emailMsg);

                                                            string subject2 = "Survey Report Submitted with Ref : " + Req.FirstOrDefault().RequestRefNo;
                                                            string content2 = "An application survey report has been submitted with Refrence Number " + Req.FirstOrDefault().RequestRefNo + " to you for processing on BHP Portal. Kindly find other details below.";
                                                            var sendEmail2 = _helpersController.SendEmailMessageAsync(user.StaffEmail, user.LastName + " " + user.FirstName, subject, content, GeneralClass.STAFF_NOTIFY, emailMsg);

                                                            response = "Saved";
                                                            message = "Your application was submitted successfully";
                                                        }
                                                        else
                                                        {
                                                            response = "update failed";
                                                            message = "Something went wrong trying to update your desk";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        response = "deleted";
                                                        message = "This application has been deleted";
                                                    }
                                                }
                                                else
                                                {
                                                    message = "Something went wrong trying to submit your application.";
                                                }
                                            }
                                        }
                                        else
                                        {
                                            response = "Work Flow Error";
                                            message = "No user or process was found to send application to.";
                                        }
                                    }
                                }
                                else
                                {
                                    message = "Something went wrong trying to save excel table data. Please try again later";
                                    response = "Exception error";
                                }
                            }
                            else
                            {
                                message = "Excel columns are not equal";
                                response = "Exception error";
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { message = ex.Message; response = "Exception error"; }

            _helpersController.LogMessages("Result from submitting application process :::: Ressponse => " + response + "; Message => " + message, _helpersController.getSessionEmail());

            return Json(new { response, req_id, message });

        }





        public JsonResult SaveApplicationForm(string ReqId, IFormFile Files)
        {
            string response = "";
            string message = "";
            string req_id = "";

            var reqid = generalClass.Encrypt(ReqId);
            try
            {
                var myid = Convert.ToInt32(ReqId);
                var longinemail = _helpersController.getSessionEmail();

                CancellationToken cancellationToken;
                var usersList = new List<Models.DB.Application>();

                if (Files == null || Files.Length <= 0)
                {
                    message = "file is empty please select file";
                    response = "No file";
                }

                else if (!Path.GetExtension(Files.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    message = "Not Support file extension";
                    response = "Wrong extension";
                }
                else
                {
                    var loginemail = _helpersController.getSessionEmail();
                    var requestid = Convert.ToInt32(ReqId);
                    var checkupdate = (from a in _context.Application
                                       join r in _context.RequestProposal on a.RequestId equals r.RequestId
                                       join c in _context.Companies on r.CompanyId equals c.CompanyId
                                       where a.RequestId == requestid && c.CompanyEmail == loginemail
                                       select a).ToList();
                    if (checkupdate != null)
                    {
                        foreach (var item in checkupdate)
                        {
                            _context.Application.Remove(item);
                            _context.SaveChanges();
                        }
                    }

                    using (var stream = new MemoryStream())
                    {
                        Files.CopyToAsync(stream, cancellationToken);

                        using (var package = new ExcelPackage(stream))
                        {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            var rowCount = workSheet.Dimension.End.Row;

                            if (workSheet.Cells[2, 1].Value.ToString() == "Quarter" && workSheet.Cells[2, 14].Value.ToString().Trim() == "Previous Year\nof Measured Reservoir Pressure")
                            {
                                for (int row = 3; row <= rowCount; row++)
                                {
                                    var cell1 = workSheet.Cells[row, 1].Value;
                                    var cell2 = workSheet.Cells[row, 2].Value;
                                    var cell3 = workSheet.Cells[row, 3].Value;
                                    var cell4 = workSheet.Cells[row, 4].Value;
                                    var cell5 = workSheet.Cells[row, 5].Value;
                                    var cell6 = workSheet.Cells[row, 6].Value;
                                    var cell7 = workSheet.Cells[row, 7].Value;
                                    var cell8 = workSheet.Cells[row, 8].Value;
                                    var cell9 = workSheet.Cells[row, 9].Value;
                                    var cell10 = workSheet.Cells[row, 10].Value;
                                    var cell11 = workSheet.Cells[row, 11].Value;
                                    var cell12 = workSheet.Cells[row, 12].Value;
                                    var cell13 = workSheet.Cells[row, 13].Value;
                                    var cell14 = workSheet.Cells[row, 14].Value;
                                    if (cell1 == null || cell2 == null || cell3 == null || cell4 == null || cell5 == null || cell6 == null || cell7 == null || cell8 == null || cell9 == null || cell10 == null || cell11 == null || cell12 == null || cell13 == null || cell14 == null)
                                    {
                                        message = "One or more excel field(s) was not filled. Please ensure to fill all the fields before uploading the excel sheet.";
                                        response = "Empty field";
                                    }
                                    else
                                    {
                                        usersList.Add(new Models.DB.Application
                                        {
                                            Qrt = workSheet.Cells[row, 1].Value.ToString(),
                                            Fields = workSheet.Cells[row, 2].Value.ToString(),
                                            Reservoir = workSheet.Cells[row, 3].Value.ToString(),
                                            InitialRpressure = Convert.ToDecimal(workSheet.Cells[row, 4].Value),
                                            RbubblePointPressure = Convert.ToInt32(workSheet.Cells[row, 5].Value),
                                            WellName = workSheet.Cells[row, 6].Value.ToString(),
                                            LastSurveyDate = Convert.ToDateTime(workSheet.Cells[row, 7].Value),
                                            MeasuredRpressure = Convert.ToDecimal(workSheet.Cells[row, 8].Value),
                                            TimeShut = workSheet.Cells[row, 9].Value.ToString(),
                                            UsedInstrument = workSheet.Cells[row, 10].Value.ToString(),
                                            OperaionWellCost = Convert.ToDecimal(workSheet.Cells[row, 11].Value),
                                            SurveyType = workSheet.Cells[row, 12].Value.ToString(),
                                            ReservoirCreatedAt = Convert.ToDateTime(workSheet.Cells[row, 13].Value),
                                            PreviousYearMeasuredRp = Convert.ToDecimal(workSheet.Cells[row, 14].Value),
                                            CreatedAt = DateTime.Now,
                                            RequestId = myid

                                        });
                                    }
                                }

                                foreach (var item in usersList)
                                {
                                    _context.Application.Add(item);
                                    _context.SaveChanges();
                                }

                                var update = (from r in _context.RequestProposal
                                              join c in _context.Companies on r.CompanyId equals c.CompanyId
                                              where r.RequestId == myid && c.CompanyEmail == longinemail
                                              select r).ToList().LastOrDefault();


                                if (update != null)
                                {
                                    var Req = _context.RequestProposal.Where(x => x.RequestId == myid && x.DeletedStatus == false);
                                    var company = _context.Companies.Where(x => x.CompanyId == Req.FirstOrDefault().CompanyId && x.DeleteStatus == false);

                                    //for proposal rejection
                                    if (update.Status == GeneralClass.Rejected)
                                    {
                                        var desk = _context.MyDesk.Where(x => x.RequestId == myid && x.DeskId == Req.FirstOrDefault().DeskId);

                                        Req.FirstOrDefault().Status = GeneralClass.Processing;
                                        Req.FirstOrDefault().UpdatedAt = DateTime.Now;

                                        desk.FirstOrDefault().HasWork = false;
                                        desk.FirstOrDefault().UpdatedAt = DateTime.Now;

                                        int done = _context.SaveChanges();

                                        if (done > 0)
                                        {
                                            var user = _context.Staff.Where(x => x.StaffId == desk.FirstOrDefault().StaffId).FirstOrDefault();

                                            var actionFrom = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());
                                            var actionTo = _helpersController.getActionHistory(user.RoleId, user.StaffId);

                                            _helpersController.SaveHistory(Req.FirstOrDefault().RequestId, actionFrom, actionTo, "Moved", "Application was re-submitted to staff.");

                                            // Saving Messages
                                            string subject = "Survey Proposal Application Resubmitted with Ref : " + Req.FirstOrDefault().RequestRefNo;
                                            string content = "You have resubmitted your survey proposal application with Refrence Number " + Req.FirstOrDefault().RequestRefNo + " for processing on BHP Portal. Kindly find other details below.";
                                            var emailMsg = _helpersController.SaveMessage(Req.FirstOrDefault().RequestId, _helpersController.getSessionUserID(), subject, content);
                                            var sendEmail = _helpersController.SendEmailMessageAsync(company.FirstOrDefault().CompanyEmail, company.FirstOrDefault().CompanyName, subject, content, GeneralClass.COMPANY_NOTIFY, emailMsg);

                                            string subject2 = "Survey Proposal Resubmitted with Ref : " + Req.FirstOrDefault().RequestRefNo;
                                            string content2 = "An application survey proposal has been resubmitted with Refrence Number " + Req.FirstOrDefault().RequestRefNo + " to you for processing on BHP Portal. Kindly find other details below.";
                                            var sendEmail2 = _helpersController.SendEmailMessageAsync(user.StaffEmail, user.LastName + " " + user.FirstName, subject, content, GeneralClass.STAFF_NOTIFY, emailMsg);

                                            req_id = generalClass.Encrypt(Req.FirstOrDefault().RequestId.ToString());
                                            response = "Resubmitted";
                                            message = "Your records and proposal were successfully resubmitted";
                                        }
                                        else
                                        {
                                            response = "Exception error";
                                            message = "Something went wrong trying to resubmit your proposal application";
                                        }
                                    }
                                    else
                                    {
                                        // for new application
                                        update.Status = GeneralClass.ProposalSubmitted; // change to proposal submitted when we go live
                                        update.DateApplied = DateTime.Now;
                                        update.UpdatedAt = DateTime.Now;

                                        if (_context.SaveChanges() > 0)
                                        {
                                            // Saving Messages
                                            string subject = "Survey Proposal Application Initiated with Ref : " + Req.FirstOrDefault().RequestRefNo;
                                            string content = "You have initiated your survey proposal application with Refrence Number " + Req.FirstOrDefault().RequestRefNo + " for processing on BHP Portal. Kindly find other details below.";
                                            var emailMsg = _helpersController.SaveMessage(Req.FirstOrDefault().RequestId, _helpersController.getSessionUserID(), subject, content);
                                            var sendEmail = _helpersController.SendEmailMessageAsync(company.FirstOrDefault().CompanyEmail, company.FirstOrDefault().CompanyName, subject, content, GeneralClass.COMPANY_NOTIFY, emailMsg);

                                            //_helpersController.SendStaffEmailMessage(staffID, subj, cont);

                                            req_id = generalClass.Encrypt(Req.FirstOrDefault().RequestId.ToString());
                                            response = "Saved";
                                            message = "Your records and proposal were successfully saved";

                                        }
                                        else
                                        {
                                            response = "Exception error";
                                            message = "Something went wrong trying to save your proposal application";
                                        }
                                    }
                                }
                                else
                                {
                                    response = "Exception error";
                                    message = "Something went wrong, application proposal not found.";
                                }
                            }
                            else
                            {
                                message = "Excel columns are not equal";
                                response = "Exception error";
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { message = ex.Message; response = "Exception error"; }

            _helpersController.LogMessages("Result from submitting application process :::: Ressponse => " + response + "; Message => " + message, _helpersController.getSessionEmail());

            return Json(new { response, req_id, message });
        }




        public IActionResult Apply()
        {
            var userid = _helpersController.getSessionUserID();

            if (userid == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, request ID is incorrect. Please try agin later") });
            }
            else
            {
                var surveystatus = (from r in _context.RequestProposal
                                    join m in _context.MyDesk on r.RequestId equals m.RequestId
                                    join p in _context.ApplicationProccess on m.ProcessId equals p.ProccessId
                                    where p.Process == GeneralClass.END && r.IsProposalApproved == true && r.IsSubmitted == false
                                    select p.Process).ToList().LastOrDefault();
                
                ViewBag.SurveyStatus = surveystatus;

                var Reqlist = new List<Models.MyModel.RequestModel>();

                var Apps = (from a in _context.RequestProposal
                            join c in _context.Companies on a.CompanyId equals c.CompanyId
                            join r in _context.RequestDuration on a.DurationId equals r.DurationId
                            where a.CompanyId == userid && a.HasAcknowledge == true && a.EmailSent == true
                            select new
                            {
                                companyname = c.CompanyName + " (" + c.CompanyEmail + ")",
                                a.RequestRefNo,
                                a.CreatedAt,
                                a.RequestId,
                                r.ProposalYear,
                                a.IsSubmitted,
                            }).ToList().LastOrDefault();

                if (Apps != null)
                {
                    Reqlist.Add(new Models.MyModel.RequestModel()
                    {
                        RequestRefNo = Apps.RequestRefNo,
                        companyname = Apps.companyname,
                        CreatedAt = Apps.CreatedAt,
                        RequestId = Apps.RequestId,
                        ProposalYear = Apps.ProposalYear,
                        IsSubmitted = Apps.IsSubmitted
                    });

                }
                ViewBag.Requestlist = Reqlist;
            }

            _helpersController.LogMessages("Opening Apply page", _helpersController.getSessionEmail());

            return View();
        }



        public IActionResult AcknowledgeRequest(string id)
        {
            var requestid = generalClass.DecryptIDs(id);

            if (requestid == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, request ID not in correct format. Please try agin later") });
            }
            else
            {
                var ack = _context.RequestProposal.Where(x => x.RequestId == requestid && x.HasAcknowledge == false && x.DeletedStatus == false);

                if(ack.Any())
                {
                    var refno = ack.FirstOrDefault().RequestRefNo;

                    ack.FirstOrDefault().HasAcknowledge = true;
                    ack.FirstOrDefault().AcknowledgeAt = DateTime.Now;

                    if (_context.SaveChanges() > 0)
                    {
                        var actionFrom = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());
                        var actionTo = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());

                        _helpersController.SaveHistory(ack.FirstOrDefault().RequestId, actionFrom, actionTo, "Acknowledge", "Request for proposal with reference " + refno + " Acknowledge successfully");

                        _helpersController.LogMessages("Request for proposal with reference " + refno + " Acknowledge successfully", _helpersController.getSessionEmail());

                        return RedirectToAction("Apply", "Company");
                    }
                    else
                    {
                        _helpersController.LogMessages("Request for proposal with reference " + refno + " was not Acknowledge. Something went wrong, please try again later", _helpersController.getSessionEmail());

                        return RedirectToAction("Apply", "Company");
                    } 
                }
                else
                {
                   _helpersController.LogMessages("Request for proposal was not found and Acknowledge. Something went wrong, please try again later", _helpersController.getSessionEmail());

                    return RedirectToAction("Apply", "Company");
                }
            }
        }




        public IActionResult MyApplications()
        {
            var userid = _helpersController.getSessionUserID();
            if (userid == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, request ID is incorrect. Please try agin later") });
            }
            else
            {
                var Reqlist = new List<Models.MyModel.RequestModel>();

                var Apps = (from r in _context.RequestProposal
                            join c in _context.Companies on r.CompanyId equals c.CompanyId
                            join d in _context.RequestDuration on r.DurationId equals d.DurationId
                            where r.CompanyId == userid && r.DeletedStatus == false && r.HasAcknowledge == true && r.EmailSent == true

                            select new
                            {
                                companyname = c.CompanyName + " (" + c.CompanyEmail + ")",
                                r.RequestRefNo,
                                r.CreatedAt,
                                r.RequestId,
                                d.ProposalYear,
                                r.Status,
                                r.IsProposalApproved,
                                r.IsReportApproved,

                            }).ToList();

                if (Apps != null)
                {
                    foreach (var item in Apps)
                    {
                        Reqlist.Add(new Models.MyModel.RequestModel()
                        {
                            RequestRefNo = item.RequestRefNo,
                            companyname = item.companyname,
                            CreatedAt = item.CreatedAt,
                            RequestId = item.RequestId,
                            ProposalYear = item.ProposalYear,
                            Status = item.Status,
                            IsProposalApproved = item.IsProposalApproved,
                            IsReportApproved = item.IsReportApproved
                        });
                    }
                }

                _helpersController.LogMessages("Displaying all My Application", _helpersController.getSessionEmail());


                ViewBag.Proposallist = Reqlist;
            }
            return View();
        }



        public IActionResult ApplicationDetails(string id)
        {
            RequestModel mymodel = new RequestModel();
            var companydetails = (from c in _context.Companies
                                  select new { c.CompanyName, c.CompanyEmail, c.Address, c.StateName, c.IdentificationCode }).FirstOrDefault();

            mymodel.companyname = companydetails.CompanyName;
            mymodel.CompanyEmail = companydetails.CompanyEmail;
            mymodel.Address = companydetails.Address;
            mymodel.StateName = companydetails.StateName;
            mymodel.IdentificationCode = companydetails.IdentificationCode;
            var loginemail = _helpersController.getSessionEmail();

            var comid = (from c in _context.Companies where c.CompanyEmail == loginemail select c.CompanyId).FirstOrDefault();

            var details = (from r in _context.RequestProposal
                           join d in _context.RequestDuration on r.DurationId equals d.DurationId
                           where r.CompanyId == comid
                           select new { d.ProposalYear, r.CreatedAt, r.RequestRefNo }).FirstOrDefault();
            if (details != null)
            {
                ViewData["ProposalYr"] = details.ProposalYear;
                ViewData["Requestdate"] = details.CreatedAt;
                ViewData["RequestRefNo"] = details.RequestRefNo;
            }

            var applist = new List<Models.DB.Application>();

            var reqid = generalClass.DecryptIDs(id);
            if (reqid == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, request ID is incorrect. Please try agin later") });
            }
            var Apps = (from a in _context.Application
                        where a.RequestId == reqid
                        select new
                        {
                            a.RequestId,
                            a.Qrt,
                            a.Fields,
                            a.Reservoir,
                            a.InitialRpressure,
                            a.RbubblePointPressure,
                            a.WellName,
                            a.LastSurveyDate,
                            a.MeasuredRpressure,
                            a.TimeShut,
                            a.UsedInstrument,
                            a.OperaionWellCost,
                            a.ReservoirCreatedAt
                        }).ToList();

            foreach (var item in Apps)
            {
                applist.Add(new Models.DB.Application
                {
                    RequestId = item.RequestId,
                    Qrt = item.Qrt,
                    Fields = item.Fields,
                    Reservoir = item.Reservoir,
                    InitialRpressure = item.InitialRpressure,
                    RbubblePointPressure = item.RbubblePointPressure,
                    WellName = item.WellName,
                    LastSurveyDate = item.LastSurveyDate,
                    MeasuredRpressure = item.MeasuredRpressure,
                    TimeShut = item.TimeShut,
                    UsedInstrument = item.UsedInstrument,
                    OperaionWellCost = item.OperaionWellCost,
                    ReservoirCreatedAt = item.ReservoirCreatedAt

                });
            }

            var appDocs = from sd in _context.SubmittedDocuments
                          join ad in _context.ApplicationDocuments on sd.AppDocId equals ad.AppDocId
                          where sd.RequestId == reqid && sd.DeletedStatus == false && ad.DeletedStatus == false && sd.CompElpsDocId != null
                          select new AppDocuument
                          {
                              LocalDocID = sd.AppDocId,
                              DocName = ad.DocName,
                              EplsDocTypeID = ad.ElpsDocTypeId,
                              CompanyDocID = (int)sd.CompElpsDocId,
                              DocType = ad.DocType,
                              DocSource = sd.DocSource
                          };

            ViewData["ReqID"] = applist.FirstOrDefault()?.RequestId;

            ViewBag.MyApplicationList = applist;
            ViewBag.AppDocs = appDocs;

            _helpersController.LogMessages("Displaying Application details for " + details.RequestRefNo , _helpersController.getSessionEmail());


            return View(mymodel);
        }




        
        public IActionResult Dashboard()
        {
            var companyID = _helpersController.getSessionUserID();

            var today = DateTime.Now;

            _helpersController.LogMessages("Displaying company's dashboard...", _helpersController.getSessionEmail());

            var reportButton = (from r in _context.RequestProposal
                                join m in _context.MyDesk on r.RequestId equals m.RequestId
                                join p in _context.ApplicationProccess on m.ProcessId equals p.ProccessId
                                where r.IsProposalApproved == true && p.Process == GeneralClass.END && r.DeletedStatus == false && r.IsReportSubmitted == false
                                select r).FirstOrDefault();


            var proposalRejectionNotification = (from r in _context.RequestProposal
                                                 join d in _context.MyDesk on r.DeskId equals d.DeskId
                                                 where r.Status == GeneralClass.Rejected && r.CompanyId == companyID && r.IsProposalApproved == false
                                                 select new { r.Status, r.RequestId, d.Comment }).FirstOrDefault();

            var reportRejectionNotification = (from r in _context.RequestProposal
                                               join d in _context.MyDesk on r.DeskId equals d.DeskId
                                               where r.Status == GeneralClass.Rejected && r.CompanyId == companyID && r.IsProposalApproved == true
                                               select new { r.Status, r.RequestId, d.Comment }).FirstOrDefault();

            ViewBag.ReportButton = reportButton;

            if (reportButton != null)
            {
                ViewBag.ReqID = reportButton.RequestId;
            }

            if (proposalRejectionNotification != null)
            {
                ViewBag.ProposalRejectionNotificationStatus = proposalRejectionNotification.Status;
                ViewBag.ProposalRejectionNotificationComment = proposalRejectionNotification.Comment;
                ViewBag.ProposalRejectionNotificationRequestId = proposalRejectionNotification.RequestId;
            }
            if (reportRejectionNotification != null)
            {
                ViewBag.reportRejectionNotificationStatus = reportRejectionNotification.Status;
                ViewBag.reportRejectionNotificationComment = reportRejectionNotification.Comment;
                ViewBag.reportRejectionNotificationRequestId = reportRejectionNotification.RequestId;
            }


            var checkRequest = from r in _context.RequestProposal
                               join d in _context.RequestDuration on r.DurationId equals d.DurationId
                               where (r.CompanyId == companyID && r.HasAcknowledge == false && r.EmailSent == true && d.ProposalYear == GeneralClass._Year) && (today <= d.RequestEndDate)
                               select new Models.DB.RequestProposal
                               {
                                   RequestId = r.RequestId
                               };

            ViewBag.RequestLists = checkRequest.ToList();

            var messages = _context.Messages.Where(x => x.CompanyId == companyID).OrderByDescending(x => x.MessageId).Take(10);

            var all_apps = from a in _context.RequestProposal.AsEnumerable()
                           join c in _context.Companies.AsEnumerable() on a.CompanyId equals c.CompanyId
                           where c.CompanyId == companyID && a.HasAcknowledge == true && a.EmailSent == true && (a.DeletedStatus == false || a.DeletedStatus == null)
                           select new
                           {
                               a
                           };

            var unfinished = _context.RequestProposal.Where(x => x.CompanyId == companyID && x.HasAcknowledge == true && x.IsSubmitted == false && x.DeletedStatus == false).Count();

            var all_permits = from p in _context.Permits.AsEnumerable()
                              join a in _context.RequestProposal.AsEnumerable() on p.RequestId equals a.RequestId
                              join c in _context.Companies.AsEnumerable() on a.CompanyId equals c.CompanyId
                              where c.CompanyId == companyID && a.DeletedStatus == false
                              select new
                              {
                                  p
                              };

            int expiryCount = 0;

            var permits = from p in all_permits
                          select new Models.DB.Permits
                          {
                              PermitNo = p.p.PermitNo,
                              PermitId = p.p.PermitId
                          };


            foreach (var pr in all_permits.ToList())
            {
                var check = pr.p.ExpireDate.AddDays(-10);
                var now = DateTime.Now;

                if (check <= now && pr.p.ExpireDate > now)
                {
                    expiryCount++;
                }
                else
                {
                    // Do nothin as the permit is still valid
                }
            }

            var notify = _context.RequestProposal.Where(x => x.CompanyId == companyID && x.HasAcknowledge == true && (x.Status != GeneralClass.Approved && x.Status != GeneralClass.Processing) && x.DeletedStatus == false).Count();

            var schedule = from a in _context.RequestProposal.AsEnumerable()
                           join s in _context.Schdules.AsEnumerable() on a.RequestId equals s.RequestId
                           where a.CompanyId == companyID && a.DeletedStatus == false && s.SupervisorApprove == 1 && (s.CustomerAccept == null || s.CustomerAccept == 0) && s.DeletedStatus == false && s.SchduleDate > DateTime.Now
                           select a;


            ViewData["ProposalRequestNotify"] = checkRequest.Count();
            ViewData["AllProcessingApps"] = all_apps.Where(x => x.a.Status == GeneralClass.Processing).Count();
            ViewData["Notify"] = notify;
            ViewData["ScheduleNotify"] = schedule.Count();
            ViewData["Unfinished"] = unfinished;
            ViewData["AllCompanyApps"] = all_apps.Count();
            ViewData["AllCompanyPermit"] = all_permits.Count();
            ViewBag.Permits = permits.ToList();
            ViewData["AppExpiring"] = expiryCount;

            _helpersController.LogMessages("Displaying company's Dashboard", _helpersController.getSessionEmail());


            return View(messages.ToList());
        }



        /*
         * Getting company profile information
         */

        public JsonResult GetCompanyProfile(string CompanyId)
        {
            var paramData = _restService.parameterData("id", CompanyId);
            var result = generalClass.RestResult("company/{id}", "GET", paramData, null); // GET

            return Json(result.Value);
        }



        /*
         * Updating full a company's profile
         */

        public JsonResult UpdateCompanyProfile(CompanyDetail companyDetail)
        {
            string results = "";

            bool emailChange = false;

            var company = _context.Companies.Where(x => x.CompanyElpsId == companyDetail.id);

            if (company.Any())
            {
                if (company.FirstOrDefault().CompanyEmail == companyDetail.user_Id)
                {
                    emailChange = false;
                }
                else
                {
                    emailChange = true;
                }
            }

            CompanyChangeModel companyChange = new CompanyChangeModel()
            {
                Name = companyDetail.name,
                RC_Number = companyDetail.rC_Number,
                Business_Type = companyDetail.business_Type,
                emailChange = emailChange,
                CompanyId = companyDetail.id,
                NewEmail = companyDetail.user_Id
            };


            CompanyInformationModel companyDetails = new CompanyInformationModel();
            companyDetails.company = companyDetail;
            var result = generalClass.RestResult("company/Edit", "PUT", null, companyDetails, "Company Updated"); // PUT
            var result2 = generalClass.RestResult("Accounts/ChangeEmail", "POST", null, companyChange, "Company Updated"); // PUT

            if (result2.Value.ToString() == "Company Updated")
            {
                if (company.Any())
                {
                    company.FirstOrDefault().CompanyName = companyDetail.name;
                    company.FirstOrDefault().CompanyEmail = companyDetail.user_Id;
                    company.FirstOrDefault().UpdatedAt = DateTime.Now;

                    if (_context.SaveChanges() > 0)
                    {
                        results = "Company Updated";
                    }
                    else
                    {
                        results = "Company profile successfully updated on ELPS but not on BHP Portal. Please try again later.";
                    }
                }
                else
                {
                    results = "Company profile updated on ELPS but not on BHP Portal. Please try again later.";
                }
            }
            else
            {
                results = result.Value.ToString();
            }

            _helpersController.LogMessages("Updating company's profile... Result : " + result, _helpersController.getSessionEmail());

            return Json(results);
        }



        /*
         * Createing a company's address
         */
        public JsonResult CreateCompanyAddress(string CompanyId, List<Address> address)
        {
            var paramData = _restService.parameterData("CompId", CompanyId);
            var result = generalClass.RestResult("Address/{CompId}", "POST", paramData, address, "Created Address"); // POST
            _helpersController.LogMessages("Creating new company's address....", _helpersController.getSessionEmail());
            return Json(result.Value);
        }


        /*
         * Updating a company's address
         */
        public JsonResult UpdateCompanyAddress(List<Address> address)
        {
            var result = generalClass.RestResult("Address", "PUT", null, address, "Address Updated"); // PUT
            _helpersController.LogMessages("Updating company's address...", _helpersController.getSessionEmail());
            return Json(result.Value);
        }



        /*
         * Getting Directors Names
         */
        public IActionResult GetDirectorsNames(string CompanyId)
        {
            var paramData = _restService.parameterData("CompId", CompanyId);
            var result = generalClass.RestResult("Directors/{CompId}", "GET", paramData, null, null); // GET
            _helpersController.LogMessages("Getting company's directors name", _helpersController.getSessionEmail());
            return Json(result.Value);
        }


        /*
         * Creating company directors
         */
        public JsonResult CreateCompanyDirectors(string CompanyId, List<Directors> directors)
        {
            var paramData = _restService.parameterData("CompId", CompanyId);
            var result = generalClass.RestResult("Directors/{CompId}", "POST", paramData, directors, "Director Created"); // POST
            _helpersController.LogMessages("Creating company's directors...", _helpersController.getSessionEmail());
            return Json(result.Value);
        }


        /*
         * Retriving a list of a particular company directors
         */
        public JsonResult GetDirectors(string DirectorID)
        {
            var paramData = _restService.parameterData("Id", DirectorID);
            var result = generalClass.RestResult("Directors/ById/{Id}", "GET", paramData, null, null); // GET
            _helpersController.LogMessages("Getting single company's director details...", _helpersController.getSessionEmail());
            return Json(result.Value);
        }


        /*
         * Updating company's director information
         */

        public JsonResult UpdateCompanyDirectors(List<Directors> directors)
        {
            var result = generalClass.RestResult("Directors", "PUT", null, directors, "Director Updated"); // PUT
            _helpersController.LogMessages("Updating company's director...", _helpersController.getSessionEmail());
            return Json(result.Value);
        }



        /*
         *  view full company information for Admin.
         */

        public IActionResult FullCompanyProfile(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found or not in correct format. Kindly contact support.") });
            }

            var company = generalClass.Decrypt(id);

            if (company == "Error")
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found or not in correct format. Kindly contact support.") });
            }
            else
            {
                var com = _context.Companies.Where(x => x.CompanyEmail == company).FirstOrDefault();

                CompanyDetail companyModels = new CompanyDetail();

                var paramData = _restService.parameterData("id", com.CompanyElpsId.ToString());
                var response = _restService.Response("/api/company/{id}/{email}/{apiHash}", paramData, "GET", null); // GET

                if (response.IsSuccessful == false)
                {
                    var comp = _context.Companies.Where(x => x.CompanyEmail == company).FirstOrDefault();
                    companyModels.user_Id = comp.CompanyEmail;
                    companyModels.name = comp.CompanyName;
                    companyModels.id = comp.CompanyElpsId;
                    ViewData["CompanyName"] = null;
                }
                else
                {
                    companyModels = JsonConvert.DeserializeObject<CompanyDetail>(response.Content);
                    ViewData["CompanyName"] = companyModels.name;
                }

                _helpersController.LogMessages("Displaying full company's profile for admin...", _helpersController.getSessionEmail());

                return View(companyModels);
            }
        }




        /*
         * Viewing a list of all meaages for a company
         * 
         * id => encrypted company id
         */
        [Authorize(Roles = "COMPANY")]
        public IActionResult Messages()
        {
            var messages = _context.Messages.Where(x => x.CompanyId == _helpersController.getSessionUserID()).OrderByDescending(x => x.MessageId);
            _helpersController.LogMessages("Displaying company's messages as list...", _helpersController.getSessionEmail());
            return View(messages.ToList());
        }



        /*
         * Viewing a single message for a company 
         * 
         * id  => encrypted company id,
         * option => encrypted message id
         */
        [Authorize(Roles = "COMPANY")]
        public IActionResult Message(string id, string option)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(option))
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Message not found or not in correct format. Kindly contact support.") });
            }

            int comp_id = 0;
            int msg_id = 0;

            var c_id = generalClass.Decrypt(id);
            var m_id = generalClass.Decrypt(option);

            if (c_id == "Error" || m_id == "Error")
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Message not found or not in correct format. Kindly contact support.") });
            }
            else
            {
                comp_id = Convert.ToInt32(c_id);
                msg_id = Convert.ToInt32(m_id);

                var msg = _context.Messages.Where(x => x.MessageId == msg_id);

                msg.FirstOrDefault().Seen = true;
                _context.SaveChanges();

                var message = from m in _context.Messages
                              join a in _context.RequestProposal on m.RequestId equals a.RequestId
                              join c in _context.Companies on a.CompanyId equals c.CompanyId into comp
                              from cm in comp.DefaultIfEmpty()
                              join tr in _context.Transactions on a.RequestId equals tr.RequestId into trans
                              from t in trans.DefaultIfEmpty()
                              where m.CompanyId == comp_id && m.MessageId == msg_id
                              select new AppMessage
                              {
                                  Subject = m.Subject,
                                  Content = m.MesgContent,
                                  RefNo = a.RequestRefNo,
                                  GeneratedNo = a.GeneratedNumber,
                                  Status = a.Status,
                                  TotalAmount = t.TotalAmt,
                                  Seen = m.Seen,
                                  CompanyDetails = cm.CompanyName + " (" + cm.Address + ", " + cm.City + ", " + cm.StateName + ")"
                              };

                ViewData["MessageTitle"] = message.FirstOrDefault().Subject;

                _helpersController.LogMessages("Displaying single company's message...", _helpersController.getSessionEmail());

                return View(message.ToList());
            }
        }




        [Authorize(Roles = "COMPANY")]
        public IActionResult MyPermits()
        {
            int companyID = _helpersController.getSessionUserID();

            var myPermits = from p in _context.Permits
                            join a in _context.RequestProposal on p.RequestId equals a.RequestId
                            join c in _context.Companies on a.CompanyId equals c.CompanyId
                            where c.CompanyId == companyID
                            select new MyPermit
                            {
                                PermitID = p.PermitId,
                                PermitNo = p.PermitNo,
                                RefNo = a.RequestRefNo,
                                IssuedDate = p.IssuedDate,
                                ExpireDate = p.ExpireDate,
                                isPrinted = p.Printed,
                                CompanyName = c.CompanyName,
                                CompanyID = c.CompanyId,
                                CompanyEmail = c.CompanyEmail,
                                CompanyAddress = c.Address + " " + c.City + " " + c.StateName
                            };

            _helpersController.LogMessages("Displaying company's permits/license list...", _helpersController.getSessionEmail());

            return View(myPermits.ToList());
        }



        [Authorize(Roles = "COMPANY")]
        public IActionResult MySchedule()
        {
            int companyID = _helpersController.getSessionUserID();

            var sch = from sh in _context.Schdules
                      join a in _context.RequestProposal on sh.RequestId equals a.RequestId
                      join c in _context.Companies on a.CompanyId equals c.CompanyId
                      where c.CompanyId == companyID && sh.SupervisorApprove == 1 && sh.DeletedStatus == false
                      select new SchdulesList
                      {
                          Schedule = sh,
                          Request = a,
                      };

            _helpersController.LogMessages("Displaying company's schedule...", _helpersController.getSessionEmail());

            return View(sch.ToList());
        }



    }


}