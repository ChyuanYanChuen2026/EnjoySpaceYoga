namespace EnjoySpace.Web.Models;

// Service 的儲存輸入，不是 EF Entity；避免持續增加位置參數。
public sealed record CourseSettings(
    short DefaultCapacity,
    bool IsPubliclyVisible,
    bool AllowsFlexibleBooking,
    bool AllowsMakeup,
    bool IsActive);
