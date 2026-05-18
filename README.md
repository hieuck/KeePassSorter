# KeePass Sorter Plugin 🚀

[![Build KeePass Sorter Plugin](https://github.com/hieuck/KeePassSorter/actions/workflows/build.yml/badge.svg)](https://github.com/hieuck/KeePassSorter/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![KeePass](https://img.shields.io/badge/KeePass-2.x-blue.svg)](https://keepass.info)

A professional C# plugin for **KeePass 2.x** that allows you to easily sort credentials inside groups based on multiple custom criteria. Includes full support for Vietnamese sorting (accented characters).

*Một plugin chuyên nghiệp cho KeePass 2.x hỗ trợ sắp xếp các mục dữ liệu mật khẩu một cách nhanh chóng theo nhiều tiêu chí tùy chỉnh, có tích hợp bộ lọc sắp xếp chuẩn Tiếng Việt.*

---

## ✨ Features / Tính năng nổi bật

* **Flexible Sorting Criteria / Đa dạng tiêu chí sắp xếp:**
  * Title / Tiêu đề (A → Z, Z → A)
  * Username / Tên người dùng
  * URL
  * Creation Time / Thời gian tạo
  * Last Modification Time / Thời gian sửa đổi gần nhất
  * Notes / Ghi chú
* **Advanced Options / Tuỳ chọn nâng cao:**
  * **Recursive Sorting / Sắp xếp đệ quy:** Sorts entries in the current group and all its sub-groups.
  * **Case Sensitive / Phân biệt chữ hoa/thường:** Choose whether to differentiate upper/lower case.
  * **Vietnamese Alphabet Support / Hỗ trợ Tiếng Việt:** Sorts Vietnamese unicode strings with correct dictionary order (taking care of accents like `á, à, ả, ã, ạ, đ, ê...`).
* **CI/CD Automated Builds / Tự động biên dịch:**
  * Pre-configured GitHub Actions to automatically compile both `.dll` and `.plgx` versions on every push!

---

## 📦 Directory Structure / Cấu trúc thư mục

The repository is organized following clean architectural practices:
* `.github/workflows/build.yml` - CI/CD pipeline automation script.
* `src/` - Contains the source code of the plugin (C# 4.0 / C# 5.0 compatible).
* `KeePassSorter.plgx` - Compiled PLGX package directly placed at the root level.
* `.gitignore` - Standard Visual Studio and build output exclude rules.

---

## 🚀 Installation & Usage / Hướng dẫn Cài đặt & Sử dụng

### 1. Installation / Cài đặt
1. Download the latest **`KeePassSorter.plgx`** file from the GitHub Actions Artifacts or Release page.
2. Copy the `.plgx` file into the `Plugins` folder of your KeePass installation directory (e.g. `C:\Program Files\KeePass\Plugins\`).
3. Restart KeePass. It will automatically compile and load the plugin!

### 2. How to Use / Cách sử dụng
1. Open your KeePass database.
2. Select the group you want to sort in the left-hand group tree.
3. Go to the top menu: **Tools ➔ KeePass Sorter ➔ Sort Entries...** (hoặc **Công cụ ➔ KeePass Sorter ➔ Sort Entries...**).
4. Configure your desired sorting options in the dialog.
5. Click **Sắp xếp (Sort)**. Your entries will be sorted instantly and the database will be marked as modified so you can save your changes!

---

## 🛠️ Development & Local Build / Phát triển & Build Cục bộ

### Requirements
* Visual Studio 2012 or newer / MSBuild.
* KeePass 2.x installed locally.

### Local Build
To build the DLL version locally:
```bash
msbuild src/KeePassSorter.csproj /p:Configuration=Release
```
The compiled DLL will be located at `bin/Release/KeePassSorter.dll`.

To pack the PLGX version locally using KeePass:
```bash
& "path/to/KeePass.exe" --plgx-create "path/to/repo/src"
```
Rename the resulting `src.plgx` at the root folder to `KeePassSorter.plgx`.

---

## 📄 License / Giấy phép
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
