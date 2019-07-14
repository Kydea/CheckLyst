prototypes.UploadProperty = function (model, property, options) {
	var
		input,
		handler,
		change = function () {
			var prototypeKey = prototypes.prototype.getPrototypeKey();
			var prototypeModel = prototypes.prototype.getModel();
			var formData = new FormData();
			formData.append("attachment", input[0].files[0]);
			formData.append("id", prototypeKey.id);
			formData.append("version", prototypeKey.version);
			formData.append("attachmentTemporaryKey", prototypes.prototype.getAttachmentTemporaryKey());
			formData.append("code", model.itemId);
			if (prototypeModel.department) formData.append("department", prototypeModel.department);
			if (prototypeModel.prototypeType) formData.append("prototypeType", prototypeModel.prototypeType);

			$.ajax({
				url: prototypes.prototype.getElement().data("action") + "UploadAttachment",
				type: 'POST',

				data: formData,
				cache: false,
				contentType: false,
				processData: false,
			}).done(function (result) {
				console.log(result);
				model.key = result.key;
				matematici.canClose = false;
				if (handler) handler.call(this, model);
			}).always(function () {
				input.val('');
			});
		},

		deleteImage = function () {
			alert("delete image");
		},

		preview = function () {
			var prototypeKey = prototypes.prototype.getPrototypeKey();
			var prototypeModel = prototypes.prototype.getModel();

			var url = prototypes.prototype.getElement().data("action") + "DownloadAttachment?";
			url += "id=" + prototypeKey.id;
			url += "&version=" + prototypeKey.version;
			url += "&department=" + (prototypeModel.department || '');
			url += "&prototypeType=" + (prototypeModel.prototypeType || '');
			url += "&attachmentTemporaryKey=" + (prototypes.prototype.getAttachmentTemporaryKey() || '');
			url += "&key=" + model.key;
			url += "&Authorization=" + encodeURIComponent("Bearer " + matematici.tokenManager.getAccessToken());

			matematici.imageViewer.show("Preview", url);
		};

	this.setHandler = function (value) {
		handler = value;
		return this;
	};

	this.create = function (valuePanel) {
		valuePanel.addClass("Upload");
		options = options || {};
		input = $("<input />")
			.attr({ type: "file", readOnly: options.readOnly })
			.val(model[property]);
		var form =
			$("<form />")
				.attr("enctype", "multipart/form-data")
				.append($("<a />").attr("href", "#").addClass("Button").text("upload").append($("<i />").addClass("Icon fa fa-link")).on("click", function () { input.click(); }))
				.append($("<a />").attr("href", "#").addClass("Button").text("preview").append($("<i />").addClass("Icon fa fa-image")).on("click", preview))
				.append($("<a />").attr("href", "#").addClass("Button").text("delete").append($("<i />").addClass("Icon fa fa-trash-alt")).on("click", deleteImage))
				.append(input);
		input.on("change", change);

		if (!options.readOnly)
			valuePanel.append(form);
	};

	this.hasChanges = function () {
		return !!model[property + "HasValue"];
	};

	this.setReadOnly = function (value) {
		options.readOnly = value;
	};

	console.log(model, property, options);
};
