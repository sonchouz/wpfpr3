using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfpr3.Models;

namespace wpfpr3.Service
{
    public class CandidateValidator
    {
        public List<ValidationResult> Validate(Candidate cand)
        {
            var context = new ValidationContext(cand);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(cand, context, results, true);
            return results;
        }
    }
}
