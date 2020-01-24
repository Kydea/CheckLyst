prototypes.UploadProperty = function (model, property, options) {
	var
		input,
		previewButton,
		deleteButton,
		handler,
		change = function () {
			if (!input[0].files.length) 
				return;

			var save = function () {
				prototypes.prototype.save(function () {
					var prototypeKey = prototypes.prototype.getPrototypeKey();
					var prototypeModel = prototypes.prototype.getModel();
					var formData = new FormData();
					formData.append("attachment", input[0].files[0]);
					formData.append("id", prototypeKey.id);
					formData.append("version", prototypeKey.version);
					formData.append("code", model.itemId);

					$.ajax({
						url: prototypes.prototype.getElement().data("action") + "UploadAttachment",
						type: 'POST',

						data: formData,
						cache: false,
						contentType: false,
						processData: false
                    }).done(function (result) {
                        model.cloned = false;
						model[property] = result.key;
						previewButton.toggle(true);
						deleteButton.toggle(true);
						matematici.canClose = false;
						if (handler) handler.call(this, model);
						prototypes.prototype.save();
					}).always(function () {
						input.val('');
					});
				});
			};

			matematici.dialog.show("warning", "The prototype will be saved. Do you want to proceed?", [
				{ text: "yes", icon: "check", handler: save },
				{ text: "no", icon: "times", cancelButton: true, handler: function () { input.val(''); } }
			]);			
		},

		deleteImage = function () {
            var
                done = function () {
                    model[property] = null;
                    previewButton.toggle(false);
                    deleteButton.toggle(false);
                    matematici.canClose = false;
                    if (handler) handler.call(this, model);
                    prototypes.prototype.save();
                },
                save = function () {
				    prototypes.prototype.save(function () {
					    var prototypeKey = prototypes.prototype.getPrototypeKey();
					    var prototypeModel = prototypes.prototype.getModel();
					    var formData = {
						    id: prototypeKey.id,
						    version: prototypeKey.version,
						    key: model[property]
					    };

					    $.ajax({
						    url: prototypes.prototype.getElement().data("action") + "DeleteAttachment",
						    type: "POST",
						    data: formData
					    }).done(done);
				    });
                };

            if (model.cloned) {
                model.cloned = false;
                done();
            } else
			    matematici.dialog.show("warning", "The prototype will be saved. Do you want to proceed?", [
				    { text: "yes", icon: "check", handler: save },
				    { text: "no", icon: "times", cancelButton: true }
			    ]);			
		},

		preview = function () {
			var prototypeKey = prototypes.prototype.getPrototypeKey();
			var prototypeModel = prototypes.prototype.getModel();

			var url = prototypes.prototype.getElement().data("action") + "DownloadAttachment?";
			url += "id=" + prototypeKey.id;
			url += "&version=" + prototypeKey.version;
			url += "&key=" + model[property];
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
		input = $("<input />").attr({ type: "file", readOnly: options.readOnly, accept: "image/*"  });
        var form = $("<form />").attr("enctype", "multipart/form-data");

        if (!options.readOnly)
            form.append($("<a />").attr("href", "#").addClass("Button").text("upload").append($("<i />").addClass("Icon fa fa-paperclip")).on("click", function () { input.click(); }));

        form.append(previewButton = $("<a />")
            .attr("href", "#")
            .addClass("Button")
            .text("preview")
            .append($("<i />").addClass("Icon fa fa-image"))
            .on("click", preview)
            .toggle(!!model[property])
        );

        if (!options.readOnly)
            form.append(deleteButton = $("<a />")
                .attr("href", "#")
                .addClass("Button")
                .text("delete")
                .append($("<i />").addClass("Icon fa fa-trash-alt"))
                .on("click", deleteImage)
                .toggle(!!model[property])
            );

		form.append(input);
        valuePanel.append(form);
		input.on("change", change);
	};

	this.hasChanges = function () {
		return !!model[property];
	};

	this.setReadOnly = function (value) {
		options.readOnly = value;
	};
};
