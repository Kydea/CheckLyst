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

        createEmptyCounter = function (previousElement, previousCounter) {
            var
                span,
                div;

            if (previousElement) {
                div = document.createElement("div");
                previousElement.insertBefore(div, previousElement.childNodes[1]);
                div.classList.add("Values");

                span = document.createElement("span");
                div.appendChild(span);
                span.innerText = "in progress instances:";
                span = document.createElement("span");
                span.classList.add("Value");
                div.appendChild(span);
                span.innerText = previousCounter.progress;

                span = document.createElement("span");
                div.appendChild(span);
                span.innerText = "completed instances:";
                span = document.createElement("span");
                span.classList.add("Value");
                div.appendChild(span);
                span.innerText = previousCounter.completed;

                span = document.createElement("span");
                div.appendChild(span);
                span.innerText = "confirmed instances:";
                span = document.createElement("span");
                span.classList.add("Value");
                div.appendChild(span);
                span.innerText = previousCounter.confirmed;

                span = document.createElement("span");
                div.appendChild(span);
                span.innerText = "deleted instances:";
                span = document.createElement("span");
                span.classList.add("Value");
                div.appendChild(span);
                span.innerText = previousCounter.deleted;
            }

            return { progress: 0, completed: 0, confirmed: 0, deleted: 0 };
        }

        resultHandler = function (result) {
            var
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
            actionBar.classList.remove("Hidden");

            for (var index in result.records) {
                var record = result.records[index];

                if (department != record.department) {
                    department = record.department
                    departmentCounter = createEmptyCounter(departmentElement, departmentCounter);
                    departmentElement = document.createElement("div");
                    resultContainer.appendChild(departmentElement);
                    departmentElement.classList.add("DepartmentItem", "Item");
                    createToggleButton(departmentElement, "department", record.departmentName)
                    prototypeType = null;
                }

                if (prototypeType != record.prototypeType) {
                    prototypeType = record.prototypeType
                    prototypeTypeCounter = createEmptyCounter(prototypeTypeElement, prototypeTypeCounter);
                    prototypeTypeElement = document.createElement("div");
                    departmentElement.appendChild(prototypeTypeElement);
                    prototypeTypeElement.classList.add("PrototypeTypeItem", "Item");
                    createToggleButton(prototypeTypeElement, "prototype type", record.prototypeTypeName)
                    prototype = null;
                }

                if (prototype != record.prototypeName) {
                    prototype = record.prototypeName
                    prototypeCounter = createEmptyCounter(prototypeElement, prototypeCounter);
                    prototypeElement = document.createElement("div");
                    prototypeTypeElement.appendChild(prototypeElement);
                    prototypeElement.classList.add("PrototypeItem", "Item");
                    createToggleButton(prototypeElement, "prototype", record.prototypeName)
                    version = null;
                }

                if (version != record.version) {
                    version = record.version
                    versionCounter = createEmptyCounter(versionElement, versionCounter);
                    versionElement = document.createElement("div");
                    prototypeElement.appendChild(versionElement);
                    versionElement.classList.add("VersionItem", "Item");
                    createToggleButton(versionElement, "version", record.version)
                }

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

            createEmptyCounter(departmentElement, departmentCounter);
            createEmptyCounter(prototypeTypeElement, prototypeTypeCounter);
            createEmptyCounter(prototypeElement, prototypeCounter);
            createEmptyCounter(versionElement, versionCounter);

            resultContainer
        },

        initialize = function () {
            new matematici.SearchPage({
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
        };

    initialize();
};