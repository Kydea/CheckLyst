instances.Attachment = function (item, model) {
	var
		button,
		input,
		readOnly,

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
            button =
                $("<a />")
                    .addClass("Button Attachment").attr({ href: "#" }).appendTo(item.getElement())
				.on("click", function () {
					if (readOnly) {
						return;
					}

                    input.click();
                });
        };

    this.update = function (model) {
        button
            .html(model.description || "upload")
            .append($("<i />").addClass("Icon fa fa-paperclip"));

        this.setColor(model.color);
		this.setFontSize(model.fontSize);

		readOnly = !!model.readOnly;
		button.toggleClass("ReadOnly", readOnly);

		if (model.fileType)
			input.attr("accept", model.fileType == "images" ? "image/*" : model.fileType == "pdf" ? ".pdf" : undefined);
    };

    this.setColor = function (value) {
        button.css({ backgroundColor: model.color });
    };

    this.setFontSize = function (value) {
        button.css({ fontSize: model.fontSize == 0 ? "" : model.fontSize + "px" });
    };

    setup();
}