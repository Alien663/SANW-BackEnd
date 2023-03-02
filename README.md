# This WebAPI Example
## SQL Connection String
location : appsettings.json > ConnectionStrings

## Model Folder
儲存所有Model用

## call API
已安裝Swagger，用於測試各API，啟動專案就可使用。

## other setting
在startup.cs中另外有其他設定
1. 靜態檔案(給前端網站使用): ./ClienApp
2. 雖然部屬環境是同一個站台，但是開發時會使用不同port進行開發，因此當環境為debug時，會自動設定跨網域localhost:3000
4. SQL連線依舊仰賴MS的SQLClient，只是資料存取使用dapper方便編寫程式碼
5. 資料進出盡量依照model設計，使資料結構嚴謹，維護時較容易理解程式碼邏輯，debug時可以依照以下寫法
6. AuthorizationFilter使用前綴判斷debug環境，開發時自動登入`MID=1`的帳號

```C#
    var data = db.Connection.Query(strSql, new { @id1, id2 }).ToList();
```