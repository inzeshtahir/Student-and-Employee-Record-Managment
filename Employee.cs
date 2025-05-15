using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LabAssignment6.DataAccess;

public partial class Employee
{
    public int Id { get; set; }

    [Display(Name = "Employee Name")]
    [Required(ErrorMessage = "Employee Name is required.")]
    [RegularExpression(@"^[A-Za-z]+(?:['-][A-Za-z]+)? [A-Za-z]+(?:['-][A-Za-z]+)?$",
    ErrorMessage = "Must be in the form of first name followed by last name.")]
    public string Name { get; set; } = null!;

    [Display(Name = "Network ID")]
    [Required(ErrorMessage = "Network ID is required.")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "Username length should be more than 3 characters.")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(30, MinimumLength = 5, ErrorMessage = "Password length should be more than 5 characters.")]
    public string Password { get; set; } = null!;

    [Display(Name = "Job Title(s)")]
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
