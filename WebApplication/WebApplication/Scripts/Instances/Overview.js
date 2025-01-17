instances.Overview = function (formSelector, resultSelector) {
    var
        overview = this,
        actionBar = document.getElementsByClassName("ActionBar")[0],
        resultContainer = document.getElementById(resultSelector),

        createToggleButton = function (container, label, description) {
            var anchor = document.createElement("a");
            container.appendChild(anchor);
            anchor.classList.add("Toggle");
            anchor.setAttribute("href", "#");
            anchor.innerText = label + " ";
            var span = document.createElement("span");
            anchor.appendChild(span);
            span.innerText = description;

            anchor.onclick = function () {
                container.classList.toggle("Expanded");
            }
        },

        createEmptyCounter = function (previousElement, previousCounter, department, prototypeType, prototype, version) {
            var
                anchor,
                span,
                div;

            if (previousElement) {
                var url = matematici.urls.instances.list + "?";
                if (department != null) url += "department=" + department + "&";
                if (prototypeType != null) url += "prototypeType=" + prototypeType + "&";
                if (prototype != null) url += "prototype=" + prototype + "&";
                if (version != null) url += "version=" + version + "&";

                div = document.createElement("div");
                previousElement.insertBefore(div, previousElement.childNodes[1]);
                div.classList.add("Values");

                anchor = document.createElement("a");
                div.appendChild(anchor);
                anchor.innerText = "in progress instances:";
                anchor.setAttribute("href", url + "state=In progress");
                anchor.setAttribute("target", "_blank");
                anchor.title = "click to open the list"
                span = document.createElement("span");
                span.classList.add("Value");
                div.appendChild(span);
                span.innerText = previousCounter.progress;

                anchor = document.createElement("a");
                div.appendChild(anchor);
                anchor.innerText = "completed instances:";
                anchor.setAttribute("href", url + "state=Completed");
                anchor.setAttribute("target", "_blank");
                anchor.title = "click to open the list"
                span = document.createElement("span");
                span.classList.add("Value");
                div.appendChild(span);
                span.innerText = previousCounter.completed;

                anchor = document.createElement("a");
                div.appendChild(anchor);
                anchor.innerText = "confirmed instances:";
                anchor.setAttribute("href", url + "state=Confirmed");
                anchor.setAttribute("target", "_blank");
                anchor.title = "click to open the list"
                span = document.createElement("span");
                span.classList.add("Value");
                div.appendChild(span);
                span.innerText = previousCounter.confirmed;

                anchor = document.createElement("a");
                div.appendChild(anchor);
                anchor.innerText = "deleted instances:";
                anchor.setAttribute("href", url + "state=Deleted");
                anchor.setAttribute("target", "_blank");
                anchor.title = "click to open the list"
                span = document.createElement("span");
                span.classList.add("Value");
                div.appendChild(span);
                span.innerText = previousCounter.deleted;
            }

            return { progress: 0, completed: 0, confirmed: 0, deleted: 0 };
        }

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

            for (var index in result.records) {
                var record = result.records[index];

                if (department != record.department) {
                    department = record.department
                    departmentCounter = createEmptyCounter(departmentElement, departmentCounter, previousRecord.department);
                    departmentElement = document.createElement("div");
                    resultContainer.appendChild(departmentElement);
                    departmentElement.classList.add("DepartmentItem", "Item");
                    createToggleButton(departmentElement, "department", record.departmentName)
                    prototypeType = null;
                }

                if (prototypeType != record.prototypeType) {
                    prototypeType = record.prototypeType
                    prototypeTypeCounter = createEmptyCounter(prototypeTypeElement, prototypeTypeCounter, previousRecord.department, previousRecord.prototypeType);
                    prototypeTypeElement = document.createElement("div");
                    departmentElement.appendChild(prototypeTypeElement);
                    prototypeTypeElement.classList.add("PrototypeTypeItem", "Item");
                    createToggleButton(prototypeTypeElement, "prototype type", record.prototypeTypeName)
                    prototype = null;
                }

                if (prototype != record.prototypeName) {
                    prototype = record.prototypeName
                    prototypeCounter = createEmptyCounter(prototypeElement, prototypeCounter, previousRecord.department, previousRecord.prototypeType, previousRecord.prototype);
                    prototypeElement = document.createElement("div");
                    prototypeTypeElement.appendChild(prototypeElement);
                    prototypeElement.classList.add("PrototypeItem", "Item");
                    createToggleButton(prototypeElement, "prototype", record.prototypeName)
                    version = null;
                }

                if (version != record.version) {
                    version = record.version
                    versionCounter = createEmptyCounter(versionElement, versionCounter, previousRecord.department, previousRecord.prototypeType, previousRecord.prototype, previousRecord.version);
                    versionElement = document.createElement("div");
                    prototypeElement.appendChild(versionElement);
                    versionElement.classList.add("VersionItem", "Item");
                    createToggleButton(versionElement, "version", record.version)
                }

                previousRecord = { department: record.department, prototypeType: record.prototypeType, prototype: record.prototype, version: record.version };

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
                span.innerText = record.instanceCode;
                span = document.createElement("span")
                anchor.appendChild(span);
                span.classList.add("Value");
                span.innerText = " (" + record.state + ")";

                var counter = record.state == "In progress" ? "progress" : record.state == "Completed" ? "completed" : record.state == "Confirmed" ? "confirmed" : "deleted";
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
                filterFormSelector: formSelector,
                resultGridSelector: "div#Content table.Grid",
                bindEvent: "Bind",
                getDataEvent: "Overview",
                searchButtonLabel: "submit",
                readOnly: true,
                filterHandler: resultHandler,
                breadCrumbs: [
                    { content: "instance overview" }
                ],
            });

            document.getElementById("ListButton").addEventListener("click", function (e) {
                window.open(matematici.urls.instances.list + searchPage.getForm().prepareRequestString() + "&lifeCycleFrom=0&advancedSearch=true", "_blank").focus()
            });
        };

    initialize();
};