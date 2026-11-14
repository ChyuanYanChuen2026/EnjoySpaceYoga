using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;
using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

var database = "EnjoySpace_AmountChecks_" + Guid.NewGuid().ToString("N");
var connection = new SqlConnectionStringBuilder { DataSource = "localhost", InitialCatalog = database, IntegratedSecurity = true, TrustServerCertificate = true, ConnectTimeout = 5 };
var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connection.ConnectionString).Options;
AppDbContext Db() => new(options);
var protection = new EphemeralDataProtectionProvider();
EnrollmentWorkflowService Service(AppDbContext db) => new(db, protection);
var created = false; int checks = 0;
long adminId = 0, studentId = 0, id = 0;
void Check(bool pass, string label) { if (!pass) throw new Exception(label); Console.WriteLine("通過：" + label); checks++; }
async Task<byte[]> Version() { await using var db = Db(); return (await db.EnrollmentApplications.AsNoTracking().SingleAsync()).RowVersion; }
async Task Reject(decimal amount, string reason, long actor, byte[]? version = null)
{
    await using var db = Db();
    try { await Service(db).AdjustAmountAsync(id, version ?? await Version(), amount, reason, actor); }
    catch (InvalidOperationException) { Check(true, "拒絕無效調整：" + amount); return; }
    throw new Exception("未拒絕無效調整");
}
try
{
    await using (var db = Db())
    {
        created = await db.Database.EnsureCreatedAsync();
        if (!created) throw new Exception("拒絕使用既有資料庫");
        Check(!db.Database.HasPendingModelChanges(), "Snapshot 與目前模型一致");
        var migration = new EnjoySpace.Web.Migrations.AddEnrollmentAmountAdjustments();
        Check(migration.UpOperations.Count == 3 && migration.DownOperations.Count == 1, "Migration 僅新增紀錄表與兩個索引");
        var generator = db.GetService<IMigrationsSqlGenerator>();
        foreach (var command in generator.Generate(migration.DownOperations)) await db.Database.ExecuteSqlRawAsync(command.CommandText);
        var target = db.GetService<IModelRuntimeInitializer>().Initialize(migration.TargetModel, designTime:true);
        foreach (var command in generator.Generate(migration.UpOperations, target)) await db.Database.ExecuteSqlRawAsync(command.CommandText);
        Check(!await db.EnrollmentAmountAdjustments.AnyAsync(), "隔離資料庫執行 Down／Up 成功");
        var admin = new User { Account="amount-admin", DisplayName="管理員測試", PasswordHash="test-only" };
        var student = new Student { User = new User { Account="amount-student", DisplayName="學生測試", PasswordHash="test-only" } };
        db.UserRoles.Add(new UserRole { User=admin, Role=new Role { RoleCode="Admin", RoleName="管理員" } });
        var teacher = new Teacher { User=new User { Account="amount-teacher", DisplayName="老師測試", PasswordHash="test-only" } };
        var location = new StudioLocation { Name="測試教室", Address="test-only" };
        var course = new Course { Name="測試課程", Term=new Term { Name="測試學期", StartDate=new(2030,1,1), EndDate=new(2030,12,31), Status="Open", ScheduleConfirmedAtUtc=DateTime.UtcNow }, ClassCategory=new ClassCategory { Code="Amount", Name="測試" }, DefaultTeacher=teacher, DefaultLocation=location, DefaultCapacity=10, TermPrice=100 };
        db.ClassSessions.Add(new ClassSession { Course=course, Teacher=teacher, StudioLocation=location, StartsAt=DateTimeOffset.UtcNow.AddDays(10), EndsAt=DateTimeOffset.UtcNow.AddDays(10).AddHours(1), Capacity=10 });
        var app = new EnrollmentApplication { Student=student, Course=course, QuotedAmount=100, QuotedSessionCount=1, Payment=new Payment { Student=student, Amount=100, PaymentPurpose="CourseEnrollment" } };
        db.EnrollmentApplications.Add(app); await db.SaveChangesAsync();
        id=app.Id; adminId=admin.UserId; studentId=student.UserId;
    }
    await Reject(-1,"負數",adminId); await Reject(1.5m,"小數",adminId); await Reject(100000000,"超限",adminId);
    await Reject(50,"   ",adminId); await Reject(50,new string('長',501),adminId); await Reject(50,"權限",studentId);
    var originalVersion=await Version();
    await using (var db=Db()) { Check(!await db.EnrollmentAmountAdjustments.AnyAsync(),"無效操作零寫入"); await Service(db).AdjustAmountAsync(id,originalVersion,50,"中途報名議價",adminId); }
    await using (var db=Db())
    {
        var app=await db.EnrollmentApplications.Include(x=>x.Payment).SingleAsync();
        Check(app.QuotedAmount==100 && app.Payment!.Amount==50,"可低於原價且保留原報價");
        Check(!app.RowVersion.SequenceEqual(originalVersion),"調價更新申請版本");
        var audit=await db.EnrollmentAmountAdjustments.SingleAsync();
        Check(audit.PreviousAmount==100 && audit.Amount==50 && audit.Reason=="中途報名議價" && audit.AdjustedByUserId==adminId,"保留完整調整紀錄");
        await Service(db).AdjustAmountAsync(id,app.RowVersion,50,"金額未變",adminId);
        Check(await db.EnrollmentAmountAdjustments.CountAsync()==1,"未變金額不新增假紀錄");
    }
    await Reject(40,"舊版本",adminId,originalVersion);
    await using (var db=Db()) await Service(db).AdjustAmountAsync(id,await Version(),0,"工作室免收費",adminId);
    await using (var db=Db())
    {
        Check(await db.EnrollmentAmountAdjustments.CountAsync()==2,"多次調整保留歷史");
        try { await Service(db).ConfirmPaymentAsync(id,await Version(),100,"Cash",DateTime.UtcNow.AddHours(8),null,adminId,true); throw new Exception("未拒絕錯誤實收"); }
        catch (InvalidOperationException) { Check(true,"實收依目前應收檢核"); }
    }
    await using (var db=Db()) await Service(db).ConfirmPaymentAsync(id,await Version(),0,"Cash",DateTime.UtcNow.AddHours(8),null,adminId,true);
    await Reject(20,"已收款",adminId);
    await using (var db=Db())
    {
        await Service(db).ReviewAsync(id,await Version(),true,null,adminId,true);
        Check(await db.CourseEnrollments.CountAsync()==1 && await db.ClassSessionParticipants.CountAsync()==1,"零元可確認及核准入班");
        Check((await db.EnrollmentApplications.SingleAsync()).QuotedAmount==100 && (await db.Payments.SingleAsync()).Amount==0,"核准仍保留原報價與目前金額");
    }
    await Reject(20,"已核准",adminId);
    Console.WriteLine($"全部 {checks} 項應收調整測試通過。");
}
finally { if(created) { await using var db=Db(); await db.Database.EnsureDeletedAsync(); Console.WriteLine("已清除隔離庫："+database); } }
