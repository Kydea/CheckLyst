matematici.Menu = function (selector) {

	var
		menu = $(selector),
		rollingOver = false,
		menuButton = $("#MenuButton"),
		mobileBadge = menu.prev(), 
		moved = false,
		notifications = new Map();

		isMobileMode = function () {
			return menuButton.is(":visible");
		},

		showItem = function (item) {
			var ul = item.addClass("Selected").find("ul").stop(true);
			if (isMobileMode())
				ul.css("opacity", 1).slideDown();
			else
				ul.fadeTo(300, 1);
		},

		hideItem = function (item) {
			if (item === undefined) item = $("li.Selected", menu);
			var ul = item.removeClass("Selected").find("ul").stop(true);
			if (isMobileMode())
				ul.slideUp(function () { $(this).hide(); });
			else
				ul.fadeTo(300, 0, function () { $(this).hide(); });
		},

		onClick = function (e) {
			e.preventDefault();

			var
				item = $(this).parent(),
				selected = item.hasClass("Selected");

			if (rollingOver && selected) return;

			hideItem();
			if (!selected) 
				showItem(item);
		},

		setup = function () {
			menu
				.on("touchstart", function () {
					moved = false;
				})
				.on("touchmove", function () {
					moved = true;
				})
				.on("touchend, click", "span.MenuItem", onClick)
				.on("mouseover", "li.MenuItem", function () {
					if (!isMobileMode()) {
						rollingOver = true;
						showItem($(this));
					}
				}).on("mouseout", "li.MenuItem", function () {
					if (!isMobileMode())
						hideItem($(this));
				}).on("touchend click", "li.MenuItem a", function () {
					if (!moved && isMobileMode() && menuButton.hasClass("Selected")) {
						hideItem();
						menuButton.trigger("click");
					}
				});

			menu.on("pointerup", "a.LightNotification", function () {
				if ($(this).parent().attr("data-badge") > 0)
					matematici.clock.updateLightNotifications();
			})

			menuButton.on("click", function (e) {
				e.preventDefault();
				menuButton.toggleClass("Selected");
				if (menuButton.hasClass("Selected"))
					menu.stop(true).slideDown();
				else
					menu.stop(true).slideUp();
			});
		};

	this.toggleItems = function () {
		$("li[data-role]", menu).each(function () {
            var
                item = $(this),
                roles = item.data("role").split(","),
                hasRole = false;

            for (var role in roles) 
                hasRole = hasRole || matematici.tokenManager.checkRole(roles[role].trim() + "#");

            item.toggle(hasRole).toggleClass("Hidden", !hasRole);
		});

		$(".MenuItem .SubMenu", menu).each(function () {
			var item = $(this);
			item.parent().toggle(item.children().filter(":not(.Hidden)").length > 0);
		});

        var currentCompany = matematici.tokenManager.getCompany();
		$("li[data-company]", menu).each(function () {
			var
				item = $(this),
				visible = currentCompany == item.data("company");

			item.toggle(visible).toggleClass("Hidden", !visible);
		});
	};

	this.setNotificationBadge = function (href, value) {
		const link = document.querySelector(`a[href="${href}"]`);
		if (!link) {
			console.error(`menu item for ${href} not found`);
			return
		}

		var li = link.closest("li");
		li.setAttribute("data-badge", value == 0 ? "" : Math.min(99, value));

		notifications.set(href, value);

		if (li.classList.contains("MenuItem")) return;

		//works only for one notification per submenu, at the moment this is enough
		li = li.closest("li.MenuItem");
		li.setAttribute("data-badge", value == 0 ? "" : Math.min(99, value));

		const sum = [...notifications.values()].reduce((n, v) => n + v, 0);
		mobileBadge.attr("data-badge", sum == 0 ? "" : Math.min(99, sum));
	};

	setup();
};
