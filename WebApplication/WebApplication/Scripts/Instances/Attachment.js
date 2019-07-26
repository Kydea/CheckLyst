instances.Attachment = function (item, model) {
	var
		uploadButton,
		viewButton,
		input,
		readOnly,
		key,

		onChange = function () {
			var data = new FormData();
			data.append("attachment", input[0].files[0]);
			new instances.Event(instances.Events.uploadAttachment, model.id, data, undefined, undefined, true).done(function (result) {
				instances.instance.update(result);
				input.val('');
			}).error(function () {
				input.val('');
			}).trigger();
		},

        setup = function () {
			input = $("<input />")
				.addClass("Hidden")
				.attr({ type: "file" })
				.on("change", onChange)
				.appendTo(item.getElement());

			uploadButton =
				$("<a />")
				.addClass("Button Attachment").attr({ href: "#" }).appendTo(item.getElement())
				.on("click", function () {
					if (readOnly) {
						return;
					}

					input.click();
				});

			viewButton =
				$("<a />")
					.text("View")
					.append($("<i />").addClass("Icon fa fa-eye"))
					.addClass("Button ViewAttachment").attr({ href: "#" }).appendTo(item.getElement())
					.on("click", function () {
						var url = matematici.urls.instances.api + "/DownloadAttachment?";
						url += "code=" + instances.instance.getCode();
						url += "&key=" + key;
						url += "&Authorization=" + encodeURIComponent("Bearer " + matematici.tokenManager.getAccessToken());

						if (model.fileType === "images") {
							matematici.imageViewer.show("Preview", url);
						}
						if (model.fileType === "pdf") {
							window.open(url);
						}
					});
        };

    this.update = function (model) {
		uploadButton
            .html(model.description || "upload")
            .append($("<i />").addClass("Icon fa fa-paperclip"));

        this.setColor(model.color);
		this.setFontSize(model.fontSize);

		readOnly = !!model.readOnly;
		key = model.key;
		uploadButton.toggleClass("ReadOnly", readOnly);
		viewButton.toggleClass("ReadOnly", readOnly);

		viewButton.toggle(!!model.key);

		if (model.fileType)
			input.attr("accept", model.fileType == "images" ? "image/*" : model.fileType == "pdf" ? ".pdf" : undefined);
    };

    this.setColor = function (value) {
		uploadButton.css({ backgroundColor: model.color });
		viewButton.css({ backgroundColor: model.color });
    };

    this.setFontSize = function (value) {
		uploadButton.css({ fontSize: model.fontSize == 0 ? "" : model.fontSize + "px" });
		viewButton.css({ fontSize: model.fontSize == 0 ? "" : model.fontSize + "px" });
    };

    setup();
}