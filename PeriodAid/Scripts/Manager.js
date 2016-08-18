var $$ = Dom7;

// Initialize app
var myApp = new Framework7();

// If we need to use custom DOM library, let's save it to $$ variable:
// Add view
var mainView = myApp.addView(".view-main", {
    // Because we want to use dynamic navbar, we need to enable it for this view:
    dynamicNavbar: true
});

$$(document).on("ajaxStart", function (e) {
    if (e.detail.xhr.requestUrl.indexOf("autocomplete-languages.json") >= 0) {
        return;
    }
    myApp.showIndicator();
});

$$(document).on("ajaxComplete", function (e) {
    if (e.detail.xhr.requestUrl.indexOf("autocomplete-languages.json") >= 0) {
        return;
    }
    myApp.hideIndicator();
});

var monthNames = ["一月份", "二月份", "三月份", "四月份", "五月份", "六月份", "七月份", "八月份", "九月份", "十月份", "十一月份", "十二月份"];

var monthNamesShort = ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"];

var dayNames = ["星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六"];

var dayNames = ["星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六"];

var dayNamesShort = ["日", "一", "二", "三", "四", "五", "六"];

//tab-list 底部工具栏转换
$$(".tab-link").on("click", function (data) {
    if (!$$(this).hasClass("active")) {
        var url = $$(this).attr("data-href");
        mainView.router.load({
            url: url,
            animatePages: false
        });
        $(this).addClass("active").siblings().removeClass("active");
    }
});

refresh_userpanel();

refresh_home();

// Pull to refresh content
var ptrContent = $$("#home-refresh");

// Add 'refresh' listener on it
ptrContent.on("refresh", function (e) {
    // Emulate 2s loading
    setTimeout(refresh_home, 500);
});

// 微信初始化
wx.config({
    debug: false,
    // 开启调试模式,调用的所有api的返回值会在客户端alert出来，若要查看传入的参数，可以在pc端打开，参数信息会通过log打出，仅在pc端时才会打印。
    appId: $("#appId").text(),
    // 必填，公众号的唯一标识
    timestamp: $("#timeStamp").text(),
    // 必填，生成签名的时间戳
    nonceStr: $("#nonce").text(),
    // 必填，生成签名的随机串
    signature: $("#signature").text(),
    // 必填，签名，见附录1
    jsApiList: ["uploadImage", "downloadImage", "chooseImage", "getLocation", "previewImage", "openLocation"]
});

// 防止缓存机制
$$(document).on("touchstart", "a.random-param", function () {
    var url = $$(this).attr("href") + "&rand=" + Math.random() * 500;
    $$(this).attr("href", url);
});

// 使用指南
$$("#manager_userpanel").on("click", ".manager-photolist", function () {
    var photo = [{
        url: "/Content/images/Manager_app_01.jpg",
        caption: "督导签到 - - 首页"
    }, {
        url: "/Content/images/Manager_app_03.jpg",
        caption: "督导签到 - - 首页"
    }, {
        url: "/Content/images/Manager_app_02.jpg",
        caption: "督导签到 - - 督导个人信息"
    }, {
        url: "/Content/images/Manager_app_04.jpg",
        caption: "督导签到 - - 督导每日签到"
    }, {
        url: "/Content/images/Manager_app_05.jpg",
        caption: "督导签到 - - 督导工作日报"
    }, {
        url: "/Content/images/Manager_app_06.jpg",
        caption: "督导签到 - - 查看个人签到信息"
    }, {
        url: "/Content/images/Manager_app_07.jpg",
        caption: "督导签到 - - 查看其他督导签到"
    }, {
        url: "/Content/images/Manager_app_08.jpg",
        caption: "督导签到 - - 店铺提报列表"
    }, {
        url: "/Content/images/Manager_app_09.jpg",
        caption: "督导巡店 - - 首页"
    }, {
        url: "/Content/images/Manager_app_10.jpg",
        caption: "督导巡店 - - 未签到"
    }, {
        url: "/Content/images/Manager_app_11.jpg",
        caption: "督导巡店 - - 未签退"
    }, {
        url: "/Content/images/Manager_app_12.jpg",
        caption: "督导巡店 - - 修改签到信息"
    }, {
        url: "/Content/images/Manager_app_13.jpg",
        caption: "督导管理 - - 首页"
    }, {
        url: "/Content/images/Manager_app_14.jpg",
        caption: "督导管理 - - 销量排名"
    }, {
        url: "/Content/images/Manager_app_15.jpg",
        caption: "督导管理 - - 红包发放"
    }, {
        url: "/Content/images/Manager_app_16.jpg",
        caption: "督导管理 - - 红包记录"
    }, {
        url: "/Content/images/Manager_app_17.jpg",
        caption: "督导暗促 - - 首页"
    }];
    var myPhotoBrowserPopupDark = myApp.photoBrowser({
        photos: photo,
        theme: "dark",
        type: "standalone",
        lazyLoading: true,
        zoom: false,
        backLinkText: "关闭"
    });
    myPhotoBrowserPopupDark.open();
});

/*************** 督导签到 *************/
$$(document).on("pageInit", ".page[data-page='manager-task']", function (e) {
    $$.ajax({
        url: "/Seller/Manager_RefreshTaskCount",
        method: "post",
        success: function (data) {
            data = JSON.parse(data);
            if (data.result = "SUCCESS") {
                $$("#managertask-count").text(data.data);
            }
        }
    });
});

//Manager_Addchekin 添加签到信息 填写备注信息字数提示
$$(document).on("pageInit", ".page[data-page='manager-task-addcheckin']", function (e) {
    // 获取当前备注文本长度
    currentTextAreaLength("manager-task-addcheckin", "Remark", 50, "checkin-currentlength");
    // 显示所有的已上传图片
    uploadCheckinFile("manager-task-addcheckin", "manager-imglist", "Photo", "current_image", 3);
    uploadLocationWithDetails("location-btn", "Location", "Location_Desc");
    $("#addcheckin_form").validate({
        debug: false,
        //调试模式取消submit的默认提交功能   
        errorClass: "custom-error",
        //默认为错误的样式类为：error   
        focusInvalid: false,
        //当为false时，验证无效时，没有焦点响应  
        onkeyup: false,
        submitHandler: function (form) {
            var array = splitArray($("#Photo").val());
            if (array.length == 0) {
                myApp.hideIndicator();
                myApp.alert("请至少上传一张图片");
                $("#addcheckin-btn").prop("disabled", false).removeClass("color-gray");
            } else if ($("#Location").val().trim() == "") {
                myApp.hideIndicator();
                myApp.alert("请上传您的地理位置");
                $("#addcheckin-btn").prop("disabled", false).removeClass("color-gray");
            } else {
                $("#addcheckin_form").ajaxSubmit(function (data) {
                    if (data == "SUCCESS") {
                        myApp.hideIndicator();
                        mainView.router.back();
                        myApp.addNotification({
                            title: "通知",
                            message: "表单提交成功"
                        });
                        setTimeout(function () {
                            myApp.closeNotification(".notifications");
                        }, 2e3);
                    } else {
                        myApp.hideIndicator();
                        myApp.addNotification({
                            title: "通知",
                            message: "表单提交失败"
                        });
                        $("#addcheckin-btn").prop("disabled", false).removeClass("color-gray");
                        setTimeout(function () {
                            myApp.closeNotification(".notifications");
                        }, 2e3);
                    }
                });
            }
        },
        errorPlacement: function (error, element) {
            myApp.hideIndicator();
            $("#addcheckin-btn").prop("disabled", false).removeClass("color-gray");
            element.attr("placeholder", error.text());
        }
    });
    $$("#addcheckin-btn").click(function () {
        myApp.showIndicator();
        $("#addcheckin-btn").prop("disabled", true).addClass("color-gray");
        setTimeout(function () {
            $("#addcheckin_form").submit();
        }, 500);
    });
});

myApp.onPageBack("manager-task-addcheckin", function () {
    $$.ajax({
        url: "/Seller/Manager_RefreshTaskCount",
        method: "post",
        success: function (data) {
            data = JSON.parse(data);
            if (data.result = "SUCCESS") {
                $$("#managertask-count").text(data.data);
            }
        }
    });
});

