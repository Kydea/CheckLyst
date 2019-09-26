prototypes.EmailProperty = function (model, readOnly) {
    var
        conditionSelect, valueInput,

        toggleControls = function () {
            valueInput.toggle(conditionSelect.val() == 1);
        },

        conditionChanged = function () {
            var condition = conditionSelect.val();
            model.sendEmail = condition != 0;
            if (condition != 1) model.sendEmailAddress = null;
            matematici.canClose = false;

            toggleControls();
        },

        valueChanged = function () {
            var
                email = valueInput.val().trim().split("; ").join(";").toLowerCase(),
                re = /^(([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5}){1,25})+([;.](([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5}){1,25})+)*$/;

            if (email.endsWith(";")) email = email.substring(0, email.length - 1);

            if (!re.test(email)) {
                valueInput.focus();
                new matematici.dialog.error("error", "Invalid email list. Provide a semicolon separated list of valid email addresses.");
                return;
            }

            model.sendEmailAddress = conditionSelect.val() == 1 ? email : null;
            matematici.canClose = false;
        };

    this.create = function (valuePanel) {
        valuePanel.addClass("Email");
        if (readOnly) {
            $("<input />")
                .attr({ type: "text" })
                .val(!model.sendEmail ? "(disabled)" : model.sendEmailAddress ? "distribution list" : "instance editors")
                .appendTo(valuePanel)
                .attr("disabled", "disabled");
            if (model.sendEmail && model.sendEmailAddress)
                $("<input />")
                    .attr({ type: "text" })
                    .val(model.sendEmailAddress)
                    .appendTo(valuePanel)
                    .attr("disabled", "disabled");
        } else {
            conditionSelect = $("<select />")
                .on("change", conditionChanged)
                .appendTo(valuePanel)
                .append($("<option />").attr("value", "0").text("(disabled)").attr("selected", !model.sendEmail))
                .append($("<option />").attr("value", "1").text("distribution list").attr("selected", model.sendEmail && model.sendEmailAddress))
                .append($("<option />").attr("value", "2").text("instance editors").attr("selected", model.sendEmail && !model.sendEmailAddress));
            valueInput = $("<input />")
                .on("change", valueChanged)
                .attr({ type: "text" })
                .val(model.sendEmailAddress)
                .appendTo(valuePanel);

            toggleControls();
        }
    };

    this.hasChanges = function () {
        return !!model.sendEmail;
    };
};

