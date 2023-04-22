using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BHP.Models.DB;
using BHP.Models;
using BHP.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using BHP.HelpersClass;
using Rotativa.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography.X509Certificates;

namespace BHP.Controllers.RequestProposal
{
    [Authorize]
    public class RequestProposalsController : Controller
    {
        private readonly BHP_DBContext _context;
        GeneralClass generalClass = new GeneralClass();
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        HelpersController _helpersController;


        public RequestProposalsController(BHP_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);

        }



        public List<RequestModal> Proposals(string id)
        {
            var req = (from c in _context.Companies
                       join r in _context.RequestProposal on c.CompanyId equals r.CompanyId into request
                       from rc in request.DefaultIfEmpty()
                       join du in _context.RequestDuration on rc.DurationId equals du.DurationId into duration
                       from d in duration.DefaultIfEmpty()
                       select new RequestModal
                       {
                           RequestId = rc == null ? "" : rc.RequestId.ToString(),
                           CompanyId = c.CompanyId,
                           RequestRefNo = rc == null ? "" : rc.RequestRefNo,
                           CompanyName = c.CompanyName,
                           CompanyAddress = c.Address + ", " + c.City + ", " + c.StateName,
                           CompanyEmail = c.CompanyEmail,
                           ProposalYear = d == null ? "" : d.ProposalYear.ToString(),
                           StartDate = d == null ? "" : d.RequestStartDate.ToString(),
                           EndDate = d == null ? "" : d.RequestEndDate.ToString(),
                           EmailSent = rc == null ? "" : rc.EmailSent.ToString(),
                           CreatedAt = rc == null ? "" : rc.CreatedAt.ToString(),
                           UpdatedAt = rc == null ? "" : rc.UpdatedAt == null ? "" : rc.UpdatedAt.ToString(),
                           Acknowledge = rc == null ? "" : rc.HasAcknowledge == null ? "" : rc.HasAcknowledge.ToString(),
                           DeletedAt = rc == null ? "" : rc.DeletedAt == null ? "" : rc.DeletedAt.ToString(),
                           DeletedStatus = c.DeleteStatus,
                           AcknowledgeAt = rc == null ? "" : rc.AcknowledgeAt == null ? "" : rc.AcknowledgeAt.ToString(),
                           ActiveStatus = c.ActiveStatus,
                           Year = d == null ? 0 : d.ProposalYear,
                           GeneratedRef = rc == null ? "" : rc.GeneratedNumber
                       });

            if (id == null) // fetching for a specific year
            {
                req = req.Where(x => x.DeletedStatus == false && x.ActiveStatus == true && x.Year == (DateTime.Now.Year + 1));
            }
            else // getting all records
            {
                req = req.Where(x => x.DeletedStatus == false && x.ActiveStatus == true);
            }
            return req.ToList();
        }




        public IActionResult Index(string id)
        {
            ViewData["ProposalYear"] = "";

            var getDuration = _context.RequestDuration.Where(x => x.ProposalYear == (DateTime.Now.Year + 1) && x.DeleteStatus == false);

            ViewData["DurationDetails"] = "";
            ViewData["DurationID"] = "";

            if (getDuration.Any())
            {
                ViewData["DurationID"] = generalClass.Encrypt(getDuration.FirstOrDefault().DurationId.ToString());
                ViewData["DurationDetails"] = "From " + getDuration.FirstOrDefault().RequestStartDate + " To " + getDuration.FirstOrDefault().RequestEndDate;
            }

            var req = Proposals(id);

            if(id == null)
            {
                ViewData["ProposalYear"] = "Proposal Request for " + (DateTime.Now.Year + 1) + " BHP Program.";
            }
            else
            {
                ViewData["ProposalYear"] = "All Proposal Request.";
            }

            _helpersController.LogMessages("Getting results : " + ViewData["ProposalYear"].ToString(),  _helpersController.getSessionEmail());
            return View(req.ToList());
        }




        public IActionResult RequestDuration()
        {
            var duration = _context.RequestDuration;
            _helpersController.LogMessages("Displaying all result for proposal request duration.",  _helpersController.getSessionEmail());

            _helpersController.LogMessages("Getting all request duration",  _helpersController.getSessionEmail());

            return View(duration.ToList());
        }




