matematici.DragContainer = function (element, options)  {
    var
        dragContext = {
            item: null,
            itemIndex: null,
            group: null,
            target: null,
            targetIndex: null
        },

        reset = function () {
            dragContext.target?.classList.remove("DragTarget")
            dragContext.item = null;
            dragContext.itemIndex = null;
            dragContext.group = null;
            dragContext.target = null;
            dragContext.targetIndex = null;
        },

        findTarget = function (group, mouseX, mouseY) {
            const elementAtPoint = document.elementFromPoint(mouseX, mouseY);
            const itemUnderMouse = elementAtPoint ? elementAtPoint.closest(options.itemSelector) : null;
            return itemUnderMouse && group.contains(itemUnderMouse) && itemUnderMouse !== dragContext.item ? itemUnderMouse : null;
        },

        dragStart = function (e) {
            const item = e.target.closest(options.itemSelector);
            if (!item) return;

            e.stopImmediatePropagation();
            e.stopPropagation();

            dragContext.item = item;

            let group = item.closest(options.groupSelector);
            if (group === item) group = item.parentElement?.closest(options.groupSelector);

            dragContext.group = group;

            e.dataTransfer.setData("text/plain", "");

            if (options.onDragStart) options.onDragStart(dragContext);
        },

        dragOver = function (e) {
            if (!dragContext.group) return;

            const group = e.target.closest(options.groupSelector);
            if (group !== dragContext.group) return;
            e.preventDefault();
            e.stopImmediatePropagation();
            e.stopPropagation();

            const target = findTarget(group, e.clientX, e.clientY);
            if (dragContext.target != target) {
                dragContext.target && dragContext.target.classList.remove("DragTarget");
                dragContext.target = target
                dragContext.target && dragContext.target.classList.add("DragTarget");
                if (options.onDragOver) options.onDragOver(dragContext);
            }
        },

        drop = function (e) {
            if (!dragContext.group) return;

            const group = e.target.closest(options.groupSelector);
            if (group !== dragContext.group) return;
            e.preventDefault();
            e.stopImmediatePropagation();
            e.stopPropagation();

            if (options.onDrop) {
                var items = Array.from(dragContext.group.querySelectorAll(options.itemSelector));
                dragContext.itemIndex = items.indexOf(dragContext.item);
                dragContext.targetIndex = dragContext.target ? items.indexOf(dragContext.target) : null;
                options.onDrop(dragContext);
            }
        },

        dragEnd = function () {
            if (options.onDragEnd) options.onDragEnd(dragContext);
            reset();
        },

        initialize = function () {
            options = options || {};
            if (!options.groupSelector || !options.itemSelector) 
                throw new Error("DragContainer: options.groupSelector e options.itemSelector are mandatory");
            element.addEventListener("dragstart", dragStart, true);
            element.addEventListener("dragover", dragOver, true);
            element.addEventListener("drop", drop, true);
            element.addEventListener("dragend", dragEnd, true);
        },

        destroy = function () {
            element.removeEventListener("dragstart", dragStart, true);
            element.removeEventListener("dragover", dragOver, true);
            element.removeEventListener("drop", drop, true);
            element.removeEventListener("dragend", dragEnd, true);
        };

    initialize();

    this.destroy = destroy;
}