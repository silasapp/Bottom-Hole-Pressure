using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BHP.Models.DB;
using BHP.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using BHP.HelpersClass;
using BHP.Models;
using Microsoft.AspNetCore.Authorization;


namespace BHP.Controllers.Applications
{
    [Authorize]
    public class ApplicationsController : Controller
    {
        private readonly BHP_DBContext _context;
        GeneralClass generalClass = new GeneralClass();
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        HelpersController _helpersController;



        public ApplicationsController(BHP_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }




        /*
         * Fetching all applications for viewing
         * 
         * Id => encrypted company id
         * option => the type of application report to display (ALL/Company)
         */
        public IActionResult Index(string id, string option)
        {
            var apps = from a in _context.RequestProposal.AsEnumerable()
                       join c in _context.Companies.AsEnumerable() on a.CompanyId equals c.CompanyId
                       join d in _context.RequestDuration.AsEnumerable() on a.DurationId equals d.DurationId
                       where a.DeletedStatus == false && a.HasAcknowledge == true
                       select new
                       {
                           RequestID = a.RequestId,
                           RefNo = a.RequestRefNo,
                           Year = d.ProposalYear,
                           CompanyName = c.CompanyName,
                           CompanyAddress = c.Address + ", " + c.City + ", " + c.StateName,
                           CompanyEmail = c.CompanyEmail,
                           CompanyID = a.CompanyId,
                           Status = a.Status,
                           DateApplied = a.DateApplied == null ? "" : a.DateApplied.ToString(),
                           DeletedStatus = a.DeletedStatus,
                           isProposalSubmitted = a.IsSubmitted == true ? "YES" : "NO",
                           isReportSubmitted = a.IsReportSubmitted == true ? "YES" : "NO",
                           isProposalApproved = a.IsProposalApproved == true ? "YES" : "NO",
                           isReportApproved = a.IsReportApproved == true ? "YES" : "NO",
                           hasAck = a.HasAcknowledge == true ? "Acknowledge" : "Pending"
                       };

            List<MyApps> myApps = new List<MyApps>();

            foreach (var a in apps)
            {
                var dks = _context.MyDesk.Where(x => x.RequestId == a.RequestID).OrderByDescending(x => x.DeskId);

                if (dks.Any())
                {
                    myApps.Add(new MyApps
                    {
                        RequestId = a.RequestID,
                        RefNo = a.RefNo,
                        CompanyName = a.CompanyName,
                        CompanyAddress = a.CompanyAddress,
                        CompanyEmail = a.CompanyEmail,
                        CompanyID = a.CompanyID,
                        Year = a.Year,
                        Status = a.Status,
                        Rate = a.Status == GeneralClass.Approved ? "100%" :
                                a.Status == GeneralClass.Rejected ? "XX%" :
                                  dks.FirstOrDefault().Sort == 1 ? "20%" :
                                  dks.FirstOrDefault().Sort == 2 ? "76%" :
                                  dks.FirstOrDefault().Sort == 3 ? "98%" :
                                  dks.FirstOrDefault().Sort == 4 ? "20%" :
                                  dks.FirstOrDefault().Sort == 5 ? "76%" :
                                  dks.FirstOrDefault().Sort == 6 ? "98%" : "XX%",
                        DateApplied = a.DateApplied,
                        isSubmitted = a.isProposalSubmitted,
                        isReportSubmitted = a.isReportSubmitted,
                        isProposalApproved = a.isProposalApproved,
                        isReportApproved = a.isReportApproved,
                        DeletedStatus = a.DeletedStatus,
                        HasAck = a.hasAck
                    });
                }
                else
                {
                    myApps.Add(new MyApps
                    {
                        RequestId = a.RequestID,
                        RefNo = a.RefNo,
                        CompanyName = a.CompanyName,
                        CompanyAddress = a.CompanyAddress,
                        CompanyEmail = a.CompanyEmail,
                        Year = a.Year,
                        CompanyID = a.CompanyID,
                        Status = a.Status,
                        Rate = a.Status == GeneralClass.Approved ? "100%" :
                                a.Status == GeneralClass.Rejected ? "XX%" : "5%",
                        DateApplied = a.DateApplied,
                        isSubmitted = a.isProposalSubmitted,
                        isReportSubmitted = a.isReportSubmitted,
                        isProposalApproved = a.isProposalApproved,
                        isReportApproved = a.isReportApproved,
                        DeletedStatus = a.DeletedStatus,
                        HasAck = a.hasAck
                    });
                }

            }

            ViewData["ClassifyApp"] = "All Applications";

            if (apps.Any())
            {
                if (!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(option))
                {
                    int ids = 0;
                    var type = generalClass.Decrypt(id);
                    var general_id = generalClass.Decrypt(option);

                    ids = Convert.ToInt32(general_id);

                    if (type == "_company")
                    {
                        myApps = myApps.Where(x => x.CompanyID == ids).ToList();

                        ViewData["ClassifyApp"] = "All applications for " + myApps.FirstOrDefault()?.CompanyName + " Company";

                    }
                }
                _helpersController.LogMessages("Displaying all record for " + ViewData["ClassifyApp"], _helpersController.getSessionEmail());
                return View(myApps);
            }
            else
            {
                return View(myApps);
            }
        }



        /*
        * Getting all companies
        */
        public IActionResult AllCompanies()
        {
            var com = _context.Companies;
            _helpersController.LogMessages("Displaying all companies...", _helpersController.getSessionEmail());
            return View(com.ToList());
        }




        public IActionResult MyDesk()
        {
            return View();
        }



        /*
        * Displaying all application on a particular staff desk
        */
        public JsonResult GetMyDeskApps()
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

            var staff = _helpersController.getSessionUserID();


