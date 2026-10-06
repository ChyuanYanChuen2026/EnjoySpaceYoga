# 報名與排課流程整合檢查

執行 `dotnet run --project Tests/WorkflowChecks --configuration Rules`。

使用本機 SQL Server 的 Windows 驗證，建立隨機名稱的獨立測試資料庫，完成後自動移除。帳號需有建立資料庫權限。可用 `ENJOYSPACE_TEST_SERVER` 指定測試伺服器；程式不接受既有資料庫名稱，也不讀取應用程式的開發資料庫連線設定。

檢查包含學期完整排課與確認、停止報名後的受控草稿還原、場次刪除及版本衝突、確認頁資料防竄改、重複報名、三階段確認、已核准報名禁止退回、舊報價堂數衝突，以及資料庫端篩選／排序／分頁。
