// Hero Section Animations
document.addEventListener('DOMContentLoaded', function() {
    // Typing animation
    const typingText = document.getElementById('typing-text');
    if (typingText) {
        const originalText = typingText.textContent;
        typingText.textContent = '';
        
        let i = 0;
        const typeWriter = () => {
            if (i < originalText.length) {
                typingText.textContent += originalText.charAt(i);
                i++;
                setTimeout(typeWriter, 100);
            }
        };
        
        // Start typing animation after a short delay
        setTimeout(typeWriter, 1000);
    }
    
    // Add hover effects to floating icons
    const floatingIcons = document.querySelectorAll('.floating-icon');
    floatingIcons.forEach(icon => {
        icon.addEventListener('mouseenter', function() {
            this.style.transform = 'scale(1.2) translateY(-15px)';
            this.style.boxShadow = '0 8px 30px rgba(0, 0, 0, 0.2)';
        });
        
        icon.addEventListener('mouseleave', function() {
            this.style.transform = '';
            this.style.boxShadow = '';
        });
    });
    
    // Note: Floating icons are now non-interactive (pointer-events: none)
    // to ensure hero buttons remain clickable
});

// Add ripple animation to CSS
const style = document.createElement('style');
style.textContent = `
    @keyframes ripple {
        to {
            transform: scale(2);
            opacity: 0;
        }
    }
`;
document.head.appendChild(style); 