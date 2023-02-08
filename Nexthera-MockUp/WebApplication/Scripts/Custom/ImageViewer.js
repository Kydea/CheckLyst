matematici.imageViewer = function () {
    var
        image,
        width,
        height,

        calculateRatio = function () {
            var
                frame = $(window),
                ratio = Math.min((frame.width() - 80) / width, (frame.height() - 80) / height);

            return ratio;
        },

        resize = function (ratio) {
            image.css({ width: width * ratio, height: height * ratio });
            matematici.dialog.setWidth(image.width());
            matematici.dialog.center();
        },

        expand = function () {
            resize(Math.max(1, calculateRatio()));
        },

        compress = function () {
            resize(Math.min(1, calculateRatio()));
        },

        popup = function (title) {
            width = image[0].width;
            height = image[0].height;

            var
                expandButton = $("<a />").attr("href", "#").append($("<i />").addClass("fas fa-expand")),
                compressButton = $("<a />").attr("href", "#").append($("<i />").addClass("fas fa-compress")),
                content = $("<div />").append(image);

            if (height > 0 && width > 0) {
                content.append($("<div />").addClass("ViewButtons").append(expandButton).append(compressButton));
                expandButton.on("click", expand);
                compressButton.on("click", compress)
            }

            matematici.dialog.show({
                title: title,
                content: content,
                cssClass: "ImageViewer",
                buttons: [{
                    hidden: true,
                    cancelButton: true
                }]
            });

            compress();
        };

    return {
        show: function (title, url) {

            if (matematici.appMode)
                window.webkit.messageHandlers.instances.postMessage("file: " + url);
            else {
                matematici.loader.show();
                image = $("<img />").on("load", function () {
                    matematici.loader.hide();
                    popup(title);
                }).on("error", function (e) {
                    matematici.loader.hide();
                    matematici.dialog.error("Error", "Unable to load the image");
                }).attr("src", url + "&" + new Date().getTime());
            }
        }
    };
}();