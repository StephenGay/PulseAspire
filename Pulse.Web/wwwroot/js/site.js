window.scrollToBottom = function (element) {
    element.scrollTop = element.scrollHeight;
};
window.elementFocus = function (element) {
    element.focus();
};
window.cleanupMenuButton = function (id) {
    var element = document.getElementById(id);
    if (element) {
        // Remove event listeners or cleanup
    }
};

function downloadHTMLFile(filename, content) {
    const blob = new Blob([content], { type: "text/html" });
    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};
window.downloadPdfFile = function (fileName, base64Data) {
    const link = document.createElement('a');
    link.href = `data:application/pdf;base64,${base64Data}`;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};
function downloadExcel(filename, byteBase64) {
    const link = document.createElement('a');
    link.download = filename;
    link.href = `data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,${byteBase64}`;
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};
function applyPulseTheme(themeN) {
    const sheet = new CSSStyleSheet();
    var darkC;
    var lightC;
    var bgPic;

    switch (themeN) {
        case 'pulse':
            darkC = "#660000";
            lightC = "#d9d9d9";
            bgPic = "PulseAspireBG.png";
            break;
        case 'HM':
            darkC = "#365b66";
            lightC = "#bcd1d4";
            bgPic = "HMBG.png";
            break;
        case 'ocean':
            darkC = "hsl(240, 100%, 15%)";
            lightC = "hsl(209, 100%, 90%)";
            bgPic = "Ocean.png";
            break;
        case 'space':
            darkC = "hsl(0, 0%, 0%)";
            lightC = "hsl(0, 0%, 86%)";
            bgPic = "Space.png";
            break;
    }
    sheet.replaceSync(':root { --pulse-dark-colour: ' + darkC + '; --pulse-light-colour:' + lightC + '; --pulse-theme-bg: url(../images/backgrounds/' + bgPic + '); --accent-fill-rest: ' + darkC + '; }');
    /*document.adoptedStyleSheets = [sheet];*/
    document.adoptedStyleSheets = document.adoptedStyleSheets.concat(sheet);
}
