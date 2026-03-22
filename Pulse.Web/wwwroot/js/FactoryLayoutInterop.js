//window.observeResize = (element, dotNetHelper) => {
//    const resizeObserver = new ResizeObserver(entries => {
//        for (let entry of entries) {
//            const { width, height } = entry.contentRect;
//            dotNetHelper.invokeMethodAsync('OnContainerResized', Math.round(width), Math.round(height));
//        }
//    });
//    resizeObserver.observe(element);
//    return () => resizeObserver.disconnect();  // cleanup
//};
//window.getElementSize = (element) => {
//    if (!element) return { width: 0, height: 0 };
//    const rect = element.getBoundingClientRect();
//    return { width: rect.width, height: rect.height };
//};
//window.getBoundingClientRect = (element) => {
//    if (!element) return { left: 0, top: 0 };
//    const r = element.getBoundingClientRect();
//    return { left: r.left, top: r.top };
//};
window.blazorSetPointerCapture = (elementRef, pointerId) => {
    if (!elementRef) return;
    const element = elementRef instanceof Element ? elementRef : elementRef.getBoundingClientRect ? elementRef : null;
    if (element && typeof element.setPointerCapture === 'function') {
        try {
            element.setPointerCapture(pointerId);
            console.log('Pointer captured on container');
        } catch (err) {
            console.warn('setPointerCapture failed:', err);
        }
    }
};
window.getBoundingClientRect = function (element) {
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

window.getElementSize = function (element) {
    return {
        width: element.clientWidth,
        height: element.clientHeight
    };
};

// Optional: For resize observation
window.observeResize = function (element, dotNetRef) {
    const observer = new ResizeObserver((entries) => {
        for (let entry of entries) {
            const { width, height } = entry.contentRect;
            dotNetRef.invokeMethodAsync('OnContainerResized', Math.round(width), Math.round(height));
        }
    });
    observer.observe(element);
    return () => observer.disconnect();
};