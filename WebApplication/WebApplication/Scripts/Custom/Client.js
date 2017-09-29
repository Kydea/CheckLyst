matematici.Client = function () {
	var
		handlers = [],

		active = false,

		trigger = function () {
			var id = window.name;
			console.log("Client id: " + id);
			for (var index in handlers)
				handlers[index](id);
		},

		set = function (id) {
			window.name = id;
			trigger();
		},

		get = function () {
			if (!!window.name)
				trigger();
			else
				$.ajax({
					url: matematici.urls.client,
					type: "post"
				}).done(function (result) {
					set(result.Id);
				}).fail(function (jqXHR, textStatus, errorThrown) {
					console.log(jqXHR, textStatus, errorThrown);
					matematici.dialog.error("server error", "Client unique identifier unavailable");
				});
		},

		toggleActivity = function (value) {
			active = value;
			if (value)
				localStorage.activeClientId = window.name;
		},

		setup = function () {
			matematici.elements.body.on("mousedown", function (e) {
				toggleActivity(true);
			});

			$(window).on("blur focus", function (e) {
				var prevType = $(this).data("prevType");

				if (prevType != e.type)
					switch (e.type) {
						case "blur":
							console.log("inactive");
							toggleActivity(false);
							break;
						case "focus":
							console.log("active");
							toggleActivity(true);
							break;
					}

				$(this).data("prevType", e.type);
			});
		};

	if (matematici.appClientId == "")
		get();
	else
		set(parseInt(matematici.appClientId));

	this.addHandler = function (handler) {
		var id = window.name;
		if (!!id)
			handler(id);
		else
			handlers.push(handler)
	};

	this.createNew = function (handler) {
		window.name = "";
		if (handler) matematici.client.addHandler(handler);
		get();
	};

	this.getId = function () {
		return window.name;
	};

	this.isActive = function () {
		return active && localStorage.activeClientId === window.name;
	};

	this.toggleActivity = function (value) {
		toggleActivity(value);
	};

	setup();
};