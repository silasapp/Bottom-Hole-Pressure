using BHP.HelpersClass;
using BHP.Models.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BHP.Helpers
{
    public class RequestReminderService : BackgroundService, IDisposable
    {
        GeneralClass generalClass = new GeneralClass();
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        HelpersController _helpersController;
        private readonly ILogger<RequestReminderService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;


        public RequestReminderService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ILogger<RequestReminderService> logger, IServiceScopeFactory scopeFactory)
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

                _logger.LogDebug("Request Reminder Service is running in background");

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
                            string subject = "REMINDER FOR " + checkDate.FirstOrDefault().ProposalYear + " BOTTOM-HOLE PRESSURE (BHP) SURVEY PROGRAMME";
                            string content = "As a reminder in accordance with Section 38 of the Petroleum (Drilling and Production) Regulations, 1969 as amended, you are requested to submit your " + checkDate.FirstOrDefault().ProposalYear + " proposed subsurface pressure surveys programme to the Director, Petroleum Resources. ";

                            if (today >= checkDate.FirstOrDefault().RequestStartDate && today <= checkDate.FirstOrDefault().RequestEndDate)
                            {
                                foreach (var c in getCompany.ToList())
                                {
                                    var checkRequest = _context.RequestProposal.Where(x => x.CompanyId == c.CompanyId && x.DurationId == checkDate.FirstOrDefault().DurationId && x.IsSubmitted == false);

                                    if (checkRequest.Any())
                                    {

                                        var getReminder = _context.RequestReminders.Where(x => x.RequestId == checkRequest.FirstOrDefault().RequestId);

                                        if(getReminder.Any())
                                        {
                                            var lastDate = getReminder.LastOrDefault().SentAt.Date.AddDays(7);

                                            if(lastDate.ToShortDateString() == DateTime.Now.Date.ToShortDateString())
                                            {
                                                var msg = await _helpersController.SendEmailMessageAsync(c.CompanyEmail, c.CompanyName, subject, content, GeneralClass.PROPOSAL_REQUEST);

                                                if (msg == "OK")
                                                {
                                                    sent = true;
                                                }

                                                RequestReminders requestReminders = new RequestReminders()
                                                {
                                                    RequestId = checkRequest.FirstOrDefault().RequestId,
                                                    Subject = subject,
                                                    Content = content,
                                                    SentAt = DateTime.Now.Date,
                                                    IsSent = sent,
                                                };

                                                _context.RequestReminders.Add(requestReminders);

                                                if (_context.SaveChanges() > 0)
                                                {
                                                    _helpersController.SaveMessage(checkRequest.FirstOrDefault().RequestId, c.CompanyId, subject, content);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var msg = await _helpersController.SendEmailMessageAsync(c.CompanyEmail, c.CompanyName, subject, content, GeneralClass.PROPOSAL_REQUEST);

                                            if (msg == "OK")
                                            {
                                                sent = true;
                                            }

                                            RequestReminders requestReminders = new RequestReminders()
                                            {
                                                RequestId = checkRequest.FirstOrDefault().RequestId,
                                                Subject = subject,
                                                Content = content,
                                                SentAt = DateTime.Now.Date,
                                                IsSent = sent,
                                            };

                                            _context.RequestReminders.Add(requestReminders);

                                            if (_context.SaveChanges() > 0)
                                            {
                                                _helpersController.SaveMessage(checkRequest.FirstOrDefault().RequestId, c.CompanyId, subject, content);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
                    }
                }

                _logger.LogDebug("Request Reminder Service has stopped in background");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
