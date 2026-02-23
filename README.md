# AutoShut for Blender

<div align="center">
  <img src="AutoShut/Resources/AppIcon/appicon-512.png" alt="AutoShut Logo" width="128"/>
  
  ### Blender Render Queue Manager with Auto-Shutdown
  
  ![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-10-512BD4?style=flat-square)
  ![Platform](https://img.shields.io/badge/Platform-Windows-blue?style=flat-square)
  ![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)
</div>

---

## 📖 About

**AutoShut** is a Windows desktop application built with .NET MAUI that allows you to queue multiple Blender projects for sequential rendering. Perfect for overnight rendering sessions, it includes an optional auto-shutdown feature to turn off your computer once all renders are complete.

### ✨ Key Features

- 🎬 **Batch Rendering** - Queue multiple `.blend` files for sequential rendering
- 🖼️ **Flexible Output** - Choose between image or animation rendering for each file
- 📊 **Progress Tracking** - Visual progress indicators for each render job
- 💤 **Auto-Shutdown** - Optionally shut down your computer after all renders complete
- 🎨 **Modern UI** - Clean, dark-themed interface
- 🔄 **Drag & Drop** - Easy file management with drag-and-drop support

---

## 🚀 Getting Started

### Prerequisites

- Windows 10/11 (64-bit)
- [.NET 10 Runtime](https://dotnet.microsoft.com/download)
- [Blender](https://www.blender.org/download/) installed on your system

### Installation

1. Download the latest installer from the [Releases](../../releases) page
2. Run `AutoShut-Setup-v1.1.exe`
3. Follow the installation wizard
4. Launch **AutoShut for Blender** from the Start Menu

---

## 💡 How to Use

1. **Add Blender Files**
   - Drag & drop `.blend` files into the application
   - Or use the "Browse Files" button to select files

2. **Configure Render Type**
   - Choose "Image" or "Animation" for each file
   - Select the appropriate render type from the dropdown

3. **Start Rendering**
   - Click "Start Rendering" to begin the queue
   - Monitor progress in real-time
   - Optionally enable "Auto Shutdown" to turn off your PC when complete

4. **Manage Queue**
   - Remove files from the queue with the delete button
   - Retry failed renders with the retry button
   - Clear all files with "Clear List"

---

## 🛠️ Building from Source

### Prerequisites for Development

- [Visual Studio 2022+](https://visualstudio.microsoft.com/) with .NET MAUI workload
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Inno Setup 6](https://jrsoftware.org/isdl.php) (for creating installers)

### Build Instructions

```bash
# Clone the repository
git clone https://github.com/APihery/AutoShut.git
cd AutoShut

# Build the application
.\Build-Windows.bat

# Or build everything (app + installer)
.\Build-All.bat
```

### Build Scripts

- `Build-Windows.bat` - Build the Windows application
- `Create-Installer.bat` - Create the installer (requires Inno Setup)
- `Build-All.bat` - Build everything in one go

---

## 📦 Project Structure

```
AutoShut/
├── AutoShut/                   # Main .NET MAUI project
│   ├── Models/                 # Data models
│   ├── ViewModels/             # MVVM view models
│   ├── Services/               # Business logic (Blender rendering)
│   ├── Behaviors/              # UI behaviors (hover effects)
│   ├── Resources/              # Images, fonts, icons
│   ├── MainPage.xaml           # Main UI
│   └── AppShell.xaml           # Application shell
├── Build/                      # Build output (gitignored)
├── Build-Windows.bat           # Build script
├── Create-Installer.bat        # Installer creation script
├── Build-All.bat               # Complete build script
└── AutoShut-Setup.iss          # Inno Setup configuration
```

---

## 🎨 Technologies Used

- **.NET MAUI** - Cross-platform UI framework
- **C#** - Primary programming language
- **XAML** - UI markup
- **Blender CLI** - Command-line rendering
- **Inno Setup** - Windows installer creation

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👨‍💻 About the Developer

**Aymeric PIHERY**

Software Engineer specializing in 3D technologies and .NET development, combining technical expertise with creative solutions.

- 🌐 **Portfolio**: [aymeric-pihery.fr/](https://aymeric-pihery.fr/)
- 💼 **LinkedIn**: [linkedin.com/in/aymeric-pihery](https://www.linkedin.com/in/aymeric-pihery/)
- 🎨 **Specialties**: C# .NET, Blender, Unity, Unreal Engine, 3D Modeling

---

## 🙏 Acknowledgments

- Built with [.NET MAUI](https://dotnet.microsoft.com/apps/maui)
- Icons created with Blender
- Installer created with [Inno Setup](https://jrsoftware.org/isinfo.php)

---

<div align="center">
  Made with ❤️ by <a href="https://aymeric-pihery.fr/">Aymeric PIHERY</a>
</div>
