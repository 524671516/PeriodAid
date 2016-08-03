var $$ = Dom7;
// Initialize app
var myApp = new Framework7();

// If we need to use custom DOM library, let's save it to $$ variable:


// Add view
var mainView = myApp.addView('.view-main', {
    // Because we want to use dynamic navbar, we need to enable it for this view:
    dynamicNavbar: true
});
$$(document).on('ajaxStart', function (e) {
    if (e.detail.xhr.requestUrl.indexOf('autocomplete-languages.json') >= 0) { return; }
    myApp.showIndicator();
});
$$(document).on('ajaxComplete', function (e) {
    if (e.detail.xhr.requestUrl.indexOf('autocomplete-languages.json') >= 0) { return; }
    myApp.hideIndicator();
});
var monthNames= ['一月份', '二月份', '三月份', '四月份', '五月份', '六月份', '七月份', '八月份', '九月份', '十月份', '十一月份', '十二月份'];
var monthNamesShort= ['一月', '二月', '三月', '四月', '五月', '六月', '七月', '八月', '九月', '十月', '十一月', '十二月'];
var dayNames = ['星期日', '星期一', '星期二', '星期三', '星期四', '星期五', '星期六'];
var dayNamesShort = ['日', '一', '二', '三', '四', '五', '六'];
//tab-list 底部工具栏转换
$$(".tab-link").on("click", function (data) {
    var url = $$(this).attr("data-href");
    mainView.router.load({ url: url, animatePages: false });
    $(this).addClass("active").siblings().removeClass("active")
});
refresh_userpanel();
//left-navbar

// 微信初始化
wx.config({
    debug: true, // 开启调试模式,调用的所有api的返回值会在客户端alert出来，若要查看传入的参数，可以在pc端打开，参数信息会通过log打出，仅在pc端时才会打印。
    appId: $("#appId").text(), // 必填，公众号的唯一标识
    timestamp: $("#timeStamp").text(), // 必填，生成签名的时间戳
    nonceStr: $("#nonce").text(), // 必填，生成签名的随机串
    signature: $("#signature").text(),// 必填，签名，见附录1
    jsApiList: ['uploadImage', 'downloadImage', 'chooseImage', 'getLocation', 'previewImage']
});




//Manager_Addchekin 添加签到信息 填写备注信息字数提示
$$(document).on("pageInit", ".page[data-page='manager-task-addchekin']", function (e) {
    // 获取当前备注文本长度
    currentTextAreaLength("Remark", 50, "checkin-currentlength");

    // 显示所有的已上传图片
    uploadCheckinFile("manager-imglist", "Photo", "current_image", 3);
    uploadLocation("location-btn", "Location");

    $("#addcheckin_form").validate({
        debug: true, //调试模式取消submit的默认提交功能   
        errorClass: "custom-error", //默认为错误的样式类为：error   
        focusInvalid: false, //当为false时，验证无效时，没有焦点响应  
        onkeyup: false,
        submitHandler: function (form) {
            $("#addcheckin-btn").prop("disabled", true).addClass("color-gray");
            var array = splitArray($("#Photo").val());
            if (array.length == 0) {
                myApp.hideIndicator();
                myApp.alert("请至少上传一张图片");
                $("#addcheckin-btn").prop("disabled", false).removeClass("color-gray");
            }
            else if($("#Location").val().trim() == "")
            {
                myApp.hideIndicator();
                myApp.alert("请上传您的地理位置");
                $("#addcheckin-btn").prop("disabled", false).removeClass("color-gray");
            }
            else{
                $("#addcheckin_form").ajaxSubmit(function (data) {
                    if (data == "SUCCESS") {
                        myApp.hideIndicator();
                        //myApp.formDeleteData("createsellerreport-form");
                        mainView.router.back();
                        myApp.addNotification({
                            title: '通知',
                            message: '表单提交成功'
                        });
                        setTimeout(function () {
                            //refresh_mainpanel();
                            myApp.closeNotification(".notifications");
                        }, 2000);
                    }
                    else {
                        myApp.hideIndicator();
                        myApp.addNotification({
                            title: '通知',
                            message: '表单提交失败'
                        });
                        $("#addcheckin-btn").prop("disabled", false).removeClass("color-gray");
                        //refresh_mainpanel();
                        setTimeout(function () {
                            myApp.closeNotification(".notifications");
                        }, 2000);
                    }
                });
            }
        },
        errorPlacement: function (error, element) {
            myApp.hideIndicator();
            element.attr("placeholder", error.text());
        }
    });
    $$("#addcheckin-btn").click(function () {
        myApp.showIndicator();
        $("#addcheckin_form").submit();
    });

});



