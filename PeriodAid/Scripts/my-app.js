﻿
// Initialize your app
// Initialize app
var myApp = new Framework7({
    cacheIgnore: ["/SellerTask/UpdateAccountInfo"],
    cacheIgnoreGetParameters: true
});

// If we need to use custom DOM library, let's save it to $$ variable:
var $$ = Framework7.$;

// Add view
var mainView = myApp.addView('.view-main', {
    // Because we want to use dynamic navbar, we need to enable it for this view:
    dynamicNavbar: true
});

refresh_userpanel();
$("input.error").parent("div").addClass("custom-error");


wx.config({
    debug: true, // 开启调试模式,调用的所有api的返回值会在客户端alert出来，若要查看传入的参数，可以在pc端打开，参数信息会通过log打出，仅在pc端时才会打印。
    appId: $("#appId").text(), // 必填，公众号的唯一标识
    timestamp: $("#timeStamp").text(), // 必填，生成签名的时间戳
    nonceStr: $("#nonce").text(), // 必填，生成签名的随机串
    signature: $("#signature").text(),// 必填，签名，见附录1
    jsApiList: ['uploadImage', 'downloadImage', 'chooseImage', 'getLocation', 'previewImage']
});

//下拉刷新
$$(document).on('ajaxStart', function (e) {
    if (e.detail.xhr.requestUrl.indexOf('autocomplete-languages.json') >= 0) { return; }
    myApp.showIndicator();
});
$$(document).on('ajaxComplete', function (e) {
    if (e.detail.xhr.requestUrl.indexOf('autocomplete-languages.json') >= 0) { return; }
    myApp.hideIndicator();
});

// 更新账户信息
$$(document).on("pageInit", ".page[data-page='UpdateAccountInfo']", function (e) {
    //mainView.router.refreshPage();

    $("#updateaccountinfo-form").validate({
        debug: true, //调试模式取消submit的默认提交功能   
        errorClass: "custom-error", //默认为错误的样式类为：error   
        focusInvalid: false, //当为false时，验证无效时，没有焦点响应  
        onkeyup: false,
        submitHandler: function (form) {   //表单提交句柄,为一回调函数，带一个参数：form
            $("#updateaccountinfo-submit").prop("disabled", true).addClass("color-gray");

            $("#updateaccountinfo-form").ajaxSubmit(function (data) {
                if (data == "SUCCESS") {
                    myApp.hideIndicator();
                    mainView.router.back();
                    myApp.addNotification({
                        title: '通知',
                        message: '表单提交成功'
                    });
                    setTimeout(function () {
                        myApp.closeNotification(".notifications");
                    }, 2000);
                }
                else {
                    myApp.hideIndicator();
                    myApp.addNotification({
                        title: '通知',
                        message: '表单提交失败'
                    });
                    $("#updateaccountinfo-submit").prop("disabled", false).removeClass("color-gray");
                    setTimeout(function () {
                        myApp.closeNotification(".notifications");
                    }, 2000);
                }
            });
        },
        rules: {
            IdNumber: {
                required: true,
                idnumber: true
            },
            CardName: {
                required: true
            },
            AccountSource: {
                required: true
            },
            AccountName: {
                required: true
            },
            CardNo: {
                required: true
            }
        },
        messages: {
            IdNumber: {
                required: "必填",
                idnumber: "请正确填写身份证号码"
            },
            CardName: {
                required: "必填"
            },
            AccountSource: {
                required: "必填"
            },
            AccountName: {
                required: "必填"
            },
            CardNo: {
                required: "必填"
            }
        },
        errorPlacement: function (error, element) {
            myApp.hideIndicator();
            element.attr("placeholder", error.text());
        }
    });
    $$("#updateaccountinfo-submit").click(function () {
        myApp.showIndicator();
        $("#updateaccountinfo-form").submit();
    });
});