        public IActionResult CreateDuration(string DurationId, string DurationEndDate, string DurationStartDate, int ProposalYear, string ReportEndDate, string Options, string ContactPerson)
        {
            var result = "";

            var startDate = DateTime.Parse(DurationStartDate.Trim());
            var endDate = DateTime.Parse(DurationEndDate.Trim());
            var reportEndDate = DateTime.Parse(ReportEndDate.Trim());

            if (Options == "Edit")
            {
                var durationID = generalClass.DecryptIDs(DurationId);

                if (durationID == 0)
                {
                    result = "Something went wrong. Duration reference error.";
                }
                else
                {
                    var checkDuration = _context.RequestDuration.Where(x => x.DurationId == durationID && x.ProposalYear == ProposalYear && x.DeleteStatus == false); ;

                    if (checkDuration.Any())
                    {
                        checkDuration.FirstOrDefault().RequestStartDate = startDate;
                        checkDuration.FirstOrDefault().RequestEndDate = endDate;
                        checkDuration.FirstOrDefault().ReportEndDate = reportEndDate;
                        checkDuration.FirstOrDefault().ContactPerson = ContactPerson;
                        checkDuration.FirstOrDefault().UpdatedAt = DateTime.Now;

                        if (_context.SaveChanges() > 0)
                        {
                            result = "Edited";
                        }
                        else
                        {
                            result = "Something went wrong trying to update this entry. Please try again later.";
                        }
                    }
                    else
                    {
                        result = "Something went wrong, cannot find this duration to update";
                    }
                }
            }
            else if (Options == "Create")
            {
                var checkDuration = _context.RequestDuration.Where(x => x.ProposalYear == ProposalYear && x.DeleteStatus == false); ;

                if (checkDuration.Any())
                {
                    result = "This proposal duration has already been created.";
                }
                else
                {
                    RequestDuration duration = new RequestDuration()
                    {
                        RequestStartDate = startDate,
                        RequestEndDate = endDate,
                        ProposalYear = ProposalYear,
                        ReportEndDate = reportEndDate,
                        CreatedAt = DateTime.Now,
                        DeleteStatus = false,
                        ContactPerson = ContactPerson
                    };

                    _context.RequestDuration.Add(duration);

                    if (_context.SaveChanges() > 0)
                    {
                        result = "Created";
                    }
                    else
                    {
                        result = "Something went wrong trying to create this request duration. Please try again later.";
                    }
                }
            }
            else
            {
                result = "No Options was found for this entry.";
            }

            _helpersController.LogMessages("Results from creating Duration : " + result,  _helpersController.getSessionEmail());

            return Json(result);
        }



        public IActionResult DeleteDuration(string DurationId)
        {
            var result = "";
            var staffID = _helpersController.getSessionUserID();

            var durationID = generalClass.DecryptIDs(DurationId.ToString());

            if (durationID == 0)
            {
                result = "Something went wrong. Duration reference not in correct format";
            }
            else
            {
                var checkRequest = _context.RequestProposal.Where(x => x.DurationId == durationID && x.DeletedStatus == false);

                if (checkRequest.Any())
                {
                    result = "You cannot delete this proposal duration since it is in use.";
                }
                else
                {
                    var getDuration = _context.RequestDuration.Where(x => x.DurationId == durationID);

                    if (getDuration.Any())
                    {
                        getDuration.FirstOrDefault().DeleteStatus = true;
                        getDuration.FirstOrDefault().DeletedAt = DateTime.Now;
                        getDuration.FirstOrDefault().DeletedBy = staffID;

                        if (_context.SaveChanges() > 0)
                        {
                            result = "Deleted";
                        }
                        else
                        {
                            result = "Something went wrong trying to delete this proposal duration. Please try again later.";
                        }
                    }
                    else
                    {
                        result = "Something went wrong. Cannot find this duration reference";
                    }
                }
            }

            _helpersController.LogMessages("Results from deleting Duration : " + result,  _helpersController.getSessionEmail());


            return Json(result);
        }



