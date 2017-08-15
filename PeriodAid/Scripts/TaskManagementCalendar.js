(function ($) {
    if (typeof ($) != "function") {
        throw new Error("jquery is not a function!");
        return;
    }
    var TmDatePicker = {};
    //获取今日数据
    TmDatePicker.getToday = function () {
        var today = new Date();
        var year = today.getFullYear();
        var month = parseInt(today.getMonth() + 1) < 10 ? "0" + parseInt(today.getMonth() + 1) : parseInt(today.getMonth() + 1);
        var day = parseInt(today.getDate()) < 10 ? "0" + parseInt(today.getDate()) : parseInt(today.getDate());
        var date = year + "-" + month + "-" + day;
        return {
            month: today.getMonth() + 1,
            year: today.getFullYear(),
            day: today.getDate(),
            fulldate: date
        };
    }

    //默认获取42个格子的所有日期数据
    TmDatePicker.getMonthDate = function (year, month) {
        var resdata = [];
        if (typeof (year) != "number" || typeof (month) != "number") {
            throw new Error('typeof yaer/month is not a number!');
        } else {
            var firstdayofmonth = new Date(year, month - 1, 1);     //本月的第一天
            var lastdayofmonth = new Date(year, month, 0);        //本月的最后一天
            var lastdayoflastmonth = new Date(year, month - 1, 0);   //上个月的最后一天
            var firstdayofweekday = firstdayofmonth.getDay();  //计算每月第一天是星期几
            if (firstdayofweekday === 0) firstdayofweekday = 7     //星期日转化为7
            var lastdayscount = firstdayofweekday - 1;           //上个月需要补充的天数
            var lastdateoflastmonth = lastdayoflastmonth.getDate();  //获取上个月最后一天是几号
            var lastdateofmonth = lastdayofmonth.getDate();         ///获取本月最后一天是几号
            for (var i = 0; i < 42; i++) {
                var virtualdate = i + 1 - lastdayscount;
                var currentdate = virtualdate;
                var currentmonth = month;
                var currrentyear = firstdayofmonth.getFullYear();
                if (virtualdate <= 0) {
                    //上个月
                    currentmonth = month - 1;
                    currentdate = lastdateoflastmonth + virtualdate;
                } else if (virtualdate > lastdateofmonth) {
                    //下个月
                    currentmonth = month + 1;
                    currentdate = currentdate - lastdateofmonth;
                }
                if (currentmonth === 0) {
                    currrentyear -= 1;
                    currentmonth = 12;
                } else if (currentmonth === 13) {
                    currrentyear += 1;
                    currentmonth = 1;
                }
                var fulldate = currrentyear + "-" + (currentmonth < 10 ? "0" + currentmonth : currentmonth) + "-" + (currentdate < 10 ? "0" + currentdate : currentdate);
                resdata.push({
                    year: currrentyear,
                    month: currentmonth,
                    date: currentdate,
                    fulldate: fulldate,
                    virtualdate: virtualdate
                });
            }
            return {
                currrentyear: firstdayofmonth.getFullYear(),
                currentmonth: firstdayofmonth.getMonth() + 1,
                lastday: lastdateofmonth,
                days: resdata
            }

        }

    }

    //创建日历模板
    TmDatePicker.createPanel = function (datearray) {
        var days = [];
        var _html = "<div class=\"tm-calendar-view-panel\" id=\"tm-calender-picker\">" +
            "<div class=\"tm-calendar-inner\">" +
                "<div class=\"tm-calendar-inner-header tm-calendar-month-header\">" +
                    "<ul class=\"clear-float\">" +
                        "<li>星期一</li>" +
                        "<li>星期二</li>" +
                        "<li>星期三</li>" +
                        "<li>星期四</li>" +
                        "<li>星期五</li>" +
                        "<li>星期六</li>" +
                        "<li>星期日</li>" +
                    "</ul></div><div class=\"tm-calendar-inner-body tm-caleder-month-body\">";
        var _htmlbodyfooter = "</div></div></div>";
        for (i = 0; i < datearray.days.length; i++) {
            var parthtml;
            if (i % 7 == 0) {
                if (datearray.days[i].virtualdate <= 0) {
                    parthtml = "<ul class=\"tm-calendar-weekofmonth clear-float\">" +
                            "<li class=\"tm-calendar-weekofday tm-calendar-prevmonth-weekofday\"><div class=\"tm-calendar-monthofdate\" date-day=" + datearray.days[i].fulldate + ">" + datearray.days[i].date + "</div></li>"
                } else if (datearray.days[i].virtualdate > datearray.lastday) {
                    parthtml = "<ul class=\"tm-calendar-weekofmonth clear-float\">" +
    "<li class=\"tm-calendar-weekofday tm-calendar-nextmonth-weekofday\"><div class=\"tm-calendar-monthofdate\" date-day=" + datearray.days[i].fulldate + ">" + datearray.days[i].date + "<\div></li>"
                } else {
                    parthtml = "<ul class=\"tm-calendar-weekofmonth clear-float\">" +
    "<li class=\"tm-calendar-weekofday\"><div class=\"tm-calendar-monthofdate tm-calendar-currentmonth-weekofday\" date-day=" + datearray.days[i].fulldate + ">" + datearray.days[i].date + "</div></li>"
                }
            } else if (i % 7 == 6) {
                if (datearray.days[i].virtualdate <= 0) {
                    parthtml = "<li class=\"tm-calendar-weekofday tm-calendar-prevmonth-weekofday\"><div class=\"tm-calendar-monthofdate\" date-day=" + datearray.days[i].fulldate + ">" + datearray.days[i].date + "</div></li></ul>";
                } else if (datearray.days[i].virtualdate > datearray.lastday) {
                    parthtml = "<li class=\"tm-calendar-weekofday tm-calendar-nextmonth-weekofday\"><div class=\"tm-calendar-monthofdate\" date-day=" + datearray.days[i].fulldate + ">" + datearray.days[i].date + "</div></li></ul>";
                } else {
                    parthtml = "<li class=\"tm-calendar-weekofday tm-calendar-currentmonth-weekofday\"><div class=\"tm-calendar-monthofdate\" date-day=" + datearray.days[i].fulldate + ">" + datearray.days[i].date + "</div></li></ul>";
                }
            } else {
                if (datearray.days[i].virtualdate <= 0) {
                    parthtml = "<li class=\"tm-calendar-weekofday tm-calendar-prevmonth-weekofday\"><div class=\"tm-calendar-monthofdate\" date-day=" + datearray.days[i].fulldate + ">" + datearray.days[i].date + "</div></li>";
                } else if (datearray.days[i].virtualdate > datearray.lastday) {
                    parthtml = "<li class=\"tm-calendar-weekofday tm-calendar-nextmonth-weekofday\"><div class=\"tm-calendar-monthofdate\" date-day=" + datearray.days[i].fulldate + ">" + datearray.days[i].date + "</div></li>";
                } else {
                    parthtml = "<li class=\"tm-calendar-weekofday tm-calendar-currentmonth-weekofday\"><div class=\"tm-calendar-monthofdate\" date-day=" + datearray.days[i].fulldate + ">" + datearray.days[i].date + "</div></li>";
                }
            }
            days.push({
                year: datearray.days[i].year,
                month: datearray.days[i].month,
                date: datearray.days[i].date,
            })
            _html += parthtml;
        };
        TmDatePicker.days = days;
        _html += _htmlbodyfooter;
        return _html;
    }

    //判断传入参数是否为数组
    TmDatePicker.isArray = function (obj) {
        return Object.prototype.toString.call(obj) === '[object Array]';
    }

    //获取参数对应的值
    TmDatePicker.getOptionValue = function (option) {
        if (TmDatePicker.userOptions[option]) {
            return TmDatePicker.userOptions[option];
        } else {
            return TmDatePicker.defaultOptions[option];
        }
    }

    //绑定事件
    TmDatePicker.bindEvent = function () {
        var container = TmDatePicker.getOptionValue("container");
        var next = TmDatePicker.getOptionValue("nextbtn");
        var prev = TmDatePicker.getOptionValue("prebtn");
        $(prev).on("click", function () {
            TmDatePicker.currentmonth -= 1;
            if (TmDatePicker.currentmonth < 1) {
                TmDatePicker.currentmonth = 12;
                TmDatePicker.currentyear -= 1;
            }
            TmDatePicker.createUi()
        });
        $(next).on("click", function () {
            TmDatePicker.currentmonth += 1;
            if (TmDatePicker.currentmonth > 12) {
                TmDatePicker.currentmonth = 1;
                TmDatePicker.currentyear += 1;
            }
            TmDatePicker.createUi()
        });
        $(container).on("click", ".tm-calendar-prevmonth-weekofday", function () {
            TmDatePicker.currentmonth -= 1;
            if (TmDatePicker.currentmonth < 1) {
                TmDatePicker.currentmonth = 12;
                TmDatePicker.currentyear -= 1;
            }
            TmDatePicker.createUi()
        });
        $(container).on("click", ".tm-calendar-nextmonth-weekofday", function () {
            TmDatePicker.currentmonth += 1;
            if (TmDatePicker.currentmonth > 12) {
                TmDatePicker.currentmonth = 1;
                TmDatePicker.currentyear += 1;
            }
            TmDatePicker.createUi()
        })
    }
    TmDatePicker.createUi = function () {
        var container = TmDatePicker.getOptionValue("container");
        var showtimearea = TmDatePicker.getOptionValue("showtimearea");
        var month = TmDatePicker.currentmonth;
        var year = TmDatePicker.currentyear;
        var date = TmDatePicker.defaultOptions.currentdate;
        var datearray = TmDatePicker.getMonthDate(year, month);
        var html = TmDatePicker.createPanel(datearray);
        $("#tm-calender-picker").remove();
        $(container).append(html);
        $(".tm-calendar-monthofdate[date-day=" + date + "]").addClass("tm-calendar-monthofcurrentdate");
        $(showtimearea).html(year + "-" + (month < 10 ? "0" + month : month));
        var onpagechanged = TmDatePicker.getOptionValue("onpagechanged");
        if (typeof (onpagechanged) == "function") {
            onpagechanged(TmDatePicker);
        } else {
            throw new Error("onpagechanged is not a function");
        }
    }
    TmDatePicker.createUiInit = function () {
        var container = TmDatePicker.getOptionValue("container");
        var month = TmDatePicker.getOptionValue("currentmonth");
        var year = TmDatePicker.getOptionValue("currentyear");
        var date = TmDatePicker.defaultOptions.currentdate;
        TmDatePicker.currentmonth = month;
        TmDatePicker.currentyear = year;
        var datearray = TmDatePicker.getMonthDate(year, month);
        var html = TmDatePicker.createPanel(datearray);
        $(container).append(html);
        $(".tm-calendar-monthofdate[date-day=" + date + "]").addClass("tm-calendar-monthofcurrentdate");
        TmDatePicker.bindEvent();
        var onpagechanged = TmDatePicker.getOptionValue("onpagechanged");
        if (typeof (onpagechanged) == "function") {
            onpagechanged(TmDatePicker);
        } else {
            throw new Error("onpagechanged is not a function");
        }
    }
    TmDatePicker.defaultOptions = {
        container: "body",                              //默认会append在body里面
        currentdate: TmDatePicker.getToday().fulldate, //默认时间
        currentmonth: TmDatePicker.getToday().month,    //默认当前月份
        currentyear: TmDatePicker.getToday().year,      //默认当前年份
        currentday: TmDatePicker.getToday().day,      //默认当前日
        nextbtn: ".tm-calendar-next",
        prebtn: ".tm-calendar-prev",
        showtimearea: ".tm-calendar-currentmonth",
        initui:"month",                               //初始ui样式 "week"和"month"可选
        changeuibtn:".tm-calendar-change-ui",         //修改ui样式 周或者月来回切换 
        onpagechanged: function (e) {

        },
        onchangeuied: function (e) {

        },

    };
    TmDatePicker.Init = function (options) {
        if (arguments.length > 1) {
            throw new Error("the function of Init only can accept object!");
        } else {
            if (typeof (options) == "object" && !TmDatePicker.isArray(options)) {
                TmDatePicker.userOptions = options;
                TmDatePicker.createUiInit();
            } else {
                throw new Error("the function of Init only can accept object!");
            };
        }

    }
    window.TmDatePicker = TmDatePicker;
})(jQuery);