// 上传信息页面
$$(document).on("pageInit", ".page[data-page='SellerReport']", function (e) {
    $("#sellerreport-imglist").html("");
    var photolist = splitArray($("#TaskPhotoList").val());
    for (var i = 0; i < photolist.length; i++) {
        $("#sellerreport-imglist").append("<li><div class=\"rep-imgitem\" data-rel='" + photolist[i] + "' style=\"background-image:url(/Seller/ThumbnailImage?filename=" + photolist[i] + "); background-size:cover\"></div></li>");
    }
    $("#sellerreport-imglist").append("<li><a href=\"javascript:;\" class=\"rep-imgitem-btn\" id=\"upload-btn\"><i class=\"fa fa-plus\"></i></a></li>");

    // 上传文件
    $$("#sellerreport-imglist").on("click", "#upload-btn", function (e) {
        var localIds
        var btn = $(this);
        wx.chooseImage({
            count: 1, // 默认9
            sizeType: ['compressed'], // 可以指定是原图还是压缩图，默认二者都有
            sourceType: ['album', 'camera'], // 可以指定来源是相册还是相机，默认二者都有
            success: function (res) {
                localIds = res.localIds; // 返回选定照片的本地ID列表，localId可以作为img标签的src属性显示图片
                $("#preview").attr("src", localIds);
                wx.uploadImage({
                    localId: localIds[0], // 需要上传的图片的本地ID，由chooseImage接口获得
                    isShowProgressTips: 1, // 默认为1，显示进度提示
                    success: function (res) {
                        var serverId = res.serverId; // 返回图片的服务器端ID
                        $.ajax({
                            url: "/Seller/SaveOrignalImage",
                            type: "post",
                            data: {
                                serverId: serverId
                            },
                            success: function (data) {
                                if (data.result == "SUCCESS") {
                                    $("#sellerreport-imglist").html("");
                                    var photolist = splitArray($("#TaskPhotoList").val());
                                    photolist.push(data.filename);
                                    $("#TaskPhotoList").val(photolist.toString());
                                    for (var i = 0; i < photolist.length; i++) {
                                        $("#sellerreport-imglist").append("<li><div class=\"rep-imgitem\" data-rel='" + photolist[i] + "' style=\"background-image:url(/Seller/ThumbnailImage?filename=" + photolist[i] + "); background-size:cover\"></div></li>");
                                    }

                                    $("#sellerreport-imglist").append("<li><a href=\"javascript:;\" class=\"rep-imgitem-btn\" id=\"upload-btn\"><i class=\"fa fa-plus\"></i></a></li>");
                                }
                                else {
                                    alert("上传失败，请重试");
                                }
                            }
                        });
                    }
                });
            }
        });
    });

    // 删除图片
    $$("#sellerreport-imglist").on("click", ".rep-imgitem", function (e) {
        var img_item = $$(this);
        $$(".rep-imgitem").each(function () {
            $$(this).html("");
        });
        img_item.html("<div class='rep-imgitem-selected'><i class='fa fa-minus'></i></div>");
    });
    $$("#sellerreport-imglist").on("click", ".rep-imgitem-selected", function () {
        
        myApp.confirm('是否确认删除已上传图片?', '提示', function () {
            //myApp.alert('You clicked Ok button');
            var delete_item = $(".rep-imgitem-selected").closest(".rep-imgitem").attr("data-rel");
            var arraylist = splitArray($("#TaskPhotoList").val());
            var pos =$.inArray(delete_item, arraylist);
            arraylist.splice(pos, 1);
            $("#TaskPhotoList").val(arraylist.toString());
            $("#sellerreport-imglist").html("");
            for (var i = 0; i < arraylist.length; i++) {
                $("#sellerreport-imglist").append("<li><div class=\"rep-imgitem\" data-rel='" + arraylist[i] + "' style=\"background-image:url(/Seller/ThumbnailImage?filename=" + arraylist[i] + "); background-size:cover\"></div></li>");
            }

            $("#sellerreport-imglist").append("<li><a href=\"javascript:;\" class=\"rep-imgitem-btn\" id=\"upload-btn\"><i class=\"fa fa-plus\"></i></a></li>");
        });
    });

});

