// Array Remove - By John Resig (MIT Licensed)
Array.remove = function (array, from, to) {
    var rest = array.slice((to || from) + 1 || array.length);
    array.length = from < 0 ? array.length + from : from;
    return array.push.apply(array, rest);
};

Array.removeElement = function (array, value) {
    var index = array.indexOf(value);
    if (index >= 0) array.splice(index, 1);
};

Array.addElement = function (array, value) {
    var index = array.indexOf(value);
    if (index < 0) array.push(value);
};