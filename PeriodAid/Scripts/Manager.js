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
//tab-list
$$(".tab-link").on("click", function (data) {
    var url = $$(this).attr("data-href");
    mainView.router.load({ url: url, animatePages: false });
    $(this).addClass("active").siblings().removeClass("active")
});
//left-navbar


//Manager_Addchekin
$$(document).on("pageInit", ".page[data-page='manager-task-addchekin']", function (e) {
    var tl = textLength();
    $$("#manager-task-currentlength").text(tl);
    function textLength() {
        return $$("#manager-task-remark").val().length;
    }
    $$("#manager-task-remark").on("change", function () {
        var tl = textLength();
        var totalLength = $$("#manager-task-totallength").text();
        if (tl < totalLength) {
            $$("#manager-task-currentlength").text(tl);
        } else {
            myApp.alert("已超出最大值，请重新填写或删除部分信息")
        }
    });
});

//Manager_TaskReport
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

//Manager_CreateCheckIn
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

//Manager_Request_Create
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
//Senior_CheckInDetails
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
//Manager_ReportList
$$(document).on("pageInit", ".page[data-page='manager-reportlist']", function (e) {
    var calendarDefault = myApp.calendar({
        input: '#manager-reportlist-date',

    });
});
//Manager_EventList
$$(document).on("pageInit", ".page[data-page='manager-eventlist']", function (e) {
    var calendarDefault = myApp.calendar({
        input: '#manager-eventlist-date',
    });
});
//Mnaager_QuerySeller
$$(document).on("pageInit", ".page[data-page='manager-queryseller']", function () {
    var mySearchbar = myApp.searchbar('.searchbar', {
        searchList: '.list-block-search',
        searchIn: '.item-title'
    });
});
//Manager_BonusList
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
//Manager_TempSellerDetails
$$(document).on("pageInit", ".page[data-page='manager-tempsellerdetails']", function (e) {
    var phList = $("#sellertask-details-phlist").val().split(",");
    var photo = new Array();
    $$.each(phList, function (num,ph) {
        var url = "http://cdn2.shouquanzhai.cn/checkin-img/" + ph;
        var obj = { url: url };
        photo.push(obj);
    });
    console.log(photo);
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
//ManagerSellerTaskMonthStatistic
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
//ManangerSellerTaskSeller
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
//ManangerSellerTaskQuery
$$(document).on("pageInit", ".page[data-page='managersellertask-query']", function () {
    var mySearchbar = myApp.searchbar('.searchbar', {
        searchList: '.list-block-search',
        searchIn: '.item-content'
    });
});