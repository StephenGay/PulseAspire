export function getInnerHtml(element) {
    return element ? element.innerHTML : '';
}

export function getOuterHtml(element) {
    return element ? element.outerHTML : '';
}

/**
 * Captures HTML and replaces all graphs (SVG + Canvas) with embedded PNG images.
 * Perfect for export files.
 */
export async function captureForExport(element) {
    if (!element) return '';

    // Clone so we don't modify the live page
    const clone = element.cloneNode(true);

    const graphs = clone.querySelectorAll('svg, canvas');

    for (const graph of graphs) {
        try {
            let dataUrl;

            if (graph.tagName === 'SVG') {
                // Convert SVG to PNG
                const serializer = new XMLSerializer();
                const svgString = serializer.serializeToString(graph);
                const svgBase64 = btoa(unescape(encodeURIComponent(svgString)));
                const img = new Image();

                await new Promise((resolve, reject) => {
                    img.onload = resolve;
                    img.onerror = reject;
                    img.src = `data:image/svg+xml;base64,${svgBase64}`;
                });

                const tempCanvas = document.createElement('canvas');
                tempCanvas.width = graph.getBoundingClientRect().width || 800;
                tempCanvas.height = graph.getBoundingClientRect().height || 500;
                const ctx = tempCanvas.getContext('2d', { alpha: true });
                ctx.drawImage(img, 0, 0);

                dataUrl = tempCanvas.toDataURL('image/png');
            } else {
                // Canvas element
                dataUrl = graph.toDataURL('image/png');
            }

            // Replace graph with <img>
            const imgElement = document.createElement('img');
            imgElement.src = dataUrl;
            imgElement.style.maxWidth = '100%';
            imgElement.style.height = 'auto';
            imgElement.style.display = 'block';

            graph.parentNode.replaceChild(imgElement, graph);

        } catch (err) {
            console.warn('Failed to convert graph for export:', err);
            // Leave original if conversion fails
        }
    }

    return clone.innerHTML;
}