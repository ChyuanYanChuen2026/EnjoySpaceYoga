using System.ComponentModel.DataAnnotations;
using EnjoySpace.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnjoySpace.Web.Areas.Admin.ViewModels;

public class LeaveMakeupListModel
{
    public string Tab { get; set; } = "leaves";
    public int Page { get; set; } = 1;
    public int TotalCount { get; set; }
    public int PageCount => Math.Max(1, (TotalCount + 19) / 20);
    public DateTimeOffset Now { get; set; }
    public List<LeaveRequest> Leaves { get; set; } = [];
    public List<MakeupEntitlement> Entitlements { get; set; } = [];
    public List<MakeupRequest> Requests { get; set; } = [];
    public static string Label(string status) => status switch
    {
        "Active" => "請假中", "Cancelled" => "已取消", "Available" => "可使用", "Reserved" => "申請待審中",
        "Consumed" => "已核准使用", "Revoked" => "已撤銷", "Pending" => "待審核", "Approved" => "已核准", "Rejected" => "已退回", _ => status
    };
}

public class LeaveCreateModel
{
    [Range(1, long.MaxValue)] public long ParticipantId { get; set; }
    [Required, MinLength(8), MaxLength(8)] public byte[] RowVersion { get; set; } = [];
    [Required(ErrorMessage = "請填寫請假原因。"), StringLength(500)] public string Reason { get; set; } = "";
    [ValidateNever] public string StudentName { get; set; } = "";
    [ValidateNever] public string CourseName { get; set; } = "";
    [ValidateNever] public DateTimeOffset StartsAt { get; set; }
    [ValidateNever] public int SessionId { get; set; }
}

public class MakeupCreateModel
{
    [Range(1, long.MaxValue)] public long EntitlementId { get; set; }
    [Required, MinLength(8), MaxLength(8)] public byte[] RowVersion { get; set; } = [];
    [Range(1, int.MaxValue, ErrorMessage = "請選擇補課場次。")] public int TargetSessionId { get; set; }
    [ValidateNever] public string StudentName { get; set; } = "";
    [ValidateNever] public string SourceDescription { get; set; } = "";
    [ValidateNever] public List<SelectListItem> Targets { get; set; } = [];
}

public class LeaveMakeupActionModel
{
    [Range(1, long.MaxValue)] public long Id { get; set; }
    [Required, MinLength(8), MaxLength(8)] public byte[] RowVersion { get; set; } = [];
    [StringLength(500)] public string? Note { get; set; }
    public bool Approve { get; set; }
}
