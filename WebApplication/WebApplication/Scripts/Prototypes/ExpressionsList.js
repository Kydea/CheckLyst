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
        verifyButton,

        select = function (index) {
			if (currentIndex != null) body.children().eq(currentIndex).removeClass("Selected").addClass("Handler").children().slice(0, 2).attr("contenteditable", false);
        	if (model.state == prototypes.Prototype.states.draft)
        	{
        	    currentIndex = index;
        	    var used = false;
				if (currentIndex != null) {
					used = model.expressionsInUse && model.expressionsInUse.includes(list[currentIndex].code);

        	        body.children().eq(currentIndex).addClass("Selected").removeClass("Handler")
                        .children((used ? ":not(:nth-child(2))" : "") + ":not(:first-child):not(:last-child):not(:nth-last-child(2))").attr("contenteditable", true).eq(0).focus();
        	    }
        		removeButton.toggle(currentIndex != null && !used);
        	}
        },

        add = function (row) {
            noRecord.remove();
            list.push(row)
            row.node = $("<tr />")
                .addClass("Handler")
                .appendTo(table)
				.append($("<td />").append($("<i />").addClass("fi flaticon-arrow-right")))
                .append($("<td />").addClass("CodeColumn").text(row.code))
                .append($("<td />").addClass("ExpressionColumn").text(row.expression))
				.append($("<td />").addClass("ValidColumn Centered").append($("<i />").attr("title", row.error || "").addClass("fi" + (row.valid == null ? "" : " flaticon-" + (row.valid ? "check" : "times")))))
				.append($("<td />").addClass("UniqueColumn Centered").toggleClass("Off", !row.unique).append($("<i />").addClass("fi flaticon-key-o")));

            if (model.state == prototypes.Prototype.states.draft)
            	matematici.canClose = false;
        },

		update = function (row) {
		    row.node.find(".ValidColumn i")
				.removeClass("flaticon-check flaticon-times flaticon-question")
				.attr("title", row.error || "")
				.addClass("flaticon-" + (row.valid ? "check" : "times"));
		},

        remove = function () {
            body.children().eq(currentIndex).remove();
            list.splice(currentIndex, 1);
            if (list.length == 0) noRecord.appendTo(body);
            select();
            matematici.canClose = false;
        },

        updateModel = function () {
            model.expressions = {};
            for (var index in list) 
                model.expressions[list[index].code] = { serializedValue: list[index].expression, error: list[index].error, dataType: list[index].dataType, unique: list[index].unique };
        },

        checkCodes = function () {
            for (var index in list) {
                var code = list[index].node.find(".CodeColumn").text();
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
		        list[index].expression = list[index].node.find(".ExpressionColumn").text();
            }
			updateModel();
		},

        verify = function (handler) {
            verifyButton.hide();

            if (!checkCodes()) return;
        	map();

            $.ajax({
                url: matematici.urls.prototypes.verifyExpressions,
                type: "post",
                contentType: "application/json",
                data: JSON.stringify(model)
            }).done(function (result) {
                if (result.error) 
                    matematici.dialog.error("Error", result.error);
                else {
                    var valid = true;
                    for (var index in list) {
                        var expressionValidity;
                        try {
                            expressionValidity = (list[index].valid = result.expressions[list[index].code].error == null);
                            list[index].error = result.expressions[list[index].code].error;
                            list[index].dataType = result.expressions[list[index].code].dataType;
                            list[index].unique = result.expressions[list[index].code].unique;
                        } catch (error) {
                            list[index].error = error;
                            list[index].dataType = "Error";
                        }
                        valid &= expressionValidity;
                        update(list[index]);
                        updateModel();
                    }

                    button.toggleClass("Invalid", !valid);
                    if (handler) handler.call(this);
                }
            }).fail(function (jqXHR, textStatus, errorThrown) {
                console.log("Server error", jqXHR, textStatus, errorThrown);
                alert("Server error: " + errorThrown);
            });
        },

        setup = function () {
            element = $("<div />").addClass("CheckListGrid").appendTo(matematici.elements.body);
            container = $("<div />").addClass("Container").appendTo(element).on("click", function (e) {
                e.preventDefault();
                e.stopPropagation();
            });
            table = $("<table />").addClass("Grid ExpressionsList").appendTo(container)
                .append($("<thead />")
                    .append($("<tr />")
                        .append($("<td />"))
                        .append($("<th />").text("code"))
                        .append($("<th />").text("expression"))
                        .append($("<th />").text("valid"))
                        .append($("<th />").text("unique"))
                    )
                ).append(body = $("<tbody />")
                    .append(noRecord = $("<tr />").addClass("NoRecord")
                        .append($("<td />").attr("colspan", "5").text("(no record)"))
                    )
                ).on("click", "tr.Handler", function () {
                    select($(this).prevAll().size());
                }).on("blur", "td", function () {
                    list[currentIndex].valid = null;
					$("td.ValidColumn i", $(this).parent()).removeClass("flaticon-check flaticon-remove").addClass("flaticon-question");
                    verifyButton.show();
                }).on("click", "tr.Selected td.UniqueColumn", function () {
                    list[currentIndex].unique = !list[currentIndex].unique;
                    $(this).toggleClass("Off", !list[currentIndex].unique);
                });


            var
                buttons =
                    $("<div />").addClass("Buttons").appendTo(container)
						.append($("<a />").addClass("Button").text("close").prepend($("<i />").addClass("fi flaticon-times")).on("click", function () {
                        if (verifyButton.is(":visible")) 
                            verify(function () {
                                element.remove();
                            });
                        else
                            element.remove();
                    }))
						.append(verifyButton = $("<a />").addClass("Button").text("verify").toggle(model.state == prototypes.Prototype.states.draft).prepend($("<i />").addClass("fi flaticon-check")).on("click", function () {
                        verify();
                    }))
						.append($("<a />").addClass("Button").text("add").toggle(model.state == prototypes.Prototype.states.draft).prepend($("<i />").addClass("fi flaticon-plus")).on("click", function () {
                        add({ code: "", expression: "", error: null });
                        select(list.length - 1);
                    }))
						.append(removeButton = $("<a />").addClass("Button").text("remove").prepend($("<i />").addClass("fi flaticon-trash-o")).on("click", function () {
                        remove();
                    }).hide());
                

            for (var code in model.expressions)
                add({
                    code: code,
                    expression: model.expressions[code].serializedValue,
                    error: model.expressions[code].error,
                    valid: model.expressions[code].error == null,
                    dataType: model.expressions[code].dataType,
                    unique: model.expressions[code].unique
                });
        };

    setup();
}