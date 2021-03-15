ace.define("ace/theme/expression", ["require", "exports", "module", "ace/lib/dom"], function (require, exports, module) {

    exports.isDark = false;
    exports.cssClass = "ace-expression";
    exports.cssText = ".ace-expression .ace_gutter {\
background: #ebebeb;\
color: #333;\
overflow : hidden;\
}\
.ace-expression .ace_print-margin {\
width: 1px;\
background: #e8e8e8;\
}\
.ace-expression {\
background-color: #FFFFFF;\
color: black;\
}\
.ace-expression .ace_cursor {\
color: black;\
}\
.ace-expression .ace_invisible {\
color: rgb(191, 191, 191);\
}\
.ace-expression .ace_constant.ace_buildin {\
color: rgb(88, 72, 246);\
}\
.ace-expression .ace_constant.ace_language {\
color: rgb(88, 92, 246);\
}\
.ace-expression .ace_constant.ace_library {\
color: rgb(6, 150, 14);\
}\
.ace-expression .ace_invalid {\
background-color: rgb(153, 0, 0);\
color: white;\
}\
.ace-expression .ace_fold {\
}\
.ace-expression .ace_support.ace_function {\
color: rgb(60, 76, 114);\
}\
.ace-expression .ace_support.ace_constant {\
color: rgb(6, 150, 14);\
}\
.ace-expression .ace_support.ace_type,\
.ace-expression .ace_support.ace_class\
.ace-expression .ace_support.ace_other {\
color: rgb(109, 121, 222);\
}\
.ace-expression .ace_variable.ace_parameter {\
font-style:italic;\
color:#FD971F;\
}\
.ace-expression .ace_keyword.ace_operator {\
color: rgb(104, 118, 135);\
}\
.ace-expression .ace_comment {\
color: #236e24;\
}\
.ace-expression .ace_comment.ace_doc {\
color: #236e24;\
}\
.ace-expression .ace_comment.ace_doc.ace_tag {\
color: #236e24;\
}\
.ace-expression .ace_constant.ace_numeric {\
color: rgb(0, 0, 205);\
}\
.ace-expression .ace_variable {\
color: rgb(49, 132, 149);\
}\
.ace-expression .ace_xml-pe {\
color: rgb(104, 104, 91);\
}\
.ace-expression .ace_entity.ace_name.ace_function {\
color: #0000A2;\
}\
.ace-expression .ace_heading {\
color: rgb(12, 7, 255);\
}\
.ace-expression .ace_list {\
color:rgb(185, 6, 144);\
}\
.ace-expression .ace_marker-layer .ace_selection {\
background: rgb(181, 213, 255);\
}\
.ace-expression .ace_marker-layer .ace_step {\
background: rgb(252, 255, 0);\
}\
.ace-expression .ace_marker-layer .ace_stack {\
background: rgb(164, 229, 101);\
}\
.ace-expression .ace_marker-layer .ace_bracket {\
margin: -1px 0 0 -1px;\
border: 1px solid rgb(192, 192, 192);\
}\
.ace-expression .ace_marker-layer .ace_active-line {\
background: rgba(0, 0, 0, 0.07);\
}\
.ace-expression .ace_gutter-active-line {\
background-color : #dcdcdc;\
}\
.ace-expression .ace_marker-layer .ace_selected-word {\
background: rgb(250, 250, 255);\
border: 1px solid rgb(200, 200, 250);\
}\
.ace-expression .ace_storage,\
.ace-expression .ace_keyword,\
.ace-expression .ace_meta.ace_tag {\
color: rgb(147, 15, 128);\
}\
.ace-expression .ace_string.ace_regex {\
color: rgb(255, 0, 0)\
}\
.ace-expression .ace_string {\
color: #1A1AA6;\
}\
.ace-expression .ace_entity.ace_other.ace_attribute-name {\
color: #994409;\
}\
.ace-expression .ace_indent-guide {\
background: url(\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAACCAYAAACZgbYnAAAAE0lEQVQImWP4////f4bLly//BwAmVgd1/w11/gAAAABJRU5ErkJggg==\") right repeat-y;\
}" +
//custom
"\
.ace_tooltip {\
background: white !important;\
font-size: 12px;\
}";

    var dom = require("../lib/dom");
    dom.importCssString(exports.cssText, exports.cssClass);
}); (function () {
    ace.require(["ace/theme/expression"], function (m) {
        if (typeof module == "object" && typeof exports == "object" && module) {
            module.exports = m;
        }
    });
})();
