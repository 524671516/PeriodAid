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
        var showmonth = parseInt(month) < 10 ? "0" + parseInt(month) : parseInt(month);
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
    //获取url中的参数
    getUrlParam = function(name){
        var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)"); //构造一个含有目标参数的正则表达式对象
        var r = window.location.search.substr(1).match(reg);  //匹配目标参数
        if (r != null) return unescape(r[2]); return null; //返回参数值
    }
    // 手机号码验证  
    jQuery.validator.addMethod("isMobile", function (value, element) {
        var length = value.length;
        var mobile = /^(13[0-9]{9})|(18[0-9]{9})|(14[0-9]{9})|(17[0-9]{9})|(15[0-9]{9})$/;
        return this.optional(element) || (length == 11 && mobile.test(value));
    }, "请正确填写您的手机号码");

    // 重置时间显示格式
    String.prototype.ToString = function (format) {
        var dateTime = new Date(parseInt(this.substring(6, this.length - 2)));
        format = format.replace("yyyy", dateTime.getFullYear());
        format = format.replace("yy", dateTime.getFullYear().toString().substr(2));
        format = format.replace("MM", (dateTime.getMonth() + 1) < 10 ? "0" + (dateTime.getMonth() + 1) : (dateTime.getMonth() + 1))
        format = format.replace("dd", dateTime.getDate() < 10 ? "0" + dateTime.getDate() : dateTime.getDate());
        format = format.replace("hh", dateTime.getHours() < 10 ? "0" + dateTime.getHours() : dateTime.getHours());
        format = format.replace("mm", dateTime.getMinutes());
        format = format.replace("ss", dateTime.getSeconds());
        format = format.replace("ms", dateTime.getMilliseconds())
        return format;
    };
    //获取ui所要展示的天数
    TmDatePicker.getMonthDate = function (year, month, day) {
        var resdata = [];
        var lastdayofmonth = new Date(year, month, 0);           //本月的最后一天
        var lastdayoflastmonth = new Date(year, month - 1, 0);   //获取上月的最后一天
        var lastdateoflastmonth = lastdayoflastmonth.getDate();  //获取上个月最后一天是几号
        var lastdateofmonth = lastdayofmonth.getDate();          //获取本月最后一天是几号
        var firstdayofmonth = new Date(year, month - 1, 1);     //本月的第一天
        if (TmDatePicker.currentui == "month") {
            if (typeof (year) != "number" || typeof (month) != "number") {
                throw new Error('typeof yaer/month is not a number!');
            } else {
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
            }
        }
        return {
            currentyear: firstdayofmonth.getFullYear(),
            currentmonth: firstdayofmonth.getMonth() + 1,
            lastday: lastdateofmonth,
            days: resdata
        }

    }



    //创建日历模板
    TmDatePicker.createPanel = function (datearray) {
        switch (TmDatePicker.currentui) {
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
                var _html="<div class=\"tm-calendar-view-panel\" id=\"tm-calender-picker\">"+
                "<div class=\"tm-calendar-inner\">"+
                "<div class=\"tm-calendar-inner-header  tm-calendar-week-header\">" +
                "<div class=\"tm-calendar-time-area float-left\"><div class=\"tm-calendar-time\"  style=\"height:40px;border-bottom: 1px solid #E5E5E5;\"></div></div>" +
                "<div class=\"tm-calendar-content-area float-left\"><ul class=\"clear-float calendar-week-area\">";
                var _htmlbodyfooter = "</ul></div></div><div class=\"tm-calendar-inner-header tm-calendar-week-header\">"+
                        "<div class=\"tm-calendar-time-area float-left\"><div class=\"tm-calendar-time\" style=\"line-height:40px;height:40px;border-bottom: 1px solid #E5E5E5;\">全天</div></div>" +
                        "<div class=\"tm-calendar-content-area float-left\"><ul class=\"clear-float calendar-week-area\">" +
                            "<li></li><li></li><li></li><li></li><li></li><li></li><li></li></ul></div>"+
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
                    _htmlfooterpart = "<ul class=\"clear-float calendar-week-area\"><li class=\"tm-calendar-weekofday\"></li><li class=\"tm-calendar-weekofday\"></li><li class=\"tm-calendar-weekofday\"></li><li class=\"tm-calendar-weekofday\"></li><li class=\"tm-calendar-weekofday\"></li><li class=\"tm-calendar-weekofday\"></li><li class=\"tm-calendar-weekofday\"></li></ul>"
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
        var next = TmDatePicker.getOptionValue("nextbtn");
        var prev = TmDatePicker.getOptionValue("prebtn");
        var changeui = TmDatePicker.getOptionValue("changeuibtn");
        $(next).off("click");
        $(prev).off("click");
        $(changeui).off("click");
        var lastdayofmonth = new Date(TmDatePicker.currentyear, TmDatePicker.currentmonth, 0);           //本月的最后一天
        var lastdayoflastmonth = new Date(TmDatePicker.currentyear, TmDatePicker.currentmonth - 1, 0);   //获取上月的最后一天
        var lastdateoflastmonth = lastdayoflastmonth.getDate();  //获取上个月最后一天是几号
        var lastdateofmonth = lastdayofmonth.getDate();          //获取本月最后一天是几号
        $(changeui).on("click", function () {
            if (TmDatePicker.currentui == "week") {
                TmDatePicker.currentui = "month";
            } else {
                TmDatePicker.currentui = "week";
            }
            var onuichanged = TmDatePicker.getOptionValue("onuichanged");
            if (typeof (onuichanged) == "function") {
                onuichanged(TmDatePicker, TmDatePicker.currentui);
            } else {
                throw new Error("onuichanged is not a function");
            }
            TmDatePicker.bindEvent();
            TmDatePicker.createUi();
        });
        $(prev).on("click", function () {
            if (TmDatePicker.currentui == "week") {
                TmDatePicker.currentday -= 7;
                if (TmDatePicker.currentday <= 0) {
                    TmDatePicker.currentday = TmDatePicker.currentday + lastdateoflastmonth;
                    TmDatePicker.currentmonth -= 1;
                }
            } else {
                TmDatePicker.currentmonth -= 1;
            }
                if (TmDatePicker.currentmonth < 1) {
                    TmDatePicker.currentmonth = 12;
                    TmDatePicker.currentyear -= 1;
                }
                TmDatePicker.createUi()
            });
            $(next).on("click", function () {
                if (TmDatePicker.currentui == "week") {
                    TmDatePicker.currentday += 7;
                    if (TmDatePicker.currentday > lastdateofmonth) {
                        TmDatePicker.currentday = TmDatePicker.currentday - lastdateofmonth;
                        TmDatePicker.currentmonth += 1;
                    }
                } else {
                    TmDatePicker.currentmonth += 1;
                }
                if (TmDatePicker.currentmonth > 12) {
                    TmDatePicker.currentmonth = 1;
                    TmDatePicker.currentyear += 1;
                }
                TmDatePicker.createUi()
            });
    }
    TmDatePicker.createUi = function () {
        var date = TmDatePicker.defaultOptions.initfulldate;
        var datearray = TmDatePicker.getMonthDate(TmDatePicker.currentyear, TmDatePicker.currentmonth, TmDatePicker.currentday);
        TmDatePicker.datearray = datearray;
        var html = TmDatePicker.createPanel(datearray);
        $("#tm-calender-picker").remove();
        TmDatePicker.container.html(html);
        if (TmDatePicker.currentui == "week") {
            if ($(".tm-calendar-currentmonth-weekofday[date-day=" + TmDatePicker.getToday().fulldate + "]").length > 0) {
                var _dindex = $(".tm-calendar-currentmonth-weekofday[date-day=" + TmDatePicker.getToday().fulldate + "]").index();
                var _hindex = TmDatePicker.getToday().hour;
                var _mindex = TmDatePicker.getToday().minute;
                var tp = $(".tm-calendar-week-body .calendar-week-area:eq(" + _hindex + ")").find("li:eq(" + _dindex + ")").height() / 60 * _mindex + "px";
                var _style = "height:1px;background-color:rgba(255,0,0,1);width:100%;position:absolute;display:block;line-height:1px;text-align:center;" + "top:" + tp;
                $("#tm-calender-picker ul").find("li:eq(" + _dindex + ")").css("background", "rgba(61, 168, 245, 0.1)");
                $(".tm-calendar-week-body .calendar-week-area:eq(" + _hindex + ")").find("li:eq(" + _dindex + ")").append("<span class=\"tm-today-line\" style=" + _style + "></span>");
            }
        } else {
            $(".tm-calendar-monthofdate[date-day=" + date + "]").addClass("tm-calendar-monthofcurrentdate");
        }
        TmDatePicker.showtimearea.html(TmDatePicker.currentyear + "-" + (TmDatePicker.currentmonth < 10 ? "0" + TmDatePicker.currentmonth : TmDatePicker.currentmonth));
        var onpagechanged = TmDatePicker.getOptionValue("onpagechanged");
        if (typeof (onpagechanged) == "function") {
            onpagechanged(TmDatePicker);
        } else {
            throw new Error("onpagechanged is not a function");
        }
    }
    // 删除数组中的某个元素
    Array.prototype.indexOf = function(val) {
            for (var i = 0; i < this.length; i++) {
            if (this[i] == val) return i;
            }
            return -1;
            };
    Array.prototype.remove = function(val) {
            var index = this.indexOf(val);
            if (index > -1) {
            this.splice(index, 1);
            }
            };  
    

    TmDatePicker.defaultOptions = {
        container: "body",                              //默认会append在body里面
        initfulldate: TmDatePicker.getToday().fulldate, //默认时间         形式为2017-8-25
        initmonth: TmDatePicker.getToday().month,       //默认当前月份      //后期删除
        inityear: TmDatePicker.getToday().year,         //默认当前年份      //后期删除
        initday: TmDatePicker.getToday().day,           //默认当前日        //后期删除
        nextbtn: ".tm-calendar-next",
        prebtn: ".tm-calendar-prev",
        changeuibtn: ".tm-change-calendar-ui",         //切换ui
        showtimearea: ".tm-calendar-currentmonth",     //展示时间的容器
        initui:"month",                                //初始ui样式 "week"和"month"可选
        onpagechanged: function (e) {

        },
        onuichanged: function (e, currentui) {

        },
    };
    TmDatePicker.Init = function (options) {
        if (arguments.length > 1) {
            throw new Error("the function of Init only can accept object!");
        } else {
            if (typeof (options) == "object" && !TmDatePicker.isArray(options)) {
                TmDatePicker.userOptions = options;                               //绑定用户参数
                var initui = TmDatePicker.getOptionValue("initui");               
                var container = TmDatePicker.getOptionValue("container");         
                var showtimearea = TmDatePicker.getOptionValue("showtimearea");   
                TmDatePicker.currentui = initui;                          //绑定初始化ui值
                TmDatePicker.container = $(container);                   //绑定初始化日历容器
                TmDatePicker.showtimearea = $(showtimearea);             //绑定初始化当前月显示位置
                TmDatePicker.currentyear = TmDatePicker.getOptionValue("inityear");
                TmDatePicker.currentmonth = TmDatePicker.getOptionValue("initmonth");
                TmDatePicker.currentday = TmDatePicker.getOptionValue("initday");
                TmDatePicker.createUi();   //创建ui
                TmDatePicker.bindEvent();  //创建事件
            } else {
                throw new Error("the function of Init only can accept object!");
            };
        }

    }
    window.TmDatePicker = TmDatePicker
}(jQuery));