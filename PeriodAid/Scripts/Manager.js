var $$ = Dom7;
// Initialize app
var myApp = new Framework7({
    cache: false
});

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

$$(".tab-link").on("click", function (data) {
    var url = $$(this).attr("data-href");
    mainView.router.load({ url: url, animatePages: false });
    $(this).addClass("active").siblings().removeClass("active")
});
//Senior_CheckInDetails
$$(document).on("pageInit", ".page[data-page='manager-chekindetails']", function () {
    var myPhotoBrowserPopupDark = myApp.photoBrowser({
        photos: [
            '/Content/images/guide-02-3.jpg'
        ],
        theme: 'dark',
        type: 'standalone',
        lazyLoading: true,
        zoom: false,
        backLinkText: '关闭'
    });
    $$('.manager-chekinphoto').on('click', function () {
        myPhotoBrowserPopupDark.open();
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
        var url = "/Content/images/" + ph;
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