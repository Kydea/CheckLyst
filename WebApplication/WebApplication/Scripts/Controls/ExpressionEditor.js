matematici.ExpressionEditor = function (id) {
    var
        languageTools = ace.require("ace/ext/language_tools"),
        autoComplete = ace.require("ace/autocomplete").Autocomplete,
        utility = ace.require("ace/autocomplete/util"),
        editor = ace.edit(id),

        execHandler = function (e) {
            var editor = e.editor;
            var hasCompleter = editor.completer && editor.completer.activated;
            if (e.command.name === "backspace") {
                if (hasCompleter && !utility.getCompletionPrefix(editor))
                    editor.completer.detach();
            }
            else if (e.command.name === "insertstring") {
                var prefix = utility.getCompletionPrefix(editor);
                if ((prefix || e.args == ".") && !hasCompleter) {
                    var completer = autoComplete.for(editor);
                    completer.autoInsert = false;
                    completer.showPopup(editor);
                }
            }
        },

        completer = {
            getCompletions: function (editor, session, pos, prefix, callback) {
                if (prefix.length == 0 && pos.column > 1) {
                    //properties
                    var
                        range = session.getAWordRange(pos.row, pos.column - 1),
                        word = session.getTextRange(range);
                    callback(null, [
                        { name: "name1", value: word, score: 100, meta: "meta1" }
                    ]);
                } else
                    //literals
                    callback(null, [
                        { name: "name1", value: "value1", score: 100, meta: "meta1" },
                        { name: "name2", value: "value2", score: 100, meta: "meta2" },
                        { name: "name3", value: "value3", score: 100, meta: "meta2" }
                    ]);
            }
        },

        setup = function () {
            editor.session.setMode("ace/mode/expression");
            editor.setOptions({
                enableBasicAutocompletion: true,
            });
            editor.setShowPrintMargin(false);

            languageTools.setCompleters([completer])
            editor.commands.on('afterExec', execHandler);

            editor.session.setValue("jsdsjidsjdasasji\njsdsjidsjdasasji\ndjsdsjidsjdasasji");

            editor.session.setAnnotations([{
                row: 0,
                column: 0,
                text: "test at line 1, row 6",
                type: "error"
            }, {
                row: 0,
                column: 0,
                text: "test line 1, row 6",
                type: "warning"
            }]);

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

    this.setError = function (position, text) {
        editor.session.clearAnnotations();
        if (text) {
            var annotation = {
                row: position && position.row || 0,
                column: position && position.column || 0,
                text: text + (position && (" at row " + position.row + ", column " + position.column)),
                type: "error" //"warning", "information"
            }
            editor.session.setAnnotations([annotation]);
            console.log(annotation);
        }
    };

    setup();
}