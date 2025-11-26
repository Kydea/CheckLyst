instances.Tasks = function (formSelector, resultSelector) {
    var
        tasks = this,
        resultContainer = document.getElementById(resultSelector),

        resultHandler = function (result) {
            resultContainer.innerHTML = "";
            resultContainer.classList.remove("Hidden");

            for (var index in result.records) {
                var record = result.records[index];

                var url = matematici.urls.instances.list + "?";

                div = document.createElement("div");
                resultContainer.appendChild(div);
                div.classList.add("Item");

                span = document.createElement("span");
                div.appendChild(span);
                span.classList.add("Label");
                span.innerText = "task";
                span = document.createElement("span");
                div.appendChild(span);
                span.classList.add("Value");
                span.innerText = record.taskCode;

                span = document.createElement("span");
                div.appendChild(span);
                span.classList.add("Description");
                span.innerText = record.taskDescription;

                span = document.createElement("span");
                div.appendChild(span);
                span.classList.add("Label");
                span.innerText = "instance";
                span = document.createElement("span");
                div.appendChild(span);
                span.classList.add("Value");
                span.innerText = record.instanceCode;

                span = document.createElement("span");
                div.appendChild(span);
                span.classList.add("Label");
                span.innerText = "department";
                span = document.createElement("span");
                div.appendChild(span);
                span.classList.add("Value");
                span.innerText = record.departmentName;

                span = document.createElement("span");
                div.appendChild(span);
                span.classList.add("Label");
                span.innerText = "peroototype type";
                span = document.createElement("span");
                div.appendChild(span);
                span.classList.add("Value");
                span.innerText = record.prototypeTypeName;

                span = document.createElement("span");
                div.appendChild(span);
                span.classList.add("Label");
                span.innerText = "prototype ";
                span = document.createElement("span");
                div.appendChild(span);
                span.classList.add("Value");
                span.innerText = record.prototypeName + "." + record.version;

                span = document.createElement("span");
                div.appendChild(span);
                span.classList.add("Description");
                span.innerText = record.description;

                var anchor = document.createElement("a");
                div.appendChild(anchor);
                anchor.setAttribute("href", matematici.urls.instances.detail + "/" + record.code);
                anchor.innerText = "open the instance";
                anchor.setAttribute("target", "_blank");
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