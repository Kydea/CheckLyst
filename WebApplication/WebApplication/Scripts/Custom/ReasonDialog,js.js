matematici.ReasonDialog = function (handler) {
    var
        input,  

        show = function () {
            matematici.dialog.show({
                title: "Reason",
                content: $("<div />")
                    .append(
                        $("<label />")
                            .attr("for", "DetailPageReasonInput")
                            .text("Reason for change")
                    )
                    .append(
                        input = $("<input />")
                            .attr("id", "DetailPageReasonInput")
                            .attr("type", "text")
                            .attr("autocomplete", "off")
                    ),
                buttons: [
                    {
                        text: "save", icon: "check", defaultButton: true, handler: function () {
                            if (input.val().trim() === "") {
                                input.addClass("Error");
                                return true;
                            } else {
                                input.removeClass("Error");
                                handler(input.val());
                                return false;
                            }
                        }
                    },
                    { text: "cancel", hidden: false, icon: "times", cancelButton: true }
                ],
            });

            setTimeout(function () { input[0].focus(); }, 1);
        }

    this.show = show;
}
