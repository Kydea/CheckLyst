matematici.Breadcrumbs = function (selector) {
    var element = $(selector);

    this.set = function (items) {
        element.empty();
        if (items == null)
            element.slideUp();
        else {
            element.slideDown();
            for (index in items) {
                var item = items[index];
                if (item.content == null)
                    console.log(new Error("content not found in bredcrumbs item"));

                element.append((item.url == null ? $("<span />") : $("<a />").attr("href", item.url)).html(item.content));
            }
        }
    }
};