//Manager_TaskReport 督导工作日报 填写内容字数提示
$$(document).on("pageInit", ".page[data-page='manager-task-report']", function () {
    $$.ajax({
        url: "/Seller/Manager_TaskReportPartial",
        data: {
            id: $$("#taskreport-date").val()
        },
        success: function (data) {
            $("#task_details").html(data);
            currentTextAreaLength("manager-task-report", "Event_Complete", 500, "tasklength-cp");
            currentTextAreaLength("manager-task-report", "Event_UnComplete", 500, "tasklength-uc");
            currentTextAreaLength("manager-task-report", "Event_Assistance", 500, "tasklength-as");
            uploadCheckinFile("manager-task-report", "manager-imglist", "Photo", "current_image", 7);
        }
    });
    $$("#taskreport-date").on("change", function () {
        $$.ajax({
            url: "/Seller/Manager_TaskReportPartial",
            data: {
                id: $$("#taskreport-date").val()
            },
            success: function (data) {
                $("#task_details").html(data);
                currentTextAreaLength("manager-task-report", "Event_Complete", 500, "tasklength-cp");
                currentTextAreaLength("manager-task-report", "Event_UnComplete", 500, "tasklength-uc");
                currentTextAreaLength("manager-task-report", "Event_Assistance", 500, "tasklength-as");
                uploadCheckinFile("manager-task-report", "manager-imglist", "Photo", "current_image", 7);
            }
        });
    });
    $$("#task_details").on("click", "#report-submit-btn", function () {
        $("#report-submit-btn").prop("disabled", true).addClass("color-gray");
        myApp.showIndicator();
        setTimeout(function () {
            myApp.hideIndicator();
        }, 5e3);
        setTimeout(function () {
            //$$("#managerreport-form").submit();
            $("#managerreport-form").ajaxSubmit({
                success: function (data) {
                    if (data == "SUCCESS") {
                        myApp.hideIndicator();
                        mainView.router.back();
                        myApp.addNotification({
                            title: "通知",
                            message: "日报修改成功"
                        });
                        setTimeout(function () {
                            //refresh_mainpanel();
                            myApp.closeNotification(".notifications");
                        }, 2e3);
                    } else {
                        myApp.hideIndicator();
                        $("#report-submit-btn").prop("disabled", true).addClass("color-gray");
                        myApp.addNotification({
                            title: "通知",
                            message: "日报修改失败"
                        });
                        $("#task_details").html(data);
                        setTimeout(function () {
                            myApp.closeNotification(".notifications");
                        }, 2e3);
                    }
                }
            });
        }, 500);
    });
});

//Manager_CheckInView 查看签到信息
$$(document).on("pageInit", ".page[data-page='manager-checkinview']", function () {
    $$.ajax({
        url: "/Seller/Manager_CheckInViewPartial",
        data: {
            id: $$(".check-date").val()
        },
        success: function (data) {
            $$(".list-content").html(data);
        }
    });
    $$(".check-date").on("change", function () {
        $$.ajax({
            url: "/Seller/Manager_CheckInViewPartial",
            data: {
                id: $$(".check-date").val()
            },
            success: function (data) {
                $$(".list-content").html(data);
            }
        });
    });
    $$(".list-content").on("deleted", ".swipeout", function (e) {
        var url = "/Seller/Mananger_CancelManagerCheckin";
        var Id = $$(e.target).attr("data-url");
        swipe_deleted(url, Id);
    });
    PhotoBrowser("manager-checkinview-content");
});

myApp.onPageBack("manager-checkinview", function (e) {
    $$.ajax({
        url: "/Seller/Manager_RefreshTaskCount",
        method: "post",
        success: function (data) {
            data = JSON.parse(data);
            if (data.result = "SUCCESS") {
                $$("#managertask-count").text(data.data);
            }
        }
    });
});

//Senior_AllCheckInList 查看其他人签到
$$(document).on("pageInit", ".page[data-page='manager-allchekinlist']", function () {
    var date = $$("#task_id").val();
    $$.ajax({
        url: "/Seller/Senior_AllCheckInListPartial",
        data: {
            date: date
        },
        success: function (data) {
            $$("#allcheckinlist-content").html(data);
        }
    });
    $$("#task_id").on("change", function () {
        var date = $$("#task_id").val();
        $$.ajax({
            url: "/Seller/Senior_AllCheckInListPartial",
            data: {
                date: date
            },
            success: function (data) {
                $$("#allcheckinlist-content").html(data);
            }
        });
    });
});

// Manager_RequestListPartial 需求列表
$$(document).on("pageInit", ".page[data-page='manager-request-list']", function () {
    $$.ajax({
        url: "/Seller/Manager_RequestListPartial",
        success: function (data) {
            $$("#manager-requestlist").html(data);
        }
    });
    $$("#manager-requestlist").on("deleted", ".swipeout", function (e) {
        $$.ajax({
            url: "Manager_CancelRequestJson",
            data: {
                id: $$(e.target).attr("data-url")
            },
            method: "post",
            success: function (data) {
                data = JSON.parse(data);
                if (data.result != "SUCCESS") {
                    myApp.alert("删除失败");
                }
            }
        });
    });
});

// Manager_RequestCreate 创建需求信息
$$(document).on("pageInit", ".page[data-page='manager-task-requestcreate']", function () {
    currentTextAreaLength("manager-task-requestcreate", "RequestContent", 500, "requestcontent_length");
    $("#requestcreate-form").validate({
        debug: false,
        //调试模式取消submit的默认提交功能   
        errorClass: "custom-error",
        //默认为错误的样式类为：error   
        focusInvalid: false,
        //当为false时，验证无效时，没有焦点响应
        onkeyup: false,
        submitHandler: function (form) {
            $("#requestcreate-form").ajaxSubmit(function (data) {
                if (data == "SUCCESS") {
                    myApp.hideIndicator();
                    mainView.router.back();
                    myApp.addNotification({
                        title: "通知",
                        message: "表单提交成功"
                    });
                    setTimeout(function () {
                        myApp.closeNotification(".notifications");
                    }, 2e3);
                } else {
                    myApp.hideIndicator();
                    myApp.addNotification({
                        title: "通知",
                        message: "表单提交失败"
                    });
                    $("#requestcreate-btn").prop("disabled", false).removeClass("color-gray");
                    setTimeout(function () {
                        myApp.closeNotification(".notifications");
                    }, 2e3);
                }
            });
        },
        rules: {
            RequestContent: {
                required: true,
                maxlength: 500
            },
            RequestRemark: {
                required: true,
                maxlength: 20
            }
        },
        messages: {
            RequestContent: {
                required: "字段不能为空",
                maxlength: jQuery.format("不能小于{0}个字符")
            },
            RequestRemark: {
                required: "字段不能为空",
                maxlength: jQuery.format("不能小于{0}个字符")
            }
        },
        errorPlacement: function (error, element) {
            myApp.hideIndicator();
            $("#requestcreate-btn").prop("disabled", false).removeClass("color-gray");
            element.attr("placeholder", error.text());
        }
    });
    $$("#requestcreate-btn").click(function () {
        myApp.showIndicator();
        $("#requestcreate-btn").prop("disabled", true).addClass("color-gray");
        setTimeout(function () {
            $("#requestcreate-form").submit();
        }, 500);
    });
});

