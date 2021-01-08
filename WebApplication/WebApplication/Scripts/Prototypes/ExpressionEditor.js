prototypes.ExpressionEditor = function (expressions, expressionIndex, confirmHandler, deleteHandler) {
    var
        expression = expressions[expressionIndex],
        newRecord = !expression,
        expressionEditor = this,
        edited = false,
        form,

        toggleNotification = function () {
            var
                notificationCheck = $("input[name=notification]", form),
                notifictatonOn = notificationCheck.prop("checked");

            matematici.controls.setEditable($("input[name=notificationEmail]", form), notifictatonOn);
            matematici.controls.setEditable($("input[name=notificationNote]", form), notifictatonOn);
        },

        cancel = function () {
            if (!edited)
                return false;

            matematici.dialog.show("warning", "Warning: the changes to the expression will be lost.", [
                { text: "discard the changes", icon: "exclamation", handler: matematici.popup.close },
                { text: "cancel", hidden: true, icon: "remove", cancelButton: true }
            ]);
            return true;
        },

        confirm = function () {
            if (!edited) return false;

            var
                expression = {},
                notificationCheck = $("input[name=notification]", form),
                code = $("input[name=code]", form).val();

            if (!matematici.codeRegularExpression.test(code)) {
                matematici.dialog.error("Invalid code", "The code " + code + " is not valid");
                return true;
            }

            for (var index in expressions)
                if (index != expressionIndex && code == expressions[index].code) {
                    matematici.dialog.error("Duplicated code", "The code " + code + " is duplicated");
                    return true;
                }

            if (notificationCheck.prop("checked")) {
                var
                    emailInput = $("input[name=notificationEmail]", form),
                    email = emailInput.val().trim().split("; ").join(";").toLowerCase(),
                    re = /^(([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5}){1,25})+([;.](([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5}){1,25})+)*$/;

                if (email.endsWith(";")) email = email.substring(0, email.length - 1);

                if (email == "" || !re.test(email)) {
                    emailInput.focus();
                    new matematici.dialog.error("error", "Invalid email list. Provide a semicolon separated list of valid email addresses.");
                    return true;
                }
            } else {
                $("input[name=notificationEmail]", form).val(null);
                $("input[name=notificationNote]", form).val(null);
            }

            $("input[type=text], textarea", form).each(function () {
                expression[this.name] = this.value;
            });

            $("input[type=checkbox]", form).each(function () {
                expression[this.name] = this.checked;
            });

            $("select", form).each(function () {
                expression[this.name] = this.value;
            });

            if (!newRecord && expressions[expressionIndex].dependentRelations && expressions[expressionIndex].dependentRelations.length > 0 && code != expressions[expressionIndex].code) {
                var message = "WARNING: the expression is referenced by " + expressions[expressionIndex].dependentRelations.length + " element(s). If you change the code from '" + expressions[expressionIndex].code + "', the referencing elements will have to be fixed to save the proptotype."
                matematici.dialog.show("warning", message, [
                    {
                        text: "confirm", icon: "check", handler: function () {
                            confirmHandler(expression);
                            matematici.popup.close();
                        }
                    },
                    { text: "cancel", hidden: true, icon: "remove", cancelButton: true }
                ]);
                return true;
            } else
                confirmHandler(expression);
        },

        remove = function () {
            var message = newRecord || expression.dependentRelations && expression.dependentRelations.length == 0 ?
                "Warning: the expression will be deleted." :
                "WARNING: the expression is referenced by " + expression.dependentRelations.length + " element(s). After the deletion of this expression, the referencing elements will have to be fixed to save the proptotype.";
            matematici.dialog.show("warning", message, [
                {
                    text: "confirm", icon: "check", handler: function () {
                        deleteHandler();
                        matematici.popup.close();
                    }
                },
                { text: "cancel", hidden: true, icon: "remove", cancelButton: true }
            ]);
            return true;
        },

        fill = function (f) {
            form = f;

            if (prototypes.prototype.getModel().state == prototypes.Prototype.states.draft) {
                $("input[name=code]", form).keyfilter(/^[$A-Z_0-9]*$/i);
                form
                    .on("change", "input, textarea, select", function () { edited = true; })
                    .on("change", "input[name=notification]", toggleNotification);
            } else
                $("input, textarea", form).each(function () {
                    matematici.controls.setEditable($(this), false);
                });

            if (!newRecord) {
                $("input[type=text], textarea", form).each(function () {
                    this.value = expression[this.name];
                });

                $("input[type=checkbox]", form).each(function () {
                    this.checked = this.value = expression[this.name];
                });

                $("select", form).each(function () {
                    this.value = expression[this.name];
                });
            }

            toggleNotification();
        },

        setup = function () {
            var buttons = [];

            buttons.push({ cancelButton: true, hidden: true, handler: cancel });

            if (prototypes.prototype.getModel().state == prototypes.Prototype.states.draft) {
                buttons.push({ text: "delete", icon: "trash-alt", handler: remove });
                buttons.push({ text: "confirm", icon: "save", handler: confirm });
            }

            matematici.popup.show({
                url: matematici.urls.prototypes.expressionEditor,
                data: expressionEditor,
                sender: expressionEditor,
                buttons: buttons,
                width: 680,
            });
        };

    this.fill = fill;

    setup();
};