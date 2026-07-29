let worker = null;
let ready = false;
let initPromise = null;
let configuredMultiPv = 0;

function createWorker() {
    // stockfish.wasm.js needs SharedArrayBuffer (COOP/COEP headers).
    // GitHub Pages does not provide them, so use the single-threaded build.
    return new Worker(new URL('stockfish.js', import.meta.url));
}

function sendAndWaitFor(expectedPrefix, command, timeoutMs = 60000) {
    return new Promise((resolve, reject) => {
        const timeout = setTimeout(() => {
            worker.removeEventListener('message', handler);
            reject(new Error(`Stockfish timeout waiting for ${expectedPrefix}`));
        }, timeoutMs);

        const handler = (event) => {
            const line = typeof event.data === 'string' ? event.data.trim() : '';
            if (!line) {
                return;
            }

            if (line === expectedPrefix || line.startsWith(expectedPrefix)) {
                clearTimeout(timeout);
                worker.removeEventListener('message', handler);
                resolve(line);
            }
        };

        worker.addEventListener('message', handler);
        if (command) {
            worker.postMessage(command);
        }
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
        await sendAndWaitFor('uciok');
        await sendAndWaitFor('readyok', 'isready');

        const hashMb = Math.min(options?.hashMb ?? 16, 32);
        if (hashMb > 0) {
            worker.postMessage(`setoption name Hash value ${hashMb}`);
            await sendAndWaitFor('readyok', 'isready');
        }

        ready = true;
    })();

    try {
        await initPromise;
    } catch (error) {
        initPromise = null;
        if (worker) {
            worker.terminate();
            worker = null;
        }
        throw error;
    }
}

export async function configure(options) {
    if (!worker || !ready) {
        return;
    }

    const hashMb = Math.min(options?.hashMb ?? 16, 32);
    if (hashMb > 0) {
        worker.postMessage(`setoption name Hash value ${hashMb}`);
        await sendAndWaitFor('readyok', 'isready');
    }
}

export async function analyze(fen, depth, multiPv) {
    if (!worker || !ready) {
        throw new Error('Stockfish is not initialized');
    }

    const evaluations = new Map();
    const targetDepth = Math.min(depth || 12, 16);
    const lines = multiPv || 1;

    if (lines !== configuredMultiPv) {
        worker.postMessage(`setoption name MultiPV value ${lines}`);
        await sendAndWaitFor('readyok', 'isready');
        configuredMultiPv = lines;
    }

    return new Promise((resolve, reject) => {
        const timeout = setTimeout(() => {
            worker.removeEventListener('message', handler);
            reject(new Error('Analysis timeout'));
        }, 180000);

        const handler = (event) => {
            const line = typeof event.data === 'string' ? event.data.trim() : '';
            if (!line) {
                return;
            }

            if (line.startsWith('info ')) {
                const parsed = parseInfo(line);
                if (parsed?.bestMove) {
                    evaluations.set(parsed.multipv, parsed);
                }
                return;
            }

            if (line.startsWith('bestmove')) {
                clearTimeout(timeout);
                worker.removeEventListener('message', handler);

                const fallbackMove = line.split(' ')[1] || 'e2e4';
                const results = [];
                for (let i = 1; i <= lines; i++) {
                    results.push(evaluations.get(i) ?? {
                        centipawns: 0,
                        mateIn: null,
                        bestMove: fallbackMove,
                        pvLine: fallbackMove,
                        depth: targetDepth
                    });
                }

                resolve(results);
            }
        };

        worker.addEventListener('message', handler);
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
    configuredMultiPv = 0;
}
