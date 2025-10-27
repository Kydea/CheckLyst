matematici.JSONEditor = function (id) {
    var
        editor = ace.edit(id),

        setup = function () {
            editor.session.setMode("ace/mode/json");
            editor.setTheme("ace/theme/custom")
            editor.setShowPrintMargin(false);
        };

    this.setReadOnly = function (value) {
        editor.setReadOnly(value);
    };

    this.setValue = function (value) {
        editor.session.setValue(value);
    };

    this.getValue = function () {
        return editor.session.getValue();
    };

    this.setChangeHandler = function (value) {
        editor.session.on('change', value);
    };

    setup();
}