//Manager_TaskReport 督导工作日报 填写内容字数提示
$$(document).on("pageInit", ".page[data-page='manager-task-report']", function () {
    textLength();
    function textLength() {
        $$("#manager-task-currentlength-cp").text($$("#manager-task-evencomplete").val().length);
        $$("#manager-task-currentlength-uc").text($$("#manager-task-evenuncomplete").val().length);
        $$("#manager-task-currentlength-as").text($$("#manager-task-evenassistent").val().length);
    };
    function T(event, $$length) {
        var totalLength = $$(".manager-task-total").text();
        if (event < totalLength) {
            $$length.text(event);
        } else {
            myApp.alert("已超出最大值，请重新填写或删除部分信息")
        }
    }
    $$("#manager-task-evencomplete").on("change", function () {
        var $$length = $$("#manager-task-currentlength-cp");
        var event = $$(this).val().length;
        T(event, $$length)
    });
    $$("#manager-task-evenuncomplete").on("change", function () {
        var $$length = $$("#manager-task-currentlength-uc");
        var event = $$(this).val().length;
        T(event,$$length)
    });
    $$("#manager-task-evenassistent").on("change", function () {
        var $$length = $$("#manager-task-currentlength-as");
        var event = $$(this).val().length;
        T(event,$$length)
    });
});

//Manager_CreateCheckIn 代提报销量  填写备注信息字数提示
$$(document).on("pageInit", ".page[data-page='manager-temp-createcheckin']", function () {
    var tl = textLength();
    $$("#manager-temp-notecurrentlength").text(tl);
    function textLength() {
        return $$("#manager-temp-notecontent").val().length;
    }
    $$("#manager-temp-notecontent").on("change", function () {
        var tl = textLength();
        var totalLength = $$("#manager-temp-notetotallength").text();
        if (tl < totalLength) {
            $$("#manager-temp-notecurrentlength").text(tl);
        } else {
            myApp.alert("已超出最大值，请重新填写或删除部分信息")
        }
    });
});

