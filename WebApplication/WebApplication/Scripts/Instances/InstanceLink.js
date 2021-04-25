instances.InstanceLink = function (item, m) {
    var
        model = m,
        code,
        description,
        state,
        button,
        element = item.getElement(),
        label,
        span,

        attach = function () {
            $.ajax({
                url: matematici.urls.api + "/Instances/List/GetData",
                type: "post",
                data: {
                    data: {
                        prototypeName: model.prototypeName
                    }
                }
            }).done(function (result) {
                var
                    columns = [
                        { label: "ID", name: "state" },
                        { label: "Code", name: "code" },
                        { label: "State", name: "state" },
                        { label: "Year", name: "year" },
                        { label: "Number", name: "number" },
                        { label: "Description", name: "description" },
                        { label: "Prototype version", name: "prototypeVersion" },
                        { label: "Created on", name: "createOn", type: "DateTimeOffset" },
                        { label: "Created by", name: "createdBy" },
                        { label: "Modified on", name: "modifiedOn", type: "DateTimeOffset" },
                        { label: "Modified by", name: "modifiedBy" },
                    ],
                    checkListGrid = new matematici.CheckListGrid("Instances of prototype " + model.prototypeName, result.data, null, null, columns);
            });
        },

        detach = function () {

        },

        create = function () {

        },

        refresh = function () {

        },

        action = function () {
            if (model.readOnly) return;

            var
                buttons = [
                    { text: "Cancel", icon: "times", cancelButton: true }
                ];

            if (model.code) {
                buttons.push({ text: "Detach", icon: "chain-broken", handler: detach });
                if (model.state != instances.Instance.states.confirmed)
                    buttons.push({ text: "Refresh values", icon: "refresh", handler: refresh });
            } else {
                if (model.actions != 2)
                    buttons.push({ text: "Create", icon: "plus", handler: create });
                if (model.actions != 1)
                    buttons.push({ text: "Attach", icon: "link", handler: attach });
            }
            matematici.dialog.popupMenu(buttons);
        },


        setup = function () {
            label = document.createElement("label");
            label.className = "Controls3";
            span = document.createElement("span");
            span.className = "Label";
            label.appendChild(span);
            if (!item.isTableContent()) element.appendChild(label);

            var parent = (item.isTableContent() ? element : label);

            var i = document.createElement("i");
            i.className = "Icon fal fa-key";
            parent.appendChild(i);
            code = document.createElement("input");
            code.className = "User";
            code.setAttribute("type", "text");
            code.readOnly = true;
            parent.appendChild(code);

            i = document.createElement("i");
            i.className = "Icon fal fa-layer-group";
            parent.appendChild(i);
            description = document.createElement("input");
            description.className = "Time";
            description.setAttribute("type", "text");
            description.readOnly = true;
            parent.appendChild(description);

            i = document.createElement("i");
            i.className = "Icon fal fa-file-invoice";
            parent.appendChild(i);
            state = document.createElement("input");
            state.className = "Date";
            state.setAttribute("type", "text");
            state.readOnly = true;
            parent.appendChild(state);

            i = document.createElement("i");
            i.className = "Icon fa fa-cog";
            button = document.createElement("a");
            button.className = "Button GreyYellowButton";
            button.setAttribute("href", "#");
            button.addEventListener("click", action);
            button.appendChild(i);
            button.innerHTML += "Actions";
            element.appendChild(button);
        };

    this.update = function (m) {
        model = m;

        span.innerHTML = model.descriptionValue || "(no description)";

        button.classList.toggle("ReadOnly", model.readOnly);

        code.value = model.record.code;
        description.value = model.record.description;
        description.setAttribute("title", model.record.description || "");
        state.value = model.record.state;

        if (item.isTableContent())
            description.style.display = "none";

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