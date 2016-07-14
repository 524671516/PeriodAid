// Initialize your app
// Initialize app
var myApp = new Framework7({
    pushState: true,
    swipePanel: 'left',
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

//index 下拉刷新时间
var numJsons = $$(".num").text();
// 下拉刷新页面
var ptrContent = $$('.pull-to-refresh-content');

// 添加'refresh'监听器
ptrContent.on('refresh', function (e) {
    // 模拟2s的加载过程
    setTimeout(function () {
        // 随机事件
        var numJson = numJsons++;
        // 前插新列表元素
        ptrContent.find('.num').text(numJson);
        // 加载完毕需要重置
        myApp.pullToRefreshDone();
    }, 2000);
});
//使用指南 图片浏览器
var myPhotoBrowserPopupDark = myApp.photoBrowser({
    photos: [
        {
            url: '/Content/images/sellertask-guide-01.jpg',
            caption:'促销管理系统'
        },
        {
            url: '/Content/images/sellertask-guide-02.jpg',
            caption:'个人信息'
        },
        {
            url: '/Content/images/sellertask-guide-03.jpg',
            caption:'每日任务'
        },
        {
            url: '/Content/images/sellertask-guide-04.jpg',
            caption:'历史记录'
        },
        {
            url: '/Content/images/sellertask-guide-05.jpg',
            caption:'库存详情'
        },
        {
            url: '/Content/images/sellertask-guide-06.jpg',
            caption:'修改记录'
        },
        {
            url: '/Content/images/sellertask-guide-07.jpg',
            caption:'提交修改'
        },
        {
            url: '/Content/images/sellertask-guide-08.jpg',
            caption:'完善信息'
        }
    ],
    theme: 'dark',
    type: 'standalone',
    lazyLoading:true,
    zoom: false,
    backLinkText:'关闭'
});
$$('.sellertask-guide').on('click', function () {
    myPhotoBrowserPopupDark.open();
});
//SellerTaskList
$$(document).on("pageInit", ".page[data-page='sellertasklist']", function (e) {
    var currentpage = 1;
    var lastIndex = $$(".seller-list li").length;//上次加载的序号
    $$.ajax({
        url: "/SellerTask/SellerTaskListPartial",
        data: {
            page: currentpage
        },
        success: function (data) {
            $$("#sellertask-list").html(data);
            currentpage++;
            if (lastIndex < 10) {
                $$.ajax({
                    url: "/SellerTask/SellerTaskListPartial",
                    data: {
                        page: currentpage
                    },
                    success: function (data) {
                        $$("#sellertask-list").append(data);
                        currentpage++;
                    }
                });
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
            if (currentpage > 4) {
                myApp.detachInfiniteScroll($$(".infinite-scroll"))//关闭滚动
                $$(".infinite-scroll-preloader").remove();//移除加载符
                $$(".infinite-pre").removeClass("hidden");
                return;
            };
            //生成新的条目
            $$.ajax({
                url: "/SellerTask/SellerTaskListPartial",
                data: {
                    page: currentpage
                },
                success: function (data) {
                    $$("#sellertask-list").append(data);
                    currentpage++;
                }
            });
        }, 1000)
    });
});
//SellerTaskDetails 轮播图
$$(document).on("pageInit", ".page[data-page='SellerTaskDetails']", function () {
    var mySwiper = myApp.swiper('.swiper-container', {
        pagination: '.swiper-pagination'
    });
})