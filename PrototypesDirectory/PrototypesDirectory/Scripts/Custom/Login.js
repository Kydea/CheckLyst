matematici.Login = function (selector, signHandler, logoutHandler) {
	var
		login = this,
		element = $(selector),
		simple = $("div.Simple", element),
		paddle = $("div.PinPaddle", element),
		display = $("input", paddle),
		userName = $("input#UserName", simple),
		password = $("input#Password", simple),
		pin = $("input#PIN", element),
		confirmPin = $("input#ConfirmPIN", element),
		note = $("input#Note", element),

		togglePaddle = function (value) {
			simple.toggle(!value);
			paddle.toggle(value);
			$("a#Simple", element).toggle(value);
			setTimeout(function () {
				matematici.popup.center();
			}, 0);
		},

		setup = function () {
			element
				.on("click", "a#Simple", function () {
					togglePaddle(false);
				})
				.on("click", "a#Sign", function () {
					if (signHandler != undefined)
						signHandler.call(this, login);
				})
				.on("click", "a#Logout", function () {
					if (logoutHandler != undefined)
						logoutHandler.call(this, login);
				})
				.on("click", "div.Digits a", function () {
					var
						digit = "" + $(this).data("digit"),
						value = display.val(),
						length = value.length;

					if (digit == "<")
						display.val(value.substring(0, length - 1));
					else if (length < display.attr("maxlength"))
						display.val(value + digit);
				});
		};

	setup();

	this.togglePaddle = togglePaddle;

	this.isPaddle = function () {
		return paddle.is(":visible");
	};

	this.isSimple = function () {
		return simple.is(":visible");
	};

	this.getUserName = function () {
		return userName.val();
	};

	this.getPassword = function () {
		return this.isSimple() ? password.val() : null;
	};

	this.getNote = function () {
		return note.val();
	};

	this.getPIN = function () {
		return String(display.val() || pin.val());
	};

	this.getConfirmPIN = function () {
		return String(confirmPin.val());
	};
};