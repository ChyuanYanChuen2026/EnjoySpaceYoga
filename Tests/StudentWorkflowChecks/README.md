# 學生流程與隔離瀏覽器測試

本工具只使用 SQL Server Windows 驗證建立的隨機 `EnjoySpace_StudentWorkflowChecks_<GUID>` 資料庫，不接受既有資料庫名稱，也不讀取網站業務資料庫連線。原始碼中的虛構資料僅於該隔離資料庫執行期間建立，不提供日常 Demo 入口。

從 `EnjoySpace.Web` 目錄執行，需有本機 SQL Server 建立／刪除測試資料庫的權限。使用 `Rules` 組態，不必停止既有開發網站。

```powershell
dotnet run --project Tests/StudentWorkflowChecks --configuration Rules
dotnet run --project Tests/StudentWorkflowChecks --configuration Rules -- --http
```

第一個命令驗證隔離 schema 升級、體驗堂數、預約、權限、競態與打卡；第二個額外啟動隨機連接埠的測試 API，驗證登入、防偽與 HTTP 資料契約。API 明確指定隔離連線、停用課表自動同步與真實郵件服務。測試完成或失敗後均進入清理流程，停止測試 API 並刪除本次資料庫；請核對最後的 `Removed isolated database` 訊息。

## 瀏覽器驗證

1. 在後端執行 `dotnet run --project Tests/StudentWorkflowChecks --configuration Rules -- --ui`，等待 `QR_TRIAL_UI_READY`。
2. 臨時 API 位址、隨機密碼及帳號資訊只寫入被 Git 忽略的 `artifacts/qr-trial-ui.json`。不要貼到聊天、提交版本控制或保留為測試資料檔。此檔案包含 QR 憑證，僅供本機隔離驗證。
3. 在另一個 PowerShell 終端機切至 `EnjoySpace.Portal`，啟動專屬前台：

```powershell
$fixturePath = '../EnjoySpace.Web/artifacts/qr-trial-ui.json'
if (-not (Test-Path -LiteralPath $fixturePath)) { throw '尚未準備好隔離測試設定' }
$fixtureConfig = Get-Content -LiteralPath $fixturePath -Raw | ConvertFrom-Json
if (-not $fixtureConfig.api -or -not $fixtureConfig.fixture.Password) { throw '隔離測試設定不完整' }
$env:API_PROXY_TARGET = $fixtureConfig.api
npm run dev -- --host 127.0.0.1 --port 4401
```

4. 瀏覽 `http://localhost:4401`，與平常的 `http://127.0.0.1:4000` 分開使用。依 JSON 的 `fixture` 帳號資訊登入；密碼不會列在終端機輸出。補課帳號完成跨學期目標選擇、送出及取消，確認資格恢復；體驗帳號完成兩堂批次預約及一堂單堂預約，再逐堂取消，確認恢復三堂。
5. 在後端目錄建立完成標記，觸發資料核對及清理：

```powershell
New-Item -Path artifacts/qr-trial-ui.json.done -ItemType File -Force
```

程式核對取消歷史、資格釋回、剩餘堂數及待審未建立名單。成功才回報通過，然後移除臨時帳號設定、標記及測試資料庫。20 分鐘逾時會回報失敗並清理。最後以 Ctrl+C 停止專屬 4401 前台，關閉該終端機，避免沿用其測試 API 環境變數。

## 版本控制範圍

後端 `.gitignore` 僅允許本目錄的 `Program.cs`、`HttpChecks.cs`、`PortalBrowserFixture.cs`、`StudentWorkflowChecks.csproj` 與本說明；專案也只編譯這三個 C# 原始碼。臨時帳號 JSON、測試資料輸出、`bin/`、`obj/`、日誌，以及舊的 `DevelopmentUpgrade.cs` 均不納入。

只支援無參數、`--http` 或 `--ui`，其他參數在建立連線前即拒絕，包括舊的 `--apply-development`。本工具沒有既有業務資料庫更新入口；Migration 僅在本次隨機測試資料庫驗證。