// Mananger_RequestEdit 修改需求信息
$$(document).on("pageInit", ".page[data-page='manager-task-requestedit']", function () {
    currentTextAreaLength("manager-task-requestedit", "RequestContent", 500, "requestcontent_length");
    $("#requestedit-form").validate({
        debug: false,
        //调试模式取消submit的默认提交功能   
        errorClass: "custom-error",
        //默认为错误的样式类为：error   
        focusInvalid: false,
        //当为false时，验证无效时，没有焦点响应
        onkeyup: false,
        submitHandler: function (form) {
            $("#requestedit-form").ajaxSubmit(function (data) {
                if (data == "SUCCESS") {
                    myApp.hideIndicator();
                    mainView.router.back();
                    myApp.addNotification({
                        title: "通知",
                        message: "表单提交成功"
                    });
                    setTimeout(function () {
                        myApp.closeNotification(".notifications");
                    }, 2e3);
                } else {
                    myApp.hideIndicator();
                    myApp.addNotification({
                        title: "通知",
                        message: "表单提交失败"
                    });
                    $("#requestedit-btn").prop("disabled", false).removeClass("color-gray").show();
                    setTimeout(function () {
                        myApp.closeNotification(".notifications");
                    }, 2e3);
                }
            });
        },
        rules: {
            RequestContent: {
                required: true,
                maxlength: 500
            },
            RequestRemark: {
                required: true,
                maxlength: 20
            }
        },
        messages: {
            RequestContent: {
                required: "字段不能为空",
                maxlength: jQuery.format("不能小于{0}个字符")
            },
            RequestRemark: {
                required: "字段不能为空",
                maxlength: jQuery.format("不能小于{0}个字符")
            }
        },
        errorPlacement: function (error, element) {
            myApp.hideIndicator();
            $("#requestedit-btn").prop("disabled", false).removeClass("color-gray").show();
            element.attr("placeholder", error.text());
        }
    });
    $$("#requestedit-btn").click(function () {
        myApp.showIndicator();
        $("#requestedit-btn").prop("disabled", true).addClass("color-gray");
        setTimeout(function () {
            $("#requestedit-form").submit();
        }, 500);
    });
});

//Senior_CheckInDetails  查看其他人签到信息  图片查看
$$(document).on("pageInit", ".page[data-page='manager-checkindetails']", function () {
    PhotoBrowser("manager-checkindetails");
    LocationBrowser("manager-checkindetails");
});

/*************** 店铺查询 *************/
$$(document).on("pageInit", ".page[data-page='manager-home']", function () {
    refresh_home();
    var ptrContent = $$("#home-refresh");
    // Add 'refresh' listener on it
    ptrContent.on("refresh", function (e) {
        // Emulate 2s loading
        setTimeout(refresh_home, 500);
    });
});

//Manager_UnCheckInList  巡店 未签到
$$(document).on("pageAfterAnimation", ".page[data-page='manager-uncheckinlist']", function () {
    var url = "/Seller/Manager_UnCheckInListPartial";
    datepicker_refresh(url);
});

myApp.onPageBack("manager-uncheckinlist", function (e) {
    var ptrContent = $$("#home-refresh");
    myApp.pullToRefreshTrigger(ptrContent);
});

//Manager_UnCheckOutList 巡店 未签退
$$(document).on("pageAfterAnimation", ".page[data-page='manager-uncheckoutlist']", function () {
    var url = "/Seller/Manager_UnCheckOutListPartial";
    datepicker_refresh(url);
    $$(".list-content").on("deleted", ".swipeout", function (e) {
        var url = "/Seller/Manager_DeleteCheckIn";
        var Id = $$(e.target).attr("data-url");
        swipe_deleted(url, Id);
    });
});

myApp.onPageBack("manager-uncheckoutlist", function (e) {
    var ptrContent = $$("#home-refresh");
    myApp.pullToRefreshTrigger(ptrContent);
});

//Manager_UnReportList 巡店 未提报销量
$$(document).on("pageAfterAnimation", ".page[data-page='manager-unreportlist']", function () {
    var url = "/Seller/Manager_UnReportListPartial";
    datepicker_refresh(url);
    $$(".list-content").on("deleted", ".swipeout", function (e) {
        var url = "/Seller/Manager_DeleteCheckIn";
        var Id = $$(e.target).attr("data-url");
        swipe_deleted(url, Id);
    });
});

myApp.onPageBack("manager-unreportlist", function (e) {
    var ptrContent = $$("#home-refresh");
    myApp.pullToRefreshTrigger(ptrContent);
});

//Manager_UnConfirmList 巡店 销量待确认
$$(document).on("pageAfterAnimation", ".page[data-page='manager-unconfirmlist']", function () {
    var url = "/Seller/Manager_UnConfirmListPartial";
    $$.ajax({
        url: "/Seller/Manager_UnConfirmListPartial",
        success: function (data) {
            $(".list-content").html(data);
        }
    });
    $$(".list-content").on("deleted", ".swipeout", function (e) {
        var url = "/Seller/Manager_DeleteCheckIn";
        var Id = $$(e.target).attr("data-url");
        swipe_deleted(url, Id);
    });
});

myApp.onPageBack("manager-unconfirmlist", function (e) {
    var ptrContent = $$("#home-refresh");
    myApp.pullToRefreshTrigger(ptrContent);
});

//Manager_CreateCheckIn 代提报销量  填写备注信息字数提示
$$(document).on("pageInit", ".page[data-page='manager-createcheckin']", function () {
    uploadCheckinFile("checkinphoto-area", "manager-checkin-imglist", "CheckinPhoto", "checkin-current-image", 1);
    uploadCheckinFile("checkoutphoto-area", "manager-checkout-imglist", "CheckoutPhoto", "checkout-current-image", 1);
    uploadCheckinFile("reportphoto-area", "manager-report-imglist", "Rep_Image", "report-current-image", 7);
    currentTextAreaLength("confirmremark-area", "Confirm_Remark", 500, "confirmremark-length");
    $("#createcheckin-form").validate({
        debug: false,
        //调试模式取消submit的默认提交功能   
        errorClass: "custom-error",
        //默认为错误的样式类为：error   
        focusInvalid: false,
        //当为false时，验证无效时，没有焦点响应
        onkeyup: false,
        submitHandler: function (form) {
            var array1 = splitArray($("#CheckinPhoto").val());
            var array2 = splitArray($("#CheckoutPhoto").val());
            var array3 = splitArray($("#Rep_Image").val());
            if ($$("#Off_Seller_Id").val().trim() == "") {
                myApp.hideIndicator();
                myApp.alert("请选择促销员");
                $("#createcheckin-btn").prop("disabled", false).removeClass("color-gray");
            } else if (array3.length == 0) {
                myApp.hideIndicator();
                myApp.alert("请至少上传一张销量图片");
                $("#createcheckin-btn").prop("disabled", false).removeClass("color-gray");
            } else {
                $("#createcheckin-form").ajaxSubmit(function (data) {
                    if (data == "SUCCESS") {
                        myApp.hideIndicator();
                        mainView.router.back();
                        myApp.addNotification({
                            title: "通知",
                            message: "表单提交成功"
                        });
                        setTimeout(function () {
                            myApp.closeNotification(".notifications");
                        }, 2e3);
                    } else {
                        myApp.hideIndicator();
                        myApp.addNotification({
                            title: "通知",
                            message: "表单提交失败"
                        });
                        $("#createcheckin-btn").prop("disabled", false).removeClass("color-gray");
                        setTimeout(function () {
                            myApp.closeNotification(".notifications");
                        }, 2e3);
                    }
                });
            }
        },
        rules: {
            Confirm_Remark: {
                required: true,
                maxlength: 500
            }
        },
        messages: {
            Confirm_Remark: {
                required: "字段不能为空",
                maxlength: jQuery.format("不能小于{0}个字符")
            }
        },
        errorPlacement: function (error, element) {
            myApp.hideIndicator();
            $("#createcheckin-btn").prop("disabled", false).removeClass("color-gray");
            element.attr("placeholder", error.text());
        }
    });
    $$("#createcheckin-btn").click(function () {
        myApp.showIndicator();
        $("#createcheckin-btn").prop("disabled", true).addClass("color-gray");
        setTimeout(function () {
            $("#createcheckin-form").submit();
        }, 500);
    });
});

