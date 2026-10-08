# ReIcon Pro - Desktop Icon Layout Manager

**ReIcon Pro** is a modern, lightweight, self-contained Windows utility for saving, managing, and restoring desktop icon layouts across screen resolution changes, multi-monitor setups, and display scaling adjustments.

![ReIcon Pro UI](https://raw.githubusercontent.com/placeholder/reicon-pro.png)

---

## ✨ Features

- **🚀 Zero Installation / Single-File Executable:** Self-contained executable — double-click to run on any Windows 10/11 x64 PC without installing .NET runtime or SDKs.
- **⚡ Modern Desktop Traversal:** Native Win32 interop handling `Progman` and dynamic `WorkerW` parent windows seamlessly.
- **🔍 64-Bit Cross-Process Memory Injection:** Accurate icon coordinate extraction (`LVM_GETITEMPOSITION`) and unicode label reading (`LVM_GETITEMTEXTW`).
- **🛡️ Smart Screen Clamping:** Automatically bounds coordinates to currently active screen dimensions so icons never get lost off-screen after resolution downscaling.
- **⚙️ Auto-Arrange Safety:** Automatically detects and safely handles Windows `LVS_AUTOARRANGE` state during restoration.
- **🎨 Modern Dark Mode Dashboard:** Clean WPF user interface with real-time feedback and profile management.

---

## 🚀 How to Run

1. Go to the [Releases](../../releases) page.
2. Download `DesktopIconLayoutManager.exe`.
3. Double-click to run! (No installation or runtime setup required).

---

## 🛠️ Building from Source

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Build Commands

```powershell
# Clone the repository
git clone https://github.com/your-username/ReIcon-Pro.git
cd ReIcon-Pro

# Build and Publish Single-File Standalone Executable
dotnet publish IconManager.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o ./dist
```

---

## 📄 License

MIT License. Free for personal and commercial use.
