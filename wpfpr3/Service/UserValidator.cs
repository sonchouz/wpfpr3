using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfpr3.Models;

namespace wpfpr3.Service
{
    public class UserValidator
    {
        //валидация юзера
        public List<ValidationResult> Validate(User user)
        {
            var context = new ValidationContext(user);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(user, context, results, true);
            return results;
        }
    }
}
