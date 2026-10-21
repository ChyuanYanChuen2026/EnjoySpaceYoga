# 老師登入與忘記密碼

## 管理員操作

1. 進入後台「老師管理」，新增老師或編輯現有老師。
2. 在編輯頁填寫老師可收信的 Email，勾選「啟用老師身分」及「啟用登入帳號」，儲存。
3. 複製編輯頁顯示的老師登入帳號，提供給老師。
4. 老師開啟 `/Account/Login`，選擇「忘記密碼／首次設定密碼」，填入帳號及登記的 Email。
5. 老師到 Email 開啟驗證連結，設定新密碼，再回登入頁登入。

老師登入後進入 `/TeacherRequests`，使用原有加開課、超收申請功能。管理員審核及其他 Admin 頁面仍僅限管理員。新老師預設不開放登入，需完成以上啟用步驟。忘記密碼流程不會啟用已停用的帳號或老師身分。

## SMTP 設定（完成後才能實際寄信）

在 Visual Studio 對 EnjoySpace.Web 專案按右鍵「管理使用者密碼」，將以下區段合併到本機 secrets.json；保留既有的其他設定。不要將密碼寫入 Git 或貼到聊天中。

```json
{
  "PasswordRecovery": {
    "PublicBaseUrl": "http://localhost:5209/",
    "Host": "smtp.your-provider.example",
    "Port": 587,
    "EnableSsl": true,
    "FromAddress": "your-verified-sender@example.com",
    "FromName": "Enjoy Space 米亞工作室",
    "UserName": "your-smtp-username",
    "Password": "填入寄信服務提供的 SMTP 密碼"
  }
}
```

請用服務商提供的真實資料替換範例。此實作使用 STARTTLS（通常為 587），不支援僅提供 implicit TLS 465 的端點，也不支援 OAuth-only 登入。請選擇支援 STARTTLS 與 SMTP 帳密（或 SMTP API Key 作為密碼）的服務端點；若服務商僅提供其他方式，需要更換寄信傳輸實作。寄件者須通過服務商的驗證。

`PublicBaseUrl` 必須是收件者能開啟的網站根網址，結尾保留 `/`。本機開發可用 `http://localhost:5209/`，但這類連結只能在執行網站的同一台電腦使用；給老師使用時須改為已部署的正式 HTTPS 網址。連結不會根據外部傳入的 Host header 產生。

本機 User Secrets 僅在 Development 載入，不是加密的秘密保管庫。正式部署請透過主機秘密設定或環境變數設定，例如 `PasswordRecovery__Host`、`PasswordRecovery__Password`、`PasswordRecovery__PublicBaseUrl`，並持久保存 ASP.NET Core Data Protection 金鑰，多台主機須共用金鑰及應用程式識別設定。

修改設定後重新啟動網站。以你控制的有效管理員或老師帳號，申請一封信確認：收件、連結網址、設定新密碼及重新登入。收到驗證信前不能視為真實寄信驗收完成。SMTP 未設定時頁面會提示服務尚未完成；寄送異常會記錄不含信箱、驗證連結或密碼的警告。為避免洩漏帳號是否存在，送出頁面使用相同訊息；若信件未到請檢查服務商紀錄與垃圾郵件。

## 功能規則

- 目前此入口適用於有效的管理員／老師帳號；學生帳號不適用此後台流程。
- 驗證連結有效 30 分鐘；開啟連結不會消耗它，成功設定密碼後才失效。
- 連結綁定帳號、Email、密碼版本及資料版本，資料變更後請重新申請。
- 新密碼為 10～200 字，兩次輸入需一致；密碼僅存雜湊。
- SQL rowversion 防止同一連結同時送出而重複重設；無需新增資料表或 migration。
- 重設後原有 MVC 與官網登入憑證會在下一次請求失效。首次部署此版本後，既有登入者也需要重新登入。
- 寄信與重設 POST 使用防偽驗證，並共用每個 IP 每分鐘最多 5 次的限制；超過會回傳 HTTP 429。
- 自動測試使用隔離資料庫及記憶體信件替身，不寄送真實信件。

## 驗證指令

```powershell
dotnet build EnjoySpace.Web.csproj --configuration Workflow --no-restore
dotnet run --project Tests/WorkflowChecks --configuration Rules --no-restore -- --http
```
