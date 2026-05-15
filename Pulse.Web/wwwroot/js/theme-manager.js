window.applyPulseTheme = function (themeName) {
    const root = document.documentElement;
    let config = {};

    switch (themeName.toLowerCase()) {
        case 'pulse':
            config = { dark: "hsl(0,100%,20%)", light: "hsl(0,0%,85%)", accentBg: "#6F8D6A", accentColour: "#FFFFFF", bgImage: "PulseAspireBG.png" };
            break;
        case 'hm':
            config = { dark: "hsl(200, 40%, 30%)", light: "hsl(190,30%,70%)", accentBg: "#C67FAE", accentColour: "#FFFFFF", bgImage: "HMBG.png" };
            break;
        case 'ocean':
            config = { dark: "hsl(240, 100%, 15%)", light: "hsl(209,100%,90%)", accentBg: "#A66E4A", accentColour: "#FFFFFF", bgImage: "Ocean.png" };
            break;
        case 'space':
            config = { dark: "hsl(0, 0%, 0%)", light: "hsl(0,0%,86%)", accentBg: "#E2552D", accentColour: "#FFFFFF", bgImage: "Space.png" };
            break;
        default:
            console.warn('Unknown theme:', themeName);
            return;
    }

    // Apply CSS Variables
    root.style.setProperty('--pulse-dark-colour', config.dark);
    root.style.setProperty('--pulse-light-colour', config.light);
    root.style.setProperty('--pulse-accent-btn-theme-bg', config.accentBg);
    root.style.setProperty('--pulse-accent-btn-theme-colour', config.accentColour);
    root.style.setProperty('--pulse-accent-btn-bg', config.accentBg);
    root.style.setProperty('--pulse-accent-btn-colour', config.accentColour);
    root.style.setProperty('--pulse-theme-bg', `url(../images/backgrounds/${config.bgImage})`);

    // Save preference
    // localStorage.setItem('pulse-theme', themeName);
    console.log(`✅ Theme applied: ${themeName}`);
};

// Auto-apply saved theme on load
// document.addEventListener('DOMContentLoaded', () => {
//     const saved = localStorage.getItem('pulse-theme');
//     if (saved) window.applyPulseTheme(saved);
// });