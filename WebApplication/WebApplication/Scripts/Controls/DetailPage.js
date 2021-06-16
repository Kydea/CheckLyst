matematici.DetailPage = function (options) {
    var
        detailPage = this,
        data = !options.wholePage && matematici.popup.getData(),
        sender = !options.wholePage && matematici.popup.getSender(),
        buttons = options.buttons || [],
        form,
        lastReason,
        title,

        submit = function (event, callback, takeOpen) {
            var settings = matematici.tokenManager.getCompanySettings(),
                input,
                trigger = function () {
                    form.trigger(event, function (result) {
                        form.handleResult(result);

                        if (settings && settings.requireUserReason) {
                            lastReason = !result.valid ? input.val() : "";
                            matematici.dialog.close();
                        }

                        if (result.valid) {
                            matematici.canClose = true;
                            matematici.popup.getResult().saved = true;
                            if (!options.wholePage && !takeOpen) matematici.popup.close();

                            if (!data && result.data) refresh(result.data);

                            matematici.blip.ok(function () {
                                callback && callback(result);
                            });
                        }
                    }, options.parametersPrepareHandler && options.parametersPrepareHandler());
                };

            if (settings && settings.requireUserReason) {
                matematici.dialog.show({
                    title: "Reason",
                    content: $("<div />")
                        .append(
                            $("<label />")
                                .attr("for", "DetailPageReasonInput")
                                .text("Reason for change")
                        )
                        .append(
                            input = $("<input />")
                                .attr("id", "DetailPageReasonInput")
                                .attr("type", "text")
                                .attr("autocomplete", "off")
                            //.val(lastReason) //we are currently asking the reason each time as desired by the customer, restore the old value to set last reason
                        ),
                    buttons: [
                        {
                            text: "save", icon: "check", defaultButton: true, handler: function () {
                                if (input.val().trim() === "")
                                    input.addClass("Error");
                                else {
                                    input.removeClass("Error");
                                    form.setValue("reason", input.val());
                                    trigger();
                                }

                                return true;
                            }
                        },
                        { text: "cancel", hidden: false, icon: "times", cancelButton: true }
                    ],
                });
                setTimeout(function () { input.focus(); }, 1);
            } else {
                trigger();
                if (event === "Delete") return false; // Close dialog in case event is delete and reason is not required
            }

            return true;
        },

        clickHandler = function (button, event, callback, takeOpen) {
            if (button && button.hasClass("ReadOnly")) return false;

            if (options && options.validateHandler) {
                options.validateHandler(event, function () {
                    submit(event, callback, takeOpen)
                });
                return false;
            } else
                return submit(event, callback, takeOpen);
        },

        deleteRecord = function (deleteHandler) {
            matematici.dialog.show("warning", "The record will be deleted.", [
                {
                    text: "confirm", icon: "check", handler: function () {
                        return clickHandler($(this), "Delete", deleteHandler);
                    }
                },
                {
                    text: "cancel", icon: "times", cancelButton: true
                }
            ]);
        },

        setup = function () {
            options.readOnly = options.readOnly != null ? options.readOnly : sender.getOptions().searchPage.isReadOnly();
            if (!options.wholePage) {
                buttons.push({
                    cancelButton: true, hidden: true, handler: function () {
                        if (matematici.canClose)
                            return false;

                        matematici.dialog.show("warning", "Warning: any unsaved change will be lost.", [
                            { text: "save", icon: "check", handler: matematici.save },
                            { text: "discard the changes", icon: "exclamation", handler: matematici.popup.close },
                            { text: "cancel", hidden: true, icon: "remove", cancelButton: true }
                        ]);
                        return true;
                    }
                });

                if (!options.readOnly) {
                    if (data !== null && !options.disableDelete)
                        buttons.push({
                            text: "delete", icon: "trash-alt", handler: function () {
                                deleteRecord();
                                return true;
                            }
                        });

                    buttons.push({
                        text: "save", defaultButton: true, icon: "save", id: "SaveButton", handler: function () {
                            clickHandler($(this), data == null ? "Add" : "Edit");
                            return true;
                        }
                    });
                }


                matematici.popup.setButtons(buttons);

                matematici.popup.setWidth(options.width || "520px");

                setTimeout(function () {
                    if (options.initHandler) options.initHandler(detailPage);
                    if (data) form.setValues(data);
                    if (options.completeHandler) options.completeHandler(detailPage);

                    title = matematici.popup.getTitle().html();
                    refresh(data, true);
                }, 0);
            }

            form = new matematici.Form(options.formSelector, {
                applyButton: "#SaveButton",
                bindEvent: options.bindEvent,
                readOnly: options.readOnly,
                resultHandler: options.resultHandler
            });
            form.initialize();
            if (!options.readOnly) {
                form.addChangeHandler(function () { matematici.canClose = false; });
                matematici.save = function (callback, takeOpen) {
                    return clickHandler($("#SaveButton"), data == null ? options.addEvent || "Add" : options.editEvent || "Edit", callback, takeOpen);
                };
            }
        },

        refresh = function (d, avoidSetValues) {
            data = d;
            if (data && !avoidSetValues) form.setValues(data);
            matematici.popup.getTitle().html(title + (!data ? " (new)" : data.id ? " (ID: " + data.id + ")" : data.code ? " (Code: " + data.code + ")" : ""));
        };

    this.getElement = function () {
        return matematici.popup.getElement();
    };

    this.getSender = function () {
        return sender;
    };

    this.getForm = function () {
        return form;
    };

    this.isNew = function () {
        return data == null;
    };

    this.isReadOnly = function () {
        return options.readOnly;
    };

    this.clickHandler = clickHandler;
    this.deleteRecord = deleteRecord;

    setup();
}