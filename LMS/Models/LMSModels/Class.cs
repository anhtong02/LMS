using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Class
    {
        public Class()
        {
            AssignmentCategories = new HashSet<AssignmentCategory>();
            Enrolleds = new HashSet<Enrolled>();
        }

        public uint SemYear { get; set; }
        public string SemSeason { get; set; } = null!;
        public string Loc { get; set; } = null!;
        public TimeOnly Start { get; set; }
        public TimeOnly End { get; set; }
        public uint CId { get; set; }
        public uint ClassId { get; set; }
        public string UId { get; set; } = null!;

        public virtual Course CIdNavigation { get; set; } = null!;
        public virtual Professor UIdNavigation { get; set; } = null!;
        public virtual ICollection<AssignmentCategory> AssignmentCategories { get; set; }
        public virtual ICollection<Enrolled> Enrolleds { get; set; }
    }
}
