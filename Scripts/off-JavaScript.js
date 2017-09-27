$(function () {
    /*----时间表汉化----*/
    $.datepicker.regional["zh-CN"] = {
        closeText: "关闭",
        prevText: "&#x3c;上月",
        nextText: "下月&#x3e;",
        currentText: "今天",
        monthNames: ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"],
        monthNamesShort: ["一", "二", "三", "四", "五", "六", "七", "八", "九", "十", "十一", "十二"],
        dayNames: ["星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六"],
        dayNamesShort: ["周日", "周一", "周二", "周三", "周四", "周五", "周六"],
        dayNamesMin: ["日", "一", "二", "三", "四", "五", "六"],
        weekHeader: "周",
        dateFormat: "yy-mm-dd",
        firstDay: 1, isRTL: !1,
        showMonthAfterYear: !0,
        yearSuffix: "年",     
    }
    $.datepicker.setDefaults($.datepicker.regional["zh-CN"]);
    /*------end--------*/
    $("#start-date").datepicker({ dateFormat: 'yy-mm-dd' });
    $("#end-date").datepicker({ dateFormat: 'yy-mm-dd' });
    
    $(".date-time").datepicker({
        autoSize: true,
    });
    /*start left navbar*/
    var left_nav = $("#current_page").val();
    $(".panel-collapse").removeClass("in");
    $(".leftnav-arrow").removeClass("panel-animate")
    $("div[aria-labelledby='" + left_nav + "']").addClass("in");
    $("i[aria-labelledby='" + left_nav + "']").addClass("panel-animate")
    /*------end------*/
    /*start scroll fixed*/
    $(document).scroll(function () {
        if (window.pageYOffset > 0) {
            $("#to-top").removeClass("hidden");
        }
        else
            $("#to-top").addClass("hidden");
    })
    $("#to-top").click(function () {
        scoll_ani();
    });
    function scoll_ani() {
        if (window.pageYOffset > 0) {
            $(document).scrollTop(window.pageYOffset - 50);
            setTimeout(scoll_ani, 50);
        }
    }
    /*------end------*/
    /*------批量删除------*/
    //全选
    $(document).delegate(".allChk", "click", function () {
        $("input[name='subChk']").prop("checked", this.checked);
    });
    //单选
    $(document).delegate("input[name='subChk']", "click", function () {
        $(".allChk").prop("checked", $("input[name='subChk']").length == $("input[name='subChk']").filter(":checked").length ? true : false);
    });
    /*-----end------*/
    /*----获取链接参数-------*/
    (function ($) {
        $.getUrlParam = function (name) {
            var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
            var r = window.location.search.substr(1).match(reg);
            if (r != null) return unescape(r[2]); return null;
        }
    })(jQuery);
    /*---end------*/
    /*------时间转换----------*/
    Date.prototype.Format = function (fmt) { //author: meizz
        var o = {
            "M+": this.getMonth() + 1,                 //月份
            "d+": this.getDate(),                    //日
            "h+": this.getHours(),                   //小时
            "m+": this.getMinutes(),                 //分
            "s+": this.getSeconds(),                 //秒
            "q+": Math.floor((this.getMonth() + 3) / 3), //季度
            "S": this.getMilliseconds()             //毫秒
        };
        if (/(y+)/.test(fmt))
            fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
        for (var k in o)
            if (new RegExp("(" + k + ")").test(fmt))
                fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
        return fmt;
    };
    /*----end------*/
    /*--------------门店列表----------------*/
    /*$.ajax({
        url: "/OffSales/ScheduleStoreListAjax",
        type: "post",
        data: {
            storesystem: $("#StoreSystem").val()
        },
        success: function (data) {
            $("#StoreList").html("");
            for (var i = 0; i < data.StoreList.length; i++) {
                $("#StoreList").append("<option value='" + data.StoreList[i].ID + "'>" + data.StoreList[i].StoreName + "</option>");
            }
        }
    });*/
    
    /*----end------*/
    /*-----------区域销售图表-------------*/
    $(".offstatistic-storesystem-btn").click(function () {
        var start = $("#start-date").val();
        var end = $("#end-date").val();
        var storesystemid = $("#StoreSystem").val();
        var storesystem = $("#StoreSystem :selected").text();
        var type = $(this).attr("data-salary");
        if (start > end) {
            $("#danger").text("开始时间不能大于结束时间")
        } else if (start == "" || end == "") {
            $("#danger").text("时间不能为空")
        } else {
            $("#danger").addClass("hidden")
            $.ajax({
                url: "/OffStatistic/StoreSystemStatisticAjax",
                type: "post",
                data: {
                    startdate: start,
                    enddate: end,
                    storesystemid: storesystemid,
                    type: type
                },
                success: function (data) {
                    if (data.data.length == 0) {
                        alert("该区域在此时间内无数据", "", function () {
                            //after click the confirm button, will run this callback function
                        }, { type: 'error' });
                    } else {
                        $("#map").show();
                        //区域销售折线图
                        var resultdata = data.data;
                        var totalarray = new Array();
                        var datearray = new Array();
                        for (var i = 0; i < resultdata.length; i++) {
                            var deeint = parseInt(data.data[i].Date.replace(/\D/igm, ""));
                            var dee = new Date(deeint)
                            datearray.push(dee.Format("MM-dd"));
                            var totaltemp = resultdata[i].SalesCount
                            totalarray.push(totaltemp);
                        }
                        $("#myChart_Bar").highcharts({
                            chart: {
                            },
                            title: {
                                text: storesystem + '每天销售总额折线图(单位：盒/天)'
                            },
                            xAxis: {
                                categories: datearray
                            },
                            series: [{
                                type: 'spline',
                                name: '销售额度',
                                data: totalarray,
                                marker: {
                                    lineWidth: 2,
                                    lineColor: Highcharts.getOptions().colors[3],
                                    fillColor: 'white'
                                }
                            }]
                        });
                        products_statisticsystem(start, end, storesystemid, type)
                        salary_statisticsystem(start, end, storesystemid, type)
                    };

                }
            });
            return false
        }
    });
    function products_statisticsystem(start, end, storesystemid, type) {
        var storesystem = $("#StoreSystem :selected").text();
        $.ajax({
            url: "/OffStatistic/StoreSystemProductStatisticAjax",
            type: "post",
            data: {
                startdate: start,
                enddate: end,
                storesystemid: storesystemid,
                type: type
            },
            success: function (data) {
                var productarry = new Array();
                for (var i = 0; i < data.data.length; i++) {
                    var productlist = {
                        name: data.data[i].SimpleName,
                        y: data.data[i].SalesCount
                    }
                    productarry.push(productlist)
                }
                $('#myChart_Pie').highcharts({
                    chart: {
                        plotBackgroundColor: null,
                        plotBorderWidth: null,
                        plotShadow: false
                    },
                    title: {
                        text: storesystem + '每天各个种类销售总额饼状图(单位：盒/天)'
                    },
                    tooltip: {
                        pointFormat: '{series.name}: <b>{point.percentage:.1f}%</b>'
                    },
                    plotOptions: {
                        pie: {
                            allowPointSelect: true,
                            cursor: 'pointer',
                            dataLabels: {
                                enabled: false
                            },
                            showInLegend: true
                        }
                    },
                    series: [{
                        type: 'pie',
                        name: '销售额度',
                        data: productarry
                    }]
                });
            }
        })
    }
    function salary_statisticsystem(start, end, storesystemid, type) {
        var storesystem = $("#StoreSystem :selected").text();
        $.ajax({
            url: "/OffStatistic/StoreSystemSalaryStatisticAjax",
            type: "post",
            data: {
                startdate: start,
                enddate: end,
                storesystemid: storesystemid,
                type: type
            },
            success: function (data) {
                if (data.data.length == 0) {
                    alert("该区域在此时间内无数据", "", function () {
                        //after click the confirm button, will run this callback function
                    }, { type: 'error' });
                } else {
                    var datearray = new Array();
                    var salaryarry = new Array();
                    var debitarry = new Array();
                    var bonusarry = new Array();
                    for (var i = 0; i < data.data.length; i++) {
                        var deeint = parseInt(data.data[i].Date.replace(/\D/igm, ""));
                        var dee = new Date(deeint)
                        datearray.push(dee.Format("MM-dd"));
                        var totalsalary = data.data[i].Salary
                        salaryarry.push(totalsalary);
                        var totaldebit = Math.abs(data.data[i].Debit)
                        debitarry.push(totaldebit);
                        var totalbonus = data.data[i].Bonus
                        bonusarry.push(totalbonus);
                    }
                    $('#myChart_Column').highcharts({
                        chart: {
                            zoomType: 'xy'
                        },
                        title: {
                            text: storesystem + '工资表'
                        },
                        xAxis: [{
                            categories: datearray
                        }],
                        yAxis: [{
                            labels: {
                                enabled: false
                            },
                            title: {
                                text: ''
                            }
                        }, {
                            labels: {
                                enabled: false
                            },
                            title: {
                                text: ''
                            }
                        }, {
                            labels: {
                                enabled: false
                            },
                            title: {
                                text: ''
                            }
                        }],
                        tooltip: {
                            shared: true
                        },

                        series: [{
                            name: '扣款',
                            color: '#89A54E',
                            type: 'column',
                            yAxis: 2,
                            data: debitarry,
                        }, {
                            name: '工资',
                            color: '#4572A7',
                            type: 'column',
                            data: salaryarry,
                        }, {
                            name: '奖金',
                            color: '#f10000',
                            type: 'column',
                            yAxis: 1,
                            data: bonusarry,
                        }]
                    });
                }
            }
        });
    }
    /*----end-----*/
    /*--------促销员销售数据图表----------------------*/
    function split(val) {
        return val.split(/,\s*/);
    };
    function extractLast(term) {
        return split(term).pop();
    };
    $("#offstatistic-seller").autocomplete({
        minLength: 0,
        max: 5,//列表里的条目数
        minChars: 0,//自动完成激活之前填入的最小字符
        width: 200,//提示的宽度，溢出隐藏
        scrollHeight: 180,//提示的高度，溢出显示滚动条
        matchContains: true,//包含匹配，就是data参数里的数据，是否只要包含文本框里的数据就显示
        autoFill: false,//自动填充
        source: function (request, response) {
            $.ajax({
                url: "/OffStatistic/Off_Statistic_QuerySeller_Ajax",
                type: "post",
                data: {
                    query: request.term
                },
                dataType: "json",
                success: function (data) {
                    response(data.data);
                }

            })
        },
        search: function () {
            var term = extractLast(this.value);
            if (term.length < 1) {
                return false;
            }
        },
        focus: function () {
            return false;
        },
        select: function (event, ui) {
            $("#offstatisric-seller-item").removeClass("hidden");
            $("#offstatisric-seller-item").val(ui.item.label + " ———— " + ui.item.desc);
            $("#offstatistic-seller").val(ui.item.label);
            $("#offstatistic-project").val(ui.item.value);
            return false;
        }
    });
    $(".offstatistic-seller-btn").click(function () {
        var start = $("#start-date").val();
        var end = $("#end-date").val();
        var sellerid = $("#offstatistic-project").val();
        var link_url = "/OffStatistic/SellerStatisticAjax";
        if (start > end) {
            $("#danger").text("开始时间不能大于结束时间")
        } else if (start == "" || end == "") {
            $("#danger").text("时间不能为空")
        } else {
            $("#danger").addClass("hidden")
            $.ajax({
                url: link_url,
                type: "post",
                data: {
                    startdate: start,
                    enddate: end,
                    sellerid: sellerid
                },
                success: function (data) {
                    if (data.data.length == 0) {
                        alert("该促销员在此时间内无数据", "", function () {
                        }, { type: 'error' });
                    } else {
                        $("#map").show();
                        var resultdata = data.data;
                        var totalarray = new Array();
                        var datearray = new Array();
                        var evatotalarry = new Array();
                        for (var i = 0; i < resultdata.length; i++) {
                            var deeint = parseInt(resultdata[i].Date.replace(/\D/igm, ""));
                            var dee = new Date(deeint)
                            datearray.push(dee.Format("MM-dd"));
                            var totaltemp = resultdata[i].SalesCount;
                            totalarray.push(totaltemp);
                            var avgtotaltemp = resultdata[i].AVG_SalesData;
                            evatotalarry.push(avgtotaltemp)
                        }
                        $("#myChart_Bar").highcharts({
                            chart: {
                            },
                            title: {
                                text: '促销员每天销售总额折线图(单位：盒/天)'
                            },
                            xAxis: {
                                categories: datearray
                            },
                            series: [{
                                type: 'column',
                                name: '每日销量',
                                data: totalarray
                            }, {
                                type: 'spline',
                                name: '历史平均销量',
                                data: evatotalarry,
                                marker: {
                                    lineWidth: 2,
                                    lineColor: Highcharts.getOptions().colors[3],
                                    fillColor: 'white'
                                }
                            }]
                        });
                    }
                }
            })
        }
    });
    /*----end-----*/
    /*---------------区域门店销售数据----------------------*/
    $(".offstatistic-store-btn").click(function () {
        var start = $("#start-date").val();
        var end = $("#end-date").val();
        var storeid = $("#StoreList").val();
        var type = $(this).attr("data-salary");
        var selectvalue = $("#StoreList").val() + "";
        var managerArray = selectvalue.split(',');
        for (var i = 0; i < managerArray.length; i++) {
            var optionitem = $("option[value='" + managerArray[i] + "']").text();
            $("#storeName").val(optionitem);
        }
        if (start > end) {
            $("#danger").text("开始时间不能大于结束时间")
        } else if (start == "" || end == "") {
            $("#danger").text("时间不能为空")
        } else {
            $("#danger").addClass("hidden")
            $.ajax({
                url: "/OffStatistic/StoreStatisticAjax",
                type: "post",
                data: {
                    startdate: start,
                    enddate: end,
                    storeid: storeid,
                    type: type
                },
                success: function (data) {
                    if (data.data.length == 0) {
                        alert("该区域门店下的这段时间内没有销售数据", "", function () {
                        }, { type: 'error' });
                    } else {
                        $("#map").show();
                        var resultdata = data.data;
                        var totalarray = new Array();//销售总额
                        var datearray = new Array();//显示时间
                        for (var i = 0; i < data.data.length; i++) {
                            var deeint = parseInt(data.data[i].Date.replace(/\D/igm, ""));
                            var dee = new Date(deeint)
                            datearray.push(dee.Format("MM-dd"));
                            var totaltemp = resultdata[i].SalesCount//每天销售总额
                            totalarray.push(totaltemp);
                        };
                        $("#myChart_Bar").highcharts({
                            chart: {
                            },
                            title: {
                                text: optionitem + '每天销售总额柱状图(单位：盒/天)'
                            },
                            xAxis: {
                                categories: datearray
                            },
                            series: [{
                                type: 'column',
                                name: '销售额度',
                                data: totalarray
                            }]
                        });
                        statistic_store_products(start, end, storeid, type)
                        statistic_store_salary(start, end, storeid, type)
                    }
                }
            })
        }
    });
    function statistic_store_products(start, end, storeid, type) {
        var selectvalue = $("#StoreList").val() + "";
        var managerArray = selectvalue.split(',');
        for (var i = 0; i < managerArray.length; i++) {
            var optionitem = $("option[value='" + managerArray[i] + "']").text();
            $("#storeName").val(optionitem);
        }
        $.ajax({
            url: "/OffStatistic/StoreProductStatisticAjax",
            type: "post",
            data: {
                startdate: start,
                enddate: end,
                storeid: storeid,
                type: type
            },
            success: function (data) {
                var productarry = new Array();
                for (var i = 0; i < data.data.length; i++) {
                    var productlist = {
                        name: data.data[i].SimpleName,
                        y: data.data[i].SalesCount
                    }
                    productarry.push(productlist)
                }
                $('#myChart_Pie').highcharts({
                    chart: {
                        plotBackgroundColor: null,
                        plotBorderWidth: null,
                        plotShadow: false
                    },
                    title: {
                        text: optionitem + '每天各个种类销售总额饼状图(单位：盒/天)'
                    },
                    tooltip: {
                        pointFormat: '{series.name}: <b>{point.percentage:.1f}%</b>'
                    },
                    plotOptions: {
                        pie: {
                            allowPointSelect: true,
                            cursor: 'pointer',
                            dataLabels: {
                                enabled: false
                            },
                            showInLegend: true
                        }
                    },
                    series: [{
                        type: 'pie',
                        name: '销售额度',
                        data: productarry
                    }]
                });
            }
        })
    }
    function statistic_store_salary(start, end, storeid, type) {
        var selectvalue = $("#StoreList").val() + "";
        var managerArray = selectvalue.split(',');
        for (var i = 0; i < managerArray.length; i++) {
            var optionitem = $("option[value='" + managerArray[i] + "']").text();
            $("#storeName").val(optionitem);
        }
        $.ajax({
            url: "/OffStatistic/StoreSalaryStatisticAjax",
            type: "post",
            data: {
                startdate: start,
                enddate: end,
                storeid: storeid,
                type: type
            },
            success: function (data) {
                if (data.data.length == 0) {
                    alert("该区域门店下的这段时间内没有销售数据", "", function () {
                    }, { type: 'error' });
                } else {
                    var datearray = new Array();
                    var salaryarry = new Array();
                    var debitarry = new Array();
                    var bonusarry = new Array();
                    for (var i = 0; i < data.data.length; i++) {
                        var deeint = parseInt(data.data[i].Date.replace(/\D/igm, ""));
                        var dee = new Date(deeint)
                        datearray.push(dee.Format("MM-dd"));
                        var totalsalary = data.data[i].Salary
                        salaryarry.push(totalsalary);
                        var totaldebit = Math.abs(data.data[i].Debit)
                        debitarry.push(totaldebit);
                        var totalbonus = data.data[i].Bonus
                        bonusarry.push(totalbonus);
                    }
                    $('#myChart_Column').highcharts({
                        chart: {
                            zoomType: 'xy'
                        },
                        title: {
                            text: optionitem + '工资表'
                        },
                        xAxis: [{
                            categories: datearray
                        }],
                        yAxis: [{ // Primary yAxis
                            labels: {
                                enabled: false
                            },
                            title: {
                                text: '',
                            }
                        }, { // Secondary yAxis
                            title: {
                                text: ''
                            },
                            labels: {
                                enabled: false
                            },
                        }, { // Secondary yAxis
                            title: {
                                text: '',
                            },
                            labels: {
                                enabled: false
                            },
                        }],
                        tooltip: {
                            shared: true
                        },

                        series: [{
                            name: '扣款',
                            color: '#89A54E',
                            type: 'column',
                            yAxis: 2,
                            data: debitarry,
                        }, {
                            name: '工资',
                            color: '#4572A7',
                            type: 'column',
                            data: salaryarry,
                        }, {
                            name: '奖金',
                            color: '#f10000',
                            type: 'column',
                            yAxis: 1,
                            data: bonusarry,
                        }]
                    });
                }
            }
        })
    }
    /*----end-----*/
    /*------活动记录表--------*/
    
    /*----end------*/
    
});
/*---删除按钮---*/
function getDeleteResult(data) {
    if (data.result == "SUCCESS") {
        alert("操作成功");
        return true;
    }
    else if (data.result == "FAIL") {
        alert("错误，无法完成操作", "", function () {
        }, { type: 'error' });
    }
    else if (data.result = "UNAUTHORIZED") {
        alert("没有权限，无法完成操作", "", function () {
        }, { type: 'error' });
    }
    return false;
}
/*---end---*/
/******dialog*****/
if (typeof $ === 'function') {
    $(function () {
        var BeAlert = {
            defaultConfig: {
                width: 320,
                height: 170,
                timer: 0,
                type: 'success',
                showConfirmButton: true,
                showCancelButton: false,
                confirmButtonText: '确认',
                cancelButtonText: '取消'
            },
            html: '<div class="BeAlert_box">' +
            '<div class="BeAlert_image"></div>' +
            '<div class="BeAlert_title"></div>' +
            '<div class="BeAlert_message"></div>' +
            '<div class="BeAlert_button">' +
            '<button class="BeAlert_confirm"></button>' +
            '<button class="BeAlert_cancel"></button>' +
            '</div>' +
            '</div>',
            overlay: '<div class="BeAlert_overlay"></div>',
            open: function (title, message, callback, o) {
                var opts = {}, that = this;
                $.extend(opts, that.defaultConfig, o);
                $('body').append(that.html).append(that.overlay);
                var box = $('.BeAlert_box');
                box.css({
                    'width': opts.width + 'px',
                    'min-height': opts.height + 'px',
                    'margin-left': -(opts.width / 2) + 'px'
                });
                $('.BeAlert_image').addClass(opts.type);
                title && $('.BeAlert_title').html(title).show(),
                message && $('.BeAlert_message').html(message).show();
                var confirmBtn = $('.BeAlert_confirm'), cancelBtn = $('.BeAlert_cancel');
                opts.showConfirmButton && confirmBtn.text(opts.confirmButtonText).show(),
                opts.showCancelButton && cancelBtn.text(opts.cancelButtonText).show();
                $('.BeAlert_overlay').unbind('click').bind('click', function () {
                    that.close();
                });
                confirmBtn.unbind('click').bind('click', function () {              
                    that.close();
                    typeof callback === 'function' && callback(true);
                });
                cancelBtn.unbind('click').bind('click', function () {
                    that.close();
                    typeof callback === 'function' && callback(false);
                });
                var h = box.height();
                box.css({
                    'margin-top': -(Math.max(h, opts.height) / 2 + 100) + 'px'
                });
            },
            close: function () {              
                $(".BeAlert_overlay,.BeAlert_box").remove();
            }
        };
        window.alert = function (title, message, callback, opts) {
            BeAlert.open(title, message, callback, opts);
        };
        var _confirm = window.confirm;
        window.confirm = function (title, message, callback, opts) {
            opts = $.extend({ type: 'question', showCancelButton: true }, opts);
            if (typeof callback === 'function') {
                BeAlert.open(title, message, callback, opts);
            } else {
                return _confirm(title);
            }
        }
    });
}
/*结束dialog*/


