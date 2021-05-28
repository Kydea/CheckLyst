matematici.SQLEditor = function (id) {
    var
        editor = ace.edit(id),

        setup = function () {
            editor.session.setMode("ace/mode/sql");
            editor.setTheme("ace/theme/custom")
            editor.setShowPrintMargin(false);
            editor.setOptions({
                enableBasicAutocompletion: true
            });

            var document = editor.session.doc;
            document.applyAnyDelta = document.applyAnyDelta || document.applyDelta;
            document.applyDelta = function (delta) {
                var joinedLines = delta.lines.join("\n");

                if (delta.action == "insert" && this.$maxLength && this.getValue().length + joinedLines.length > this.$maxLength) {
                    var newPasteLength = this.$maxLength - this.getValue().length;
                    if (newPasteLength > 0) {
                        delta.lines = joinedLines.substr(0, newPasteLength).split("\n");
                        delta.end = delta.lines.length == 1 && delta.start.row == delta.end.row ?
                            {
                                row: delta.start.row,
                                column: delta.start.column + newPasteLength
                            } :
                            {
                                row: delta.start.row + delta.lines.length,
                                column: delta.lines[delta.lines.length - 1].length
                            }
                    } else
                        return false;
                }
                return this.applyAnyDelta(delta);
            }

            document.$maxLength = 10000;
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

    this.addCompleterItems = function (values) {
        editor.completers.push({
            getCompletions: function (editor, session, pos, prefix, callback) {
                callback(null, values);
            }
        });
    };

    this.setError = function (position, text, type) {
        editor.session.clearAnnotations();
        if (text) {
            if (position > 0) //position is a number
            {
                var
                    query = editor.session.getValue(),
                    characterPosition = position;

                position = { column: 0, row: 0 };
                for (var index = 0; index < characterPosition && query != null && index < query.length; ++index)
                    if (query[index] == "\n") {
                        ++position.row;
                        position.column = 0;
                    } else
                        ++position.column;
            }
            var annotation = {
                row: position && position.row || 0,
                column: position && position.column || 0,
                text: text + (position && position.row && position.column && (" at row " + (position.row + 1) + ", column " + (position.column + 1)) || ""),
                type: type
            }
            editor.session.setAnnotations([annotation]);
        }
    };

    setup();
}