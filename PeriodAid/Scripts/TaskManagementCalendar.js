(function ($) {
    if (typeof ($) != "function") {
        throw new Error("jquery is not a function!");
        return;
    }
    var TmDatePicker = {
        initui: {
            week: "week",   //每周的ui
            month:"month",  //每月的ui
        }
    };
    //获取今日实时数据
    TmDatePicker.getToday = function () {
        var today = new Date();
        var year = today.getFullYear();
        var month = today.getMonth() + 1;
        var day = today.getDate();
        var hour = today.getHours();
        var minute = today.getMinutes();
        var showmonth = parseInt(month) < 10 ? "0" + parseInt(month) : parseInt(month + 1);
        var showday = parseInt(day) < 10 ? "0" + parseInt(day) : parseInt(day);
        var date = year + "-" + showmonth + "-" + showday;
        return {
            year: year,
            month:month,
            day: day,
            hour: hour,
            minute:minute,
            fulldate: date
        };
    }

    //获取ui所要展示的天数
    TmDatePicker.getMonthDate = function (year, month, day) {
        var resdata = [];
        var lastdayofmonth = new Date(year, month, 0);           //本月的最后一天
        var lastdayoflastmonth = new Date(year, month - 1, 0);   //获取上月的最后一天
        var lastdateoflastmonth = lastdayoflastmonth.getDate();  //获取上个月最后一天是几号
        var lastdateofmonth = lastdayofmonth.getDate();          //获取本月最后一天是几号
        if (TmDatePicker.currentui == "month") {
            if (typeof (year) != "number" || typeof (month) != "number") {
                throw new Error('typeof yaer/month is not a number!');
            } else {
                var firstdayofmonth = new Date(year, month - 1, 1);     //本月的第一天
                var firstdayofweekday = firstdayofmonth.getDay();  //计算每月第一天是星期几
                if (firstdayofweekday === 0) firstdayofweekday = 7     //星期日转化为7
                var lastdayscount = firstdayofweekday - 1;           //上个月需要补充的天数
                for (var i = 0; i < 42; i++) {
                    var virtualdate = i + 1 - lastdayscount;
                    var currentdate = virtualdate;
                    var currentmonth = month;
                    var currentyear = firstdayofmonth.getFullYear();
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
                        currentyear -= 1;
                        currentmonth = 12;
                    } else if (currentmonth === 13) {
                        currentyear += 1;
                        currentmonth = 1;
                    }
                    var fulldate = currentyear + "-" + (currentmonth < 10 ? "0" + currentmonth : currentmonth) + "-" + (currentdate < 10 ? "0" + currentdate : currentdate);
                    resdata.push({
                        year: currentyear,
                        month: currentmonth,
                        date: currentdate,
                        fulldate: fulldate,
                        virtualdate: virtualdate
                    });
                }
                return {
                    currentyear: firstdayofmonth.getFullYear(),
                    currentmonth: firstdayofmonth.getMonth() + 1,
                    lastday: lastdateofmonth,
                    days: resdata
                }

            }
        } else {
            if (typeof (year) != "number" || typeof (month) != "number" || typeof (day) != "number") {
                throw new Error('typeof yaer/month/day is not a number!');
            } else {
                var today = new Date(year, month - 1, day);              //获取当天
                var week = today.getDay();                               //获取当天是星期几
                if (week == 0) week = 7;        //星期日转化为7
                var discount = week - 1;        //本周需要补充的天数
                for (i = 0; i < 7; i++) {
                    var virtualdate = day + i - discount;
                    var currentdate = virtualdate;
                    var currentmonth = month;
                    var currentyear = year;
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
                        //上一年
                        currentyear -= 1;
                        currentmonth = 12;
                    } else if (currentmonth === 13) {
                        //下一年
                        currentyear += 1;
                        currentmonth = 1;
                    }
                    var currentday = new Date(currentyear, currentmonth - 1, currentdate).getDay();
                    if (currentday == 0) currentday = 7;
                    switch (currentday) {
                        case 1:
                            currentday = "星期一"
                            break;
                        case 2:
                            currentday = "星期二"
                            break;
                        case 3:
                            currentday = "星期三"
                            break;
                        case 4:
                            currentday = "星期四"
                            break;
                        case 5:
                            currentday = "星期五"
                            break;
                        case 6:
                            currentday = "星期六"
                            break;
                        case 7:
                            currentday = "星期日"
                            break;
                        default:
                            currentday = "未知"
                            break;
                    }
                    var fulldate = currentyear + "-" + (currentmonth < 10 ? "0" + currentmonth : currentmonth) + "-" + (currentdate < 10 ? "0" + currentdate : currentdate);
                    resdata.push({
                        year: currentyear,
                        month: currentmonth,
                        date: currentdate,
                        day: currentday,
                        fulldate: fulldate,
                        virtualdate: virtualdate
                    });
                }
                return {
                    currentyear: firstdayofmonth.getFullYear(),
                    currentmonth: firstdayofmonth.getMonth() + 1,
                    lastday: lastdateofmonth,
                    days: resdata
                }
            }
        }

    }



    //创建日历模板
    TmDatePicker.createPanel = function (datearray) {
        var initui = TmDatePicker.getOptionValue("initui");
        switch (initui) {
            case "month":
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
                    _html+=parthtml;
                };
                _html += _htmlbodyfooter;
                break;
            case "week":
                var _html="<div class=\"tm-calendar-view-panel\">"+
                "<div class=\"tm-calendar-inner\">"+
                "<div class=\"tm-calendar-inner-header  tm-calendar-week-header\">"+
                "<ul class=\"clear-float calendar-week-area\"><li></li>";
                var _htmlbodyfooter = "</ul></div><div class=\"tm-calendar-inner-header tm-calendar-week-header\">"+
                        "<div class=\"tm-calendar-time-area\"></div>"+
                        "<ul class=\"clear-float calendar-week-area\">"+
                            "<li>全天</li><li></li><li></li><li></li><li></li><li></li><li></li><li></li></ul>"+
                    "</div><div class=\"tm-calendar-inner-body tm-calendar-week-body clear-float\">"+
                        "<div class=\"tm-calendar-time-area float-left\">"+
                           " <div class=\"tm-calendar-time\"><div>凌晨1点</div></div><div class=\"tm-calendar-time\"><div>凌晨2点</div></div>"+
                            "<div class=\"tm-calendar-time\"><div>凌晨3点</div></div><div class=\"tm-calendar-time\"><div>凌晨4点</div></div>"+
                            "<div class=\"tm-calendar-time\"><div>凌晨5点</div></div><div class=\"tm-calendar-time\"><div>早上6点</div></div>"+
                            "<div class=\"tm-calendar-time\"><div>早上7点</div></div><div class=\"tm-calendar-time\"><div>早上8点</div></div>"+
                            "<div class=\"tm-calendar-time\"><div>上午9点</div></div><div class=\"tm-calendar-time\"><div>上午10点</div></div>"+
                            "<div class=\"tm-calendar-time\"><div>上午11点</div></div><div class=\"tm-calendar-time\"><div>中午12点</div></div>"+
                            "<div class=\"tm-calendar-time\"><div>下午1点</div></div><div class=\"tm-calendar-time\"><div>下午2点</div></div>"+
                            "<div class=\"tm-calendar-time\"><div>下午3点</div></div><div class=\"tm-calendar-time\"><div>下午4点</div></div>"+
                            "<div class=\"tm-calendar-time\"><div>下午5点</div></div><div class=\"tm-calendar-time\"><div>下午6点</div></div>"+
                            "<div class=\"tm-calendar-time\"><div>晚上7点</div></div><div class=\"tm-calendar-time\"><div>晚上8点</div></div>"+
                            "<div class=\"tm-calendar-time\"><div>夜晚9点</div></div><div class=\"tm-calendar-time\"><div>夜晚10点</div></div>"+
                            "<div class=\"tm-calendar-time\"><div>夜晚11点</div></div><div class=\"tm-calendar-time\"></div></div><div class=\"tm-calendar-content-area float-left\">"
                for(i=0;i<24;i++){
                    var _htmlfooterpart;
                    _htmlfooterpart="<ul class=\"clear-float calendar-week-area\"><li>1</li><li>1</li><li>1</li><li>1</li><li>1</li><li>1</li><li>1</li></ul>"
                    _htmlbodyfooter+= _htmlfooterpart;
                }
                _htmlbodyfooter += "</div></div></div></div>";
                for (i = 0; i < datearray.days.length; i++) {
                    var parthtml;
                    if(datearray.days[i].virtualdate <= 0){
                        parthtml=  "<li class=\"tm-calendar-prevmonth-weekofday\" date-day=" + datearray.days[i].fulldate + ">"+datearray.days[i].day+ "&nbsp;"+datearray.days[i].month+"/"+ datearray.days[i].date+ "</li>";
                    }else if(datearray.days[i].virtualdate > datearray.lastday){
                        parthtml=  "<li class=\"tm-calendar-nextmonth-weekofday\" date-day=" + datearray.days[i].fulldate + ">"+datearray.days[i].day+ "&nbsp;"+datearray.days[i].month+"/"+ datearray.days[i].date+ "</li>";
                    }else{
                        parthtml=  "<li class=\"tm-calendar-currentmonth-weekofday\" date-day=" + datearray.days[i].fulldate + ">"+datearray.days[i].day+ "&nbsp;"+datearray.days[i].month+"/"+ datearray.days[i].date+ "</li>";
                    }
                    _html+=parthtml;
                }
                _html += _htmlbodyfooter;
                return _html;
                        
                break;
            default:
                throw new Error(initui +"is not a valid parameter");
        }
        
        return _html;
    }
    //判断传入参数是否为数组
    TmDatePicker.isArray = function (obj) {
        return Object.prototype.toString.call(obj) === '[object Array]';
    }

    //获取参数对应的值
    TmDatePicker.getOptionValue = function (option) {
        if (TmDatePicker.userOptions[option] && TmDatePicker.userOptions[option]!="") {
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
        var month = TmDatePicker.getOptionValue("initmonth");
        var year = TmDatePicker.getOptionValue("inityear");
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
        initfulldate: TmDatePicker.getToday().fulldate, //默认时间         形式为2017-8-25
        initmonth: TmDatePicker.getToday().month,       //默认当前月份      //后期删除
        inityear: TmDatePicker.getToday().year,         //默认当前年份      //后期删除
        initday: TmDatePicker.getToday().day,           //默认当前日        //后期删除
        nextbtn: ".tm-calendar-next",
        prebtn: ".tm-calendar-prev",
        showtimearea: ".tm-calendar-currentmonth",     //展示时间的容器
        initui:"month",                                //初始ui样式 "week"和"month"可选
        changeuibtn:".tm-calendar-change-ui",          //修改ui样式 周或者月来回切换 
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
                var initui = TmDatePicker.getOptionValue("initui");
                TmDatePicker.currentui = initui;
                TmDatePicker.createUiInit();
            } else {
                throw new Error("the function of Init only can accept object!");
            };
        }

    }
    window.TmDatePicker = TmDatePicker;
})(jQuery);