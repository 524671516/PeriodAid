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

refresh_userpanel();
$("input.error").parent("div").addClass("custom-error");

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