matematici.Pager = function (handler, pager, form) {
    var
        current,
        count,
        setup = function () {
            pager = pager == undefined ? $(".Pager") : $(pager);
            form = form == undefined ? $("form") : $(form);
            pager.empty();

            current = parseInt($("input.Page", form).val());
            current = current == 0 ? 1 : current;
            count = parseInt($("input.PagesCount", form).val());

            var addPage = function (page) {
                pager.append(
                    $("<a />")
                        .addClass(page == current ? "Current" : "")
                        .data("page", page)
                        .html(page)
                );
            };

            if (count > 1) {
                addPage(1);
                if (current - 5 > 0 && current != 6 && current != 7)
                    addPage("...");

                var i = current - 5;
                while (i <= current + 5) {
                    if (i > 1 && i <= count)
                        addPage(i);
                    i++;
                }

                if (i <= count) {
                    if (current != count - 6)
                        addPage("...");
                    addPage(count);
                }
            }

            $("a", pager).click(function (e) {
                e.preventDefault();
                if ($(this).hasClass("Current"))
                    return;

                $(".Current", pager).removeClass("Current");
                $(this).addClass("Current");

                if (parseInt($(this).data("page")) > 0)
                    $("input.Page", form).val($(this).data("page"));
                else
                    return;

                if (handler !== undefined)
                    handler();
            });
        };

    setup();

    this.setup = function (page, pages) {
        current = page,
        count = pages;
        setup();
    };
};
/*
	usage:
    [pager] and [form] by default are $(".Pager") and $("form")
*/