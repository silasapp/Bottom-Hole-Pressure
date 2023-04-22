using BHP.Helpers;
using BHP.HelpersClass;
using BHP.Models;
using BHP.Models.DB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BHP.Controllers.NominationRequest
{
    [Authorize]

    public class NominationRequestController : Controller
    {

        private readonly BHP_DBContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        private readonly HelpersController _helpersController;
        private readonly GeneralClass generalClass = new GeneralClass();
        public RestSharpServices _restService = new RestSharpServices();

        public NominationRequestController(BHP_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }



       
        public IActionResult Requests(string id)
        {
            List<NominationRequestList> requestLists = new List<NominationRequestList>();

            var nom = (from n in _context.NominationRequest.AsEnumerable()
                      join a in _context.RequestProposal.AsEnumerable() on n.AppId equals a.RequestId
                      join s in _context.Staff.AsEnumerable() on n.StaffId equals s.StaffId
                      join r in _context.UserRoles.AsEnumerable() on s.RoleId equals r.RoleId
                      select new NominationRequestList
                      {
                          StaffName = s.LastName + " " + s.FirstName,
                          Email = s.StaffEmail,
                          StaffId = s.StaffId,
                          NominationRequestId = n.RequestId,
                          Role = r.RoleName,
                          RefNo = a.RequestRefNo,
                          RequestId = a.RequestId,
                          isDone = n.HasDone,
                          Comment = n.Comment
                      }).ToList();

            if(id == "_desk")
            {
                requestLists = nom.Where(x => x.StaffId == _helpersController.getSessionUserID() && x.isDone == false).ToList();

                if (requestLists.Any())
                {
                    ViewData["NominationName"] = "Pending Nomination request for " + requestLists.FirstOrDefault().StaffName.ToString();
                }
                else
                {
                    ViewData["NominationName"] = "No record to show";
                }
            }
            else if(id == "_self")
            {
                requestLists = nom.Where(x => x.StaffId == _helpersController.getSessionUserID()).ToList();

                if (requestLists.Any())
                {
                    ViewData["NominationName"] = "All Nomination request for " + requestLists.FirstOrDefault().StaffName.ToString();
                }
                else
                {
                    ViewData["NominationName"] = "No record to show";
                }
            }
            else
            {
                ViewData["NominationName"] = "Showing all request";
                requestLists = nom.ToList();
            }

            return View(requestLists.ToList());
        }



        public IActionResult NominatedStaff()
        {
            var get = from n in _context.NominatedStaff.AsEnumerable()
                      join a in _context.RequestProposal.AsEnumerable() on n.RequestId equals a.RequestId
                      join d in _context.RequestDuration.AsEnumerable() on a.DurationId equals d.DurationId
                      join c in _context.Companies.AsEnumerable() on a.CompanyId equals c.CompanyId
                      join st in _context.Staff.AsEnumerable() on n.StaffId equals st.StaffId
                      where n.StaffId == _helpersController.getSessionUserID()
                      select new NominatedList
                      {
                          AppRef = a.RequestRefNo,
                          CreatedAt = n.CreatedAt,
                          hasSubmitted = n.HasSubmitted,
                          isActive = n.IsActive,
                          CompanyName = c.CompanyName,
                          StaffName = st.LastName + " " + st.FirstName,
                          RequestId = n.RequestId,
                          NominationID = n.NominateId,
                          Duration = d.ProposalYear,
                          PermitId = _context.Permits.Where(x => x.RequestId == a.RequestId).FirstOrDefault()?.PermitId
                      };

            return View(get.ToList());
        }




        public IActionResult AddReport(string id)
        {
            int NormID = 0;

            var norm_id = generalClass.Decrypt(id.Trim());

            if (norm_id == "Error")
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, No link was found for this application history.") });
            }
            else
            {
                NormID = Convert.ToInt32(norm_id);

                List<PresentDocuments> presentDocuments = new List<PresentDocuments>();
                List<MissingDocument> missingDocuments = new List<MissingDocument>();
                List<BothDocuments> bothDocuments = new List<BothDocuments>();
                List<NominationReport> nominationReports = new List<NominationReport>();

                var apps = from n in _context.NominatedStaff.AsEnumerable()
                           join r in _context.RequestProposal.AsEnumerable() on n.RequestId equals r.RequestId
                           join du in _context.RequestDuration.AsEnumerable() on r.DurationId equals du.DurationId
                           join c in _context.Companies.AsEnumerable() on r.CompanyId equals c.CompanyId
                           where ((n.NominateId == NormID))
                           select new RequestApplication
                           {
                               RefNo = r.RequestRefNo,
                               RequestId = r.RequestId,
                               CompanyName = c.CompanyName,
                               CompanyAddress = c.Address,
                               CompanyEmail = c.CompanyEmail,
                               CompanyElpsID = c.CompanyElpsId,
                               Year = du.ProposalYear,
                               Status = r.Status,
                               DateApplied = r.DateApplied,
                               NominationID = n.NominateId
                           };

                if (apps.Any())
                {

                    var getDocuments = _context.ApplicationDocuments.Where(x => x.DocType == "Company" && x.DocName.Trim().Contains(GeneralClass.submit_exercise_company_doc) && x.DeletedStatus == false);

                    if (getDocuments.Any())
                    {
                        ViewData["CompanyElpsID"] = apps.FirstOrDefault().CompanyElpsID;
                        ViewData["RequestId"] = apps.FirstOrDefault().RequestId;
                        ViewData["AppRefNo"] = apps.FirstOrDefault().RefNo;
                        ViewData["AppStatus"] = apps.FirstOrDefault().Status;
                        ViewData["NominationID"] = apps.FirstOrDefault().NominationID;

                        List<LpgLicense.Models.Document> CompanyDoc = generalClass.getCompanyDocuments(apps.FirstOrDefault().CompanyElpsID.ToString());

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


                            nominationReports.Add(new NominationReport
                            {
                                requestApplications = apps.ToList(),
                                bothDocuments = bothDocuments.ToList()
                            });

                            _helpersController.LogMessages("Loading information and document for nominated staff report upload : " + apps.FirstOrDefault().RefNo, _helpersController.getSessionEmail());

                            _helpersController.LogMessages("Displaying/Viewing more application details.  Reference : " + apps.FirstOrDefault().RefNo, _helpersController.getSessionEmail());
                        }

                        return View(nominationReports.ToList());
                    }
                    else
                    {
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong trying fetch documents, please try again later.") });
                    }
                }
                else
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, Application was not found, please try again later.") });
                }
            }
        }



        public IActionResult EditNominationReport(string id)
        {
            int NormID = 0;

            var norm_id = generalClass.Decrypt(id.Trim());

            if (norm_id == "Error")
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, No link was found for this application history.") });
            }
            else
            {
                NormID = Convert.ToInt32(norm_id);

                List<PresentDocuments> presentDocuments = new List<PresentDocuments>();
                List<MissingDocument> missingDocuments = new List<MissingDocument>();
                List<BothDocuments> bothDocuments = new List<BothDocuments>();
                List<NominationReport> nominationReports = new List<NominationReport>();

                var apps = from n in _context.NominatedStaff.AsEnumerable()
                           join r in _context.RequestProposal.AsEnumerable() on n.RequestId equals r.RequestId
                           join du in _context.RequestDuration.AsEnumerable() on r.DurationId equals du.DurationId
                           join c in _context.Companies.AsEnumerable() on r.CompanyId equals c.CompanyId
                           where ((n.NominateId == NormID))
                           select new RequestApplication
                           {
                               RefNo = r.RequestRefNo,
                               RequestId = r.RequestId,
                               CompanyName = c.CompanyName,
                               CompanyAddress = c.Address,
                               CompanyEmail = c.CompanyEmail,
                               CompanyElpsID = c.CompanyElpsId,
                               Year = du.ProposalYear,
                               Status = r.Status,
                               DateApplied = r.DateApplied,
                               Titile = n.Title,
                               Content = n.Contents,
                               NominationID = n.NominateId
                           };

                if (apps.Any())
                {

                    var getDocuments = _context.ApplicationDocuments.Where(x => x.DocType == "Company" && x.DocName.Trim().Contains(GeneralClass.submit_exercise_company_doc) && x.DeletedStatus == false);

                    if (getDocuments.Any())
                    {
                        ViewData["CompanyElpsID"] = apps.FirstOrDefault().CompanyElpsID;
                        ViewData["RequestId"] = apps.FirstOrDefault().RequestId;
                        ViewData["AppRefNo"] = apps.FirstOrDefault().RefNo;
                        ViewData["AppStatus"] = apps.FirstOrDefault().Status;
                        ViewData["NominationID"] = apps.FirstOrDefault().NominationID;

                        List<LpgLicense.Models.Document> CompanyDoc = generalClass.getCompanyDocuments(apps.FirstOrDefault().CompanyElpsID.ToString());

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


                            nominationReports.Add(new NominationReport
                            {
                                requestApplications = apps.ToList(),
                                bothDocuments = bothDocuments.ToList()
                            });

                            _helpersController.LogMessages("Loading information and document for nominated staff report upload : " + apps.FirstOrDefault().RefNo, _helpersController.getSessionEmail());

                            _helpersController.LogMessages("Displaying/Viewing more application details.  Reference : " + apps.FirstOrDefault().RefNo, _helpersController.getSessionEmail());
                        }

                        return View(nominationReports.ToList());
                    }
                    else
                    {
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong trying fetch documents, please try again later.") });
                    }
                }
                else
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, Application was not found, please try again later.") });
                }
            }
        }



        /*
        * Saving nominated staff application report
        *
        * AppID => encrypted application id
        * txtReport => the comment for the report
        */
        public JsonResult SaveNominationReport(string NormID, string txtReport, string txtReportTitle, List<SubmitDoc> SubmittedDocuments)
        {
            string result = "";

            int norm_id = 0;

            var normid = generalClass.Decrypt(NormID.ToString().Trim());

            if (normid == "Error")
            {
                result = "Application link error";
            }
            else
            {
                norm_id = Convert.ToInt32(normid);

                var getNorm = _context.NominatedStaff.Where(x => x.NominateId == norm_id).ToList();

                if (getNorm.Any())
                {
                    int appid = getNorm.FirstOrDefault().RequestId;

                    getNorm.FirstOrDefault().Title = txtReportTitle.ToUpper();
                    getNorm.FirstOrDefault().ElpsDocId = SubmittedDocuments.FirstOrDefault()?.CompElpsDocID;
                    getNorm.FirstOrDefault().DocSource = SubmittedDocuments.FirstOrDefault()?.DocSource;
                    getNorm.FirstOrDefault().AppDocId = SubmittedDocuments.FirstOrDefault()?.LocalDocID;
                    getNorm.FirstOrDefault().Contents = txtReport;
                    getNorm.FirstOrDefault().SubmittedAt = DateTime.Now;
                    getNorm.FirstOrDefault().UpdatedAt = DateTime.Now;
                    getNorm.FirstOrDefault().HasSubmitted = true;

                    if (_context.SaveChanges() > 0)
                    {
                        var apps = _context.RequestProposal.Where(x => x.RequestId == appid);

                        result = "Report Saved";

                        var actionFrom = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());
                        var actionTo = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());

                        _helpersController.LogMessages("Saving report for application : " + apps.FirstOrDefault().RequestRefNo, _helpersController.getSessionEmail());
                        _helpersController.SaveHistory(apps.FirstOrDefault().RequestId, actionFrom, actionTo, "Witness Report", "A report has been added for the witnessing of WELL Test.");

                    }
                    else
                    {
                        result = "Something went wrong trying to save your report";
                    }
                }
            }
            _helpersController.LogMessages("Operation result from saving report : " + result, _helpersController.getSessionEmail());
            return Json(result);
        }




        public JsonResult EditNominationReports(string NormID, string txtReport, string txtReportTitle, List<SubmitDoc> SubmittedDocuments)
        {
            string result = "";

            int rID = 0;

            var id = generalClass.Decrypt(NormID.ToString().Trim());

            if (id == "Error")
            {
                result = "Application link error";
            }
            else
            {
                rID = Convert.ToInt32(id);

                var get = _context.NominatedStaff.Where(x => x.NominateId == rID);

                if (get.Any())
                {
                    int appid = get.FirstOrDefault().RequestId;

                    get.FirstOrDefault().Contents = txtReport;
                    get.FirstOrDefault().UpdatedAt = DateTime.Now;
                    get.FirstOrDefault().Title = txtReportTitle.ToUpper();

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
                        var apps = _context.RequestProposal.Where(x => x.RequestId == appid);

                        result = "Report Edited";

                        var actionFrom = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());
                        var actionTo = _helpersController.getActionHistory(_helpersController.getSessionRoleID(), _helpersController.getSessionUserID());

                        _helpersController.LogMessages("Saving witnessing report for application : " + apps.FirstOrDefault().RequestRefNo, _helpersController.getSessionEmail());
                        _helpersController.SaveHistory(appid, actionFrom, actionTo, "Edit Report", "A nomination witnesss report has been updated to this application.");
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

            _helpersController.LogMessages("Report update status :" + result + " Report ID : " + rID, _helpersController.getSessionEmail());

            return Json(result);
        }



        public IActionResult ViewNominationReport(string id)
        {
            var norm_id = generalClass.DecryptIDs(id.Trim());

            if (norm_id == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, No link was found for this application history.") });
            }
            else
            {
                var report = from re in _context.NominatedStaff.AsEnumerable()
                             join r in _context.RequestProposal.AsEnumerable() on re.RequestId equals r.RequestId
                             join d in _context.RequestDuration.AsEnumerable() on r.DurationId equals d.DurationId
                             join st in _context.Staff.AsEnumerable() on re.StaffId equals st.StaffId
                             join c in _context.Companies.AsEnumerable() on r.CompanyId equals c.CompanyId
                             where (re.NominateId == norm_id)
                             select new ReportViewModel
                             {
                                 RefNo = r.RequestRefNo,
                                 CompanyName = c.CompanyName,
                                 CompanyAddress = c.Address,
                                 CompanyEmail = c.CompanyEmail,
                                 Status = r.Status,
                                 DateApplied = r.CreatedAt,
                                 Staff = st.LastName + " " + st.FirstName + " (" + st.StaffEmail + ")",
                                 ReportDate = re.CreatedAt,
                                 UpdatedAt = re.UpdatedAt,
                                 Subject = re.Title,
                                 Comment = re.Contents,
                                 DocSource = re.DocSource,

                             };

                ViewData["AppRef"] = report.FirstOrDefault()?.RefNo;

                _helpersController.LogMessages("Displaying application nomination report for Application reference : " + report.FirstOrDefault().RefNo, _helpersController.getSessionEmail());

                return View(report.ToList());
            }
        }




        public JsonResult GetNominationRequest()
        {
            var nom = _context.NominationRequest.Where(x => x.StaffId == _helpersController.getSessionUserID() && x.HasDone == false);
            return Json(nom.Count());
        }


        public JsonResult MyNominationCount()
        {
            var count = _context.NominatedStaff.Where(x => x.StaffId == _helpersController.getSessionUserID() && x.HasSubmitted == false);
            return Json(count.Count());
        }

    }
}
