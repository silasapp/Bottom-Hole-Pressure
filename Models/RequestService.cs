using BHP.Helpers;
using BHP.HelpersClass;
using BHP.Models.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BHP.Controllers.RequestProposal
{
    public class RequestService : BackgroundService, IDisposable
    {

       
        GeneralClass generalClass = new GeneralClass();
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        HelpersController _helpersController;
        private readonly ILogger<RequestService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;


        public RequestService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ILogger<RequestService> logger, IServiceScopeFactory scopeFactory)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var year = DateTime.Now.Year + 1;
                var today = DateTime.Now;
                bool sent = false;

                string subject = year + " BOTTOM-HOLE PRESSURE (BHP) SURVEY PROGRAMME";
                string content = "In accordance with Section 38 of the Petroleum (Drilling and Production) Regulations, 1969 as amended, you are requested to submit your " + year + " proposed subsurface pressure surveys programme to the Director, Petroleum Resources. ";

                _logger.LogDebug("Request Service is running in background");

                while (!stoppingToken.IsCancellationRequested)
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var _context = scope.ServiceProvider.GetRequiredService<BHP_DBContext>();

                        _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);

                        var getCompany = _context.Companies.Where(x => x.DeleteStatus == false && x.ActiveStatus == true);

                        var checkDate = _context.RequestDuration.Where(x => x.ProposalYear == year && x.DeleteStatus == false);

                        if (checkDate.Any())
                        {
                            if (today >= checkDate.FirstOrDefault().RequestStartDate && today <= checkDate.FirstOrDefault().RequestEndDate)
                            {
                                foreach (var c in getCompany.ToList())
                                {
                                    var checkRequest = _context.RequestProposal.Where(x => x.CompanyId == c.CompanyId && x.DurationId == checkDate.FirstOrDefault().DurationId);

                                    if (checkRequest.Count() <= 0)
                                    {
                                        var msg = await _helpersController.SendEmailMessageAsync(c.CompanyEmail, c.CompanyName, subject, content, GeneralClass.PROPOSAL_REQUEST);

                                        if (msg == "OK")
                                        {
                                            sent = true;
                                        }

                                        var seq = _context.RequestProposal.Count();

                                        Models.DB.RequestProposal requestProposal = new Models.DB.RequestProposal()
                                        {
                                            CompanyId = c.CompanyId,
                                            DurationId = checkDate.FirstOrDefault().DurationId,
                                            RequestRefNo = generalClass.Generate_Application_Number(),
                                            EmailSent = sent,
                                            Comment = subject,
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

                                        if (_context.SaveChanges() > 0)
                                        {
                                            var user = _context.Companies.Where(x => x.CompanyId == c.CompanyId).FirstOrDefault();

                                            var actionFrom = "NUPRC UMR Division";
                                            var actionTo = user.CompanyName;

                                            _helpersController.SaveHistory(requestProposal.RequestId, actionFrom, actionTo, "Moved", "Application completed & submitted; landed on staff desk");

                                            _helpersController.SaveMessage(requestProposal.RequestId, c.CompanyId, subject, content);
                                        }
                                    }
                                }
                            }
                        }

                        await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
                    }
                }

                _logger.LogDebug("Request Service has stopped in background");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
