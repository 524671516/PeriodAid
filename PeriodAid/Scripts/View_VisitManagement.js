$(function () {
    //日期格式改变
    $(".file-date").datepicker({ dateFormat: "yy-mm-dd" });
    //星期格式汉化
    $(".file-date").datepicker("option", "dayNamesMin", ["日", "一", "二", "三", "四", "五", "六"]);
    //月份格式汉化
    $(".file-date").datepicker("option", "monthNames", ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"]);
    $(".file-date").datepicker();
    
})