instances.Image = function (item, model) {
    var
        span,
        a,
        title,

		clickHandler = function () {
			if (a.hasClass("ReadOnly") || a.hasClass("GreyYellowButton")) 
				return false;

			var id = instances.instance.getPrototype();
			var version = instances.instance.getVersion();
			var department = instances.instance.getDepartment();
			var prototypeType = instances.instance.getPrototypeType();

			var url = instances.instance.getElement().data("api") + "/Prototypes/Prototype/DownloadAttachment?";
			url += "id=" + id;
			url += "&version=" + version;
			url += "&department=" + department;
			url += "&prototypeType=" + prototypeType;
			url += "&key=" + model.key;
			url += "&Authorization=" + encodeURIComponent("Bearer " + matematici.tokenManager.getAccessToken());

            matematici.imageViewer.show(title, url);
            return true;
        },

        setup = function () {
            span = $("<span />");
            a = $("<a />").addClass("Image Button").append(span).append($("<i />").addClass("Icon fa fa-caret-right")).appendTo(item.getElement()).on("click", clickHandler);
        };

    this.update = function (model) {
        span.html(title = model.descriptionValue || "show image");

        this.setColor(model.color);
		this.setFontSize(model.fontSize);

		a.toggleClass("GreyYellowButton", !model.key || model.key === '');
    };

    this.setColor = function (value) {
        a.css({ color: model.color });
    };

    this.setFontSize = function (value) {
        a.css({ fontSize: model.fontSize && model.fontSize + "px" });
    };

    this.trigger = clickHandler;

	setup();
}