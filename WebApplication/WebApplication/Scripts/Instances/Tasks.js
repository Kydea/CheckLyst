instances.Tasks = function (formSelector, resultSelector) {
    var
        tasks = this,
        resultContainer = document.getElementById(resultSelector),

        testEmpty = function (value) {
            return value == "" || value == null ? "&nbsp;" : value;
        },

        resultHandler = function (result) {
            resultContainer.innerHTML = "";
            resultContainer.classList.remove("Hidden");
            var div, span, h2, i;

            if (result.records == null || result.records.length == 0) {
                div = document.createElement("div");
                resultContainer.appendChild(div);
                div.classList.add("Item", "Container", "NoRecord");
                div.innerText = "(no task found)";
            } else
                for (var index in result.records) {
                    var record = result.records[index];

                    h2 = document.createElement("h2");
                    resultContainer.appendChild(h2);
                    h2.innerText = record.taskDescription;

                    h2.addEventListener("click", function () {
                        this.classList.toggle("Expanded");
                        var expanded = this.classList.contains("Expanded");
                        this.nextSibling.style.display = expanded ? "block" : "none";
                        this.nextSibling.nextSibling.style.display = expanded ? "none" : "block";
                    });

                    div = document.createElement("div");
                    resultContainer.appendChild(div);
                    div.classList.add("Item", "Container", "Expanded");

                    var collapsedDiv = document.createElement("div");
                    resultContainer.appendChild(collapsedDiv);
                    collapsedDiv.classList.add("Item", "Container", "Collapsed");

                    span = document.createElement("span");
                    div.appendChild(span);
                    span.classList.add("Value", "Date");
                    if (record.taskValue == "True")
                        span.innerText = "(no date)";
                    else {
                        var
                            date = moment(record.taskValue),
                            format = matematici.dateFormatter.format(date);

                        if (format.hours == "00" && format.minutes == "00" && format.seconds == "00")
                            span.innerText = format.date;
                        else {
                            span.innerText = format.date + " " + format.time;
                            span.setAttribute("title", format.offset);
                        }

                        if (date.isBefore(new Date())) span.classList.add("Overdue");
                    }
                    collapsedDiv.appendChild(span.cloneNode(true));

                    span = document.createElement("span");
                    div.appendChild(span);
                    span.classList.add("Label");
                    span.innerText = "Task code:";
                    span = document.createElement("span");
                    div.appendChild(span);
                    span.classList.add("Value");
                    span.innerText = record.taskCode;

                    span = document.createElement("span");
                    div.appendChild(span);
                    span.classList.add("Label");
                    span.innerText = "Task description:";
                    span = document.createElement("span");
                    div.appendChild(span);
                    span.classList.add("Value");
                    span.innerHTML = testEmpty(record.taskDescription);

                    span = document.createElement("span");
                    div.appendChild(span);
                    span.classList.add("Label");
                    span.innerText = "Instance code:";
                    collapsedDiv.appendChild(span.cloneNode(true));
                    span = document.createElement("span");
                    div.appendChild(span);
                    span.classList.add("Value");
                    span.innerText = record.instanceCode;
                    collapsedDiv.appendChild(span.cloneNode(true));

                    span = document.createElement("span");
                    div.appendChild(span);
                    span.classList.add("Label");
                    span.innerText = "Instance ID:";
                    collapsedDiv.appendChild(span.cloneNode(true));
                    span = document.createElement("span");
                    div.appendChild(span);
                    span.classList.add("Value");
                    span.innerText = record.id;
                    collapsedDiv.appendChild(span.cloneNode(true));

                    span = document.createElement("span");
                    div.appendChild(span);
                    span.classList.add("Label");
                    span.innerText = "Department:";
                    span = document.createElement("span");
                    div.appendChild(span);
                    span.classList.add("Value");
                    span.innerText = record.departmentName;

                    span = document.createElement("span");
                    div.appendChild(span);
                    span.classList.add("Label");
                    span.innerText = "Prototype type:";
                    span = document.createElement("span");
                    div.appendChild(span);
                    span.classList.add("Value");
                    span.innerText = record.prototypeTypeName;

                    span = document.createElement("span");
                    div.appendChild(span);
                    span.classList.add("Label");
                    span.innerText = "Prototype:";
                    span = document.createElement("span");
                    div.appendChild(span);
                    span.classList.add("Value");
                    span.innerText = record.prototypeName + "." + record.version;

                    span = document.createElement("span");
                    div.appendChild(span);
                    span.classList.add("Label");
                    span.innerText = "Instance description:";
                    span = document.createElement("span");
                    div.appendChild(span);
                    span.classList.add("Value");
                    span.innerHTML = testEmpty(record.description);

                    var anchor = document.createElement("a");
                    div.appendChild(anchor);
                    anchor.classList.add("Button", "GreyYellowButton");
                    anchor.setAttribute("href", matematici.urls.instances.detail + "/" + record.code);
                    anchor.innerText = "open the instance";
                    anchor.setAttribute("target", "_blank");

                    i = document.createElement("i");
                    anchor.appendChild(i);
                    i.classList.add("Icon", "fa", "fa-link");

                    collapsedDiv.appendChild(anchor.cloneNode(true));
                }

            matematici.clock.updateLightNotifications();
        },

        initialize = function () {
            var searchPage = new matematici.SearchPage({
                pageName: "Tasks",
                helpPage: "Tasks",
                filterFormSelector: formSelector,
                resultGridSelector: "div#Content table.Grid",
                bindEvent: "Bind",
                getDataEvent: "Tasks",
                searchButtonLabel: "submit",
                readOnly: true,
                filterHandler: resultHandler,
                breadCrumbs: [
                    { content: "tasks" }
                ],
            });
        };

    initialize();
};