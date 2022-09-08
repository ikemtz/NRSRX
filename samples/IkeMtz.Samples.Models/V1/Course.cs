using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IkeMtz.Samples.Models.V1
{
  // Generated by the SQL POCO Class Generator Script
  // Script is available at:
  // https://raw.githubusercontent.com/ikemtz/NRSRx/master/tools/sql-poco-class-generator.sql

  public partial class Course
  : IkeMtz.NRSRx.Core.Models.IIdentifiable, IkeMtz.NRSRx.Core.Models.IAuditable
  {
    public Course()
    {
      StudentCourses = new HashSet<StudentCourse>();
      SchoolCourses = new HashSet<SchoolCourse>();
    }

    [Required]
    public Guid Id { get; set; }
    [Required]
    [MaxLength(150)]
    public string Department { get; set; }
    [Required]
    [MaxLength(10)]
    public string Num { get; set; }
    [Required]
    [MaxLength(150)]
    public string Title { get; set; }
    [MaxLength(500)]
    public string Description { get; set; }
    [DefaultValue(0)]
    public double? PassRate { get; set; }
    [DefaultValue(0)]
    public double? AvgScore { get; set; }
    [Required]
    [MaxLength(250)]
    public string CreatedBy { get; set; }
    [Required]
    public DateTimeOffset CreatedOnUtc { get; set; }
    [MaxLength(250)]
    public string? UpdatedBy { get; set; }
    public DateTimeOffset? UpdatedOnUtc { get; set; }
    public virtual ICollection<StudentCourse> StudentCourses { get; }
    public virtual ICollection<SchoolCourse> SchoolCourses { get; }
  }

}