$$(document).on('pageInit', '.page[data-page="refresh"]', function (e) {
    var songs = ['Yellow Submarine', 'Don\'t Stop Me Now', 'Billie Jean', 'Californication'];
    var authors = ['Beatles', 'Queen', 'Michael Jackson', 'Red Hot Chili Peppers'];

    // 下拉刷新页面
    var ptrContent = $$('.pull-to-refresh-content');

    // 添加'refresh'监听器
    ptrContent.on('refresh', function (e) {
        // 模拟2s的加载过程
        setTimeout(function () {
            // 随机图片
            var picURL = 'http://hhhhold.com/88/d/jpg?' + Math.round(Math.random() * 100);
            // 随机音乐
            var song = songs[Math.floor(Math.random() * songs.length)];
            // 随机作者
            var author = authors[Math.floor(Math.random() * authors.length)];
            // 列表元素的HTML字符串
            var itemHTML = '<li class="item-content">' +
                              '<div class="item-media"><img src="' + picURL + '" width="44"/></div>' +
                              '<div class="item-inner">' +
                                '<div class="item-title-row">' +
                                  '<div class="item-title">' + song + '</div>' +
                                '</div>' +
                                '<div class="item-subtitle">' + author + '</div>' +
                              '</div>' +
                            '</li>';
            // 前插新列表元素
            ptrContent.find('ul').prepend(itemHTML);
            // 加载完毕需要重置
            myApp.pullToRefreshDone();
        }, 2000);
    });
});
//end
//无限循环
$$(document).on('pageInit', '.page[data-page="infinitescroll"]', function (e) {
    var loading = false;//加载flag
    var lastIndex = $$(".list-card li").length;//上次加载的序号
    var maxItem = 30;//最大可以增加的条数
    var itemPerload = 10;//每次可增加的条数
    $$(".infinite-scroll").on("infinite", function (e) {
        if (loading) return;
        loading = true;
        setTimeout(function () {
            loading = false;//重置flag
            if (lastIndex >= maxItem) {
                myApp.detachInfiniteScroll($$(".infinite-scroll"))//关闭滚动
                $$(".infinite-scroll-preloader").remove();//移除加载符
                $$(".infinite-pre").removeClass("hidden");
                return;
            };
            //生成新的条目
            itemList = '';
            for (var i = lastIndex + 1; i <= lastIndex + itemPerload; i++) {
                itemList += "<li class='card'><div class='card-content demo-card-header-pic'><div style='background-image: url('../../Content/images/img_" + i + ".jpg');' class='card-header'></div><div class='card-content-inner'><p class='color-gray'>2016-07-06</p></div></div></li>";
            };
            $$(".list-card").append(itemList);//添加
            lastIndex = $$(".list-card li").length//新的条数
        }, 1000)
    });
});
//history
$$(document).on("pageInit", ".page[data-page='history']", function () {
    //时间
    var calendarDateFormat = myApp.calendar({
        input: '#history-calender',
        dateFormat: 'yyyy-mm-dd'
    });
    $$("#history-calender").on("change", function () {

    })
});
//search
$$(document).on("pageInit", ".page[data-page='search']", function () {
    //搜索促销员
    var mySearchbar = myApp.searchbar('.searchbar', {
        searchList: '.list-block-search',
        searchIn: '.item-title'
    });
    //促销员详情
    $$(".item-link").on("click", function () {
        var $$btn = $$(this);
    });
});
//ManagerSystem-Patrol
$$(document).on("pageInit", ".page[data-page='managersystem-patrol']", function () {
    //时间
    var calendarDateFormat = myApp.calendar({
        input: '#calendar-default',
        dateFormat: 'yyyy-mm-dd'
    });
    //无限循环
    var loading = false;//加载flag
    var lastIndex = $$(".list-card li").length;//上次加载的序号
    var maxItem = 30;//最大可以增加的条数
    var itemPerload = 10;//每次可增加的条数
    $$(".infinite-scroll").on("infinite", function () {
        if (loading) return;
        loading = true;
        setTimeout(function () {
            loading = false;//重置flag
            if (lastIndex >= maxItem) {
                myApp.detachInfiniteScroll($$(".manager-patrol-infinitescroll"))//关闭滚动
                $$(".infinite-scroll-preloader").remove();//移除加载符
                $$(".infinite-pre").removeClass("hidden");
                return;
            };
            //生成新的条目
            itemList = '';
            for (var i = lastIndex + 1; i <= lastIndex + itemPerload; i++) {
                itemList += "<li class='card'><div class='card-content demo-card-header-pic'><div style='background-image: url('../../Content/images/img_" + i + ".jpg');' class='card-header'></div><div class='card-footer'><span>大润发-江山店</span><span class='manager-patrol-date'>2016-07-07</span></div></div></li>";
            };
            $$(".list-card").append(itemList);//添加
            lastIndex = $$(".list-card li").length//新的条数
        }, 1000);
    });
    //查询
    $$("#calendar-default").on("change", function () {
        var date = $$("#calendar-default").val();
    });
});
//ManagerSystem_Warning
$$(document).on("pageInit", ".page[data-page='managersystem-warning']", function () {
    $$('.manager-warning-open').on('click', function () {
        myApp.pickerModal('.picker-info')
    });
})

// 用户模板更新
function refresh_userpanel() {
    $$.ajax({
        url: "/SellerTask/UserInfoPartial",
        method: "POST",
        success: function (data) {
            if (data != "Error")
                $$("#left-user-panel").html(data);
        }
    });
}

// 数组分离
function splitArray(value) {
    var list = new Array();
    if (value.trim() != "") {
        list = value.trim().split(',');
        return list;
    }
    return list;
}
//SellerTaskList
$$(document).on("pageInit", ".page[data-page='sellertasklist]", function (e) {
    alert("22")
    $$(function () {
        $$.ajax({
            url: "/SellerTask/SellerTaskListPartial",
            success: function (data) {
                $$("#sellertask-list").html(data)
            }
        });
        return false;
    })
});