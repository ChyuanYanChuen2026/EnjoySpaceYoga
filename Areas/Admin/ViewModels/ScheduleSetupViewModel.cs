using EnjoySpace.Web.Models;
using EnjoySpace.Web.Models.Entities;

namespace EnjoySpace.Web.Areas.Admin.ViewModels;

// One view model for the wizard; each POST binds and validates only its current step.
public class ScheduleSetupViewModel
{
    public int Step { get; set; } = 1;
    public long TermId { get; set; }
    public long CourseId { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public TermCreateViewModel Term { get; set; } = new();
    public CourseCreateViewModel Course { get; set; } = new();
    public ClassSessionCreateViewModel Session { get; set; } = new();
    public string CourseName { get; set; } = "";
    public short Capacity { get; set; }
    public List<Term> DraftTerms { get; set; } = [];
    public List<TermCourseScheduleItem> Courses { get; set; } = [];
    public List<ClassSessionListItem> Sessions { get; set; } = [];
    public List<string> Issues { get; set; } = [];
}
