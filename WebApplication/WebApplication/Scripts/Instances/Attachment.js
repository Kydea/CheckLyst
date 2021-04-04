instances.Attachment = function (item, model) {
    var
        uploadButton,
        viewButton,
        input,
        readOnly,
        key,

        onChange = function () {
            var data = new FormData();
            data.append("attachment", input.files[0]);
            new instances.Event(instances.Events.uploadAttachment, model.id, data, undefined, undefined, true).done(function (result) {
                instances.instance.update(result);
                input.value = "";
            }).error(function () {
                input.value = "";
            }).trigger();
        },

        view = function () {
            var url = matematici.urls.instances.api + "/DownloadAttachment?";
            url += "code=" + instances.instance.getCode();
            url += "&key=" + key;
            url += "&Authorization=" + encodeURIComponent("Bearer " + matematici.tokenManager.getAccessToken());

            if (model.fileType === "images")
                matematici.imageViewer.show("Preview", url);
            else if (model.fileType === "pdf") {
                if (matematici.appMode)
                    window.webkit.messageHandlers.instances.postMessage("file: " + url);
                else
                    window.open(url);
            }
        },

        setup = function () {
            var element = item.getElement();
            input = document.createElement("input");
            input.classList.add("Hidden");
            input.setAttribute("type", "file");
            input.addEventListener("change", onChange);
            element.appendChild(input);

            uploadButton = document.createElement("a");
            uploadButton.className = "Button Attachment";
            uploadButton.setAttribute("href", "#");
            uploadButton.addEventListener("click", function () {
                if (readOnly) return;
                input.click();
            });
            element.appendChild(uploadButton);

            viewButton = document.createElement("a");
            viewButton.className = "Button ViewAttachment";
            viewButton.textContent = "View";
            viewButton.setAttribute("href", "#");
            var i = document.createElement("i");
            i.className = "Icon fa fa-eye";
            viewButton.appendChild(i);
            viewButton.addEventListener("click", view);
            element.appendChild(viewButton);
        };

    this.update = function (model) {
        uploadButton.innerHTML = model.descriptionValue || "upload";
        var i = document.createElement("i");
        i.className = "Icon fa fa-paperclip";
        uploadButton.appendChild(i);

        this.setColor(model.color);
        this.setFontSize(model.fontSize);

        readOnly = !!model.readOnly;
        key = model.key;
        uploadButton.classList.toggle("ReadOnly", readOnly);

        viewButton.style.display = model.key ? "" : "none";

        if (model.fileType)
            input.setAttribute("accept", model.fileType == "images" ? "image/*" : model.fileType == "pdf" ? ".pdf" : undefined);
    };

    this.setColor = function (value) {
        uploadButton.style.backgroundColor = model.color;
        viewButton.style.backgroundColor = model.color;
    };

    this.setFontSize = function (value) {
        uploadButton.style.fontSize = model.fontSize && model.fontSize + "px";
        viewButton.style.fontSize = model.fontSize && model.fontSize + "px";
    };

    this.trigger = function () {
        if (key) {
            view();
            return true;
        } else if (readOnly)
            return false;
        else {
            input.click();
            return true;
        }
    }

    setup();
}