let boardEl = null;
let dotNetRef = null;
let dragState = null;

const DRAG_THRESHOLD = 8;

export function init(element, dotnet) {
    dispose();
    boardEl = element;
    dotNetRef = dotnet;
    boardEl.addEventListener('pointerdown', onPointerDown);
    window.addEventListener('pointermove', onPointerMove);
    window.addEventListener('pointerup', onPointerUp);
    window.addEventListener('pointercancel', onPointerCancel);
}

export function dispose() {
    if (boardEl) {
        boardEl.removeEventListener('pointerdown', onPointerDown);
    }
    window.removeEventListener('pointermove', onPointerMove);
    window.removeEventListener('pointerup', onPointerUp);
    window.removeEventListener('pointercancel', onPointerCancel);
    cleanupDrag();
    boardEl = null;
    dotNetRef = null;
}

function getSquareElement(target) {
    return target?.closest?.('[data-square]') ?? null;
}

function getSquareName(target) {
    return getSquareElement(target)?.dataset?.square ?? null;
}

function onPointerDown(e) {
    if (!boardEl || boardEl.dataset.interactive !== 'true' || e.button !== 0) {
        return;
    }

    const squareEl = getSquareElement(e.target);
    if (!squareEl) {
        return;
    }

    const square = squareEl.dataset.square;
    const pieceImg = squareEl.querySelector('.piece-img');

    dragState = {
        pointerId: e.pointerId,
        square,
        hasPiece: !!pieceImg,
        canDrag: false,
        legalTargets: new Set(),
        moved: false,
        startX: e.clientX,
        startY: e.clientY,
        ghost: null
    };

    if (pieceImg) {
        dotNetRef.invokeMethodAsync('HandleDragStartJs', square)
            .then((result) => {
                if (!dragState || dragState.square !== square || dragState.pointerId !== e.pointerId) {
                    return;
                }

                if (!result?.canDrag) {
                    return;
                }

                dragState.canDrag = true;
                dragState.legalTargets = new Set(result.legalTargets ?? []);
                updateLegalHighlights();

                const img = pieceImg.cloneNode(true);
                img.classList.add('piece-drag-ghost');
                img.style.width = `${pieceImg.offsetWidth}px`;
                img.style.height = `${pieceImg.offsetHeight}px`;
                document.body.appendChild(img);
                dragState.ghost = img;
                positionGhost(e.clientX, e.clientY);

                squareEl.classList.add('drag-source');
                boardEl.setPointerCapture(e.pointerId);
            })
            .catch(() => cleanupDrag());
    }
}

function onPointerMove(e) {
    if (!dragState || e.pointerId !== dragState.pointerId) {
        return;
    }

    const dx = e.clientX - dragState.startX;
    const dy = e.clientY - dragState.startY;

    if (!dragState.moved) {
        if (Math.hypot(dx, dy) < DRAG_THRESHOLD) {
            return;
        }

        dragState.moved = true;
        if (dragState.canDrag) {
            e.preventDefault();
        }
    }

    if (!dragState.canDrag || !dragState.ghost) {
        return;
    }

    positionGhost(e.clientX, e.clientY);
    updateHoverSquare(getSquareName(document.elementFromPoint(e.clientX, e.clientY)));
}

function onPointerUp(e) {
    if (!dragState || e.pointerId !== dragState.pointerId) {
        return;
    }

    finishDrag(getSquareName(document.elementFromPoint(e.clientX, e.clientY)) ?? dragState.square);
}

function onPointerCancel(e) {
    if (!dragState || e.pointerId !== dragState.pointerId) {
        return;
    }

    finishDrag(null);
}

function finishDrag(targetSquare) {
    if (!dragState || !dotNetRef) {
        cleanupDrag();
        return;
    }

    const { square, moved, canDrag } = dragState;

    try {
        boardEl?.releasePointerCapture?.(dragState.pointerId);
    } catch {
        // ignore
    }

    cleanupDrag();

    if (moved && canDrag && targetSquare) {
        dotNetRef.invokeMethodAsync('HandleDropJs', square, targetSquare);
        return;
    }

    dotNetRef.invokeMethodAsync('HandleTapJs', targetSquare ?? square);
}

function positionGhost(x, y) {
    if (!dragState?.ghost) {
        return;
    }

    dragState.ghost.style.left = `${x}px`;
    dragState.ghost.style.top = `${y}px`;
}

function updateLegalHighlights() {
    if (!boardEl || !dragState) {
        return;
    }

    boardEl.querySelectorAll('[data-square]').forEach((el) => {
        const square = el.dataset.square;
        el.classList.toggle('drag-legal', dragState.legalTargets.has(square));
    });
}

function updateHoverSquare(square) {
    if (!boardEl || !dragState) {
        return;
    }

    boardEl.querySelectorAll('[data-square].drag-hover').forEach((el) => {
        el.classList.remove('drag-hover');
    });

    if (!square) {
        return;
    }

    const el = boardEl.querySelector(`[data-square="${square}"]`);
    if (el) {
        el.classList.add('drag-hover');
    }
}

function cleanupDrag() {
    if (!dragState) {
        return;
    }

    dragState.ghost?.remove();

    if (boardEl) {
        boardEl.querySelectorAll('.drag-source, .drag-legal, .drag-hover').forEach((el) => {
            el.classList.remove('drag-source', 'drag-legal', 'drag-hover');
        });
    }

    dragState = null;
}
