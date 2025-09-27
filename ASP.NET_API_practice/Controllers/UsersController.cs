/// <summary>
/// 這裡是 API 的核心，負責處理所有跟「使用者」相關的請求。
/// 客戶端（例如手機 App 或網頁）會透過發送 HTTP 請求到這裡來操作使用者資料。
/// </summary>
using Microsoft.AspNetCore.Mvc;
using ASP.NET_API_practice.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ASP.NET_API_practice.Controllers
{
    [ApiController] // 標示這是一個 API 控制器
    [Route("api/[controller]")] // 設定路由規則，例如 /api/users
    public class UsersController : ControllerBase
    {
        // --- 模擬資料庫 ---
        // 在真實世界中，你會連接到像 SQL Server 或 MySQL 這樣的資料庫。
        // 為了教學方便，我們先用一個靜態的 List 來假裝是我們的使用者資料庫。
        private static List<User> _users = new List<User>
        {
            new User { Id = 1, Name = "鋼鐵人", Email = "ironman@avengers.com" },
            new User { Id = 2, Name = "美國隊長", Email = "captain@avengers.com" }
        };

        // 這是用來記錄日誌的工具，可以幫我們在後台看到程式的執行狀況
        private readonly ILogger<UsersController> _logger;

        // 透過「依賴注入」(Dependency Injection) 的方式取得 Logger 實例
        public UsersController(ILogger<UsersController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// [GET] /api/users
        /// 獲取所有使用者列表
        /// </summary>
        [HttpGet]
        public IActionResult GetUsers()
        {
            // --- 使用 try-catch 來捕捉潛在的錯誤 ---
            // 就像為程式碼買保險，如果 try 區塊裡的程式碼出錯了，
            // catch 區塊會立刻接手處理，防止程式崩潰。
            try
            {
                _logger.LogInformation("正在獲取所有使用者資料...");
                if (!_users.Any())
                {
                    _logger.LogWarning("資料庫中沒有任何使用者。");
                    return NotFound("找不到任何使用者。"); // 回傳 404 Not Found
                }
                _logger.LogInformation($"成功獲取了 {_users.Count} 位使用者。");
                return Ok(_users); // 回傳 200 OK 和使用者列表
            }
            catch (Exception ex)
            {
                // 記錄下詳細的錯誤訊息，方便工程師排查問題
                _logger.LogError(ex, "獲取所有使用者時發生了未預期的錯誤。");
                // 回傳一個通用的錯誤訊息給客戶端，避免洩漏系統細節
                return StatusCode(500, "哎呀！伺服器好像打瞌睡了，請稍後再試。");
            }
        }

        /// <summary>
        /// [GET] /api/users/{id}
        /// 根據 ID 獲取單一使用者
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            try
            {
                _logger.LogInformation($"正在搜尋 ID 為 {id} 的使用者...");
                var user = _users.FirstOrDefault(u => u.Id == id);

                if (user == null)
                {
                    _logger.LogWarning($"找不到 ID 為 {id} 的使用者。");
                    return NotFound($"抱歉，我們找不到 ID 為 {id} 的使用者。"); // 回傳 404 Not Found
                }

                _logger.LogInformation($"成功找到 ID 為 {id} 的使用者: {user.Name}。");
                return Ok(user); // 回傳 200 OK 和該使用者資料
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"獲取 ID 為 {id} 的使用者時發生錯誤。");
                return StatusCode(500, "哎呀！伺服器好像迷路了，請稍後再試。");
            }
        }

        /// <summary>
        /// [POST] /api/users
        /// 新增一位使用者
        /// </summary>
        [HttpPost]
        public IActionResult CreateUser([FromBody] User user)
        {
            // --- 驗證傳入的資料 ---
            // ModelState.IsValid 會自動根據 User Model 中的 [Required] 等屬性來檢查資料是否合格。
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("傳入的使用者資料驗證失敗。");
                return BadRequest(ModelState); // 回傳 400 Bad Request 和詳細的錯誤訊息
            }

            try
            {
                _logger.LogInformation($"準備新增使用者: {user.Name}...");
                // 模擬資料庫自動產生的 ID
                user.Id = _users.Any() ? _users.Max(u => u.Id) + 1 : 1;
                _users.Add(user);

                _logger.LogInformation($"成功新增使用者，ID 為 {user.Id}。");
                // 回傳 201 Created，並在 Header 中告訴客戶端新資源的位置
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"新增使用者 {user.Name} 時發生錯誤。");
                return StatusCode(500, "哎呀！伺服器新增資料時手滑了，請稍後再試。");
            }
        }

        /// <summary>
        /// [PUT] /api/users/{id}
        /// 更新指定 ID 的使用者資料
        /// </summary>
        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] User updatedUser)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"ID 為 {id} 的使用者更新資料驗證失敗。");
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation($"正在尋找並更新 ID 為 {id} 的使用者...");
                var user = _users.FirstOrDefault(u => u.Id == id);

                if (user == null)
                {
                    _logger.LogWarning($"嘗試更新一個不存在的使用者，ID: {id}。");
                    return NotFound($"找不到 ID 為 {id} 的使用者，無法更新。");
                }

                // 更新資料
                user.Name = updatedUser.Name;
                user.Email = updatedUser.Email;

                _logger.LogInformation($"成功更新 ID 為 {id} 的使用者資料。");
                return NoContent(); // 回傳 204 No Content，表示成功但沒有內容返回
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新 ID 為 {id} 的使用者時發生錯誤。");
                return StatusCode(500, "哎呀！伺服器更新資料時卡住了，請稍後再試。");
            }
        }

        /// <summary>
        /// [DELETE] /api/users/{id}
        /// 刪除指定 ID 的使用者
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                _logger.LogInformation($"正在尋找並刪除 ID 為 {id} 的使用者...");
                var user = _users.FirstOrDefault(u => u.Id == id);

                if (user == null)
                {
                    _logger.LogWarning($"嘗試刪除一個不存在的使用者，ID: {id}。");
                    return NotFound($"找不到 ID 為 {id} 的使用者，無法刪除。");
                }

                _users.Remove(user);
                _logger.LogInformation($"成功刪除 ID 為 {id} 的使用者。");

                return NoContent(); // 回傳 204 No Content
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"刪除 ID 為 {id} 的使用者時發生錯誤。");
                return StatusCode(500, "哎呀！伺服器刪除資料時出錯了，請稍後再試。");
            }
        }
    }
}