// Manager_EditCheckin 修改签到信息
$$(document).on("pageInit", ".page[data-page='manager-editcheckin']", function () {
    uploadCheckinFile("checkinphoto-area", "manager-checkin-imglist", "CheckinPhoto", "checkin-current-image", 1);
    uploadCheckinFile("checkoutphoto-area", "manager-checkout-imglist", "CheckoutPhoto", "checkout-current-image", 1);
    uploadCheckinFile("reportphoto-area", "manager-report-imglist", "Rep_Image", "report-current-image", 7);
    currentTextAreaLength("confirmremark-area", "Confirm_Remark", 500, "confirmremark-length");
    $("#editcheckin-form").validate({
        debug: false,
        //调试模式取消submit的默认提交功能   
        errorClass: "custom-error",
        //默认为错误的样式类为：error   
        focusInvalid: false,
        //当为false时，验证无效时，没有焦点响应
        onkeyup: false,
        submitHandler: function (form) {
            $("#editcheckin-form").ajaxSubmit(function (data) {
                if (data == "SUCCESS") {
                    myApp.hideIndicator();
                    mainView.router.back();
                    myApp.addNotification({
                        title: "通知",
                        message: "表单提交成功"
                    });
                    setTimeout(function () {
                        myApp.closeNotification(".notifications");
                    }, 2e3);
                } else {
                    myApp.hideIndicator();
                    myApp.addNotification({
                        title: "通知",
                        message: "表单提交失败"
                    });
                    $("#editcheckin-btn").prop("disabled", false).removeClass("color-gray");
                    setTimeout(function () {
                        myApp.closeNotification(".notifications");
                    }, 2e3);
                }
            });
        },
        rules: {
            Confirm_Remark: {
                required: true,
                maxlength: 500
            }
        },
        messages: {
            Confirm_Remark: {
                required: "字段不能为空",
                maxlength: jQuery.format("不能小于{0}个字符")
            }
        },
        errorPlacement: function (error, element) {
            myApp.hideIndicator();
            $("#editcheckin-btn").prop("disabled", false).removeClass("color-gray");
            element.attr("placeholder", error.text());
        }
    });
    $$("#editcheckin-btn").click(function () {
        myApp.showIndicator();
        $("#editcheckin-btn").prop("disabled", true).addClass("color-gray");
        setTimeout(function () {
            $("#editcheckin-form").submit();
        }, 500);
    });
});

// Manager_CheckinConfirm 审核销量
$$(document).on("pageInit", ".page[data-page='manager-checkinconfirm']", function () {
    PhotoBrowser("checkin-photo-link");
    PhotoBrowser("checkout-photo-link");
    PhotoBrowser("report-photo-link");
    LocationBrowser("checkin-location");
    LocationBrowser("checkout-location");
    currentTextAreaLength("confirmremark-area", "Confirm_Remark", 500, "confirmremark-length");
    $("#checkinconfirm-form").validate({
        debug: false,
        //调试模式取消submit的默认提交功能   
        errorClass: "custom-error",
        //默认为错误的样式类为：error   
        focusInvalid: false,
        //当为false时，验证无效时，没有焦点响应
        onkeyup: false,
        submitHandler: function (form) {
            $("#checkinconfirm-form").ajaxSubmit(function (data) {
                if (data == "SUCCESS") {
                    myApp.hideIndicator();
                    mainView.router.back();
                    myApp.addNotification({
                        title: "通知",
                        message: "表单提交成功"
                    });
                    setTimeout(function () {
                        myApp.closeNotification(".notifications");
                    }, 2e3);
                } else {
                    myApp.hideIndicator();
                    myApp.addNotification({
                        title: "通知",
                        message: "表单提交失败"
                    });
                    $("#checkinconfirm-btn").prop("disabled", false).removeClass("color-gray");
                    setTimeout(function () {
                        myApp.closeNotification(".notifications");
                    }, 2e3);
                }
            });
        },
        rules: {
            Confirm_Remark: {
                required: true,
                maxlength: 500
            }
        },
        messages: {
            Confirm_Remark: {
                required: "字段不能为空",
                maxlength: jQuery.format("不能小于{0}个字符")
            }
        },
        errorPlacement: function (error, element) {
            myApp.hideIndicator();
            $("#checkinconfirm-btn").prop("disabled", false).removeClass("color-gray");
            element.attr("placeholder", error.text());
        }
    });
    $$("#checkinconfirm-btn").click(function () {
        myApp.showIndicator();
        $("#checkinconfirm-btn").prop("disabled", true).addClass("color-gray");
        setTimeout(function () {
            $("#checkinconfirm-form").submit();
        }, 500);
    });
});

//Manager_ViewConfirm 查看图片
$$(document).on("pageInit", ".page[data-page='manager-viewconfirm']", function () {
    PhotoBrowser("manager-viewconfirm");
    LocationBrowser("manager-viewconfirm");
});

/*************** 督导工具 *************/
//Manager_ReportList  销量排名 查看日期
$$(document).on("pageAfterAnimation", ".page[data-page='manager-reportlist']", function (e) {
    var calendarDefault = myApp.calendar({
        input: "#manager-reportlist-date",
        monthNames: monthNames,
        monthNamesShort: monthNamesShort,
        dayNames: dayNames,
        dayNamesShort: dayNamesShort,
        closeOnSelect: true
    });
    var date = $$("#manager-reportlist-date").val();
    var storesystem = $$("#manager-reportlist-storesystem").val();
    $$.ajax({
        url: "/Seller/Manager_ReportListPartial",
        data: {
            date: date,
            storesystem: storesystem
        },
        success: function (data) {
            $$("#manager-reportlist-content").html(data);
        }
    });
    $$("#manager-reportlist-date").on("change", function () {
        var date = $$("#manager-reportlist-date").val();
        var storesystem = $$("#manager-reportlist-storesystem").val();
        $$.ajax({
            url: "/Seller/Manager_ReportListPartial",
            data: {
                date: date,
                storesystem: storesystem
            },
            success: function (data) {
                $$("#manager-reportlist-content").html(data);
            }
        });
    });
    $$("#manager-reportlist-storesystem").on("change", function () {
        var date = $$("#manager-reportlist-date").val();
        var storesystem = $$("#manager-reportlist-storesystem").val();
        $$.ajax({
            url: "/Seller/Manager_ReportListPartial",
            data: {
                date: date,
                storesystem: storesystem
            },
            success: function (data) {
                $$("#manager-reportlist-content").html(data);
            }
        });
    });
});

//Manager_TaskSellerDetails 销量详情
$$(document).on("pageInit", ".page[data-page='manager-tasksellerdetails']", function () {
    //查看签到图片
    var myPhotoManagerSellerChekinIn = myApp.photoBrowser({
        photos: ["/Content/images/guide-02-3.jpg"],
        theme: "dark",
        type: "standalone",
        lazyLoading: true,
        zoom: false,
        backLinkText: "关闭"
    });
    var myPhotoManagerSellerChekinOut = myApp.photoBrowser({
        photos: ["/Content/images/guide-02-2.jpg"],
        theme: "dark",
        type: "standalone",
        lazyLoading: true,
        zoom: false,
        backLinkText: "关闭"
    });
    var myPhotoManagerSellerSales = myApp.photoBrowser({
        photos: ["/Content/images/guide-02-3.jpg"],
        theme: "dark",
        type: "standalone",
        lazyLoading: true,
        zoom: false,
        backLinkText: "关闭"
    });
    $$(".manager-tasksellerdetails-checkin").on("click", function () {
        myPhotoManagerSellerChekinIn.open();
    });
    $$(".manager-tasksellerdetails-checkout").on("click", function () {
        myPhotoManagerSellerChekinOut.open();
    });
    $$(".manager-tasksellerdetails-sales").on("click", function () {
        myPhotoManagerSellerSales.open();
    });
});

// Manager_CheckinRemark 添加红包信息
$$(document).on("pageInit", ".page[data-page='manager-bonusremark']", function () {
    currentTextAreaLength("manager-bonusremark", "Bonus_Remark", 100, "confirmremark_length");
    $("#bonusremark-form").validate({
        debug: false,
        //调试模式取消submit的默认提交功能   
        errorClass: "custom-error",
        //默认为错误的样式类为：error   
        focusInvalid: false,
        //当为false时，验证无效时，没有焦点响应  
        onkeyup: false,
        submitHandler: function (form) {
            $("#bonusremark-form").ajaxSubmit(function (data) {
                if (data == "SUCCESS") {
                    myApp.hideIndicator();
                    mainView.router.back();
                    myApp.addNotification({
                        title: "通知",
                        message: "表单提交成功"
                    });
                    setTimeout(function () {
                        myApp.closeNotification(".notifications");
                    }, 2e3);
                } else {
                    myApp.hideIndicator();
                    myApp.addNotification({
                        title: "通知",
                        message: "表单提交失败"
                    });
                    $("#bonusremark-btn").prop("disabled", false).removeClass("color-gray");
                    setTimeout(function () {
                        myApp.closeNotification(".notifications");
                    }, 2e3);
                }
            });
        },
        rules: {
            Bonus: {
                required: true,
                range: [5, 200]
            },
            BonusRemark: {
                required: true,
                maxlength: 100
            }
        },
        messages: {
            Bonus: {
                required: "字段不能为空",
                range: jQuery.format("请输入一个介于 {0} 和 {1} 之间的值")
            },
            RequestRemark: {
                required: "字段不能为空",
                maxlength: jQuery.format("不能大于{0}个字符")
            }
        },
        errorPlacement: function (error, element) {
            myApp.hideIndicator();
            $("#bonusremark-btn").prop("disabled", false).removeClass("color-gray");
            element.attr("placeholder", error.text());
        }
    });
    $$("#bonusremark-btn").click(function () {
        myApp.showIndicator();
        $("#bonusremark-btn").prop("disabled", true).addClass("color-gray");
        setTimeout(function () {
            $("#bonusremark-form").submit();
        }, 500);
    });
});

