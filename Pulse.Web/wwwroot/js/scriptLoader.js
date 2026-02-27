// wwwroot/js/scriptLoader.js
window.loadScriptOnce = function (url) {
    return new Promise((resolve, reject) => {
        // Prevent loading the same script multiple times
        if (document.querySelector(`script[src="${url}"]`)) {
            resolve();
            return;
        }

        const script = document.createElement('script');
        script.src = url;
        script.onload = () => resolve();
        script.onerror = () => reject(`Failed to load script: ${url}`);
        document.head.appendChild(script);
    });
};
