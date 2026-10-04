using EnjoySpace.Web.Models.Enums;

namespace EnjoySpace.Web.Models;

public static class WorkflowLabels
{
    public static string Payment(string? status) => status switch
    {
        "Pending" => "待付款確認", 
        "Confirmed" => "已確認收款",
        "RefundPending" => "待退款", 
        "Refunded" => "已登記退款",
        "Cancelled" => "已取消", 
        _ => "尚無付款"
    };
    public static string Application(EnrollmentApplicationStatus status) => status switch
    {
        EnrollmentApplicationStatus.Pending => "待審核",
        EnrollmentApplicationStatus.Approved => "已核准",
        EnrollmentApplicationStatus.Rejected => "已退回",
        EnrollmentApplicationStatus.Cancelled => "已取消", _ => "未知"
    };
}
