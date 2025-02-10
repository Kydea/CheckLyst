instances.LifeCycle = function (formSelector, resultSelector) {
    var
        lifeCycle = this,
        actionBar = document.getElementsByClassName("ActionBar")[0],
        resultContainer = document.getElementById(resultSelector),
        maxCount,
        createToggleButton = function (container, label, description) {
            var anchor = document.createElement("a");
            container.appendChild(anchor);
            anchor.classList.add("Toggle");
            anchor.setAttribute("href", "#");
            anchor.innerText = label + " ";
            var span = document.createElement("span");
            anchor.appendChild(span);
            span.classList.add("Black");
            span.innerText = description;

            anchor.onclick = function () {
                container.classList.toggle("Expanded");
            }
        },

        createEmptyCounter = function (previousElement, previousCounter, department, prototypeType, prototype, version) {
            var
                span,
                div,
                maxHeight = 55,
                minHeight = 5,

                openList = function (from, to) {
                    var parameters = "?";
                    if (from != null) parameters += "lifeCycleFrom=" + from + "&";
                    if (to != null) parameters += "lifeCycleTo=" + to + "&";
                    if (department != null) parameters += "department=" + department + "&";
                    if (prototypeType != null) parameters += "prototypeType=" + prototypeType + "&";
                    if (prototype != null) parameters += "prototype=" + prototype + "&";
                    if (version != null) parameters += "version=" + version + "&";
                    parameters += "advancedSearch=true";
                    //console.log(matematici.urls.instances.list + parameters);
                    window.open(matematici.urls.instances.list + parameters, "_blank");
                }

            if (previousElement) {
                div = document.createElement("div");
                previousElement.insertBefore(div, previousElement.childNodes[1]);
                div.classList.add("Values");

                var
                    total = previousCounter.green + previousCounter.yellow + previousCounter.red,
                    height = total / maxCount * (maxHeight - minHeight) + minHeight,
                    percentage;

                span = document.createElement("span");
                span.classList.add("Green");
                div.appendChild(span);
                percentage = previousCounter.green / total * 100;
                span.style.setProperty("width", percentage + "%")
                span.style.setProperty("height", height + "px")
                span.title = previousCounter.green + " instances (" + Math.round(percentage) + "% of the " + total + " total instances) under 80% of life cicle (click to open the list)";
                span.addEventListener("click", function () {
                    openList(0, 80);
                });

                span = document.createElement("span");
                span.classList.add("Yellow");
                div.appendChild(span);
                percentage = previousCounter.yellow / total * 100;
                span.style.setProperty("width", percentage + "%")
                span.style.setProperty("height", height + "px")
                span.title = previousCounter.yellow + " instances (" + Math.round(percentage) + "% of the " + total + " total instances) over 80% of life cicle (click to open the list)";
                span.addEventListener("click", function () {
                    openList(80, 100);
                });

                span = document.createElement("span");
                span.classList.add("Red");
                div.appendChild(span);
                percentage = previousCounter.red / total * 100;
                span.style.setProperty("width", percentage + "%")
                span.style.setProperty("height", height + "px")
                span.title = previousCounter.red + " instances (" + Math.round(percentage) + "% of the " + total + " total instances) over 100% of life cicle (click to open the list)";
                span.addEventListener("click", function () {
                    openList(100, null);
                });
            }

            return { green: 0, yellow: 0, red: 0 };
        },

        resultHandler = function (result) {
            var
                previousRecord = {},

                department,
                prototypeType,
                prototype,
                version,

                departmentCounter = createEmptyCounter(),
                prototypeTypeCounter = createEmptyCounter(),
                prototypeCounter = createEmptyCounter(),
                versionCounter = createEmptyCounter(),

                departmentElement,
                prototypeTypeElement,
                prototypeElement,
                versionElement;

            resultContainer.innerHTML = "";
            resultContainer.classList.remove("Hidden");

            maxCount = 0;
            var length = 0;
            for (var index in result.records) {
                var record = result.records[index];
                if (department == undefined)
                    department = record.department;
                if (department != record.department) {
                    department = record.department;
                    if (length > maxCount) maxCount = length;
                    length = 0;
                    version = null;
                }
                ++length;
            }
            if (length > maxCount) maxCount = length;

            department = undefined;

            for (var index in result.records) {
                var record = result.records[index];

                if (department != record.department) {
                    department = record.department;
                    departmentCounter = createEmptyCounter(departmentElement, departmentCounter, previousRecord.department);
                    departmentElement = document.createElement("div");
                    resultContainer.appendChild(departmentElement);
                    departmentElement.classList.add("DepartmentItem", "Item");
                    createToggleButton(departmentElement, "department", record.departmentName)
                    prototypeType = null;
                }

                if (prototypeType != record.prototypeType) {
                    prototypeType = record.prototypeType;
                    prototypeTypeCounter = createEmptyCounter(prototypeTypeElement, prototypeTypeCounter, previousRecord.department, previousRecord.prototypeType);
                    prototypeTypeElement = document.createElement("div");
                    departmentElement.appendChild(prototypeTypeElement);
                    prototypeTypeElement.classList.add("PrototypeTypeItem", "Item");
                    createToggleButton(prototypeTypeElement, "prototype type", record.prototypeTypeName)
                    prototype = null;
                }

                if (prototype != record.prototypeName) {
                    prototype = record.prototypeName;
                    prototypeCounter = createEmptyCounter(prototypeElement, prototypeCounter, previousRecord.department, previousRecord.prototypeType, previousRecord.prototype);
                    prototypeElement = document.createElement("div");
                    prototypeTypeElement.appendChild(prototypeElement);
                    prototypeElement.classList.add("PrototypeItem", "Item");
                    createToggleButton(prototypeElement, "prototype", record.prototypeName)
                    version = null;
                }

                if (version != record.version) {
                    version = record.version;
                    versionCounter = createEmptyCounter(versionElement, versionCounter, previousRecord.department, previousRecord.prototypeType, previousRecord.prototype, previousRecord.version);
                    versionElement = document.createElement("div");
                    prototypeElement.appendChild(versionElement);
                    versionElement.classList.add("VersionItem", "Item");
                    createToggleButton(versionElement, "version", record.version + " (life cicle: " + record.lifeCycle + " days)");
                }

                previousRecord = { department: record.department, prototypeType: record.prototypeType, prototype: record.prototype, version: record.version };

                var
                    percentage = Math.round(record.days / record.lifeCycle * 100),
                    counter = percentage < 80 ? "green" : percentage < 100 ? "yellow" : "red",
                    cssClass = percentage < 80 ? "Green" : percentage < 100 ? "Yellow" : "Red";

                var instanceElement = document.createElement("div");
                versionElement.appendChild(instanceElement);
                instanceElement.classList.add("InstanceItem", "Item");

                var anchor = document.createElement("a");
                instanceElement.appendChild(anchor);
                anchor.setAttribute("href", matematici.urls.instances.detail + "/" + record.code);
                anchor.innerText = "instance ";
                anchor.title = "open the instance";
                anchor.setAttribute("target", "_blank");
                var span = document.createElement("span");
                anchor.appendChild(span);
                span.classList.add("Black");
                span.innerText = record.instanceCode;
                span = document.createElement("span")
                anchor.appendChild(span);
                span.innerText = " (" + record.state + ", " + record.days + " days of life / " + record.lifeCycle + " days of life cicle = ";

                span = document.createElement("span")
                anchor.appendChild(span);
                span.classList.add(cssClass);
                span.innerText = percentage + "%";

                span = document.createElement("span")
                anchor.appendChild(span);
                span.classList.add("Value");
                span.innerText = ")";

                ++departmentCounter[counter];
                ++prototypeTypeCounter[counter];
                ++prototypeCounter[counter];
                ++versionCounter[counter];
            }

            createEmptyCounter(departmentElement, departmentCounter, previousRecord.department);
            createEmptyCounter(prototypeTypeElement, prototypeTypeCounter, previousRecord.department, previousRecord.prototypeType);
            createEmptyCounter(prototypeElement, prototypeCounter, previousRecord.department, previousRecord.prototypeType, previousRecord.prototype);
            createEmptyCounter(versionElement, versionCounter, previousRecord.department, previousRecord.prototypeType, previousRecord.prototypeName, previousRecord.version);

            resultContainer
        },

        initialize = function () {
            var searchPage = new matematici.SearchPage({
                pageName: "Instances",
                helpPage: "InstanceLifeCycle",
                filterFormSelector: formSelector,
                resultGridSelector: "div#Content table.Grid",
                bindEvent: "Bind",
                getDataEvent: "LifeCycle",
                searchButtonLabel: "submit",
                readOnly: true,
                filterHandler: resultHandler,
                breadCrumbs: [
                    { content: "instance life cycle" }
                ],
            });

            document.getElementById("ListButton").addEventListener("click", function (e) {
                var parameters = searchPage.getForm().prepareRequestString();
                if (parameters == "") parameters = "?";
                window.open(matematici.urls.instances.list + parameters + "&lifeCycleFrom=0&advancedSearch=true", "_blank").focus()
            });
        };

    initialize();
};