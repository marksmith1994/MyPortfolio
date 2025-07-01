// Update Skills Cards
document.addEventListener('DOMContentLoaded', function() {
    updateSkillCards();
});

function updateSkillCards() {
    const skillCards = document.querySelectorAll('.skill-card');
    
    skillCards.forEach(card => {
        // Remove experience years
        const experienceElement = card.querySelector('.skill-experience');
        if (experienceElement) {
            experienceElement.remove();
        }
        
        // Update skill levels
        const skillLevelElement = card.querySelector('.skill-level');
        if (skillLevelElement) {
            const currentLevel = skillLevelElement.textContent;
            const dataLevel = card.getAttribute('data-level');
            
            // Convert percentage to skill level
            let newLevel = 'Intermediate';
            if (dataLevel >= 90 || currentLevel.includes('95%') || currentLevel.includes('90%')) {
                newLevel = 'Expert';
                card.setAttribute('data-level', 'expert');
            } else if (dataLevel >= 80 || currentLevel.includes('85%') || currentLevel.includes('80%')) {
                newLevel = 'Advanced';
                card.setAttribute('data-level', 'advanced');
            } else if (dataLevel >= 70 || currentLevel.includes('75%') || currentLevel.includes('70%')) {
                newLevel = 'Intermediate';
                card.setAttribute('data-level', 'intermediate');
            } else {
                newLevel = 'Beginner';
                card.setAttribute('data-level', 'beginner');
            }
            
            skillLevelElement.textContent = newLevel;
        }
    });
} 