        [HttpPost]
        public async Task<JsonResult> CreateRequestAsync(string DurationID, string txtComment)
        {
            int done = 0;
            var result = "";
            var year = DateTime.Now.Year + 1;
            bool sent = false;

            string subject = year + " BOTTOM-HOLE PRESSURE (BHP) SURVEY PROGRAMME";
            string content = "In accordance with Section 38 of the Petroleum (Drilling and Production) Regulations, 1969 as amended, you are requested to submit your " + year + " proposed subsurface pressure surveys programme to the Director, Petroleum Resources. ";

            var durationID = generalClass.DecryptIDs(DurationID.ToString());

            if (durationID == 0)
            {
                result = "Something went wrong. Duration reference not in correct format";
            }
            else
            {
                var getCompany = _context.Companies.Where(x => x.DeleteStatus == false && x.ActiveStatus == true);

                if (getCompany.Any()) {

                    foreach (var c in getCompany.ToList())
                    {
                        var checkRequest = _context.RequestProposal.Where(x => x.CompanyId == c.CompanyId && x.DurationId == durationID);

                        if (checkRequest.Count() <= 0)
                        {
                            var msg = await _helpersController.SendEmailMessageAsync(c.CompanyEmail, c.CompanyName, subject, content, GeneralClass.PROPOSAL_REQUEST);

                            if (msg == "OK")
                            {
                                sent = true;
                            }

                            var seq = _context.RequestProposal.Count();

                            BHP.Models.DB.RequestProposal requestProposal = new Models.DB.RequestProposal()
                            {
                                CompanyId = c.CompanyId,
                                DurationId = durationID,
                                RequestRefNo = generalClass.Generate_Application_Number(),
                                EmailSent = sent,
                                Comment = txtComment,
                                CreatedAt = DateTime.Now,
                                DeletedStatus = false,
                                HasAcknowledge = false,
                                Status = GeneralClass.ApplicaionRequired,
                                IsSubmitted = false,
                                RequestSequence = (seq + 1),
                                GeneratedNumber = _helpersController.GenerateReferenceNumber(),
                                IsProposalApproved = false,
                                IsReportApproved = false,
                                IsReportSubmitted = false

                            };

                            _context.RequestProposal.Add(requestProposal);
                            done += _context.SaveChanges();
                            _helpersController.SaveMessage(requestProposal.RequestId, getCompany.FirstOrDefault().CompanyId, subject, content);
                        }
                    }

                    if (done > 0)
                    {
                      
                        result = "Sent";
                    }
                    else
                    {
                        result = "Some proposal request must have been sent, please check the list of request sent.";
                    }
                }
                else
                {
                    result = "No company records";
                }
            }

            _helpersController.LogMessages("Results from creating proposal request : " + result,  _helpersController.getSessionEmail());

            return Json(result);
        }



        [HttpPost]
        public async Task<JsonResult> ResendEmailAsync(string RequestId)
        {
            try
            {
                var result = "";
                var year = DateTime.Now.Year + 1;
                bool sent = false;

                string subject = "RESENT " + year + " BOTTOM-HOLE PRESSURE (BHP) SURVEY PROGRAMME";
                string content = "In accordance with Section 38 of the Petroleum (Drilling and Production) Regulations, 1969 as amended, you are requested to submit your " + year + " proposed subsurface pressure surveys programme to the Director, Petroleum Resources. ";

                var requestid = generalClass.DecryptIDs(RequestId.ToString());

                if (requestid == 0)
                {
                    result = "Something went wrong. Duration reference not in correct format";
                }
                else
                {
                    var checkRequest = _context.RequestProposal.Where(x => x.RequestId == requestid && x.EmailSent == false);
                    var getCompany = _context.Companies.Where(x => x.CompanyId == checkRequest.FirstOrDefault().CompanyId && x.ActiveStatus == true && x.DeleteStatus == false);

                    if (checkRequest.Any() && getCompany.Any())
                    {
                        var com_id = checkRequest.FirstOrDefault().CompanyId;

                        var msg = await _helpersController.SendEmailMessageAsync(getCompany.FirstOrDefault().CompanyEmail, getCompany.FirstOrDefault().CompanyName, subject, content, GeneralClass.PROPOSAL_REQUEST);

                        if (msg == "OK")
                        {
                            sent = true;
                        }

                        checkRequest.FirstOrDefault().EmailSent = sent;
                        checkRequest.FirstOrDefault().UpdatedAt = DateTime.Now;
                        checkRequest.FirstOrDefault().HasAcknowledge = false;
                        checkRequest.FirstOrDefault().IsSubmitted = false;
                        checkRequest.FirstOrDefault().Status = GeneralClass.ApplicaionRequired;

                        if (_context.SaveChanges() > 0)
                        {
                            _helpersController.SaveMessage(requestid, com_id, subject, content);
                            result = "Sent";
                        }
                        else
                        {
                            result = "Something went wrong trying to resend this request, please try again later.";
                        }
                    }
                }

                _helpersController.LogMessages("Results from resending proposal request : " + result, _helpersController.getSessionEmail());

                return Json(result);
            }
            catch(Exception ex)
            {
                return Json("An Error occured. See error message - " + ex.Message);
            }
        }




