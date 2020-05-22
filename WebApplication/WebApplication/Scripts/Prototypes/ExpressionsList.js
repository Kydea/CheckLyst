prototypes.ExpressionsList = function () {
    var
        model = prototypes.prototype.getModel(),
        autoCenter,
        element,
        table,
        tableBody,
        noRecord,
        currentIndex,
        list = [],
        verifyButton,
        openButton,
        orderBar,
        valid = true,

        getRow = function (index) {
            return $("tr", tableBody).eq(index);
        },

        toggleMoveButton = function () {
            orderBar.children().eq(0).toggleClass("Off", currentIndex == null || currentIndex == 0);
            orderBar.children().eq(1).toggleClass("Off", currentIndex == null || currentIndex == list.length - 1);
        },

        showDependencies = function () {
            var
                openAgain = function () {
                    open(currentIndex);
                },
                handler = function () {
                    var
                        code = list[currentIndex].code,
                        model = prototypes.prototype.getModel().expressions[code];
                    if (valid)
                        matematici.dependenciesDialog.show("Expression " + (code || "(new)"), model.dependingRelations, model.dependentRelations, openAgain);
                };
            if (matematici.canClose)
                handler();
            else
                matematici.dialog.show("warning", "There are unsaved changes.", [
                    {
                        text: "save", icon: "check", handler: function () {
                            prototypes.prototype.save(function () {
                                verify(handler);
                            });
                        }
                    },
                    { text: "cancel", icon: "times", cancelButton: true, handler: openAgain }
                ]);
        },

        up = function () {
            var
                item = list[currentIndex],
                row = getRow(currentIndex);

            list[currentIndex] = list[currentIndex - 1];
            list[currentIndex - 1] = item;

            row.insertBefore(row.prev());
            --currentIndex;

            toggleMoveButton();
            matematici.canClose = false;
        },

        down = function () {
            var
                item = list[currentIndex],
                row = getRow(currentIndex);

            list[currentIndex] = list[currentIndex + 1];
            list[currentIndex + 1] = item;

            row.insertAfter(row.next());
            ++currentIndex;

            toggleMoveButton();
            matematici.canClose = false;
        },

        select = function (index) {
            if (currentIndex != null) {
                getRow(currentIndex).removeClass("Selected");
                if (currentIndex == index) open(currentIndex);
            }
            currentIndex = index;
            if (currentIndex != null)
                getRow(currentIndex).addClass("Selected");

            toggleMoveButton();
            openButton.toggle(model.state == prototypes.Prototype.states.draft && currentIndex != null);
        },

        remove = function () {
            matematici.canClose = false;
            verifyButton.show();
            getRow(currentIndex).remove();
            list.splice(currentIndex, 1);
            if (list.length == 0) noRecord.appendTo(body);
            select();
        },

        open = function (index) {
            new prototypes.ExpressionEditor(list[index], function (row) {
                matematici.canClose = false;
                verifyButton.show();
                if (index) {
                    list[index] = row;
                    update(index);
                } else
                    select(insert(row, currentIndex));
            }, remove, showDependencies);
        },

        insert = function (row, index) {
            noRecord.remove();
            var tr = $("<tr />")
                .addClass("Handler")
                .append($("<td />").append($("<i />").addClass("fa fa-caret-right")).attr("title", row.serializedValue))
                .append($("<td />").addClass("CodeColumn"))
                .append($("<td />").addClass("DescriptionColumn"))
                .append($("<td />").addClass("Centered")
                    .append($("<i />").addClass("Valid fal"))
                    .append($("<i />").attr("title", "Unique").addClass("Unique fal fa-key"))
                    .append($("<i />").attr("title", "Exception").addClass("Exception fal fa-exclamation"))
                    .append($("<i />").attr("title", "Notification").addClass("Notification fal fa-bell"))
                );

            if (index == null) {
                list.push(row);
                tr.appendTo(tableBody)
                update(list.length - 1);
                return list.length - 1;
            } else {
                list.splice(index + 1, 0, row);
                tr.insertAfter(getRow(index));
                update(index + 1);
                return index + 1;
            }
        },

        update = function (index) {
            var
                row = list[index],
                cells = $("td", getRow(index)),
                commands = cells.last();

            cells.filter(".CodeColumn").text(row.code).attr("title", row.serializedValue);
            cells.filter(".DescriptionColumn").text(row.description).attr("title", row.serializedValue);
            commands.find(".Valid")
                .removeClass("fa-check fa-times fa-question Off")
                .attr("title", row.error || "")
                .addClass(row.edited ? "fa-question Off" : row.error ? "fa-times" : "fa-check");

            commands.find(".Unique").toggleClass("Off", !row.unique);
            commands.find(".Exception").toggleClass("Off", !row.exception);
            commands.find(".Notification").toggleClass("Off", !row.notification);
        },

        updatePrototype = function () {
            model.expressions = {};
            for (var index in list)
                model.expressions[list[index].code] = list[index];
        },

        verify = function (handler) {
            verifyButton.hide();

            updatePrototype();

            $.ajax({
                url: matematici.urls.api + "/Prototypes/Prototype/VerifyExpressions",
                type: "post",
                contentType: "application/json",
                data: JSON.stringify({ checkList: model })
            }).done(function (result) {
                valid = true;
                for (var index in list) {
                    var row = list[index] = result.expressions[list[index].code]
                    valid &= (row.error == null);

                    getRow(index).find("i.Valid")
                        .removeClass("fa-check fa-times fa-question Off")
                        .attr("title", row.error || "")
                        .addClass(row.error == null ? "fa-check" : "fa-times");
                    model.expressions = result.expressions;
                }

                if (result.checkListException) {
                    valid = false;
                    matematici.dialog.error("Error", result.checkListException.Message);
                }

                $("div.ActionBar a.ExpressionsList").toggleClass("Invalid", !valid);
                if (handler) handler.call(this);
            });
        },

        close = function () {
            element.remove();
            matematici.elements.body.removeClass("CheckListGrid");
            autoCenter.destroy();
        },

        setup = function () {
            matematici.elements.body.addClass("CheckListGrid");

            element = $("<div />").addClass("CheckListGrid").appendTo(matematici.elements.body);
            var container = $("<div />").appendTo($("<div />").addClass("Container").appendTo(element).on("click", function (e) {
                e.preventDefault();
                e.stopPropagation();
            }));

            autoCenter = new matematici.AutoCenter(container.parent());
            autoCenter.initialize();

            orderBar = $("<div />")
                .appendTo(container)
                .addClass("OrderBar")
                .append($("<a />").addClass("Icon Up").append($("<i />").addClass("fa fa-caret-up")))
                .append($("<a />").addClass("Icon Down").append($("<i />").addClass("fa fa-caret-down")))
                .on("click", "a.Up:not(.Off)", function (e) {
                    e.preventDefault();
                    up();
                }).on("click", "a.Down:not(.Off)", function (e) {
                    e.preventDefault();
                    down();
                });

            toggleMoveButton();

            var scrollablePanel = $("<div />").addClass("ScrollablePanel").appendTo(container);

            table = $("<table />").addClass("Grid ExpressionsList").appendTo(scrollablePanel)
                .append($("<thead />")
                    .append($("<tr />")
                        .append($("<th />"))
                        .append($("<th />").text("code"))
                        .append($("<th />").text("description"))
                        .append($("<th />"))
                    )
                ).append(tableBody = $("<tbody />")
                    .append(noRecord = $("<tr />").addClass("NoRecord")
                        .append($("<td />").addClass("NoRecord").attr("colspan", "8").text("(no record)"))
                    )
                ).on("click", "tr.Handler td:not(.Inactive)", function () {
                    select($(this).parent().prevAll().size());
                });

            var
                buttons =
                    $("<div />").addClass("Buttons").appendTo(container.parent())
                        .append($("<a />").addClass("Button CancelButton").text("close").prepend($("<i />").addClass("Icon fa fa-times")).on("click", function () {
                            if (verifyButton.is(":visible"))
                                verify(function () {
                                    close();
                                });
                            else {
                                updatePrototype();
                                close();
                            }
                        }))
                        .append(openButton = $("<a />").addClass("Button").text("open").hide().prepend($("<i />").addClass("Icon fa fa-edit")).on("click", function () {
                            open(currentIndex);
                        }))
                        .append(verifyButton = $("<a />").addClass("Button").text("verify").hide().prepend($("<i />").addClass("Icon fa fa-check")).on("click", function () {
                            verify();
                        }))
                        .append($("<a />").addClass("Button").text("add").toggle(model.state == prototypes.Prototype.states.draft).prepend($("<i />").addClass("Icon fa fa-plus")).on("click", function () {
                            open();
                        }));

            for (var code in model.expressions)
                insert(model.expressions[code]);
        };

    setup();
};