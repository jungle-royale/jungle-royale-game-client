const express = require('express');
const path = require('path');
const compression = require('compression');
const zlib = require('zlib');

const app = express();
const PORT = 3000;

// Brotli 및 Gzip 압축 설정
app.use(
    compression({
        threshold: 1024,
        brotliOptions: {
            params: {
                [zlib.constants.BROTLI_PARAM_QUALITY]: 11,
            },
        },
    })
);

// 캐시 비활성화
app.use((req, res, next) => {
    res.set('Cache-Control', 'no-store, no-cache, must-revalidate, proxy-revalidate');
    res.set('Pragma', 'no-cache');
    res.set('Expires', '0');
    res.set('Surrogate-Control', 'no-store');
    next();
});

// 요청 로깅
app.use((req, res, next) => {
    console.log(`Requested URL: ${req.url}`);
    console.log(`Accept-Encoding: ${req.headers['accept-encoding']}`);
    next();
});

// 정적 파일 제공
const buildsPath = path.join(__dirname, 'Builds');
app.use(
    express.static(buildsPath, {
        setHeaders: (res, filePath) => {
            if (filePath.endsWith('.br')) {
                res.set('Content-Encoding', 'br');
                res.set('Content-Type', 'application/javascript');
            } else if (filePath.endsWith('.gz')) {
                res.set('Content-Encoding', 'gzip');
                res.set('Content-Type', 'application/javascript');
            } else if (filePath.endsWith('.js')) {
                res.set('Content-Type', 'application/javascript');
            }
        },
    })
);

// index.html 라우팅
app.get('*', (req, res) => {
    res.sendFile(path.join(buildsPath, 'index.html'));
});

// 에러 핸들링
app.use((err, req, res, next) => {
    if (err instanceof URIError) {
        console.error('Invalid URI:', req.originalUrl);
        res.status(400).send('Bad Request: Invalid URI');
    } else {
        console.error('Unexpected Error:', err);
        res.status(500).send('Internal Server Error');
    }
});

app.listen(PORT, () => {
    console.log(`WebGL server running at http://localhost:${PORT}/room?roomId=test&clientId=test`);
});
