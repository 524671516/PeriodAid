
// Initialize your app
// Initialize app
var myApp = new Framework7();

// If we need to use custom DOM library, let's save it to $$ variable:
var $$ = Framework7.$;

// Add view
var mainView = myApp.addView('.view-main', {
    // Because we want to use dynamic navbar, we need to enable it for this view:
    dynamicNavbar: true
});
//下拉刷新
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