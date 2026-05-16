window.observeAnimatedElement = function (element, dotNetHelper) {
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                dotNetHelper.invokeMethodAsync('OnElementVisible');
                observer.unobserve(element);
            }
        });
    }, {
        threshold: 0.15
    });

    observer.observe(element);
};