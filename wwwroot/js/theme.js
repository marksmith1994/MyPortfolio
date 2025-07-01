// Theme management functions
function setTheme(theme) {
    const html = document.documentElement;
    const body = document.body;
    
    if (theme === 'dark') {
        html.setAttribute('data-theme', 'dark');
        body.classList.add('dark-mode');
    } else {
        html.removeAttribute('data-theme');
        body.classList.remove('dark-mode');
    }
}

function initializeTheme() {
    const savedTheme = localStorage.getItem('theme');
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    
    // Use saved theme, or system preference, or default to light
    const theme = savedTheme || (prefersDark ? 'dark' : 'light');
    setTheme(theme);
    
    // Listen for system theme changes
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
        if (!localStorage.getItem('theme')) {
            setTheme(e.matches ? 'dark' : 'light');
        }
    });
}

// Make functions available globally
window.setTheme = setTheme;
window.initializeTheme = initializeTheme; 