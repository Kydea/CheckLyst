matematici.Help = function () {
    var
        helpButton,
        help = this,
        element,
        container,
        toc,
        content,
        closeButton,
        contextualHelp,
        currentPage,

        scroll = function (paragraph) {
            if (paragraph) content.animate({ 'scrollTop': $("#" + paragraph, content).offset().top }, 500);
        },

        load = function (page, paragraph) {
            if (currentPage == page)
                scroll(paragraph);
            else
                $.ajax(matematici.urls.help + "/" + (page || ""), {
                    method: 'POST',
                    cache: false
                }).done(function (result, status, request) {
                    content.html(result);
                    scroll(paragraph);
                }).fail(function (e) {
                    alert("error loading help");
                    console.log(e);
                });

            currentPage = page;
        },

        show = function () {
            matematici.elements.body.addClass("Help");
            load();
        },

        hide = function () {
            matematici.elements.body.removeClass("Help");
        },

        setCode = function (page, paragraph) {
            contextualHelp.data({ page: page, paragraph: paragraph }).parent().toggle(!!page);
        },

        setup = function () {
            helpButton = $("a#HelpButton", matematici.elements.header).on("click", function (e) {
                e.preventDefault();
                show();
            });

            element = matematici.elements.body.children("div.Help");
            container = element.children("div.Container").on("click", "div.TOC a, a.Help", function (e) {
                e.preventDefault();
                var anchor = $(this);
                load(anchor.data("page") || currentPage, anchor.data("paragraph"));
            });;
            content = container.children("div.Content");
            toc = container.children("div.TOC");

            contextualHelp = $("li.ContextualHelp a", toc);

            closeButton = container.children("a.CloseButton").on("click", function (e) {
                e.preventDefault();
                hide();
            });
        };

    help.setCode = function (page, paragraph) {
        setCode(page, paragraph);
    };

    help.show = function () {
        show();
    };

    setup();
};