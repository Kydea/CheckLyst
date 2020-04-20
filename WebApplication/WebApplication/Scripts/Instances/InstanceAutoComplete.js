matematici.InstanceAutoComplete = function (node) {
    //this is a light version of autocomplete whitout jquery used only in instances

    var
        autoComplete = this,
        initialized = false,
        options,
        items,
        currentItem,
        iconNode,
        icon,
        labelNode,
        label,
        input,
        list,
        noRow,
        oldValue,
        multiple,
        open,
        readOnly,

        deferredInitialization = function () {
            //all the initialization needed only after first show list

            element = $(node);
            icon = $(iconNode);
            label = $(labelNode);
            options = $("option", element);
            multiple = element.prop("multiple");
            input = $("<input />").addClass("AutoComplete-Filter Scan").attr({ type: "text" }).hide().insertAfter(label);
            list = $("<div />").addClass("AutoComplete-List").insertAfter(input);
            noRow = $("<i />").addClass("fa fa-minus");

            input
                .on("keydown", function (e) {
                    if (e.which == 38 || e.which == 40) { //up and down
                        e.preventDefault();
                        if (!multiple) setCurrentItem(currentItem + (e.which == 38 ? -1 : 1) || 0);
                    } else if (e.which == 13) { //return
                        e.preventDefault();
                        e.stopPropagation();
                        hideList();
                    } else if (e.which == 27) { //esc
                        e.preventDefault();
                        e.stopPropagation();
                        hideList();
                    }

                })
                .on("keyup", function (e) {
                    if (e.which < 37 || e.which > 40) { //arrow keys
                        updateList();
                        if (!multiple) setCurrentItem(0);
                    }
                });

            list
                .on("click", "a", function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    if (multiple)
                        toggleItem($(this).prevAll().size());
                    else {
                        setCurrentItem($(this).prevAll().size());
                        hideList();
                    }
                });

            updateList();

            initialized = true;
        },

        initialize = function (value) {
            node.className += " AutoComplete";
            var parent = node.parentNode;

            iconNode = document.createElement("i");
            iconNode.className = "AutoComplete fa fa-caret-down";
            parent.insertBefore(iconNode, node.nextSibling);

            labelNode = document.createElement("input");
            labelNode.setAttribute("type", "text");
            parent.insertBefore(labelNode, node.nextSibling);

            parent.addEventListener("click", function (e) {
                if (e.target.tagName != "INPUT") {
                    $("input:visible", this).eq(0).focus();
                    e.stopPropagation();
                }
            });

            labelNode.addEventListener("focus", function () {
                showList();
                setTimeout(function () {
                    input.focus();
                    input.select();
                }, 0);
            });
            labelNode.addEventListener("click", function (e) {
                e.stopPropagation();
            });

            iconNode.addEventListener("click", function (e) {
                if (open)
                    hideList();
                else
                    labelNode.focus();
                e.preventDefault();
                e.stopPropagation();
            });

            checkReadOnly();

            matematici.elements.window.on("resize", setWidth);
            matematici.destroyers.push(function (context) {
                destroy();
            });
        },

        isNullable = function () {
            return !!element.data("nullable") || element.attr("data-nullable") == "true";
        },

        setValue = function () {
            var value = element.val();
            if (multiple) {
                texts = [];
                items.each(function () {
                    var item = $(this).removeClass("Selected");
                    for (var index in value)
                        if (item.data("value") == value[index]) {
                            item.addClass("Selected");
                            texts.push(item.text());
                            continue;
                        }
                });
                var text = texts.join(", ");
                label.val(text).attr("title", text);
            }
            else
                items.each(function (index) {
                    if ($(this).data("value") == value) {
                        setCurrentItem(index);
                        return false;
                    }
                });
        },

        setWidth = function () {
            var width = label.outerWidth(), position = label.position();
            input.width(width - parseInt(input.css("padding-left")) - parseInt(input.css("padding-right")) - parseInt(input.css("border-left-width")) - parseInt(input.css("border-right-width")));
            items.each(function () {
                var itemWidth = $(this).outerWidth();
                if (itemWidth > width)
                    width = itemWidth;
            });
            list.outerWidth(width);

            var labelPosition = {
                'left': position.left + parseInt(label.css('marginLeft'), 10),
                'top': position.top + label.outerHeight(true)
            };

            input.css({ top: label.offset().top + label.outerHeight(), left: label.offset().left });
            list.css({ top: label.offset().top + input.outerHeight(), left: label.offset().left });

            icon.css({
                'left': labelPosition.left + width - 24,
                'top': position.top + parseInt(label.css('marginTop'), 10)
            });
        },

        checkReadOnly = function () {
            if (node.classList.contains("ReadOnly")) {
                readOnly = true;
                labelNode.className += " ReadOnly";
                labelNode.readOnly = true;
                iconNode.remove();
            } else {
                readOnly = false;
                labelNode.classList.remove("ReadOnly");
                labelNode.readOnly = false;
                if (input)
                    icon.insertAfter(input);
                else
                    labelNode.parentNode.insertBefore(iconNode, labelNode.nextSibling);
            }
		},

		destroy = function () {
			matematici.elements.window.off("resize", setWidth);
		},

		toggleItem = function (index) {
            var
                item = items.eq(index),
                value = item.data("value"),
                option = options.filter("[value='" + value + "']"),
				on = !option.prop("selected");

			items.eq(index).toggleClass("Selected", on);
			option.prop('selected', on);

			var texts = [];
			options.each(function () {
				var option = $(this);
				if (option.prop("selected"))
					texts.push(option.text());
			});
			var text = texts.join(", ");
            label.val(text).attr("title", text);

            element.trigger("toggle", [ value, on ]);
		},

		setCurrentItem = function (value) {
			if (currentItem != null) items.eq(currentItem).removeClass("Selected");
			currentItem = items.size() == 0 ? null : value < 0 ? 0 : items.size() <= value ? items.size() - 1 : value;
			if (currentItem != null) {
				var
					item = items.eq(currentItem).addClass("Selected"),
					itemHeight = item.outerHeight(),
					scroll = list.scrollTop(),
					listHeight = list.height(),
					top = currentItem * itemHeight;
				if (top < scroll)
					list.scrollTop(top);
				else if (top + itemHeight - listHeight > scroll)
					list.scrollTop(top + itemHeight - listHeight);

				element.val(item.data("value"));
                label.val(item.hasClass("None") ? "" : item.text());
			}
		},

        showList = function () {
            if (!initialized) deferredInitialization();
			if (readOnly) return;

            if (oldValue == null) oldValue = element.val() || "";
            element.data("autoComplete", autoComplete);
			$("select.AutoComplete.Open").each(function () {
				$(this).data("autoComplete").hideList();
			});

			open = true;
			element.addClass("Open");
			label.addClass("Open");
			input.show();
			matematici.elements.body.append(input);
			matematici.elements.body.append(list);
			input.css({ top: label.offset().top + label.outerHeight(), left: label.offset().left });
			list.css({ top: label.offset().top + input.outerHeight(), left: label.offset().left });
			list.show();
			setValue();
			setTimeout(function () {
				$(document).on("click.autocomplete", hideList)
				$(document).on("keydown.autocomplete", function (e) {
					if (e.which == 13)
						hideList();
				});
                matematici.elements.body.children(".Popup").on("scroll.autocomplete", hideList);
			}, 0);
		},

		hideList = function () {
			$(document)
				.off("click.autocomplete")
				.off("keydown.autocomplete");
            matematici.elements.body.children(".Popup").off("scroll.autocomplete");
			list.hide();
			input.hide();
			if (!multiple) setCurrentItem(null);
			label.removeClass("Open");
			element.removeClass("Open");
			open = false;
			if (oldValue != (element.val() || "")) element.change();
			oldValue = null;
			input.insertAfter(label);
			list.insertAfter(label);
		},

		updateList = function () {
			list.empty();
			if (isNullable())
				list.append($("<a />").addClass("None").attr("href", "#").append($("<i />").text(element.data("nullable") === true ? "(none)" : element.data("nullable"))));

			var count = 0;
			options
				.each(function () {
					var
						option = $(this),
						filter = input.val().toUpperCase();
					if (filter == "" || option.text().toUpperCase().indexOf(filter) >= 0) {
						list.append(
							$("<a />")
								.attr("href", "#")
								.data("value", option.attr("value"))
								.text(option.text())
								.attr("title", option.text())
								.toggleClass("Selected", option.prop("selected"))
						);
						++count;
					}
				});

			items = $("a", list);
			if (items.size() == 0)
				list.append(noRow);
			setWidth();
		};

	this.refresh = function () {
		icon.show();
		options = $("option", element);
		updateList();
		setValue();
	};

	this.getControl = function () {
		return label;
    };

    this.getValue = function () {
        return element.val();
    }

    this.setValue = function (value) {
        node.value = label.value = value;
	};

	this.isNullable = isNullable;

	this.hideList = hideList;

	this.initialize = initialize;

	this.checkReadOnly = checkReadOnly;

};
