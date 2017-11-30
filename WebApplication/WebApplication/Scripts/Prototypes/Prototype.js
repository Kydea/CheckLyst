prototypes.Prototype = function (code, version, isNew) {
	var
        prototype = this,
        element = $("div#Content"),
        page = new prototypes.Group($("div.Page", element), "Page", prototype),
        header = new prototypes.Group($("div#Header", element), "Group", prototype),
        pageIndex,
        title,
        tab,
        pageLock,
        model,
        selectedItem,
        itemHandler,
		updateHandler,

        completeModel = function () {
        	var analyzeElement = function (node) {
        		switch (node.TypeName) {
        			case "Group":
        				for (var itemIndex in node.Items || [])
        					analyzeElement(node.Items[itemIndex]);
        				break;
        			case "Signature":
        				if (node.Code)
        					model.Signatures[node.Code] = node;
        				break;
        		}
        	};

        	model.TypeName = "Prototype";
        	model.Signatures = {};

        	analyzeElement(model.Header);
        	for (var pageIndex in model.Pages || []) {
        		var page = model.Pages[pageIndex];
        		for (var groupIndex in page.Groups || [])
        			analyzeElement(page.Groups[groupIndex])
        	}
        },

        update = function () {
        	completeModel();
        	title.text(model.Description || "(Prototype title)");

        	tab.clear();
        	for (var index in model.Pages)
        		tab.add(model.Pages[index].Description);

        	if (model.State == prototypes.Prototype.states.draft) tab.add($("<i >").addClass("Add fa fa-plus"));
        	page.setIndex(pageIndex = pageIndex ? pageIndex : 0);
        	tab.select(pageIndex);

        	if (model.Header) {
        		header.clear();
        		header.update(model.Header);
        	} else
        		header.getElement().hide();

        	page.clear();
        	if (model.Pages[tab.getCurrentIndex()])
        		page.update(model.Pages[tab.getCurrentIndex()]);

        	for (var index in model.Expressions)
        		if (model.Expressions[index].Error) {
        			$("div.ActionBar a.Button.ExpressionsList").addClass("Invalid");
        			break;
        		}

        	prototypes.propertyPanel.select(null);
        	prototypes.propertyPanel.windowScrolled();

        	if (updateHandler) updateHandler(model);
        },

        addPage = function () {
        	if (!model.Pages) model.Pages = [];
        	model.Pages.push({ TypeName: "Page", Description: "(new page)" })
        },

        setup = function () {
        	tab = new matematici.Tab($("div.TabContainer", element)).addHandler(function (index) {
        		pageIndex = index;
        		if (index >= model.Pages.length) {
        			matematici.canClose = false;
        			addPage();
        		}
        		update();

        		prototype.select(null, null);
        		prototype.select(null, page);
        	});

        	title = $("h2", element).on("click", function () {
        		prototype.select(null, prototype);
        	});

        	prototypes.propertyPanel.resize();

        	$.ajax({
        		url: matematici.urls.prototypes.open,
        		type: "post",
        		data: { ClientId: matematici.client.getId(), Code: code, Version: version, IsNew: isNew }
        	}).done(function (result) {
        		if (result.Error)
        			matematici.dialog.error("Error", result.Error);
        		else {
        			model = result;
        			update();
        		}
        	}).fail(function (jqXHR, textStatus, errorThrown) {
        		console.log("Server error", jqXHR, textStatus, errorThrown);
        		alert("Server error: " + errorThrown);
        	});
        },

		sign = function (action, grid, signature, username, rowVersion) {
			var
				options = {
					ajax: {
						url: matematici.urls.prototypes.signature,
						type: "POST",
						data: {
							Prototype: model.Code,
							Version: model.Version,
							Action: action,
							SignatureId: signature.Id,
							Reason: signature.SignatureReason,
							Code: username,
							RowVersion: rowVersion
						}
					},
					data: {
						Prototype: model.Code,
						Version: model.Version,
						Action: action,
						SignatureId: signature.Id,
						Code: username,
						RowVersion: rowVersion,

						completeHandler: function (data) {
							var signature = this;
							$.ajax({
								url: matematici.urls.prototypes.sign,
								type: "post",
								data: data
							}).done(function (result) {
								if (result.error) {
									if (result.concurrency) {
										signature.setConcurrencyException(true);
										grid.close();
									}

									signature.showError(result.error);
								} else {
									matematici.popup.close();
									grid.close();
								}

								var changed = false;
								if (result.state) {
									model.State = result.state;
									changed = true;
								}
								if (result.validUntil !== undefined) {
									model.ValidUntil = result.validUntil;
									changed = true;
								}
								if (result.rowVersion !== undefined) {
									model.RowVersion = result.rowVersion;
									changed = true;
								}
								if (changed) {
									update();
								}
							}).fail(function (jqXHR, textStatus, errorThrown) {
								console.log("Server error", jqXHR, textStatus, errorThrown);
								alert("Server error: " + errorThrown);
							});
						}
					}
				};

			matematici.popup.show(options);
		};

    prototype.getElement = function () {
        return element;
    };

    prototype.select = function (event, item) {
        if (event) {
            event.stopPropagation();
            event.preventDefault();
        }

        if (itemHandler && !itemHandler.call(item.getElement()))
            return;

        if (selectedItem) selectedItem.getElement().removeClass("Selected");
        if (selectedItem && selectedItem.isPage && selectedItem.isPage()) $("a.Selected", tab.getElement()).removeClass("Red");
        selectedItem = selectedItem == item ? null : item;
        prototypes.propertyPanel.select(selectedItem);
        if (selectedItem) selectedItem.getElement().addClass("Selected");
        if (selectedItem && selectedItem.isPage && selectedItem.isPage()) $("a.Selected", tab.getElement()).addClass("Red");
    };

    prototype.getModel = function () {
        return model;
    };

    prototype.getChildren = function () {
        return model.Pages;
    };

    prototype.setChildren = function (value) {
        model.Pages = value;
    };

    prototype.setItemHandler = function (value) {
        itemHandler = value;
    };

    prototype.getItemHandler = function () {
        return itemHandler;
    };

    prototype.getTab = function () {
        return tab;
    };

    prototype.preview = function () {
        var handler = function () {
            $("<a href='" + matematici.urls.instances.preview + "?Prototype=" + model.Code + "&Version=" + model.Version + "' target='_blank'></a>")[0].click();
        };

        if (matematici.canClose)
            handler();
        else
            matematici.dialog.show("warning", "There are unsaved changes.", [
                { text: "save", icon: "check", handler: function () { prototype.save(handler); } },
                { text: "cancel", icon: "remove", cancelButton: true }
            ]);
    };

    prototype.startSimulation = function () {
        var handler = function () {
            prototype.setState(prototypes.Prototype.states.simulation);
        };

        if (matematici.canClose)
            handler();
        else
            matematici.dialog.show("warning", "There are unsaved changes.", [
                { text: "save", icon: "check", handler: function() { prototype.save(handler); } },
                { text: "cancel", icon: "remove", cancelButton: true }
            ]);
    };

    prototype.save = function (callback) {
        var templateParameters = isNew ? "&TemplateCode=" + code + "&TemplateVersion=" + version : "";
        prototype.select();
        $.ajax({
    		url: matematici.urls.prototypes.save + "?IsNew=" + isNew + templateParameters,
    		type: "post",
    		contentType: "application/json",
    		data: JSON.stringify(model)
    	}).done(function (result) {
    		if (result.error) {
    			matematici.dialog.error("Error", result.message);
    		} else if (result.redirect) {
    			matematici.canClose = true;
    			matematici.navigator.open(result.redirect, false, false);
    		} else {
				var check = $("<i />").addClass("fi flaticon-check").appendTo($("div.ActionBar a.Save")).hide().fadeIn(1000);
    			setTimeout(function () {
    				check.fadeOut(2000, function () { check.remove() });
    			}, 3000);

    			matematici.canClose = true;
    			model = result;
    			update();
    			if (callback) callback.call();
    		}
    	}).fail(function (jqXHR, textStatus, errorThrown) {
    		console.log("Server error", jqXHR, textStatus, errorThrown);
    		alert("Server error: " + errorThrown);
    	});
    };

    prototype.setState = function (state, callback, skipConfirmation) {
        prototype.select();
        var handler = function () {
            model.State = state;
            $.ajax({
                url: matematici.urls.prototypes.setState,
                type: "post",
                contentType: "application/json",
                data: JSON.stringify(model)
            }).done(function (result) {
                if (result.error) {
                    matematici.dialog.error("Error", result.error);
                } else {
                    model = result;
                    update();
                    if (callback) callback.call();
                }
            }).fail(function (jqXHR, textStatus, errorThrown) {
                console.log("Server error", jqXHR, textStatus, errorThrown);
                alert("Server error: " + errorThrown);
            });
        };

        if (skipConfirmation) {
        	handler.call();
        } else {
        	matematici.dialog.show("warning", "Please confirm the change of state.", [
				{
					text: "confirm", icon: "check", handler: function () {
						if (matematici.canClose)
							handler.call();
						else
							prototype.save(handler);
					}
				},
				{ text: "cancel", icon: "remove", cancelButton: true }
        	]);
        }
    };

    prototype.delete = function () {
        matematici.dialog.show("warning", "This draft will be permanently deleted.", [
            {
                text: "confirm", icon: "check", handler: function () {
                    $.ajax({
                        url: matematici.urls.prototypes.delete,
                        type: "post",
                        data: { Prototype: model.Code, Version: model.Version }
                    }).done(function (result) {
                        if (result.error) {
                            matematici.dialog.error("Error", result.error);
                        } else {
                            matematici.canClose = true;
                            matematici.navigator.open(result.redirect, false, false);
                        }
                    }).fail(function (jqXHR, textStatus, errorThrown) {
                        console.log("Server error", jqXHR, textStatus, errorThrown);
                        alert("Server error: " + errorThrown);
                    });
                }
            },
            { text: "cancel", icon: "remove", cancelButton: true }
        ]);
    };

    prototype.loadSignatureBook = function (action) {
    	return $.ajax(matematici.urls.signatureBook.load, {
    		type: "POST",
			data: { Prototype: model.Code, Version: model.Version, Action: action }
    	}).done(function (result) {
    		if (result.error) {
    			matematici.dialog.error("Error", result.error);
    		} else {
    			new matematici.CheckListGrid(result.rows, function (instance, row) {
					if (!row.Flag) prototype.validateSignature(action, instance, row, result.rowVersion);
    				return true;
    			}, function (row, column, value) {
    				return value == "Id" || column == "Id";
    			});
    		}
    	});
    };

    prototype.validateSignature = function (action, grid, signature, rowVersion) {
    	return $.ajax(matematici.urls.signatureBook.validate, {
    		type: "POST",
    		data: { Prototype: model.Code, Version: model.Version, Action: action, SignatureId: signature.Id }
    	}).done(function (result) {
    		if (result.error) {
    			matematici.dialog.error("Error", result.error);
    		} else {
    			sign(action, grid, signature, matematici.userPanel.getUsers()[0], rowVersion);
    		}
    	});
    };

    prototype.quit = function () {
        matematici.navigator.open(matematici.urls.prototypes.list, false, true)
    };

    prototype.setOnUpdateHandler = function (handler) {
    	updateHandler = handler;
    };

    prototype.completeModel = function () {
        completeModel();
    };

    prototype.showHistory = function () {
        $.ajax({
            url: matematici.urls.prototypes.history,
            type: "post",
            data: { Prototype: code, Version: version  }
        }).done(function (result) {
            if (result.Error)
                matematici.dialog.error("Error", result.Error);
            else {
                var
                    checkListGrid = new matematici.CheckListGrid(result.Data),
                    parameters = checkListGrid.getParameters(),
                    attributes = { type: "text", readonly: "readonly" };
                
                parameters.append($("<label />").text("Company").append($("<input />").attr(attributes).val(result.Company)));
                parameters.append($("<label />").text("Department").append($("<input />").attr(attributes).val(result.Department)));
                parameters.append($("<label />").text("Prototype Type").append($("<input />").attr(attributes).val(result.PrototypeType)));
                parameters.append($("<label />").text("Prototype").append($("<input />").attr(attributes).val(result.Prototype)));
                parameters.append($("<label />").text("Version").append($("<input />").attr(attributes).val(result.Version)));
            }
        }).fail(function (jqXHR, textStatus, errorThrown) {
            console.log("Server error", jqXHR, textStatus, errorThrown);
            alert("Server error: " + errorThrown);
        });
    };

    prototype.isNew = function () {
        return isNew;
    };

    prototype.update = update;
    prototypes.prototype = prototype;

    setup();
}

prototypes.Prototype.states = {
	active: "Active",
	approved: "Approved",
	draft: "Draft",
	approval: "In approval",
	simulation: "In simulation",
	obsolete: "Obsolete"
}