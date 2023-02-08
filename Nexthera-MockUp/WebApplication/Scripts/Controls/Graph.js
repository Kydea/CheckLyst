matematici.Graph = function (selector, data) {
    var
        graph = this,
        element = $(selector),
        nodes = [],
        paths = [],

        addNode = function (column, nodeData) {
            var node = $("<div />").addClass("Node");
            node.append($("<span />").addClass("Label").html(nodeData.label));
            node.append($("<span />").addClass("Center").html(nodeData.center));
            column.append(node);

            nodes[nodeData.id] = node;
        },

        addColumn = function (columnData) {
            var column = $("<div />").addClass("Column Container");
            for (var node in columnData.nodes)
                addNode(column, columnData.nodes[node]);
            element.append(column);
        },

        addPath = function (pathData) {
            var
                containerOffset = element.offset(),
                source = nodes[pathData.sourceId],
                destination = nodes[pathData.destinationId],
                sourceOffset = source.offset();
            sourceOffset.left -= containerOffset.left;
            sourceOffset.top -= containerOffset.top;
            var
                sourceSize = {
                    width: source.outerWidth(), height: source.outerHeight()
                },
                destinationOffset = destination.offset(),
                destinationSize = {
                    width: destination.outerWidth(), height: destination.outerHeight()
                };
            destinationOffset.left -= containerOffset.left;
            destinationOffset.top -= containerOffset.top;
            var
                sourcePoint = {
                    x:
                        pathData.sourcePosition == "l" ? sourceOffset.left :
                            pathData.sourcePosition == "r" ? sourceOffset.left + sourceSize.width :
                                sourceOffset.left + sourceSize.width / 2,
                    y:
                        pathData.sourcePosition == "t" ? sourceOffset.top :
                            pathData.sourcePosition == "b" ? sourceOffset.top + sourceSize.height :
                                sourceOffset.top + sourceSize.height / 2
                },
                destinationPoint = {
                    x:
                        pathData.destinationPosition == "l" ? destinationOffset.left :
                            pathData.destinationPosition == "r" ? destinationOffset.left + destinationSize.width :
                                destinationOffset.left + destinationSize.width / 2,
                    y:
                        pathData.destinationPosition == "t" ? destinationOffset.top :
                            pathData.destinationPosition == "b" ? destinationOffset.top + destinationSize.height :
                                destinationOffset.top + destinationSize.height / 2
                };

            if (pathData.sourcePosition == "r" && pathData.destinationPosition == "l" || pathData.sourcePosition == "l" && pathData.destinationPosition == "r") {
                var
                    a = pathData.sourcePosition == "r" ? sourcePoint : destinationPoint,
                    b = pathData.sourcePosition == "r" ? destinationPoint : sourcePoint;

                paths.push($("<div />").appendTo(element).addClass("Path Horizontal").css({
                    left: a.x,
                    top: a.y,
                    width: (b.x - a.x) / 2,
                }));

                paths.push($("<div />").appendTo(element).addClass("Path Vertical").css({
                    left: a.x + (b.x - a.x) / 2,
                    top: Math.min(a.y, b.y),
                    height: Math.abs(b.y - a.y) + 1
                }));

                paths.push($("<div />").appendTo(element).addClass("Path Horizontal").css({
                    left: a.x + (b.x - a.x) / 2,
                    top: b.y,
                    width: (b.x - a.x) / 2,
                }));
            } else if (pathData.sourcePoint == "r" || pathData.sourcePoint == "l")
                alert("TODO");
            else if (pathData.sourcePosition == "b" && pathData.destinationPosition == "t" || pathData.sourcePosition == "t" && pathData.destinationPosition == "b") {
                var
                    a = pathData.sourcePosition == "b" ? sourcePoint : destinationPoint,
                    b = pathData.sourcePosition == "b" ? destinationPoint : sourcePoint;

                paths.push($("<div />").appendTo(element).addClass("Path Vertical").css({
                    left: a.x,
                    top: a.y,
                    height: (b.y - a.y) / 2,
                }));

                paths.push($("<div />").appendTo(element).addClass("Path Horizontal").css({
                    left: Math.min(a.x, b.x),
                    top: a.y + (b.y - a.y) / 2,
                    width: Math.abs(b.x - a.x) + 1
                }));

                paths.push($("<div />").appendTo(element).addClass("Path Vertical").css({
                    left: b.x,
                    top: a.y + (b.y - a.y) / 2,
                    height: (b.y - a.y) / 2,
                }));
            } else if (pathData.sourcePoint == "r" || pathData.sourcePoint == "l")
                alert("TODO")


            paths.push($("<div />").appendTo(element).addClass("Arrow To" + pathData.destinationPosition.toUpperCase()).css({
                left: destinationPoint.x,
                top: destinationPoint.y,
            }));
        },

        addPaths = function () {
            for (var path in data.paths)
                addPath(data.paths[path]);

        }

        clearPaths = function () {
            for (var path in paths)
                paths[path].remove();
        },

        resize = function () {
            clearPaths();
            addPaths();
        },

        setup = function () {
            element.addClass("Container Columns" + data.columns.length);
            for (var column in data.columns)
                addColumn(data.columns[column]);

            addPaths();

            matematici.elements.window.on("resize", resize);
        };

    this.initialize = function () {
        setup();
    };

    this.destroy = function () {
        matematici.elements.window.off("resize", resize);
    };

    this.getElement = function () {
        return element;
    }
};