namespace wpfpr3.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class User
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        public int roleID { get; set; }

        [Required(ErrorMessage = "Firstname is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Firstname must be 2-50 characters.")]
        [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ\- ]+$", ErrorMessage = "Firstname contains invalid characters.")]
        public string firstname { get; set; }

        [Required(ErrorMessage = "Surname is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Surname must be 2-50 characters.")]
        [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ\- ]+$", ErrorMessage = "Surname contains invalid characters.")]
        public string surname { get; set; }

        [Required(ErrorMessage = "Birthday is required.")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(User), nameof(ValidateBirthday))]
        public DateTime birthday { get; set; }

        [Required(ErrorMessage = "Phone is required.")]
        [Phone(ErrorMessage = "Invalid phone format.")]
        [StringLength(20)]
        public string phone { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(100)]
        public string email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(255, ErrorMessage = "Password is too long.")]
        public string hashpass { get; set; }

        public int? networkID { get; set; }

        public virtual Candidate Candidate { get; set; }
        public virtual Employer Employer { get; set; }
        public virtual Network Network { get; set; }
        public virtual Role Role { get; set; }

        // Проверка возраста
        public static ValidationResult ValidateBirthday(DateTime birthday, ValidationContext context)
        {
            if (birthday > DateTime.Now)
                return new ValidationResult("Birthday cannot be in the future.");

            int age = DateTime.Now.Year - birthday.Year;
            if (birthday > DateTime.Now.AddYears(-age)) age--;

            if (age < 14)
                return new ValidationResult("User must be at least 14 years old.");

            return ValidationResult.Success;
        }
    }
}
