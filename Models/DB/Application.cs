using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class Application
    {
        public int AppId { get; set; }
        public int RequestId { get; set; }
        public string Qrt { get; set; }
        public string Fields { get; set; }
        public string Reservoir { get; set; }
        public decimal InitialRpressure { get; set; }
        public decimal RbubblePointPressure { get; set; }
        public string WellName { get; set; }
        public DateTime LastSurveyDate { get; set; }
        public decimal MeasuredRpressure { get; set; }
        public DateTime? SurveyProposedDate { get; set; }
        public string SurveyType { get; set; }
        public string TimeShut { get; set; }
        public string UsedInstrument { get; set; }
        public decimal OperaionWellCost { get; set; }
        public DateTime ReservoirCreatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool? DeletedStatus { get; set; }
        public int? DeletedBy { get; set; }
        public decimal? PreviousYearMeasuredRp { get; set; }
    }
}
