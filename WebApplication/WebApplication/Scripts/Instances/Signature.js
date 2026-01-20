instances.Signature = function (item, model) {
    var
        user,
        date,
        time,
        button,
        cancelButton,
        exclamation,
        element = item.getElement(),
        label,
        span,
        check,
        readOnly = model.readOnly,

        sign = function () {
            if (readOnly) return;
            var
                signers = matematici.tokenManager.getSigners(),
                mainUser = matematici.tokenManager.getUserCode(),
                sign = function (code, accessToken) {
                    var
                        options = {
                            title: model.descriptionValue || "Signature",
                            width: 460,
                            ajax: {
                                type: "post",
                                url: matematici.urls.accessControl.signature
                            },
                            data: {
                                showPaddle: !!model.acceptsPin,
                                username: code,
                                signHandler: function (panel) {
                                    new instances.Event(instances.Events.signature, model.id, {
                                        userName: mainUser, //used to check the lock of the page, the signer is taken from the accesstoken
                                        password: panel.getPassword(),
                                        pin: panel.getPIN(),
                                        simple: panel.isSimple()
                                    }, accessToken).done(function (result) {
                                        instances.instance.update(result);
                                        matematici.popup.close();
                                        if (model.sendEmail) {
                                            if (model.sendEmailAddress) {
                                                var
                                                    addresses = model.sendEmailAddress.split(";"),
                                                    emails = [];
                                                for (var emailIndex in addresses)
                                                    emails.push({ code: addresses[emailIndex], description: addresses[emailIndex], address: addresses[emailIndex] });
                                                new instances.Event(instances.Events.sendEmail, model.id, { emails: emails }).trigger();
                                            } else
                                                new instances.SignatureEmail(model.id).show();
                                        }
                                        if (model.closeInstance)
                                            instances.instance.complete(true);
                                    }).trigger();
                                }
                            }
                        };

                    matematici.popup.show(options);
                },

                trySign = function (code, accessToken) {
                    new instances.Event(instances.Events.signatureCheck, model.id, { userName: code }).done(function () {
                        sign(code, accessToken);
                    }).trigger();
                };

            if (signers.length == 0)
                trySign(mainUser);
            else {
                var buttons = [];
                buttons.push({
                    text: "Cancel",
                    icon: "times",
                    cancelButton: true
                });
                $.each(signers, function () {
                    var signer = this;
                    buttons.push({
                        text: signer.code,
                        title: signer.name,
                        icon: "user",
                        data: signer,
                        handler: function (signer) {
                            trySign(signer.code, signer.accessToken || "-");
                        }
                    });
                });
                buttons.push({
                    text: mainUser,
                    title: matematici.tokenManager.getUserName(),
                    icon: "user",
                    handler: function () {
                        trySign(mainUser);
                    }
                });
                matematici.dialog.popupMenu(buttons);
            };
        },

        unsign = function () {
            if (cancelButton.classList.contains("ReadOnly")) return;

            var
                options = {
                    title: model.descriptionValue || "Signature cancelation",
                    width: 460,
                    ajax: {
                        type: "post",
                        url: matematici.urls.accessControl.signature
                    },
                    data: {
                        unsign: true,
                        username: matematici.tokenManager.getUserCode(),
                        signHandler: function (panel) {
                            new instances.Event(instances.Events.signatureCancelation, model.id, {
                                userName: panel.getUserName(),
                                password: panel.getPassword(),
                                note: panel.getNote()
                            }).done(function (result) {
                                instances.instance.update(result);
                                matematici.popup.close();
                            }).trigger();
                        }
                    }
                };

            matematici.popup.show(options);
        },

        setup = function () {
            label = document.createElement("label");
            label.className = "Controls3";
            span = document.createElement("span");
            span.className = "Label";
            span.innerHTML = model.descriptionValue || "(no description)";
            label.appendChild(span);
            if (!item.isTableContent()) element.appendChild(label);

            check = document.createElement("i");
            check.className = "Icon fa fa-check";
            check.style.display = "none";
            element.appendChild(check);

            var parent = (item.isTableContent() ? element : label);

            var i = document.createElement("i");
            i.className = "Icon fal fa-user";
            parent.appendChild(i);
            user = document.createElement("input");
            user.className = "User";
            user.setAttribute("type", "text");
            user.readOnly = true;
            parent.appendChild(user);

            i = document.createElement("i");
            i.className = "Icon fal fa-calendar-alt";
            parent.appendChild(i);
            date = document.createElement("input");
            date.className = "Date";
            date.setAttribute("type", "text");
            date.readOnly = true;
            parent.appendChild(date);

            i = document.createElement("i");
            i.className = "Icon fal fa-lock";
            parent.appendChild(i);
            time = document.createElement("input");
            time.className = "Time";
            time.setAttribute("type", "text");
            time.readOnly = true;
            parent.appendChild(time);

            exclamation = document.createElement("i");
            exclamation.className = "fa fa-exclamation";
            exclamation.setAttribute("title", "This signature has been canceled, see the history");
            exclamation.style.display = "none";
            span.appendChild(exclamation);

            button = document.createElement("a");
            button.className = "Button SignButton";
            button.setAttribute("href", "#");
            button.addEventListener("click", sign);
            element.appendChild(button);

            if (model.cancelable) {
                cancelButton = document.createElement("a");
                cancelButton.className = "Button CancelButton";
                cancelButton.setAttribute("href", "#");
                i = document.createElement("i");
                i.className = "Icon fa fa-times";
                cancelButton.appendChild(i);
                cancelButton.innerHTML += "Cancel signature";
                cancelButton.addEventListener("click", unsign);
                element.appendChild(cancelButton);
            }
        };

    this.update = function (model) {
        readOnly = model.readOnly;

        var icon = document.createElement("i");
        icon.className = "Icon fa fa-" + (model.closeInstance ? "lock" : "check");

        span.innerHTML = model.descriptionValue || "(no description)";
        if (model.descriptionValue)
            span.setAttribute("title", model.descriptionValue);
        span.appendChild(exclamation);

        button.textContent = "";
        button.appendChild(icon);
        button.innerHTML += model.buttonLabel ? model.buttonLabel : "Signature";

        button.classList.toggle("ReadOnly", model.readOnly);
        if (cancelButton) cancelButton.classList.toggle("ReadOnly", model.readOnly);

        if (model.signed) {
            user.value = model.signedBy + " - " + model.signerDescription;
            var format = matematici.dateFormatter.format(moment.parseZone(model.signedOn));
            date.value = format.date;
            time.value = format.time + ":" + format.seconds;
            element.setAttribute("title", user.value + " | " + format.date + " " + " " + format.time + ":" + format.seconds + " " + format.offset);
            if (cancelButton) {
                cancelButton.style.display = "inline-block";
                button.style.display = "none";
            } else if (item.isTableContent()) {
                button.style.display = "none";
                check.style.display = "inline";
            }
        } else {
            user.value = "";
            date.value = "";
            time.value = "";
            element.setAttribute("title", "");
            if (cancelButton) {
                button.style.display = "inline-block";
                cancelButton.style.display = "none";
            }
        }

        button.classList.toggle("Red", !model.signed && !model.readOnly && !!model.closeInstance);
        exclamation.style.display = model.history && model.history.length > 1 ? "" : "none";

        this.setColor(model.color);
        this.setFontSize(model.fontSize);
    };

    this.setColor = function () {
        label.style.color = model.color;
    };

    this.setFontSize = function () {
        label.style.fontSize = model.fontSize && model.fontSize + "px";
    }

    setup();
}