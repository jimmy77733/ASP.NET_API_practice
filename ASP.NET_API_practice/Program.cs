/// <summary>
/// 這是應用程式的進入點，所有服務和中間件都在這裡註冊和設定。
/// 想像一下這裡是整個 API 專案的總開關和控制面板。
/// </summary>
using Microsoft.AspNetCore.HttpLogging; // 引入 HTTP 日誌記錄功能

var builder = WebApplication.CreateBuilder(args);

// --- 註冊服務 (把你需要的工具都先準備好) ---

// 1. 加入 HTTP 日誌記錄服務 (AddHttpLogging)
//    這能幫我們把所有進來的請求細節都記錄下來，方便除錯。
builder.Services.AddHttpLogging(logging =>
{
    // 設定要記錄的內容，HttpLoggingFields.All 表示全部都記！
    logging.LoggingFields = HttpLoggingFields.All;
});

// 2. 加入身分驗證服務 (AddAuthentication)
//    告訴系統我們要開始使用身分驗證機制了。
//    注意：這裡只是註冊服務，實際的驗證工作由後面的「中間件」完成。
//    "Bearer" 是一種常見的驗證方案，通常與 JWT (JSON Web Tokens) 搭配使用。
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(); // 這裡我們假設未來會使用 JWT，先加上這個設定。

// 3. 加入控制器服務 (AddControllers)
//    這樣我們的 UsersController 才能被系統識別和使用。
builder.Services.AddControllers();

// 4. 加入 API 探索和 Swagger 服務，這會產生一個超酷的 API 測試頁面
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- 設定 HTTP 請求管道 (設定請求進來後要經過的關卡) ---
// 這裡的順序「超級重要」！請求會像流水線一樣，依序通過每一個 Use...() 中間件。

// 在開發模式下，啟用 Swagger 相關工具
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 關卡1：強制使用 HTTPS (UseHttpsRedirection)
// 所有 HTTP 請求都會被自動導向到 HTTPS，確保通訊安全。
app.UseHttpsRedirection();

// 關卡2：路由 (UseRouting)
// 幫請求找到它該去的地方，例如看到 /api/users 就知道要去 UsersController。
app.UseRouting();

// 關卡3：身分驗證 (UseAuthentication) - 「你是誰？」
// 檢查請求有沒有帶「身分證」(例如 Token)，並試圖解析出使用者的身分。
// 必須在 UseAuthorization (授權) 之前！要先知道你是誰，才能決定你有沒有權限。
app.UseAuthentication();

// 關卡4：授權 (UseAuthorization) - 「你有哪些權限？」
// 根據解析出來的身分，判斷使用者是否有權限執行請求的動作。
app.UseAuthorization();

// 關卡5：HTTP 日誌記錄 (UseHttpLogging) - 「把所有事情都記下來！」
// 放在這裡，是因為它不僅能記錄請求的內容，還能記錄經過驗證和授權後的使用者是誰。
// 如果放在 UseAuthentication 前面，日誌裡就看不到使用者資訊了。
app.UseHttpLogging();

// 終點站：將請求對應到具體的 Controller Action (MapControllers)
// 根據路由結果，執行對應的程式碼，例如 GetUsers() 或 CreateUser()。
app.MapControllers();

app.Run();
