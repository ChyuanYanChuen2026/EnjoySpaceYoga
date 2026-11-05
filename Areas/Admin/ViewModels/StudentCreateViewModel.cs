using System.ComponentModel.DataAnnotations;

using EnjoySpace.Web.Services;

namespace EnjoySpace.Web.Areas.Admin.ViewModels;

public class StudentCreateViewModel
{
    [Required(ErrorMessage = "請輸入姓名")]
    [StringLength(100, ErrorMessage = "姓名最多 100 字")]
    [Display(Name = "姓名")]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(30, ErrorMessage = "學號最多 30 字")]
    [Display(Name = "學號")]
    public string? StudentNo { get; set; }

    [RegularExpression(TaiwanMobile.InputPattern, ErrorMessage = TaiwanMobile.ErrorMessage)]
    [StringLength(10, ErrorMessage = TaiwanMobile.ErrorMessage)]
    [DisplayFormat(ConvertEmptyStringToNull = false)]
    [Display(Name = "手機號碼")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "請輸入有效的 Email")]
    [StringLength(200, ErrorMessage = "Email 最多 200 字")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "加入日期")]
    public DateOnly? JoinedDate { get; set; }

    [Display(Name = "啟用學生身分")]
    public bool IsActive { get; set; } = true;

    [StringLength(500, ErrorMessage = "備註最多 500 字")]
    [Display(Name = "備註")]
    public string? Note { get; set; }
