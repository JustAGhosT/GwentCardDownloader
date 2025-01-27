# Gwent Card Downloader

A modern .NET Core application for downloading card images from Gwent, the Witcher Card Game. Built with performance and reliability in mind, featuring concurrent downloads, robust error handling, and comprehensive progress tracking.

## ✨ Features

- 🎴 **Smart Card Downloads**: Efficiently downloads card images from gwent.one
- 📊 **Progress Tracking**: Real-time progress bars with detailed status information
- 🔄 **Resilient Operations**: 
  - Automatic retry mechanism with exponential backoff
  - Resume capability for interrupted downloads
  - Robust error handling
- 🖼️ **Image Processing**:
  - Multiple quality options
  - Automatic image verification
  - Optimized storage
- 📝 **Comprehensive Logging**: Detailed activity and error logging
- ⚡ **Performance Optimized**:
  - Concurrent downloads
  - Configurable rate limiting
  - Memory-efficient processing

## 🚀 Getting Started

### Prerequisites

- .NET 6.0 SDK or later
- Visual Studio 2019/2022 or VS Code (optional)

### Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/GwentCardDownloader.git
cd GwentCardDownloader
```

2. Install dependencies:
```bash
dotnet restore
```

3. Run the application:
```bash
dotnet run
```

## ⚙️ Configuration

### Command Line Arguments

```bash
dotnet run -- [baseUrl] [imageFolder] [delay]
```

| Parameter    | Description                                    | Default Value                |
|-------------|------------------------------------------------|----------------------------|
| baseUrl     | Base URL for card downloads                     | https://gwent.one/en/cards/ |
| imageFolder | Destination folder for downloaded images        | gwent_cards                |
| delay       | Delay between downloads (milliseconds)          | 100                        |

## 🏗️ Project Structure

```
GwentCardDownloader/
├── Program.cs                 # Application entry point
├── Models/
│   └── Card.cs               # Card data model
├── Services/
│   ├── DownloadManager.cs    # Manages concurrent downloads
│   ├── StateManager.cs       # Download state management
│   └── ErrorHandler.cs       # Error handling
└── Utils/
    ├── CommandLineParser.cs  # Command line argument parsing
    └── Logger.cs             # Logging implementation
```

## 🎯 Features in Detail

### Image Quality Options

| Quality Level | Resolution | Processing                    |
|--------------|------------|-------------------------------|
| Low          | 100x100px  | Grayscale                    |
| Medium       | 200x200px  | Brightness adjusted          |
| High         | 300x300px  | Color enhanced               |

### Progress Tracking

- Overall download progress with percentage
- Individual card download status
- Current download speed
- Estimated time remaining
- Failed downloads count

### Error Handling

The application implements comprehensive error handling for:

- 🌐 Network connectivity issues
- 💾 File system operations
- 🖼️ Image processing errors
- 🔄 Rate limiting responses

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch:
```bash
git checkout -b feature/AmazingFeature
```
3. Commit your changes:
```bash
git commit -m 'Add some AmazingFeature'
```
4. Push to the branch:
```bash
git push origin feature/AmazingFeature
```
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Thanks to gwent.one for providing card data
- Built with .NET Core and the following packages:
  - NLog for logging
  - HtmlAgilityPack for HTML parsing
  - ShellProgressBar for console progress bars

## 📝 Note

This tool is for personal use only. Please respect gwent.one's terms of service and implement appropriate rate limiting in your downloads.
