prototypes.ExpressionEditor = function (expression, confirmHandler, deleteHendler, dependencyHandler) {
    var
        expressionEditor = this,
        canClose = true,
        form,

        toggleNotification = function () {
            var
                notificationCheck = $("input[name=notification]", form),
                notifictatonOn = notificationCheck.prop("checked");

            matematici.controls.setEditable($("input[name=notificationEmail]", form), notifictatonOn);
            matematici.controls.setEditable($("input[name=notificationNote]", form), notifictatonOn);
        },

        showDependencies = function () {
            dependencyHandler();
        },

        cancel = function () {
            if (canClose)
                return false;

            matematici.dialog.show("warning", "Warning: the chnges to the expression will be lost.", [
                { text: "discard the changes", icon: "exclamation", handler: matematici.popup.close },
                { text: "cancel", hidden: true, icon: "remove", cancelButton: true }
            ]);
            return true;
        },

        confirm = function () {
            var
                expression = {},
                notificationCheck = $("input[name=notification]", form),
                code = $("input[name=code]", form).val();

            if (!matematici.codeRegularExpression.test(code)) {
                matematici.dialog.error("Invalid code", "The code " + code + " is not valid");
                return false;
            }

            if (!notificationCheck.prop("checked")) {
                $("input[name=notificationEmail]", form).val(null);
                $("input[name=notificationNote]", form).val(null);
            }

            $("input[type=text], textarea", form).each(function () {
                expression[this.name] = this.value;
            });

            $("input[type=checkbox]", form).each(function () {
                expression[this.name] = this.checked;
            });

            confirmHandler(expression);
        },

        remove = function () {
            matematici.dialog.show("warning", "Warning: the expression will be deleted.", [
                {
                    text: "confirm", icon: "check", handler: function () {
                        deleteHendler();
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
                    .on("change", "input, textarea", function () { canClose = false; })
                    .on("change", "input[name=notification]", toggleNotification);
            } else
                $("input, textarea", form).each(function () {
                    matematici.controls.setEditable($(this), false);
                });

            if (expression) {
                $("input[type=text], textarea", form).each(function () {
                    this.value = expression[this.name];
                });

                $("input[type=checkbox]", form).each(function () {
                    this.checked = this.value = expression[this.name];
                });
            }

            toggleNotification();
        },

        setup = function () {
            var buttons = [];

            buttons.push({ cancelButton: true, hidden: true, handler: cancel });

            if (prototypes.prototype.getModel().state == prototypes.Prototype.states.draft) {
                buttons.push({ text: "confirm", icon: "save", handler: confirm });
                buttons.push({ text: "delete", icon: "trash-alt", handler: remove });
                buttons.push({ text: "dependencies", icon: "sitemap", handler: showDependencies });
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