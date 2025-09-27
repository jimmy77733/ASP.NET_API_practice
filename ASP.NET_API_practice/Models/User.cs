/// <summary>
/// 這個檔案定義了「使用者」的資料結構。
/// 我們可以把它想像成是一個使用者的會員資料卡藍圖。
/// </summary>
using System.ComponentModel.DataAnnotations; // 引入這個命名空間來使用「資料驗證」功能

namespace ASP.NET_API_practice.Models
{
    public class User
    {
        // 使用者的唯一識別碼
        public int Id { get; set; }

        // [Required] 是一個「屬性」(Attribute)，它告訴 ASP.NET Core 這個欄位是必填的。
        // 如果傳入的資料少了這個欄位，就會驗證失敗。
        [Required(ErrorMessage = "使用者名稱是必要的！不能不給喔！")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "名稱長度必須介於 2 到 40 個字元之間。")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Email 是必要的！不然我怎麼寄信給你？")]
        [EmailAddress(ErrorMessage = "Email 格式有問題，請檢查一下。")]
        public string? Email { get; set; }
    }
}
