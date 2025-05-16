window.checkScrollEnd = function (dotNetHelper, elementId) {
    const element = document.getElementById(elementId);
    if (!element) return;

    element.addEventListener('scroll', () => {
     
        if (element.scrollTop + element.clientHeight >= element.scrollHeight - 3) {
            dotNetHelper.invokeMethodAsync('OnScrollEnd');
        }
    });
};