const cluster = require('cluster');
const os = require('os');
const express = require('express');
const path = require('path');
const compression = require('compression');
const zlib = require('zlib');
const { LRUCache } = require('lru-cache');

// 마스터 프로세스: 워커 생성 및 관리
if (cluster.isMaster) {
    const numCPUs = os.cpus().length; // CPU 코어 수
    console.log(`Master process ${process.pid} is running`);
    console.log(`Starting ${numCPUs} workers...`);

    // CPU 코어 수만큼 워커 생성
    for (let i = 0; i < numCPUs; i++) {
        cluster.fork();
    }

    // 워커 종료 시 재시작
    cluster.on('exit', (worker, code, signal) => {
        console.log(`Worker ${worker.process.pid} died`);
        console.log('Starting a new worker...');
        cluster.fork();
    });
} else {
    // LRU Cache 설정 (모든 워커에서 개별적으로 사용)
    const cache = new LRUCache({
        maxSize: 50 * 1024 * 1024, // 최대 50MB
        sizeCalculation: (value, key) => value.length, // 항목 크기 계산
        ttl: 1000 * 60 * 60, // 1시간 동안 캐싱
    });

    // Express 서버 설정
    const app = express();
    const PORT = 3000; // 모든 워커는 동일한 포트에서 동작

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

    // 캐시 미들웨어
    const cacheMiddleware = (req, res, next) => {
        const cachedData = cache.get(req.url); // 요청 URL 기반 캐시 확인
        if (cachedData) {
            console.log(`Cache hit for: ${req.url}`);
            res.setHeader('X-Cache', 'HIT');
            res.send(cachedData);
        } else {
            console.log(`Cache miss for: ${req.url}`);
            res.setHeader('X-Cache', 'MISS');
            next(); // 캐시가 없을 경우 다음 미들웨어로 이동
        }
    };

    app.use(cacheMiddleware); // 캐싱 미들웨어 추가

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

    // index.html 및 기타 요청 캐싱
    app.get('*', (req, res) => {
        const cachedPage = cache.get('index.html'); // 항상 index.html을 캐시로 확인
        if (cachedPage) {
            res.setHeader('X-Cache', 'HIT');
            res.send(cachedPage);
        } else {
            const filePath = path.join(buildsPath, 'index.html');
            res.setHeader('X-Cache', 'MISS');
            res.sendFile(filePath, (err) => {
                if (!err) {
                    const fileContent = require('fs').readFileSync(filePath); // 파일 내용을 읽어 캐싱
                    cache.set('index.html', fileContent); // 캐시에 저장
                } else {
                    console.error(`Error serving index.html`, err);
                    res.status(404).send('File not found');
                }
            });
        }
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

    // 서버 실행
    app.listen(PORT, () => {
        console.log(`Worker process ${process.pid} started on port ${PORT}`);
    });
}