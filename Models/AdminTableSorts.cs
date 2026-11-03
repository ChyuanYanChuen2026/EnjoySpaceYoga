using EnjoySpace.Web.Models.Entities;

namespace EnjoySpace.Web.Models;

public static class AdminTableSorts
{
    public static readonly TableSort<Term> Terms = new TableSort<Term>("date_desc", x => x.TermId)
        .Column("date", "開始日期", x => x.StartDate).Column("name", "學期名稱", x => x.Name)
        .Column("end", "結束日期", x => x.EndDate).Column("registrationStart", "報名開始時間", x => x.RegistrationStartAtUtc)
        .Column("registrationEnd", "報名結束時間", x => x.RegistrationEndAtUtc).Column("status", "狀態", x => x.Status);
    public static readonly TableSort<Course> Courses = new TableSort<Course>("date_desc", x => x.CourseId)
        .Column("date", "學期開始日期", x => x.Term.StartDate).Column("term", "學期名稱", x => x.Term.Name)
        .Column("name", "課程名稱", x => x.Name).Column("capacity", "預設名額", x => x.DefaultCapacity)
        .Column("price", "期課價格", x => x.TermPrice).Column("status", "啟用／公開狀態", x => (x.IsActive ? 2 : 0) + (x.IsPubliclyVisible ? 1 : 0));
    public static readonly TableSort<ClassSession> Sessions = new TableSort<ClassSession>("recent", x => x.Id)
        .Preset("recent", "近期優先（今天起）", query => OrderRecentSessions(query, DateTimeOffset.UtcNow))
        .Column("date", "開始時間", x => x.StartsAt).Column("name", "課程名稱", x => x.Course.Name)
        .Column("teacher", "老師", x => x.Teacher.User.DisplayName).Column("location", "地點", x => x.StudioLocation.Name)
        .Column("end", "結束時間", x => x.EndsAt).Column("capacity", "名額上限", x => x.CapacityOverride ?? x.Capacity);
    public static readonly TableSort<Student> Students = new TableSort<Student>("number_asc", x => x.StudentId)
        .Column("number", "學號", x => x.StudentNo).Column("name", "姓名", x => x.User.DisplayName)
        .Column("phone", "電話", x => x.User.Phone).Column("email", "Email", x => x.User.Email)
        .Column("date", "加入日期", x => x.JoinedDate).Column("status", "學生狀態", x => x.IsActive)
        .Column("account", "登入帳號狀態", x => x.User.IsActive);
    public static readonly TableSort<Teacher> Teachers = new TableSort<Teacher>("number_asc", x => x.TeacherId)
        .Column("number", "老師編號", x => x.TeacherNo).Column("name", "姓名", x => x.User.DisplayName)
        .Column("phone", "電話", x => x.User.Phone).Column("email", "Email", x => x.User.Email)
        .Column("status", "老師狀態", x => x.IsActive).Column("account", "登入帳號狀態", x => x.User.IsActive);
    public static readonly TableSort<LessonPlan> Plans = new TableSort<LessonPlan>("id_desc", x => x.LessonPlanId)
        .Column("id", "建立順序", x => x.LessonPlanId).Column("name", "方案名稱", x => x.PlanName)
        .Column("audience", "對象", x => x.AudienceType).Column("quota", "堂數", x => x.SessionQuota)
        .Column("price", "價格", x => x.Price).Column("validity", "效期（先單位、再數值）", x => x.ValidityUnit, x => x.ValidityValue)
        .Column("status", "狀態", x => x.IsActive);
    public static readonly TableSort<LessonPass> Passes = new TableSort<LessonPass>("id_desc", x => x.LessonPassId)
        .Column("id", "建立順序", x => x.LessonPassId).Column("name", "購買對象", x => x.TrialGuest != null ? x.TrialGuest.Name : x.Student!.User.DisplayName)
        .Column("plan", "購買時方案", x => x.PlanNameAtPurchase).Column("quota", "堂數", x => x.SessionQuotaAtPurchase)
        .Column("price", "金額", x => x.AmountAtPurchase).Column("date", "生效日期", x => x.ValidFromDate)
        .Column("payment", "付款狀態", x => x.Payment.Status);
    public static readonly TableSort<Payment> Payments = new TableSort<Payment>("id_desc", x => x.PaymentId)
        .Column("id", "建立順序", x => x.PaymentId).Column("name", "付款人", x => x.Student != null ? x.Student.User.DisplayName : x.TrialGuest!.Name)
        .Column("amount", "金額", x => x.Amount).Column("status", "狀態", x => x.Status);
    public static readonly TableSort<EnrollmentApplication> Applications = new TableSort<EnrollmentApplication>("date_desc", x => x.Id)
        .Column("date", "報名時間", x => x.SubmittedAt).Column("name", "學生", x => x.Student.User.DisplayName)
        .Column("course", "學期／課程", x => x.Course.Term.Name + " · " + x.Course.Name)
        .Column("quota", "報價堂數", x => x.QuotedSessionCount).Column("price", "報價金額", x => x.QuotedAmount)
        .Column("payment", "付款狀態", x => x.Payment == null ? "" : x.Payment.Status).Column("status", "審核狀態", x => x.Status);
    public static readonly TableSort<CourseEnrollment> Enrollments = new TableSort<CourseEnrollment>("date_desc", x => x.CourseEnrollmentId)
        .Column("date", "報名時間", x => x.EnrolledAtUtc).Column("name", "學生", x => x.Student.User.DisplayName)
        .Column("course", "學期／課程", x => x.Course.Term.Name + " · " + x.Course.Name).Column("status", "狀態", x => x.Status);
    public static readonly TableSort<TeacherRequest> TeacherRequests = new TableSort<TeacherRequest>("id_desc", x => x.Id)
        .Column("id", "建立順序", x => x.Id).Column("teacher", "老師", x => x.Teacher.User.DisplayName)
        .Column("course", "課程", x => x.Course.Name).Column("kind", "類型", x => x.Kind)
        .Column("date", "時間", x => x.StartsAt).Column("capacity", "申請總名額", x => x.RequestedCapacity)
        .Column("status", "狀態", x => x.Status);

    public static IOrderedQueryable<ClassSession> OrderRecentSessions(IQueryable<ClassSession> query, DateTimeOffset now)
    {
        var offset = TimeSpan.FromHours(8);
        var local = now.ToOffset(offset);
        var today = new DateTimeOffset(local.Year, local.Month, local.Day, 0, 0, 0, offset);
        // Keep all records: today/future ascend, then past dates descend. SQL sorts before pagination.
        return query.OrderBy(x => x.StartsAt < today)
            .ThenBy(x => x.StartsAt >= today ? (DateTimeOffset?)x.StartsAt : null)
            .ThenByDescending(x => x.StartsAt < today ? (DateTimeOffset?)x.StartsAt : null)
            .ThenBy(x => x.Id);
    }

    public static ITableSort Get(string name) => name switch {
        "terms" => Terms, "courses" => Courses, "sessions" => Sessions, "students" => Students, "teachers" => Teachers,
        "plans" => Plans, "passes" => Passes, "payments" => Payments,
        "applications" => Applications, "enrollments" => Enrollments, "teacherRequests" => TeacherRequests,
        _ => throw new ArgumentException("Unknown table sort", nameof(name)) };
}
