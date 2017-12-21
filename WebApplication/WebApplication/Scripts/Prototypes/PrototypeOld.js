prototypes.PrototypeOld = function (selector, id, isNew) {

    prototype.preview = function () {
        var handler = function () {
            $("<a href='" + matematici.urls.instances.preview + "?Prototype=" + model.Code + "&Version=" + model.Version + "' target='_blank'></a>")[0].click();
        };

        if (matematici.canClose)
            handler();
        else
            matematici.dialog.show("warning", "There are unsaved changes.", [
                { text: "save", icon: "check", handler: function () { prototype.save(handler); } },
                { text: "cancel", icon: "remove", cancelButton: true }
            ]);
    };

    prototype.showHistory = function () {
        $.ajax({
            url: matematici.urls.prototypes.history,
            type: "post",
            data: { Prototype: code, Version: version  }
        }).done(function (result) {
            if (result.Error)
                matematici.dialog.error("Error", result.Error);
            else {
                var
                    checkListGrid = new matematici.CheckListGrid(result.Data),
                    parameters = checkListGrid.getParameters(),
                    attributes = { type: "text", readonly: "readonly" };
                
                parameters.append($("<label />").text("Company").append($("<input />").attr(attributes).val(result.Company)));
                parameters.append($("<label />").text("Department").append($("<input />").attr(attributes).val(result.Department)));
                parameters.append($("<label />").text("Prototype Type").append($("<input />").attr(attributes).val(result.PrototypeType)));
                parameters.append($("<label />").text("Prototype").append($("<input />").attr(attributes).val(result.Prototype)));
                parameters.append($("<label />").text("Version").append($("<input />").attr(attributes).val(result.Version)));
            }
        }).fail(function (jqXHR, textStatus, errorThrown) {
            console.log("Server error", jqXHR, textStatus, errorThrown);
            alert("Server error: " + errorThrown);
        });
    };

}