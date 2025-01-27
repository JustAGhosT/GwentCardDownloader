# Gwent Card Downloader

A .NET Core application that downloads card images from Gwent, the Witcher Card Game. This tool features concurrent downloads, progress tracking, image optimization, and robust error handling.

## Features

- 🎴 Downloads card images from gwent.one
- 📊 Real-time progress tracking with progress bars
- 🔄 Automatic retry mechanism for failed downloads
- 💾 State management and resume capability
- 🖼️ Image optimization with multiple quality options
- 📝 Comprehensive logging
- ⚡ Concurrent downloads with rate limiting

## Prerequisites

- .NET 6.0 SDK or later
- Visual Studio 2019/2022 or VS Code (optional)

## Quick Start

1. Clone the repository:
```sh
git clone https://github.com/yourusername/GwentCardDownloader.git
cd GwentCardDownloader
```

2. Install dependencies:
```sh
dotnet restore
```

3. Run the application:
```sh
dotnet run
```

## Configuration

The application can be configured using command-line arguments:

```sh
dotnet run -- [baseUrl] [imageFolder] [delay]
```

- `baseUrl`: Base URL for card downloads (default: https://gwent.one/en/cards/)
- `imageFolder`: Destination folder for downloaded images (default: gwent_cards)
- `delay`: Delay between downloads in milliseconds (default: 100)

## Project Structure

```
GwentCardDownloader/
├── Program.cs                 # Application entry point
├── Card.cs                    # Card model
├── Downloader.cs             # Main download logic
├── DownloadManager.cs        # Manages concurrent downloads
├── DownloadProgress.cs       # Progress tracking
├── ImageProcessor.cs         # Image optimization
├── StateManager.cs           # Download state management
└── ErrorHandler.cs           # Error handling
```

## Features in Detail

### Image Quality Options

The downloader supports three quality levels:
- Low: 100x100px, grayscale
- Medium: 200x200px, brightness adjusted
- High: 300x300px, color enhanced

### Progress Tracking

The application provides real-time progress information:
- Overall download progress
- Individual card download status
- Download speed and estimated time remaining

### Error Handling

Robust error handling for:
- Network issues
- File system errors
- Image processing problems
- Automatic retries with exponential backoff

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Thanks to gwent.one for providing card data
- Built with .NET Core and various open-source packages
