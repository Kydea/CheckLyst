prototypes.Note = function (item, model) {
    var
        p,

        setSize = function () {
            var element = item.getElement();
            element.toggleClass("Icon", model.icon);
        },

        setup = function () {
            $("<i />").addClass("fa fa-info").appendTo(item.getElement());
            p = $("<p />").appendTo(item.getElement());
            setSize();
        };

    this.update = function (model) {
        p.html(model.description ? model.description.split('\n').join('<br />') : "(specify the content of the note)");
    };

    this.setSize = setSize;

    setup();
}