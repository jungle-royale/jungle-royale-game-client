const cluster = require('cluster');
const os = require('os');
const express = require('express');
const path = require('path');
const compression = require('compression');
const zlib = require('zlib');
const { LRUCache } = require('lru-cache');

if (cluster.isMaster) {
    const numCPUs = os.cpus().length;
    console.log(`Master process ${process.pid} is running`);
    console.log(`Starting ${numCPUs} workers...`);

    for (let i = 0; i < numCPUs; i++) {
        cluster.fork();
    }

    cluster.on('exit', (worker, code, signal) => {
        console.log(`Worker ${worker.process.pid} died`);
        console.log('Starting a new worker...');
        cluster.fork();
    });
} else {
    const cache = new LRUCache({
        maxSize: 50 * 1024 * 1024, // 최대 50MB
        sizeCalculation: (value, key) => value.length, // 항목 크기 계산
        ttl: 1000 * 60 * 60, // 1시간 동안 캐싱
    });

    const app = express();
    const PORT = 3000;

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
            next();
        }
    };

    app.use(cacheMiddleware);

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

    // 모든 경로를 index.html로 매핑
    app.get('*', (req, res) => {
        const cachedPage = cache.get('index.html');
        if (cachedPage) {
            console.log(`Cache hit for index.html`);
            res.setHeader('Content-Type', 'text/html'); // 명시적으로 설정
            res.setHeader('X-Cache', 'HIT');
            res.send(cachedPage);
        } else {
            const filePath = path.join(buildsPath, 'index.html');
            res.setHeader('Content-Type', 'text/html'); // 명시적으로 설정
            res.setHeader('X-Cache', 'MISS');
            res.sendFile(filePath, (err) => {
                if (!err) {
                    const fileContent = require('fs').readFileSync(filePath); // 파일 내용을 읽어 캐싱
                    cache.set('index.html', fileContent);
                } else {
                    console.error(`Error serving index.html`, err);
                    res.status(404).send('File not found');
                }
            });
        }
    });

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
        console.log(`Worker process ${process.pid} started on port ${PORT}`);
    });
}