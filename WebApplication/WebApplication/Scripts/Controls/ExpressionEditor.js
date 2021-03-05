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
                    callback(null, matematici.ExpressionEditorConstants.properties.field);
                } else
                    //literals
                    callback(null, matematici.ExpressionEditorConstants.tokens);
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

    this.setError = function (position, text, type) {
        editor.session.clearAnnotations();
        if (text) {
            var annotation = {
                row: position && position.row || 0,
                column: position && position.column || 0,
                text: text + (position && (" at row " + (position.row + 1) + ", column " + (position.column + 1)) || ""),
                type: type
            }
            editor.session.setAnnotations([annotation]);
            console.log(annotation);
        }
    };

    setup();
}

//[{ name: "...", value: "...", score: 100, meta: "..." }]

matematici.ExpressionEditorConstants = {};

matematici.ExpressionEditorConstants.functionCodes = [
    "DateIncrement", "DateTimeIncrement", "Error", "IIf"
]
matematici.ExpressionEditorConstants.tokens = matematici.ExpressionEditorConstants.functionCodes.map(function (value) { return { name: value, value: value, score: 100, meta: "keyword" } });

matematici.ExpressionEditorConstants.operatorCodes = [
    "and", "or", "not"
]
matematici.ExpressionEditorConstants.tokens.concat(matematici.ExpressionEditorConstants.operatorCodes.map(function (value) { return { name: value, value: value, score: 100, meta: "operator" } }))

matematici.ExpressionEditorConstants.properties = {};
matematici.ExpressionEditorConstants.propertieCodes = {
    field: ["Value", "Valid", "Error", "ModifiedBy", "ModifiedOn"]
}
matematici.ExpressionEditorConstants.properties.field = matematici.ExpressionEditorConstants.propertieCodes.field.map(function (value) { return { name: value, value: value, score: 100, meta: "field property" } });

console.log(matematici.ExpressionEditorConstants.properties.tokens);
console.log(matematici.ExpressionEditorConstants.properties.field);