//Manager_EventList  活动门店列表  查看日期
$$(document).on("pageInit", ".page[data-page='manager-eventlist']", function (e) {
    
    var calendarDefault = myApp.calendar({
        input: "#manager-eventlist-date",
        monthNames: monthNames,
        monthNamesShort: monthNamesShort,
        dayNames: dayNames,
        dayNamesShort: dayNamesShort,
        closeOnSelect: true
    });
    var url = "/Seller/Manager_EventListPartial";
    var date = $$("#manager-eventlist-date").val();
    $$.ajax({
        url: url,
        data: {
            date: date
        },
        success: function (data) {
            $$(".list-content").html(data);
        }
    });
    $$("#manager-eventlist-date").on("change", function () {
        var date = $$("#manager-eventlist-date").val();
        $$.ajax({
            url: "/Seller/Manager_EventListPartial",
            data: {
                date: date
            },
            success: function (data) {
                $$(".list-content").html(data);
            }
        });
    });
    //滑动删除
    $$(".list-content").on("deleted", ".swipeout", function (e) {
        var url = "/Seller/Manager_DeleteEvent";
        var Id = $$(e.target).attr("data-url");
        console.log(Id);
        swipe_deleted(url, Id);
    });
});

//Manager_CreateEvent 添加活动日程
$$(document).on("pageInit", ".page[data-page='manager-addschedule']", function () {
    var starttime = $$("#startTime").val().split(":");
    var endtime = $$("#endTime").val().split(":");
    var calendarMultiple = myApp.calendar({
        input: "#actDate",
        dateFormat: "yyyy-mm-dd",
        multiple: true,
        monthNames: monthNames,
        monthNamesShort: monthNamesShort,
        dayNames: dayNames,
        dayNamesShort: dayNamesShort
    });
    var pickerInline = myApp.picker({
        input: "#startTime",
        toolbar: true,
        rotateEffect: true,
        toolbarCloseText: "关闭",
        formatValue: function (p, values) {
            return values[0] + ":" + values[1];
        },
        value: [starttime[0], starttime[1]],
        cols: col
    });
    var pickerInline = myApp.picker({
        input: "#endTime",
        toolbar: true,
        rotateEffect: true,
        toolbarCloseText: "关闭",
        formatValue: function (p, values) {
            return values[0] + ":" + values[1];
        },
        value: [endtime[0], endtime[1]],
        cols: col
    });
    //表单提交
    $("#createevent-form").validate({
        debug: false,
        //调试模式取消submit的默认提交功能   
        errorClass: "custom-error",
        //默认为错误的样式类为：error   
        focusInvalid: false,
        //当为false时，验证无效时，没有焦点响应  
        onkeyup: false,
        submitHandler: function (form) {
            var store = $$("#actStore").val();
            var start = $$("#startTime").val();
            var end = $$("#endTime").val();
            if (start > end) {
                myApp.hideIndicator();
                myApp.addNotification({
                    title: "通知",
                    message: "开始时间不能大于结束时间"
                });
                $("#manangerschedule-btn").prop("disabled", false).removeClass("color-gray");
                setTimeout(function () {
                    myApp.closeNotification(".notifications");
                }, 2e3);
            } else if (/Invalid|NaN|undefined/.test(store) || store == "") {
                myApp.hideIndicator();
                myApp.addNotification({
                    title: "通知",
                    message: "必须选择一个门店"
                });
                $("#manangerschedule-btn").prop("disabled", false).removeClass("color-gray");
                setTimeout(function () {
                    myApp.closeNotification(".notifications");
                }, 2e3);
            } else {
                $("#createevent-form").ajaxSubmit(function (data) {
                    if (data == "SUCCESS") {
                        myApp.hideIndicator();
                        mainView.router.back();
                        myApp.addNotification({
                            title: "通知",
                            message: "表单提交成功"
                        });
                        setTimeout(function () {
                            myApp.closeNotification(".notifications");
                        }, 2e3);
                    } else {
                        myApp.hideIndicator();
                        myApp.addNotification({
                            title: "通知",
                            message: "表单提交失败"
                        });
                        $("#manangerschedule-btn").prop("disabled", false).removeClass("color-gray");
                        setTimeout(function () {
                            myApp.closeNotification(".notifications");
                        }, 2e3);
                    }
                });
            }
        },
        rules: {
            actDate: {
                required: true,
                datearray: true
            },
            startTime: {
                required: true,
                time: true
            },
            endTime: {
                required: true,
                time: true
            },
            Salary: {
                required: true,
                range: [0, 500]
            }
        },
        messages: {
            actDate: {
                required: "必填",
                datearray: "时间格式不正确"
            },
            startTime: {
                required: "必填",
                time: "时间格式不正确"
            },
            endTime: {
                required: "必填",
                time: "时间格式不正确"
            },
            Salary: {
                required: "必填",
                range: jQuery.format("请输入一个介于 {0} 和 {1} 之间的值")
            }
        },
        errorPlacement: function (error, element) {
            myApp.hideIndicator();
            $("#manangerschedule-btn").prop("disabled", false).removeClass("color-gray");
            element.attr("placeholder", error.text());
        }
    });
    $$("#manangerschedule-btn").click(function () {
        myApp.showIndicator();
        $("#manangerschedule-btn").prop("disabled", true).addClass("color-gray");
        setTimeout(function () {
            $("#createevent-form").submit();
        }, 500);
    });
});


