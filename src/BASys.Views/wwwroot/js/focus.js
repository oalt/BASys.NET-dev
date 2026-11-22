// Sets the focus to an element of a dialog; the text of an input is selected so that it can be overwritten at once.
window.basysFocus = {
    focusAndSelect: function (element) {
        if (element) {
            element.focus();
            if (typeof element.select === "function") {
                element.select();
            }
        }
    }
};
