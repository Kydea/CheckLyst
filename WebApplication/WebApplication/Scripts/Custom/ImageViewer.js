matematici.imageViewer = function () {
    var
        popup = function (title) {
            var width = image[0].width;
            matematici.dialog.show({
                title: title,
                content: image,
                buttons: [{
                    hidden: true,
                    cancelButton: true
                }]
            });

            matematici.dialog.setWidth(width);
        };

	return {
		show: function (title, url) {
			matematici.loader.show();
			image = $("<img />").on("load", function () {
				matematici.loader.hide();
				popup(title);
			}).on("error", function (e) {
				matematici.loader.hide();
				matematici.dialog.error("Error", "Unable to load the image");
			}).attr("src", url);
		}
	};
}();