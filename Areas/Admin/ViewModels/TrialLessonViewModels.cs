using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using EnjoySpace.Web.Models.Entities;

namespace EnjoySpace.Web.Areas.Admin.ViewModels;

public class TrialGuestEditModel
{
    public long Id { get; set; }
    public byte[]? RowVersion { get; set; }
    [Required, StringLength(100)] public string Name { get; set; } = "";
    [Required, StringLength(30)] public string Phone { get; set; } = "";
    [EmailAddress, StringLength(200)] public string? Email { get; set; }
    [StringLength(100)] public string? LineId { get; set; }
    [Required] public string SourceType { get; set; } = "Direct";
    public long? ReferrerStudentId { get; set; }
    public bool IsActive { get; set; } = true;
    [StringLength(500)] public string? Note { get; set; }
    [ValidateNever] public List<SelectListItem> Referrers { get; set; } = [];
}

public class LessonPlanEditModel
{
    public long Id { get; set; }
    public byte[]? RowVersion { get; set; }
    [Required, StringLength(50)] public string PlanCode { get; set; } = "";
    [Required, StringLength(100)] public string PlanName { get; set; } = "";
    [Required] public string PlanType { get; set; } = "Trial";
    [Range(1, short.MaxValue)] public short SessionQuota { get; set; } = 1;
    [Range(typeof(decimal), "0", "99999999.99")] public decimal Price { get; set; }
    [Required] public string ValidityUnit { get; set; } = "Days";
    [Range(1, short.MaxValue)] public short? ValidityValue { get; set; } = 30;
    public bool IsActive { get; set; } = true;
}

public class LessonPurchaseModel
{
    [Range(1, long.MaxValue)] public long PlanId { get; set; }
    [Required] public string OwnerType { get; set; } = "TrialGuest";
    [Range(1, long.MaxValue)] public long OwnerId { get; set; }
    public DateOnly ValidFromDate { get; set; }
    [ValidateNever] public List<SelectListItem> Plans { get; set; } = [];
    [ValidateNever] public List<SelectListItem> Owners { get; set; } = [];
}

public class LessonQuoteModel
{
    public string Token { get; set; } = "";
    public string OwnerName { get; set; } = "";
    public string PlanName { get; set; } = "";
    public short Quota { get; set; }
    public decimal Amount { get; set; }
    public DateOnly From { get; set; }
    public DateOnly? Until { get; set; }
}

public class LessonPaymentModel
{
    [Range(1, long.MaxValue)] public long Id { get; set; }
    [Required, MinLength(8), MaxLength(8)] public byte[] RowVersion { get; set; } = [];
    [Range(typeof(decimal), "0", "99999999.99")] public decimal ReceivedAmount { get; set; }
    [Required] public string Method { get; set; } = "Cash";
    public DateTime PaidAtTaiwan { get; set; }
    public bool ConfirmDetails { get; set; }
    [StringLength(500)] public string? Note { get; set; }
}

public class PaymentListModel
{
    public List<PaymentListRow> Items { get; set; } = [];
    public int Page { get; set; }
    public int TotalCount { get; set; }
    public int PageCount => Math.Max(1, (TotalCount + 19) / 20);
}
public class PaymentListRow
{
    public Payment Payment { get; set; } = null!;
    public long? ApplicationId { get; set; }
    public long? PassId { get; set; }
    public string Description { get; set; } = "";
}

public class PagedRecords<T>
{
    public List<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int TotalCount { get; set; }
    public int PageCount => Math.Max(1, (TotalCount + 19) / 20);
}
