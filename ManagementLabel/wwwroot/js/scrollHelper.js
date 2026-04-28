window.checkScrollEnd = function (dotnetObj, elementId) {
    const element = document.getElementById(elementId);
    if (!element) {
        setTimeout(() => window.initScrollListener(dotnetObj, elementId), 50);
        return;
    }
    element.removeEventListener('scroll', element._scrollHandler);

    element.addEventListener('scroll', () => {
     
        if (element.scrollTop + element.clientHeight >= element.scrollHeight - 5) {
            dotnetObj.invokeMethodAsync('OnScrollEnd');
        }
    });
};