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

        createEmptyCounter = function (previousElement, previousCounter) {
            var
                span,
                div,
                maxHeight = 55
            minHeight = 5;

            if (previousElement) {
                div = document.createElement("div");
                previousElement.insertBefore(div, previousElement.childNodes[1]);
                div.classList.add("Values");

                var
                    total = previousCounter.green + previousCounter.yellow + previousCounter.red,
                    height = total / maxCount * (maxHeight - minHeight) + minHeight,
                    percentage;

                if (height < 5 || height > 50 || !height) console.log(previousCounter.green, previousCounter.yellow, previousCounter.red, height);

                span = document.createElement("span");
                span.classList.add("Green");
                div.appendChild(span);
                percentage = previousCounter.green / total * 100;
                span.style.setProperty("width", percentage + "%")
                span.style.setProperty("height", height + "px")
                span.title = previousCounter.green + " intances (" + Math.round(percentage) + "% of the " + total + " in total) under 80% of life cicle";

                span = document.createElement("span");
                span.classList.add("Yellow");
                div.appendChild(span);
                percentage = previousCounter.yellow / total * 100;
                span.style.setProperty("width", percentage + "%")
                span.style.setProperty("height", height + "px")
                span.title = previousCounter.yellow + " intances (" + Math.round(percentage) + "% of the " + total + " in total) over 80% of life cicle";

                span = document.createElement("span");
                span.classList.add("Red");
                div.appendChild(span);
                percentage = previousCounter.red / total * 100;
                span.style.setProperty("width", percentage + "%")
                span.style.setProperty("height", height + "px")
                span.title = previousCounter.red + " intances (" + Math.round(percentage) + "% of the " + total + " in total) over 100% of life cicle";
            }

            return { green: 0, yellow: 0, red: 0 };
        },

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

            deparment = undefined;

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
                    createToggleButton(versionElement, "version", record.version + " (life cicle: " + record.lifeCycle + " days)");
                }

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

            createEmptyCounter(departmentElement, departmentCounter);
            createEmptyCounter(prototypeTypeElement, prototypeTypeCounter);
            createEmptyCounter(prototypeElement, prototypeCounter);
            createEmptyCounter(versionElement, versionCounter);

            resultContainer
        },

        initialize = function () {
            var searchPage = new matematici.SearchPage({
                pageName: "Instances",
                filterFormSelector: formSelector,
                resultGridSelector: "div#Content table.Grid",
                bindEvent: "Bind",
                getDataEvent: "LifeCycle",
                searchButtonLabel: "submit",
                readOnly: true,
                filterHandler: resultHandler,
                breadCrumbs: [
                    { content: "instance overview" }
                ],
            });

            document.getElementById("ListButton").addEventListener("click", function (e) {
                window.open(matematici.urls.instances.list + searchPage.getForm().prepareRequestString(), "_blank").focus()
            });
        };

    initialize();
};