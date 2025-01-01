const express = require('express');
const path = require('path');

const app = express();
const PORT = 3000; // 원하는 포트 번호 설정

//// 캐시를 비활성화하는 미들웨어 추가
app.use((req, res, next) => {
  res.set('Cache-Control', 'no-store, no-cache, must-revalidate, proxy-revalidate');
  res.set('Pragma', 'no-cache');
  res.set('Expires', '0');
  res.set('Surrogate-Control', 'no-store');
  next();
});

// 정적 파일 서빙
app.use(express.static(path.join(__dirname, 'Build'), {
  etag: false
}));

// WebGL 빌드 폴더를 정적 파일로 서빙
const buildPath = path.join(__dirname, 'Builds/WebGL');
app.use(express.static(buildPath));

// 모든 요청을 index.html로 라우팅
app.get('*', (req, res) => {
    res.sendFile(path.join(buildPath, 'index.html'));
});

app.listen(PORT, () => {
    console.log(`WebGL server running at http://localhost:${PORT}/room?roomId=test&clientId=test`);
});

