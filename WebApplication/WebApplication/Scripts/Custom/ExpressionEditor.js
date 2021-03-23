matematici.ExpressionEditor = function (id) {
    var
        languageTools = ace.require("ace/ext/language_tools"),
        autoComplete = ace.require("ace/autocomplete").Autocomplete,
        utility = ace.require("ace/autocomplete/util"),
        snippetManager = ace.require("ace/snippets").snippetManager,
        lang = ace.require("ace/lib/lang"),
        editor = ace.edit(id),
        elementCodes,

        execHandler = function (e) {
            var editor = e.editor;
            var hasCompleter = editor.completer && editor.completer.activated;
            if (e.command.name === "backspace") {
                if (hasCompleter && !utility.getCompletionPrefix(editor))
                    editor.completer.detach();
            }
            else if (e.command.name === "insertstring") {
                var prefix = utility.getCompletionPrefix(editor);
                if (prefix && !hasCompleter) {
                    var completer = autoComplete.for(editor);
                    completer.autoInsert = false;
                    completer.showPopup(editor);
                }
                else if (e.args === ".") {
                    if (hasCompleter)
                        editor.completer.detach();
                    var completer = autoComplete.for(editor);
                    completer.autoInsert = false;
                    completer.showPopup(editor);
                }
            }
        },

        customCompleter = {
            getCompletions: function (editor, session, pos, prefix, callback) {
                var method = prefix.length == 0 && pos.column > 1; //following a dot

                var scopes = [];
                var token = session.getTokenAt(pos.row, pos.column);
                if (token && token.type.match(/(tag-name|tag-open|tag-whitespace|attribute-name|attribute-value)\.xml$/))
                    scopes.push('html-tag');
                else
                    scopes = snippetManager.getActiveScopes(editor);

                var snippetMap = snippetManager.snippetMap;
                var completions = [];
                scopes.forEach(function (scope) {
                    var snippets = snippetMap[scope] || [];
                    for (var i = snippets.length; i--;) {
                        var
                            s = snippets[i],
                            caption = s.name || s.tabTrigger,
                            matches = !method && !s.interface;

                        if (!matches) {
                            var
                                range = session.getAWordRange(pos.row, pos.column - 1),
                                word = session.getTextRange(range),
                                element = word && prototypes.prototype.findItemByCode(word);

                            matches = element &&
                            (
                                s.interface == "ValidityItem"
                                || s.interface == "Field" && element.typeName == "Field"
                                || s.interface == "EditableItem" && (element.typeName == "Attachment" || element.typeName == "Field" || element.typeName == "Group" || element.typeName == "Signature")
                                || s.interface == "Iterator" && (element.typeName == "Group" && element.iterator)
                                || s.interface == "Signature" && element.typeName == "Signature"
                            );
                        }

                        if (matches)
                            completions.push({
                                caption: caption,
                                snippet: s.content,
                                meta: s.meta,
                                type: "snippet",
                                comment: s.comment
                            });
                    }
                }, this);

                if (!method)
                    for (var index in elementCodes) {
                        var element = elementCodes[index];
                        completions.push({
                            name: "#" + element.code,
                            value: element.code,
                            meta: element.type
                        });
                    }
                callback(null, completions);
            },
            getDocTooltip: function (item) {
                if (item.type == "snippet" && !item.docHTML && item.comment) {
                    item.docHTML = [
                        "<strong>", lang.escapeHTML(item.caption), "</strong><br></br>",  
                        lang.escapeHTML(item.comment)
                    ].join("");
                }
            }
        },

        setup = function () {
            editor.session.setMode("ace/mode/expression");
            editor.setOptions({
                enableBasicAutocompletion: true,
                enableSnippets: true
            });
            editor.setTheme("ace/theme/custom")
            editor.setShowPrintMargin(false);

            languageTools.setCompleters([customCompleter])
            editor.commands.on('afterExec', execHandler);

            elementCodes = prototypes.prototype.getElementCodes();

            setTimeout(function () {
                editor.focus();
            }, 0);
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
        }
    };

    setup();
}