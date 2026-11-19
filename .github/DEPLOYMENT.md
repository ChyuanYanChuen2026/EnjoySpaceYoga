# 後端自動驗證與手動部署

`.github/workflows/main_app-enjoyspace-prod.yml` 保留原本的工作流程檔名，以取代 main 現有的自動部署流程；不是增加第二個可同時部署的入口。

## 觸發行為

| 事件 | 測試／建置 | 部署 |
| --- | --- | --- |
| main 的 push | 執行 | 跳過 |
| 目標為 main 的 PR | 執行 | 跳過 |
| 手動執行，未勾 deploy | 執行 | 跳過 |
| 手動執行，非 main 或確認字不符 | 明確拒絕部署要求 | 不執行 |
| 手動執行 main，勾 deploy 且確認目標正確 | 必須通過 | 才執行 |

驗證 job 使用 Windows 2022 runner 的 SQL Server LocalDB 建立 CI 專用 `EnjoySpaceCi` 執行個體。測試程式仍只建立隨機 `EnjoySpace_StudentWorkflowChecks_` 資料庫；`ENJOYSPACE_TEST_SERVER` 僅可選伺服器，不可指定既有資料庫名稱。若 LocalDB 不可用則失敗，不跳過測試，也不改用業務連線。完成後清理測試資料庫及執行個體。

驗證命令：

```powershell
dotnet run --project Tests/StudentWorkflowChecks --configuration Rules -- --http
dotnet build EnjoySpace.Web.csproj --configuration Release
dotnet publish EnjoySpace.Web.csproj --configuration Release --no-build --output artifacts/publish
```

publish 只產生檔案，不連線更新資料庫或部署。專案排除 Tests、Tools、artifacts，且 `appsettings.Development.json` 不隨網站發布。CI artifact 保留七天，名稱包含目前 SHA；手動部署只下載同一次 run 的產物。

## 日後手動發布

1. 工作流程進入 main／預設分支後，到 GitHub Actions 選「後端測試建置與手動部署」。
2. 按 Run workflow，分支選 `main`，勾選 `deploy`，在 `confirm_target` 填入 `app-enjoyspace-prod`。
3. 當次測試、建置及 publish 全部成功後，deploy job 才使用既有 Azure OIDC Secrets，發布至 `app-enjoyspace-prod` 的 Production slot。只有 deploy job 有 `id-token: write` 權限。
4. 核對 run 的 SHA 與預計發布版本。一般 push／PR 不會執行 Azure login 或部署。未勾 deploy 的手動執行也不部署。

沿用既有 `AZUREAPPSERVICE_CLIENTID_…`、`AZUREAPPSERVICE_TENANTID_…`、`AZUREAPPSERVICE_SUBSCRIPTIONID_…` Secrets 名稱，不將值寫入檔案。既有 Azure OIDC 信任設定及 Secrets 是否有效，仍須在將來獲准部署時確認；本機驗證不代表已實際登入 Azure。

## 啟用前核對

- 必須以本檔取代 main 同名的舊流程，不能讓舊的無條件部署版本並存。此修改基於 Dev；若與 main 同名工作流程發生 add/add 衝突，須保留本次受手動條件限制的版本，再驗證最終候選內容。
- 檢查其他 GitHub workflows、Azure Deployment Center 或外部 webhook 是否仍會自動部署。本次沒有登入雲端調整這些外部入口。
- 首次 main 的 CI 應通過測試與建置，deploy job 應為 skipped。必須在允許推送後才可驗證 GitHub hosted runner 的實際執行。
- 不含業務資料庫 Migration 更新；若日後發布涉及結構變更，另依既有資料庫確認流程處理。
