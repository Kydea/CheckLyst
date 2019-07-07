prototypes.UploadProperty = function (model, property, options) {
	var
		input,
		handler,
		change = function () {
			var file = input[0].files[0];
			console.log(file);
			console.log(model, property, options);

			// TODO get data from prototype or instance
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
				// Your server script to process the upload
				url: prototypes.prototype.getElement().data("action") + "Upload",
				type: 'POST',

				data: formData,
				cache: false,
				contentType: false,
				processData: false,

				xhr: function () {
					var xhr = $.ajaxSettings.xhr();
					//if (xhr.upload) {
					//	xhr.upload.addEventListener('progress', function (e) {
					//		if (e.lengthComputable) {
					//			$('progress').attr({
					//				value: e.loaded,
					//				max: e.total,
					//			});
					//		}
					//	}, false);
					//}
					return xhr;
				}
			}).fail(function (e) {
				console.log(e);
				new matematici.dialog.error("error", "Impossible to upload the image");
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
			alert("preview");
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
				.append($("<a />").attr("href", "#").addClass("Button").text("preview").append($("<i />").addClass("Icon fa fa-image")).on("click", deleteImage))
				.append($("<a />").attr("href", "#").addClass("Button").text("delete").append($("<i />").addClass("Icon fa fa-trash-alt")).on("click", preview))
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
