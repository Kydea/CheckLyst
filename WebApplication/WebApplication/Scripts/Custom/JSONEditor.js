matematici.JSONEditor = function (id) {
    var
        editor = ace.edit(id),

        setup = function () {
            editor.session.setMode("ace/mode/json");
            editor.setTheme("ace/theme/custom")
            editor.setShowPrintMargin(false);
        };

    this.setChangeHandler = function (value) {
        editor.session.on('change', value);
    };

    setup();
}
