# My Portfolio Website

A modern, responsive portfolio website built with Blazor WebAssembly, showcasing my professional experience, projects, and skills.

## Features

- 🎨 Modern and responsive design with a custom color scheme
- 📱 Mobile-first approach for optimal viewing on all devices
- 🔍 Dynamic GitHub repository integration
- 🎯 Clean and intuitive navigation
- 🎭 Interactive components and smooth animations
- 📊 Skills showcase with visual indicators
- 📧 Contact form for easy communication

## Tech Stack

- **Frontend**: Blazor WebAssembly
- **Styling**: Bootstrap 5, Custom CSS
- **Icons**: Font Awesome
- **API Integration**: GitHub API
- **Deployment**: GitHub Pages

## Project Structure

```
MyPortfolio/
├── Components/
│   ├── Layout/           # Layout components (MainLayout, NavMenu)
│   ├── Pages/            # Page components (Home, Projects, About, Contact)
│   └── Shared/           # Shared components
├── Models/               # Data models
├── Services/             # Service classes (GitHubService)
├── wwwroot/              # Static files
│   ├── css/             # Stylesheets
│   └── images/          # Image assets
└── Program.cs           # Application entry point
```

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/MyPortfolio.git
   ```

2. Navigate to the project directory:
   ```bash
   cd MyPortfolio
   ```

3. Restore dependencies:
   ```bash
   dotnet restore
   ```

4. Run the application:
   ```bash
   dotnet run
   ```

5. Open your browser and navigate to `https://localhost:5001`

## Customization

### Color Scheme

The website uses a custom color scheme:
- Primary: `#B85042` (Terracotta)
- Background: `#E7E8D1` (Soft off-white)
- Secondary: `#A7BEAE` (Sage green)

To modify the colors, update the CSS variables in `wwwroot/css/app.css`.

### GitHub Integration

The Projects page automatically fetches and displays your GitHub repositories. To customize this:

1. Update the GitHub username in `Services/GitHubService.cs`
2. Modify the repository display in `Components/Pages/Projects.razor`

## Deployment

The website is configured for deployment to GitHub Pages. To deploy:

1. Push your changes to the `main` branch
2. GitHub Actions will automatically build and deploy the site

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contact

- GitHub: [@yourusername](https://github.com/yourusername)
- LinkedIn: [Your Name](https://linkedin.com/in/yourusername)
- Email: your.email@example.com 