window.applyPulseTheme = function (themeName) {

    const body = document.body;

    // Trigger transition effect
    body.classList.add('theme-changing');

    const root = document.documentElement;
    let config = {};

    const isDark = themeName.endsWith('-dark');
    const baseTheme = themeName.replace('-dark', '');

    switch (baseTheme.toLowerCase()) {
        case 'pulse':
            config = isDark
                ? { dark: "hsl(0,100%,20%)", light: "hsl(0,0%,85%)", accentBg: "#6F8D6A", accentColour: "#FFFFFF", bgImage: "PulseAspireBG-dark.png" }
                : { dark: "hsl(0,0%,85%)", light: "hsl(0,100%,20%)", accentBg: "#FFFFFF", accentColour: "#6F8D6A", bgImage: "PulseAspireBG.png" };
            break;
        case 'hm':
            config = isDark
                ? { dark: "hsl(200, 40%, 30%)", light: "hsl(190,30%,70%)", accentBg: "#C67FAE", accentColour: "#FFFFFF", bgImage: "HMBG-dark.png" }
                : { dark: "hsl(190,30%,70%)", light: "hsl(200, 40%, 30%)", accentBg: "#FFFFFF", accentColour: "#C67FAE", bgImage: "HMBG.png" };
            break;
        case 'ocean':
            config = isDark
                ? { dark: "hsl(240, 100%, 15%)", light: "hsl(209,100%,90%)", accentBg: "#A66E4A", accentColour: "#FFFFFF", bgImage: "Ocean-dark.png" }
                : { dark: "hsl(209,100%,90%)", light: "hsl(240, 100%, 15%)", accentBg: "#FFFFFF", accentColour: "#A66E4A", bgImage: "Ocean.png" };
            break;
        case 'space':
            config = isDark
                ? { dark: "hsl(0, 0%, 0%)", light: "hsl(0,0%,86%)", accentBg: "#E2552D", accentColour: "#FFFFFF", bgImage: "Space-dark.png" }
                : { dark: "hsl(0,0%,86%)", light: "hsl(0, 0%, 0%)", accentBg: "#FFFFFF", accentColour: "#E2552D", bgImage: "Space.png" };
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

    setTimeout(() => {
        body.classList.remove('theme-changing');
    }, 650);
    // Save preference
    // localStorage.setItem('pulse-theme', themeName);
    console.log(`✅ Theme applied: ${themeName}`);
};

// Auto-apply saved theme on load
// document.addEventListener('DOMContentLoaded', () => {
//     const saved = localStorage.getItem('pulse-theme');
//     if (saved) window.applyPulseTheme(saved);
// });