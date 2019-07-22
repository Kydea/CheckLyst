instances.Attachment = function (item, model) {
	var
		button,
		input,

		onChange = function () {
			var data = new FormData();
			data.append("attachment", input[0].files[0]);
			new instances.Event(instances.Events.uploadAttachment, model.id, data, undefined, undefined, true).done(function (result) {
				instances.instance.update(result);
				input.val('');
			}).trigger();
		},

        setup = function () {
			input = $("<input />").addClass("Hidden").attr({ type: "file" }).on("change", onChange).appendTo(item.getElement());
            button =
                $("<a />")
                    .addClass("Button Attachment").attr({ href: "#" }).appendTo(item.getElement())
                    .on("click", function () {
                        input.click();
                    });
        };

    this.update = function (model) {
        button
            .html(model.description || "upload")
            .append($("<i />").addClass("Icon fa fa-paperclip"));

        this.setColor(model.color);
        this.setFontSize(model.fontSize);
    };

    this.setColor = function (value) {
        button.css({ backgroundColor: model.color });
    };

    this.setFontSize = function (value) {
        button.css({ fontSize: model.fontSize == 0 ? "" : model.fontSize + "px" });
    };

    setup();
}