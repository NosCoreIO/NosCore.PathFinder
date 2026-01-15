//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

const canvas = document.getElementById('canvas');
const ctx = canvas.getContext('2d');

let currentMap = null;
let mapData = null;
let flowFieldData = null;
let cellSize = 20;
let mode = 'flowfield';
let showArrows = true;
let showEntities = true;
let maxDistance = 22;
let stopDistance = 0;
let ws = null;
let lastMousePos = { x: -1, y: -1 };

let entities = [];
let lastFrameTime = 0;
const MOVE_SPEED = 3;
const RETURN_SPEED = 2;

let returnFlowFields = new Map();

let zoomLevel = 1;
let panX = 0;
let panY = 0;
let isDragging = false;
let dragStart = { x: 0, y: 0 };
let gridBytes = null;
let wallCanvas = null;
let wallCtx = null;

async function init() {
    const maps = await fetch('/api/maps').then(r => r.json());
    const select = document.getElementById('mapSelect');

    const dbMaps = maps.filter(m => m.source === 'database');
    const sampleMaps = maps.filter(m => m.source === 'sample');

    if (dbMaps.length > 0) {
        const dbGroup = document.createElement('optgroup');
        dbGroup.label = 'Database Maps';
        dbMaps.forEach(m => {
            const opt = document.createElement('option');
            opt.value = m.id;
            opt.textContent = `Map ${m.id} (${m.width}x${m.height})`;
            dbGroup.appendChild(opt);
        });
        select.appendChild(dbGroup);
    }

    if (sampleMaps.length > 0) {
        const sampleGroup = document.createElement('optgroup');
        sampleGroup.label = 'Sample Maps';
        sampleMaps.forEach(m => {
            const opt = document.createElement('option');
            opt.value = m.id;
            opt.textContent = `Sample ${Math.abs(m.id)} (${m.width}x${m.height})`;
            sampleGroup.appendChild(opt);
        });
        select.appendChild(sampleGroup);
    }

    select.addEventListener('change', () => loadMap(parseInt(select.value)));

    document.querySelectorAll('.toggle').forEach(btn => {
        btn.addEventListener('click', () => {
            document.querySelectorAll('.toggle').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            mode = btn.dataset.mode;
            render();
        });
    });

    document.getElementById('maxDistance').addEventListener('input', (e) => {
        maxDistance = parseInt(e.target.value);
        document.getElementById('distanceValue').textContent = maxDistance;
    });

    document.getElementById('stopDistance').addEventListener('input', (e) => {
        stopDistance = parseFloat(e.target.value);
        document.getElementById('stopDistanceValue').textContent = stopDistance;
    });

    document.getElementById('showArrows').addEventListener('change', (e) => {
        showArrows = e.target.checked;
        render();
    });

    document.getElementById('showEntities').addEventListener('change', (e) => {
        showEntities = e.target.checked;
        render();
    });

    canvas.addEventListener('mousemove', onMouseMove);
    canvas.addEventListener('wheel', onWheel, { passive: false });
    canvas.addEventListener('mousedown', onMouseDown);
    canvas.addEventListener('mouseup', onMouseUp);
    canvas.addEventListener('mouseleave', onMouseUp);

    connectWebSocket();

    if (maps.length > 0) {
        loadMap(maps[0].id);
    }

    setInterval(updateAggregateStats, 2000);
    requestAnimationFrame(animate);
}

function connectWebSocket() {
    const protocol = location.protocol === 'https:' ? 'wss:' : 'ws:';
    ws = new WebSocket(`${protocol}//${location.host}/ws`);

    ws.onmessage = (event) => {
        const data = JSON.parse(event.data);
        if (data.type === 'flowfield') {
            flowFieldData = data;
            updatePerfStats(data.performance);
            render();
        }
    };

    ws.onclose = () => setTimeout(connectWebSocket, 1000);
}

async function loadMap(mapId) {
    currentMap = mapId;
    mapData = await fetch(`/api/maps/${mapId}`).then(r => r.json());
    flowFieldData = null;

    const container = canvas.parentElement;
    const maxWidth = container.clientWidth - 40;
    const maxHeight = container.clientHeight - 40;

    cellSize = Math.min(
        Math.floor(maxWidth / mapData.width),
        Math.floor(maxHeight / mapData.height),
        30
    );
    cellSize = Math.max(cellSize, 4);

    zoomLevel = 1;
    panX = 0;
    panY = 0;

    gridBytes = mapData.grid ? Uint8Array.from(atob(mapData.grid), c => c.charCodeAt(0)) : null;

    preRenderWalls();
    updateCanvasSize();
    initEntities();
    updateEntityCount();
    render();
}