$$(document).on("pageAfterBack", ".page[data-page='manager-addschedule']", function () {
    var url = "/Seller/Manager_EventListPartial";
    var date = $$("#manager-eventlist-date").val();
    $$.ajax({
        url: url,
        data: {
            date: date
        },
        success: function (data) {
            $$(".list-content").html(data);
        }
    });
});
//Manager_EditSchedule 修改活动日程
$$(document).on("pageInit", ".page[data-page='manager-editschedule']", function () {
    var starttime = $$("#Standard_CheckIn").val().split(":");
    var endtime = $$("#Standard_CheckOut").val().split(":");
    var pickerInline = myApp.picker({
        input: "#Standard_CheckIn",
        toolbar: true,
        rotateEffect: true,
        toolbarCloseText: "关闭",
        formatValue: function (p, values) {
            return values[0] + ":" + values[1];
        },
        value: [starttime[0], starttime[1]],
        cols: col
    });
    var pickerInline = myApp.picker({
        input: "#Standard_CheckOut",
        toolbar: true,
        rotateEffect: true,
        toolbarCloseText: "关闭",
        formatValue: function (p, values) {
            return values[0] + ":" + values[1];
        },
        value: [endtime[0], endtime[1]],
        cols: col
    });
    //表单提交
    $("#editschedule-form").validate({
        debug: false,
        //调试模式取消submit的默认提交功能   
        errorClass: "custom-error",
        //默认为错误的样式类为：error   
        focusInvalid: false,
        //当为false时，验证无效时，没有焦点响应  
        onkeyup: false,
        submitHandler: function (form) {
            var start = $$("#Standard_CheckIn").val();
            var end = $$("#Standard_CheckOut").val();
            if (start > end) {
                myApp.hideIndicator();
                myApp.addNotification({
                    title: "通知",
                    message: "开始时间不能大于结束时间"
                });
                $("#editschedule-btn").prop("disabled", false).removeClass("color-gray");
                setTimeout(function () {
                    myApp.closeNotification(".notifications");
                }, 2e3);
            } else {
                $("#editschedule-form").ajaxSubmit(function (data) {
                    if (data == "SUCCESS") {
                        myApp.hideIndicator();
                        mainView.router.back();
                        myApp.addNotification({
                            title: "通知",
                            message: "表单提交成功"
                        });
                        setTimeout(function () {
                            myApp.closeNotification(".notifications");
                        }, 2e3);
                    } else {
                        myApp.hideIndicator();
                        myApp.addNotification({
                            title: "通知",
                            message: "表单提交失败"
                        });
                        $("#editschedule-btn").prop("disabled", false).removeClass("color-gray");
                        setTimeout(function () {
                            myApp.closeNotification(".notifications");
                        }, 2e3);
                    }
                });
            }
        },
        rules: {
            Standard_CheckIn: {
                required: true,
                time: true
            },
            Standard_CheckOut: {
                required: true,
                time: true
            },
            Standard_Salary: {
                required: true,
                range: [0, 500]
            }
        },
        messages: {
            Standard_CheckIn: {
                required: "必填",
                time: "时间格式不正确"
            },
            Standard_CheckOut: {
                required: "必填",
                time: "时间格式不正确"
            },
            Standard_Salary: {
                required: "必填",
                range: jQuery.format("请输入一个介于 {0} 和 {1} 之间的值")
            }
        },
        errorPlacement: function (error, element) {
            myApp.hideIndicator();
            $("#editschedule-btn").prop("disabled", false).removeClass("color-gray");
            element.attr("placeholder", error.text());
        }
    });
    $$("#editschedule-btn").click(function () {
        myApp.showIndicator();
        $("#editschedule-btn").prop("disabled", true).addClass("color-gray");
        setTimeout(function () {
            $("#editschedule-form").submit();
        }, 500);
    });
});

//Manager_QuerySeller  搜索促销员
$$(document).on("pageInit", ".page[data-page='manager-queryseller']", function () {
    var mySearchbar = myApp.searchbar(".searchbar", {
        searchList: ".list-block-search",
        searchIn: ".item-content"
    });
});

// Manager_EditSellerInfo 修改促销员信息
$$(document).on("pageInit", ".page[data-page='manager-editsellerinfo']", function () {
    $("#editsellerinfo-form").validate({
        debug: false,
        //调试模式取消submit的默认提交功能   
        errorClass: "custom-error",
        //默认为错误的样式类为：error   
        focusInvalid: false,
        //当为false时，验证无效时，没有焦点响应  
        onkeyup: false,
        submitHandler: function (form) {
            $("#editsellerinfo-form").ajaxSubmit(function (data) {
                if (data == "SUCCESS") {
                    myApp.hideIndicator();
                    mainView.router.back();
                    myApp.addNotification({
                        title: "通知",
                        message: "表单提交成功"
                    });
                    setTimeout(function () {
                        myApp.closeNotification(".notifications");
                    }, 2e3);
                } else {
                    myApp.hideIndicator();
                    myApp.addNotification({
                        title: "通知",
                        message: "表单提交失败"
                    });
                    $("#editsellerinfo-btn").prop("disabled", false).removeClass("color-gray");
                    setTimeout(function () {
                        myApp.closeNotification(".notifications");
                    }, 2e3);
                }
            });
        },
        rules: {
            IdNumber: {
                required: true,
                idnumber: true
            },
            AccountSource: {
                required: true,
                maxlength: 50
            },
            AccountName: {
                required: true,
                maxlength: 10
            },
            CardNo: {
                required: true,
                maxlength: 30
            },
            StandardSalary: {
                range: [0, 500]
            }
        },
        messages: {
            IdNumber: {
                required: "必填",
                idnumber: "请正确填写身份证号码"
            },
            AccountSource: {
                required: "必填",
                maxlength: jQuery.format("不能大于{0}个字符")
            },
            AccountName: {
                required: "必填",
                maxlength: jQuery.format("不能大于{0}个字符")
            },
            CardNo: {
                required: "必填",
                maxlength: jQuery.format("不能大于{0}个字符")
            },
            StandardSalary: {
                range: jQuery.format("请输入一个介于 {0} 和 {1} 之间的值")
            }
        },
        errorPlacement: function (error, element) {
            myApp.hideIndicator();
            $("#editsellerinfo-btn").prop("disabled", false).removeClass("color-gray");
            element.attr("placeholder", error.text());
        }
    });
    $$("#editsellerinfo-btn").click(function () {
        myApp.showIndicator();
        $("#editsellerinfo-btn").prop("disabled", true).addClass("color-gray");
        setTimeout(function () {
            $("#editsellerinfo-form").submit();
        }, 500);
    });
});

//Manager_StoreList  督导管理门店  查询
$$(document).on("pageInit", ".page[data-page='manager-storelist']", function () {
    var mySearchbar = myApp.searchbar(".searchbar", {
        searchList: ".list-block-search",
        searchIn: ".item-content"
    });
});

//Manager_BonusList  红包列表 下拉刷新  
$$(document).on("pageInit", ".page[data-page='manager-bonuslist']", function () {
    // 列表内容更新
    $$.ajax({
        url: "/Seller/Manager_BonusList_HistoryPartial",
        success: function (html) {
            $$("#history-content").html(html);
        }
    });
    $$.ajax({
        url: "/Seller/Manager_BonusList_UnSendPartial",
        success: function (data) {
            $$("#bonus-content").html(data);
        }
    });
    // 滑动删除
    $$("#bonus-content").on("deleted", ".swipeout", function (e) {
        $$.ajax({
            url: "/Seller/Manager_BonusDismiss",
            data: {
                id: $$(e.target).attr("data-url")
            },
            method: "post",
            success: function (data) {
                data = JSON.parse(data);
                if (data.result != "SUCCESS") {
                    myApp.alert("删除失败");
                }
            }
        });
    });
    // 确认红包
    $$("#bonus-content").on("click", ".confirmbonus", function (e) {
        var data_url = $$(this).attr("data-url");
        myApp.confirm("是否确认发放红包?", function () {
            $$.ajax({
                url: "/Seller/Manager_BonusConfirm",
                data: {
                    id: data_url
                },
                method: "post",
                success: function (data) {
                    data = JSON.parse(data);
                    if (data.result == "SUCCESS") {
                        // 页面刷新
                        $$.ajax({
                            url: "/Seller/Manager_BonusList_UnSendPartial",
                            success: function (data) {
                                $$("#bonus-content").html(data);
                            }
                        });
                    } else {
                        myApp.alert("红包发放失败");
                    }
                }
            });
        });
    });
    // Pull to refresh content
    var ptrContent = $$(".pull-to-refresh-content");
    // Add 'refresh' listener on it
    ptrContent.on("refresh", function (e) {
        // Emulate 2s loading
        setTimeout(function () {
            // Random song
            $$.ajax({
                url: "/Seller/Manager_BonusList_HistoryRefresh",
                method: "post",
                success: function (e) {
                    var data = JSON.parse(e);
                    if (data.result == "SUCCESS") {
                        $$.ajax({
                            url: "/Seller/Manager_BonusList_HistoryPartial",
                            success: function (html) {
                                $$("#history-content").html(html);
                            }
                        });
                    }
                }
            });
            myApp.pullToRefreshDone();
        }, 2e3);
    });
});

//Mnanager_StoreList 门店位置信息
$$(document).on("pageInit", ".page[data-page='manager-storelist']", function () {
    $$(".store_details").click(function () {
        var btn = $$(this);
        $.ajax({
            url: "http://apis.map.qq.com/ws/coord/v1/translate",
            type: "get",
            dataType: "jsonp",
            data: {
                locations: $$(this).attr("data-latitude") + "," + $$(this).attr("data-longitude"),
                type: 3,
                output: "jsonp",
                key: "FAKBZ-YPIW4-TOLUE-XLQOL-MAYZQ-3FFGF"
            },
            success: function (data) {
                if (data.status == 0) {
                    wx.openLocation({
                        latitude: data.locations[0].lat,
                        // 纬度，浮点数，范围为90 ~ -90
                        longitude: data.locations[0].lng,
                        // 经度，浮点数，范围为180 ~ -180。
                        name: btn.attr("data-storename"),
                        // 位置名
                        address: btn.attr("data-address"),
                        // 地址详情说明
                        scale: 25,
                        // 地图缩放级别,整形值,范围从1~28。默认为最大
                        infoUrl: ""
                    });
                }
            }
        });
    });
});

