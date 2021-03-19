instances.Instance = function (element, options) {
    var
        instance = this,
        title = $("h2", element),
        header = new instances.Group($("div#Header", element)[0], "Group"),
        page = new instances.Group($("div.Page", element)[0], "Page"),
        versionState,
        code,
        id,
        state,
        department,
        departmentName,
        prototypeType,
        prototypeTypeName,
        prototype,
        prototypeName,
        version,
        year,
        number,
        instanceDescription,
        workFlow,
        tab,
        pageLock,
        updateHandler,
        externalSources = {},
        expressions = [],
        exceptions = [],
        expressionsButton,
        exceptionsButton,
        lastResult,
        deleted,
        removedIterationsVisible = false,
        Button,

        setupInstanceDescription = function () {
            $(header.getElement()).prepend(instanceDescription = $("<div />").addClass("InstanceDescription"));
        },

        setupTab = function (pages) {
            for (var pageIndex in pages)
                tab.add(pages[pageIndex].description, null, null, "lock")

            pageLock = new instances.PageLock(instance);

            tab.select(0)
            tab.resize();
        },

        setupActionBar = function () {
            var
                editor = hasPermission("Edit"),
                deleter = hasPermission("Delete"),

                actionBar = $("div.ActionBar", element),
                buttonGroup = $("a.ButtonGroup", actionBar),
                confirmButton = $("a.Confirm", buttonGroup).toggleClass("Hidden", state != instances.Instance.states.completed),
                completeButton = $("a.Complete", buttonGroup).toggleClass("Hidden", !editor || state != instances.Instance.states.progress),
                deleteButton = $("a.Delete", buttonGroup).toggleClass("Hidden", !deleter || state != instances.Instance.states.draft && state != instances.Instance.states.progress),
                saveButton = $("a.Save", buttonGroup).toggleClass("Hidden", !editor || state != instances.Instance.states.draft && state != instances.Instance.states.progress),
                historyButton = $("a.History", buttonGroup).toggleClass("Hidden", state == instances.Instance.states.draft),
                showRemovedIterationsButton = $("a.ShowRemovedIterations", buttonGroup).toggleClass("Hidden", !matematici.tokenManager.checkRole("Trail#View#") || removedIterationsVisible),
                hideRemovedIterationsButton = $("a.HideRemovedIterations", buttonGroup).toggleClass("Hidden", !removedIterationsVisible);

            expressionsButton = $("a.ExpressionsButton", actionBar);
            exceptionsButton = $("a.ExceptionsButton", actionBar);

            if ($("a:not(.Hidden)", buttonGroup).size() == 0)
                buttonGroup.hide();
        },

        getFormattedYear = function () {
            return versionState == prototypes.Prototype.states.draft ? "test" : versionState == prototypes.Prototype.states.simulation || year == 0 ? "SE" : year || "?";
        },

        setupBreadcrumbs = function () {
            matematici.breadcrumbs.set([
                { content: departmentName, label: "department" },
                { content: prototypeTypeName, label: "prototype type" },
                { content: prototypeName + "." + version + "." + getFormattedYear() + "." + (number || "?"), label: "code" },
                { content: state, label: "state" }
            ]);
        },

        showExceptions = function () {
            var message = "";

            for (var index in exceptions) 
                message += exceptions[index] + " (" + index + ")<br /><br />";

            matematici.dialog.error("exceptions list", message);
        },

        navigateArray = function (value) {
            var element = $("<div />").addClass("Array");
            for (var index in value) {
                element.append($("<span />").addClass("Index").text(parseInt(index) + 1));
                var
                    item = value[index],
                    div = $("<div />").addClass("Value").appendTo(element),
                    itemValue = item && item.value,
                    itemType = item && item.type && item.type.typeName;
                switch (itemType) {
                    case "DateTime":
                        var formattedDate = matematici.dateFormatter.format(moment.parseZone(itemValue));
                        div.html(formattedDate.date + " " + formattedDate.time).attr("title", formattedDate.offset);
                        break;
                    case "Boolean":
                        div.addClass("Centered").append($("<i />").addClass("fa").addClass("fa-" + (itemValue || itemValue == true ? "check" : "times")));
                        break;
                    case "Array":
                        div.append(navigateArray(itemValue));
                        break;
                    default:
                        div.html(itemValue);
                }
            }
            return element;
        },

        showArray = function (tr) {
            var content = navigateArray(tr.data("array"));
            matematici.dialog.show($("td", tr).eq(0).text(), content);
        },

        update = function (result) {
            lastResult = result;
            var model = result.checkList;
            versionState = result.versionState || versionState;

            if (result.prototypeDescription) title.text(result.prototypeDescription);
            instanceDescription.html(result.checkList.description).toggle(!!result.checkList.description && result.checkList.description != "");
            instanceDescription.attr("title", instanceDescription.text());

            pageLock.update(model);
            if (model.header)
                header.update(model.header);
            else
                $(header.getElement()).hide();

            var pageIndex;
            var pageLabels = tab.getLabels();
            for (pageIndex in model.pages)
                pageLabels.eq(pageIndex).toggle(!model.pages[pageIndex].hidden);

            model.pages[pageIndex]

            pageIndex = tab.getCurrentItem().index;
            page.setPageIndex(pageIndex);
            page.update(model.pages[pageIndex]);

            if (updateHandler) updateHandler(model);
            externalSources = model.externalSources;
            expressions = model.expressions;
            exceptions = model.exceptions;
            exceptionsButton.toggleClass("Hidden", Object.entries(exceptions).length === 0);

            expressionsButton.toggle(!!model.expressions);
        },

        load = function () {
            var
                newInstance = options.code == "",
                parameters = [];

            if (newInstance) {
                //set the items default
                var tokens = window.location.search.substring(1).split('&');

                for (var index in tokens) {
                    var
                        parameter = tokens[index].split('='),
                        key = parameter.length === 2 && parameter[0] && parameter[0].length > 5 && parameter[0].indexOf("item.") == 0 ? parameter[0].substring(5) : null;

                    if (key != null && key.indexOf("'") == -1)
                        parameters.push({ key: key, value: decodeURIComponent(parameter[1]) });
                }
            }

            $.ajax({
                url: matematici.urls.instances.api + "/" + (newInstance ? "Create" : "Get"),
                type: "post",
                data: {
                    code: options.code,
                    prototype: options.prototype,
                    version: options.version == "-1" ? null : options.version,
                    parameters: parameters
                }
            }).done(function (result) {
                id = result.id;
                state = result.state;
                code = result.code;
                department = result.department;
                departmentName = result.departmentName;
                prototypeType = result.prototypeType;
                prototypeTypeName = result.prototypeTypeName;
                prototype = result.prototype;
                prototypeName = result.prototypeName;
                version = result.version;
                versionState = result.versionState;
                year = result.year;
                number = result.number;

                setupInstanceDescription();
                setupTab(result.checkList.pages);
                setupActionBar();
                setupBreadcrumbs();

                update(result);
                if (newInstance)
                    matematici.navigator.setUrl(options.detailUrl + "/" + result.code);

                element.removeClass("Loading");
            });
        },

        sign = function (grid, rows, row) {
            if (row.approved) {
                matematici.dialog.error("error", "This signature has already been issued");
                return true;
            }

            var
                allowed = false,
                tags = row.tags.split(", ");

            for (var role in tags)
                if (
                    matematici.tokenManager.checkRole("Instances#Confirm#" + department + "##" + tags[role])
                    || matematici.tokenManager.checkRole("Instances#Confirm#" + department + "#" + prototypeType + "#" + tags[role])
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

            for (rowIndex in rows) {
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
                title: "Instance confirmation",
                reason: row.reason,
                url: element.data("signature"),
                instance: id,
                applicationStep: row.id,
                rows: rows,
                concurrencyInformation: { modifiedBy: null, modifiedOn: null }, //TODO: concurrency fot the signature 

                done: function (result) {
                    grid.close();
                    matematici.popup.close();
                    var stateChanged = state != result.state;
                    state = result.state;
                    setupActionBar();
                    setupBreadcrumbs();
                    if (!stateChanged)
                        instance.confirm();
                    if (result.approved)
                        matematici.blip.ok();
                    else
                        matematici.blip.ko();
                }
            });
            return true;
        },

        hasPermission = function (instanceFunction) {
            return versionState != prototypes.Prototype.states.draft && versionState != prototypes.Prototype.states.simulation
                && (state == instances.Instance.states.draft || state == instances.Instance.states.progress)
                && instanceFunction //when null, only prototype grants are checked
                && (matematici.tokenManager.checkRole("Instances#" + instanceFunction + "#" + department + "##")
                    || matematici.tokenManager.checkRole("Instances#" + instanceFunction + "#" + department + "#" + prototypeType + "#"))
                || (versionState == prototypes.Prototype.states.draft || versionState == prototypes.Prototype.states.simulation)
                && (state == instances.Instance.states.draft || state == instances.Instance.states.progress)
                && (matematici.tokenManager.checkRole("Prototypes#Preview#" + department + "##")
                    || matematici.tokenManager.checkRole("Prototypes#Preview#" + department + "#" + prototypeType + "#"));
        },

        send = function (action, note, callback) {
            $.ajax({
                url: matematici.urls.instances.api + "/" + action,
                type: "post",
                data: {
                    code: code,
                    note: note
                }
            }).done(function (result) {
                id = result.id || id;
                state = result.state || state;
                year = result.year || year;
                number = result.number || number;

                setupActionBar();
                setupBreadcrumbs();

                if (result.checkList)
                    update(result);

                if (callback)
                    callback();
            });
        };

    instance.getCode = function () {
        return code;
    };

    instance.getElement = function () {
        return element;
    };

    instance.getExpressions = function () {
        return expressions;
    };

    instance.getExternalSources = function () {
        return externalSources;
    };

    instance.getVersionState = function () {
        return versionState;
    };

    instance.getState = function () {
        return state;
    };

    instance.getDepartment = function () {
        return department;
    };

    instance.getPrototypeType = function () {
        return prototypeType;
    };

    instance.getPrototype = function () {
        return prototype;
    };

    instance.getVersion = function () {
        return version;
    };

    instance.update = update;

    instances.instance = instance;

    instance.refresh = function () {
        new instances.Event(instances.Events.refresh).trigger();
    };

    instance.download = function () {
        $.ajax({
            url: matematici.urls.instances.api + "/Download",
            type: "post",
            data: { code: code }
        }).done(function (result) {
            location = matematici.urls.instances.api + "/Download?Token=" + result.token;
        });
    };

    instance.quit = function () {
        var hasPermissionOnPrototype =
            hasPermission(null)
            && (
                matematici.tokenManager.checkRole("Prototypes#View#" + department + "##")
                || matematici.tokenManager.checkRole("Prototypes#View#" + department + "#" + prototypeType + "#")
            );

        if (options.appMode)
            window.webkit.messageHandlers.instances.postMessage("quit");
        else
            matematici.navigator.open(
                hasPermissionOnPrototype && (versionState == prototypes.Prototype.states.draft || versionState == prototypes.Prototype.states.simulation) ?
                    matematici.urls.prototypes.detail + "/" + prototype + "/" + version :
                    options.listUrl
            );
    };

    instance.save = function () {
        send(state == instances.Instance.states.draft ? "Open" : "Save");
    };

    instance.delete = function () {
        if (!(versionState == prototypes.Prototype.states.draft || versionState == prototypes.Prototype.states.simulation) && state == instances.Instance.states.progress)
            matematici.dialog.show("Instance deletion", "Instance will be set as deleted. Provide a note for the deletion.", [
                {
                    text: "Confirm", icon: "check", handler: function () {
                        send("Delete", $("textarea.Textarea", $(this).closest("div.Dialog")).val(), function () {
                            deleted = true;
                            instance.quit();
                        });
                    }
                },
                { icon: "times", hidden: true, cancelButton: true },
                {
                    textarea: true,
                    marginTop: "10px"
                }
            ]);
        else
            matematici.dialog.show("Instance deletion", "The instance will be deleted", [
                {
                    text: "Confirm", icon: "check", handler: function () {
                        send("Delete", null, function () {
                            deleted = true;
                            instance.quit();
                        });
                    }
                },
                { icon: "times", hidden: true, cancelButton: true },
            ]);
    };

    instance.complete = function () {
        matematici.dialog.show("warning", "Please confirm the change of state.", [
            {
                text: "confirm", icon: "check", handler: function () {
                    send("Complete");
                }
            },
            { text: "cancel", icon: "times", cancelButton: true }
        ]);
    };

    instance.confirm = function () {
        return $.ajax({
            url: element.data("api") + "/WorkFlows/Applications/GetSteps",
            type: "POST",
            data: {
                instance: id
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
                    return sign(grid, result.steps, row);
                },
                null,
                columns);
        });
    };

    instance.setOnUpdateHandler = function (handler) {
        updateHandler = handler;
    };

    instance.showHistory = function () {
        $.ajax({
            url: matematici.urls.instances.api + "/History",
            type: "post",
            data: {
                id: id
            }
        }).done(function (result) {
            var
                columns = [
                    { label: "State", name: "state" },
                    { label: "Date", name: "date", type: "DateTimeOffset" },
                    { label: "User", name: "user" },
                    { label: "Note", name: "note" },
                    { label: "Approved", name: "approved", type: "Boolean", align: "center" },
                    { label: "Proposed state", name: "proposedState" },
                    { label: "Signature step", name: "step", align: "center" },
                    { label: "Signature reason", name: "reason" },
                    { label: "Signature comment", name: "comment" },
                    { label: "Appointed signers", name: "appointedUser" }
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

            parameters.append($("<label />").append($("<span />").text("Department")).append($("<i />").addClass("Icon fal fa-industry")).append($("<input />").attr(attributes).val(departmentName)));
            parameters.append($("<label />").append($("<span />").text("Prototype type")).append($("<i />").addClass("Icon fal fa-folder")).append($("<input />").attr(attributes).val(prototypeTypeName)));
            parameters.append($("<label />").append($("<span />").text("Prototype version")).append($("<i />").addClass("Icon fal fa-file-invoice")).append($("<input />").attr(attributes).val(prototypeName + "." + version)));
            parameters.append($("<label />").append($("<span />").text("Instance")).append($("<i />").addClass("Icon fal fa-list-ol")).append($("<input />").attr(attributes).val(getFormattedYear() + "." + number)));
        });
    };

    instance.toggleRemovedIterations = function (value) {
        removedIterationsVisible = value;
        setupActionBar();
        instance.update(lastResult);
    };

    instance.areRemovedIterationsVisible = function () {
        return removedIterationsVisible;
    };

    instance.showArray = function (value) {
        showArray(value);
    };

    instance.initialize = function () {
        instanceElementInformation = new instances.ElementInformation(element[0]);
        tab = new matematici.Tab($("div.TabContainer", element)).addHandler(function (item) {
            var
                scroll = matematici.elements.document.scrollTop(),
                pageIndex = page.getPageIndex(),
                top = page.getElement().offsetTop + 110;
            if (item.index == pageIndex && scroll > top)
                $("document, html").animate({ scrollTop: top }, 300);
            else {
                page.clear();
                if (state == instances.Instance.states.draft || state == instances.Instance.states.progress)
                    new instances.Event(instances.Events.pageChange, item.index).trigger();
                else
                    update(lastResult);
            }
        });
        load();
    };

    instance.close = function (callback) {
        if (!deleted && hasPermission("Edit")) {
            new instances.Event(instances.Events.quit, null, null, null, true).done(function () {
                callback();
            }).error(function () {
                callback();
            }).trigger();
        } else
            callback();
    };

    instance.destroy = function () {
    };

    instance.showExceptions = showExceptions;

    instance.initialize();
};

instances.Instance.states = {
    draft: "Draft",
    progress: "In progress",
    completed: "Completed",
    confirmed: "Confirmed",
    deleted: "Deleted"
};