/*******页面按钮*******/
function PageBtn(obj) {
    this.obj = obj;
    this.init = init;
    this.initAjax = initAjax;
}
function initAjax() {
    var partialBox = this.obj.init.partialBox
    var _url = this.obj.init.url
    $.ajax({
        url: _url,
        success: function (data) {
            $(partialBox).html(data);
        }
    })
}
function init() {
    this.initAjax();
    var partialBox = this.obj.init.partialBox
    for (var item in this.obj) {
        if (item == "search") {
            var __url = this.obj[item].url;
            var _querybox = this.obj[item].query
            $(this.obj[item].btn).on("click", function () {
                var _query = $(_querybox).val()
                    $.ajax({
                        url: __url,
                        data: {
                            query: _query,
                        },
                        success: function (data) {
                            $(partialBox).html(data);
                        }
                    });
 
            })
        } else {
            var _url = this.obj[item].url;
            var _head = this.obj[item].head;
            $(this.obj[item].btn).on("click", function () {
                $.ajax({
                    url: _url,
                    data: {
                        random: Date.now()
                    },
                    success: function (data) {
                        $(".modal-con").html(data);
                        $(".modal-box").modal();
                        $(".modal-head").text(_head)
                    }
                });

            })
        }
    }
}
/*结束页面按钮*/


/********图片上传********/
if (typeof $ === 'function') {
    function UpLoadImg(obj) {
        if (typeof (obj) != "object") {

        }
    }
} else {
    throw new error(msg)
}
/*图片上传结束*/






