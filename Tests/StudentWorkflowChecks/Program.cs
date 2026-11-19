using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;
using EnjoySpace.Web.Services;
using EnjoySpace.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

if (args.Any(arg => arg is not ("--http" or "--ui")))
    throw new ArgumentException("只支援 --http 或 --ui；此測試工具不提供既有資料庫更新功能。");
var name = "EnjoySpace_StudentWorkflowChecks_" + Guid.NewGuid().ToString("N");
var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer($"Server=localhost;Database={name};Integrated Security=true;TrustServerCertificate=true").Options;
AppDbContext Db() => new(options);
var protection = new EphemeralDataProtectionProvider();
var clock = new TestClock(new DateTimeOffset(2031,1,1,0,0,0,TimeSpan.Zero));
using var services = new ServiceCollection().AddScoped(_ => Db()).BuildServiceProvider();
CheckInService Qr(AppDbContext db) => new(db, clock, protection, services.GetRequiredService<IServiceScopeFactory>(), NullLogger<CheckInService>.Instance);
TrialBookingService Trial(AppDbContext db) => new(db,clock);
async Task<T> Read<T>(Func<AppDbContext,Task<T>> work) { await using var db = Db(); return await work(db); }
async Task Write(Func<AppDbContext,Task> work) { await using var db = Db(); await work(db); }
var checks=0;
void Assert(bool pass, string label) { if(!pass) throw new Exception("FAIL: "+label); Console.WriteLine("PASS: "+label); checks++; }
async Task Reject(Func<Task> work, string label) { try { await work(); } catch(Exception e) when(e is InvalidOperationException or UnauthorizedAccessException or DbUpdateException) { Assert(true,label); return; } throw new Exception("Expected rejection: "+label); }
JsonElement Json(object value) => JsonSerializer.SerializeToElement(value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
bool created=false;
try {
  await using(var db=Db()) {
    await db.GetService<IRelationalDatabaseCreator>().CreateAsync(); created=true;
    var migrator=db.GetService<IMigrator>();
    await migrator.MigrateAsync("20260928064739_AddStudioLocations");
    await db.Database.ExecuteSqlRawAsync("INSERT dbo.StudioLocations (Name,Address,CheckInRadiusMeters,IsActive,CreatedAtUtc) VALUES (N'Enjoy Sport Space 享運動空間',N'104臺北市中山區撫順街16號2樓',500,1,SYSUTCDATETIME());");
    await migrator.MigrateAsync("20261102021456_ReconcileRecoveredSchema");
    Assert(true,"legacy schema migrates to recovered baseline");
    await db.Database.ExecuteSqlRawAsync("INSERT dbo.Users(Account,PasswordHash,DisplayName) VALUES(N'preserve-row',N'test-only',N'歷史資料'); DELETE dbo.__EFMigrationsHistory WHERE MigrationId=N'20261102021456_ReconcileRecoveredSchema';");
    foreach(var id in new[]{"20261022040727_AddStudentFlexibleBookings","20261023165439_AddQrCheckInTimestamp","20261025064123_AddSessionOperations","20261026093213_AddTrialSessionEnrollment"})
      await db.Database.ExecuteSqlInterpolatedAsync($"INSERT dbo.__EFMigrationsHistory(MigrationId,ProductVersion) VALUES({id},N'10.0.14')");
    await migrator.MigrateAsync();
    Assert(await db.Users.AnyAsync(x=>x.Account=="preserve-row" && x.DisplayName=="歷史資料"),"current-schema upgrade preserves existing row and missing-source migration history");
    Assert(!db.Database.HasPendingModelChanges(),"migration snapshot matches current model");
    await migrator.MigrateAsync(); Assert(!(await db.Database.GetPendingMigrationsAsync()).Any(),"migration retry is idempotent");
  }
  long admin=0, teacherActor=0, studentActor=0, studentId=0, otherActor=0, parent=0; int[] sessions=[];
  await Write(async db=>{
    var adminRole=await db.Roles.SingleAsync(x=>x.RoleCode=="Admin"); var teacherRole=await db.Roles.SingleAsync(x=>x.RoleCode=="Teacher"); var studentRole=await db.Roles.SingleAsync(x=>x.RoleCode=="Student");
    User U(string account)=>new(){Account=account,DisplayName=account,PasswordHash="test-only"};
    var a=U("qr-admin"); var t=new Teacher{User=U("qr-teacher")}; var s=new Student{User=U("qr-student")}; var o=new Student{User=U("qr-other")};
    db.AddRange(new UserRole{User=a,Role=adminRole},new UserRole{User=t.User,Role=teacherRole},new UserRole{User=s.User,Role=studentRole},new UserRole{User=o.User,Role=studentRole},t,s,o);
    var term=new Term{Name="QR test",StartDate=new(2031,1,1),EndDate=new(2031,12,31),Status="Open"}; var location=await db.StudioLocations.FirstAsync();
    var course=new Course{Name="QR 與體驗測試",Term=term,ClassCategory=new(){Code="QR",Name="測試"},DefaultTeacher=t,DefaultLocation=location,DefaultCapacity=10,IsPubliclyVisible=true};
    var rows=Enumerable.Range(1,6).Select(i=>new ClassSession{Course=course,Teacher=t,StudioLocation=location,StartsAt=clock.GetUtcNow().AddDays(i).AddHours(1),EndsAt=clock.GetUtcNow().AddDays(i).AddHours(2),Capacity=10}).ToArray();
    var plan=new LessonPlan{PlanCode="QR-TRIAL",PlanName="三堂體驗",PlanType="Trial",AudienceType="TrialGuest",SessionQuota=3,Price=1000,ValidityUnit="Months",ValidityValue=2};
    var request=new PortalTrialRequest{User=s.User,TrialGuest=new(){Name="體驗學生",Phone="0900000000",SourceType="Direct"},ClassSession=rows[0],LessonPlan=plan,PlanName=plan.PlanName,QuotedPrice=1000,QuotedQuota=3,QuotedValidityUnit="Months",QuotedValidityValue=2,RequiresReferrerAtRequest=false,SubmissionKey=Guid.NewGuid(),CreatedAtUtc=clock.GetUtcNow().UtcDateTime};
    db.AddRange(rows); db.Add(request); await db.SaveChangesAsync(); admin=a.UserId; teacherActor=t.UserId; studentActor=s.UserId;studentId=s.StudentId;otherActor=o.UserId; parent=request.Id;sessions=rows.Select(x=>x.Id).ToArray();
  });
  Task<byte[]> ParentVersion()=>Read(db=>db.PortalTrialRequests.Where(x=>x.Id==parent).Select(x=>x.RowVersion).SingleAsync());
  async Task<long[]> Book(int[] ids,Guid? batch=null) {var v=await ParentVersion();return await Read(db=>Trial(db).BookAsync(parent,v,batch??Guid.NewGuid(),ids,studentActor));}
  async Task Review(long id,bool approve,string? note=null) {var v=await Read(db=>db.TrialBookings.Where(x=>x.Id==id).Select(x=>x.RowVersion).SingleAsync());await Write(db=>Trial(db).ReviewBookingAsync(id,v,approve,note,admin));}
  await Reject(()=>Read(db=>Trial(db).OptionsAsync(parent,studentActor)),"pending parent cannot schedule");
  var pv=await ParentVersion(); await Write(db=>Trial(db).ReviewAsync(parent,pv,true,null,new(2031,1,1),null,admin));
  await Reject(()=>Book([sessions[0]]),"approved but unpaid plan cannot reserve");
  await Write(async db=>{var pass=await db.PortalTrialRequests.Where(x=>x.Id==parent).Select(x=>x.LessonPass!).SingleAsync(); await new TrialLessonService(db,protection,clock).ConfirmPaymentAsync(new(){Id=pass.LessonPassId,RowVersion=pass.RowVersion,ReceivedAmount=1000,Method="Cash",PaidAtTaiwan=clock.GetUtcNow().ToOffset(TimeSpan.FromHours(8)).DateTime,ConfirmDetails=true},admin);});
  Assert(Json(await Read(db=>Trial(db).OptionsAsync(parent,studentActor))).GetProperty("remaining").GetInt32()==3,"payment unlocks three reservations");
  await Reject(()=>Book([sessions[0],sessions[0]]),"duplicate selection rejected before any children saved");
  var key=Guid.NewGuid();var ids=await Book(sessions.Take(3).ToArray(),key);
  Assert(ids.Length==3 && ids.Distinct().Count()==3,"three independent booking IDs share one parent");
  Assert((await Book(sessions.Take(3).ToArray(),key)).SequenceEqual(ids),"lost submit response retry keeps same child IDs");
  await Reject(()=>Book([sessions[3]]),"pending bookings reserve all quota");
  await Review(ids[0],true);await Review(ids[1],true);await Review(ids[2],false,"改約其他課堂");
  Assert(await Read(db=>db.ClassSessionParticipants.CountAsync(x=>x.TrialRequestId==parent))==2,"same trial parent supports two approved roster rows");
  Assert(Json(await Read(db=>Trial(db).OptionsAsync(parent,studentActor))).GetProperty("remaining").GetInt32()==1,"rejected booking frees quota");
  await Reject(()=>Book([sessions[0]]),"already approved class cannot be booked again");
  await Reject(()=>Read(db=>Trial(db).OptionsAsync(parent,otherActor)),"another student cannot access trial options");
  var b=await Book([sessions[3]]);var bv=await Read(db=>db.TrialBookings.Where(x=>x.Id==b[0]).Select(x=>x.RowVersion).SingleAsync());await Write(db=>Trial(db).CancelAsync(b[0],bv,"時間不合",studentActor));
  Assert(Json(await Read(db=>Trial(db).OptionsAsync(parent,studentActor))).GetProperty("remaining").GetInt32()==1,"cancel pending releases quota");
  var concurrent=await Task.WhenAll(new[]{sessions[4],sessions[5]}.Select(async id=>{try{await Book([id]);return true;}catch(Exception e) when(e is InvalidOperationException or DbUpdateException or Microsoft.Data.SqlClient.SqlException){return false;}}));
  Assert(concurrent.Count(x=>x)==1,"concurrent submissions cannot overuse last quota");
  var start=await Read(db=>db.ClassSessions.Where(x=>x.Id==sessions[0]).Select(x=>x.StartsAt).SingleAsync());
  clock.Now=start.AddMinutes(-31);await Reject(()=>Read(db=>Qr(db).IssueAsync(sessions[0],admin)),"QR unavailable before opening window");
  clock.Now=start.AddMinutes(-30);var qr=await Read(db=>Qr(db).IssueAsync(sessions[0],admin));
  var teacherQr=await Read(db=>Qr(db).IssueAsync(sessions[0],teacherActor));Assert(qr.Token==teacherQr.Token && qr.Token.Length==64,"admin and assigned teacher reopen same random QR");
  var originalTeacher=await Read(db=>db.ClassSessions.Where(x=>x.Id==sessions[0]).Select(x=>x.TeacherId).SingleAsync());
  await Write(async db=>{var substitute=new Teacher{UserId=otherActor};db.Add(substitute);await db.SaveChangesAsync();(await db.ClassSessions.FindAsync(sessions[0]))!.TeacherId=substitute.TeacherId;await db.SaveChangesAsync();});
  await Reject(()=>Read(db=>Qr(db).IssueAsync(sessions[0],teacherActor)),"teacher cannot open another teacher's session QR");
  await Write(async db=>{(await db.ClassSessions.FindAsync(sessions[0]))!.TeacherId=originalTeacher;await db.SaveChangesAsync();});
  await Reject(()=>Read(db=>Qr(db).IssueAsync(sessions[0],studentActor)),"student cannot issue classroom QR");
  var preview=Json(await Read(db=>Qr(db).ScanAsync(qr.Token,studentActor,false)));Assert(preview.GetProperty("canCheckIn").GetBoolean(),"approved paid trial student can preview check-in");
  Assert(await Read(db=>db.AttendanceRecords.Where(x=>x.Participant.TrialRequestId==parent).AllAsync(x=>x.Status=="Scheduled")),"preview never marks attendance");
  await Reject(()=>Read(db=>Qr(db).ScanAsync(qr.Token,otherActor,true)),"non-roster student cannot check in");
  Assert(await Read(db=>db.CheckInEvents.AnyAsync(x=>x.UserId==otherActor && x.IsError && x.ClassSessionId==sessions[0])),"failed check-in stored separately with student and session");
  clock.Now=start.AddMinutes(10);var present=Json(await Read(db=>Qr(db).ScanAsync(qr.Token,studentActor,true)));Assert(present.GetProperty("status").GetString()=="Present","exactly ten minutes after start is Present");
  var firstTime=present.GetProperty("checkedInAt").GetString();clock.Now=clock.Now.AddMinutes(2);var again=Json(await Read(db=>Qr(db).ScanAsync(qr.Token,studentActor,true)));Assert(again.GetProperty("checkedInAt").GetString()==firstTime && again.GetProperty("status").GetString()=="Present","repeated scan preserves first timestamp and status");
  await Reject(()=>Write(async db=>{var p=await db.ClassSessionParticipants.Include(x=>x.Attendance).SingleAsync(x=>x.TrialBookingId==ids[0]);await new AttendanceService(db,clock).RecordAsync(new(){SessionId=sessions[0],ParticipantId=p.SessionParticipantId,RowVersion=p.Attendance.RowVersion,Status="Present"},teacherActor);}),"manual rewrite of same QR status also requires reason");
  var fullReason=new string('核',500);
  await Write(async db=>{var p=await db.ClassSessionParticipants.Include(x=>x.Attendance).SingleAsync(x=>x.TrialBookingId==ids[0]);await new AttendanceService(db,clock).RecordAsync(new(){SessionId=sessions[0],ParticipantId=p.SessionParticipantId,RowVersion=p.Attendance.RowVersion,Status="Absent",ManualReason=fullReason},teacherActor);});
  Assert(await Read(db=>db.CheckInEvents.AnyAsync(x=>x.UserId==teacherActor && x.Code=="MANUAL_ATTENDANCE" && x.Message.EndsWith(fullReason))),"full correction reason is preserved in actor audit history");
  var manual=Json(await Read(db=>Qr(db).ScanAsync(qr.Token,studentActor,true)));Assert(manual.GetProperty("status").GetString()=="Absent" && !manual.GetProperty("canCheckIn").GetBoolean() && manual.GetProperty("checkedInAt").GetString()==firstTime,"scan cannot overwrite manual correction or first arrival time");
  clock.Now=(await Read(db=>db.ClassSessions.Where(x=>x.Id==sessions[1]).Select(x=>x.StartsAt).SingleAsync())).AddMinutes(10).AddSeconds(1);
  var qr2=await Read(db=>Qr(db).IssueAsync(sessions[1],admin));
  await Write(async db=>{(await db.ClassSessionParticipants.SingleAsync(x=>x.TrialBookingId==ids[1])).Status="OnLeave";await db.SaveChangesAsync();});
  await Reject(()=>Read(db=>Qr(db).ScanAsync(qr2.Token,studentActor,true)),"student on leave cannot check in");
  await Write(async db=>{(await db.ClassSessionParticipants.SingleAsync(x=>x.TrialBookingId==ids[1])).Status="Active";await db.SaveChangesAsync();});
  var late=Json(await Read(db=>Qr(db).ScanAsync(qr2.Token,studentActor,true)));Assert(late.GetProperty("status").GetString()=="Late","more than ten minutes is Late");
  Assert(await Read(db=>db.CheckInEvents.AnyAsync(x=>x.UserId==studentActor && x.ClassSessionId==sessions[1] && x.IsError && x.ResolvedAt!=null)),"successful retry resolves previous check-in anomaly");
  clock.Now=qr2.ExpiresAt;await Reject(()=>Read(db=>Qr(db).IssueAsync(sessions[1],admin)),"QR expires exactly at session end");
  Assert(Json(await Read(db=>Qr(db).ReceiptAsync(sessions[1],studentActor))).GetProperty("status").GetString()=="Late","receipt can recover successful check-in after closing");
  await Reject(()=>Read(db=>Qr(db).ReceiptAsync(sessions[1],otherActor)),"receipt does not disclose another student's attendance");
  clock.Now = new DateTimeOffset(2031,1,1,0,0,0,TimeSpan.Zero);
  long freeId=0;
  await Write(async db=>{var existing=await db.PortalTrialRequests.SingleAsync(x=>x.Id==parent);var free=new PortalTrialRequest{UserId=studentActor,TrialGuestId=existing.TrialGuestId,ClassSessionId=sessions[3],LessonPlan=new(){PlanCode="FREE-QR",PlanName="舊生介紹",PlanType="Trial",AudienceType="TrialGuest",SessionQuota=1,Price=0,ValidityUnit="Unlimited",ValidityValue=null,RequiresReferrer=true},PlanName="舊生介紹",QuotedPrice=0,QuotedQuota=1,QuotedValidityUnit="Unlimited",RequiresReferrerAtRequest=true,SubmissionKey=Guid.NewGuid(),ReferrerName="介紹學生"};db.Add(free);await db.SaveChangesAsync();freeId=free.Id;});
  var fv=await Read(db=>db.PortalTrialRequests.Where(x=>x.Id==freeId).Select(x=>x.RowVersion).SingleAsync());
  await Reject(()=>Write(db=>Trial(db).ReviewAsync(freeId,fv,true,null,new(2031,1,1),studentId,admin)),"free referral cannot name self as referrer");
  var referrer=await Read(db=>db.Students.Where(x=>x.UserId==otherActor).Select(x=>x.StudentId).SingleAsync());
  await Write(db=>Trial(db).ReviewAsync(freeId,fv,true,null,new(2031,1,1),referrer,admin));
  Assert(await Read(db=>db.PortalTrialRequests.AnyAsync(x=>x.Id==freeId && x.LessonPass!.Status=="Active" && x.LessonPass.Payment.Status=="Confirmed" && x.LessonPass.Payment.PaidAtUtc==null)),"free referral activates without inventing collection");
  await Reject(()=>Read(db=>Trial(db).OptionsAsync(freeId,studentActor)),"free referral requires staff scheduling");
  fv=await Read(db=>db.PortalTrialRequests.Where(x=>x.Id==freeId).Select(x=>x.RowVersion).SingleAsync());
  var freeBookings=await Read(db=>Trial(db).BookAsync(freeId,fv,Guid.NewGuid(),[sessions[3]],admin,true)); await Review(freeBookings[0],true);
  Assert(await Read(db=>db.ClassSessionParticipants.AnyAsync(x=>x.TrialBookingId==freeBookings[0])),"admin can schedule approved free referral");
  if(args.Contains("--http") || args.Contains("--ui")) await HttpChecks.RunAsync(Db, parent, sessions[0], studentActor, admin, teacherActor, Assert, args.Contains("--ui"));
  Console.WriteLine($"All {checks} QR/trial/migration checks passed.");
} finally { if(created){await using var db=Db();await db.Database.EnsureDeletedAsync();Console.WriteLine("Removed isolated database: "+name);} }
sealed class TestClock(DateTimeOffset now):TimeProvider {public DateTimeOffset Now{get;set;}=now; public override DateTimeOffset GetUtcNow()=>Now;}