/*************** 暗促信息 *************/
//Manager_TempSellerDetails  暗促系统  暗促签到图片查看
$$(document).on("pageInit", ".page[data-page='manager-tempsellerdetails']", function () {
    var phList = $("#sellertask-details-phlist").val().split(",");
    var photo = new Array();
    $$.each(phList, function (num, ph) {
        var url = "http://cdn2.shouquanzhai.cn/checkin-img/" + ph;
        var obj = {
            url: url
        };
        photo.push(obj);
    });
    var myPhotoBrowserPopupDark = myApp.photoBrowser({
        photos: photo,
        theme: "dark",
        type: "standalone",
        lazyLoading: true,
        zoom: false,
        backLinkText: "关闭"
    });
    $$(".ph-tempseller").on("click", function (e) {
        myPhotoBrowserPopupDark.open();
    });
});

//ManagerSellerTaskMonthStatistic 暗促系统  暗促信息查询
$$(document).on("pageInit", ".page[data-page='manager-sellertask-month']", function () {
    $$.ajax({
        url: "/Seller/ManagerSellerTaskMonthStatisticPartial",
        data: {
            querydate: $("#statistic-month").val()
        },
        success: function (data) {
            $$("#manager-seller-taskmonth").html(data);
        }
    });
    $("#statistic-month").on("change", function (e) {
        $$.ajax({
            url: "/Seller/ManagerSellerTaskMonthStatisticPartial",
            data: {
                querydate: $("#statistic-month").val()
            },
            success: function (data) {
                $$("#manager-seller-taskmonth").html(data);
            }
        });
    });
});

//ManangerSellerTaskSeller 暗促系统  暗促签到列表  无限循环
$$(document).on("pageInit", ".page[data-page='managerseller-taskdate']", function () {
    var page = 1;
    var url = "/Seller/ManagerSellerTaskSellerPartial";
    $$.ajax({
        url: url,
        data: {
            page: page,
            id: $("#sellerid").val()
        },
        success: function (data) {
            if (data != "FAIL") {
                $$("#managerseller-list").html(data);
                page++;
            }
        }
    });
    //刷新
    var loading = false;
    //加载flag
    $$(".infinite-scroll").on("infinite", function (e) {
        $$(".infinite-scroll-preloader").removeClass("hidden");
        if (loading) return;
        loading = true;
        setTimeout(function () {
            loading = false;
            //重置flag
            //生成新的条目
            $$.ajax({
                url: url,
                data: {
                    page: page,
                    id: $("#sellerid").val()
                },
                success: function (data) {
                    if (data == "NONE" || data == "FAIL") {
                        myApp.detachInfiniteScroll($$(".infinite-scroll"));
                        //关闭滚动
                        $$(".infinite-scroll-preloader").remove();
                        //移除加载符
                        $$(".infinite-pre").removeClass("hidden");
                        return;
                    } else {
                        $$("#managerseller-list").append(data);
                        page++;
                    }
                }
            });
        }, 1e3);
    });
});

//ManangerSellerTaskQuery  暗促信息查询
$$(document).on("pageInit", ".page[data-page='managersellertask-query']", function () {
    var mySearchbar = myApp.searchbar(".searchbar", {
        searchList: ".list-block-search",
        searchIn: ".item-content"
    });
});

// 辅助程序
// 用户模板更新
function refresh_userpanel() {
    $$.ajax({
        url: "/Seller/Manager_UserPanel",
        success: function (data) {
            if (data != "Error") $$("#manager_userpanel").html(data);
        }
    });
}

// 当前字数更新
function currentTextAreaLength(pagename, id_name, max_length, result_id) {
    var tl_c = $$("#" + id_name).val().length;
    $$("#" + result_id).text(tl_c);
    //$$("#" + pagename).off("change", "#" + id_name);
    $$("#" + pagename).on("change", "#" + id_name, function () {
        var tl = $$("#" + id_name).val().length;
        if (tl < max_length) {
            $$("#" + result_id).text(tl);
        } else {
            myApp.alert("已超出最大值，请重新填写或删除部分信息");
            var str = $$("#" + id_name).val();
            $$("#" + id_name).val(str.slice(0, 50));
            $$("#" + result_id).text("50");
        }
    });
}

// 上传签到图片文件模块
function uploadCheckinFile(pagename, imglist, photolist_id, current_count, max_count) {
    $$("#" + imglist).html("");
    var photolist = splitArray($$("#" + photolist_id).val());
    $$("#" + current_count).text(photolist.length);
    for (var i = 0; i < photolist.length; i++) {
        $$("#" + imglist).append('<li><div class="rep-imgitem" data-rel=\'' + photolist[i] + "' style=\"background-image:url(/Seller/ThumbnailImage?filename=" + photolist[i] + '); background-size:cover"></div></li>');
    }
    $$("#" + imglist).append('<li><a href="javascript:;" class="rep-imgitem-btn" id="' + imglist + '-upload-btn"><i class="fa fa-plus"></i></a></li>');
    $$("#" + imglist).on("click", "#" + imglist + "-upload-btn", function (e) {
        var localIds;
        var photolist = splitArray($("#" + photolist_id).val());
        if (photolist.length < max_count) {
            wx.chooseImage({
                count: 1,
                // 默认9
                sizeType: ["compressed"],
                // 可以指定是原图还是压缩图，默认二者都有
                sourceType: ["album", "camera"],
                // 可以指定来源是相册还是相机，默认二者都有
                success: function (res) {
                    localIds = res.localIds;
                    // 返回选定照片的本地ID列表，localId可以作为img标签的src属性显示图片
                    //$("#preview").attr("src", localIds);
                    wx.uploadImage({
                        localId: localIds[0],
                        // 需要上传的图片的本地ID，由chooseImage接口获得
                        isShowProgressTips: 1,
                        // 默认为1，显示进度提示
                        success: function (res) {
                            var serverId = res.serverId;
                            // 返回图片的服务器端ID
                            $$.ajax({
                                url: "/Seller/SaveOrignalImage",
                                type: "post",
                                data: {
                                    serverId: serverId
                                },
                                success: function (resource) {
                                    data = JSON.parse(resource);
                                    if (data.result == "SUCCESS") {
                                        $$("#" + imglist).html("");
                                        photolist.push(data.filename);
                                        $$("#" + current_count).text(photolist.length);
                                        $$("#" + photolist_id).val(photolist.toString());
                                        for (var i = 0; i < photolist.length; i++) {
                                            $$("#" + imglist).append('<li><div class="rep-imgitem" data-rel=\'' + photolist[i] + "' style=\"background-image:url(/Seller/ThumbnailImage?filename=" + photolist[i] + '); background-size:cover"></div></li>');
                                        }
                                        $$("#" + imglist).append('<li><a href="javascript:;" class="rep-imgitem-btn" id="' + imglist + '-upload-btn"><i class="fa fa-plus"></i></a></li>');
                                    } else {
                                        myApp.alert("上传失败，请重试");
                                    }
                                }
                            });
                        }
                    });
                }
            });
        } else {
            myApp.alert("上传图片不得大于" + max_count + "张，无法添加");
        }
    });
    // 删除图片
    //$$("#" + pagename).off("click", ".rep-imgitem");
    $$("#" + imglist).on("click", ".rep-imgitem", function (e) {
        var img_item = $$(this);
        $$(".rep-imgitem").each(function () {
            $$(this).html("");
        });
        img_item.html("<div class='rep-imgitem-selected'><i class='fa fa-minus'></i></div>");
    });
    $$("#" + imglist).on("click", ".rep-imgitem-selected", function () {
        myApp.confirm("是否确认删除已上传图片?", "提示", function () {
            //myApp.alert('You clicked Ok button');
            var delete_item = $(".rep-imgitem-selected").closest(".rep-imgitem").attr("data-rel");
            var arraylist = splitArray($("#" + photolist_id).val());
            var pos = $.inArray(delete_item, arraylist);
            arraylist.splice(pos, 1);
            $$("#" + photolist_id).val(arraylist.toString());
            $$("#" + current_count).text(arraylist.length);
            $$("#" + imglist).html("");
            for (var i = 0; i < arraylist.length; i++) {
                $("#" + imglist).append('<li><div class="rep-imgitem" data-rel=\'' + arraylist[i] + "' style=\"background-image:url(/Seller/ThumbnailImage?filename=" + arraylist[i] + '); background-size:cover"></div></li>');
            }
            $$("#" + imglist).append('<li><a href="javascript:;" class="rep-imgitem-btn" id="' + imglist + '-upload-btn"><i class="fa fa-plus"></i></a></li>');
        });
    });
}

