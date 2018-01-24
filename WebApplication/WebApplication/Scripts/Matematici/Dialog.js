matematici.dialog = function () {

	var
		element,
		container,
		options = {},
		autoCenter,
		closeHandler,

		addButton = function (buttons, button) {
			if (button.textarea) {
				var textArea = $("<textarea />").addClass("Textarea");
				button.content = $("<div />");
				button.content.prepend(textArea);
				if (button.text) button.content.prepend($("<label />").text(button.text));
				if (button.val) textArea.val(button.val);
				if (button.readOnly) textArea.prop("readonly", true).addClass("ReadOnly");
				if (button.handler) textArea.on("click", button.handler);
				if (button.height) textArea.height(button.height);
				container.append(button.content);
			} else if (button.text || button.hidden) {
				button.content = $("<a />").addClass("Button").text((button.icon ? " " : "") + button.text).on("click", function () {
					if (button.handler) {
						if (!button.handler.call(this, button.data)) {
							matematici.dialog.close();
						}
					} else
						matematici.dialog.close();
				});
				if (button.icon) button.content.prepend($("<i />").addClass("fi flaticon-" + button.icon));
				if (button.hidden) button.content.hide();
				if (button.defaultButton) button.content.addClass("DefaultButton");
				if (button.title) button.content.attr("title", button.title);
				if (button.cancelButton) {
					container
						.append($("<a />").attr("href", "#").addClass("CloseButton")
							.append($("<i />").addClass("fi flaticon-times-circle"))
							.on("click", function () {
								button.content.click();
							}));
					button.content.addClass("CancelButton");
				}
				if (button.id) button.content.attr("id");

				buttons.prepend(button.content);
			}
			return matematici.dialog;
		},

		setButtons = function (value) {
			container.children("Buttons").remove();
			var buttons = $("<div />").addClass("Buttons");
			if (value.constructor === Array)
				for (button in value)
					addButton(buttons, value[button]);
			else
				addButton(buttons, value);
			container.append(buttons);
		},

		setCloseHandler = function (value) {
			closeHandler = value;
		},

		show = function () {
			if (element) element.remove();

			element = $("<div />").addClass("Dialog").appendTo(matematici.elements.body);
			container = $("<div />").addClass("Container AutoCenter").appendTo(element);
			result = {};

			if (options.cssClass) {
				element.addClass(options.cssClass);
				matematici.elements.body.addClass("Dialog-" + options.cssClass);
			}
			if (options.title)
				container.append($("<h2 />").html(options.title));

			if (options.content) container.append($("<div />").addClass("Content").append(options.content));
			if (options.buttons) setButtons(options.buttons);
			if (options.closeHandler) setCloseHandler(options.closeHandler);

			matematici.elements.body.addClass("Dialog");
			autoCenter = new matematici.AutoCenter(container);
			autoCenter.initialize();
			element.show();
		};

	return {
		close: function () {
			if (element) {
				if (closeHandler) closeHandler();
				element.remove();
				matematici.elements.body.removeClass("Dialog");
				if (options.cssClass)
					matematici.elements.body.removeClass("Dialog-" + options.cssClass);
				element = null;
				options = {};
				autoCenter.destroy();
				closeHandler = null;
			}
		},

		getData: function () {
			return options.data;
		},

		getSender: function () {
			return options.sender;
		},

		show: function (titleOrSettings, content, buttons) {
			if (content || buttons) {
				options.title = titleOrSettings;
				options.content = content || "";
				options.buttons = buttons || { text: "ok", icon: "check" };
			} else if (titleOrSettings)
				options = titleOrSettings;

			if (options.ajax)
				ajax();
			else
				show();
		},

		error: function (titleOrSettings, content) {
			if (typeof (titleOrSettings) == "string")
				titleOrSettings = { title: titleOrSettings, content: content };
			titleOrSettings.cssClass = "Error";
			titleOrSettings.buttons = { text: "ok", icon: "check", defaultButton: true, cancelButton: true };
			matematici.dialog.show(titleOrSettings);
		},

		popupMenu: function (buttons, data) {
			titleOrSettings = { title: "", content: "", cssClass: "PopupMenu", buttons: buttons, data: data };
			matematici.dialog.show(titleOrSettings);
		},

		getElement: function () {
			return element;
		},

		getResult: function () {
			return result;
		},

		setButtons: function (value) {
			setButtons(value);
		},

		setWidth: function (value) {
			container.width(value);
		},

		setCloseHandler: function (value) {
			setCloseHandler(value);
		}
	};

}();

/*
    shows a modal dialog

    usage: 
        matematici.dialog.show(title, content[, buttons])

		title - type: htmlString or Element or Array or jQuery, description: the title of the dialog
		content - type: htmlString or Element or Array or jQuery, description: the contente of the dialog
        buttons - type: Array or sigle object, description: an Array of or a single PlainObjects, if null a default "ok" button will be created:
            content - type: htmlString or Element or Array or jQuery, description: the button
            - or -
            text - type: String, description: the text of the button (i.e. <a class="Button">[text]</a>)
            handler - type: function, description: called when the button is clicked
            icon - type: function, description: the icon of the button (i.e. <a class="Button"><i class="fi flaticon[icon]"></i> [text]</a>)

    or:
        show(settings) 

        settings - type: PlainObject, description: a set of key/value pairs that configure the dialog

    		title - type: htmlString or Element or Array or jQuery, description: the title of the dialog
	    	content - type: htmlString or Element or Array or jQuery, description: the contente of the dialog
            data - type: PlainObject, description: this object will be available as matematici.dialog.getData() while the dialog is showing
			sender - type: jQuery, description: this object will be available as matematici.dialog.getSender(), e.g. the object of which the click has been made
			cssClass - type: String, description: the custom css class of the dialog element and for the body element (with the prefix "Dialog-")
			closeHandler - type: function, description: the function passed in invoked when the dialog is closed
            buttons - type: Array or sigle object, description: an Array of or a single PlainObjects, if null a default "ok" button will be created:
                content - type: htmlString or Element or Array or jQuery, description: the button
                - or -
                text - type: String, description: the text of the button (i.e. <a class="Button">[text]</a>)
                handler - type: function, description: called when the button is clicked
                icon - type: function, description: the icon of the button (i.e. <a class="Button"><i class="fi flaticon[icon]"></i> [text]</a>)
				hidden - type: boolean, description: the button is hidden
				defaultButton - type: boolean, description: the click action is invoked when user press return
				cancelButton - type: boolean, description: the click action is invoked when user press esc, a close button accociated is displayed (also if this button is hidden)

    or:
        error(title, content) 
        see show(...)

    or:
        error(settings)
        see show(...)

 */