//Manager_Request_Create 店铺需求提报 填写需求信息字数提示
$$(document).on("pageInit", ".page[data-page='manager-task-requestcreate']", function () {
    textLength();
    function textLength() {
        $$("#manager-task-requestcreate-contentlength").text($$("#manager-task-requestcreate-content").val().length);
        $$("#manager-task-requestcreate-remarktlength").text($$("#manager-task-requestcreate-remark").val().length);
    };
    function T(event, totalLength, $$length) {
        if (event < totalLength) {
            $$length.text(event);
        } else {
            myApp.alert("已超出最大值，请重新填写或删除部分信息")
        }
    }
    $$("#manager-task-requestcreate-content").on("change", function () {
        var totalLength = $$("#manager-task-requestcreate-contenttotal").text();
        var $$length = $$("#manager-task-requestcreate-contentlength");
        var event = $$(this).val().length;
        T(event, totalLength, $$length)
    });
    $$("#manager-task-requestcreate-remark").on("change", function () {
        var totalLength = $$("#manager-task-requestcreate-remarktotal").text();
        var $$length = $$("#manager-task-requestcreate-remarklength");
        var event = $$(this).val().length;
        T(event, totalLength, $$length)
    });
});
//Senior_CheckInDetails  查看其他人签到信息  图片查看
$$(document).on("pageInit", ".page[data-page='manager-chekindetails']", function () {
    var myPhotoManagerChekin = myApp.photoBrowser({
        photos: [
            '/Content/images/guide-02-3.jpg'
        ],
        theme: 'dark',
        type: 'standalone',
        lazyLoading: true,
        zoom: false,
        backLinkText: '关闭'
    });
    var myPhotoManagerSeller = myApp.photoBrowser({
        photos: [
            '/Content/images/guide-02-2.jpg'
        ],
        theme: 'dark',
        type: 'standalone',
        lazyLoading: true,
        zoom: false,
        backLinkText: '关闭'
    });
    $$('.manager-chekinphoto').on('click', function () {
        myPhotoManagerChekin.open();
    });
    $$('.manager-sellerphoto').on('click', function () {
        myPhotoManagerSeller.open();
    });
});
//Manager_ReportList  销量排名 查看日期
$$(document).on("pageInit", ".page[data-page='manager-reportlist']", function (e) {
    var calendarDefault = myApp.calendar({
        input: '#manager-reportlist-date',
        monthNames: monthNames,
        monthNamesShort:monthNamesShort,
        dayNames: dayNames,
        dayNamesShort:dayNamesShort
    });
});
//Manager_EventList  活动门店列表  查看日期
$$(document).on("pageInit", ".page[data-page='manager-eventlist']", function (e) {
    var calendarDefault = myApp.calendar({
        input: '#manager-eventlist-date',
        monthNames: monthNames,
        monthNamesShort: monthNamesShort,
        dayNames: dayNames,
        dayNamesShort: dayNamesShort
    });
    var url = "/Seller/Manager_EventListPartial";
    var date = $$("#manager-eventlist-date").val();
    $$.ajax({
        url: url,
        data: {
            date:date
        },
        success: function (data) {
            $$("#manager-eventlist-content").html(data);
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
                $$("#manager-eventlist-content").html(data);
            }
        });
    });
});
//Mnaager_QuerySeller  搜索促销员
$$(document).on("pageInit", ".page[data-page='manager-queryseller']", function () {
    var mySearchbar = myApp.searchbar('.searchbar', {
        searchList: '.list-block-search',
        searchIn: '.item-content'
    });
});
//Manager_BonusList  红包列表 下拉刷新  
$$(document).on("pageInit", ".page[data-page='manager-bonuslist']", function () {
    var songs = ['Yellow Submarine', 'Don\'t Stop Me Now', 'Billie Jean', 'Californication'];
    var authors = ['Beatles', 'Queen', 'Michael Jackson', 'Red Hot Chili Peppers'];

    // Pull to refresh content
    var ptrContent = $$('.pull-to-refresh-content');

    // Add 'refresh' listener on it
    ptrContent.on('refresh', function (e) {
        // Emulate 2s loading
        setTimeout(function () {
            // Random song
            var song = songs[Math.floor(Math.random() * songs.length)];
            // Random author
            var author = authors[Math.floor(Math.random() * authors.length)];
            // List item html
            var itemHTML = '<li class="item-content">' +
                              '<div class="item-inner">' +
                                '<div class="item-title-row">' +
                                  '<div class="item-title">' + song + '</div>' +
                                '</div>' +
                                '<div class="item-subtitle">' + author + '</div>' +
                              '</div>' +
                            '</li>';
            // Prepend new list element
            ptrContent.find('ul').prepend(itemHTML);
            // When loading done, we need to reset it
            myApp.pullToRefreshDone();
        }, 2000);
    });
});
//Manager_TempSellerDetails  暗促系统  暗促签到图片查看
$$(document).on("pageInit", ".page[data-page='manager-tempsellerdetails']", function (e) {
    var phList = $("#sellertask-details-phlist").val().split(",");
    var photo = new Array();
    $$.each(phList, function (num,ph) {
        var url = "http://cdn2.shouquanzhai.cn/checkin-img/" + ph;
        var obj = { url: url };
        photo.push(obj);
    });
    var myPhotoBrowserPopupDark = myApp.photoBrowser({
        photos: photo,
        theme: 'dark',
        type: 'standalone',
        lazyLoading: true,
        zoom: false,
        backLinkText: '关闭'
    });
    $$('.ph-tempseller').on('click', function (e) {
        myPhotoBrowserPopupDark.open();
    });
});
//ManagerSellerTaskMonthStatistic 暗促系统  暗促信息查询
$$(document).on("pageInit", ".page[data-page='manager-sellertask-month']", function () {
    $$.ajax({
        url: "/Seller/ManagerSellerTaskMonthStatisticPartial",
        data:{
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
    })
});
//ManangerSellerTaskSeller 暗促系统  暗促签到列表  无限循环
$$(document).on("pageInit", ".page[data-page='managerseller-taskdate']", function (e) {
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
    var loading = false;//加载flag
    $$(".infinite-scroll").on("infinite", function (e) {
        $$(".infinite-scroll-preloader").removeClass("hidden");
        if (loading) return;
        loading = true;
        setTimeout(function () {
            loading = false;//重置flag

            //生成新的条目
            $$.ajax({
                url: url,
                data: {
                    page: page,
                    id: $("#sellerid").val()
                },
                success: function (data) {
                    if (data == "NONE"|| data == "FAIL") {
                        myApp.detachInfiniteScroll($$(".infinite-scroll"))//关闭滚动
                            $$(".infinite-scroll-preloader").remove();//移除加载符
                            $$(".infinite-pre").removeClass("hidden");
                            return;
                    }
                    else {
                       $$("#managerseller-list").append(data);
                        page++;
                    }
                }
            });
        }, 1000)
    });
});
//ManangerSellerTaskQuery  暗促信息查询
$$(document).on("pageInit", ".page[data-page='managersellertask-query']", function () {
    var mySearchbar = myApp.searchbar('.searchbar', {
        searchList: '.list-block-search',
        searchIn: '.item-content'
    });
});

//Manager_StoreList  督导管理门店  查询
$$(document).on("pageInit", ".page[data-page='manager-storelist']", function () {
    var mySearchbar = myApp.searchbar('.searchbar', {
        searchList: '.list-block-search',
        searchIn: '.item-content'
    });
});

//Manager_CheckInView 查看签到信息
$$(document).on("pageInit", ".page[data-page='manager-chekinview']", function () {
    $$.ajax({
        url: "/Seller/Manager_CheckInViewPartial",
        data: {
            id: $$("#task_id").val()
        },
        success: function (data) {
            $$("#manager-checkinview-content").html(data);
        }
    });
    $$("#task_id").on("change", function () {
        $$.ajax({
            url: "/Seller/Manager_CheckInViewPartial",
            data: {
                id: $$("#task_id").val()
            },
            success: function (data) {
                $$("#manager-checkinview-content").html(data);
            }
        });
    });
    $$("#manager-checkinview-content").on("click", ".swipeout-delete", function () {
        myApp.confirm("是否删除该信息？", function () {
            $$.ajax({
                url: "/Seller/Mananger_CancelManagerCheckin",
                method: "POST",
                data: {
                    id: $$(".swipeout-delete").attr("data-url")
                },
                success: function (res) {
                    var data = JSON.parse(res);
                    if (data.result == "SUCCESS") {
                        $$.ajax({
                            url: "/Seller/Manager_CheckInViewPartial",
                            data: {
                                id: $$("#task_id").val()
                            },
                            success: function (data) {
                                $$("#manager-checkinview-content").html(data);
                            }
                        });
                    }
                }
            });
        });
    });
});


// 辅助程序

// 用户模板更新
function refresh_userpanel() {
    $$.ajax({
        url: "/Seller/Manager_UserPanel",
        method: "POST",
        success: function (data) {
            if (data != "Error")
                $$("#manager_userpanel").html(data);
        }
    });
}

// 当前字数更新
function currentTextAreaLength(id_name, max_length, result_id) {
    
    $$("#" + id_name).on("change", function () {
        var tl = $$("#" + id_name).val().length;
        if (tl < max_length) {
            $$("#" + result_id).text(tl);
        } else {
            myApp.alert("已超出最大值，请重新填写或删除部分信息")
            var str = $$("#" + id_name).val();
            $$("#" + id_name).val(str.slice(0, 50));
            $$("#" + result_id).text("50");
        }
});
}

// 上传签到图片文件模块
function uploadCheckinFile(imglist, photolist_id, current_count, max_count) {
    $$("#"+imglist).html("");
    var photolist = splitArray($$("#"+photolist_id).val());
    $$("#"+current_count).text(photolist.length);
    for (var i = 0; i < photolist.length; i++) {
        $$("#"+imglist).append("<li><div class=\"rep-imgitem\" data-rel='" + photolist[i] + "' style=\"background-image:url(/Seller/ThumbnailImage?filename=" + photolist[i] + "); background-size:cover\"></div></li>");
    }
    $$("#"+imglist).append("<li><a href=\"javascript:;\" class=\"rep-imgitem-btn\" id=\"upload-btn\"><i class=\"fa fa-plus\"></i></a></li>");

    // 上传文件
    $$("#" + imglist).on("click", "#upload-btn", function (e) {
        var localIds;
        var photolist = splitArray($("#"+photolist_id).val());
        if (photolist.length < max_count) {
            wx.chooseImage({
                count: 1, // 默认9
                sizeType: ['compressed'], // 可以指定是原图还是压缩图，默认二者都有
                sourceType: ['album', 'camera'], // 可以指定来源是相册还是相机，默认二者都有
                success: function (res) {
                    localIds = res.localIds; // 返回选定照片的本地ID列表，localId可以作为img标签的src属性显示图片
                    //$("#preview").attr("src", localIds);
                    wx.uploadImage({
                        localId: localIds[0], // 需要上传的图片的本地ID，由chooseImage接口获得
                        isShowProgressTips: 1, // 默认为1，显示进度提示
                        success: function (res) {
                            var serverId = res.serverId; // 返回图片的服务器端ID
                            alert(serverId);
                            $$.ajax({
                                url: "/Seller/SaveOrignalImage",
                                type: "post",
                                data: {
                                    serverId: serverId
                                },
                                success: function (resource) {
                                    data = JSON.parse(resource);
                                    //alert(data.result);
                                    if (data.result == "SUCCESS") {
                                        $$("#"+imglist).html("");
                                        photolist.push(data.filename);
                                        $$("#"+current_count).text(photolist.length);
                                        $$("#"+photolist_id).val(photolist.toString());
                                        for (var i = 0; i < photolist.length; i++) {
                                            $$("#"+imglist).append("<li><div class=\"rep-imgitem\" data-rel='" + photolist[i] + "' style=\"background-image:url(/Seller/ThumbnailImage?filename=" + photolist[i] + "); background-size:cover\"></div></li>");
                                        }
                                        $$("#"+imglist).append("<li><a href=\"javascript:;\" class=\"rep-imgitem-btn\" id=\"upload-btn\"><i class=\"fa fa-plus\"></i></a></li>");
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
        }
        else {
            myApp.alert("上传图片不得大于" + max_count + "张，无法添加");
        }
    });

    // 删除图片
    $$("#"+imglist).on("click", ".rep-imgitem", function (e) {
        var img_item = $$(this);
        $$(".rep-imgitem").each(function () {
            $$(this).html("");
        });
        img_item.html("<div class='rep-imgitem-selected'><i class='fa fa-minus'></i></div>");
    });
    $$("#"+imglist).on("click", ".rep-imgitem-selected", function () {
        myApp.confirm('是否确认删除已上传图片?', '提示', function () {
            //myApp.alert('You clicked Ok button');
            var delete_item = $(".rep-imgitem-selected").closest(".rep-imgitem").attr("data-rel");
            var arraylist = splitArray($("#"+photolist_id).val());
            var pos = $.inArray(delete_item, arraylist);
            arraylist.splice(pos, 1);
            $$("#"+photolist_id).val(arraylist.toString());
            $$("#"+current_count).text(arraylist.length);
            $$("#"+imglist).html("");
            for (var i = 0; i < arraylist.length; i++) {
                $("#"+imglist).append("<li><div class=\"rep-imgitem\" data-rel='" + arraylist[i] + "' style=\"background-image:url(/Seller/ThumbnailImage?filename=" + arraylist[i] + "); background-size:cover\"></div></li>");
            }
            $$("#"+imglist).append("<li><a href=\"javascript:;\" class=\"rep-imgitem-btn\" id=\"upload-btn\"><i class=\"fa fa-plus\"></i></a></li>");
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
        }, 4000);
        
        var loc_success = false;
        wx.getLocation({
            type: 'wgs84', // 默认为wgs84的gps坐标，如果要返回直接给openLocation用的火星坐标，可传入'gcj02'
            success: function (res) {
                var latitude = res.latitude; // 纬度，浮点数，范围为90 ~ -90
                var longitude = res.longitude; // 经度，浮点数，范围为180 ~ -180。
                var speed = res.speed; // 速度，以米/每秒计
                var accuracy = res.accuracy; // 位置精度
                var gps_location = longitude + "," + latitude;
                //alert(location)
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

function splitArray(value) {
    var list = new Array();
    if(value!=null){
        if (value.trim() != "") {
            list = value.trim().split(',');
            return list;
        }
    }
    return list;
}
