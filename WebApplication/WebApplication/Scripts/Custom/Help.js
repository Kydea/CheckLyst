matematici.Help = function () {
	var
		helpButton,
		help = this,
		element,
		container,
		toc,
		content,
		closeButton,
		currentPage,
		currentParagraph,
		contextualPage,
		contextualParagraph,
		backButton,
		history,

		scroll = function (paragraph) {
			if (paragraph) element.animate({ scrollTop: $("#" + paragraph, content).offset().top }, 300);
		},

		load = function (page, paragraph, back) {
			if (!back) history.push({ page: currentPage, paragraph: currentParagraph });
			if (currentPage == page)
				scroll(paragraph);
			else {
				content.empty();
				backButton.toggle(history.length > 1);
				$.ajax(matematici.urls.help + "/" + (page || ""), {
					method: 'POST',
					cache: false
				}).done(function (result, status, request) {
					content.html(result);
					scroll(paragraph);
				});
				currentPage = page;
			}

			currentParagraph = paragraph;
		},

		show = function (page, paragraph) {
			matematici.elements.body.addClass("Help");
			history = []
			load(page || contextualPage, paragraph || contextualParagraph);
		},

		hide = function () {
			matematici.elements.body.removeClass("Help");
		},

		setCode = function (page, paragraph) {
			contextualPage = page;
			contextualParagraph = paragraph;
		},

		back = function () {
			var item = history.pop();
			load(item.page, item.paragraph, true);
		},

		setup = function () {
			helpButton = $("a#HelpButton", matematici.elements.header).on("click", function (e) {
				e.preventDefault();
				show();
			});

			element = matematici.elements.body.children("div.Help");
			container = element.children("div.Container").on("click", "div.TOC ul a, a.Help", function (e) {
				e.preventDefault();
				var
					anchor = $(this),
					page = anchor.data("page") || currentPage,
					paragraph = anchor.data("paragraph");

				load(page, paragraph);
			});;
			content = container.children("div.Content");
			toc = container.children("div.TOC");

			backButton = $("div.Back a", toc).on("click", function () {
				back();
			}).parent();

			closeButton = container.children("a.CloseButton").on("click", function (e) {
				e.preventDefault();
				hide();
			});
		};

	help.setCode = function (page, paragraph) {
		setCode(page, paragraph);
	};

	help.show = function (page, paragraph) {
		show(page, paragraph);
	};

	setup();
};