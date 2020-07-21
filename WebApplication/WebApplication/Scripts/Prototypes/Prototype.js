prototypes.Prototype = function (selector, options) {
    var
        prototype = this,
        element = $(selector),
        page,
        header,
        pageIndex,
        title,
        tab,
        pageLock,
        model,
        prototypeKey,
        concurrencyInformation,
        currentItem,
        itemHandler,
        commands,
        dataSources,
        imagesToDelete = [],

        toBase64 = function (arrayBuffer, callback) {
            var blob = new Blob([arrayBuffer])

            var reader = new FileReader();
            reader.onload = function (event) {
                var
                    base64 = event.target.result,
                    binaryIndex = base64.indexOf("base64,");
                callback(base64.substring(binaryIndex + 7));
            };

            reader.readAsDataURL(blob);
        },

        getNewItemsId = function () {
            if (!model.itemIDSequence) model.itemIDSequence = 1;
            return model.itemIDSequence++;
        },

        setNewItemsId = function (value) {
            model.itemIDSequence = Math.max(model.itemIDSequence, value + 1);
        },

        completeModel = function () {
            var analyzeElement = function (node) {
                switch (node.typeName) {
                    case "Group":
                        for (var itemIndex in node.items || [])
                            analyzeElement(node.items[itemIndex]);
                        break;
                    case "Signature":
                        if (node.code)
                            model.signatures[node.code] = node;
                        break;
                }
            };

            model.typeName = "Prototype";
            model.signatures = {};

            analyzeElement(model.header);
            for (var pageIndex in model.pages || []) {
                var page = model.pages[pageIndex];
                for (var groupIndex in page.groups || [])
                    analyzeElement(page.groups[groupIndex])
            }
        },

        setupActionBar = function () {
            var
                buttonGroup = $("div.ActionBar a.ButtonGroup", element),
                previewButton = $("a.Preview", buttonGroup).addClass("Hidden"),
                startSimulationButton = $("a.StartSimulation", buttonGroup).addClass("Hidden"),
                cancelSimulationButton = $("a.CancelSimulation", buttonGroup).addClass("Hidden"),
                deleteButton = $("a.Delete", buttonGroup).addClass("Hidden"),
                submitForApprovalButton = $("a.SubmitForApproval", buttonGroup).addClass("Hidden"),
                approveButton = $("a.Approve", buttonGroup).addClass("Hidden"),
                submitForObsolescence = $("a.SubmitForObsolescence", buttonGroup).addClass("Hidden"),
                obsoleteButton = $("a.Obsolete", buttonGroup).addClass("Hidden"),
                saveButton = $("a.Save", buttonGroup).addClass("Hidden"),
                historyButton = $("a.History", buttonGroup).toggle(model.version > 0),
                importButton = $("a.Import", buttonGroup).addClass("Hidden"),
                exportButton = $("a.Export", buttonGroup),
                downloadButton = $("a.Download", buttonGroup);

            switch (model.state) {
                case prototypes.Prototype.states.active:
                case prototypes.Prototype.states.approved:
                    if (model.proposedState)
                        obsoleteButton.removeClass("Hidden");
                    else if (
                        matematici.tokenManager.checkRole("Prototypes#Edit#" + model.department + "##")
                        || matematici.tokenManager.checkRole("Prototypes#Edit#" + model.department + "#" + model.prototypeType)
                    )
                        submitForObsolescence.removeClass("Hidden");
                    break;
                case prototypes.Prototype.states.draft:
                    if (model.version > 0) {
                        previewButton.removeClass("Hidden");
                        startSimulationButton.removeClass("Hidden");
                        deleteButton.removeClass("Hidden");
                    }
                    importButton.removeClass("Hidden");
                    saveButton.removeClass("Hidden");
                    break;
                case prototypes.Prototype.states.simulation:
                    previewButton.removeClass("Hidden");
                    submitForApprovalButton.removeClass("Hidden");
                    cancelSimulationButton.removeClass("Hidden");
                    break;
                case prototypes.Prototype.states.approval:
                    approveButton.removeClass("Hidden");
                    break;
                case prototypes.Prototype.states.draftReadOnly:
                    if (
                        matematici.tokenManager.checkRole("Prototypes#Preview#" + model.department + "##")
                        || matematici.tokenManager.checkRole("Prototypes#Preview#" + model.department + "#" + model.prototypeType)
                    )
                        previewButton.removeClass("Hidden");
                    break;
                case prototypes.Prototype.states.simulationReadOnly:
                    if (
                        matematici.tokenManager.checkRole("Prototypes#Preview#" + model.department + "##")
                        || matematici.tokenManager.checkRole("Prototypes#Preview#" + model.department + "#" + model.prototypeType)
                    ) {
                        previewButton.removeClass("Hidden");
                        submitForApprovalButton.removeClass("Hidden");
                        cancelSimulationButton.removeClass("Hidden");
                    }
                    break;
                case prototypes.Prototype.states.obsolete:
                    break;
                default:
                    alert("Invalid prototype state: " + (model.state || "(null)"));
                    console.log(model);
                    break;
            }

            if ($("a:not(.Hidden)", buttonGroup).size() == 0)
                buttonGroup.hide();
        },

        checkProfile = function () {
            if (
                prototypeKey.id >= 0
                && !matematici.tokenManager.checkRole("Prototypes#Edit#" + model.department + "##")
                && !matematici.tokenManager.checkRole("Prototypes#Edit#" + model.department + "#" + model.prototypeType)
            ) {
                if (model.state == prototypes.Prototype.states.draft)
                    model.state = prototypes.Prototype.states.draftReadOnly;
                if (model.state == prototypes.Prototype.states.simulation)
                    model.state = prototypes.Prototype.states.simulationReadOnly;
            }
        },

        updateDependencies = function (model, newModel) {
            model.dependingRelations = newModel.dependingRelations;
            model.dependentRelations = newModel.dependentRelations;

            switch (model.typeName) {
                case "Prototype":
                    model.itemIDSequence = newModel.itemIDSequence;
                    updateDependencies(model.header, newModel.header);
                    model.exception = newModel.exception;
                    for (var index in model.pages)
                        updateDependencies(model.pages[index], newModel.pages[index]);

                    model.externalSourceDependencyNodes = newModel.externalSourceDependencyNodes;

                    var invalid = false;
                    for (var index in model.expressions) {
                        var
                            expression = model.expressions[index],
                            newExpression = newModel.expressions[index];
                        expression.dataType = newExpression.dataType;
                        expression.error = newExpression.error || (newExpression.dataType && newExpression.dataType.typeName ? null : "the expression could not be evaluated");
                        if (expression.error) invalid = true;
                        updateDependencies(expression, newExpression);
                    }
                    $("div.ActionBar a.ExpressionsList").toggleClass("Invalid", invalid);

                    break;
                case "Page":
                    for (var index in model.groups)
                        updateDependencies(model.groups[index], newModel.groups[index]);
                    break;
                case "Group":
                    for (var index in model.items)
                        updateDependencies(model.items[index], newModel.items[index]);
                    break;
            }
        },

        update = function () {
            var
                currentItemId = currentItem && (currentItem.isPage && currentItem.isPage() && $("a.Current", element).attr("id") || $("div.Current", element).attr("id")); //saves current item

            completeModel();
            title.children("span").text(model.description || "(Prototype name)");

            commands = $("<div />").addClass("Commands")
                .append($("<a />").attr("title", "properties").attr("data-command", "properties").addClass("Button MiniIcon Properties").append($("<i />").addClass("fal fa-cogs")))
                .append($("<a />").attr("title", "dependencies").attr("data-command", "dependencies").addClass("Button MiniIcon Dependencies").append($("<i />").addClass("fal fa-sitemap")))
                .append($("<a />").attr("title", "copy").attr("data-command", "copy").addClass("Button MiniIcon Copy").append($("<i />").addClass("fal fa-copy")));
            if (model.state == prototypes.Prototype.states.draft)
                commands
                    .append($("<a />").attr("title", "paste").attr("data-command", "paste").addClass("Button MiniIcon Paste").append($("<i />").addClass("fal fa-paste")))
                    .append($("<a />").attr("title", "add").attr("data-command", "add").addClass("Button MiniIcon Add").append($("<i />").addClass("fal fa-plus")))
                    .append($("<a />").attr("title", "move up").attr("data-command", "up").addClass("Button MiniIcon Up").append($("<i />").addClass("fal fa-caret-up")))
                    .append($("<a />").attr("title", "move down").attr("data-command", "down").addClass("Button MiniIcon Down").append($("<i />").addClass("fal fa-caret-down")))
                    .append($("<a />").attr("title", "remove").attr("data-command", "remove").addClass("Button MiniIcon Remove").append($("<i />").addClass("fal fa-trash-alt")));


            $("a.Paste", commands).css("display", localStorage.prototypeClipboard ? "inline-block" : "none");

            tab.clear();
            for (var index in model.pages)
                tab.add(model.pages[index].description, "Page" + index);

            if (model.state == prototypes.Prototype.states.draft) tab.add($("<span />").prepend($("<i >").addClass("Add fal fa-plus")), null, "Add");
            page.setIndex(pageIndex = pageIndex >= model.pages.length ? model.pages.length - 1 : pageIndex ? pageIndex : 0);
            tab.select(pageIndex);

            if (model.header) {
                header.clear();
                header.update(model.header);
            } else
                header.getElement().hide();

            page.clear();
            if (model.pages[tab.getCurrentItem().index])
                page.update(model.pages[tab.getCurrentItem().index]);

            for (var index in model.expressions)
                if (model.expressions[index].Error) {
                    $("div.ActionBar a.Button.ExpressionsList").addClass("Invalid");
                    break;
                }

            if (currentItemId) { selectByElementId(currentItemId) };

            setupActionBar();
        },

        selectById = function (className, id) {
            if (currentItem) select(null, null);
            currentItem = null;
            var result = {};
            if (className != "Page") findItem(model, id, null, result);
            if (result.item && result.page) $("#Page" + result.page, element).click();
            selectByElementId(className + id);
        },

        selectByElementId = function (elementId) {
            $("#" + elementId, element).removeClass("Current").removeClass("Selected").click(); //class "Selected" is for the tab when current item is a page
        },

        select = function (event, item) {
            if (event) {
                event.stopPropagation();
                event.preventDefault();
            }

            if (itemHandler && !itemHandler.call(item.getElement()))
                return;

            if (currentItem) {
                currentItem.getElement().removeClass("Current");
                commands.remove();
                if (currentItem.isPage && currentItem.isPage())
                    $("div.Commands", $("a.Selected", tab.getElement()).removeClass("Current")).remove();
            }
            currentItem = currentItem == item ? null : item;
            if (currentItem) {
                currentItem.getElement().addClass("Current");
                switch (currentItem.getModel().typeName) {
                    case "Prototype":
                        commands.insertBefore(title)
                        break;
                    case "Page":
                        commands.insertAfter($("a.Selected", tab.getElement()).addClass("Current"));
                        break;
                    default:
                        currentItem.getElement().append(commands);
                        break;
                }

                commands.on("click", "a", command);
            }
        },

        command = function (e) {
            e.preventDefault();
            e.stopPropagation();

            var
                parent = currentItem.getParent && currentItem.getParent(),
                siblings = !!parent && parent.getChildren(),
                itemIndex = currentItem.getIndex && currentItem.getIndex();

            switch ($(this).data("command")) {
                case "up":
                    var items = [];
                    for (var index in siblings)
                        items.push(siblings[index == itemIndex - 1 ? itemIndex : index == itemIndex ? index - 1 : index]);
                    parent.setChildren(items);
                    update();
                    matematici.canClose = false;
                    break;
                case "down":
                    var items = [];
                    for (var index in siblings)
                        items.push(siblings[index == itemIndex ? itemIndex + 1 : index == itemIndex + 1 ? itemIndex : index]);
                    parent.setChildren(items);
                    update();
                    matematici.canClose = false;
                    break;
                case "add":
                    currentItem.add();
                    break;
                case "remove":
                    matematici.dialog.show("Warning", "The current item will be removed", [
                        {
                            text: "confirm", icon: "check", handler: function () {
                                parent.getChildren().splice(itemIndex, 1);
                                update();
                                matematici.canClose = false;
                            }
                        },
                        { text: "cancel", icon: "times", cancelButton: true }
                    ]);
                    break;
                case "paste":
                    paste();
                    break;
                case "copy":
                    localStorage.prototypeClipboard = JSON.stringify(currentItem.getModel());
                    $("a.Paste", commands).css("display", "inline-block");
                    matematici.blip.ok();
                    break;
                case "properties":
                    prototypes.propertyPanel.select(currentItem);
                    break;
                case "dependencies":
                    var
                        handler = function (error) {
                            if (error) return;
                            var
                                model = currentItem.getModel(),
                                title = model.typeName + (model.index != null && " - Index: " + (parseInt(model.index) + 1) || model.id != null && " - ID: " + model.id || "") + (model.code && " - Code: " + model.code || "")
                            matematici.dependenciesDialog.show(title, model.dependingRelations, model.dependentRelations);
                        };

                    prototype.verify(handler);
                    break;
                default:
                    alert("invalid command");
                    console.log(this);
                    break;
            }
        },

        traverseForPaste = function (element) {
            element.code = null;
            element.id = getNewItemsId();
            if (element.typeName == "Page")
                for (var index in element.groups)
                    traverseForPaste(element.groups[index]);
            else if (element.typeName == "Group")
                for (var index in element.items)
                    traverseForPaste(element.items[index]);
            else if (element.typeName == "Image")
                element.cloned = true;
        },

        findItem = function (element, itemId, pageIndex, result) {
            if (element.id == itemId) {
                result.page = pageIndex;
                result.item = element;
                return;
            }

            if (element.typeName == "Group")
                for (var index in element.items) {
                    findItem(element.items[index], itemId, pageIndex, result);
                    if (result.item) return;
                }
            else if (element.typeName == "Prototype")
                for (var pageIndex in element.pages) {
                    var page = element.pages[pageIndex];
                    for (var groupIndex in page.groups) {
                        findItem(page.groups[groupIndex], itemId, pageIndex, result);
                        if (result.item) return;
                    }
                }
        },

        paste = function () {
            if (!localStorage.prototypeClipboard) {
                matematici.dialog.error("Error", "Nothing to paste.");
                return;
            }

            var
                itemModel = currentItem.getModel(),
                clipboardModel = JSON.parse(localStorage.prototypeClipboard);

            traverseForPaste(clipboardModel);
            switch (clipboardModel.typeName) {
                case "Page":
                    if (itemModel.typeName != "Page" && itemModel.typeName != "Prototype") {
                        matematici.dialog.error("Error", "Select a page or the prototype to paste another page.");
                        return;
                    }
                    model.pages.push(clipboardModel);
                    select(null, null);
                    update();
                    break;
                case "Group":
                    if (itemModel.typeName == "Page")
                        currentItem.addGroup(clipboardModel);
                    else
                        currentItem.addElement(null, clipboardModel);
                    break;
                default:
                    if (itemModel.typeName == "Page") {
                        matematici.dialog.error("Error", "A page cannot directly contain an item. Paste the item into a group.");
                        return;
                    }
                    if (itemModel.typeName == "Prototype") {
                        matematici.dialog.error("Error", "Only a page can be pasted directly into a prototype.");
                        return;
                    }
                    currentItem.addElement(null, clipboardModel);
                    break;
            }

            matematici.canClose = false;
        },

        addPage = function () {
            if (!model.pages) model.pages = [];
            var pageIndex = 0;
            for (var index in model.pages)
                if (pageIndex < model.pages[index].index)
                    pageIndex = model.pages[index].index;

            model.pages.push({ typeName: "Page", description: "(page)", index: pageIndex + 1 });
        },

        sign = function (action, grid, rows, row) {
            if (row.approved) {
                matematici.dialog.error("error", "This signature has already been issued");
                return true;
            }

            var
                allowed = false,
                tags = row.tags.split(", ");

            for (var role in tags)
                if (
                    matematici.tokenManager.checkRole("Prototypes#Approve#" + model.department + "##" + tags[role])
                    || matematici.tokenManager.checkRole("Prototypes#Approve#" + model.department + "#" + model.prototypeType + "#" + tags[role])
                ) {
                    allowed = true;
                    break;
                }

            if (!allowed) {
                matematici.dialog.error("error", "Permission denied");
                return true;
            }

            for (var rowIndex in rows) {
                if (rows[rowIndex].step < row.step && !rows[rowIndex].approved) {
                    allowed = false;
                    break;
                }

                if (rows[rowIndex].id == row.id)
                    break;
            }
            if (!allowed) {
                matematici.dialog.error("error", "The previous step is not completed");
                return true;
            }

            for (var rowIndex in rows) {
                if (rows[rowIndex].step == row.step && rows[rowIndex].user == matematici.tokenManager.getUserCode()) {
                    allowed = false;
                    break;
                }
            }
            if (!allowed) {
                matematici.dialog.error("error", "You have already signed for this step");
                return true;
            }

            workFlowSignature = new matematici.WorkFlowSignature({
                title: action,
                reason: row.reason,
                url: element.data("signature"),
                prototype: prototypeKey.id,
                version: prototypeKey.version,
                applicationStep: row.id,
                model: model,
                rows: rows,
                concurrencyInformation: concurrencyInformation,

                done: function (result) {
                    grid.close();
                    matematici.popup.close();
                    model.state = result.state;
                    model.validFrom = result.validFrom;
                    model.validUntil = result.validUntil;
                    if (result.completed)
                        model.proposedState = model.proposedDate = null;
                    concurrencyInformation = {
                        modifiedBy: result.modifiedBy,
                        modifiedOn: result.modifiedOn
                    };
                    checkProfile();
                    update();
                    setupActionBar();
                    if (!result.completed)
                        prototype.loadSignatureBook(action);
                    if (result.approved)
                        matematici.blip.ok();
                    else
                        matematici.blip.ko();
                }
            });
            return true;
        },

        setup = function () {
            page = new prototypes.Group($("div.Page", element), "Page", prototype);

            header = new prototypes.Group($("div#Header", element), "Group", prototype);

            tab = new matematici.Tab($("div.TabContainer", element), {}).addHandler(function (item) {
                var
                    pageChange = pageIndex != item.index,
                    newPage = item.index == model.pages.length;

                if (pageChange) {
                    pageIndex = item.index;
                    if (newPage) {
                        matematici.canClose = false;
                        addPage();
                    }
                    select(null, null);
                    update();
                }

                select(null, page);
            });

            title = $("h2", element).on("click", function () {
                select(null, prototype);
            });

            //opening
            $.ajax({
                url: element.data("action") + options.action,
                type: "post",
                data: { id: options.id, version: options.version }
            }).done(function (result) {
                if (result.error)
                    matematici.dialog.error("Error", result.error);
                else {
                    model = result.checkList;

                    if (model.exception) {
                        console.log(model.exception);
                        matematici.dialog.error("Error", model.exception && model.exception.Message || "unexpected error");
                    }

                    prototypeKey = {
                        id: result.id,
                        version: result.version
                    };
                    if (result.version > 0) model.version = result.version;
                    concurrencyInformation = {
                        modifiedBy: result.modifiedBy,
                        modifiedOn: result.modifiedOn
                    };

                    model.state = result.state;
                    model.proposedState = result.proposedState;
                    model.proposedDate = result.proposedDate;
                    model.validFrom = result.validFrom;
                    model.validUntil = result.validUntil;
                    model.instanceConfirmationWorkFlow = result.instanceConfirmationWorkFlow;
                    model.name = result.name;
                    model.description = result.description;
                    model.department = result.department;
                    model.prototypeType = result.prototypeType;
                    model.departmentName = result.departmentName;
                    model.prototypeTypeName = result.prototypeTypeName;
                    checkProfile();
                    update();
                    element.removeClass("Loading");
                }
            });
        };

    prototype.getElement = function () {
        return element;
    };

    prototype.getTab = function () {
        return tab;
    };

    prototype.getModel = function () {
        return model;
    };

    prototype.getPrototypeKey = function () {
        return prototypeKey;
    };

    prototype.setItemHandler = function (value) {
        itemHandler = value;
    };

    prototype.getItemHandler = function () {
        return itemHandler;
    };

    prototype.getChildren = function () {
        return model.pages;
    };

    prototype.setChildren = function (value) {
        model.pages = value;
    };

    prototype.verify = function (callback) {
        if (matematici.canClose)
            callback();
        else {
            var
                array = pako.gzip(JSON.stringify(model)),
                handler = function (base64CheckList) {
                    var
                        //base64CheckList = btoa(String.fromCharCode.apply(null, new Uint8Array(array))),
                        data = JSON.stringify({
                            base64CheckList: base64CheckList
                        });

                    $.ajax({
                        url: element.data("action") + "verify",
                        type: "post",
                        contentType: "application/json",
                        data: data
                    }).done(function (result) {
                        if (result.checkList.exception) matematici.dialog.error("Error", result.checkList.exception.Message);
                        updateDependencies(model, result.checkList);
                        callback(result.checkList.exception);
                    });
                }

            toBase64(array, handler);
        }
    };

    prototype.save = function (callback) {
        var currentItemId = currentItem && (currentItem.isPage && currentItem.isPage() && $("a.Current", element).attr("id") || $("div.Current", element).attr("id")); //saves current item

        select();

        var
            array = pako.gzip(JSON.stringify(model)),
            handler = function (base64CheckList) {
                var
                    //base64CheckList = btoa(String.fromCharCode.apply(null, new Uint8Array(array))),
                    data = JSON.stringify({
                        id: prototypeKey.id,
                        version: prototypeKey.version,
                        modifiedOn: moment.parseZone(concurrencyInformation.modifiedOn),
                        modifiedby: concurrencyInformation.modifiedBy,
                        validFrom: model.validFrom,
                        validUntil: model.validUntil,
                        instanceConfirmationWorkFlow: model.instanceConfirmationWorkFlow,
                        name: model.name,
                        description: model.description,
                        department: model.department,
                        prototypeType: model.prototypeType,
                        imagesToDelete: imagesToDelete,
                        base64CheckList: base64CheckList
                    });

                $.ajax({
                    url: element.data("action") + "Save",
                    type: "post",
                    contentType: "application/json",
                    data: data //pako.gzip(data, { to: 'string' })
                }).done(function (result) {
                    if (result.checkList.exception) {
                        matematici.dialog.error("Error", result.checkList.exception.Message);
                        updateDependencies(model, result.checkList);
                    } else {
                        matematici.canClose = true;
                        prototypeKey.id = result.id;
                        model.expressions = result.checkList.expressions;
                        model.externalSourceDependencyNodes = result.checkList.externalSourceDependencyNodes;
                        model.header = result.checkList.header;
                        model.pages = result.checkList.pages;
                        model.dependingRelations = result.checkList.dependingRelations;
                        model.dependentRelations = result.checkList.dependentRelations;
                        model.itemIDSequence = result.checkList.itemIDSequence;
                        imagesToDelete = [];
                        if (model.version == null) {
                            matematici.navigator.setUrl(options.detailUrl + "/" + result.id + "/" + result.version);
                            model.version = prototypeKey.version = result.version;
                        }
                        concurrencyInformation = {
                            modifiedBy: result.modifiedBy,
                            modifiedOn: result.modifiedOn
                        };
                        update();

                        if (currentItemId) {
                            //restores current item
                            currentItem = null;
                            $("#" + currentItemId, element).removeClass("Current").removeClass("Selected").click(); //Selected is for the tab when current item is a page
                        }

                        $("div.ActionBar a.ExpressionsList").removeClass("Invalid");

                        if (callback)
                            callback.call();
                        else
                            matematici.blip.ok();
                    }
                });
            };

        toBase64(array, handler);
    };

    prototype.delete = function () {
        select();
        matematici.dialog.show("warning", "This draft will be permanently deleted.", [
            {
                text: "confirm", icon: "check", handler: function () {
                    var done = function () {
                        matematici.canClose = true;
                        matematici.navigator.open(options.listUrl, false, false);
                    };

                    if (prototypeKey.version < 0)
                        done()
                    else
                        $.ajax({
                            url: element.data("action") + "Delete",
                            type: "post",
                            data: {
                                id: prototypeKey.id,
                                version: prototypeKey.version
                            }
                        }).done(function (result) {
                            done();
                        });
                }
            },
            { text: "cancel", icon: "times", cancelButton: true }
        ]);
    };

    prototype.setState = function (state, proposed) {
        select();
        var handler = function () {
            model.State = state;
            $.ajax({
                url: element.data("action") + "SetState",
                type: "post",
                data: {
                    id: prototypeKey.id,
                    version: prototypeKey.version,
                    state: state,
                    stateProposed: proposed
                }
            }).done(function (result) {
                model.state = result.state;
                model.proposedState = result.proposedState;
                model.proposedDate = result.proposedDate;
                checkProfile();
                update();
                matematici.blip.ok();
            });
        };

        matematici.dialog.show("warning", "Please confirm the change of state.", [
            {
                text: "confirm", icon: "check", handler: function () {
                    if (matematici.canClose)
                        handler();
                    else
                        prototype.save(handler);
                }
            },
            { text: "cancel", icon: "times", cancelButton: true }
        ]);
    };

    prototype.loadSignatureBook = function (action) {
        return $.ajax({
            url: element.data("api") + "/WorkFlows/Applications/GetSteps",
            type: "POST",
            data: {
                prototype: prototypeKey.id,
                version: prototypeKey.version,
            }
        }).done(function (result) {
            var columns = [
                { name: "step", label: "Step", align: "center" },
                { name: "tags", label: "Level" },
                { name: "reason", label: "Reason" },
                { name: "date", label: "Date", type: "DateTimeOffset" },
                { name: "user", label: "User" },
                { name: "approved", label: "Approved", align: "center" },
            ];
            new matematici.CheckListGrid(result.steps,
                function (grid, row) {
                    return sign(result.action, grid, result.steps, row);
                },
                null,
                columns);
        });
    };

    prototype.preview = function () {
        if (
            !matematici.tokenManager.checkRole("Prototypes#Preview#" + model.department + "##")
            && !matematici.tokenManager.checkRole("Prototypes#Preview#" + model.department + "#" + model.prototypeType + "#")
        ) {
            matematici.dialog.error("error", "Permission denied");
            return;
        }

        var handler = function () {
            matematici.navigator.open(options.previewUrl + "/" + prototypeKey.id + "/" + prototypeKey.version);
        };

        if (matematici.canClose)
            handler();
        else
            matematici.dialog.show("warning", "There are unsaved changes.", [
                { text: "save", icon: "check", handler: function () { prototype.save(handler); } },
                { text: "cancel", icon: "times", cancelButton: true }
            ]);
    };

    prototype.showHistory = function () {
        $.ajax({
            url: element.data("action") + "History",
            type: "post",
            data: {
                id: prototypeKey.id,
                version: prototypeKey.version,
            }
        }).done(function (result) {
            var
                columns = [
                    { label: "State", name: "state" },
                    { label: "Date", name: "date", type: "DateTimeOffset" },
                    { label: "User", name: "user" },
                    { label: "Approved", name: "approved", type: "Boolean", align: "center" },
                    { label: "Proposed state", name: "proposedState" },
                    { label: "Signature step", name: "step", align: "center" },
                    { label: "Signature reason", name: "reason" },
                    { label: "Signature comment", name: "comment" },
                    { label: "Appointed signer", name: "appointedUser" }
                ],
                rowRenderHandler = function (checkListGrid, rowIndex, data) {
                    checkListGrid.addRow(data, rowIndex % 2 == 0 ? "Odd" : "Even");
                    for (var subRowIndex in data.steps) {
                        var
                            subRow = data.steps[subRowIndex],
                            tr = checkListGrid.addRow(subRow, "SubRow " + (subRowIndex % 2 == 0 ? "Odd " : "Even ") + (subRow.approved == true ? "Green" : subRow.approved == false ? "Red" : ""));

                        $("td", tr).eq(0).addClass(rowIndex % 2 == 0 ? "Odd" : "Even");
                    }
                    return true;
                },
                checkListGrid = new matematici.CheckListGrid(result, null, null, columns, rowRenderHandler),
                parameters = checkListGrid.getParameters(),
                attributes = { type: "text", readonly: "readonly" };

            parameters.append($("<label />").append($("<span />").text("Department")).append($("<i />").addClass("Icon fal fa-industry")).append($("<input />").attr(attributes).val(model.departmentName)));
            parameters.append($("<label />").append($("<span />").text("Prototype type")).append($("<i />").addClass("Icon fal fa-folder")).append($("<input />").attr(attributes).val(model.prototypeTypeName)));
            parameters.append($("<label />").append($("<span />").text("Prototype")).append($("<i />").addClass("Icon fal fa-file-invoice")).append($("<input />").attr(attributes).val(model.name)));
            parameters.append($("<label />").append($("<span />").text("Version")).append($("<i />").addClass("Icon fal fa-list-ol")).append($("<input />").attr(attributes).val(prototypeKey.version)));
        });
    };

    prototype.import = function () {
        var handler = function () {
            var oldModel = model;
            try {
                model = JSON.parse($("textarea", matematici.dialog.getElement()).val());
                model.name = oldModel.name;
                model.department = oldModel.department;
                model.prototypeType = oldModel.prototypeType;
                model.version = oldModel.version;
                model.validFrom = oldModel.validFrom;
                model.validUntil = oldModel.validUntil;
                model.state = prototypes.Prototype.states.draft;
                model.itemIDSequence = 0;
                update();
                matematici.canClose = false;
                prototype.verify(function () { matematici.blip.ok(); })
            } catch (error) {
                matematici.dialog.error("Error", error);
                model = oldModel;
                update();
                return true;
            }
        };

        matematici.dialog.show({
            title: "Prototype import",
            content: "Beware, the current prototype properties and items will be replaced by the imported one:",
            buttons: [{
                textarea: true,
                height: 300,
            }, {
                icon: "check",
                text: "ok",
                handler: handler
            }, {
                icon: "times",
                text: "cancel",
                cancelButton: true
            }]
        });
    };

    prototype.export = function () {
        matematici.dialog.show({
            title: "Click to copy",
            buttons: [{
                textarea: true,
                val: JSON.stringify(model),
                readOnly: true,
                height: 300,
                handler: function () {
                    $(this).select();
                    document.execCommand("copy");
                    matematici.blip.ok(matematici.dialog.close());
                }
            }, {
                hidden: true,
                cancelButton: true
            }]
        });

        matematici.dialog.setWidth(600);
    };

    prototype.download = function () {
        var handler = function () {
            $.ajax({
                url: matematici.urls.prototypes.api + "/Download",
                type: "post",
                data: { id: prototypeKey.id, version: prototypeKey.version }
            }).done(function (result) {
                location = matematici.urls.prototypes.api + "/Download?Token=" + result.token;
            });
        };

        if (matematici.canClose)
            handler();
        else
            matematici.dialog.show("warning", "There are unsaved changes.", [
                { text: "save", icon: "check", handler: function () { prototype.save(handler); } },
                { text: "cancel", icon: "times", cancelButton: true }
            ]);
    };

    prototype.update = function () {
        select();
        update();
    };

    prototype.setDataSources = function (value) {
        dataSources = value;
    };

    prototype.getDataSources = function () {
        return dataSources;
    };

    prototype.quit = function () {
        matematici.navigator.open(options.listUrl);
    };

    prototype.deleteImage = function (key) {
        imagesToDelete.push(key);
    };

    prototype.ensureImageWillNotBeDeleted = function (key) {
        Array.removeElement(imagesToDelete, key);
    };

    prototype.getNewItemsId = getNewItemsId;
    prototype.setNewItemsId = setNewItemsId;
    prototypes.prototype = prototype;
    prototype.selectById = selectById;
    prototype.select = select;
    prototype.completeModel = completeModel;

    setup();
}

prototypes.Prototype.states = {
    draft: "Draft",
    draftReadOnly: "Draft ",
    simulation: "In simulation",
    simulationReadOnly: "In simulation ",
    approval: "In approval",
    approved: "Approved",
    active: "Active",
    obsolete: "Obsolete"
}