        public IActionResult Remiders(string id)
        {
            if (id == null)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, request proposal reference is empty. Please try agin later") });
            }
            else
            {
                var requestid = generalClass.DecryptIDs(id);

                if (requestid == 0)
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, request proposal reference error. Please try agin later") });
                }
                else
                {
                    ViewData["RemiderTitle"] = "";

                    var rem = from rm in _context.RequestReminders
                              join r in _context.RequestProposal on rm.RequestId equals r.RequestId
                              join d in _context.RequestDuration on r.DurationId equals d.DurationId
                              where  rm.RequestId == requestid
                              select new ReminderModel
                              {
                                  RefNo = r.RequestRefNo,
                                  Year = d.ProposalYear,
                                  Subject = rm.Subject,
                                  Content = rm.Content,
                                  SentAt = rm.SentAt,
                                  IsSent = rm.IsSent
                              };

                    if(rem.Any())
                    {
                        ViewData["RemiderTitle"] = "Sent Request Reminders for Application " + rem.FirstOrDefault().RefNo;
                    }
                    else
                    {
                        ViewData["RemiderTitle"] = "Sent Request Reminders for Application";

                    }

                    return View(rem.ToList());
                }
            }
        }






        public IActionResult ViewLetter(string id, string option)       
        {
            List<ProposalRequestLetter> requestLetters = new List<ProposalRequestLetter>();

            if (id == null)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, request proposal reference is empty. Please try agin later") });
            }
            else
            {
                var requestid = generalClass.DecryptIDs(id);

                if (requestid == 0)
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, request proposal reference error. Please try agin later") });
                }
                else
                {
                    var req = from r in _context.RequestProposal
                              join d in _context.RequestDuration on r.DurationId equals d.DurationId
                              join c in _context.Companies on r.CompanyId equals c.CompanyId
                              where r.RequestId == requestid
                              select new ProposalRequestLetterModel
                              {
                                  CompanyName = c.CompanyName,
                                  CompanyAddress = c.Address,
                                  CompanyCity = c.City,
                                  CompanyState = c.StateName,
                                  Year = d.ProposalYear,
                                  StartDate = d.RequestStartDate,
                                  EndDate = d.RequestEndDate,
                                  ProposalRefNo = r.RequestRefNo,
                                  ReportEndDate = d.ReportEndDate,
                                  PreviousYear = (d.ProposalYear - 1),
                                  GeneratedNo = r.GeneratedNumber,
                                  ContactName = d.ContactPerson
                              };


                    ViewData["ProposalLetter"] = "";

                    if (req.Any())
                    {
                        ViewData["ProposalLetter"] = req.FirstOrDefault().Year + " Request Proposal letter for - " + req.FirstOrDefault().CompanyName + "(" + req.FirstOrDefault().ProposalRefNo + ")";

                        requestLetters.Add(new ProposalRequestLetter
                        {
                            letterModels = req.ToList(),
                        });

                       
                        if (option == "_download")
                        {
                            _helpersController.LogMessages("Downloading Request proposal letter for : " + req.FirstOrDefault().ProposalRefNo,  _helpersController.getSessionEmail());

                            return new ViewAsPdf("ViewLetter", requestLetters.ToList())
                            {
                                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                                FileName = req.FirstOrDefault().Year + " Request Proposal letter for - " + req.FirstOrDefault().CompanyName + "(" + req.FirstOrDefault().ProposalRefNo + ").pdf",
                            };
                        }
                        else
                        {
                            _helpersController.LogMessages("Displaying Request proposal letter for : " + req.FirstOrDefault().ProposalRefNo,  _helpersController.getSessionEmail());

                            return new ViewAsPdf("ViewLetter", requestLetters.ToList())
                            {
                                PageSize = Rotativa.AspNetCore.Options.Size.A4
                            };
                        }
                    }
                    else
                    {
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Opps!!! Something went wrong, you can not view this Letter. Kindly contact support.") });
                    }
                }
            }

        }

    }

   
}
