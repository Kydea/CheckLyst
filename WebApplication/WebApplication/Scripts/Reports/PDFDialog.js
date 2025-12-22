reports.PDFDialog = function (options, tableHead, parameters) {
    var
        show = function () {
            var
                content = $("<div />"),

                lastOrientation = localStorage.ReportOrientation || "0",

                fontSizeSelect = $("<select />"),
                formatSelect = $("<select />"),
                orientationSelect = $("<select />");

            content.append($("<label />").text("Font size"));
            var index = 0
            var option = $("<option />").text("(Auto)").attr("value", index).attr("selected", "selected");
            fontSizeSelect.append(option);
            for (index = 5; index < 21; ++index) {
                option = $("<option />").text(index + "px").attr("value", index);
                fontSizeSelect.append(option);
            }
            content.append(fontSizeSelect);

            content.append($("<label />").text("Paper format").css("margin-top", 10));
            var formats = [
                "(Auto)",
                "A0",
                "A1",
                "A2",
                "A3 ",
                "A4",
                "Letter"
            ]
            for (index = 0; index < formats.length; ++index) {
                option = $("<option />").text(formats[index]).attr("value", index - 1);
                if (index == 0)
                    option.attr("selected", "selected");
                formatSelect.append(option);
            }
            content.append(formatSelect);

            content.append($("<label />").text("Orientation").css("margin-top", 10));
            index = 0
            option = $("<option />").text("Portrait").attr("value", index++);
            if (lastOrientation != 1) option.attr("selected", "selected")
            orientationSelect.append(option);
            option = $("<option />").text("Landscape").attr("value", index++);
            if (lastOrientation == 1) option.attr("selected", "selected")
            orientationSelect.append(option);
            content.append(orientationSelect);

            var columnWidths = [];
            if (tableHead != null) 
                $("th", tableHead).each(function () {
                    columnWidths.push(this.offsetWidth);
                });

            setTimeout(function () {
                matematici.dialog.show("download pdf", content, [
                    {
                        text: "OK", icon: "check", defaultButton: true, handler: function () {
                            $("body").css("format", formatSelect.val());

                            localStorage.ReportOrientation = orientationSelect.val();

                            $.ajax({
                                url: matematici.urls.reports.api + "/Viewer/Download",
                                type: "post",
                                data: {
                                    id: options.id,
                                    run: options.run,
                                    parameters: parameters,
                                    fontSize: fontSizeSelect.val() == "0" ? "12" : fontSizeSelect.val(),
                                    pageFormat: formatSelect.val() == -1 ? 0 : formatSelect.val(),
                                    orientation: orientationSelect.val(),
                                    columnWidths: formatSelect.val() == -1 ? columnWidths : null
                                }
                            }).done(function (result) {
                                //window.open(matematici.urls.reports.api + "/Viewer/Download?Token=" + result.token);
                                location = matematici.urls.reports.api + "/Viewer/Download?Token=" + result.token;
                            });
                        }
                    },
                    { hidden: true, cancelButton: true }
                ]);
            }, 0);
        }

    this.show = show;
};