instances.PageLock = function () {
    var
        model = null,

        format = function (date) {
            date = new Date(date);
            return matematici.dateFormatter.both(date);
        },

        createContent = function (index) {
			var content =
				$("<div />")
					.append($("<span />").text("locked by: "))
					.append($("<strong />").text(model.pages[index].lock.user))
					.append($("<br />"))
					.append($("<span />").text("locked on: "))
					.append($("<strong />").text(format(model.pages[index].lock.date)))
					.append($("<br />"))
					.append($("<span />").text("updated on: "))
					.append($("<strong />").text(format(model.pages[index].lock.date)));
            return content;
        },

		setup = function () {
			labels = $("div.TabContainer div.TabLabels a i.flaticon-lock-o", instances.instance.getElement());
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
								instances.instance.update(result);
								matematici.dialog.close();
							})
							.trigger();

						return true;
					},
                    handler = function () {
                        setTimeout(function() {
							matematici.dialog.show($("<span><i class=\"fi flaticon-lock-o\"></i> Page locked</span>"), "Please confirm you want to unlock the page.",
								[
									{ text: "unlock the page", icon: "exclamation", handler: unlockHandler },
									{ text: "cancel", icon: "times", cancelButton: true }
								]
							)
                        }, 0);
                    };
				matematici.dialog.show($("<span><i class=\"fi flaticon-lock-o\"></i> Page locked</span>"), createContent(index), [{ text: "close", icon: "times", cancelButton: true }, { text: "unlock the page", icon: "exclamation", handler: handler }]);
            });
        };

	this.update = function (newModel) {
		model = newModel;
		var user = matematici.tokenManager.getUserCode();
        for (var index in model.pages)
			labels.eq(index).toggle(!model.readOnly && !!model.pages[index].lock && model.pages[index].lock.user != user);
    };

    setup();
}