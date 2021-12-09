using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IkeMtz.Samples.Models.V1
{
 // Generated by the SQL POCO Class Generator Script
 // Script is available at:
 // https://raw.githubusercontent.com/ikemtz/NRSRx/master/tools/sql-poco-class-generator.sql

  public partial class Student
  : IkeMtz.NRSRx.Core.Models.IIdentifiable, IkeMtz.NRSRx.Core.Models.IAuditable
  {
    public Student()
    {
      StudentCourses = new HashSet<StudentCourse>();
      StudentSchools = new HashSet<StudentSchool>();
    }

    [Required]
    public Guid Id { get; set; }
    [MaxLength(50)]
    public string Title { get; set; }
    [Required]
    [MaxLength(250)]
    public string FirstName { get; set; }
    [Required]
    [MaxLength(250)]
    public string LastName { get; set; }
    [MaxLength(250)]
    public string MiddleName { get; set; }
    [Required]
    public DateTime BirthDate { get; set; }
    [Required]
    [MaxLength(250)]
    [EmailAddress]
    public string Email { get; set; }
    [MaxLength(15)]
    public string Tel1 { get; set; }
    [MaxLength(15)]
    public string Tel2 { get; set; }
    [Required]
    [MaxLength(250)]
    public string CreatedBy { get; set; }
    [Required]
    public DateTimeOffset CreatedOnUtc { get; set; }
    [MaxLength(250)]
    public string UpdatedBy { get; set; }
    public DateTimeOffset? UpdatedOnUtc { get; set; }
    public virtual ICollection<StudentCourse> StudentCourses { get; }
    public virtual ICollection<StudentSchool> StudentSchools { get; }
  }
}