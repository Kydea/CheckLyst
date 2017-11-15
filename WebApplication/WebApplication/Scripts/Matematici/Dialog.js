matematici.dialog = function () {

	var
		element,
		container,
		options = {},
		autoCenter,
		closeHandler,

		ajax = function () {
			$.ajax(options.ajax)
				.done(function (result) {
					options.content = result;
					show();

					if (options.ajax.callback != undefined)
						options.ajax.callback.call();
				})
				.fail(function (jqXHR, textStatus, errorThrown) {
					if (options.errorHandler)
						options.errorHandler.call(matematici.dialog, jqXHR, textStatus, errorThrown);
					else
						matematici.dialog.error("Error", errorThrown);
				});
		},

		addButton = function (buttons, button) {
			if (button.textarea) {
				button.content = $("<div />");
				button.content.prepend($("<textarea />").addClass("Textarea"));
				if (button.text) button.content.prepend($("<label />").text(button.text));
			} else if (button.text || button.hidden) {
				button.content = $("<a />").addClass("Button").text((button.icon ? " " : "") + button.text).on("click", function () {
					if (button.handler) {
						if (!button.handler.call(this, button.data)) {
							matematici.dialog.close();
						}
					} else
						matematici.dialog.close();
				});
				if (button.icon) button.content.prepend($("<i />").addClass("fa fa-" + button.icon));
				if (button.hidden) button.content.hide();
				if (button.defaultButton) button.content.addClass("DefaultButton");
				if (button.cancelButton) {
					container
						.append($("<a />").attr("href", "#").addClass("CloseButton")
							.append($("<i />").addClass("fa fa-times"))
							.on("click", function () {
								button.content.click();
							}));
					button.content.addClass("CancelButton");
				}
				if (button.id) button.content.attr("id");
			}

			buttons.prepend(button.content);
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

		show = function () {
			if (element) matematici.dialog.close();

			element = $("<div />").addClass("Dialog").appendTo(matematici.elements.body);
			container = $("<div />").addClass("Container AutoCenter").appendTo(element);

			if (options.cssClass) {
				element.addClass(options.cssClass);
				matematici.elements.body.addClass("Dialog-" + options.cssClass);
			}
			if (options.title)
				container.append($("<h2 />").html(options.title));

			if (options.content) container.append(options.content);
			if (options.buttons) setButtons(options.buttons);

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
			if (content) {
				options.title = titleOrSettings;
				options.content = content;
				options.buttons = buttons ? buttons : { text: "ok", icon: "check" };
			} else if (titleOrSettings)
				options = titleOrSettings;

			if (options.ajax)
				ajax();
			else
				show();
		},

		error: function (titleOrSettings, content) {
			if (content)
				titleOrSettings = { title: titleOrSettings, content: content };
			titleOrSettings.cssClass = "Error";
			titleOrSettings.buttons = { text: "ok", icon: "check", defaultButton: true, cancelButton: true };
			matematici.dialog.show(titleOrSettings);
		},

		detail: function (settings) {
			matematici.dialog.show({
				ajax: {
					type: "post",
					url: settings.url
				},
				cssClass: "DetailPage",
				data: settings.data, 
				sender: settings.sender
			});
		},

		popupMenu: function (buttons, data) {
			titleOrSettings = { title: "", content: "", cssClass: "PopupMenu", buttons: buttons, data: data };
			matematici.dialog.show(titleOrSettings);
		},

		getElement: function () {
			return element;
		},

		setButtons: function (value) {
			setButtons(value);
		},

		setWidth: function (value) {
			container.width(value);
		},

		setCloseHandler: function (value) {
			closeHandler = value;
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
            icon - type: function, description: the icon of the button (i.e. <a class="Button"><i class="fa fa-[icon]"></i> [text]</a>)

    or:
        show(settings) 

        settings - type: PlainObject, description: a set of key/value pairs that configure the dialog

    		title - type: htmlString or Element or Array or jQuery, description: the title of the dialog
	    	content - type: htmlString or Element or Array or jQuery, description: the contente of the dialog
            ajax - type: PlainObject, description: the prameters to use with ajax call, the result will be used to replace content, errors will be handled
            ajaxErrorHandler - type: function, description: is triggered if ajax call fails, if not set an error dialog is shown
            data - type: PlainObject, description: this object will be available as matematici.dialog.getData() while the dialog is showing, use it for example to customize handlers for ajax content
			sender - type: jQuery, description: this object will be available as matematici.dialog.getSender(), e.g. the object of which the click has been made
			cssClass - tpe: String, description: the custom css class of the dialog element and for the body element (with the prefix "Dialog-")
            buttons - type: Array or sigle object, description: an Array of or a single PlainObjects, if null a default "ok" button will be created:
                content - type: htmlString or Element or Array or jQuery, description: the button
                - or -
                text - type: String, description: the text of the button (i.e. <a class="Button">[text]</a>)
                handler - type: function, description: called when the button is clicked
                icon - type: function, description: the icon of the button (i.e. <a class="Button"><i class="fa fa-[icon]"></i> [text]</a>)
				hidden - type: boolean, description: the button is hidden
				defaultButton - type: boolean, description: the click action is invoked when user press return
				cancelButton - type: boolean, description: the click action is invoked when user press esc, a close button accociated is displayed (also if this button is hidden)

    or:
        error(title, content) 
        see show(...)

    or:
        error(settings)
        see show(...)

	or:
		detail(settings), shows a dialog which contains the detail of a record for CRUD operations
            url - type: string, description: the url of the detail page
            data - type: PlainObject, description: this object will be available as matematici.dialog.getData() while the dialog is showing, use it for example to customize handlers for ajax content
			sender - type: jQuery, description: this object will be available as matematici.dialog.getSender(), e.g. the object of which the click has been made

 */