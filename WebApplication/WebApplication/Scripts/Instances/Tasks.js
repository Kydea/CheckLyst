instances.Tasks = function (formSelector, resultSelector) {
    var
        tasks = this,
        resultContainer = document.getElementById(resultSelector),

        resultHandler = function (result) {
            resultContainer.innerHTML = "";
            resultContainer.classList.remove("Hidden");
            var div, span, h2, i;

            for (var index in result.records) {
                var record = result.records[index];

                h2 = document.createElement("h2");
                resultContainer.appendChild(h2);
                h2.innerText = record.taskCode;

                div = document.createElement("div");
                resultContainer.appendChild(div);
                div.classList.add("Item", "Container");

                span = document.createElement("span");
                div.appendChild(span);
                span.classList.add("Value", "Date");
                span.innerText = record.taskValue == "True" ? "(no date)" : record.taskValue;

                span = document.createElement("span");
                div.appendChild(span);
                span.classList.add("Label");
                span.innerText = "Instance:";
                span = document.createElement("span");
                div.appendChild(span);
                span.classList.add("Value");
                span.innerText = record.instanceCode;

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
                span.innerText = record.description;

                span = document.createElement("span");
                div.appendChild(span);
                span.classList.add("Label");
                span.innerText = "Task description:";
                span = document.createElement("span");
                div.appendChild(span);
                span.classList.add("Value");
                span.innerText = record.taskDescription;

                var anchor = document.createElement("a");
                div.appendChild(anchor);
                anchor.classList.add("Button", "GreyYellowButton");
                anchor.setAttribute("href", matematici.urls.instances.detail + "/" + record.code);
                anchor.innerText = "open the instance";
                anchor.setAttribute("target", "_blank");

                i = document.createElement("i");
                anchor.appendChild(i);
                i.classList.add("Icon", "fa", "fa-link");

            }
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