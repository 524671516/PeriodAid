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

$$.ajax({
    url: "/Seller/Seller_Panel",
    success: function (data) {
        $$("#user-panel").html(data);
    }
})

// 微信初始化
wx.config({
    debug: true,
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

$$(document).on("pageInit", ".page[data-page='seller-changeaccount']", function (e) {
    myApp.closePanel();
    $$("#SystemId").change(function (e) {
        var selectlist = $$("#SystemId");
        $$.ajax({
            url: "/Seller/Seller_RefreshBindListAjax",
            data: {
                id: selectlist.val()
            },
            method: "post",
            success: function (data) {
                $$("#BindId").html(data);
                $$("#bindstore").text($$("#BindId>option")[0].innerText);
            }
        });
    })
    $$("#changeaccount-submit").click(function () {
        console.log($$("#SystemId").val());
        console.log($$("#BindId").val());
        myApp.showIndicator();
        $("#changeaccount-submit").prop("disabled", true).addClass("color-gray");
        setTimeout(function () {
            $("#changeaccount-form").submit();
        }, 500);
    });
    $("#changeaccount-form").validate({
        debug: false,
        //调试模式取消submit的默认提交功能   
        errorClass: "custom-error",
        //默认为错误的样式类为：error   
        focusInvalid: false,
        //当为false时，验证无效时，没有焦点响应
        onkeyup: false,
        submitHandler: function (form) {
            $("#changeaccount-form").ajaxSubmit(function (data) {
                if (data == "SUCCESS") {
                    myApp.hideIndicator();
                    window.location.href = "/Seller/Seller_Home";
                } else {
                    myApp.hideIndicator();
                    myApp.addNotification({
                        title: "通知",
                        message: "表单提交失败"
                    });
                    $("#changeaccount-submit").prop("disabled", false).removeClass("color-gray");
                    setTimeout(function () {
                        myApp.closeNotification(".notifications");
                    }, 2e3);
                }
            });
        },
        errorPlacement: function (error, element) {
            myApp.hideIndicator();
            $("#changeaccount-submit").prop("disabled", false).removeClass("color-gray");
            element.attr("placeholder", error.text());
        }
    });
})
$$("#checkin").on("click", function () {
    if (!$$(this).hasClass("readonly")) {
        var url = $$(this).attr("data-url");
        mainView.router.load({
            url: url,
            animatePages: false
        });
        $$("#checkin span").text("重新签到");
        $$("#checkout").removeClass("readonly");
        $$("#report").removeClass("active");
        $$(this).addClass("active")
    }
});
$$("#checkout").on("click", function () {
    if (!$$(this).hasClass("readonly")) {
        $$("#report").removeClass("active");
        $$("#checkin").removeClass("active").addClass("readonly");
        var url = $$(this).attr("data-url");
        mainView.router.load({
            url: url,
            animatePages: false
        });
        $$("#checkout span").text("重新签退");
        $$(this).addClass("active");
    };
});
$$("#report").on("click", function () {
    $$(this).addClass("active");
    $$("#checkin").removeClass("active").children("span").text("开始签到");
    $$("#checkout").removeClass("active").addClass("readonly").children("span").text("开始签退");
    var url = $$(this).attr("data-url");
    mainView.router.load({
        url: url,
        animatePages: false
    });
    $$("#report span").text("重新提报");
});
//Seller_CheckIn 签到
$$(document).on("pageInit", ".page[data-page='seller-checkin']", function (e) {
    //上传位置信息、图片信息
    uploadLocation("location-btn", "CheckinLocation");
    uploadImage("img-btn", "CheckinPhoto");
    //提交
    $("#checkin_form").validate({
        debug: false,
        //调试模式取消submit的默认提交功能   
        errorClass: "custom-error",
        //默认为错误的样式类为：error   
        focusInvalid: false,
        //当为false时，验证无效时，没有焦点响应  
        onkeyup: false,
        submitHandler: function (form) {
            var photoList = splitArray($("#CheckinPhoto").val());
            if ($("#CheckinLocation").val().trim == "") {
                myApp.hideIndicator();
                myApp.alert("请上传您的地理位置");
                $("#checkin-btn").prop("disabled", true).removeClass("color-gray");
            }
            else if (photoList.length == 0) {
                myApp.hideIndicator();
                myApp.alert("至少上传一张照片");
                $("#checkin-btn").prop("disabled", true).removeClass("color-gray");
            }
            else {
                $("#checkin_form").ajaxSubmit(function (data) {
                    if (data == "SUCCESS") {
                        myApp.hideIndicator();
                        mainView.router.back();
                        myApp.addNotification({
                            title: "通知",
                            message: "签到成功"
                        });
                        setTimeout(function () {
                            myApp.closeNotification(".notifications");
                        }, 2000);
                    } else {
                        myApp.hideIndicator();
                        myApp.addNotification({
                            title: "通知",
                            message: "签到失败"
                        });
                        $("#checkin-btn").prop("disabled", false).removeClass("color-gray");
                        setTimeout(function () {
                            myApp.closeNotification(".notifications");
                        }, 2000);
                    }
                });
            }
        }

    });
    $$("#checkin-btn").click(function () {
        myApp.showIndicator();
        $("#checkin-btn").prop("disabled", true).addClass("color-gray");
        setTimeout(function () {
            $("#checkin_form").submit();
        }, 500);
    });
});

//Seller_CheckOut 签退
$$(document).on("pageInit", ".page[data-page='seller-checkout']", function () {
    //上传位置信息、签退图片
    uploadLocation("location-btn", "CheckoutLocation");
    uploadImage("img-btn", "CheckoutPhoto");
    //提交
    $("#checkout_form").validate({
        debug: false,//调试模式取消submit的默认提交功能
        errorClass: "custom-error",//默认为错误的样式类为：error;
        focusInvalid: false,//当为false时，验证无效时，没有焦点相应
        onkeyup: false,
        submitHandler: function (form) {
            var photoList = splitArray($("#CheckoutPhoto").val());
            if (photoList.length == 0) {
                myApp.hideIndicator();
                myApp.alert("至少上传一张图片");
                $("#checkout-btn").prop("disabled", true).removeClass("color-gray");
            }
            else if ($("#CheckoutLocation").val().trim == "") {
                myApp.hideIndicator();
                myApp.alert("请上传您的位置信息");
                $("#checkout-btn").prop("disabled", true).removeClass("color-gray");
            }
            else {
                $("#checkout_form").ajaxSubmit(function (data) {
                    if (data == "SUCCESS") {
                        myApp.hideIndicator();
                        mainView.router.back();
                        myApp.addNotification({
                            title: "通知",
                            message: "签退成功"
                        });
                        setTimeout(function () {
                            myApp.closeNotification(".notifications");
                        }, 2000);
                    } else {
                        myApp.hideIndicator();
                        myApp.addNotification({
                            title: "通知",
                            message: "签退失败"
                        });
                        $("#checkout-btn").prop("disabled", false).removeClass("color-gray");
                        setTimeout(function () {
                            myApp.closeNotification(".notifications");
                        }, 2000);
                    }
                });
            }
        }
    });
    $$("#checkout-btn").click(function () {
        myApp.showIndicator();
        $$("#checkout-btn").prop("disabled", true).addClass("color-gray");
        setTimeout(function () {
            $("#checkout_form").submit();
        }, 500);
    });
});

//Seller_Report 销量报表
$$(document).on("pageInit", ".page[data-page='seller-report']", function () {
    $$.ajax({
        url: "/Seller/Seller_ReportPartial",
        data: {
            id: $$("#reportlist").val()
        },
        success: function (data) {
            $$("#report-content").html(data);
            currentTextAreaLength("report-content", "Remark", 500, "report-curracount");
            uploadCheckinFile("report-content", "report-imglist", "Rep_Image", "report-imgaccount", 7)
        }
    });
    $$("#reportlist").on("change", function () {
        $$.ajax({
            url: "/Seller/Seller_ReportPartial",
            data: {
                id: $$("#reportlist").val()
            },
            success: function (data) {
                $$("#report-content").html(data);
                currentTextAreaLength("report-content", "Remark", 500, "report-curracount");
                uploadCheckinFile("report-content", "report-imglist", "Rep_Image", "report-imgaccount", 7)
            }
        });
    });
    //提交
    $$("#report-content").on("click", "#report-btn", function () {
        $$("#report-btn").prop("disabled", true).addClass("color-gray");
        myApp.showIndicator();
        setTimeout(function () {
            myApp.hideIndicator();
        }, 2000);
        setTimeout(function () {
            var photoArray = splitArray($$("#Rep_Image").val());
            if (photoArray.length == 0) {
                myApp.hideIndicator();
                myApp.alert("至少上传一张图片");
                $("#report-btn").prop("disabled", true).removeClass("color-gray");
            }
            else {
                $("#report_form").ajaxSubmit({
                    success: function (data) {
                        if (data == "SUCCESS") {
                            myApp.hideIndicator();
                            mainView.router.back();
                            myApp.addNotification({
                                title: "通知",
                                message: "销量报表提交成功"
                            });
                            setTimeout(function () {
                                myApp.closeNotification(".notifications");
                            }, 2000);
                        } else {
                            myApp.hideIndicator();
                            $("#report-btn").prop("disabled", true).addClass("color-gray");
                            myApp.addNotification({
                                title: "通知",
                                message: "销量报表提交失败"
                            });
                            $("#report-content").html(data);
                            setTimeout(function () {
                                myApp.closeNotification(".notifications");
                            }, 2000);
                        }
                    }
                });
            }
        }, 500);
    });
});

//辅助程序

//上传位置信息
function uploadLocation(btn_id, location_id) {
    $$("#" + btn_id).on("click", function () {
        myApp.showIndicator();
        setTimeout(function () {
            if (!loc_success) {
                myApp.hideIndicator();
                myApp.alert("获取位置信息失败");
            }
        }, 2000);
        var loc_success = false;
        wx.getLocation({
            type: 'wgs84', // 默认为wgs84的gps坐标，如果要返回直接给openLocation用的火星坐标，可传入'gcj02'
            success: function (res) {
                var latitude = res.latitude; // 纬度，浮点数，范围为90 ~ -90
                var longitude = res.longitude; // 经度，浮点数，范围为180 ~ -180。
                var speed = res.speed; // 速度，以米/每秒计
                var accuracy = res.accuracy; // 位置精度
                var gps_location = longitude + "," + latitude;
                loc_success = true;
                $$("#" + btn_id).find(".item-title").children("i").remove();
                $$("#" + btn_id).find(".item-title").prepend("<i class='fa fa-check-circle color-green' aria-hidden='true'></i>");
                $$("#" + btn_id).find(".item-after").text("上传位置成功");
                $$("#" + location_id).val(gps_location);
                myApp.hideIndicator();
            }
        });
        return false;
    });
}

//上传图片信息
function uploadImage(img_id, img_filename) {
    $$("#" + img_id).on("click", function () {
        myApp.showIndicator();
        setTimeout(function () {
            if (!img_success) {
                myApp.hideIndicator();
                myApp.alert("上传图片失败");
            }
        }, 2000);
        var localIds
        wx.chooseImage({
            count: 1, // 默认9
            sizeType: ['compressed'], // 可以指定是原图还是压缩图，默认二者都有
            sourceType: ['album', 'camera'], // 可以指定来源是相册还是相机，默认二者都有
            success: function (res) {
                localIds = res.localIds; // 返回选定照片的本地ID列表，localId可以作为img标签的src属性显示图片
                wx.uploadImage({
                    localId: localIds[0], // 需要上传的图片的本地ID，由chooseImage接口获得
                    isShowProgressTips: 1, // 默认为1，显示进度提示
                    success: function (res) {
                        var serverId = res.serverId; // 返回图片的服务器端ID
                        var img_success = false;
                        $.ajax({
                            url: "/Seller/SaveOrignalImage",
                            type: "post",
                            data: {
                                serverId: serverId
                            },
                            success: function (data) {
                                if (data.result == "SUCCESS") {
                                    img_success = true;
                                    $$("#" + img_id).find(".item-title").children("i").remove();
                                    $$("#" + img_id).find(".item-title").prepend("<i class='fa fa-check-circle color-green' aria-hidden='true'></i>");
                                    $$("#" + img_id).find(".item-after").text("图片上传成功");
                                    $$("#" + img_filename).val(data.filename);
                                    myApp.hideIndicator();
                                }
                                else {
                                    myApp.alert("上传失败，请重试");
                                }
                            }
                        });
                    }
                });
            }
        });
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
    $$("#" + imglist).on("click", ".rep-imgitem", function (e) {
        var img_item = $$(this);
        $$(".rep-imgitem").each(function () {
            $$(this).html("");
        });
        img_item.html("<div class='rep-imgitem-selected'><i class='fa fa-minus'></i></div>");
    });
    $$("#" + imglist).on("click", ".rep-imgitem-selected", function () {
        myApp.confirm("是否确认删除已上传图片?", "提示", function () {
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
// 当前字数更新
function currentTextAreaLength(pagename, id_name, max_length, result_id) {
    var tl_c = $$("#" + id_name).val().length;
    $$("#" + result_id).text(tl_c);
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