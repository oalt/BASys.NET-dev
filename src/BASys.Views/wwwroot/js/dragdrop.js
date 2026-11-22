// Pointer based drag and drop for the palette of the structural view.
// Elements with data-drag-item are dragged; elements with data-drop-target are drop targets.
(function () {
    var threshold = 5;
    var dotNetState = null;
    var pending = null;
    var ghost = null;
    var hovered = null;
    var dragging = false;

    function findTarget(x, y) {
        var element = document.elementFromPoint(x, y);
        return element ? element.closest("[data-drop-target]") : null;
    }

    function setHovered(target) {
        if (hovered !== target) {
            if (hovered) {
                hovered.removeAttribute("data-drop-hover");
            }
            hovered = target;
            if (hovered) {
                hovered.setAttribute("data-drop-hover", "true");
            }
        }
    }

    function startDrag(item, x, y) {
        dragging = true;
        ghost = item.cloneNode(true);
        ghost.classList.add("drag-ghost");
        document.body.appendChild(ghost);
        document.body.classList.add("is-dragging");
        moveGhost(x, y);
        dotNetState.invokeMethodAsync("BeginDrag", item.getAttribute("data-drag-item"));
    }

    function moveGhost(x, y) {
        ghost.style.left = (x + 12) + "px";
        ghost.style.top = (y + 12) + "px";
    }

    function endDrag() {
        setHovered(null);
        if (ghost) {
            ghost.remove();
            ghost = null;
        }
        document.body.classList.remove("is-dragging");
        dragging = false;
        pending = null;
    }

    function onPointerDown(e) {
        if (e.button !== 0 || !dotNetState) {
            return;
        }
        var item = e.target.closest("[data-drag-item]");
        if (item) {
            e.preventDefault();
            pending = { item: item, x: e.clientX, y: e.clientY };
        }
    }

    function onPointerMove(e) {
        if (dragging) {
            moveGhost(e.clientX, e.clientY);
            setHovered(findTarget(e.clientX, e.clientY));
        } else if (pending && Math.abs(e.clientX - pending.x) + Math.abs(e.clientY - pending.y) > threshold) {
            startDrag(pending.item, e.clientX, e.clientY);
        }
    }

    function onPointerUp(e) {
        if (dragging) {
            var target = findTarget(e.clientX, e.clientY);
            var targetId = target ? target.getAttribute("data-drop-target") : null;
            endDrag();
            if (targetId) {
                dotNetState.invokeMethodAsync("Drop", targetId);
            } else {
                dotNetState.invokeMethodAsync("EndDrag");
            }
        } else {
            pending = null;
        }
    }

    function onKeyDown(e) {
        if (dragging && e.key === "Escape") {
            endDrag();
            dotNetState.invokeMethodAsync("EndDrag");
        }
    }

    window.basysDragDrop = {
        initialize: function (state) {
            if (dotNetState) {
                return;
            }
            dotNetState = state;
            document.addEventListener("pointerdown", onPointerDown, true);
            document.addEventListener("pointermove", onPointerMove, true);
            document.addEventListener("pointerup", onPointerUp, true);
            document.addEventListener("keydown", onKeyDown, true);
        }
    };
})();
