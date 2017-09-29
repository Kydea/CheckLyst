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
		error = $("div.FormError", element).hide();

	setup = function () {
		element
			.on("click", "a#Simple", function () {
				simple.show();
				paddle.hide();
			})
			.on("click", "a#Sign", function () {
				if (signHandler != undefined)
					signHandler.call(this, login);
			})
			.on("click", "a#Logout", function () {
				if (logoutHandler != undefined)
					logoutHandler.call(this, login);
			})
			.on("click", "a#Cancel", function () {
				matematici.dialog.close();
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

	this.showError = function (value) {
		return error.show().html(value);
	}
};