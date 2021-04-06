ace.define("ace/snippets/expression", ["require", "exports", "module"], function (require, exports, module) {
    "use strict";
    exports.snippets = [
        //operators
        {
            content: "and ${1:boolean expression}",
            name: "and",
            scope: "expression",
            meta: "operator"
        },
        {
            content: "not ${1:boolean expression}",
            name: "not",
            scope: "expression",
            meta: "operator"
        },
        {
            content: "or ${1:boolean expression}",
            name: "or",
            scope: "expression",
            meta: "operator"
        },

        //constants
        {
            content: "true",
            name: "true",
            scope: "expression",
            meta: "costant"
        },
        {
            content: "false",
            name: "false",
            scope: "expression",
            meta: "costant"
        },

        //functions
        {
            content: "DateIncrement(${1:date/date-time}, ${2:years}, ${3:months}, ${4:days})",
            name: "DateIncrement",
            scope: "expression",
            meta: "function",
            comment: "returns the provided date or date-time (the latter is truncated to the corresponding date value) incremented by the provided years, months and days"
        },
        {
            content: "DateTimeIncrement(${1:date/date-time}, ${2:years}, ${3:months}, ${4:days}, ${5:hours}, ${6:minutes})",
            name: "DateTimeIncrement",
            scope: "expression",
            meta: "function",
            comment: "returns the provided date (considered as date time woth 0 hours and minutes) or date-time incremented by the provided years, months, days, hours and minutes"
        },
        {
            content: "Error(${1:error message})",
            name: "Error",
            scope: "expression",
            meta: "function",
            comment: "returns an error with the provided description"
        },
        {
            content: "Null(${1:argument})",
            name: "Null",
            scope: "expression",
            meta: "function",
            comment: "returns a value null of with the same data type of the provided argument"
        },
        {
            content: "Date(${1:year}, ${2:month}, ${3:day})",
            name: "Date",
            scope: "expression",
            meta: "function",
            comment: "returns a date with the provided year, month and day"
        },
        {
            content: "DateTime(${1:year}, ${2:month}, ${3:day}, ${4:hour}, ${3:minute})",
            name: "DateTime",
            scope: "expression",
            meta: "function",
            comment: "returns a date-time with the provided year, month, day, hour and minute"
        },
        {
            content: "StateDateTime(${1:state})",
            name: "StateDateTime",
            scope: "expression",
            meta: "function",
            comment: "returns the date and time when the instance got the state \"Draft\" or \"In progress\""
        },
        {
            content: "Time(${1:hour}, ${2:minute})",
            name: "Time",
            scope: "expression",
            meta: "function",
            comment: "returns a time with the provided hour and minute"
        },

        //string funtions
        {
            content: "Format(${1:value}, ${2:format})",
            name: "Format",
            scope: "expression",
            meta: "function",
            comment: "returns the value formatted as string using the provided format"
        },
        {
            content: "StringLength(${1:value})",
            name: "StringLength",
            scope: "expression",
            meta: "function",
            comment: "returns the number of characters in value"
        },
        {
            content: "StringToLowerCase(${1:value})",
            name: "StringToLowerCase",
            scope: "expression",
            meta: "function",
            comment: "returns the value converted to lower case"
        },
        {
            content: "StringToUpperCase(${1:value})",
            name: "StringToUpperCase",
            scope: "expression",
            meta: "function",
            comment: "returns the value converted to upper case"
        },
        {
            content: "StringFind(${1:value}, ${2:keyword})",
            name: "StringFind",
            scope: "expression",
            meta: "function",
            comment: "returns the position of keyword into value, -1 if the keyword is not found"
        },
        {
            content: "StringReplace(${1:value}, ${2:keyword}, ${3:new-keyword})",
            name: "StringReplace",
            scope: "expression",
            meta: "function",
            comment: "returns the provided value with all the occurrences of keyword replaced by new-keyword"
        },
        {
            content: "Substring(${1:value}, ${2:initial-position}, ${3:length})",
            name: "Substring",
            scope: "expression",
            meta: "function",
            comment: "returns length characters (or all the characters if length is not provided) starting from initial-position"
        },
        {
            content: "StringRight(${1:value}, ${2:length})",
            name: "StringRight",
            scope: "expression",
            meta: "function",
            comment: "returns the last length characters to the right of value"
        },
        {
            content: "StringLeftPad(${1:value}, ${2:length}, ${3:pattern})",
            name: "StringLeftPad",
            scope: "expression",
            meta: "function",
            comment: "returns the concatenation of copies of pattern (or of spaces if pattern is omitted) to the left of value until the length is greater or equals to length"
        },
        {
            content: "StringRightPad(${1:value}, ${2:length}, ${3:pattern})",
            name: "StringRightPad",
            scope: "expression",
            meta: "function",
            comment: "returns the concatenation of copies of pattern (or of spaces if pattern is omitted) to the right of value until the length is greater or equals to length"
        },
        {
            content: "StringLeftTrim(${1:value}, ${2:pattern})",
            name: "StringLeftTrim",
            scope: "expression",
            meta: "function",
            comment: "returns the provided value, trimmered on the left of the longest sequence of concatenation of pattern (of spaces if pattern is not provided) possible"
        },
        {
            content: "StringRightTrim(${1:value}, ${2:pattern})",
            name: "StringRightTrim",
            scope: "expression",
            meta: "function",
            comment: "returns the provided value, trimmered on the right of the longest sequence of concatenation of pattern (of spaces if pattern is not provided) possible"
        },
        {
            content: "StringTrim(${1:value}, ${2:pattern})",
            name: "StringTrim",
            scope: "expression",
            meta: "function",
            comment: "returns the provided value, trimmered both sides of the longest sequence of concatenation of pattern (of spaces if pattern is not provided) possible"
        },
        {
            content: "StringToNumber(${1:value})",
            name: "StringToNumber",
            scope: "expression",
            meta: "function",
            comment: "returns the provided value converted to number"
        },
    
        //math functions
        {
            content: "Atan(${1:argument})",
            name: "Atan",
            scope: "expression",
            meta: "math function",
            comment: "returns the arctangent of the argument"
        },
        {
            content: "Cos(${1:argument})",
            name: "Cos",
            scope: "expression",
            meta: "math function",
            comment: "returns the cosine of the argument"
        },
        {
            content: "Ln(${1:argument})",
            name: "Ln",
            scope: "expression",
            meta: "math function",
            comment: "returns the natural logarithm of the argument"
        },
        {
            content: "Log(${1:argument})",
            name: "Log",
            scope: "expression",
            meta: "math function",
            comment: "returns the base-10 logarithm of the argument"
        },
        {
            content: "Round(${1:number}, ${1:decimals})",
            name: "Round",
            scope: "expression",
            meta: "math function",
            comment: "returns the number rounded to the provided number of decimals"
        },
        {
            content: "Sin(${1:argument})",
            name: "Sin",
            scope: "expression",
            meta: "math function",
            comment: "returns the sin of the argument"
        },
        {
            content: "Sqrt(${1:argument})",
            name: "Sqrt",
            scope: "expression",
            meta: "math function",
            comment: "returns the square root of the argument"
        },
        {
            content: "Tan(${1:argument})",
            name: "Tan",
            scope: "expression",
            meta: "math function",
            comment: "returns the tangent of the argument"
        },

        //conditional operator
        {
            content: "iif(${1:condition}, ${2:true part}, ${3:false part})",
            name: "iif",
            scope: "expression",
            meta: "operator"
        },

        //methods
        {
            content: "Value",
            name: "Value",
            scope: "expression",
            meta: "property",
            interface: "Field",
            comment: "returns the value of the field"
        },

        {
            content: "Valid",
            name: "Valid",
            scope: "expression",
            meta: "property",
            interface: "ValidityItem",
            comment: "returns true if the item is valid, false otherwise"
        },

        {
            content: "ModifiedBy",
            name: "ModifiedBy",
            scope: "expression",
            meta: "property",
            interface: "EditableItem",
            comment: "returns the user code of last edit"
        },
        {
            content: "ModifiedOn",
            name: "ModifiedOn",
            scope: "expression",
            meta: "property",
            interface: "EditableItem",
            comment: "returns the date and time of last edit"
        },

        {
            content: "Signed",
            name: "Signed",
            scope: "expression",
            meta: "property",
            interface: "Signature",
            comment: "returns true if the signature is signed, false otherwise"
        },
        {
            content: "SignedOn",
            name: "SignedOn",
            scope: "expression",
            meta: "property",
            interface: "Signature",
            comment: "returns the date and time of the signature"
        },
        {
            content: "SignedBy",
            name: "SignedBy",
            scope: "expression",
            meta: "property",
            interface: "Signature",
            comment: "returns the signer user code"
        },
        {
            content: "HistoryLength",
            name: "HistoryLength",
            scope: "expression",
            meta: "property",
            interface: "Signature",
            comment: "returns the number of times a signature has been signed or unsigned"
        },

        {
            content: "Count",
            name: "Count",
            scope: "expression",
            meta: "property",
            interface: "Iterator",
            comment: "returns the number of itearations"
        },
        {
            content: "IterationIndex",
            name: "IterationIndex",
            scope: "expression",
            meta: "property",
            interface: "Iterator",
            comment: "returns the index of the current itearation"
        },
        {
            content: "Sum(${1:argument})",
            name: "Sum",
            scope: "expression",
            meta: "method",
            interface: "Iterator",
            comment: "returns the sum of the argument"
        },
        {
            content: "Avg(${1:argument})",
            name: "Avg",
            scope: "expression",
            meta: "method",
            interface: "Iterator",
            comment: "returns the average value of the argument"
        },
        {
            content: "StDev(${1:argument})",
            name: "StDev",
            scope: "expression",
            meta: "method",
            interface: "Iterator",
            comment: "returns the standard variation of the argument"
        },
        {
            content: "Any(${1:argument})",
            name: "Any",
            scope: "expression",
            meta: "method",
            interface: "Iterator",
            comment: "returns true if for any iteration the argument is true"
        },
        {
            content: "All(${1:argument})",
            name: "All",
            scope: "expression",
            meta: "method",
            interface: "Iterator",
            comment: "returns true if for all the iterations the argument is true"
        },
        {
            content: "Corr(${1:y}, ${1:x})",
            name: "Corr",
            scope: "expression",
            meta: "method",
            interface: "Iterator",
            comment: "returns the correlation coefficient of the provided (x, y) set"
        },
        {
            content: "Slope(${1:y}, ${1:x})",
            name: "Slope",
            scope: "expression",
            meta: "method",
            interface: "Iterator",
            comment: "returns the slope of the provided (x, y) set"
        },
        {
            content: "Intercept(${1:y}, ${1:x})",
            name: "Intercept",
            scope: "expression",
            meta: "method",
            interface: "Iterator",
            comment: "returns the intercept of the provided (x, y) set"
        },
        {
            content: "Each(${1:argument})",
            name: "Each",
            scope: "expression",
            meta: "method",
            interface: "Iterator",
            comment: "returns and array containing the provided argument evaluated over all the iterations"
        },
        {
            content: "IterationValue(${1:index}, ${2:argument})",
            name: "IterationValue",
            scope: "expression",
            meta: "method",
            interface: "Iterator",
            comment: "returns the argument evaluated over index-th iteration"
        },
        {
            content: "RecurrenceTerm(${1:delta}, ${2:default})",
            name: "RecurrenceTerm",
            scope: "expression",
            meta: "method",
            interface: "Iterator",
            comment: "returns the recurrence term in current position - delta or default if delta is less or equal to 0 or grater than the current position"
        },


    ];
    exports.scope = "";

}); (function () {
    ace.require(["ace/snippets/expression"], function (m) {
        if (typeof module == "object" && typeof exports == "object" && module) {
            module.exports = m;
        }
    });
})();
