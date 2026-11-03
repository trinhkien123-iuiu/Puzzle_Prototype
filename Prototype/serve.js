// Máy chủ tĩnh tối giản để chơi thử các prototype HTML trên máy này hoặc trên điện thoại cùng mạng Wi-Fi.
// Chạy:  node Prototype/serve.js            -> chỉ máy này:   http://127.0.0.1:8765/mitoosis.html
//        node Prototype/serve.js --lan      -> cả mạng Wi-Fi: http://<IP máy này>:8765/mitoosis.html
// Chỉ phục vụ file trong Prototype/games, không cho đọc ra ngoài thư mục đó.
const http = require('http');
const fs = require('fs');
const os = require('os');
const path = require('path');

const root = path.resolve(__dirname, 'games');
const lan = process.argv.includes('--lan');
const port = 8765;
const types = { '.html': 'text/html; charset=utf-8', '.js': 'text/javascript', '.css': 'text/css', '.png': 'image/png',
  '.svg': 'image/svg+xml', '.ogg': 'audio/ogg', '.mp3': 'audio/mpeg', '.json': 'application/json', '.txt': 'text/plain; charset=utf-8' };

const server = http.createServer((req, res) => {
  let file = path.resolve(root, '.' + decodeURIComponent(req.url.split('?')[0]));
  if (file !== root && !file.startsWith(root + path.sep)) { res.writeHead(403); res.end(); return; }
  if (file === root) file = path.join(root, 'mitoosis.html');
  fs.readFile(file, (err, data) => {
    if (err) { res.writeHead(404); res.end('not found'); return; }
    res.writeHead(200, { 'Content-Type': types[path.extname(file)] || 'application/octet-stream', 'Cache-Control': 'no-cache' });
    res.end(data);
  });
});
// Cổng đã có máy chủ khác giữ (thường là chính script này đang chạy ở cửa sổ khác): báo rõ thay vì in lỗi đỏ.
server.on('error', err => {
  if (err.code !== 'EADDRINUSE') throw err;
  console.log(`Cổng ${port} đang được dùng: có lẽ máy chủ đã chạy sẵn ở một cửa sổ khác.`);
  console.log(`Cứ mở http://127.0.0.1:${port}/mitoosis.html; nếu không vào được thì tắt cửa sổ kia rồi chạy lại lệnh này.`);
  process.exit(1);
});
server.listen(port, lan ? '0.0.0.0' : '127.0.0.1', () => {
  console.log(`Đang phục vụ ${root}`);
  console.log(`  Máy này: http://127.0.0.1:${port}/mitoosis.html`);
  if (lan) for (const nets of Object.values(os.networkInterfaces())) for (const n of nets || []) {
    if (n.family === 'IPv4' && !n.internal) console.log(`  Cùng Wi-Fi: http://${n.address}:${port}/mitoosis.html`);
  }
});
