$(function () {
    //验证特殊字符
    $.validator.addMethod("nospecilword", function (value, element, params) {
        var word_regexp = /[`~!@#\$%\^\&\*\(\)_\+<>\?:"\{\},\.\\\/;'\[\]]/;
        if (value.length == 0) {
            return true;
        } else {
            if (word_regexp.test(value)) {
                return false;
            } else {
                return true;
            }
        }
    });
    //验证手机号
    $.validator.addMethod("validatetel", function (value, element, params) {
        var word_regexp = /^1[34578]\d{9}$/;
        if (word_regexp.test(value)) {
            return true;
        } else {
            return false;
        }
    });
    //验证邮箱
    $.validator.addMethod("validateemail", function (value, element, params) {
        var word_regexp = /^\s*([A-Za-z0-9_-]+(\.\w+)*@([\w-]+\.)+\w{2,3})\s*$/
        if (value.length == 0) {
            return true;
        } else {
            if (word_regexp.test(value)) {
                return true;
            } else {
                return false;
            }
        }
    });
    //地区匹配
    $.validator.addMethod("validatearea", function (value, element, params) {
        var word_regexp = /请选择/;
        if (word_regexp.test(value)) {
            return false;
        } else {
            return true;
        }
    });
});