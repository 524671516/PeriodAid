$.validator.addMethod("phone", function (value, element) {
    var tel = /^(13[0-9]|14[0-9]|15[0-9]|18[0-9])\d{8}$/;
    return this.optional(element) || (tel.test(value));
}, "请正确填写您的手机号码");
$.validator.addMethod("idnumber", function (value, element) {
    var tel = /(^\d{18}$)|(^\d{15}$)|(^\d{17}(\\d|X|x))$/;
    return this.optional(element) || (tel.test(value));
}, "请正确填写您的身份证号码");