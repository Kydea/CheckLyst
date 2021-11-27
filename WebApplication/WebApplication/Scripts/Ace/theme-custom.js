ace.define("ace/theme/custom", ["require", "exports", "module", "ace/lib/dom"], function (require, exports, module) {

    exports.isDark = false;
    exports.cssClass = "ace-custom";
    exports.cssText = ".ace-custom .ace_gutter {\
background: #ebebeb;\
color: #333;\
overflow : hidden;\
}\
.ace-custom .ace_print-margin {\
width: 1px;\
background: #e8e8e8;\
}\
.ace-custom {\
background-color: #FFFFFF;\
color: black;\
}\
.ace-custom .ace_cursor {\
color: black;\
}\
.ace-custom .ace_invisible {\
color: rgb(191, 191, 191);\
}\
.ace-custom .ace_constant.ace_buildin {\
color: rgb(88, 72, 246);\
}\
.ace-custom .ace_constant.ace_language {\
color: rgb(88, 92, 246);\
}\
.ace-custom .ace_constant.ace_library {\
color: rgb(6, 150, 14);\
}\
.ace-custom .ace_invalid {\
background-color: rgb(153, 0, 0);\
color: white;\
}\
.ace-custom .ace_fold {\
}\
.ace-custom .ace_support.ace_function {\
color: rgb(60, 76, 114);\
}\
.ace-custom .ace_support.ace_constant {\
color: rgb(6, 150, 14);\
}\
.ace-custom .ace_support.ace_type,\
.ace-custom .ace_support.ace_class\
.ace-custom .ace_support.ace_other {\
color: rgb(109, 121, 222);\
}\
.ace-custom .ace_variable.ace_parameter {\
font-style:italic;\
color:#FD971F;\
}\
.ace-custom .ace_keyword.ace_operator {\
color: rgb(104, 118, 135);\
}\
.ace-custom .ace_comment {\
color: #236e24;\
}\
.ace-custom .ace_comment.ace_doc {\
color: #236e24;\
}\
.ace-custom .ace_comment.ace_doc.ace_tag {\
color: #236e24;\
}\
.ace-custom .ace_constant.ace_numeric {\
color: rgb(0, 0, 205);\
}\
.ace-custom .ace_variable {\
color: rgb(49, 132, 149);\
}\
.ace-custom .ace_xml-pe {\
color: rgb(104, 104, 91);\
}\
.ace-custom .ace_entity.ace_name.ace_function {\
color: #0000A2;\
}\
.ace-custom .ace_heading {\
color: rgb(12, 7, 255);\
}\
.ace-custom .ace_list {\
color:rgb(185, 6, 144);\
}\
.ace-custom .ace_marker-layer .ace_selection {\
background: rgb(181, 213, 255);\
}\
.ace-custom .ace_marker-layer .ace_step {\
background: rgb(252, 255, 0);\
}\
.ace-custom .ace_marker-layer .ace_stack {\
background: rgb(164, 229, 101);\
}\
.ace-custom .ace_marker-layer .ace_bracket {\
margin: -1px 0 0 -1px;\
border: 1px solid rgb(192, 192, 192);\
}\
.ace-custom .ace_marker-layer .ace_active-line {\
background: rgba(0, 0, 0, 0.07);\
}\
.ace-custom .ace_gutter-active-line {\
background-color : #dcdcdc;\
}\
.ace-custom .ace_marker-layer .ace_selected-word {\
background: rgb(250, 250, 255);\
border: 1px solid rgb(200, 200, 250);\
}\
.ace-custom .ace_storage,\
.ace-custom .ace_keyword,\
.ace-custom .ace_meta.ace_tag {\
color: rgb(147, 15, 128);\
}\
.ace-custom .ace_string.ace_regex {\
color: rgb(255, 0, 0)\
}\
.ace-custom .ace_string {\
color: #1A1AA6;\
}\
.ace-custom .ace_entity.ace_other.ace_attribute-name {\
color: #994409;\
}\
.ace-custom .ace_indent-guide {\
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
    ace.require(["ace/theme/custom"], function (m) {
        if (typeof module == "object" && typeof exports == "object" && module) {
            module.exports = m;
        }
    });
})();
