using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;
using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

internal static class HttpChecks {
 public static async Task RunAsync(Func<AppDbContext> createDb,long parent,int session,long student,long admin,long teacher,Action<bool,string> check,bool ui) {
  var password="Isolated-http-" + Guid.NewGuid().ToString("N") + "!"; string connection;
  await using(var db=createDb()){
   connection=db.Database.GetConnectionString()!;
   foreach(var u in await db.Users.Where(x=>x.UserId==student||x.UserId==admin||x.UserId==teacher).ToListAsync())u.PasswordHash=new PasswordHasher<User>().HashPassword(u,password);
   var s=await db.ClassSessions.FindAsync(session);s!.StartsAt=DateTimeOffset.UtcNow.AddMinutes(2);s.EndsAt=s.StartsAt.AddHours(1);
   var attendance=await db.AttendanceRecords.SingleAsync(x=>x.Participant.TrialRequestId==parent && x.Participant.ClassSessionId==session);attendance.Status="Scheduled";attendance.CheckInMethod=null;attendance.CheckedInAtUtc=null;await db.SaveChangesAsync();
  }
  var dir=new DirectoryInfo(AppContext.BaseDirectory);while(dir!=null&&!File.Exists(Path.Combine(dir.FullName,"EnjoySpace.Web.csproj")))dir=dir.Parent;
  var start=new ProcessStartInfo("dotnet"){WorkingDirectory=dir!.FullName,UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden,RedirectStandardOutput=true,RedirectStandardError=true};
  foreach(var arg in new[]{typeof(AppDbContext).Assembly.Location,"--urls","http://127.0.0.1:0","--environment","Development"})start.ArgumentList.Add(arg);
  start.Environment["ConnectionStrings__DefaultConnection"]=connection;start.Environment["Seed__SyncAutumn2026Schedule"]="false";start.Environment["Portal__PublicBaseUrl"]="http://127.0.0.1:4400";start.Environment["PasswordRecovery__Host"]="";
  var ready=new TaskCompletionSource<string>(); using var process=new Process{StartInfo=start};
  process.OutputDataReceived+=(_,e)=>{if(e.Data?.Contains("Now listening on: ")==true)ready.TrySetResult(e.Data.Split("Now listening on: ")[1].Trim());};process.ErrorDataReceived+=(_,_)=>{};
  process.Start();process.BeginOutputReadLine();process.BeginErrorReadLine();
  try{
   var url=await ready.Task.WaitAsync(TimeSpan.FromSeconds(30));
   HttpClient Client()=>new(new HttpClientHandler{AllowAutoRedirect=false,CookieContainer=new CookieContainer()}){BaseAddress=new Uri(url)};
   using var staff=Client();using var learner=Client();using var tutor=Client();
   string Hidden(string html)=>WebUtility.HtmlDecode(Regex.Match(html,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]*)\"").Groups[1].Value);
   async Task StaffLogin(HttpClient client,string account){var html=await client.GetStringAsync("/Account/Login");var response=await client.PostAsync("/Account/Login",new FormUrlEncodedContent(new Dictionary<string,string>{{"Account",account},{"Password",password},{"__RequestVerificationToken",Hidden(html)}}));check(response.StatusCode==HttpStatusCode.Redirect,"staff login "+account);}
   await StaffLogin(staff,"qr-admin");await StaffLogin(tutor,"qr-teacher");
   async Task<HttpResponseMessage> Post(string path,object body,bool csrf=true){var token=await learner.GetFromJsonAsync<JsonElement>("/api/portal/csrf");using var request=new HttpRequestMessage(HttpMethod.Post,"/api/portal/"+path){Content=JsonContent.Create(body)};if(csrf)request.Headers.Add("X-CSRF-TOKEN",token.GetProperty("token").GetString());return await learner.SendAsync(request);}
   check((await Post("login",new{account="qr-student",password})).IsSuccessStatusCode,"student login for QR HTTP checks");
   var page=await staff.GetStringAsync("/Classroom?sessionId="+session);check(page.Contains("classroom.js"),"admin classroom renders polling and QR controls");
   check((await tutor.GetAsync("/Classroom?sessionId="+session)).IsSuccessStatusCode,"assigned teacher can open classroom");
   var qrResponse=await staff.PostAsync("/Classroom/Qr",new FormUrlEncodedContent(new Dictionary<string,string>{{"sessionId",session.ToString()},{"__RequestVerificationToken",Hidden(page)}}));var qrImage=await qrResponse.Content.ReadFromJsonAsync<JsonElement>();check(qrResponse.IsSuccessStatusCode && qrImage.GetProperty("image").GetString()!.StartsWith("data:image/png;base64,"),"classroom QR renders PNG");
   string token;
   // Same persisted key ring and content root discriminator as the host.
   var protection=DataProtectionProvider.Create(new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"ASP.NET","DataProtection-Keys")),b=>b.SetApplicationName(dir.FullName+Path.DirectorySeparatorChar));
   await using(var db=createDb()){var record=await db.SessionQrCodes.SingleAsync(x=>x.ClassSessionId==session);token=protection.CreateProtector("EnjoySpace.SessionQr.v1").Unprotect(record.ProtectedToken);}
   check((await Post("check-in",new{token},false)).StatusCode==HttpStatusCode.BadRequest,"QR confirm requires CSRF");
   check((await Post("check-in/preview",new{token})).IsSuccessStatusCode,"QR preview HTTP contract");
   check((await Post("check-in",new{token})).IsSuccessStatusCode,"student confirmation records attendance");
   var snapshot=await tutor.GetFromJsonAsync<JsonElement>("/Classroom/Snapshot?sessionId="+session);check(snapshot.GetProperty("records").EnumerateArray().Any(x=>x.GetProperty("status").GetString()=="Present"),"teacher polling reads student Present immediately");
   var parents=await learner.GetFromJsonAsync<JsonElement>("/api/portal/my-trials");check(parents.GetArrayLength()==2 && parents[0].TryGetProperty("bookings",out _),"student trial list and per-session bookings serialize");
   check((await staff.GetAsync("/Admin/TrialDesk/Details/"+parent)).IsSuccessStatusCode,"admin trial review view renders");
   check((await learner.GetAsync("/api/portal/my-trials/"+parent+"/options")).IsSuccessStatusCode,"student booking options query translates to SQL");
   check((await Post("my-trials/"+parent+"/bookings",new{rowVersion=Convert.ToBase64String(new byte[8]),batchKey=Guid.NewGuid(),sessionIds=new[]{session}},false)).StatusCode==HttpStatusCode.BadRequest,"trial booking requires CSRF");
   check((await Post("check-in",new{token="bad"})).StatusCode==HttpStatusCode.Conflict,"invalid scan returns recoverable error");
   check((await staff.GetAsync("/Classroom/Errors")).IsSuccessStatusCode,"admin anomaly history renders");
   if(ui){
    var fixture = await PortalBrowserFixture.CreateAsync(createDb, admin);
    var marker=Path.Combine(dir.FullName,"artifacts","qr-trial-ui.json");var done=marker+".done";
    if(File.Exists(done))File.Delete(done);
    await File.WriteAllTextAsync(marker,JsonSerializer.Serialize(new{api=url,token,session,parent,account="qr-student",admin="qr-admin",teacher="qr-teacher",password,done,fixture}));Console.WriteLine("QR_TRIAL_UI_READY "+marker);
    try {
     var until=DateTime.UtcNow.AddMinutes(20);while(!File.Exists(done)&&DateTime.UtcNow<until)await Task.Delay(500);
     if (!File.Exists(done)) throw new TimeoutException("隔離瀏覽器驗證逾時，未宣稱通過。");
     await fixture.VerifyAsync(createDb, check);
    } finally { File.Delete(marker); if(File.Exists(done)) File.Delete(done); }
   }
  }finally{if(!process.HasExited)process.Kill(true);await process.WaitForExitAsync();}
 }
}
