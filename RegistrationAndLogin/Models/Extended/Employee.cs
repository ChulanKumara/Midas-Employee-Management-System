using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RegistrationAndLogin.Models
{
    [MetadataType(typeof(EmployeeMetaData))]
    public partial class Employee
    {}

    public class EmployeeMetaData
    {
        // FirstName
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide the first name")]
        public string FirstName { get; set; }

        // LastName
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide the last name")]
        public string LastName { get; set; }
    }
}