$.validator.addMethod("phone", function (value, element) {
    var tel = /^(13[0-9]|14[0-9]|15[0-9]|18[0-9])\d{8}$/;
    return this.optional(element) || (tel.test(value));
}, "请正确填写您的手机号码");
$.validator.addMethod("idnumber", function (value, element) {
    var tel = /(^\d{18}$)|(^\d{15}$)|(^\d{17}(\\d|X|x))$/;
    return this.optional(element) || (tel.test(value));
}, "请正确填写您的身份证号码");
$.validator.addMethod("time", function (value, element) {
    var time = /^([01]\d|2[01234]):([0-5]\d|60)$/;
    return this.optional(element) || (time.test(value));
}, "请正确填写您的时间");
$.validator.addMethod("datearray", function (value, element) {
    var date = /([0-9]{4})-([0-9]{2})-([0-9]{2})$/;
    var array = value.split(",");
    var result = true;
    for (var i = 0; i < array.length; i++) {
        var r = !/Invalid|NaN/.test(new Date(array[i]).toString());
        result = r && result;
    }
    return this.optional(element) || (result);
}, "请正确填写您的时间");

(function (factory) {
    if (typeof define === "function" && define.amd) {
        define(["jquery", "../jquery.validate"], factory);
    } else {
        factory(jQuery);
    }
}(function ($) {
    /*
     * Translated default messages for the jQuery validation plugin.
     * Locale: ZH (Chinese, 中文 (Zhōngwén), 汉语, 漢語)
     */
    $.extend($.validator.messages, {
        required: "这是必填字段",
        remote: "请修正此字段",
        email: "请输入有效的电子邮件地址",
        url: "请输入有效的网址",
        date: "请输入有效的日期",
        dateISO: "请输入有效的日期 (YYYY-MM-DD)",
        number: "请输入有效的数字",
        digits: "只能输入数字",
        creditcard: "请输入有效的信用卡号码",
        equalTo: "你的输入不相同",
        extension: "请输入有效的后缀",
        maxlength: $.validator.format("最多可以输入 {0} 个字符"),
        minlength: $.validator.format("最少要输入 {0} 个字符"),
        rangelength: $.validator.format("请输入长度在 {0} 到 {1} 之间的字符串"),
        range: $.validator.format("请输入范围在 {0} 到 {1} 之间的数值"),
        max: $.validator.format("请输入不大于 {0} 的数值"),
        min: $.validator.format("请输入不小于 {0} 的数值")
    });
}));