function preRenderWalls() {
    wallCanvas = document.createElement('canvas');
    wallCanvas.width = mapData.width * cellSize;
    wallCanvas.height = mapData.height * cellSize;
    wallCtx = wallCanvas.getContext('2d');

    if (!gridBytes) return;

    wallCtx.fillStyle = '#21262d';
    for (let y = 0; y < mapData.height; y++) {
        for (let x = 0; x < mapData.width; x++) {
            const idx = y * mapData.width + x;
            if (gridBytes[idx >> 3] & (1 << (idx & 7))) {
                wallCtx.fillRect(x * cellSize, y * cellSize, cellSize, cellSize);
            }
        }
    }
}

function updateCanvasSize() {
    const container = canvas.parentElement;
    const mapWidth = mapData.width * cellSize * zoomLevel;
    const mapHeight = mapData.height * cellSize * zoomLevel;

    canvas.width = Math.min(mapWidth, container.clientWidth - 40);
    canvas.height = Math.min(mapHeight, container.clientHeight - 40);
}

function initEntities() {
    entities = [];
    returnFlowFields.clear();
    if (!mapData?.entities) return;

    let id = 0;
    for (const e of mapData.entities) {
        entities.push({
            id: id++,
            type: e.type,
            homeX: e.x,
            homeY: e.y,
            x: e.x,
            y: e.y,
            vnum: e.vNum,
            needsReturnPath: false
        });
    }
}

function updateEntityCount() {
    const monsterCount = mapData?.entities?.filter(e => e.type === 'monster').length || 0;
    const npcCount = mapData?.entities?.filter(e => e.type === 'npc').length || 0;
    document.getElementById('entityCount').textContent = `${monsterCount} monsters, ${npcCount} NPCs`;
}

function animate(timestamp) {
    const deltaTime = (timestamp - lastFrameTime) / 1000;
    lastFrameTime = timestamp;

    if (mapData && entities.length > 0) {
        updateEntities(deltaTime);
        render();
    }

    requestAnimationFrame(animate);
}

function isWalkable(x, y) {
    if (x < 0 || y < 0 || x >= mapData.width || y >= mapData.height) return false;
    if (!gridBytes) return true;
    const ix = Math.floor(x);
    const iy = Math.floor(y);
    const idx = iy * mapData.width + ix;
    return !(gridBytes[idx >> 3] & (1 << (idx & 7)));
}

function updateEntities(deltaTime) {
    const vectorMap = new Map();
    if (flowFieldData?.vectors) {
        for (const v of flowFieldData.vectors) {
            vectorMap.set(`${v.x},${v.y}`, { dx: v.dx, dy: v.dy });
        }
    }

    for (const entity of entities) {
        const cellX = Math.floor(entity.x);
        const cellY = Math.floor(entity.y);
        const key = `${cellX},${cellY}`;
        const vector = vectorMap.get(key);

        if (vector) {
            returnFlowFields.delete(entity.id);
            entity.needsReturnPath = false;

            const newX = entity.x + vector.dx * MOVE_SPEED * deltaTime;
            const newY = entity.y + vector.dy * MOVE_SPEED * deltaTime;
            if (isWalkable(newX, newY)) {
                entity.x = newX;
                entity.y = newY;
            } else if (isWalkable(newX, entity.y)) {
                entity.x = newX;
            } else if (isWalkable(entity.x, newY)) {
                entity.y = newY;
            }
        } else {
            const dx = entity.homeX - entity.x;
            const dy = entity.homeY - entity.y;
            const dist = Math.sqrt(dx * dx + dy * dy);

            if (dist < 0.1) {
                entity.x = entity.homeX;
                entity.y = entity.homeY;
                returnFlowFields.delete(entity.id);
                entity.needsReturnPath = false;
                entity.stuckFrames = 0;
                continue;
            }

            const nx = dx / dist;
            const ny = dy / dist;
            const move = Math.min(dist, RETURN_SPEED * deltaTime);
            const directX = entity.x + nx * move;
            const directY = entity.y + ny * move;

            let moved = false;
            if (isWalkable(directX, directY)) {
                entity.x = directX;
                entity.y = directY;
                moved = true;
                entity.stuckFrames = 0;
                returnFlowFields.delete(entity.id);
                entity.needsReturnPath = false;
            } else if (isWalkable(directX, entity.y)) {
                entity.x = directX;
                moved = true;
                entity.stuckFrames = 0;
            } else if (isWalkable(entity.x, directY)) {
                entity.y = directY;
                moved = true;
                entity.stuckFrames = 0;
            }

            if (!moved) {
                entity.stuckFrames = (entity.stuckFrames || 0) + 1;

                if (entity.stuckFrames > 5 && !entity.needsReturnPath) {
                    entity.needsReturnPath = true;
                    requestReturnPath(entity);
                }

                const returnField = returnFlowFields.get(entity.id);
                if (returnField) {
                    const returnVector = returnField.get(key);
                    if (returnVector) {
                        const pathX = entity.x + returnVector.dx * RETURN_SPEED * deltaTime;
                        const pathY = entity.y + returnVector.dy * RETURN_SPEED * deltaTime;
                        if (isWalkable(pathX, pathY)) {
                            entity.x = pathX;
                            entity.y = pathY;
                        } else if (isWalkable(pathX, entity.y)) {
                            entity.x = pathX;
                        } else if (isWalkable(entity.x, pathY)) {
                            entity.y = pathY;
                        }
                    }
                }
            }
        }
    }
}

