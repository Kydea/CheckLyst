prototypes.PrototypeTypeProperty = function (model, options) {
	var
		input,
		handler,
		department,
		prototypeType,
		departments,

		preparePrototypeTypes = function () {
			departments = [];

			var
				data = prototypes.propertyPanel.getPrototypeTypesList(),
				departmentID = null,
				department;

			for (var index in data) {
				var record = data[index];
				if (departmentID != record.department) {
					departments.push(department = { code: record.department, name: record.departmentName, prototypeTypes: [] });
					departmentID = record.department;
				}

				department.prototypeTypes.push({
					id: record.id,
					name: record.name
				});
			}
		},

		refreshPrototypeTypes = function (initialValue) {
			prototypeType.empty();
			var departmentIndex = department.prop('selectedIndex');
			if (departmentIndex >= 0) {
				var prototypeTypes = departments[departmentIndex].prototypeTypes;
				for (var index in prototypeTypes)
					prototypeType.append($("<option />").text(prototypeTypes[index].name).attr("value", prototypeTypes[index].id));
				prototypeType.val(initialValue).change();
			} else 
				prototypeType.val(null).change();

            setTimeout(function () {
                matematici.AutoComplete.get(prototypeType[0]).refresh();
			}, 0);
		};


	this.create = function (valuePanel) {
		valuePanel.addClass("PrototypeType");

		preparePrototypeTypes();

		var label = $("<label />").appendTo(valuePanel);
        $("<i />").addClass("Icon fal fa-industry").appendTo(label);
		department =
			$("<select />")
                .attr("data-nullable", "")
				.on("change", function () { refreshPrototypeTypes(null); })
				.appendTo(label);

		for (var index in departments)
			department.append($("<option />").text(departments[index].name).attr("value", departments[index].code));

		label = $("<label />").appendTo(valuePanel);
		$("<i />").addClass("Icon fal fa-folder").appendTo(label);
		prototypeType =
			$("<select />")
				.attr("data-nullable", "")
				.on("change", function () {
					model.department = department.val();
					model.departmentName = department.next().val();
					model.prototypeType = prototypeType.val();
					model.prototypeTypeName = prototypeType.next().val();
					if (!options.readOnly)
						matematici.canClose = false;
				})
				.appendTo(label);

		department.val(model.department);
		if (!!model.department) refreshPrototypeTypes(model.prototypeType);

		if (options.readOnly) {
			matematici.controls.setEditable(department, false);
			matematici.controls.setEditable(prototypeType, false);
		}
	}

	this.hasChanges = function () {
		return !!(model.department || model.prototypeType);
	}
}

