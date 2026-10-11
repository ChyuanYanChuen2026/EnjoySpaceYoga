# 課堂名單與出缺席

## 使用方式

後台「場次管理 → 名單／點名」，或從行事曆開啟場次詳細後選「課堂名單與出缺席」。

- 只有已核准且有效的固定期課報名會建立逐堂名單；待付款、待核准申請不會加入。
- 核准報名、正式入班、每堂名單與未點名紀錄在同一筆交易完成，失敗時一起回復。
- 預設為 Scheduled（未點名）；不會依是否繳款或是否過了上課時間推定出席或缺席。
- 管理員可逐人登記 Present（出席）、Late（遲到）、Absent（缺席），也能更正回未點名。
- 課堂開始前只能查看名單。手動登記不會偽造實際到場時間；保存的是最後登記時間、登記人與備註。
- 更改已登記的狀態須填原因，備註最多 500 字。每人分別儲存，畫面顯示最新登記人與台灣時間。
- 舊頁面的資料版本不能覆蓋新紀錄；資料已變更時，重新核對頁面再送出。
- 名單或正式報名不再有效時，保留舊紀錄但禁止點名。

## 資料庫更新

Migration：`20261008072606_AddSessionParticipantsAndAttendance`。

新增 `ClassSessionParticipants` 與 `AttendanceRecords`，替既有 Active／Approved 正式報名補建各場次名單，歷史出缺席全部維持未點名。

本階段模型只實作固定期課來源，外鍵依現有 MVC 資料庫的 `ClassSessions.Id`（int）。原始 29 表 SQL 為設計參考，不能直接與現有 EF migrations 混用；Trial、SessionBooking、MakeupRequest 及 GPS 欄位需隨後續功能擴充。

更新指令：`dotnet ef database update`。回復此 migration 會刪除新增的名單及點名資料，不適合用來回復已有點名的環境。

## 驗證

`dotnet run --project Tests/WorkflowChecks --configuration Rules` 使用獨立測試資料庫驗證既有報名、排課與新增出缺席流程。
