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
