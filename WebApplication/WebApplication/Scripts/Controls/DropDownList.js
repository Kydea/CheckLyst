matematici.DropDownList = function (selector) {
	var
		dropDownList = this,
		element = $(selector),
		options = $("option", element),
		input,
		list,

		setup = function () {
			element.val("");
			input = $("<input />").attr({ type: "text", "for": element.attr("id") }).insertAfter(element).width(element.width() - 26)
				.on("focus", function () {
					showList();
					input.select();
					if (input.val() == "")
						updateList();
				})
				.on("click", function (e) {
					e.stopPropagation();
				})
				.on("keydown", function (e) {
					if (e.which == 40) {
						//down
						e.preventDefault();
						list.focus();
						list.scrollTop();
						list.val($("option", list).eq(0).attr("value"));
					}
				})
				.on("keyup", function () {
					updateList();
					showList();
				});
			list = $("<select />").addClass("AutoComplete-List").insertAfter(input).width(element.width())
				.on("change", function () {
					element.val(list.val()).change();
					input.val(options.filter(":selected").text());
				})
				.on("click", function () {
					hideList();
				});
			$("<i />").addClass("DropDownList fi flaticon-freccia-giu").insertAfter(input).css("margin-left", element.width() - 16)
				.on("click", function (e) {
					e.stopPropagation();
					input.focus();
				});
			updateList();
		},

		showList = function () {
			list.show();
			setTimeout(function () {
				$(document).on("click.dropdownlist", hideList)
				$(document).on("keydown.dropdownlist", function (e) {
					if (e.which == 13) {
						//return
						list.change()
						hideList();
					}
				});
			}, 0);
		},

		hideList = function () {
			$(document)
				.off("click.dropdownlist")
				.off("keydown.dropdownlist");
			list.change().hide();
		},

		updateList = function () {
			list.empty();
			var count = 0;
			options
				.each(function () {
					var
						option = $(this),
						filter = input.val().toUpperCase();
					if (filter == "" || option.text().toUpperCase().indexOf(filter) >= 0) {
						list.append(option.clone());
						++count;
					}
				});
			list
				.attr("size", Math.min(5, Math.max(count, 2)))
				.val("");
		};

	setup();
};