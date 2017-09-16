prototypes.ActionsProperty = function (buttons) {
    this.create = function (valuePanel) {
        valuePanel.addClass("Action");
        for (var index in buttons)
            if (buttons[index].visible == null || buttons[index].visible)
                valuePanel.append($("<a />")
                    .attr("href", "#")
                    .addClass("Button")
                    .append($("<i />").addClass(buttons[index].icon ? "fa fa-" + buttons[index].icon : ""))
                    .text(buttons[index].text)
                    .data("handler", buttons[index].handler)
                    .on("click", function (e) {
                        e.preventDefault();
                        $(this).data("handler").call(this);
                    })
                );
    }

    this.hasChanges = function () {
        return false;
    }
}

