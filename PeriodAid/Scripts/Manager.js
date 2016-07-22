var $$ = Framework7.$;
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