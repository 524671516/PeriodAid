$(function () {
    /*----时间表汉化----*/
    $.datepicker.regional["zh-CN"] = { closeText: "关闭", prevText: "&#x3c;上月", nextText: "下月&#x3e;", currentText: "今天", monthNames: ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"], monthNamesShort: ["一", "二", "三", "四", "五", "六", "七", "八", "九", "十", "十一", "十二"], dayNames: ["星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六"], dayNamesShort: ["周日", "周一", "周二", "周三", "周四", "周五", "周六"], dayNamesMin: ["日", "一", "二", "三", "四", "五", "六"], weekHeader: "周", dateFormat: "yy-mm-dd", firstDay: 1, isRTL: !1, showMonthAfterYear: !0, yearSuffix: "年" }
    $.datepicker.setDefaults($.datepicker.regional["zh-CN"]);
    /*------end--------*/
    $("#start-date").datepicker();
    $("#end-date").datepicker();
    $(".date-time").datepicker();
    /*start downloadsalary*/
    $("#download-salary").click(function () {
        $("#downloadModal").modal();
    });
    $("#download-submit").click(function () {
        var start = $("#start-date").val();
        var end = $("#end-date").val();
        window.location.href = "/OfflineSales/Ajax_DownloadSalary?start=" + start + "&end=" + end;
        return false;
    });
    /*-------end--------*/
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
    function bactchDelete(url, link_url,$list) {
        // 判断是否至少选择一项
        var checkedNum = $("input[name='subChk']:checked").length;
        if (checkedNum == 0) {
            alert("请选择至少一项！");
            return;
        }
        // 批量选择
        if (confirm("确定要删除所选项目？")) {
            var checkedList = new Array();
            $("input[name='subChk']:checked").each(function () {
                checkedList.push($(this).val());
            });
            $.ajax({
                type: "post",
                url: url,
                data: {
                    ids: checkedList.toString()
                },
                success: function (data) {
                    if (data == "SUCCESS") {
                        $.ajax({
                            url: link_url,
                            success: function (data) {
                                $list.html(data)
                            }
                        })
                    }
                }
            });
        }
    }
    /*-----end------*/
});