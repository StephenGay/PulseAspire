window.getBoundingClientRect = function(element) {
    const rect = element.getBoundingClientRect();
    return {
        left: rect.left,
        top: rect.top,
        right: rect.right,
        bottom: rect.bottom,
        width: rect.width,
        height: rect.height
    };
};

window.getElementSize = function(element) {
    return {
        width: element.clientWidth,
        height: element.clientHeight
    };
};

// Optional: For resize observation
window.observeResize = function(element, dotNetRef) {
    const observer = new ResizeObserver((entries) => {
        for (let entry of entries) {
            const { width, height } = entry.contentRect;
            dotNetRef.invokeMethodAsync('OnContainerResized', Math.round(width), Math.round(height));
        }
    });
    observer.observe(element);
    return () => observer.disconnect();
};

window.getSvgPoint = function(svgElement, clientX, clientY) {
    try {
        const ctm = svgElement.getScreenCTM();
        if (!ctm) {
            console.warn("getScreenCTM returned null - SVG may not be rendered");
            return { x: clientX, y: clientY };
        }
        
        const point = svgElement.createSVGPoint();
        point.x = clientX;
        point.y = clientY;
        
        const inverseCtm = ctm.inverse();
        const transformedPoint = point.matrixTransform(inverseCtm);
        
        console.log(`Client: ${clientX}, ${clientY} → SVG: ${transformedPoint.x.toFixed(2)}, ${transformedPoint.y.toFixed(2)}`);
        
        return transformedPoint;
    } catch (error) {
        console.error("Error transforming point:", error);
        return { x: clientX, y: clientY };
    }
};

// Alternative: Get SVG coordinates handling zoom/pan on the SVG or parent group
window.getSvgPointWithTransform = function(svgElement, clientX, clientY) {
    try {
        const rect = svgElement.getBoundingClientRect();
        const svg = svgElement;
        
        // Get the viewBox
        const viewBox = svg.viewBox.baseVal;
        
        // Calculate scale factors
        const scaleX = viewBox.width / rect.width;
        const scaleY = viewBox.height / rect.height;
        
        // Convert client coordinates to SVG coordinates
        const svgX = (clientX - rect.left) * scaleX + viewBox.x;
        const svgY = (clientY - rect.top) * scaleY + viewBox.y;
        
        return { x: svgX, y: svgY };
    } catch (error) {
        console.error("Error in getSvgPointWithTransform:", error);
        return { x: clientX, y: clientY };
    }
};

window.applySvgZoom = function(svgElement, zoomLevel) {
    const viewBox = svgElement.viewBox.baseVal;
    const centerX = viewBox.x + viewBox.width / 2;
    const centerY = viewBox.y + viewBox.height / 2;
    
    const newWidth = viewBox.width / zoomLevel;
    const newHeight = viewBox.height / zoomLevel;
    
    svgElement.setAttribute('viewBox', 
        `${centerX - newWidth / 2} ${centerY - newHeight / 2} ${newWidth} ${newHeight}`);
};