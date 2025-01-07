mergeInto(LibraryManager.library, {
    Redirect: function(urlPtr) {
        // C#에서 전달된 URL을 문자열로 변환
        var url = UTF8ToString(urlPtr);
        window.location.href = url; // 리다이렉트 수행
    },
    IsMobileDevice: function() {
        const userAgent = navigator.userAgent || navigator.vendor || window.opera;
        // 모바일인지 여부를 확인
        return /android|iPad|iPhone|iPod/i.test(userAgent) ? 1 : 0;
    },
    Vibrate: function(duration) {
        if (navigator.vibrate) {
            navigator.vibrate(duration); // 진동을 요청 (ms 단위)
        } else {
            console.log("Vibration API not supported on this device.");
        }
    },
    RemoveLoadingScreen: function() {
        // Assets/WebGLTemplates/index.html에서 custom-loading-screen id 가진 tag 삭제
        var loadingScreen = document.getElementById('custom-loading-screen');
        if (loadingScreen) {
            document.body.removeChild(loadingScreen);
            console.log(`Element with custom-loading-screen removed.`);
        } else {
            console.log(`No element found with id '${id}'.`);
        }
    }
});