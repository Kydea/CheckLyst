matematici.AutoComplete = function (node) {
    //this is a light version of autocomplete whitout jquery in initialization used only in instances to make the process faster

    var
        autoComplete = this,
        initialized = false,
        element,
        valuesMap,
        options,
        fullHeight,
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

            initialized = true;
        },

        initialize = function (value) {
            matematici.AutoComplete.map.set(node, autoComplete);
            matematici.AutoComplete.list.push(autoComplete);

            multiple = node.hasAttribute("multiple");

            node.classList.add("AutoComplete");
            var parent = node.parentNode;

            iconNode = document.createElement("i");
            iconNode.className = "AutoComplete fa fa-caret-down";
            parent.insertBefore(iconNode, node.nextSibling);

            labelNode = document.createElement("input");
            labelNode.setAttribute("type", "text");
            labelNode.classList = "AutoComplete";
            parent.insertBefore(labelNode, node.nextSibling);

            labelNode.addEventListener("focus", function () {
                showList();
                setTimeout(function () {
                    if (!initialized) deferredInitialization();
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

            setValue();
            checkReadOnly();

            fullHeight = node.classList.contains("FullHeight");
            if (fullHeight) {
                deferredInitialization();
                label.hide();
                icon.addClass("FullHeight");
                input.addClass("FullHeight");
                list.insertAfter(input).addClass("FullHeight");
            }

            matematici.elements.window.on("resize", setWidth);
        },

        isNullable = function () {
            return node.getAttribute("data-nullable") != null;
        },

        createMap = function () {
            valuesMap = new Map();

            for (var index = 0; index < node.options.length; ++index) {
                var option = node.options[index];
                valuesMap.set(option.value, option.text);
            }
        },

        setValue = function () {
            var
                value = node.value,
                texts = [],
                text;

            if (value == null || value == "")
                text = "";
            else {
                if (valuesMap == null) createMap();
                if (multiple) {
                    for (var index = 0, length = node.options.length; index < length; index++) {
                        var option = node.options[index];
                        if (option.selected)
                            texts.push(option.text);
                    }
                    text = texts.join(", ");
                } else
                    text = value && valuesMap.get(value) || "";
            }

            labelNode.value = text;
            labelNode.setAttribute("title", text);

            if (!open) return;

            if (multiple) {
                value = element.val();
                texts = [];
                items.each(function () {
                    var item = $(this).removeClass("Selected");
                    for (var index in value)
                        if (item.data("value") == value[index]) {
                            item.addClass("Selected");
                            continue;
                        }
                });
            } else if (items)
                items.each(function (index) {
                    if ($(this).data("value") == value) {
                        setCurrentItem(index);
                        return false;
                    }
                });
        },

        resizeList = function () {
            if (fullHeight) return;
            var width = labelNode.offsetWidth;
            input.width(width - parseInt(input.css("padding-left")) - parseInt(input.css("padding-right")) - parseInt(input.css("border-left-width")) - parseInt(input.css("border-right-width")));
            list.outerWidth(width);
            input.css({ top: label.offset().top + label.outerHeight() + 1, left: label.offset().left });
            list.css({ top: label.offset().top + input.outerHeight() + 1, left: label.offset().left });
        },

        setWidth = function () {
            if (open) resizeList();
            iconNode.style.left = labelNode.offsetLeft + labelNode.offsetWidth - 24 + "px";
            iconNode.style.top = labelNode.offsetTop + "px";
        },

        checkReadOnly = function () {
            if (node.classList.contains("ReadOnly")) {
                readOnly = true;
                labelNode.classList.add("ReadOnly");
                labelNode.readOnly = true;
                iconNode.remove();
                if (open) setTimeout(hideList, 0);
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

            if (oldValue == null) oldValue = node.value || "";

            if (matematici.AutoComplete.current) matematici.AutoComplete.current.hideList();
            matematici.AutoComplete.current = autoComplete;
            options = $("option", element);
            updateList();
			open = true;
			element.addClass("Open");
			label.addClass("Open");
			input.show();
			matematici.elements.body.append(input);
			matematici.elements.body.append(list);
            resizeList();
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
            if (oldValue != (node.value || "")) node.dispatchEvent(new Event('change', { 'bubbles': true }));
			oldValue = null;
			input.insertAfter(label);
            list.insertAfter(label);
            matematici.AutoComplete.current = null;
		},

		updateList = function () {
            list.empty();
            var nullValue = node.getAttribute("data-nullable");
            if (nullValue != null) 
                list.append($("<a />").addClass("None").attr("href", "#").append($("<i />").text(nullValue == "" ? "(none)" : nullValue)));

			options
				.each(function () {
					var
						option = $(this),
						filter = input.val().toUpperCase();
					if (filter == "" || option.text().toUpperCase().indexOf(filter) >= 0)
						list.append(
							$("<a />")
								.attr("href", "#")
								.data("value", option.attr("value"))
								.text(option.text())
								.attr("title", option.text())
								.toggleClass("Selected", option.prop("selected"))
						);
				});

			items = $("a", list);
			if (items.size() == 0)
				list.append(noRow);
			setWidth();
		};

    autoComplete.refresh = function () {
        iconNode.style.display = "block";
        setWidth();
        setValue();
    };

    autoComplete.refreshValues = function () {
        valuesMap = null;
        setValue();
    };

    autoComplete.getControl = function () {
		return label;
    };

    autoComplete.getValue = function () {
        return element.val();
    }

    autoComplete.setValue = function (values) {
        if (Array.isArray(values)) {
            var valuesSet = new Set(values);
            for (var optionIndex = 0, optionsCount = node.options.length; optionIndex < optionsCount; ++optionIndex) {
                var option = node.options[optionIndex];
                option.selected = valuesSet.has(option.value);
            }
        } else
            node.value = values;
        setValue();
	};

    autoComplete.isNullable = isNullable;

    autoComplete.hideList = hideList;

    autoComplete.initialize = initialize;

    autoComplete.destroy = destroy;

    autoComplete.checkReadOnly = checkReadOnly;
};

matematici.AutoComplete.map = new WeakMap();
matematici.AutoComplete.list = [];
matematici.AutoComplete.current = null;
matematici.AutoComplete.get = function (select) {
    return matematici.AutoComplete.map.get(select);
};

matematici.initializers.push(function (context) {
    $("select", context).each(function () {
        if (!matematici.AutoComplete.map.has(this))
            new matematici.AutoComplete(this).initialize();
    });
});

matematici.destroyers.push(function (context) {
    for (var index in matematici.AutoComplete.list)
        matematici.AutoComplete.list[index].destroy();
    matematici.AutoComplete.map = new WeakMap();
    matematici.AutoComplete.list = [];
    matematici.AutoComplete.current = null;
});
