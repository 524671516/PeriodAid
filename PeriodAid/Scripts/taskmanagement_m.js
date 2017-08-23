var myApp = new Framework7({
    // Default title for modals
    modalTitle: '项目管理',

    // If it is webapp, we can enable hash navigation:
    pushState: true,

    // Hide and show indicator during ajax requests
    onAjaxStart: function (xhr) {
        myApp.showIndicator();
    },
    onAjaxComplete: function (xhr) {
        myApp.hideIndicator();
    }
});
var $$ = Dom7;
var mainView = myApp.addView('.view-main', {
    dynamicNavbar: true,
});
//初始化主页数据
HomeInitAjax();
$$(".tm-dropdown-menu-item").on("click", function () {
    var link_id = $$(this).attr("data-id");
    if ($$(this).hasClass("open")) {
        $$(this).removeClass("open");
        $$("#" + link_id).removeClass("open");
        $$(".tm-dropdown-mask").hide();
    } else {
        $$(".tm-dropdown-menu-item").removeClass("open");
        $$(".tm-dropdown-menu-list.open").removeClass("open");
        $$(".tm-dropdown-mask").show();
        $$(this).addClass("open");
        var movepx = $$(".tm-dropdown-menu-item[data-id='tm-type-dropdown']").find("span").offset().left + "px";
        $$("#" + link_id).addClass("open");
        $$("#" + link_id).find("label").css("paddingLeft", movepx);
    }
})
$$(".label-radio").on("click", function () {
    alert($$(this).find("input").val())
});
$$(".tm-dropdown-mask").on("click", function () {
    $$(this).hide();
    $$(".tm-dropdown-menu-item").removeClass("open");
    $$(".tm-dropdown-menu-list.open").removeClass("open");
})
myApp.onPageInit('home', function (page) {
    alert("home");
});
myApp.onPageInit('my', function (page) {
    alert("my");
});
myApp.onPageInit('subject_procedure', function (page) {
    alert("初始化项目过程");
    $$(".tab-link-procedure").on("click", function () {
        if (!$$(this).hasClass("active")) {
            alert("开始加载数据");
        }
    });
});
$$(".tab-link").on("click", function (page) {
    var tab_link = $$(this).attr("href");
    if (!$$(this).hasClass("active")) {
        if (!$$(tab_link).hasClass("init-finsh")) {
            $$(tab_link).addClass("init-finsh");
            alert("开始加载数据")
        };
    }
});
function HomeInitAjax() {
    $$.ajax({
        url: "/MobileTaskManagement/PersonalSubject",
        type: "post",
        success: function (data) {
            var _data = JSON.parse(data);
            if (_data.result == "FAIL") {
                myApp.alert("数据获取失败。")
            } else {
                $$(".tm-subject-area ul").html("");
                var _sublen = _data.subject.length;
                for (i = 0; i < _sublen; i++) {
                    var _defaultimg = "/Content/images/cover-default.jpg";
                    if (_data.subject[i].imgUrl != null) {
                        _defaultimg = "http://cdn2.shouquanzhai.cn/" + _data.subject[i].imgUrl;
                    }
                    if (_data.subject[i].status == 1) {
                        $$(".tm-subject-area.active ul").append("<li><a class=\"item-link\" href=\"/MobileTaskManagement/SubjectProcedure?SubjectId=" + _data.subject[i].id + "\">"
                                            + "<div class=\"item-content\"><div class=\"item-media\"><img width=\"44\" src=" + _defaultimg + "></div>"
                                            + "<div class=\"item-inner\"><div class=\"item-title-row\">"
                                            + "<div class=\"item-title\">" + _data.subject[i].title + "</div></div>"
                                            + "<div class=\"item-subtitle color-gray\">" + _data.subject[i].holderName + "</div></div></div></a></li>")

                    } else {
                        $$(".tm-subject-area.archived ul").append("<li><a class=\"item-link\" href=\"/MobileTaskManagement/SubjectProcedure?SubjectId=" + _data.subject[i].id + "\">"
                                          + "<div class=\"item-content\"><div class=\"item-media\"><img width=\"44\" src=" + _defaultimg + "></div>"
                                          + "<div class=\"item-inner\"><div class=\"item-title-row\">"
                                          + "<div class=\"item-title\">" + _data.subject[i].title + "</div></div>"
                                          + "<div class=\"item-subtitle color-gray\">" + _data.subject[i].holderName + "</div></div></div></a></li>")
                    }
                }
                //检查是否有数据和显示方式
                $$(".tm-subject-area").each(function () {
                    if ($$(this).find("ul li").length == 0) {
                        $$(this).find(".tm-empty").show();
                        $$(this).find("ul").hide();
                    }
                })
            }
        },
        error: function () {
            myApp.alert("请求数据失败。")
        }
    })
}

