using Microsoft.JSInterop;

namespace MyPortfolio.Services;

public class ThemeService
{
    private readonly IJSRuntime _jsRuntime;
    public event Action? OnThemeChanged;

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> IsDarkModeAsync()
    {
        try
        {
            var theme = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "theme");
            return theme == "dark";
        }
        catch
        {
            // Fallback to light mode if localStorage is not available
            return false;
        }
    }

    public async Task SetThemeAsync(bool isDark)
    {
        try
        {
            var theme = isDark ? "dark" : "light";
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "theme", theme);
            await _jsRuntime.InvokeVoidAsync("setTheme", theme);
            OnThemeChanged?.Invoke();
        }
        catch
        {
            // Handle error silently
        }
    }

    public async Task ToggleThemeAsync()
    {
        var isDark = await IsDarkModeAsync();
        await SetThemeAsync(!isDark);
    }

    public async Task InitializeThemeAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("initializeTheme");
        }
        catch
        {
            // Handle error silently
        }
    }
} 