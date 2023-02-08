prototypes.PrototypeProperty = function (model) {
    var
        base,
        prototypeSelect,
        readOnly = prototypes.prototype.getModel().state != prototypes.Prototype.states.draft,

        loadPrototypes = function () {
            $.ajax({
                type: "post",
                url: matematici.urls.api + "/Prototypes/List/GetSyntheticData"
            }).done(function (result) {
                setup(result);
            });
        },

        setup = function (prototypesList) {
            var label = $("<label />").appendTo(valuePanel);
            $("<i />").addClass("Icon fal fa-file-invoice").appendTo(label);
            prototypeSelect =
                $("<select />")
                    .attr("data-nullable", "")
                    .on("change", function () {
                        model.prototypeID = prototypeSelect.val();
                        model.prototypeName = prototypeSelect.next().val();
                        if (!readOnly)
                            base.updateModel(model);

                        if (!!model.prototypeName && model.errors)
                            base.setError(model.errors, ["prototype"], null);
                    })
                    .appendTo(label);

            for (var index in prototypesList)
                prototypeSelect.append($("<option />").text(prototypesList[index].name).attr("value", prototypesList[index].id));

            if (readOnly)
                matematici.controls.setEditable(prototypeSelect, false);

            var autoComplete = new matematici.AutoComplete(prototypeSelect[0]);
            autoComplete.initialize();
            autoComplete.setValue(model.prototypeID);
        };


    this.create = function (vp, baseClass) {
        base = baseClass;
        valuePanel = vp;
        valuePanel.addClass("Prototype");
        loadPrototypes();
    }

    this.hasChanges = function () {
        return !!model.prototypeName;
    }

    this.setValue = function (node) {
        node.text(model.prototypeName);
    };

    this.setError = function (handler) {
        handler(model.errors, ["prototype"]);
    };
};