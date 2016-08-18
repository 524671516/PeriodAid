var $$ = Dom7;

// Initialize app
var myApp = new Framework7();

// If we need to use custom DOM library, let's save it to $$ variable:
// Add view
var mainView = myApp.addView(".view-main", {
    // Because we want to use dynamic navbar, we need to enable it for this view:
    dynamicNavbar: true
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

//下拉刷新主界面数据
// Pull to refresh content
var ptrContent = $$('.pull-to-refresh-content');
ptrContent.on("refresh", function (e) {
    setTimeout(function () {

        // When loading done, we need to reset it
        myApp.pullToRefreshDone();
    },2000);
});

//Seller_ConfirmedData 数据表单 滚动循环
$$(document).on("pageInit", ".page[data-page='seller-confirmeddata']", function () {

});

//Seller_CreditInfo 修改个人信息 提交
$$(document).on("pageInit", ".page[data-page='seller-creditinfo']", function () {
  
});