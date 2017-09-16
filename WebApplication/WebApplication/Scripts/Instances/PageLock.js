instances.PageLock = function () {
    var
        model = null,
        labels = $("div.TabContainer div.TabLabels a i.fa-lock", instances.instance.getElement()),

        format = function (date) {
            date = new Date(date);
            return matematici.dateFormatter.both(date);
        },

        createContent = function (index) {
            var content =
                $("<div />")
                    .append($("<span />").text("locked by: "))
                    .append($("<strong />").text(model.Pages[index].Lock.User))
                    .append($("<br />"))
                    .append($("<span />").text("locked on: "))
                    .append($("<strong />").text(format(model.Pages[index].Lock.Date)))
                    .append($("<br />"))
                    .append($("<span />").text("updated on: "))
                    .append($("<strong />").text(format(model.Pages[index].Lock.Date)))
                    .append($("<br />"))
                    .append($("<span />").text("client id: "))
                    .append($("<strong />").text(model.Pages[index].Lock.ClientId));
            return content;
        },

        setup = function () {
            labels.on("click", function (e) {
                e.preventDefault();
                e.stopPropagation();

            	var
					index = $(this).parent().prevAll().size(),
					unlockHandler = function () {
						var buttons = $(this).closest("div.Buttons");

						$("div.Errrors", buttons.parent()).remove();
						new instances.Event(instances.Events.unlockPage, index)
							.done(function (result) {
								if (result.Error) {
									$("<div />").addClass("Errors").text(result.Error).insertAfter(buttons).show();
								} else {
									instances.instance.update(result);
									matematici.dialog.close();
								}
							})
							.trigger();

						return true;
					},
                    handler = function () {
                        setTimeout(function() {
                        	matematici.dialog.show($("<span><i class=\"fa fa-lock\"></i> Page locked</span>"), "Please confirm you want to unlock the page.",
								[
									{ text: "unlock the page", icon: "exclamation", handler: unlockHandler },
									{ text: "cancel", icon: "remove", cancelButton: true }
								]
							)
                        }, 0);
                    };
                matematici.dialog.show($("<span><i class=\"fa fa-lock\"></i> Page locked</span>"), createContent(index), [{ text: "close", icon: "remove", cancelButton: true }, { text: "unlock the page", icon: "exclamation", handler: handler }]);
            });
        };

    this.update = function (newModel) {
        model = newModel;
        for (var index in model.Pages)
            labels.eq(index).toggle(model.Pages[index].Locked);
    };

    setup();
}