using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class SurveyReport
    {
        public int SurveyReportId { get; set; }
        public int RequestId { get; set; }
        public string Field { get; set; }
        public string Reservior { get; set; }
        public decimal RemainingReserve { get; set; }
        public string WellName { get; set; }
        public DateTime DateOfSurvey { get; set; }
        public string TypeOfSurvey { get; set; }
        public decimal Oil { get; set; }
        public decimal Water { get; set; }
        public decimal Gas { get; set; }
        public decimal Bsw { get; set; }
        public decimal Gor { get; set; }
        public decimal Bean { get; set; }
        public decimal DatumDepth { get; set; }
        public decimal InitialPressure { get; set; }
        public decimal Thp { get; set; }
        public decimal Chp { get; set; }
        public decimal FbhpMp { get; set; }
        public decimal SbhpMp { get; set; }
        public decimal RpDatum { get; set; }
        public decimal ProductivityIndex { get; set; }
        public string Remark { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DateOfLastSurvey { get; set; }
    }
}
