prototypes.ExpressionsList = function (button) {
    var
        model = prototypes.prototype.getModel(),
        element,
        container,
        table,
        body,
        noRecord,
        currentIndex,
        list = [],
        removeButton,
        dependencyButton,
        verifyButton,
        expressionsInUse,

        up = function (icon) {
            var
                row = $(icon).closest("tr"),
                index = row.prevAll().length,
                item = list[index];

            list[index] = list[index - 1];
            list[index - 1] = item;

            row.insertBefore(row.prev());
        },

        down = function (icon) {
            var
                row = $(icon).closest("tr"),
                index = row.prevAll().length,
                item = list[row.prevAll().length];

            list[index] = list[index + 1];
            list[index + 1] = item;

            row.insertAfter(row.next());
        },

        select = function (index) {
            if (currentIndex != null) body.children().eq(currentIndex).removeClass("Selected").addClass("Handler").children().slice(0, 2).attr("contenteditable", false);
            if (model.state == prototypes.Prototype.states.draft) {
                currentIndex = index;
                var used = false;
                if (currentIndex != null) {
                    var
                        item = list[currentIndex],
                        code = item.code,
                        dependingRelations = item.dependingRelations,
                        dependentRelations = item.dependentRelations;
                    used = !!expressionsInUse[code] || (dependingRelations && dependingRelations.length > 0) || (dependentRelations && dependentRelations.length > 0);

                    body.children().eq(currentIndex).addClass("Selected").removeClass("Handler")
                        .children((used ? ":not(:nth-child(2))" : "") + ":not(:first-child):not(:last-child):not(:nth-last-child(2)):not(:nth-last-child(3)):not(:nth-last-child(4))").attr("contenteditable", true).eq(0).focus();
                }
                removeButton.toggle(true || currentIndex != null && !used); //always enabled to avoid dead locks when a user delete a referenced item
                dependencyButton.toggle(currentIndex != null && used);
            }
        },

        add = function (row) {
            noRecord.remove();
            list.push(row);
            row.node = $("<tr />")
                .addClass("Handler")
                .appendTo(table)
                .append($("<td />").append($("<i />").addClass("fa fa-caret-right")))
                .append($("<td />").addClass("CodeColumn").text(row.code))
                .append($("<td />").addClass("DescriptionColumn").text(row.description))
                .append($("<td />").addClass("ExpressionColumn").text(row.expression))
                .append($("<td />").addClass("ValidColumn Centered").append($("<i />").attr("title", row.error || "").addClass("fa" + (row.valid == null ? "" : " fa-" + (row.valid ? "check" : "times")))))
                .append($("<td />").addClass("UniqueColumn Centered").toggleClass("Off", !row.unique).append($("<i />").addClass("fa fa-key")))
                .append($("<td />").addClass("ExceptionColumn Centered").toggleClass("Off", !row.exception).append($("<i />").addClass("fa fa-exclamation")))
                .append($("<td />").addClass("Order Centered Inactive").append($("<i />").addClass("Up fa fa-caret-up")).append($("<i />").addClass("Down fa fa-caret-down")))
        },

        update = function (row) {
            row.node.find(".ValidColumn i")
                .removeClass("fa-check fa-times fa-question")
                .attr("title", row.error || "")
                .addClass("fa-" + (row.valid ? "check" : "times"));
        },

        remove = function () {
            body.children().eq(currentIndex).remove();
            list.splice(currentIndex, 1);
            if (list.length == 0) noRecord.appendTo(body);
            select();
        },

        updateModel = function () {
            model.expressions = {};
            for (var index in list)
                model.expressions[list[index].code] = {
                    serializedValue: list[index].expression,
                    description: list[index].description,
                    error: list[index].error,
                    dataType: list[index].dataType,
                    innerDataType: list[index].innerDataType,
                    unique: list[index].unique,
                    exception: list[index].exception,
                    dependingRelations: list[index].dependingRelations,
                    dependentRelations: list[index].dependentRelations
                };
        },

        checkCodes = function () {
            for (var index in list) {
                var code = list[index].node.find(".CodeColumn").text();
                code = code.replace(/(?:^\w|[A-Z]|\b\w|\s+)/g, function (match, index) {
                    if (+match === 0) return ""; // or if (/\s+/.test(match)) for white spaces
                    //return index == 0 ? match.toLowerCase() : match.toUpperCase();
                    return match;
                });
                list[index].node.find(".CodeColumn").text(code);
                if (!matematici.codeRegularExpression.test(code)) {
                    matematici.dialog.error("Invalid code", "The code " + code + " is not valid");
                    return false;
                }
            }
            return true;
        },

        map = function () {
            for (var index in list) {
                list[index].code = list[index].node.find(".CodeColumn").text();
                list[index].description = list[index].node.find(".DescriptionColumn").text();
                list[index].expression = list[index].node.find(".ExpressionColumn").text();
            }
            updateModel();
        },

        verify = function (handler) {
            verifyButton.hide();

            if (!checkCodes()) return;
            map();

            $.ajax({
                url: matematici.urls.api + "/Prototypes/Prototype/VerifyExpressions",
                type: "post",
                contentType: "application/json",
                data: JSON.stringify({ checkList: model })
            }).done(function (result) {
                var valid = true;
                for (var index in list) {
                    var expressionValidity;
                    try {
                        expressionValidity = list[index].valid = result.expressions[list[index].code].error == null;
                        list[index].description = result.expressions[list[index].code].description;
                        list[index].error = result.expressions[list[index].code].error;
                        list[index].dataType = result.expressions[list[index].code].dataType;
                        list[index].innerDataType = result.expressions[list[index].code].innerDataType;
                        list[index].unique = result.expressions[list[index].code].unique;
                        list[index].exception = result.expressions[list[index].code].exception;
                        list[index].dependingRelations = result.expressions[list[index].code].dependingRelations;
                        list[index].dependentRelations = result.expressions[list[index].code].dependentRelations;
                    } catch (error) {
                        list[index].error = error;
                        list[index].dataType = "Error";
                    }
                    valid &= expressionValidity;
                    update(list[index]);
                    updateModel();
                }

                if (result.checkListException) {
                    valid = false;
                    matematici.dialog.error("Error", result.checkListException.Message);
                }

                button.toggleClass("Invalid", !valid);
                if (handler) handler.call(this);
            });
        },

        close = function () {
            setTimeout(function () {
                element.remove();
                matematici.elements.body.removeClass("CheckListGrid");
            }, 0); //handles the blur event handler of the text boxes
        },

        checkElementExpressionsInUse = function (element) {
            if (element.hiddenExpressionCode) expressionsInUse[element.hiddenExpressionCode] = true;
            if (element.readOnlyExpressionCode) expressionsInUse[element.readOnlyExpressionCode] = true;
            if (element.validExpressionCode) expressionsInUse[element.validExpressionCode] = true;
            if (element.countDownCondition != 6 && element.countDownToCode) expressionsInUse[element.countDownToCode] = true;
        },

        checkGroupExpressionsInUse = function (group) {
            checkElementExpressionsInUse(group);
            for (var itemIndex in group.items) {
                var item = group.items[itemIndex]
                if (item.type == "Group")
                    checkElementExpressionsInUse(item);
                else
                    checkGroupExpressionsInUse(item);
            }
        },

        setup = function () {
            matematici.elements.body.addClass("CheckListGrid");
            element = $("<div />").addClass("CheckListGrid").appendTo(matematici.elements.body);
            container = $("<div />").addClass("Container").appendTo(element).on("click", function (e) {
                e.preventDefault();
                e.stopPropagation();
            });
            table = $("<table />").addClass("Grid ExpressionsList").appendTo(container)
                .append($("<thead />")
                    .append($("<tr />")
                        .append($("<th />"))
                        .append($("<th />").text("code"))
                        .append($("<th />").text("description"))
                        .append($("<th />").text("expression"))
                        .append($("<th />").text("valid"))
                        .append($("<th />").text("unique"))
                        .append($("<th />").text("exception"))
                        .append($("<th />"))
                    )
                ).append(body = $("<tbody />")
                    .append(noRecord = $("<tr />").addClass("NoRecord")
                        .append($("<td />").attr("colspan", "6").text("(no record)"))
                    )
                ).on("click", "tr td.Order i.Up", function (e) {
                    e.stopPropagation();
                    up(this);
                }).on("click", "tr td.Order i.Down", function (e) {
                    e.stopPropagation();
                    down(this);
                }).on("click", "tr.Handler td:not(.Inactive)", function () {
                    select($(this).parent().prevAll().size());
                }).on("click", "tr td:not(.Inactive)", function () {
                    verifyButton.show();
                }).on("focus", "td", function () {
                    var textBox = $(this);
                    textBox.data("oldValue", textBox.text());
                }).on("blur", "td", function () {
                    var textBox = $(this);
                    if (textBox.data("oldValue") != textBox.text()) {
                        matematici.canClose = false;
                        list[currentIndex].valid = null;
                    }
                    $("td.ValidColumn i", $(this).parent()).removeClass("fa-check fa-remove").addClass("fa-question");
                }).on("click", "tr.Selected td.UniqueColumn", function () {
                    matematici.canClose = false;
                    verifyButton.show();
                    $("td.ValidColumn i", $(this).parent()).removeClass("fa-check fa-remove").addClass("fa-question");
                    list[currentIndex].unique = !list[currentIndex].unique;
                    $(this).toggleClass("Off", !list[currentIndex].unique);
                }).on("click", "tr.Selected td.ExceptionColumn", function () {
                    matematici.canClose = false;
                    verifyButton.show();
                    $("td.ValidColumn i", $(this).parent()).removeClass("fa-check fa-remove").addClass("fa-question");
                    list[currentIndex].exception = !list[currentIndex].exception;
                    $(this).toggleClass("Off", !list[currentIndex].exception);
                });


            var
                buttons =
                    $("<div />").addClass("Buttons").appendTo(container)
                        .append($("<a />").addClass("Button").text("close").prepend($("<i />").addClass("Icon fa fa-times")).on("click", function () {
                            if (verifyButton.is(":visible"))
                                verify(function () {
                                    close();
                                });
                            else
                                close();
                        }))
                        .append(verifyButton = $("<a />").addClass("Button").text("verify").toggle(model.state == prototypes.Prototype.states.draft).prepend($("<i />").addClass("Icon fa fa-check")).on("click", function () {
                            verify();
                        }))
                        .append($("<a />").addClass("Button").text("add").toggle(model.state == prototypes.Prototype.states.draft).prepend($("<i />").addClass("Icon fa fa-plus")).on("click", function () {
                            add({ code: "", expression: "", error: null });
                            verifyButton.show();
                            matematici.canClose = false;
                            select(list.length - 1);
                        }))
                        .append(removeButton = $("<a />").addClass("Button").text("remove").prepend($("<i />").addClass("Icon fa fa-trash-alt")).on("click", function () {
                            remove();
                            matematici.canClose = false;
                        }).hide())
                        .append(dependencyButton = $("<a />").addClass("Button").text("dependencies").prepend($("<i />").addClass("Icon fa fa-sitemap")).on("click", function () {
                            var
                                handler = function () {
                                    var model = prototypes.prototype.getModel().expressions[list[currentIndex].code];
                                    matematici.dependenciesDialog.show(model.dependingRelations, model.dependentRelations);
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
                                    { text: "cancel", icon: "times", cancelButton: true }
                                ]);
                        }).hide());


            for (var code in model.expressions)
                add({
                    code: code,
                    expression: model.expressions[code].serializedValue,
                    description: model.expressions[code].description,
                    error: model.expressions[code].error,
                    valid: model.expressions[code].error == null,
                    dataType: model.expressions[code].dataType,
                    innerDataType: model.expressions[code].innerDataType,
                    unique: model.expressions[code].unique,
                    exception: model.expressions[code].exception,
                    dependingRelations: model.expressions[code].dependingRelations,
                    dependentRelations: model.expressions[code].dependentRelations
                });

            expressionsInUse = {};
            checkGroupExpressionsInUse(model.header);
            for (var pageIndex in model.pages) {
                var page = model.pages[pageIndex];
                checkElementExpressionsInUse(page);
                for (var groupIndex in page.groups)
                    checkGroupExpressionsInUse(page.groups[groupIndex]);
            }

            verify();

            canClose = true;
        };

    setup();
};