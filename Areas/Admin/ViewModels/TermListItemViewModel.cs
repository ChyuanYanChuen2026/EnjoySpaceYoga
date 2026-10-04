namespace EnjoySpace.Web.Areas.Admin.ViewModels;

public class TermListItemViewModel
{
    public long TermId { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public DateTime? RegistrationStartAtUtc { get; set; }

    public DateTime? RegistrationEndAtUtc { get; set; }

    public string Status { get; set; } = string.Empty;
}