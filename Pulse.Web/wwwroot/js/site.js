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
    var darkCt50;
    var lightC;
    var lightC2;
    var bgPic;

    switch (themeN) {
        case 'pulse':
            darkC = "hsl(0,100%,20%)";
            darkCt50 = "hsla(0,100%,20%,0.5)";
            lightC = "hsl(0, 0%, 85%)";
            lightC2 = "hsl(0, 0%, 75%)";
            bgPic = "PulseAspireBG.png";
            break;
        case 'HM':
            darkC = "hsl(200, 40%, 30%)";
            darkCt50 = "hsla(200, 40%, 30%, 0.5)";
            lightC = "hsl(190, 30%, 80%)";
            lightC2 = "hsl(190, 30%, 70%)";
            bgPic = "HMBG.png";
            break;
        case 'ocean':
            darkC = "hsl(240, 100%, 15%)";
            darkCt50 = "hsla(240, 100%, 15%,0.5)"; 
            lightC = "hsl(209, 100%, 90%)";
            lightC2 = "hsl(209, 70%, 80%)";
            bgPic = "Ocean.png";
            break;
        case 'space':
            darkC = "hsl(0, 0%, 0%)";
            darkCt50 = "hsla(0, 0%, 0%, 0.5)";
            lightC = "hsl(0, 0%, 86%)";
            lightC2 = "hsl(0, 0%, 75%)";
            bgPic = "Space.png";
            break;
    }
    sheet.replaceSync(':root { --pulse-dark-colour: ' + darkC + '; --pulse-dark-t50-colour:' + darkCt50 + '; --pulse-light-colour:' + lightC + '; --pulse-light-2-colour:' + lightC2 + '; --pulse-theme-bg: url(../images/backgrounds/' + bgPic + '); --accent-fill-rest: ' + darkC + '; }');
    /*document.adoptedStyleSheets = [sheet];*/
    document.adoptedStyleSheets = document.adoptedStyleSheets.concat(sheet);
}
