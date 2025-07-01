// Skills Visualization
document.addEventListener('DOMContentLoaded', function() {
    // Initialize skill card interactions
    initSkillCards();
    
    // Update skill levels to text-based
    updateSkillLevels();
});

function updateSkillLevels() {
    const skillCards = document.querySelectorAll('.skill-card');
    
    skillCards.forEach(card => {
        const skillLevelElement = card.querySelector('.skill-level');
        if (skillLevelElement) {
            const currentLevel = skillLevelElement.textContent;
            const dataLevel = card.getAttribute('data-level');
            
            // Convert percentage to skill level
            let newLevel = 'Intermediate';
            let newDataLevel = 'intermediate';
            
            if (dataLevel >= 90 || currentLevel.includes('95%') || currentLevel.includes('90%')) {
                newLevel = 'Expert';
                newDataLevel = 'expert';
            } else if (dataLevel >= 80 || currentLevel.includes('85%') || currentLevel.includes('80%')) {
                newLevel = 'Advanced';
                newDataLevel = 'advanced';
            } else if (dataLevel >= 70 || currentLevel.includes('75%') || currentLevel.includes('70%')) {
                newLevel = 'Intermediate';
                newDataLevel = 'intermediate';
            } else {
                newLevel = 'Beginner';
                newDataLevel = 'beginner';
            }
            
            skillLevelElement.textContent = newLevel;
            card.setAttribute('data-level', newDataLevel);
        }
    });
}

function initSkillCards() {
    const skillCards = document.querySelectorAll('.skill-card');
    
    skillCards.forEach(card => {
        // Add click event for detailed view
        card.addEventListener('click', function() {
            const skillName = this.querySelector('.skill-name').textContent;
            const skillLevel = this.querySelector('.skill-level').textContent;
            const skillDescription = this.querySelector('.skill-description').textContent;
            
            showSkillDetails(skillName, skillLevel, skillDescription);
        });
        
        // Add hover effects
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-8px) scale(1.02)';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0) scale(1)';
        });
    });
}

function showSkillDetails(skillName, skillLevel, description) {
    // Create modal or tooltip with detailed information
    const modal = document.createElement('div');
    modal.className = 'skill-modal';
    modal.innerHTML = `
        <div class="skill-modal-content">
            <div class="skill-modal-header">
                <h3>${skillName}</h3>
                <button class="skill-modal-close">&times;</button>
            </div>
            <div class="skill-modal-body">
                <div class="skill-level-display">
                    <span class="skill-level-text">${skillLevel}</span>
                </div>
                <p class="skill-description">${description}</p>
                <div class="skill-details">
                    <h4>Key Competencies:</h4>
                    <ul class="skill-competencies">
                        <li>Advanced implementation</li>
                        <li>Best practices</li>
                        <li>Performance optimization</li>
                        <li>Problem solving</li>
                    </ul>
                </div>
            </div>
        </div>
    `;
    
    // Add modal styles
    const style = document.createElement('style');
    style.textContent = `
        .skill-modal {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0, 0, 0, 0.5);
            display: flex;
            justify-content: center;
            align-items: center;
            z-index: 1000;
            animation: fadeIn 0.3s ease;
        }
        
        .skill-modal-content {
            background: var(--surface);
            border-radius: 12px;
            padding: 2rem;
            max-width: 500px;
            width: 90%;
            box-shadow: var(--shadow-xl);
            animation: slideIn 0.3s ease;
        }
        
        .skill-modal-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 1.5rem;
        }
        
        .skill-modal-close {
            background: none;
            border: none;
            font-size: 1.5rem;
            cursor: pointer;
            color: var(--text-muted);
        }
        
        .skill-level-display {
            text-align: center;
            margin-bottom: 1.5rem;
        }
        
        .skill-level-text {
            font-size: 1.5rem;
            font-weight: 700;
            color: var(--primary);
            display: block;
            margin-bottom: 0.5rem;
        }
        
        .skill-competencies {
            list-style: none;
            padding: 0;
        }
        
        .skill-competencies li {
            padding: 0.5rem 0;
            border-bottom: 1px solid var(--border);
        }
        
        .skill-competencies li:before {
            content: '✓';
            color: var(--success);
            font-weight: bold;
            margin-right: 0.5rem;
        }
        
        @keyframes fadeIn {
            from { opacity: 0; }
            to { opacity: 1; }
        }
        
        @keyframes slideIn {
            from { transform: translateY(-50px); opacity: 0; }
            to { transform: translateY(0); opacity: 1; }
        }
    `;
    
    document.head.appendChild(style);
    document.body.appendChild(modal);
    
    // Close modal functionality
    const closeBtn = modal.querySelector('.skill-modal-close');
    closeBtn.addEventListener('click', () => {
        modal.remove();
        style.remove();
    });
    
    modal.addEventListener('click', (e) => {
        if (e.target === modal) {
            modal.remove();
            style.remove();
        }
    });
} 