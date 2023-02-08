matematici.clipboard = function () {
    return {
        copy: function (text) {
            var textArea = $("<textarea />").addClass("ClipboardBridge").appendTo(matematici.elements.body).val(text);
            textArea[0].focus();
            textArea[0].select();

            try {
                successful = document.execCommand("copy");
                if (successful)
                    matematici.blip.ok();
                else
                    matematici.blip.ko();
            } catch (e) {
                console.log(e);
                matematici.blip.ko();
            }
            setTimeout(function () { textArea.remove(); }, 0);
        }
    };
}();