// alert
window.showAlert = (message) => {
    alert(message);  
};
// click on element 
window.elementClick = (elementId) => {
    var button = document.getElementById(elementId);
    if (button) {
        button.click();
    }
    else
        alert("Button not found");
};
// focus on element
window.focusElement = (elementId) => {
    if (elementId) {
        var element = document.getElementById(elementId);
        element.focus();
    }
};
