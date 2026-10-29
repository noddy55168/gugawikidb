# Guga資料站

一個 Wiki 網頁練習專案，分為「C# 後端＋Vue 前端」與「GitHub Pages 靜態閱讀版」。

此倉庫用於發布靜態閱讀版：由 C# 匯出公開資料，再由 Vue 建置網站。倉庫包含建置完成的網站，以及 C#（.NET 10）發布驗證工具。GitHub Pages 只提供靜態檔案，不執行 C# 後端。

## 網站連結

- [Guga資料站](https://noddy55168.github.io/gugawikidb/)
- [幹員詳情範例](https://noddy55168.github.io/gugawikidb/#/operators/4)

## 手動發布

1. 檢查準備好的檔案，提交（commit）至 `main`，再推送（push）到 GitHub。
2. 首次設定時，到 **Settings → Pages → Build and deployment**，將 **Source** 設為 **GitHub Actions**。
3. 開啟 **Actions → Publish Guga資料站 → Run workflow**，選擇 `main` 並執行。
4. 等待 `package` 與 `deploy` 都成功，再開啟部署結果提供的網址。

只推送倉庫不會自動更新網站；目前僅透過 `workflow_dispatch` 手動發布。

發布流程會使用 C# 檢查檔案清單與 SHA256，通過後只上傳 `site/`。此流程不重新建置 Vue、不連接資料庫，也不需要自行提供個人存取權杖（Personal Access Token）。

## 更新網站內容

1. 在開發專案完成內容或介面修改。
2. 使用 `/gugawikidb/` 作為 base，產生並驗證完整的新版靜態封包。
3. 以新版封包完整替換 `site/`，包含移除已不再使用的舊檔案；不要混用不同版本的 JavaScript 與資料清單。
4. 檢查變更後 commit、push，再依上方步驟手動發布。

請保留 `.gitattributes`，避免 Git 的換行轉換改變成品內容，造成雜湊驗證失敗。

安裝 .NET 10 SDK 後，也可以在倉庫根目錄執行本機驗證：

```powershell
dotnet run --project tools/PagesReleaseCheck/PagesReleaseCheck.csproj --configuration Release -- site
```

`site/release.json` 記錄資料版本、base、建置時間及檔案雜湊。

若要回復舊版，請將先前已驗證的完整 `site/` 封包還原為一筆新的提交，推送後再次手動發布。

## 功能與內容範圍

此版本是公開資料的閱讀快照，不提供帳戶、編輯或管理功能。

資料庫檔案、憑證、完整研究匯出及私人開發文件不應放入此發布倉庫。

遊戲名稱、美術圖片及其他遊戲資產的權利，歸各自的權利人所有。

特別感謝以下協助：

[https://github.com/Variante](https://github.com/Variante)