            var get = from ad in _context.MyDesk.AsEnumerable()
                      join a in _context.RequestProposal.AsEnumerable() on ad.RequestId equals a.RequestId
                      join c in _context.Companies.AsEnumerable() on a.CompanyId equals c.CompanyId
                      join d in _context.RequestDuration.AsEnumerable() on a.DurationId equals d.DurationId
                      orderby ad.DeskId descending
                      where ((ad.StaffId == staff && ad.HasWork == false) && (a.DeletedStatus == false && a.IsSubmitted == true && a.HasAcknowledge == true) && (c.DeleteStatus == false) && (d.DeleteStatus == false))
                      select new
                      {
                          DeskID = ad.DeskId,
                          RequestID = ad.RequestId,
                          HasWork = ad.HasWork,
                          HasPushed = ad.HasPushed,
                          ProcessID = ad.ProcessId,
                          RefNo = a.RequestRefNo,
                          CompanyName = c.CompanyName,
                          CompanyAddress = c.Address + ", " + c.City + ", " + c.StateName,
                          Year = d.ProposalYear.ToString(),
                          DateApplied = a.DateApplied.ToString(),
                          CreatedAt = ad.CreatedAt.ToString(), // submited date
                          UpdatedAt = ad.UpdatedAt.ToString(),
                          Status = a.Status,
                          ProposalApproved = a.IsProposalApproved == true ? "YES" : "NO",
                          ReportApproved = a.IsReportApproved == true ? "YES" : "NO",
                          Activity = ad.Sort == 1 ? "(20%) Application Verification; Work (Create Schedule and Fill Form); Approve/Reject" :
                                 ad.Sort == 2 ? "(76%) Application Verification; Work (Create Schedule and Fill Form); Approve/Reject" :
                                 ad.Sort == 3 ? "(98%) Application Verification; Approve/Reject" :
                                 ad.Sort == 4 ? "(20%) Application Verification; Work (Create Schedule and Fill Form); Approve/Reject" :
                                 ad.Sort == 5 ? "(76%) Application Verification; Work (Create Schedule and Fill Form); Approve/Reject" :
                                 ad.Sort == 6 ? "(98%) Approve/Reject (Verify Again)"
                                 : "Application Report Verification; Work (Create Schedule and Fill Form); Approve/Reject"
                      };


            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    get = sortColumn == "refNo" ? get.OrderByDescending(c => c.RefNo) :
                               sortColumn == "companyName" ? get.OrderByDescending(c => c.CompanyName) :
                               sortColumn == "year" ? get.OrderByDescending(c => c.Year) :
                               sortColumn == "createdAt" ? get.OrderByDescending(c => c.CreatedAt) :
                               sortColumn == "updatedAt" ? get.OrderByDescending(c => c.CreatedAt) :
                               get.OrderByDescending(c => c.DeskID + " " + sortColumnDir);
                }
                else
                {
                    get = sortColumn == "refNo" ? get.OrderBy(c => c.RefNo) :
                                sortColumn == "companyName" ? get.OrderBy(c => c.CompanyName) :
                               sortColumn == "year" ? get.OrderBy(c => c.Year) :
                               sortColumn == "createdAt" ? get.OrderBy(c => c.CreatedAt) :
                               sortColumn == "updatedAt" ? get.OrderBy(c => c.CreatedAt) :
                               get.OrderBy(c => c.DeskID + " " + sortColumnDir);
                }
            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                get = get.Where(c => c.RefNo.Contains(txtSearch.ToUpper()) || c.CompanyName.Contains(txtSearch.ToUpper()) || c.Year.Contains(txtSearch.ToUpper()) || c.CreatedAt.Contains(txtSearch));
            }

            totalRecords = get.Count();
            var data = get.Skip(skip).Take(pageSize).ToList();

            _helpersController.LogMessages("Displaying list of applications on staff desk.", _helpersController.getSessionEmail());

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });
        }



        /*
         * Getting role base staff to move application to
         */
        public JsonResult GetPushStaff()
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

            List<StaffPushApps> staff = new List<StaffPushApps>();

            var staff_id = _helpersController.getSessionUserID();

            staff = _helpersController.GetPushStaff(staff_id); // staff session id


            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();

            _helpersController.LogMessages("Displaying list of staff to push applications to.", _helpersController.getSessionEmail());

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }



        /*
        * 
        * Distributing application to staff (MyDesk),
        * Updating MyDesk, and Application Table
        */
        public async Task<JsonResult> DistributAppToStaffAsync(int staffID, List<int> DeskID, string PushComment)
        {
            string result = "";

            // getting applications id from desk id

            if (DeskID.Any())
            {
                foreach (var desk in DeskID)
                {
                    var process = from dsk in _context.MyDesk
                                  join ap in _context.ApplicationProccess on dsk.ProcessId equals ap.ProccessId
                                  where dsk.DeskId == desk && ap.DeleteStatus == false
                                  select new
                                  {
                                      RequestId = dsk.RequestId,
                                      ProcessID = ap.ProccessId,
                                      Sort = ap.Sort,
                                  };

                    if (process.Any())
                    {
                        // getting the process and sorting for this application
                        foreach (var p in process.ToList())
                        {
                            var dsk = _context.MyDesk.Where(x => x.RequestId == p.RequestId && x.StaffId == staffID && x.HasWork == false);

                            if (dsk.Count() <= 0)
                            {
                                List<ApplicationProccess> proccesses = _helpersController.GetAppProcess(0, (p.Sort + 1));

                                // pushing the application to inspectors
                                MyDesk desks = new MyDesk()
                                {
                                    ProcessId = proccesses.FirstOrDefault().ProccessId,
                                    RequestId = p.RequestId,
                                    StaffId = staffID,
                                    HasWork = false,
                                    HasPushed = false,
                                    Sort = proccesses.FirstOrDefault().Sort,
                                    CreatedAt = DateTime.Now,
                                    Comment = PushComment
                                };

                                _context.MyDesk.Add(desks);

                                if (_context.SaveChanges() > 0)
                                {
                                    var user = _context.Staff.Where(x => x.StaffId == staffID).FirstOrDefault();

                                    var actionFrom = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());
                                    var actionTo = _helpersController.getActionHistory(user.RoleId, user.StaffId);

                                    _helpersController.SaveHistory(p.RequestId, actionFrom, actionTo, "Moved", "Application was assign to staff for processing. See comment => " + PushComment);

                                    var getApps = _context.RequestProposal.Where(x => x.RequestId == p.RequestId);

                                    string subj = "Application (" + getApps.FirstOrDefault().RequestRefNo + ") distributed to you and is on your desk";
                                    string cont = "An application with reference number " + getApps.FirstOrDefault().RequestRefNo + " has been distributed to you and is on your desk for processing.";

                                    var getStaff = _context.Staff.Where(x => x.StaffId == staffID);

                                    var msg = await _helpersController.SendEmailMessageAsync(getStaff.FirstOrDefault().StaffEmail, getStaff.FirstOrDefault().LastName + " " + getStaff.FirstOrDefault().FirstName, subj, cont, GeneralClass.STAFF_NOTIFY);

                                    result = "Pushed";

                                    // updating the working value for these applications
                                    var update_desk = _context.MyDesk.Where(x => x.DeskId == desk);
                                    update_desk.FirstOrDefault().HasWork = true;
                                    update_desk.FirstOrDefault().HasPushed = true;
                                    update_desk.FirstOrDefault().UpdatedAt = DateTime.Now;

                                    if (_context.SaveChanges() > 0)
                                    {
                                        var app = _context.RequestProposal.Where(x => x.RequestId == p.RequestId);
                                        app.FirstOrDefault().CurrentDeskId = staffID;
                                        app.FirstOrDefault().UpdatedAt = DateTime.Now;
                                        app.FirstOrDefault().Status = "Processing";

                                        if (_context.SaveChanges() > 0)
                                        {
                                            _helpersController.LogMessages("Application successfully distributed to staff. Application reference : " + app.FirstOrDefault().RequestRefNo, _helpersController.getSessionEmail());
                                            result = "Pushed";
                                        }
                                        else
                                        {
                                            result = "Somthing went wrong trying to update some application(s) status.";
                                        }
                                    }
                                    else
                                    {
                                        result = "Something went wrong trying to update some application(s) on your desk.";
                                    }
                                }
                                else
                                {
                                    result = "Something went wrong trying to push some application(s) to staff";
                                }
                            }
                            else
                            {
                                result = "Some of these application(s) has already been pushed to this staff.";
                            }
                        }
                    }
                    else
                    {
                        result = "Some selected application(s) were not found. Please try again later.";
                        break;
                    }
                }
            }
            else
            {
                result = "Application desk reference is empty. Please select Application(s) you want to distribute.";
            }

            _helpersController.LogMessages("Operation result for distributing apps : " + result, _helpersController.getSessionEmail());

            return Json(result);
        }




        /*
         * Viewing uploaded application survey proposal 
         * with different criteria as the result
         * 
         * id => encrypted request id
         */
        public IActionResult ViewSurvey(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Request reference is empty. Kindly contact support.") });
            }

            var requestid = generalClass.DecryptIDs(id);

            if (requestid == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Request reference not found or not in correct format. Kindly contact support.") });
            }
            else
            {
                List<Criteria> criterias = new List<Criteria>();
                List<SurveyCriteria> surveyCriterias = new List<SurveyCriteria>();
                List<CriteriaModel> criteriaModels = new List<CriteriaModel>();

                var request = from r in _context.RequestProposal
                              join d in _context.RequestDuration on r.DurationId equals d.DurationId
                              join c in _context.Companies on r.CompanyId equals c.CompanyId
                              where r.RequestId == requestid && r.DeletedStatus == false
                              select new
                              {
                                  RefNo = r.RequestRefNo,
                                  RequestId = r.RequestId,
                                  CompanyName = c.CompanyName,
                                  CompanyAddress = c.Address,
                                  CompanyEmail = c.CompanyEmail,
                                  ProposalYear = d.ProposalYear,
                                  DateApplied = r.DateApplied
                              };

                if (request.Any())
                {
                    ViewData["ViewSurveyDescription"] = "View " + request.FirstOrDefault().ProposalYear + " Survey for : " + request.FirstOrDefault().CompanyName;

                    var GetApps = _context.Application.Where(x => x.RequestId == requestid);

                    var GetSurveyReport = _context.SurveyReport.Where(x => x.RequestId == requestid);

                    var apps = (from a in _context.Application.AsEnumerable()
                                where a.RequestId == request.FirstOrDefault().RequestId
                                select new
                                {
                                    a
                                }).GroupBy(x => new
                                {
                                    Reservoir = x.a.Reservoir
                                }).Select(x => new
                                {
                                    Reservoir = x.Key.Reservoir,
                                    NoOfSurvey = x.Count(),
                                    ProductionYear = x.FirstOrDefault().a.ReservoirCreatedAt,
                                    InitialPressure = x.FirstOrDefault().a.InitialRpressure,
                                    CurrentPressure = x.FirstOrDefault().a.MeasuredRpressure, //I5
                                    PreviousPressure = x.FirstOrDefault().a.PreviousYearMeasuredRp,
                                    BubblePointPressure = x.FirstOrDefault().a.RbubblePointPressure, //F5
                                    DateApplied = request.FirstOrDefault().DateApplied
                                });

                    var surveyApps = (from s in _context.SurveyReport.AsEnumerable()
                                      where s.RequestId == request.FirstOrDefault().RequestId
                                      select new
                                      {
                                          s
                                      }).GroupBy(x => new
                                      {
                                          Reservoir = x.s.Reservior
                                      }).Select(x => new
                                      {
                                          Reservoir = x.Key.Reservoir,
                                          InitialPressure = x.FirstOrDefault().s.InitialPressure,
                                          SBHP_MP = x.FirstOrDefault().s.SbhpMp,
                                          Oil = x.FirstOrDefault().s.Oil,
                                          RemainingReserves = x.FirstOrDefault().s.RemainingReserve
                                      });

                    if (apps.Any())
                    {
                        foreach (var a in apps.ToList())
                        {
                            // Criteria 1
                            if (a.CurrentPressure > a.BubblePointPressure)
                            {
                                int ExpectedSurvey = 4;
                                var frequency = a.NoOfSurvey >= ExpectedSurvey ? "YES" : "NO";
                                var result = frequency == "YES" ? "Do BHP 4 times in a year" : "";

                                criterias.Add(new Criteria
                                {
                                    Reservoir = a.Reservoir,
                                    NoOfSurvey = a.NoOfSurvey,
                                    ExpectedNoOfSurvey = ExpectedSurvey,
                                    SurveyFrequecy = frequency,
                                    BubblePointPressure = a.BubblePointPressure,
                                    PreviousPressure = a.PreviousPressure,
                                    CurrentPressure = a.CurrentPressure,
                                    ProductionYear = a.ProductionYear,
                                    InitialPressure = a.InitialPressure,
                                    DateApplied = a.DateApplied,
                                    ResultDescription = result,
                                    ReservoirState = "Reservoir pressure above bubble point",
                                }); ;
                            }
                            else
                            {
                                // Criteria 4
                                var Years = (int)(a.DateApplied?.Year - a.ProductionYear.Year);

                                if (Years >= 10) // production years is greater and equal to 10
                                {
                                    var criteria4 = (((a.InitialPressure - a.CurrentPressure) / a.InitialPressure) * 100 / 1);
                                    int ExpectedSurvey = 1;
                                    var frequency = a.NoOfSurvey >= ExpectedSurvey ? "YES" : "NO";
                                    var result = frequency == "YES" ? "Do BHP once in a year" : "";

                                    if (criteria4 < 10) // less than 10%
                                    {
                                        criterias.Add(new Criteria
                                        {
                                            Reservoir = a.Reservoir,
                                            NoOfSurvey = a.NoOfSurvey,
                                            ExpectedNoOfSurvey = ExpectedSurvey,
                                            SurveyFrequecy = frequency,
                                            BubblePointPressure = a.BubblePointPressure,
                                            PreviousPressure = a.PreviousPressure,
                                            CurrentPressure = a.CurrentPressure,
                                            YearsOfProduction = Years,
                                            ProductionYear = a.ProductionYear,
                                            InitialPressure = a.InitialPressure,
                                            DateApplied = a.DateApplied,
                                            ResultDescription = result,
                                            ReservoirState = "Reservoir pressure below bubble point (and more than 10 years production)",
                                            PressureDecline = "˂ 10% over the initial pressure"
                                        });
                                    }
                                    else // greater or equal to 10%
                                    {
                                        criterias.Add(new Criteria
                                        {
                                            Reservoir = a.Reservoir,
                                            NoOfSurvey = a.NoOfSurvey,
                                            ExpectedNoOfSurvey = ExpectedSurvey,
                                            SurveyFrequecy = frequency,
                                            BubblePointPressure = a.BubblePointPressure,
                                            PreviousPressure = a.PreviousPressure,
                                            CurrentPressure = a.CurrentPressure,
                                            YearsOfProduction = Years,
                                            ProductionYear = a.ProductionYear,
                                            InitialPressure = a.InitialPressure,
                                            DateApplied = a.DateApplied,
                                            ResultDescription = "Do BHP once in a year with recommendation of pressure maintainance project"

                                        });
                                    }
                                }
                                else // production years is less than 10 years
                                {
                                    var criteria2and3 = (((a.PreviousPressure - a.CurrentPressure) / a.PreviousPressure) * 100 / 1);

                                    // Criteria 2
                                    if (criteria2and3 < 2)  // less than 2%
                                    {
                                        int ExpectedSurvey = 4;
                                        var frequency = a.NoOfSurvey >= ExpectedSurvey ? "YES" : "NO";
                                        var result = frequency == "YES" ? "Do BHP once a quarter." : "";

                                        criterias.Add(new Criteria
                                        {
                                            Reservoir = a.Reservoir,
                                            NoOfSurvey = a.NoOfSurvey,
                                            ExpectedNoOfSurvey = ExpectedSurvey,
                                            SurveyFrequecy = frequency,
                                            BubblePointPressure = a.BubblePointPressure,
                                            PreviousPressure = a.PreviousPressure,
                                            CurrentPressure = a.CurrentPressure,
                                            YearsOfProduction = Years,
                                            ProductionYear = a.ProductionYear,
                                            InitialPressure = a.InitialPressure,
                                            DateApplied = a.DateApplied,
                                            ResultDescription = result,
                                            ReservoirState = "Reservoir pressure below bubble point",
                                            PressureDecline = "˂ 2% over the previous year"
                                        });
                                    }
                                    else // greater or equal to 2% (Criteria 3)
                                    {
                                        int ExpectedSurvey = 2;
                                        var frequency = a.NoOfSurvey >= ExpectedSurvey ? "YES" : "NO";
                                        var result = frequency == "YES" ? "Do BHP twice a year." : "";

                                        criterias.Add(new Criteria
                                        {
                                            Reservoir = a.Reservoir,
                                            NoOfSurvey = a.NoOfSurvey,
                                            ExpectedNoOfSurvey = ExpectedSurvey,
                                            SurveyFrequecy = frequency,
                                            BubblePointPressure = a.BubblePointPressure,
                                            PreviousPressure = a.PreviousPressure,
                                            CurrentPressure = a.CurrentPressure,
                                            YearsOfProduction = Years,
                                            ProductionYear = a.ProductionYear,
                                            InitialPressure = a.InitialPressure,
                                            DateApplied = a.DateApplied,
                                            ResultDescription = result,
                                            ReservoirState = "Reservoir pressure below bubble point",
                                            PressureDecline = ">= 2% over the previous year"
                                        }); ;
                                    }
                                }

                            }
                        }

                        if (surveyApps.Any())
                        {
                            foreach (var s in surveyApps.ToList())
                            {
                                var recommendation = "";

                                var pressureDecline = (((s.InitialPressure - s.SBHP_MP) / s.InitialPressure) * (100 / 1));

                                var fractionRecovery = ((s.Oil / (s.Oil + s.RemainingReserves)) * (100 / 1));

                                if (pressureDecline >= 10 && fractionRecovery <= 75 && s.RemainingReserves > 5)
                                {
                                    recommendation = "Recommended for pressure maintainance.";
                                }

                                surveyCriterias.Add(new SurveyCriteria
                                {
                                    Oil = s.Oil,
                                    InitialPressure = s.InitialPressure,
                                    SBHP_MP = s.SBHP_MP,
                                    RemainingReservers = s.RemainingReserves,
                                    Reservoir = s.Reservoir,
                                    PressureDecline = pressureDecline,
                                    FractionalRecovery = fractionRecovery,
                                    AltimateRecovery = (s.Oil + s.RemainingReserves),
                                    Recommendation = recommendation
                                });
                            }
                        }

                        criteriaModels.Add(new CriteriaModel
                        {
                            criterias = criterias.ToList(),
                            applications = GetApps.ToList(),
                            surveyCriterias = surveyCriterias.ToList(),
                            surveyReports = GetSurveyReport.ToList()
                        });

                        return View(criteriaModels.ToList());
                    }
                    else
                    {
                        criteriaModels.Add(new CriteriaModel
                        {
                            criterias = criterias.ToList(),
                            applications = GetApps.ToList(),
                            surveyCriterias = surveyCriterias.ToList(),
                            surveyReports = GetSurveyReport.ToList()
                        });

                        return View(criteriaModels.ToList());
                    }

                }
                else
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong trying to get all uploaded Subsurface pressure survay, please try again later.") });
                }
            }

        }



        /*
        * Viewing application details with operation control
        * 
        * 
        * id => encrypted desk id
        * option => encrypted process id
        */
        public IActionResult ViewApplication(string id, string option)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(option))
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found or not in correct format. Kindly contact support.") });
            }

            var deskID = generalClass.DecryptIDs(id);
            var processID = generalClass.DecryptIDs(option);

            if (deskID == 0 || processID == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found or not in correct format. Kindly contact support.") });
            }
            else
            {
                List<RequestApplicationModel> requestApplicationModels = new List<RequestApplicationModel>();

                var staff = _helpersController.getSessionUserID();

                var Request = from d in _context.MyDesk.AsEnumerable()
                              join r in _context.RequestProposal.AsEnumerable() on d.RequestId equals r.RequestId
                              join du in _context.RequestDuration.AsEnumerable() on r.DurationId equals du.DurationId
                              join c in _context.Companies.AsEnumerable() on r.CompanyId equals c.CompanyId
                              join t in _context.Transactions.AsEnumerable() on r.RequestId equals t.RequestId into trans
                              from tr in trans.DefaultIfEmpty()
                              where ((d.StaffId == staff && d.HasWork == false && d.DeskId == deskID && d.ProcessId == processID) && (r.DeletedStatus == false) && (c.DeleteStatus == false))
                              select new RequestApplication
                              {
                                  DeskId = d.DeskId,
                                  RefNo = r.RequestRefNo,
                                  RequestId = r.RequestId,
                                  CompanyName = c.CompanyName,
                                  CompanyAddress = c.Address,
                                  CompanyEmail = c.CompanyEmail,
                                  CompanyElpsId = c.CompanyElpsId.ToString(),
                                  TotalAmount = tr?.TotalAmt,
                                  RRR = tr?.Rrr,
                                  TransType = tr?.TransactionType,
                                  AmountPaid = tr?.AmtPaid,
                                  ServiceCharge = tr?.ServiceCharge,
                                  TransDate = tr?.TransactionDate,
                                  TransDescription = tr?.Description,
                                  TransStatus = tr?.TransactionStatus,
                                  Year = du.ProposalYear,
                                  Status = r.Status,
                                  DateApplied = r.DateApplied,
                                  GeneratedNo = r.GeneratedNumber,
                                  ReportApproved = r.IsReportApproved == true ? "YES" : "NO",
                                  ProposalApproved = r.IsProposalApproved == true ? "YES" : "NO",
                              };

                if (Request.Any())
                {
                    var apps = _context.Application.Where(x => x.RequestId == Request.FirstOrDefault().RequestId && x.DeletedStatus == false);

                    var appHistory = from h in _context.AppDeskHistory
                                     orderby h.HistoryId descending
                                     where h.RequestId == Request.FirstOrDefault().RequestId
                                     select new History
                                     {
                                         Status = h.Status,
                                         Comment = h.Comment,
                                         HistoryDate = h.CreatedAt.ToString(),
                                         ActionFrom = h.ActionFrom,
                                         ActionTo = h.ActionTo,
                                     };

                    var nominationRequest = from r in _context.NominationRequest.AsEnumerable()
                                            join s in _context.Staff.AsEnumerable() on r.StaffId equals s.StaffId
                                            join ro in _context.UserRoles.AsEnumerable() on s.RoleId equals ro.RoleId
                                            where r.AppId == Request.FirstOrDefault().RequestId
                                            select new NominationRequests
                                            {
                                                Staff = s.LastName + " " + s.FirstName + "(" + ro.RoleName + ")",
                                                hasDone = r.HasDone == false ? "NO" : "YES",
                                                CreatedAt = r.CreatedAt,
                                                ReminderedAt = r.ReminderDate,
                                                UpdatedAt = r.UpdatedAt,
                                                Comment = r.Comment
                                            };


                    var appDocs = from sd in _context.SubmittedDocuments
                                  join ad in _context.ApplicationDocuments on sd.AppDocId equals ad.AppDocId
                                  where sd.RequestId == Request.FirstOrDefault().RequestId && sd.DeletedStatus == false && ad.DeletedStatus == false && sd.CompElpsDocId != null
                                  select new AppDocuument
                                  {
                                      LocalDocID = sd.AppDocId,
                                      DocName = ad.DocName,
                                      EplsDocTypeID = ad.ElpsDocTypeId,
                                      CompanyDocID = (int)sd.CompElpsDocId,
                                      DocType = ad.DocType,
                                      DocSource = sd.DocSource,
                                  };


                    var appReport = from r in _context.Reports
                                    join s in _context.Staff on r.StaffId equals s.StaffId
                                    orderby r.ReportId descending
                                    where r.RequestId == Request.FirstOrDefault().RequestId && r.DeletedStatus == false
                                    select new AppReport
                                    {
                                        ReportID = r.ReportId,
                                        Staff = s.StaffEmail,
                                        StaffID = r.StaffId,
                                        Comment = r.Comment,
                                        Title = r.Subject,
                                        CreatedAt = r.CreatedAt.ToString(),
                                        UpdatedAt = r.UpdatedAt == null ? "" : r.UpdatedAt.ToString()
                                    };


                    var appSchdule = from s in _context.Schdules
                                     join sby in _context.Staff on s.SchduleBy equals sby.StaffId
                                     orderby s.SchduleId descending
                                     where s.RequestId == Request.FirstOrDefault().RequestId && s.DeletedStatus == false
                                     select new AppSchdule
                                     {
                                         SchduleID = s.SchduleId,
                                         SchduleByID = s.SchduleBy,
                                         SchduleByEmail = sby.StaffEmail,
                                         SchduleType = s.SchduleType,
                                         SchduleLocation = s.SchduleLocation,
                                         SchduleDate = s.SchduleDate.ToString(),
                                         cResponse = s.CustomerAccept == 1 ? "Accepted" : s.CustomerAccept == 2 ? "Rejected" : "Awaiting Response",
                                         sResponse = s.SupervisorApprove == 1 ? "Accepted" : s.SupervisorApprove == 2 ? "Rejected" : "Awaiting Response",
                                         SchduleComment = s.Comment,
                                         CustomerComment = s.CustomerComment,
                                         SupervisorComment = s.SupervisorComment,
                                         CreatedAt = s.CreatedAt.ToString(),
                                         UpdatedAt = s.UpdatedAt == null ? "" : s.UpdatedAt.ToString()
                                     };

                    List<ApplicationProccess> appProcess = _context.ApplicationProccess.Where(x => x.ProccessId == processID && x.DeleteStatus == false).ToList();


                    var currentDesk = from a in _context.RequestProposal
                                      join s in _context.Staff on a.CurrentDeskId equals s.StaffId
                                      where a.RequestId == Request.FirstOrDefault().RequestId
                                      select new CurrentDesk
                                      {
                                          Staff = s.LastName + " " + s.FirstName + " (" + s.StaffEmail + ")"
                                      };

                    var staffList = from s in _context.Staff.AsEnumerable()
                                    join r in _context.UserRoles.AsEnumerable() on s.RoleId equals r.RoleId
                                    join f in _context.FieldOffices.AsEnumerable() on s.FieldOfficeId equals f.FieldOfficeId
                                    join zf in _context.ZoneFieldOffice.AsEnumerable() on f.FieldOfficeId equals zf.FieldOfficeId
                                    join z in _context.ZonalOffice.AsEnumerable() on zf.ZoneId equals z.ZoneId
                                    where ((s.ActiveStatus == true && s.DeleteStatus == false) && (r.RoleName == GeneralClass.ZOPSCON || r.RoleName == GeneralClass.OPSCON))
                                    select new StaffNomination
                                    {
                                        FullName = s.LastName + " " + s.FirstName,
                                        ZonalOffice = z.ZoneName,
                                        StaffId = s.StaffId,
                                        StaffEmail = s.StaffEmail,
                                        FieldOffice = f.OfficeName,
                                        RoleName = r.RoleName
                                    };

                    var nomination = from n in _context.NominatedStaff
                                     join s in _context.Staff on n.StaffId equals s.StaffId
                                     join r in _context.UserRoles on s.RoleId equals r.RoleId
                                     join a in _context.RequestProposal on n.RequestId equals a.RequestId
                                     where a.RequestId == Request.FirstOrDefault().RequestId
                                     select new Nomination
                                     {
                                         NominationID = n.NominateId,
                                         StaffName = s.LastName + " " + s.FirstName,
                                         UserRoles = r.RoleName,
                                         StaffEmail = s.StaffEmail.Trim(),
                                         hasSubmitted = n.HasSubmitted,
                                         CreatedBy = n.CreatedBy
                                     };

                    var surveyReport = _context.SurveyReport.Where(x => x.RequestId == Request.FirstOrDefault().RequestId);


                    var getDocuments = _context.ApplicationDocuments.Where(x => x.DocType == "Company" && x.DocName.Trim().Contains(GeneralClass.staff_application_report_company_doc) && x.DeletedStatus == false);

                    List<PresentDocuments> presentDocuments = new List<PresentDocuments>();
                    List<MissingDocument> missingDocuments = new List<MissingDocument>();
                    List<BothDocuments> bothDocuments = new List<BothDocuments>();

                    if (getDocuments.Any())
                    {
                        ViewData["CompanyElpsID"] = Request.FirstOrDefault().CompanyElpsId;

                        List<LpgLicense.Models.Document> CompanyDoc = generalClass.getCompanyDocuments(Request.FirstOrDefault().CompanyElpsId.ToString());

                        if (CompanyDoc != null)
                        {
                            foreach (var fDoc in CompanyDoc)
                            {
                                if (fDoc.document_type_id == getDocuments.FirstOrDefault().ElpsDocTypeId.ToString())
                                {
                                    presentDocuments.Add(new PresentDocuments
                                    {
                                        Present = true,
                                        FileName = fDoc.fileName,
                                        Source = fDoc.source,
                                        CompElpsDocID = fDoc.id,
                                        DocTypeID = Convert.ToInt32(fDoc.document_type_id),
                                        LocalDocID = getDocuments.FirstOrDefault().AppDocId,
                                        DocType = getDocuments.FirstOrDefault().DocType,
                                        TypeName = getDocuments.FirstOrDefault().DocName

                                    });
                                }
                            }

                            var result = getDocuments.AsEnumerable().Where(x => !presentDocuments.AsEnumerable().Any(x2 => x2.LocalDocID == x.AppDocId));

                            foreach (var r in result)
                            {
                                missingDocuments.Add(new MissingDocument
                                {
                                    Present = false,
                                    DocTypeID = r.ElpsDocTypeId,
                                    LocalDocID = r.AppDocId,
                                    DocType = r.DocType,
                                    TypeName = r.DocName
                                });
                            }

                            bothDocuments.Add(new BothDocuments
                            {
                                missingDocuments = missingDocuments,
                                presentDocuments = presentDocuments,
                            });

                            _helpersController.LogMessages("Loading facility information and document for report upload : " + Request.FirstOrDefault().RefNo, _helpersController.getSessionEmail());

                            _helpersController.LogMessages("Displaying/Viewing more application details.  Reference : " + Request.FirstOrDefault().RefNo, _helpersController.getSessionEmail());
                        }


                        requestApplicationModels.Add(new RequestApplicationModel
                        {
                            requestApplications = Request.ToList(),
                            currentDesks = currentDesk.ToList(),
                            applicationProccesses = appProcess.ToList(),
                            appSchdules = appSchdule.ToList(),
                            appReports = appReport.ToList(),
                            histories = appHistory.Take(3).ToList(),
                            applications = apps.ToList(),
                            surveyReports = surveyReport.Take(1).ToList(),
                            appDocuuments = appDocs.ToList(),
                            staffs = staffList.ToList(),
                            nominations = nomination.ToList(),
                            bothDocuments = bothDocuments.ToList(),
                            NominationRequest = nominationRequest.ToList()
                        });

                        ViewData["AppRefNo"] = Request.FirstOrDefault().RefNo;

                        _helpersController.LogMessages("Displaying/Viewing more application details.  Reference : " + Request.FirstOrDefault().RefNo, _helpersController.getSessionEmail());

                        return View(requestApplicationModels);
                    }
                    else
                    {
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong trying get staff report for application. Kindly contact support.") });
                    }

                }
                else
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found or not in correct format. Kindly contact support.") });
                }
            }
        }




        /*
         * Viewing application details without operation controls
         * 
         * id => encrypted request id
         */
        public IActionResult Apps(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found or not in correct format. Kindly contact support.") });
            }

            var appid = generalClass.DecryptIDs(id);

            if (appid == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found or not in correct format. Kindly contact support.") });
            }
            else
            {
                List<RequestApplicationModel> requestApplicationModels = new List<RequestApplicationModel>();

                var staff = _helpersController.getSessionUserID();

                var Request = from r in _context.RequestProposal.AsEnumerable()
                              join du in _context.RequestDuration.AsEnumerable() on r.DurationId equals du.DurationId
                              join c in _context.Companies.AsEnumerable() on r.CompanyId equals c.CompanyId
                              join t in _context.Transactions.AsEnumerable() on r.RequestId equals t.RequestId into trans
                              from tr in trans.DefaultIfEmpty()
                              where ((r.RequestId == appid))
                              select new RequestApplication
                              {
                                  RefNo = r.RequestRefNo,
                                  RequestId = r.RequestId,
                                  CompanyName = c.CompanyName,
                                  CompanyAddress = c.Address,
                                  CompanyEmail = c.CompanyEmail,
                                  TotalAmount = tr?.TotalAmt,
                                  RRR = tr?.Rrr,
                                  TransType = tr?.TransactionType,
                                  AmountPaid = tr?.AmtPaid,
                                  ServiceCharge = tr?.ServiceCharge,
                                  TransDate = tr?.TransactionDate,
                                  TransDescription = tr?.Description,
                                  TransStatus = tr?.TransactionStatus,
                                  Year = du.ProposalYear,
                                  Status = r.Status,
                                  DateApplied = r.DateApplied,
                                  GeneratedNo = r.GeneratedNumber,
                                  ReportApproved = r.IsReportApproved == true ? "YES" : "NO",
                                  ProposalApproved = r.IsProposalApproved == true ? "YES" : "NO",
                              };

                if (Request.Any())
                {
                    var apps = _context.Application.Where(x => x.RequestId == Request.FirstOrDefault().RequestId && x.DeletedStatus == false);

                    var appHistory = from h in _context.AppDeskHistory
                                     orderby h.HistoryId descending
                                     where h.RequestId == Request.FirstOrDefault().RequestId
                                     select new History
                                     {
                                         Status = h.Status,
                                         Comment = h.Comment,
                                         HistoryDate = h.CreatedAt.ToString(),
                                         ActionFrom = h.ActionFrom,
                                         ActionTo = h.ActionTo,
                                     };

                    var appReport = from r in _context.Reports
                                    join s in _context.Staff on r.StaffId equals s.StaffId
                                    orderby r.ReportId descending
                                    where r.RequestId == Request.FirstOrDefault().RequestId && r.DeletedStatus == false
                                    select new AppReport
                                    {
                                        ReportID = r.ReportId,
                                        Staff = s.StaffEmail,
                                        StaffID = r.StaffId,
                                        Comment = r.Comment,
                                        Title = r.Subject,
                                        CreatedAt = r.CreatedAt.ToString(),
                                        UpdatedAt = r.UpdatedAt == null ? "" : r.UpdatedAt.ToString()
                                    };


                    var appSchdule = from s in _context.Schdules
                                     join sby in _context.Staff on s.SchduleBy equals sby.StaffId
                                     orderby s.SchduleId descending
                                     where s.RequestId == Request.FirstOrDefault().RequestId && s.DeletedStatus == false
                                     select new AppSchdule
                                     {
                                         SchduleID = s.SchduleId,
                                         SchduleByID = s.SchduleBy,
                                         SchduleByEmail = sby.StaffEmail,
                                         SchduleType = s.SchduleType,
                                         SchduleLocation = s.SchduleLocation,
                                         SchduleDate = s.SchduleDate.ToString(),
                                         cResponse = s.CustomerAccept == 1 ? "Accepted" : s.CustomerAccept == 2 ? "Rejected" : "Awaiting Response",
                                         sResponse = s.SupervisorApprove == 1 ? "Accepted" : s.SupervisorApprove == 2 ? "Rejected" : "Awaiting Response",
                                         SchduleComment = s.Comment,
                                         CustomerComment = s.CustomerComment,
                                         SupervisorComment = s.SupervisorComment,
                                         CreatedAt = s.CreatedAt.ToString(),
                                         UpdatedAt = s.UpdatedAt == null ? "" : s.UpdatedAt.ToString()
                                     };


                    var currentDesk = from a in _context.RequestProposal
                                      join s in _context.Staff on a.CurrentDeskId equals s.StaffId
                                      where a.RequestId == Request.FirstOrDefault().RequestId
                                      select new CurrentDesk
                                      {
                                          Staff = s.LastName + " " + s.FirstName + " (" + s.StaffEmail + ")"
                                      };

                    var surveyReport = _context.SurveyReport.Where(x => x.RequestId == Request.FirstOrDefault().RequestId);

                    var nomination = from n in _context.NominatedStaff
                                     join s in _context.Staff on n.StaffId equals s.StaffId
                                     join r in _context.UserRoles on s.RoleId equals r.RoleId
                                     join a in _context.RequestProposal on n.RequestId equals a.RequestId
                                     where a.RequestId == Request.FirstOrDefault().RequestId
                                     select new Nomination
                                     {
                                         NominationID = n.NominateId,
                                         StaffName = s.LastName + " " + s.FirstName,
                                         UserRoles = r.RoleName,
                                         StaffEmail = s.StaffEmail.Trim(),
                                         hasSubmitted = n.HasSubmitted,
                                         CreatedBy = n.CreatedBy
                                     };

                    var nominationRequest = from r in _context.NominationRequest.AsEnumerable()
                                            join s in _context.Staff.AsEnumerable() on r.StaffId equals s.StaffId
                                            join ro in _context.UserRoles.AsEnumerable() on s.RoleId equals ro.RoleId
                                            where r.AppId == Request.FirstOrDefault().RequestId
                                            select new NominationRequests
                                            {
                                                Staff = s.LastName + " " + s.FirstName + "(" + ro.RoleName + ")",
                                                hasDone = r.HasDone == false ? "NO" : "YES",
                                                CreatedAt = r.CreatedAt,
                                                ReminderedAt = r.ReminderDate,
                                                UpdatedAt = r.UpdatedAt,
                                                Comment = r.Comment
                                            };

                    requestApplicationModels.Add(new RequestApplicationModel
                    {
                        requestApplications = Request.ToList(),
                        currentDesks = currentDesk.ToList(),
                        appSchdules = appSchdule.ToList(),
                        appReports = appReport.ToList(),
                        histories = appHistory.Take(3).ToList(),
                        applications = apps.ToList(),
                        surveyReports = surveyReport.Take(1).ToList(),
                        nominations = nomination.ToList(),
                        NominationRequest = nominationRequest.ToList()
                    });

                    ViewData["AppRefNo"] = Request.FirstOrDefault().RefNo;

                    _helpersController.LogMessages("Displaying/Viewing more application details.  Reference : " + Request.FirstOrDefault().RefNo, _helpersController.getSessionEmail());

                    return View(requestApplicationModels);

                }
                else
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found or not in correct format. Kindly contact support.") });
                }
            }
        }




        /*
        * Geetting the histories of a single application
        * 
        * id => encrypted request id
        */
        public IActionResult ApplicationHistory(string id)
        {
            var request_id = generalClass.DecryptIDs(id.Trim());

            if (request_id == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, No link was found for this application history.") });
            }
            else
            {
                var Request = from r in _context.RequestProposal.AsEnumerable()
                              join du in _context.RequestDuration.AsEnumerable() on r.DurationId equals du.DurationId
                              join c in _context.Companies.AsEnumerable() on r.CompanyId equals c.CompanyId
                              join t in _context.Transactions.AsEnumerable() on r.RequestId equals t.RequestId into trans
                              from tr in trans.DefaultIfEmpty()
                              where ((r.RequestId == request_id))
                              select new RequestApplication
                              {
                                  RefNo = r.RequestRefNo,
                                  RequestId = r.RequestId,
                                  CompanyName = c.CompanyName,
                                  CompanyAddress = c.Address,
                                  CompanyEmail = c.CompanyEmail,
                                  TotalAmount = tr?.TotalAmt,
                                  RRR = tr?.Rrr,
                                  TransType = tr?.TransactionType,
                                  AmountPaid = tr?.AmtPaid,
                                  ServiceCharge = tr?.ServiceCharge,
                                  TransDate = tr?.TransactionDate,
                                  TransDescription = tr?.Description,
                                  TransStatus = tr?.TransactionStatus,
                                  Year = du.ProposalYear,
                                  Status = r.Status,
                                  DateApplied = r.DateApplied
                              };

                var appHistory = from h in _context.AppDeskHistory
                                 orderby h.HistoryId descending
                                 where h.RequestId == Request.FirstOrDefault().RequestId
                                 select new History
                                 {
                                     Status = h.Status,
                                     Comment = h.Comment,
                                     HistoryDate = h.CreatedAt.ToString(),
                                     ActionFrom = h.ActionFrom,
                                     ActionTo = h.ActionTo
                                 };

                ViewData["AppRef"] = Request.FirstOrDefault().RefNo;

                List<HistoryInformation> historyInformation = new List<HistoryInformation>();

                historyInformation.Add(new HistoryInformation
                {
                    requestApplications = Request.ToList(),
                    histories = appHistory.ToList(),
                });

                _helpersController.LogMessages("Displaying application histories. Application reference : " + Request.FirstOrDefault().RefNo, _helpersController.getSessionEmail());

                return View(historyInformation);
            }
        }




        /*
         * Viewing an application report created by a staff
         * 
         * id => encrypted report ID
         * 
         */
        public IActionResult ViewReport(string id)
        {
            var report_id = generalClass.DecryptIDs(id.Trim());

            if (report_id == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, No link was found for this application history.") });
            }
            else
            {
                var Request = from re in _context.Reports.AsEnumerable()
                              join r in _context.RequestProposal.AsEnumerable() on re.RequestId equals r.RequestId
                              join s in _context.Staff.AsEnumerable() on re.StaffId equals s.StaffId
                              join du in _context.RequestDuration.AsEnumerable() on r.DurationId equals du.DurationId
                              join c in _context.Companies.AsEnumerable() on r.CompanyId equals c.CompanyId
                              join ro in _context.UserRoles.AsEnumerable() on s.RoleId equals ro.RoleId
                              join t in _context.Transactions.AsEnumerable() on r.RequestId equals t.RequestId into trans
                              from tr in trans.DefaultIfEmpty()
                              where ((re.ReportId == report_id && re.DeletedStatus == false))
                              select new RequestApplication
                              {
                                  RefNo = r.RequestRefNo,
                                  RequestId = r.RequestId,
                                  CompanyName = c.CompanyName,
                                  CompanyAddress = c.Address,
                                  CompanyEmail = c.CompanyEmail,
                                  TotalAmount = tr?.TotalAmt,
                                  RRR = tr?.Rrr,
                                  TransType = tr?.TransactionType,
                                  AmountPaid = tr?.AmtPaid,
                                  ServiceCharge = tr?.ServiceCharge,
                                  TransDate = tr?.TransactionDate,
                                  TransDescription = tr?.Description,
                                  TransStatus = tr?.TransactionStatus,
                                  Year = du.ProposalYear,
                                  Status = r.Status,
                                  DateApplied = r.DateApplied,

                                  Staff = s.LastName + " " + s.FirstName + " (" + s.StaffEmail + " -- " + ro.RoleName + ")",
                                  ReportDate = re.CreatedAt,
                                  UpdatedAt = re.UpdatedAt,
                                  Comment = re.Comment,
                                  Subject = re.Subject,
                                  DocSource = re.DocSource,
                              };

                ViewData["AppRef"] = Request.FirstOrDefault().RefNo;

                _helpersController.LogMessages("Displaying application report with Application reference : " + Request.FirstOrDefault().RefNo, _helpersController.getSessionEmail());

                return View(Request.ToList());
            }
        }


        /*
         * Saving application report
         * 
         * Request => encrypted request id
         * txtReport => the comment for the report
         */
        public JsonResult SaveReport(string RequestID, string txtReport, string txtReportTitle, List<SubmitDoc> SubmittedDocuments)
        {
            string result = "";

            var request_id = generalClass.DecryptIDs(RequestID.ToString().Trim());

            if (request_id == 0)
            {
                result = "Application link error";
            }
            else
            {
                Reports reports = new Reports()
                {
                    RequestId = request_id,
                    StaffId = _helpersController.getSessionUserID(),
                    Subject = txtReportTitle.ToUpper(),
                    ElpsDocId = SubmittedDocuments.FirstOrDefault()?.CompElpsDocID,
                    DocSource = SubmittedDocuments.FirstOrDefault()?.DocSource,
                    AppDocId = SubmittedDocuments.FirstOrDefault()?.LocalDocID,
                    Comment = txtReport,
                    CreatedAt = DateTime.Now,
                    DeletedStatus = false
                };

                _context.Reports.Add(reports);

                if (_context.SaveChanges() > 0)
                {
                    var apps = _context.RequestProposal.Where(x => x.RequestId == request_id);

                    var actionFrom = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());
                    var actionTo = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());

                    _helpersController.SaveHistory(request_id, actionFrom, actionTo, "Report", "A report has been added to this application.");

                    result = "Report Saved";
                    _helpersController.LogMessages("Saving report for application : " + apps.FirstOrDefault().RequestRefNo, _helpersController.getSessionEmail());
                }
                else
                {
                    result = "Something went wrong trying to save your report";
                }
            }

            _helpersController.LogMessages("Results from saving application report : " + result, _helpersController.getSessionEmail());

            return Json(result);
        }



        /*
         * Getting application report for editing
         * 
         * ReportID => encrypted report id
         */
        public JsonResult GetReport(string ReportID)
        {
            string result = "";

            int rID = 0;

            var rid = generalClass.Decrypt(ReportID.Trim());

            if (rid == "Error")
            {
                result = "0|Application report link error";
            }
            else
            {
                rID = Convert.ToInt32(rid);

                var get = _context.Reports.Where(x => x.ReportId == rID);

                if (get.Any())
                {
                    result = "1|" + get.FirstOrDefault().Comment + "|" + get.FirstOrDefault().Subject;
                }
                else
                {
                    result = "0|Error... cannot find this application report.";
                }
            }

            _helpersController.LogMessages("Displaying report. Report ID : " + rID, _helpersController.getSessionEmail());

            return Json(result);
        }



        /*
         * Editing application report
         * 
         * ReportID => encrypted report id
         * txtReport => comment for the report
         */
        public JsonResult EditReport(string ReportID, string txtReport, string txtReportTitle, List<SubmitDoc> SubmittedDocuments)
        {
            string result = "";

            int rID = 0;

            var reportId = generalClass.Decrypt(ReportID.ToString().Trim());

            if (reportId == "Error")
            {
                result = "Application link error";
            }
            else
            {
                rID = Convert.ToInt32(reportId);

                var get = _context.Reports.Where(x => x.ReportId == rID);

                if (get.Any())
                {
                    int requestid = get.FirstOrDefault().RequestId;

                    get.FirstOrDefault().Comment = txtReport;
                    get.FirstOrDefault().UpdatedAt = DateTime.Now;
                    get.FirstOrDefault().Subject = txtReportTitle.ToUpper();

                    if (SubmittedDocuments.FirstOrDefault()?.CompElpsDocID != 0)
                    {
                        get.FirstOrDefault().ElpsDocId = SubmittedDocuments.FirstOrDefault()?.CompElpsDocID;
                    }

                    if (SubmittedDocuments.FirstOrDefault()?.LocalDocID != 0)
                    {
                        get.FirstOrDefault().AppDocId = SubmittedDocuments.FirstOrDefault()?.LocalDocID;
                    }

                    if (SubmittedDocuments.FirstOrDefault()?.DocSource != "NILL")
                    {
                        get.FirstOrDefault().DocSource = SubmittedDocuments.FirstOrDefault()?.DocSource;
                    }


                    if (_context.SaveChanges() > 0)
                    {
                        var actionFrom = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());
                        var actionTo = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());

                        _helpersController.SaveHistory(requestid, actionFrom, actionTo, "Edit Report", "A report has been updated to this application with title " + txtReportTitle.ToUpper());

                        result = "Report Edited";
                    }
                    else
                    {
                        result = "Something went wrong trying to save your report";
                    }
                }
                else
                {
                    result = "Something went wrong trying to find this report.";
                }
            }

            _helpersController.LogMessages("Report status :" + result + " Report ID : " + rID, _helpersController.getSessionEmail());

            return Json(result);
        }



        /*
       * Deleting application report
       * 
       * ReportID => encryted report id
       */
        public JsonResult DeleteReport(string ReportID)
        {
            string result = "";

            int rID = 0;

            var reportId = generalClass.Decrypt(ReportID.ToString().Trim());

            if (reportId == "Error")
            {
                result = "Application link error";
            }
            else
            {
                rID = Convert.ToInt32(reportId);
            }

            var get = _context.Reports.Where(x => x.ReportId == rID);

            if (get.Any())
            {
                get.FirstOrDefault().DeletedBy = _helpersController.getSessionUserID(); // change to session id
                get.FirstOrDefault().DeletedStatus = true;
                get.FirstOrDefault().DeletedAt = DateTime.Now;

                if (_context.SaveChanges() > 0)
                {
                    result = "Report Deleted";
                }
                else
                {
                    result = "Something went wrong trying to delete your report";
                }
            }
            else
            {
                result = "Something went wrong trying to find this report.";
            }

            _helpersController.LogMessages("Report status :" + result + " Report ID : " + rID, _helpersController.getSessionEmail());

            return Json(result);
        }



        /*
         * Creating schdule for application
         * 
         * RequestID => encrypted request id
         * DeskID => encrypted application desk id
         * SchduleComment => comment for the schedule
         * ScheduleDate => The date and time for the schedule
         * SchduleLoaction => Where the schedule is ment to be held
         * ScheduleType => the type of schedule
         */
        public async Task<JsonResult> CreateSchduleAsync(string RequestID, string DeskID, string SchduleDate, string SchduleComment, string SchduleLocation, string SchduleType)
        {
            string result = ""; ;

            var date = DateTime.Parse(SchduleDate.Trim());

            var appID = generalClass.DecryptIDs(RequestID.Trim());
            var deskID = generalClass.DecryptIDs(DeskID.Trim());

            var staff_id = _helpersController.getSessionUserID();

            if (appID == 0 || deskID == 0)
            {
                result = "Application link error";
            }
            else
            {
                var getApps = _context.RequestProposal.Where(x => x.RequestId == appID && x.DeletedStatus == false).ToList();

                var getCompany = _context.Companies.Where(x => x.CompanyId == getApps.FirstOrDefault().CompanyId && x.DeleteStatus == false && x.ActiveStatus == true).ToList();

                if (getApps.Any() && getCompany.Any())
                {
                    var myDesk = _context.MyDesk.Where(x => x.DeskId == deskID);
                    var schdule = _context.Schdules.Where(x => x.RequestId == appID && x.SchduleType.Trim() == SchduleType.Trim() && x.DeletedStatus == false);

                    if (schdule.Any())
                    {
                        var startDate = DateTime.Parse(schdule.FirstOrDefault().CreatedAt.ToString());
                        var expDate = startDate.AddDays(3);

                        if (DateTime.Now < expDate) // schdule has not expired
                        {
                            result = "A " + schdule.FirstOrDefault().SchduleType + " has already been created for " + schdule.FirstOrDefault().SchduleDate.ToString();
                        }
                        else
                        {
                            Schdules schdules = new Schdules()
                            {
                                RequestId = appID,
                                SchduleBy = staff_id,
                                SchduleType = SchduleType,
                                SchduleLocation = SchduleLocation,
                                SchduleDate = date,
                                Supervisor = staff_id,
                                SupervisorApprove = 1,
                                Comment = SchduleComment,
                                SupervisorComment = SchduleComment,
                                CreatedAt = DateTime.Now,
                                DeletedStatus = false,
                                CustomerAccept = 0,
                                IsDone = false
                            };

                            _context.Schdules.Add(schdules);

                            if (_context.SaveChanges() > 0)
                            {
                                var user = _context.Companies.Where(x => x.CompanyId == getCompany.FirstOrDefault().CompanyId).FirstOrDefault();

                                var actionFrom = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());
                                var actionTo = _helpersController.getActionHistory(user.RoleId, user.CompanyId);

                                // change to staff session id
                                _helpersController.SaveHistory(appID, actionFrom, actionTo, "Schedule", "An Application " + SchduleType + " schedule was created ");

                                // Saving Messages
                                string subject = "Application Schedule with Ref : " + getApps.FirstOrDefault().RequestRefNo;
                                string content = "You have been schedule for a " + schdules.SchduleType + " on " + schdules.SchduleDate.ToString() + " with comment -: " + schdules.Comment + " Kindly find other details below.";
                                var emailMsg = _helpersController.SaveMessage(appID, getApps.FirstOrDefault().CompanyId, subject, content);
                                var sendEmail = await _helpersController.SendEmailMessageAsync(getCompany.FirstOrDefault().CompanyEmail, getCompany.FirstOrDefault().CompanyName, subject, content, GeneralClass.COMPANY_NOTIFY, emailMsg);

                                _helpersController.LogMessages("Schedule created successfully with application reference : " + getApps.FirstOrDefault().RequestRefNo, _helpersController.getSessionEmail());


                                result = "Schdule Created";
                            }
                            else
                            {
                                result = "Something went wrong trying to create your schedule.";
                            }
                        }
                    }
                    else
                    {
                        Schdules schdules = new Schdules()
                        {
                            RequestId = appID,
                            SchduleBy = staff_id, // session staff id
                            SchduleType = SchduleType,
                            SchduleLocation = SchduleLocation,
                            SchduleDate = date,
                            Supervisor = staff_id,
                            SupervisorApprove = 1,
                            Comment = SchduleComment,
                            SupervisorComment = SchduleComment,
                            CreatedAt = DateTime.Now,
                            CustomerAccept = 0,
                            DeletedStatus = false,
                            IsDone = false
                        };

                        _context.Schdules.Add(schdules);

                        if (_context.SaveChanges() > 0)
                        {
                            result = "Schdule Created";

                            _helpersController.LogMessages("Schedule created successfully with application reference : " + getApps.FirstOrDefault().RequestRefNo, _helpersController.getSessionEmail());

                            var user = _context.Companies.Where(x => x.CompanyId == getCompany.FirstOrDefault().CompanyId).FirstOrDefault();

                            var actionFrom = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());
                            var actionTo = _helpersController.getActionHistory(user.RoleId, user.CompanyId);


                            // change to staff session id
                            _helpersController.SaveHistory(appID, actionFrom, actionTo, "Schedule", "An Application " + SchduleType + " schedule was created ");

                            // Saving Messages
                            string subject = "Application Schedule with Ref : " + getApps.FirstOrDefault().RequestRefNo;
                            string content = "You have been schedule for a " + schdules.SchduleType + " on " + schdules.SchduleDate.ToString() + " with comment -: " + schdules.Comment + " Kindly find other details below.";
                            var emailMsg = _helpersController.SaveMessage(appID, getApps.FirstOrDefault().CompanyId, subject, content);
                            var sendEmail = await _helpersController.SendEmailMessageAsync(getCompany.FirstOrDefault().CompanyEmail, getCompany.FirstOrDefault().CompanyName, subject, content, GeneralClass.COMPANY_NOTIFY, emailMsg);

                            result = "Schdule Created";
                        }
                        else
                        {
                            result = "Something went wrong trying to create your schedule.";
                        }
                    }
                }
                else
                {
                    result = "Opps! Some reference was not found maybe the application ref, stage ref or company ref. Please try again later or contact support.";
                }
            }

            _helpersController.LogMessages("Schdule result status :" + result, _helpersController.getSessionEmail());

            return Json(result);
        }



        /*
         * Get schdule for editing 
         * 
         * scheduleID => encrytped schedule id
         */

        public JsonResult GetSchdule(string schduleID)
        {
            string result = "";

            int rID = 0;

            var rid = generalClass.Decrypt(schduleID.Trim());

            if (rid == "Error")
            {
                result = "0|Application report link error";
            }
            else
            {
                rID = Convert.ToInt32(rid);

                var get = _context.Schdules.Where(x => x.SchduleId == rID);

                if (get.Any())
                {
                    result = "1|" + get.FirstOrDefault().Comment + "|" + get.FirstOrDefault().SchduleDate.ToString();
                }
                else
                {
                    result = "0|Error... cannot find this application schedule.";
                }
            }

            _helpersController.LogMessages("Displaying schedule : " + result + ". Schedule ID : " + rID, _helpersController.getSessionEmail());

            return Json(result);
        }





        /*
         * Editing a schdule
         * 
         * scheduleID => encrypted schedule id
         * txtComment => comment for the schedule
         * txtScheduleDate => The date and time for the schedule
         * txtSchduleLoaction => Where the schedule is ment to be held
         * txtScheduleType => the type of schedule
         */
        public async Task<JsonResult> EditSchduleAsync(string schduleID, string txtComment, string txtSchduleDate, string txtSchduleLoaction, string txtSchduleType)
        {
            string result = "";

            var schid = generalClass.DecryptIDs(schduleID.ToString().Trim());

            if (schid == 0)
            {
                result = "Application link error";
            }
            else
            {
                var get = _context.Schdules.Where(x => x.SchduleId == schid);
                var getApps = _context.RequestProposal.Where(x => x.RequestId == get.FirstOrDefault().RequestId && x.DeletedStatus == false);
                var getCompany = _context.Companies.Where(x => x.CompanyId == getApps.FirstOrDefault().CompanyId && x.DeleteStatus == false && x.ActiveStatus == true);

                if (get.Any() && getApps.Any() && getCompany.Any())
                {
                    get.FirstOrDefault().Comment = txtComment;
                    get.FirstOrDefault().SchduleLocation = txtSchduleLoaction;
                    get.FirstOrDefault().SchduleType = txtSchduleType;
                    get.FirstOrDefault().SchduleDate = DateTime.Parse(txtSchduleDate.Trim());
                    get.FirstOrDefault().UpdatedAt = DateTime.Now;
                    get.FirstOrDefault().CustomerAccept = 0;
                    get.FirstOrDefault().SupervisorApprove = 1;
                    get.FirstOrDefault().SupervisorComment = txtComment;

                    if (_context.SaveChanges() > 0)
                    {
                        var user = _context.Companies.Where(x => x.CompanyId == getCompany.FirstOrDefault().CompanyId).FirstOrDefault();

                        var actionFrom = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());
                        var actionTo = _helpersController.getActionHistory(user.RoleId, user.CompanyId);


                        _helpersController.SaveHistory(get.FirstOrDefault().RequestId, actionFrom, actionTo, "Schedule", "An Application " + txtSchduleType + " schedule was edited ");

                        // Saving Messages
                        string subject = "Application ReScheduled with Ref : " + getApps.FirstOrDefault().RequestRefNo;
                        string content = "You have been Reschedule for a " + get.FirstOrDefault().SchduleType + " on " + get.FirstOrDefault().SchduleDate + " with comment -: " + get.FirstOrDefault().Comment + " Kindly find other details below.";
                        var emailMsg = _helpersController.SaveMessage(get.FirstOrDefault().RequestId, getCompany.FirstOrDefault().CompanyId, subject, content);
                        var sendEmail = await _helpersController.SendEmailMessageAsync(getCompany.FirstOrDefault().CompanyEmail, getCompany.FirstOrDefault().CompanyName, subject, content, GeneralClass.COMPANY_NOTIFY, emailMsg);

                        result = "Schdule Edited";
                    }
                    else
                    {
                        result = "Something went wrong trying to update your schedule";
                    }
                }
                else
                {
                    result = "Something went wrong trying to find some reference. Maybe App ref, Schedule Ref, Stage Ref, or Company Ref";
                }
            }

            _helpersController.LogMessages("Schedule Status : " + result + ". Schedule ID : " + schid, _helpersController.getSessionEmail());

            return Json(result);
        }




        /*
         * Delete Application schdule
         * 
         * scheduleID => encrypted schedule id
         */

        public JsonResult DeleteSchdule(string schduleID)
        {
            string result = "";

            int rID = 0;

            var reportId = generalClass.Decrypt(schduleID.ToString().Trim());

            if (reportId == "Error")
            {
                result = "Application link error";
            }
            else
            {
                rID = Convert.ToInt32(reportId);

                var get = _context.Schdules.Where(x => x.SchduleId == rID);

                if (get.Any())
                {
                    get.FirstOrDefault().DeletedBy = _helpersController.getSessionUserID();
                    get.FirstOrDefault().DeletedStatus = true;
                    get.FirstOrDefault().DeletedAt = DateTime.Now;

                    if (_context.SaveChanges() > 0)
                    {
                        result = "Schdule Deleted";
                    }
                    else
                    {
                        result = "Something went wrong trying to delete your schedule";
                    }
                }
                else
                {
                    result = "Something went wrong trying to find this schedule.";
                }
            }

            _helpersController.LogMessages("Schedule Status : " + result + ". Schedule ID : " + rID, _helpersController.getSessionEmail());

            return Json(result);
        }





        /*
         * All application approval process/method
         * 
         * txtComment => Not encrypted; comment for approval.
         * txtDeskID => Encrypted desk id
         * txtRequestID => Encrypted request id
         * txtApproveOption => option to take weeks, days or months
         * txtApproveDuration => a number for how long the permit will last
         */
        public async Task<JsonResult> ApprovalAsync(string txtComment, string txtDeskID, string txtRequestID)
        {
            string result = "";

            int deskID = generalClass.DecryptIDs(txtDeskID);
            int requestid = generalClass.DecryptIDs(txtRequestID);
            int staff_id = _helpersController.getSessionUserID();

            var start = "";

            if (deskID == 0 || requestid == 0)
            {
                result = "Something went wrong, Application or Desk reference not in correct format. Please refreash the page.";
            }
            else
            {
                // current process
                var getDesk = from d in _context.MyDesk.AsEnumerable()
                              join p in _context.ApplicationProccess.AsEnumerable() on d.ProcessId equals p.ProccessId
                              join r in _context.UserRoles.AsEnumerable() on p.OnAcceptRoleId equals r.RoleId
                              join a in _context.RequestProposal.AsEnumerable() on d.RequestId equals a.RequestId
                              join c in _context.Companies.AsEnumerable() on a.CompanyId equals c.CompanyId
                              where d.DeskId == deskID && a.RequestId == requestid && d.HasWork == false && a.DeletedStatus == false
                              select new
                              {
                                  d,
                                  p,
                                  a,
                                  c
                              };

                if (getDesk.Any())
                {
                    start = getDesk.FirstOrDefault().p.Process;

                    if (getDesk.FirstOrDefault().p.Process == GeneralClass.START || getDesk.FirstOrDefault().p.Process == GeneralClass.NEXT || getDesk.FirstOrDefault().p.Process == GeneralClass.BEGIN)
                    {
                        // getting staff to drop application on 
                        int AppDropStaffID = _helpersController.ApplicationDropStaff((getDesk.FirstOrDefault().d.Sort + 1));
                        List<ApplicationProccess> process = _helpersController.GetAppProcess(0, (getDesk.FirstOrDefault().d.Sort + 1));

                        if (AppDropStaffID > 0 && process.Any())
                        {
                            var checkdesk = _context.MyDesk.Where(x => x.RequestId == requestid && x.ProcessId == process.FirstOrDefault().ProccessId && x.Sort == process.FirstOrDefault().Sort && x.HasWork == false);

                            if (checkdesk.Any())
                            {
                                result = "Sorry, this application is already on a staff desk.";
                            }
                            else
                            {
                                MyDesk desk = new MyDesk()
                                {
                                    ProcessId = process.FirstOrDefault().ProccessId,
                                    RequestId = requestid,
                                    StaffId = AppDropStaffID,
                                    HasWork = false,
                                    CreatedAt = DateTime.Now,
                                    HasPushed = true,
                                    Sort = process.FirstOrDefault().Sort,
                                };

                                _context.MyDesk.Add(desk);

                                if (_context.SaveChanges() > 0)
                                {
                                    var user = _context.Staff.Where(x => x.StaffId == AppDropStaffID).FirstOrDefault();

                                    var actionFrom = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());
                                    var actionTo = _helpersController.getActionHistory(user.RoleId, user.StaffId);


                                    _helpersController.SaveHistory(requestid, actionFrom, actionTo, "Recommended", "Application recommendation approval sent and landed on staff desk =>" + txtComment);

                                    var desks = _context.MyDesk.Where(x => x.DeskId == deskID);
                                    var apps = _context.RequestProposal.Where(x => x.RequestId == requestid);

                                    desks.FirstOrDefault().HasWork = true;
                                    desks.FirstOrDefault().UpdatedAt = DateTime.Now;
                                    desks.FirstOrDefault().Comment = txtComment;

                                    apps.FirstOrDefault().UpdatedAt = DateTime.Now;
                                    apps.FirstOrDefault().CurrentDeskId = AppDropStaffID;

                                    string subject = "Application with Ref On Your Desk : " + apps.FirstOrDefault().RequestRefNo;
                                    string content = "An application with reference number " + apps.FirstOrDefault().RequestRefNo + " has landed on your desk with comment -: " + txtComment + ".";


                                    if (_context.SaveChanges() > 0)
                                    {
                                        // Saving Messages
                                       var sendEmail = _helpersController.SendEmailMessageAsync(user.StaffEmail, user.LastName + " " + user.FirstName, subject, content, GeneralClass.STAFF_NOTIFY, null);

                                        result = "Approved Next";
                                    }
                                    else
                                    {
                                        result = "Something went wrong trying to update your desk";
                                    }
                                }
                                else
                                {
                                    result = "Something went wrong trying to push this application to the next processing officer.";
                                }
                            }
                        }
                        else
                        {
                            result = "Sorry, could not find staff to push to or process reference. Please try again later.";
                        }
                    }
                    else if (getDesk.FirstOrDefault().p.Process == GeneralClass.DONE) // for approve report
                    {
                        var myDesk = _context.MyDesk.Where(x => x.DeskId == deskID);
                        var getApps = _context.RequestProposal.Where(x => x.RequestId == requestid);

                        myDesk.FirstOrDefault().HasWork = true;
                        myDesk.FirstOrDefault().HasPushed = true;
                        myDesk.FirstOrDefault().UpdatedAt = DateTime.Now;
                        myDesk.FirstOrDefault().Comment = txtComment;

                        getApps.FirstOrDefault().Status = GeneralClass.Approved;
                        getApps.FirstOrDefault().UpdatedAt = DateTime.Now;
                        getApps.FirstOrDefault().IsReportApproved = true;

                        if (_context.SaveChanges() > 0)
                        {
                            var getApp = _context.RequestProposal.Where(x => x.RequestId == requestid);
                            var company = _context.Companies.Where(x => x.CompanyId == getApp.FirstOrDefault().CompanyId);

                            var actionFrom = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());
                            var actionTo = _helpersController.getActionHistory(company.FirstOrDefault().RoleId, company.FirstOrDefault().CompanyId);

                            _helpersController.SaveHistory(requestid, actionFrom, actionTo, "Report Approval", "Application Report has been approved (Report APPROVAL) => " + txtComment);

                            string subject = "Application Survey Report Approved With Ref : " + getApp.FirstOrDefault().RequestRefNo;
                            string content = "Your application survey report has been APPROVED successfully. Kindly find other details below. You can login to the NUPRC's BHP portal to view.";
                            var emailMsg = _helpersController.SaveMessage(requestid, company.FirstOrDefault().CompanyId, subject, content);
                            var sendEmail = await _helpersController.SendEmailMessageAsync(company.FirstOrDefault().CompanyEmail, company.FirstOrDefault().CompanyName, subject, content, GeneralClass.COMPANY_NOTIFY, emailMsg);
                            _helpersController.UpdateElpsApplication(getApp.ToList());

                            result = "Report Approved";
                        }
                        else
                        {
                            result = "Something went wrong trying to update applicatioin report status.";
                        }
                    }
                    else // END Generate Permit
                    {
                        // check if permit already generated
                        var checkPermit = _context.Permits.Where(x => x.RequestId == requestid);

                        if (checkPermit.Any())
                        {
                            result = "Permit has already been generated for this application";
                        }
                        else
                        {
                            var checkNominate = _context.NominatedStaff.Where(x => x.RequestId == requestid);

                            if(checkNominate.Any())
                            {
                                var PermitNO = _helpersController.GeneratePermitNumber();
                                var seq = _context.Permits.Count();

                                BHP.Models.DB.Permits permits = new BHP.Models.DB.Permits()
                                {
                                    RequestId = requestid,
                                    PermitNo = PermitNO,
                                    Printed = false,
                                    IsRenewed = false,
                                    IssuedDate = DateTime.Now,
                                    ExpireDate = DateTime.Now.AddYears(1),
                                    CreatedAt = DateTime.Now,
                                    ApprovedBy = staff_id,
                                    PermitSequence = (seq + 1)
                                };

                                _context.Permits.Add(permits);

                                if (_context.SaveChanges() > 0)
                                {
                                    // posting permits to elps
                                    var posted = _helpersController.PostPermitToElps(permits.PermitId, permits.PermitNo, getDesk.FirstOrDefault().a.RequestRefNo, getDesk.FirstOrDefault().c.CompanyElpsId, permits.IssuedDate, permits.ExpireDate, (bool)permits.IsRenewed);

                                    var myDesk = _context.MyDesk.Where(x => x.DeskId == deskID);
                                    var getApps = _context.RequestProposal.Where(x => x.RequestId == requestid);

                                    myDesk.FirstOrDefault().HasWork = true;
                                    myDesk.FirstOrDefault().HasPushed = true;
                                    myDesk.FirstOrDefault().UpdatedAt = DateTime.Now;
                                    myDesk.FirstOrDefault().Comment = txtComment;

                                    getApps.FirstOrDefault().Status = GeneralClass.Approved;
                                    getApps.FirstOrDefault().UpdatedAt = DateTime.Now;
                                    getApps.FirstOrDefault().IsProposalApproved = true;

                                    if (_context.SaveChanges() > 0)
                                    {
                                        var getApp = _context.RequestProposal.Where(x => x.RequestId == requestid);
                                        var company = _context.Companies.Where(x => x.CompanyId == getApp.FirstOrDefault().CompanyId);

                                        var actionFrom = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());
                                        var actionTo = _helpersController.getActionHistory(company.FirstOrDefault().RoleId, company.FirstOrDefault().CompanyId);

                                        _helpersController.SaveHistory(requestid, actionFrom, actionTo, "Final Approval", "Application has been approved (FINAL APPROVAL) => " + txtComment);

                                        string subject = "Application Approved with Permit NO : " + permits.PermitNo;
                                        string content = "Your application  with Permit Number " + permits.PermitNo + " has been APPROVED. Kindly find other details below. You can login to the NUPRC's BHP portal and download your approval letter.";
                                        var emailMsg = _helpersController.SaveMessage(requestid, company.FirstOrDefault().CompanyId, subject, content);
                                        var sendEmail = await _helpersController.SendEmailMessageAsync(company.FirstOrDefault().CompanyEmail, company.FirstOrDefault().CompanyName, subject, content, GeneralClass.COMPANY_NOTIFY, emailMsg);
                                        _helpersController.UpdateElpsApplication(getApp.ToList());

                                        result = "Approved";
                                    }
                                    else
                                    {
                                        result = "Application approved but Something went wrong trying to update applicatioin status.";
                                    }
                                }
                                else
                                {
                                    result = "Something went wrong trying to save this permit. Please try again later";
                                }
                            }
                            else
                            {
                                result = "You need to designate one or more staff to witness the survey.";
                            }
                        }
                    }
                }
                else
                {
                    result = "Opps!!! something went wrong trying to fetch your desk. Please try again later.";
                }
            }

            _helpersController.LogMessages("Result from Application Approval process : " + result, _helpersController.getSessionEmail());

            return Json(result);
        }



        /*
         * A application rejection process/method
         * 
         * DeskID => encrypted application desk ID
         * txtComment => rejection comment
         * RequiredDocs => a list of document id for rejection
         * 
         */
        public async Task<JsonResult> RejectionAsync(string DeskID, string txtComment)
        {
            string result = "";

            var deskID = generalClass.DecryptIDs(DeskID);
            int previousSort = 0;

            var refNo = "";

            if (deskID == 0)
            {
                result = "Something went wrong, Desk reference not in correct format.";
            }
            else
            {
                var getDesk = _context.MyDesk.Where(x => x.DeskId == deskID);

                var getApps = _context.RequestProposal.Where(x => x.RequestId == getDesk.FirstOrDefault().RequestId && x.DeletedStatus == false);

                // finding rejection role of a staff
                var getProcess = from ap in _context.ApplicationProccess.AsEnumerable()
                                 join r in _context.UserRoles.AsEnumerable() on ap.OnRejectRoleId equals r.RoleId
                                 where ap.ProccessId == getDesk.FirstOrDefault().ProcessId && ap.DeleteStatus == false && r.DeleteStatus == false
                                 select new
                                 {
                                     ap,
                                     r
                                 };

                var company = _context.Companies.Where(x => x.CompanyId == getApps.FirstOrDefault().CompanyId);


                if (getDesk.Any() && getApps.Any() && getProcess.Any() && company.Any())
                {
                    refNo = getApps.FirstOrDefault().RequestRefNo;

                    // Planning or reviewer staff rejecting with no documents to attached back to customer
                    if (getProcess.FirstOrDefault().r.RoleName == GeneralClass.COMPANY)
                    {
                        getApps.FirstOrDefault().DeskId = getDesk.FirstOrDefault().DeskId;
                        getApps.FirstOrDefault().CurrentDeskId = getDesk.FirstOrDefault().StaffId;
                        getApps.FirstOrDefault().UpdatedAt = DateTime.Now;
                        getApps.FirstOrDefault().Status = GeneralClass.Rejected;

                        getDesk.FirstOrDefault().HasWork = true;
                        getDesk.FirstOrDefault().UpdatedAt = DateTime.Now;
                        getDesk.FirstOrDefault().Comment = txtComment;

                        if (_context.SaveChanges() > 0)
                        {
                            var actionFrom = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());
                            var actionTo = _helpersController.getActionHistory(company.FirstOrDefault().RoleId, company.FirstOrDefault().CompanyId);

                            _helpersController.SaveHistory(getDesk.FirstOrDefault().RequestId, actionFrom, actionTo, "Rejected", "Staff rejected customer's application => " + txtComment);

                            // Saving Messages
                            string subject = "Application REJECTED with Ref : " + getApps.FirstOrDefault().RequestRefNo;
                            string content = "Your application have been Rejected with comment -: " + txtComment + " Kindly find other details below.";
                            var emailMsg = _helpersController.SaveMessage(getApps.FirstOrDefault().RequestId, company.FirstOrDefault().CompanyId, subject, content);
                            var sendEmail = await _helpersController.SendEmailMessageAsync(company.FirstOrDefault().CompanyEmail, company.FirstOrDefault().CompanyName, subject, content, GeneralClass.COMPANY_NOTIFY, emailMsg);

                            result = "External Rejection";

                            _helpersController.LogMessages("Staff rejected customer's application. Application reference : " + getApps.FirstOrDefault().RequestRefNo, _helpersController.getSessionEmail());

                            _helpersController.UpdateElpsApplication(getApps.ToList());
                        }
                        else
                        {
                            result = "Something went wrong trying to update your desk.";
                        }
                    }
                    else // supervisor and the staff above rejecting back to staff
                    {
                        while (getDesk.Any())
                        {
                            previousSort++;

                            var getPreviousStaff = from d in _context.MyDesk.AsEnumerable()
                                                   join pr in _context.ApplicationProccess.AsEnumerable() on d.ProcessId equals pr.ProccessId
                                                   join s in _context.Staff.AsEnumerable() on d.StaffId equals s.StaffId
                                                   join r in _context.UserRoles.AsEnumerable() on s.RoleId equals r.RoleId
                                                   where r.RoleName == getProcess.FirstOrDefault().r.RoleName && (d.Sort == (getDesk.FirstOrDefault().Sort - previousSort) && d.RequestId == getDesk.FirstOrDefault().RequestId && s.ActiveStatus == true && s.DeleteStatus == false && pr.DeleteStatus == false && r.DeleteStatus == false)
                                                   select new
                                                   {
                                                       d,
                                                       pr,
                                                   };

                            if (getPreviousStaff.Any())
                            {
                                var prevDesk = _context.MyDesk.Where(x => x.RequestId == getDesk.FirstOrDefault().RequestId && x.StaffId == getPreviousStaff.FirstOrDefault().d.StaffId && x.HasWork == true).OrderByDescending(c => c.DeskId).Take(1);

                                if (prevDesk.Any())
                                {
                                    MyDesk desk = new MyDesk()
                                    {
                                        ProcessId = prevDesk.FirstOrDefault().ProcessId,
                                        RequestId = prevDesk.FirstOrDefault().RequestId,
                                        StaffId = prevDesk.FirstOrDefault().StaffId,
                                        HasWork = false,
                                        CreatedAt = DateTime.Now,
                                        HasPushed = true,
                                        Sort = prevDesk.FirstOrDefault().Sort
                                    };

                                    _context.MyDesk.Add(desk);

                                    if (_context.SaveChanges() > 0)
                                    {
                                        var user = _context.Staff.Where(x => x.StaffId == prevDesk.FirstOrDefault().StaffId);

                                        var actionFrom = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());
                                        var actionTo = _helpersController.getActionHistory(user.FirstOrDefault().RoleId, user.FirstOrDefault().StaffId);

                                        _helpersController.SaveHistory(getDesk.FirstOrDefault().RequestId, actionFrom, actionTo, "Moved", "Application landed on staff desk");

                                        getDesk.FirstOrDefault().HasWork = true;
                                        getDesk.FirstOrDefault().UpdatedAt = DateTime.Now;
                                        getDesk.FirstOrDefault().Comment = txtComment;

                                        if (_context.SaveChanges() > 0)
                                        {
                                            _helpersController.SaveHistory(getDesk.FirstOrDefault().RequestId, actionFrom, actionTo, "In-Reject", "Application was rejected (Internal) to staff => " + txtComment);

                                            var app = _context.RequestProposal.Where(x => x.RequestId == getDesk.FirstOrDefault().RequestId);
                                            app.FirstOrDefault().CurrentDeskId = desk.StaffId;
                                            app.FirstOrDefault().UpdatedAt = DateTime.Now;
                                            
                                            
                                            string subject =  "Application REJECTED with Ref : " + app.FirstOrDefault().RequestRefNo;
                                            string content = "Your application have been Rejected with comment -: " + txtComment + " Kindly find application details below.";
                                            var sendEmail = _helpersController.SendEmailMessageAsync(user.FirstOrDefault().StaffEmail, user.FirstOrDefault().LastName + " " + user.FirstOrDefault().FirstName, subject, content, GeneralClass.STAFF_NOTIFY, null);
                                            
                                            _context.SaveChanges();

                                            result = "Internal Rejection";
                                            break;
                                        }
                                        else
                                        {
                                            result = "Something went wrong trying to update your desk";
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        result = "Something went wrong trying to reject this application. Try again.";
                                        break;
                                    }
                                }
                                else
                                {
                                    result = "Sorry, could not find previouse processing staff to reject application back.";
                                    break;
                                }
                            }
                            else
                            {
                                result = "Cannot find previouse processing staff. Please try again later.";
                                break;
                            }
                        }
                    }
                }
                else
                {
                    result = "Something went wrong trying to find either desk application reference, application reference, processing reference, stage reference or company profile";
                }
            }

            _helpersController.LogMessages("Result from application rejection : " + result + ". Ref No : " + refNo, _helpersController.getSessionEmail());

            return Json(result);
        }


        /*
        * Sending Nomination request to a zopscon
        * 
        */
        public JsonResult SendNominationRequest(string RequestID, int txtStaffID, string txtComment)
        {
            try
            {
                string result = "";

                var appRequestId = generalClass.DecryptIDs(RequestID);

                if (appRequestId == 0)
                {
                    result = "Something went wrong, request reference not in correct format.";
                }
                else
                {
                    var staff = _context.Staff.Where(x => x.StaffId == txtStaffID && x.ActiveStatus == true && x.DeleteStatus == false).Select(x => new Staff { StaffId = x.StaffId });

                    if (staff.Any())
                    {
                        var checkStaff = _context.NominationRequest.Where(x => x.StaffId == staff.FirstOrDefault().StaffId && x.AppId == appRequestId && x.HasDone == false);

                        if (checkStaff.Any())
                        {
                            result = "This nomination request has already been sent to this staff.";
                        }
                        else
                        {
                            BHP.Models.DB.NominationRequest nominated = new BHP.Models.DB.NominationRequest()
                            {
                                StaffId = staff.FirstOrDefault().StaffId,
                                AppId = appRequestId,
                                Comment = txtComment,
                                HasDone = false,
                                CreatedAt = DateTime.Now,
                            };

                            _context.NominationRequest.Add(nominated);

                            if (_context.SaveChanges() > 0)
                            {
                                result = "Saved";

                                var apps = _context.RequestProposal.Where(x => x.RequestId == appRequestId);

                                var user = _context.Staff.Where(x => x.StaffId == txtStaffID);

                                var actionFrom = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());
                                var actionTo = _helpersController.getActionHistory(user.FirstOrDefault().RoleId, user.FirstOrDefault().StaffId);

                                var NominationLink = ElpsServices.link + "Applications/NominationRequest/" + generalClass.Encrypt(nominated.RequestId.ToString());

                                var subject = "BHP REQUEST FOR NOMINATION (" + apps.FirstOrDefault().RequestRefNo + ")";
                                var content = "You have a request to nominate some staff that will take part in the witnessing of an application with reference number : " + apps.FirstOrDefault().RequestRefNo + ". Please click on this link to select staff => <a href='" + NominationLink + "'>(RESPOND HERE)</a> <hr> <b>See Comment </b> <br> " + txtComment;
                                var sendEmail = _helpersController.SendEmailMessageAsync(user.FirstOrDefault().StaffEmail, user.FirstOrDefault().LastName + " " + user.FirstOrDefault().FirstName, subject, content, GeneralClass.STAFF_NOTIFY, null);
                                _helpersController.SaveHistory(appRequestId, actionFrom, actionTo, "Nomination", "Request has been sent to nominate satff for this application");

                            }
                            else
                            {
                                result = "Something went wrong trying to send this nomination request to staff. Please try again later.";
                            }
                        }
                    }
                    else
                    {
                        result = "Opps!!! this staff was not found, please try again later.";
                    }
                }

                _helpersController.LogMessages("Result from sending nomination to staff : " + result, _helpersController.getSessionEmail());

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json("Error " + ex.Message);
            }
        }



        public JsonResult SaveNominatedStaff(int txtNomId, int RequestId, List<int> StaffId)
        {
            try
            {
                string result = "";

                var nomid = txtNomId;
                var requestid = RequestId;
                int done = 0;

                var request = _context.NominationRequest.Where(x => x.RequestId == RequestId);


                if (nomid == 0 && request.Any())
                {
                    result = "Something went wrong, request reference not in correct format.";
                }
                else
                {
                    int staff_id = request.FirstOrDefault().StaffId;


                    var removeStaff = _context.NominatedStaff.Where(x => x.RequestId == requestid && x.HasSubmitted == false && x.IsActive == false);

                    _context.NominatedStaff.RemoveRange(removeStaff);
                    _context.SaveChanges();

                    foreach (var p in StaffId)
                    {
                        if (p != 0)
                        {
                            var checkStaff = _context.NominatedStaff.Where(x => x.StaffId == p && x.RequestId == requestid);

                            if (checkStaff.Count() <= 0)
                            {
                                NominatedStaff nominated = new NominatedStaff()
                                {
                                    StaffId = p,
                                    RequestId = requestid,
                                    Designation = "UMR",
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = staff_id,
                                    HasSubmitted = false,
                                    IsActive = false
                                };

                                _context.NominatedStaff.Add(nominated);
                                done += _context.SaveChanges();
                            }
                        }
                    }

                    if (done > 0)
                    {
                        var NominationRequest = _context.NominationRequest.Where(x => x.RequestId == requestid);

                        NominationRequest.FirstOrDefault().HasDone = true;
                        NominationRequest.FirstOrDefault().UpdatedAt = DateTime.Now;

                        _context.SaveChanges();

                        var staff = _context.Staff.Where(x => x.StaffId == staff_id);

                        var actionFrom = _helpersController.getActionHistory(staff.FirstOrDefault().RoleId, staff.FirstOrDefault().StaffId);
                        var actionTo = _helpersController.getActionHistory(staff.FirstOrDefault().RoleId, staff.FirstOrDefault().StaffId);

                        _helpersController.SaveHistory(requestid, actionFrom, actionTo, "Nomination", "Saff has been save for this nomination");

                        _helpersController.LogMessages("Result from Nominating a staff : " + result, staff.FirstOrDefault().StaffEmail);

                        result = "Saved";
                    }
                    else
                    {
                        result = "Something went wrong trying to save this nominated staff. Please try again later.";
                    }
                }



                return Json(result);
            }
            catch (Exception ex)
            {
                return Json("Error " + ex.Message);
            }
        }



        [AllowAnonymous]
        public IActionResult NominationLink(string id)
        {
            var norm_id = generalClass.DecryptIDs(id.Trim());

            if (norm_id == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, Nomination link reference not in correct format, please try again later.") });
            }
            else
            {
                var get = from n in _context.NominatedStaff
                          join a in _context.RequestProposal on n.RequestId equals a.RequestId
                          join c in _context.Companies on a.CompanyId equals c.CompanyId
                          join s in _context.Staff on n.StaffId equals s.StaffId
                          where n.NominateId == norm_id && n.HasSubmitted == false && n.IsActive == true
                          select new
                          {
                              AppRef = a.RequestRefNo,
                              RequestId = a.RequestId,
                              StaffName = s.LastName + " " + s.FirstName + " (" + s.StaffEmail + ")",
                              CompanyName = c.CompanyName
                          };

                if (get.Any())
                {
                    ViewData["RefNo"] = get.FirstOrDefault().AppRef;
                    ViewData["RequestId"] = get.FirstOrDefault().RequestId;
                    ViewData["StaffName"] = get.FirstOrDefault().StaffName;
                    ViewData["CompanyName"] = get.FirstOrDefault().CompanyName;
                    ViewData["NominationID"] = norm_id;

                    return View();
                }
                else
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, Nomination not found, please try again later.") });
                }
            }
        }



        [AllowAnonymous]
        public IActionResult NominationRequest(string id)
        {
            var nominationId = generalClass.DecryptIDs(id.Trim());

            if (nominationId == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, Nomination link reference not in correct format, please try again later.") });
            }
            else
            {
                var get = from n in _context.NominationRequest
                          join a in _context.RequestProposal on n.RequestId equals a.RequestId
                          join d in _context.RequestDuration on a.DurationId equals d.DurationId
                          join c in _context.Companies on a.CompanyId equals c.CompanyId
                          join st in _context.Staff on n.StaffId equals st.StaffId
                          join r in _context.UserRoles on st.RoleId equals r.RoleId
                          join f in _context.FieldOffices.AsEnumerable() on st.FieldOfficeId equals f.FieldOfficeId
                          join zf in _context.ZoneFieldOffice.AsEnumerable() on f.FieldOfficeId equals zf.FieldOfficeId
                          join z in _context.ZonalOffice.AsEnumerable() on zf.ZoneId equals z.ZoneId
                          where n.RequestId == nominationId
                          select new
                          {
                              AppRef = a.RequestRefNo,
                              RequestId = a.RequestId,
                              StaffName = st.LastName + " " + st.FirstName + " (" + r.RoleName + ")",
                              CompanyName = c.CompanyName,
                              CompanyAddress = c.Address + ", " + c.City + ", " + c.StateName,
                              CompanyEmail = c.CompanyEmail,
                              hasDone = n.HasDone == false ? "NO" : "YES",
                              CreatedAt = n.CreatedAt,
                              Comment = n.Comment,
                              ZonalId = z.ZoneId,
                              StaffId = st.StaffId,
                              NomId = n.RequestId
                          };

                if (get.Any())
                {
                    ViewData["RefNo"] = get.FirstOrDefault().AppRef;
                    ViewData["RequestId"] = get.FirstOrDefault().RequestId;
                    ViewData["StaffName"] = get.FirstOrDefault().StaffName;
                    ViewData["CompanyName"] = get.FirstOrDefault().CompanyName;
                    ViewData["CompanyAddress"] = get.FirstOrDefault().CompanyAddress;
                    ViewData["NomId"] = nominationId;
                    ViewData["HasDone"] = get.FirstOrDefault().hasDone;
                    ViewData["CreatedAt"] = get.FirstOrDefault().CreatedAt;
                    ViewData["Comment"] = get.FirstOrDefault().Comment;

                    var staffList = from s in _context.Staff.AsEnumerable()
                                    join r in _context.UserRoles.AsEnumerable() on s.RoleId equals r.RoleId
                                    join f in _context.FieldOffices.AsEnumerable() on s.FieldOfficeId equals f.FieldOfficeId
                                    join zf in _context.ZoneFieldOffice.AsEnumerable() on f.FieldOfficeId equals zf.FieldOfficeId
                                    join z in _context.ZonalOffice.AsEnumerable() on zf.ZoneId equals z.ZoneId
                                    where ((s.ActiveStatus == true && s.DeleteStatus == false && z.ZoneId == get.FirstOrDefault().ZonalId && s.StaffId != get.FirstOrDefault().StaffId))
                                    select new StaffNomination
                                    {
                                        FullName = s.LastName + " " + s.FirstName + " (" + s.StaffEmail + ")",
                                        RoleName = r.RoleName,
                                        StaffId = s.StaffId,
                                        FieldOffice = f.OfficeName,
                                        ZonalOffice = z.ZoneName
                                    };

                    return View(staffList.ToList());
                }
                else
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, Nomination not found, please try again later.") });
                }
            }
        }



        public JsonResult NominationAction(int id, string option, string comment)
        {
            var status = "";
            var result = "";

            if (option == "Accept")
            {
                status = "YES";
            }
            else
            {
                status = "NO";
            }

            var get = _context.NominatedStaff.Where(x => x.NominateId == id);

            if (get.Any())
            {
                var RequestId = get.FirstOrDefault().RequestId;
                var staffid = get.FirstOrDefault().StaffId;
                var createdby = get.FirstOrDefault().CreatedBy;

                get.FirstOrDefault().RespondStatus = status;
                get.FirstOrDefault().RespondComment = comment;
                get.FirstOrDefault().UpdatedAt = DateTime.Now;

                if (_context.SaveChanges() > 0)
                {
                    var staff = _context.Staff.Where(x => x.StaffId == staffid);

                    var actionFrom = _helpersController.getActionHistory(staff.FirstOrDefault().RoleId, staff.FirstOrDefault().StaffId);
                    var actionTo = _helpersController.getActionHistory(staff.FirstOrDefault().RoleId, staff.FirstOrDefault().StaffId);

                    _helpersController.SaveHistory(RequestId, actionFrom, actionTo, "Nomination " + option, "Nomination has been " + option + "ed by staff with comment : " + comment);

                    var subject = "Nomination " + option.ToUpper() + "ED BY STAFF";
                    var content = "Nomination has been " + option + "ed by staff with comment : " + comment;

                    var getStaff = _context.Staff.Where(x => x.StaffId == createdby).FirstOrDefault();

                    var send = _helpersController.SendEmailMessageAsync(getStaff?.StaffEmail, getStaff?.LastName + " " + getStaff?.FirstName, subject, content, GeneralClass.STAFF_NOTIFY, null);

                    result = "Done";
                }
                else
                {
                    result = "Something went wrong trying to " + option + " this nomination, please try again later";
                }
            }
            else
            {
                result = "nomination link not found, please try again later.";
            }

            return Json(result);
        }


        /*
         * Deleting an already nominated staff
         * 
         * NominationID => encrypted
         */
        public JsonResult DeleteNomination(string NominationID)
        {
            try
            {
                string result = "";

                var id = generalClass.DecryptIDs(NominationID);

                if (id == 0)
                {
                    result = "Something went wrong, request reference not in correct format.";
                }
                else
                {

                    var checkNom = _context.NominatedStaff.FirstOrDefault(x => x.NominateId == id);

                    if (checkNom != null)
                    {
                        var remove = _context.NominatedStaff.Remove(checkNom);

                        if (_context.SaveChanges() > 0)
                        {
                            result = "Deleted";
                        }
                        else
                        {
                            result = "Something went wrong trying to delete this nomination, please try again later.";
                        }
                    }
                    else
                    {
                        result = "Something went wrong, Nomination not found, please try agin later.";
                    }
                }

                _helpersController.LogMessages("Result from deleting nominated a staff : " + result, _helpersController.getSessionEmail());

                return Json(result);

            }
            catch (Exception ex)
            {
                return Json("Error " + ex.Message);
            }

        }


    }

    
}
