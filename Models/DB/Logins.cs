using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class Logins
    {
        public int LoginId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string HostName { get; set; }
        public string MacAddress { get; set; }
        public string LocalIp { get; set; }
        public string RemoteIp { get; set; }
        public string UserAgent { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string LoginStatus { get; set; }
    }
}
