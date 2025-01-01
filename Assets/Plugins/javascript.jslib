mergeInto(LibraryManager.library, {
    Redirect: function(urlPtr) {
        // C#에서 전달된 URL을 문자열로 변환
        var url = UTF8ToString(urlPtr);
        window.location.href = url; // 리다이렉트 수행
    },
});