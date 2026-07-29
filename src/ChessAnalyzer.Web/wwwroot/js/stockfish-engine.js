let worker = null;
let ready = false;
let initPromise = null;

function createWorker() {
    const wasmSupported = typeof WebAssembly === 'object'
        && WebAssembly.validate(Uint8Array.of(0x0, 0x61, 0x73, 0x6d, 0x01, 0x00, 0x00, 0x00));
    const script = wasmSupported ? 'stockfish.wasm.js' : 'stockfish.js';
    return new Worker(new URL(script, import.meta.url));
}

function waitReady() {
    return new Promise((resolve, reject) => {
        const timeout = setTimeout(() => reject(new Error('Stockfish timeout')), 30000);
        const handler = (event) => {
            if (event.data === 'readyok') {
                clearTimeout(timeout);
                worker.removeEventListener('message', handler);
                ready = true;
                resolve();
            }
        };
        worker.addEventListener('message', handler);
        worker.postMessage('isready');
    });
}

export async function initialize(options) {
    if (ready) {
        return;
    }

    if (initPromise) {
        await initPromise;
        return;
    }

    initPromise = (async () => {
        worker = createWorker();
        worker.onerror = (error) => {
            console.error('Stockfish worker error', error);
        };
        worker.postMessage('uci');
        await waitReady();

        if (options?.hashMb) {
            worker.postMessage(`setoption name Hash value ${options.hashMb}`);
            await waitReady();
        }
    })();

    await initPromise;
}

export async function configure(options) {
    if (!worker) {
        return;
    }

    if (options?.hashMb) {
        worker.postMessage(`setoption name Hash value ${options.hashMb}`);
        await waitReady();
    }
}

export async function analyze(fen, depth, multiPv) {
    if (!worker || !ready) {
        throw new Error('Stockfish is not initialized');
    }

    const evaluations = new Map();
    const targetDepth = depth || 14;
    const lines = multiPv || 1;

    return new Promise((resolve, reject) => {
        const timeout = setTimeout(() => {
            worker.removeEventListener('message', handler);
            reject(new Error('Analysis timeout'));
        }, 120000);

        const handler = (event) => {
            const line = event.data;
            if (typeof line !== 'string') {
                return;
            }

            if (line.startsWith('info ')) {
                const parsed = parseInfo(line);
                if (parsed && parsed.bestMove) {
                    evaluations.set(parsed.multipv, parsed);
                }
                return;
            }

            if (line.startsWith('bestmove')) {
                clearTimeout(timeout);
                worker.removeEventListener('message', handler);

                const results = [];
                for (let i = 1; i <= lines; i++) {
                    results.push(evaluations.get(i) ?? {
                        centipawns: 0,
                        mateIn: null,
                        bestMove: line.split(' ')[1] || 'e2e4',
                        pvLine: line.split(' ')[1] || 'e2e4',
                        depth: targetDepth
                    });
                }

                resolve(results);
            }
        };

        worker.addEventListener('message', handler);
        worker.postMessage(`setoption name MultiPV value ${lines}`);
        worker.postMessage(`position fen ${fen}`);
        worker.postMessage(`go depth ${targetDepth}`);
    });
}

function parseInfo(line) {
    const parts = line.split(' ');
    let multipv = 1;
    let cp = 0;
    let mate = null;
    let pvDepth = 0;
    let pv = [];

    for (let i = 0; i < parts.length; i++) {
        switch (parts[i]) {
            case 'multipv':
                multipv = parseInt(parts[++i], 10);
                break;
            case 'depth':
                pvDepth = parseInt(parts[++i], 10);
                break;
            case 'score':
                if (parts[i + 1] === 'cp') {
                    cp = parseInt(parts[i + 2], 10);
                    i += 2;
                } else if (parts[i + 1] === 'mate') {
                    mate = parts[i + 2];
                    i += 2;
                }
                break;
            case 'pv':
                pv = parts.slice(i + 1);
                i = parts.length;
                break;
        }
    }

    if (pv.length === 0) {
        return null;
    }

    return {
        multipv,
        centipawns: cp,
        mateIn: mate,
        bestMove: pv[0],
        pvLine: pv.join(' '),
        depth: pvDepth
    };
}

export function dispose() {
    if (worker) {
        worker.terminate();
        worker = null;
    }

    ready = false;
    initPromise = null;
}