async function requestReturnPath(entity) {
    try {
        const dx = Math.abs(entity.x - entity.homeX);
        const dy = Math.abs(entity.y - entity.homeY);
        const pathDistance = Math.max(dx, dy) + 10;
        const res = await fetch(`/api/maps/${currentMap}/flowfield?x=${entity.homeX}&y=${entity.homeY}&maxDistance=${pathDistance}&stopDistance=0`);
        if (!res.ok) return;

        const data = await res.json();
        const vectorMap = new Map();
        for (const v of data.vectors) {
            vectorMap.set(`${v.x},${v.y}`, { dx: v.dx, dy: v.dy });
        }
        returnFlowFields.set(entity.id, vectorMap);
    } catch (e) {}
}

function onMouseMove(e) {
    if (!mapData) return;

    const rect = canvas.getBoundingClientRect();

    if (isDragging) {
        panX += e.clientX - dragStart.x;
        panY += e.clientY - dragStart.y;
        dragStart = { x: e.clientX, y: e.clientY };
        render();
        return;
    }

    const scale = cellSize * zoomLevel;
    const x = Math.floor((e.clientX - rect.left - panX) / scale);
    const y = Math.floor((e.clientY - rect.top - panY) / scale);

    if (x === lastMousePos.x && y === lastMousePos.y) return;
    if (x < 0 || x >= mapData.width || y < 0 || y >= mapData.height) return;

    lastMousePos = { x, y };

    if (ws && ws.readyState === WebSocket.OPEN) {
        ws.send(JSON.stringify({
            mapId: currentMap,
            x: x,
            y: y,
            maxDistance: maxDistance,
            stopDistance: stopDistance
        }));
    }
}

function onWheel(e) {
    if (!mapData) return;
    e.preventDefault();

    const rect = canvas.getBoundingClientRect();
    const mouseX = e.clientX - rect.left;
    const mouseY = e.clientY - rect.top;

    const worldX = (mouseX - panX) / (cellSize * zoomLevel);
    const worldY = (mouseY - panY) / (cellSize * zoomLevel);

    const zoomFactor = e.deltaY < 0 ? 1.15 : 0.87;
    const newZoom = Math.max(0.5, Math.min(5, zoomLevel * zoomFactor));

    panX = mouseX - worldX * cellSize * newZoom;
    panY = mouseY - worldY * cellSize * newZoom;
    zoomLevel = newZoom;

    render();
}

function onMouseDown(e) {
    if (e.button === 0 || e.button === 1) {
        isDragging = true;
        dragStart = { x: e.clientX, y: e.clientY };
        canvas.style.cursor = 'grabbing';
        e.preventDefault();
    }
}

function onMouseUp() {
    isDragging = false;
    canvas.style.cursor = 'default';
}

