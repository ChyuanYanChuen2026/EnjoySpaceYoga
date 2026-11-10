using System.Security;
using System.Text;
using EnjoySpace.Tools.FirstAdminSetup;
using EnjoySpace.Web.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

const string server = "sql-enjoyspace-prod.database.windows.net";
const string database = "EnjoySpaceProd";
const string sqlUser = "enjoyspaceadmin";

try
{
    if (args.Length != 0 || Console.IsInputRedirected)
        throw new InvalidOperationException("請在本機互動式 PowerShell 執行，不接受命令列參數或重新導向輸入。");

    Console.WriteLine("Enjoy Space：建立首位後台管理員");
    Console.WriteLine($"伺服器：{server}\n資料庫：{database}\nSQL 登入：{sqlUser}");
    Console.WriteLine("SQL 密碼與網站管理員密碼分別輸入；密碼不會顯示或寫入設定檔。");
    Console.Write("請輸入 SQL 登入密碼：");
    using var sqlPassword = ReadSqlPassword();
    var connectionString = new SqlConnectionStringBuilder
    {
        DataSource = "tcp:" + server + ",1433", InitialCatalog = database,
        Encrypt = SqlConnectionEncryptOption.Mandatory, TrustServerCertificate = false,
        ConnectTimeout = 30, Pooling = false, MultipleActiveResultSets = false,
        ApplicationName = "EnjoySpace.FirstAdminSetup"
    };
    await using var connection = new SqlConnection(connectionString.ConnectionString, new SqlCredential(sqlUser, sqlPassword));
    await connection.OpenAsync();
    await using (var command = connection.CreateCommand())
    {
        command.CommandText = "SELECT DB_NAME(), CONVERT(int, SERVERPROPERTY('EngineEdition'));";
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync() || reader.GetString(0) != database || reader.GetInt32(1) != 5)
            throw new InvalidOperationException("目前連線不是指定的 Azure SQL EnjoySpaceProd，已停止。");
    }
    var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connection).Options;
    await using var db = new AppDbContext(options);
    if (await db.UserRoles.AnyAsync(x => x.Role.RoleCode == "Admin"))
        throw new InvalidOperationException("資料庫已有管理員權限紀錄，首位管理員工具不會再次建立或覆蓋帳號。");

    Console.WriteLine("已確認 Azure SQL 目標。以下設定網站後台的管理員帳號。");
    Console.Write("請輸入新的網站管理員帳號（1～100 字）：");
    var account = Console.ReadLine() ?? "";
    Console.Write("請輸入網站管理員密碼（12～200 字元，不會顯示）：");
    var password = ReadPassword();
    Console.Write("再次輸入網站管理員密碼：");
    var confirmation = ReadPassword();
    var user = await FirstAdminInitializer.CreateAsync(db, account, password, confirmation);
    Console.WriteLine($"管理員已建立並提交。帳號：{user.Account}");
    Console.WriteLine("請使用剛才設定的網站帳號與密碼登入：");
    Console.WriteLine("https://app-enjoyspace-prod-heeghtdcbte9hyd8.eastasia-01.azurewebsites.net/Account/Login");
    return 0;
}
catch (SqlException ex)
{
    Console.Error.WriteLine($"SQL 作業未能確認完成（錯誤代碼 {ex.Number}）。請保留此代碼，檢查連線與建立結果後再繼續。");
    return 1;
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"初始化未能確認完成（{ex.GetType().Name}）。請先確認結果；工具不會覆蓋既有帳號。");
    return 1;
}

static SecureString ReadSqlPassword()
{
    var result = new SecureString();
    try
    {
        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                if (result.Length == 0) throw new InvalidOperationException("SQL 密碼不可空白。");
                result.MakeReadOnly();
                return result;
            }
            if (key.Key == ConsoleKey.Backspace && result.Length > 0) result.RemoveAt(result.Length - 1);
            else if (!char.IsControl(key.KeyChar)) result.AppendChar(key.KeyChar);
        }
    }
    catch { result.Dispose(); throw; }
}

static string ReadPassword()
{
    var result = new StringBuilder();
    while (true)
    {
        var key = Console.ReadKey(true);
        if (key.Key == ConsoleKey.Enter) { Console.WriteLine(); return result.ToString(); }
        if (key.Key == ConsoleKey.Backspace && result.Length > 0) result.Length--;
        else if (!char.IsControl(key.KeyChar)) result.Append(key.KeyChar);
    }
}
