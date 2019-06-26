prototypes.UploadProperty = function (model, property, options) {
    var
        input,
        handler,
        change = function () {
            var file = input[0].files[0];
            console.log(file);

            $.ajax({
                // Your server script to process the upload
                url: 'upload.php',
                type: 'POST',

                data: new FormData($('form')[0]),
                cache: false,
                contentType: false,
                processData: false,

                xhr: function () {
                    var xhr = $.ajaxSettings.xhr();
                    if (xhr.upload) {
                        xhr.upload.addEventListener('progress', function (e) {
                            if (e.lengthComputable) {
                                $('progress').attr({
                                    value: e.loaded,
                                    max: e.total,
                                });
                            }
                        }, false);
                    }
                    return xhr;
                }
            });

            return;

            var value = input.val();
            if (value == "" && options.number) value = 0;
            if (check) 
                for(var index in check)
                    if (!check[index].action.call(this, value)) {
                        new matematici.dialog.error("error", check[index].message);
                        return;
                    }

            model[property] = value;
            matematici.canClose = false;
            if (handler) handler.call(this, model);
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
                .append($("<a />").attr("href", "#").addClass("Button").text("upload").append($("<i />").addClass("fi flaticon-link-o")))
                .append(input)
                .on("click", "a", function () { input.click(); });
        input.on("change", change);

        if (!options.readOnly)
            valuePanel.append(form);
    };

    this.hasChanges = function () {
        return !!model[property + "HasValue"];
    };

    this.setReadOnly = function (value) {
        options.readOnly = value
    };
}

