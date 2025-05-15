using System;
using System.Collections.Generic;

namespace LabAssignment6.DataAccess;

public partial class Student
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Academicrecord> Academicrecords { get; set; } = new List<Academicrecord>();

    public virtual ICollection<Course> CourseCourses { get; set; } = new List<Course>();
}
