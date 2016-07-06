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

//����ˢ��
$$(document).on('pageInit', '.page[data-page="refresh"]', function (e) {
    var songs = ['Yellow Submarine', 'Don\'t Stop Me Now', 'Billie Jean', 'Californication'];
    var authors = ['Beatles', 'Queen', 'Michael Jackson', 'Red Hot Chili Peppers'];

    // ����ˢ��ҳ��
    var ptrContent = $$('.pull-to-refresh-content');

    // ���'refresh'������
    ptrContent.on('refresh', function (e) {
        // ģ��2s�ļ��ع���
        setTimeout(function () {
            // ���ͼƬ
            var picURL = 'http://hhhhold.com/88/d/jpg?' + Math.round(Math.random() * 100);
            // �������
            var song = songs[Math.floor(Math.random() * songs.length)];
            // �������
            var author = authors[Math.floor(Math.random() * authors.length)];
            // �б�Ԫ�ص�HTML�ַ���
            var itemHTML = '<li class="item-content">' +
                              '<div class="item-media"><img src="' + picURL + '" width="44"/></div>' +
                              '<div class="item-inner">' +
                                '<div class="item-title-row">' +
                                  '<div class="item-title">' + song + '</div>' +
                                '</div>' +
                                '<div class="item-subtitle">' + author + '</div>' +
                              '</div>' +
                            '</li>';
            // ǰ�����б�Ԫ��
            ptrContent.find('ul').prepend(itemHTML);
            // ���������Ҫ����
            myApp.pullToRefreshDone();
        }, 2000);
    });
});
//end
//����ѭ��
$$(document).on('pageInit', '.page[data-page="infinitescroll"]', function (e) {
    var loading = false;//����flag
    var lastIndex = $$(".list-block li").length;//�ϴμ��ص����
    var maxItem = 100;//���������ӵ�����
    var itemPerload = 10;//ÿ�ο����ӵ�����
    $$(".infinite-scroll").on("infinite", function (e) {
        if (loading) return;
        loading = true;
        setTimeout(function () {
            loading = false;//����flag
            if (lastIndex >= maxItem) {
                myApp.detachInfiniteScroll($$(".infinite-scroll"))//�رչ���
                $$(".infinite-scroll-preloader").remove();//�Ƴ����ط�
                return;
            };
            //�����µ���Ŀ
            //itemList = '';
            //for (var i = lastIndex + 1; i <= lastIndex + itemPerload; i++) {
            //    itemList += "";
            //}
            var itemList = "<li class='item-content'><div class='item-inner'><div class='item-title'>item list</div><div class='item-after'>2016-07-07</div></div></li>"
            $$(".list-block ul").append(itemList);//���
            lastIndex = $$(".list-block li").length//�µ�����
        }, 1000)

    });

});