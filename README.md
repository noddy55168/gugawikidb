# Guga資料站

一個 Wiki 網頁練習專案，分為「C# 後端＋Vue 前端」與「GitHub Pages 靜態閱讀版」。

此倉庫用於發布靜態閱讀版：由 C# 匯出公開資料，再由 Vue 建置網站。倉庫包含建置完成的網站，以及 C#（.NET 10）發布驗證工具。GitHub Pages 只提供靜態檔案，不執行 C# 後端。

## 網站連結

- [Guga資料站](https://noddy55168.github.io/gugawikidb/)
- [幹員詳情範例](https://noddy55168.github.io/gugawikidb/#/operators/4)

## 設計思路與實現

本專案以 C# 與 Vue 為主要技術，目的是練習從資料整理、後端設計到前端呈現與網站發布的完整流程，並建立方便查閱的遊戲資料站。

### 1. C# 資料流程與後端架構

C# 負責從資料匯入到 API 提供的完整流程：透過 **DataImporter** 解析與驗證來源資料，再使用 **SqlSugar** 存取資料庫。

後端採分層設計，由 **Repository** 負責資料存取、**Service** 處理業務邏輯、**Controller** 對外提供 API，並透過 **DI（依賴注入）** 組合各層服務，方便維護與測試。另設獨立驗證工具，檢查資料與讀取結果。

一般版提供帳戶、分級授權及內容修訂功能；靜態版則由 **StaticExport** 匯出公開 JSON，供共用 Vue 前端閱讀。

### 2. Vue 畫面與操作

Vue 專注於畫面呈現與操作互動。讓資料來源、處理方式與顯示結果能夠對照，減少重複維護與內容不一致的情況。

### 3. 共用前端，支援兩種版本

網站保留一般後端版與 GitHub Pages 靜態閱讀版。兩個版本共用同一套前端，透過建置模式決定讀取 **API 或 JSON**，讓介面修改能同步套用，也避免日後維護兩份畫面。

### 4. 視覺風格與查找體驗

介面參考遊戲的視覺風格，同時重視閱讀與查找便利性。透過圖鑑分類、搜尋、篩選及一致的圖示，讓使用者更容易找到需要的資料。

### 5. 分階段確認與手動發布

發布採分階段確認與手動操作。靜態版只輸出公開閱讀內容，發布前檢查檔案完整性；確認後再更新網站，並保留舊版作為回復依據。

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

遊戲名稱、美術圖片及其他遊戲資產的權利，歸各自的權利人所有。

## 特別感謝

特別感謝以下協助：

[https://github.com/Variante](https://github.com/Variante)
