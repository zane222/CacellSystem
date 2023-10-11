mergeInto(LibraryManager.library, {

    alertWeb: function (text) { 
        window.alert(UTF8ToString(text));
    },

    changeTitleWeb: function (text) {
        document.title = UTF8ToString(text);
    },

    printLabelWeb: function () {
        printLabel();
    },

    getCurrentURLPathWeb: function () {
        var returnStr = window.location.href.substring(window.location.origin.length);
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        return buffer;
    },

    changeCurrentURLPathWeb: function (url) {
        window.history.pushState({}, '', UTF8ToString(url));
    },

    makeTicketLabelImageWeb: function (subject, password, itemsLeft, creationDate, name, ticketNumber, phoneNumber, width) {
        makeTicketLabelImage(UTF8ToString(subject), UTF8ToString(password), UTF8ToString(itemsLeft), UTF8ToString(creationDate), UTF8ToString(name), UTF8ToString(ticketNumber), UTF8ToString(phoneNumber), width);
    },

    getCompletedTicketImageWeb: function () {
        var returnStr = getCompletedTicketImage();
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        return buffer;
    },

    getImageFromURLWeb: function (url) {
        GetImageFromURL(UTF8ToString(url));
    },

    getCompletedImageFromURLWeb: function () {
        var returnStr = getCompletedImageFromURL();
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        return buffer;
    },

    isDoneMakingTicketImageWeb: function () {
        return isDoneMakingTicketImage();
    },

    isDoneGettingImageFromURLWeb: function () {
        return isDoneGettingImageFromURL();
    },

    signInWeb: function () {
        signIn();
    },

    isSignedInWeb: function () {
        return isSignedIn();
    },

    getAccessTokenWeb: function () {
        var returnStr = getAccessToken();
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        return buffer;
    },

    logWeb: function (text) {
        console.log(UTF8ToString(text));
    },

    getClipboardWeb: function () {
        clipboard = null;
        navigator.clipboard.readText().then(clipboardText => {
            clipboard = clipboardText;
        });
    },

    getCompletedClipboardTextWeb: function () {
        var returnStr = clipboard;
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        return buffer;
    },

    isDoneGettingClipboardWeb: function () {
        return clipboard != null;
    },

    goBackWeb: function () {
        history.back();
    },

    goForwardWeb: function () {
        history.forward();
    },
      
});