using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Submission
    {
        public DateTime Time { get; set; }
        public uint Score { get; set; }
        public string Contents { get; set; } = null!;
        public string UId { get; set; } = null!;
        public uint AId { get; set; }
        public uint SId { get; set; }

        public virtual Assignment AIdNavigation { get; set; } = null!;
        public virtual Student UIdNavigation { get; set; } = null!;
    }
}
