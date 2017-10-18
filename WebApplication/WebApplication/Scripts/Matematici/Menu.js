matematici.Menu = function (selector) {

	var
		menu = $(selector),
		rollingOver = false,
		menuButton = $("a#MenuButton"),
		moved = false,

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

		hideItems = function () {
			rollingOver = false;
			$(document).off(matematici.touch.isEnabled() ? "touchend" : "click", hideItems);
			hideItem();
		},

		toggleItemsByRole = function () {
			var roles = "," + localStorage.getItem("roles");
			$("li[data-role]", menu).each(function () {
				var item = $(this)
				if (roles.indexOf("," + item.data("role") + "#") < 0)
					item.remove();
			});
		},

		onClick = function (e) {
			e.preventDefault();

			var
				item = $(this).parent(),
				alreadySelected = item.hasClass("Selected");

			if (rollingOver && alreadySelected) return;

			hideItem();
			$(document).off(matematici.touch.isEnabled() ? "touchend" : "click", hideItems);

			if (!alreadySelected) {
				setTimeout(function () { $(document).on(matematici.touch.isEnabled() ? "touchend" : "click", hideItems); }, 0);
				showItem(item);
			}
		}, setup = function () {

			toggleItemsByRole();

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
				});

			menuButton.on("click", function (e) {
				e.preventDefault();
				menuButton.toggleClass("Selected");
				if (menuButton.hasClass("Selected"))
					menu.stop(true).slideDown();
				else
					menu.stop(true).slideUp();
			});
		}

	setup();
};