function render() {
    if (!mapData) return;

    const scale = cellSize * zoomLevel;

    ctx.fillStyle = '#0d1117';
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    ctx.save();
    ctx.translate(panX, panY);
    ctx.scale(zoomLevel, zoomLevel);

    if (wallCanvas) {
        ctx.drawImage(wallCanvas, 0, 0);
    }

    if (flowFieldData) {
        const maxDist = Math.max(...flowFieldData.distances.map(d => d.d), 1);

        for (const cell of flowFieldData.distances) {
            const intensity = 1 - (cell.d / maxDist);
            const r = Math.floor(88 + 100 * intensity);
            const g = Math.floor(166 + 60 * intensity);
            const b = Math.floor(255 * intensity);
            ctx.fillStyle = `rgba(${r},${g},${b},0.6)`;
            ctx.fillRect(cell.x * cellSize, cell.y * cellSize, cellSize, cellSize);
        }

        if (showArrows && mode === 'flowfield') {
            ctx.strokeStyle = 'rgba(255,255,255,0.8)';
            ctx.fillStyle = 'rgba(255,255,255,0.8)';
            ctx.lineWidth = 1 / zoomLevel;

            for (const v of flowFieldData.vectors) {
                drawArrow(v.x, v.y, v.dx, v.dy);
            }
        }

        ctx.fillStyle = '#58a6ff';
        ctx.beginPath();
        ctx.arc(
            flowFieldData.origin.x * cellSize + cellSize / 2,
            flowFieldData.origin.y * cellSize + cellSize / 2,
            cellSize / 2.5,
            0, Math.PI * 2
        );
        ctx.fill();
        ctx.strokeStyle = '#fff';
        ctx.lineWidth = 2 / zoomLevel;
        ctx.stroke();
    }

    if (showEntities) {
        for (const e of entities) {
            if (e.type === 'monster') {
                drawEntity(e.x, e.y, '#f85149', '#ff7b72');
            } else {
                drawEntity(e.x, e.y, '#d29922', '#e3b341');
            }
        }
    }

    ctx.strokeStyle = 'rgba(255,255,255,0.05)';
    ctx.lineWidth = 0.5 / zoomLevel;
    for (let x = 0; x <= mapData.width; x++) {
        ctx.beginPath();
        ctx.moveTo(x * cellSize, 0);
        ctx.lineTo(x * cellSize, mapData.height * cellSize);
        ctx.stroke();
    }
    for (let y = 0; y <= mapData.height; y++) {
        ctx.beginPath();
        ctx.moveTo(0, y * cellSize);
        ctx.lineTo(mapData.width * cellSize, y * cellSize);
        ctx.stroke();
    }

    ctx.restore();
}

function drawEntity(x, y, fillColor, strokeColor) {
    ctx.fillStyle = fillColor;
    ctx.beginPath();
    ctx.arc(
        x * cellSize + cellSize / 2,
        y * cellSize + cellSize / 2,
        cellSize / 3,
        0, Math.PI * 2
    );
    ctx.fill();
    ctx.strokeStyle = strokeColor;
    ctx.lineWidth = 1.5 / zoomLevel;
    ctx.stroke();
}

function drawArrow(x, y, dx, dy) {
    const cx = x * cellSize + cellSize / 2;
    const cy = y * cellSize + cellSize / 2;
    const len = cellSize * 0.35;
    const headLen = cellSize * 0.15;

    const ex = cx + dx * len;
    const ey = cy + dy * len;

    ctx.beginPath();
    ctx.moveTo(cx, cy);
    ctx.lineTo(ex, ey);
    ctx.stroke();

    const angle = Math.atan2(dy, dx);
    ctx.beginPath();
    ctx.moveTo(ex, ey);
    ctx.lineTo(ex - headLen * Math.cos(angle - 0.5), ey - headLen * Math.sin(angle - 0.5));
    ctx.lineTo(ex - headLen * Math.cos(angle + 0.5), ey - headLen * Math.sin(angle + 0.5));
    ctx.closePath();
    ctx.fill();
}

function updatePerfStats(perf) {
    document.getElementById('lastMs').textContent = perf.elapsedMs.toFixed(2) + ' ms';
    document.getElementById('vectorCount').textContent = perf.vectorCount.toLocaleString();

    const cellsPerSec = perf.elapsedMs > 0 ? Math.round(perf.vectorCount / perf.elapsedMs * 1000) : 0;
    document.getElementById('cellsPerSec').textContent = cellsPerSec.toLocaleString();

    const barWidth = Math.min(100, perf.elapsedMs * 10);
    document.getElementById('perfBar').style.width = barWidth + '%';
}

async function updateAggregateStats() {
    try {
        const stats = await fetch('/api/performance').then(r => r.json());
        document.getElementById('totalReqs').textContent = stats.totalRequests.toLocaleString();

        const ff = stats.flowField;
        if (ff.count > 0) {
            document.getElementById('avgMs').textContent = ff.avgMs.toFixed(2) + ' ms';
            document.getElementById('p95Ms').textContent = ff.p95Ms.toFixed(2) + ' ms';
        }

        document.getElementById('cpuPercent').textContent = stats.cpuPercent.toFixed(1) + '%';
        document.getElementById('memoryMb').textContent = stats.memoryMb.toFixed(1) + ' MB';
    } catch (e) {}
}

init();
