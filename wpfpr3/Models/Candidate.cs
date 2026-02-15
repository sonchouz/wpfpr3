namespace wpfpr3.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Candidate
    {
        public Candidate()
        {
            this.Applications = new HashSet<Application>();
        }

        public int id { get; set; }

        [Required(ErrorMessage = "Citizenship is required.")]
        [StringLength(50, MinimumLength = 2)]
        public string citizenship { get; set; }

        public int statusID { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [StringLength(100, MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ\- ]+$", ErrorMessage = "City contains invalid characters.")]
        public string livingcity { get; set; }

        [Required(ErrorMessage = "Education is required.")]
        public int educationID { get; set; }

        public int? langID { get; set; }
        public int? levelID { get; set; }

        public virtual ICollection<Application> Applications { get; set; }
        public virtual Education Education { get; set; }
        public virtual Language Language { get; set; }
        public virtual Level Level { get; set; }
        public virtual Status Status { get; set; }
        public virtual User User { get; set; }
    }
}
