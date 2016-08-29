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