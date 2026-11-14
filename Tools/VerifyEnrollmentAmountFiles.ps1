#requires -Version 5.1
param([switch]$RequireTracked)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$files = @(
    'Models/Entities/EnrollmentAmountAdjustment.cs',
    'Data/AppDbContext.cs',
    'Data/EnrollmentModelConfiguration.cs',
    'Services/EnrollmentWorkflowService.cs',
    'Areas/Admin/ViewModels/EnrollmentWorkflowModels.cs',
    'Areas/Admin/Controllers/EnrollmentApplicationsController.cs',
    'Areas/Admin/Views/EnrollmentApplications/Details.cshtml',
    'Areas/Admin/Views/EnrollmentApplications/Index.cshtml',
    'Controllers/PortalEnrollmentsController.cs',
    'Migrations/20261113182516_AddEnrollmentAmountAdjustments.cs',
    'Migrations/20261113182516_AddEnrollmentAmountAdjustments.Designer.cs',
    'Migrations/AppDbContextModelSnapshot.cs',
    'Tests/EnrollmentAmountChecks/EnrollmentAmountChecks.csproj',
    'Tests/EnrollmentAmountChecks/Program.cs'
)
foreach ($file in $files) {
    if (-not (Test-Path -LiteralPath (Join-Path $root $file) -PathType Leaf)) { throw "缺少必要檔案：$file" }
    if ($RequireTracked) {
        $tracked = @(& git -C $root ls-files -- $file)
        if ($LASTEXITCODE -ne 0) { throw '無法查詢 Git 追蹤狀態，請確認專案目錄。' }
        if ($tracked.Count -eq 0) { throw "檔案尚未納入 Git，提交前請補齊：$file" }
    }
}
Write-Output "應收調整功能的 $($files.Count) 個必要檔案檢查通過。"
if (-not $RequireTracked) { Write-Output '本次僅檢查檔案存在；提交前請加上 -RequireTracked，確認新檔已納入 Git。' }