// 上传地理位置信息
function uploadLocation(btn_id, location_id) {
    $$("#" + btn_id).on("click", function () {
        myApp.showIndicator();
        // 4秒后强制关闭
        setTimeout(function () {
            if (!loc_success) {
                myApp.hideIndicator();
                myApp.alert("获取位置失败");
            }
        }, 4e3);
        var loc_success = false;
        wx.getLocation({
            type: "wgs84",
            // 默认为wgs84的gps坐标，如果要返回直接给openLocation用的火星坐标，可传入'gcj02'
            success: function (res) {
                var latitude = res.latitude;
                // 纬度，浮点数，范围为90 ~ -90
                var longitude = res.longitude;
                // 经度，浮点数，范围为180 ~ -180。
                var speed = res.speed;
                // 速度，以米/每秒计
                var accuracy = res.accuracy;
                // 位置精度
                var gps_location = longitude + "," + latitude;
                loc_success = true;
                $$("#" + btn_id).find(".item-after").text("上传位置成功");
                //cell_success_location(btn, "位置获取成功", latitude, longitude);
                $$("#" + location_id).val(gps_location);
                myApp.hideIndicator();
            }
        });
        return false;
    });
}

function uploadLocationWithDetails(btn_id, location_id, lbs_details_id) {
    $$("#" + btn_id).on("click", function () {
        myApp.showIndicator();
        // 4秒后强制关闭
        setTimeout(function () {
            if (!loc_success) {
                myApp.hideIndicator();
                myApp.alert("获取位置失败");
            }
        }, 4e3);
        var loc_success = false;
        wx.getLocation({
            type: "wgs84",
            // 默认为wgs84的gps坐标，如果要返回直接给openLocation用的火星坐标，可传入'gcj02'
            success: function (res) {
                var latitude = res.latitude;
                // 纬度，浮点数，范围为90 ~ -90
                var longitude = res.longitude;
                // 经度，浮点数，范围为180 ~ -180。
                var speed = res.speed;
                // 速度，以米/每秒计
                var accuracy = res.accuracy;
                // 位置精度
                var gps_location = longitude + "," + latitude;
                loc_success = true;
                $$("#" + btn_id).find(".item-after").text("上传位置成功");
                //cell_success_location(btn, "位置获取成功", latitude, longitude);
                //var translbs = 
                $$("#" + location_id).val(gps_location);
                var translbs = latitude + "," + longitude;
                $.ajax({
                    url: "http://apis.map.qq.com/ws/coord/v1/translate",
                    method: "get",
                    dataType: "jsonp",
                    data: {
                        locations: translbs,
                        type: 1,
                        output: "jsonp",
                        key: "FAKBZ-YPIW4-TOLUE-XLQOL-MAYZQ-3FFGF"
                    },
                    success: function (data) {
                        if (data.status == 0) {
                            geocoder = new qq.maps.Geocoder({
                                complete: function (result) {
                                    $("#" + lbs_details_id).val(result.detail.address);
                                }
                            });
                            var coord = new qq.maps.LatLng(data.locations[0].lat, data.locations[0].lng);
                            geocoder.getAddress(coord);
                            myApp.hideIndicator();
                        }
                    }
                });
            }
        });
        return false;
    });
}

// 图片浏览模块
function PhotoBrowser(pagename) {
    $$("#" + pagename).on("click", ".checkin-photos", function () {
        var photos = $(this).attr("data-photos");
        if (photos.trim() != "") {
            var images = photos.split(",");
            for (var i = 0; i < images.length; i++) {
                images[i] = "http://cdn2.shouquanzhai.cn/checkin-img/" + images[i];
            }
            wx.previewImage({
                current: images[0],
                // 当前显示图片的http链接
                urls: images
            });
        } else {
            myApp.alert("没有找到图片");
        }
    });
}

// 查看位置模块
function LocationBrowser(pagename) {
    $$("#" + pagename).on("click", ".checkin-lbs", function () {
        var lbs = $$(this).attr("data-location").split(",");
        var translbs = lbs[1] + "," + lbs[0];
        var loc_details = $$(this).attr("data-desc");
        $.ajax({
            url: "http://apis.map.qq.com/ws/coord/v1/translate",
            type: "get",
            dataType: "jsonp",
            data: {
                locations: translbs,
                type: 1,
                output: "jsonp",
                key: "FAKBZ-YPIW4-TOLUE-XLQOL-MAYZQ-3FFGF"
            },
            success: function (data) {
                if (data.status == 0) {
                    wx.openLocation({
                        latitude: data.locations[0].lat,
                        // 纬度，浮点数，范围为90 ~ -90
                        longitude: data.locations[0].lng,
                        // 经度，浮点数，范围为180 ~ -180。
                        name: "位置信息",
                        // 位置名
                        address: loc_details,
                        // 地址详情说明
                        scale: 25,
                        // 地图缩放级别,整形值,范围从1~28。默认为最大
                        infoUrl: ""
                    });
                }
            }
        });
    });
}

// 图片数组转化
function splitArray(value) {
    var list = new Array();
    if (value != null) {
        if (value.trim() != "") {
            list = value.trim().split(",");
            return list;
        }
    }
    return list;
}

//巡店 ajax异步提交
function datepicker_refresh(url) {
    var calendarDefault = myApp.calendar({
        input: ".check-date",
        monthNames: monthNames,
        monthNamesShort: monthNamesShort,
        dayNames: dayNames,
        dayNamesShort: dayNamesShort,
        closeOnSelect: true
    });
    var date = $$(".check-date").val();
    $$.ajax({
        url: url,
        data: {
            date: date
        },
        success: function (data) {
            $$(".list-content").html(data);
        }
    });
    $$(".check-date").on("change", function () {
        var date = $$(".check-date").val();
        $$.ajax({
            url: url,
            data: {
                date: date
            },
            success: function (data) {
                $$(".list-content").html(data);
            }
        });
    });
}

//删除
function swipe_deleted(url, Id) {
    $$.ajax({
        url: url,
        method: "POST",
        data: {
            id: Id
        },
        success: function (res) {
            var data = JSON.parse(res);
            if (data.result == "SUCCESS") { } else {
                myApp.alert("删除失败");
            }
        }
    });
}

// 刷新首页
function refresh_home() {
    $$.ajax({
        url: "/Seller/Manager_RefreshAllCount",
        method: "post",
        success: function (data) {
            data = JSON.parse(data);
            if (data.result == "SUCCESS") {
                var countlist = data.data;
                $$("#uncheckin-count").text(countlist.uncheckin);
                $$("#uncheckout-count").text(countlist.uncheckout);
                $$("#unreport-count").text(countlist.unreport);
                $$("#unconfirm-count").text(countlist.unconfirm);
            } else {
                $$("#uncheckin-count").text("N/A");
                $$("#uncheckout-count").text("N/A");
                $$("#unreport-count").text("N/A");
                $$("#unconfirm-count").text("N/A");
            }
            myApp.pullToRefreshDone();
        }
    });
}

//小时 分钟
var col = [ // Hours
{
    values: function () {
        var arr = [];
        for (var i = 0; i <= 23; i++) {
            arr.push(i < 10 ? "0" + i : i);
        }
        return arr;
    }()
}, // Divider
{
    divider: true,
    content: ":"
}, // Minutes
{
    values: function () {
        var arr = [];
        for (var i = 0; i <= 59; i++) {
            arr.push(i < 10 ? "0" + i : i);
        }
        return arr;
    }()
}];