
let start = null;
let end = null;

function injectButtons() {
    const controls = document.querySelector(".ytp-left-controls");
    if (!controls || document.getElementById("cut-start-btn")) return;

    injectPanel();

    const startBtn = document.createElement("button");
    startBtn.id = "cut-start-btn";
    startBtn.innerText = "Cut Start";
    startBtn.className = "yt-cut-btn";
    startBtn.onclick = () => {
        removeHighlightBar();

        const video = document.querySelector("video");
        start = Math.floor(video.currentTime);

        const percentStart = (start / video.duration) * 100;
        createStartMarker(percentStart);
    };

    const endBtn = document.createElement("button");
    endBtn.innerText = "Cut End";
    endBtn.className = "yt-cut-btn";
    endBtn.onclick = () => {
        const marker = document.getElementById("yt-cut-start-flag");
        if (marker) marker.remove();

        const video = document.querySelector("video");
        end = Math.floor(video.currentTime);

        addHighlightBar(start, end, `highlight-${start}-${end}`);

        const videoId = new URLSearchParams(window.location.search).get("v");
        const data = { videoId, start, end };

        const rowId = `cut-row-${start}-${end}`;
        const table = document.getElementById("yt-cut-table");

        const row = document.createElement("tr");
        row.id = rowId;
        row.dataset.start = start;
        row.dataset.end = end;

        const checkboxCell = document.createElement("td");
        const checkbox = document.createElement("input");
        checkbox.type = "checkbox";
        checkbox.checked = true;
        checkbox.onchange = (e) => {
            const row = e.target.closest("tr");
            const s = row.dataset.start;
            const e_ = row.dataset.end;
            const highlight = document.getElementById(`highlight-${s}-${e_}`);
            if (highlight) {
                highlight.style.background = e.target.checked
                    ? "rgba(137, 100, 255, 0.9)"
                    : "rgba(255, 255, 255, 0.3)";
            }
        };
        checkboxCell.appendChild(checkbox);

        const textCell = document.createElement("td");
        const duration = formatDuration(end - start);
        textCell.textContent = `${formatTime(start)} - ${formatTime(end)} (${duration})`;

        row.appendChild(checkboxCell);
        row.appendChild(textCell);
        table.appendChild(row);
    };

    controls.appendChild(startBtn);
    controls.appendChild(endBtn);
}

function injectPanel() {
    if (document.getElementById("yt-cut-panel")) return;

    const panel = document.createElement("div");
    panel.id = "yt-cut-panel";
    panel.innerHTML = `
        <table id="yt-cut-table"></table>
        <div id="yt-cut-panel-buttons">
            <button id="yt-cut-send">Send</button>
            <button id="yt-cut-delete">Delete</button>
            <button id="yt-cut-toggle-select">Select All</button>
        </div>
    `;
    document.body.appendChild(panel);
}

function addHighlightBar(startSec, endSec, id = "yt-cut-highlight") {
    const video = document.querySelector("video");
    const duration = video?.duration;
    if (!duration) return;

    const container = document.querySelector(".ytp-progress-bar-container");
    if (!container) return;

    const existing = document.getElementById(id);
    if (existing) existing.remove();

    const bar = document.createElement("div");
    bar.id = id;

    const percentStart = (startSec / duration) * 100;
    const percentEnd = (endSec / duration) * 100;

    Object.assign(bar.style, {
        position: "absolute",
        top: "-6px",
        height: "18px",
        background: "rgba(137, 100, 255, 0.9)",
        pointerEvents: "none",
        zIndex: "10",
        left: `${percentStart}%`,
        width: `${percentEnd - percentStart}%`,
        borderRadius: "2px",
    });

    container.style.position = "relative";
    container.appendChild(bar);
}

function removeHighlightBar() {
    const el = document.getElementById("yt-cut-highlight");
    if (el) el.remove();
}

function createStartMarker(positionPercent) {
    const container = document.querySelector('.ytp-progress-bar-container');
    if (!container) return;

    const existing = document.getElementById('yt-cut-start-flag');
    if (existing) existing.remove();

    const marker = document.createElement('div');
    marker.id = 'yt-cut-start-flag';
    marker.style.position = 'absolute';
    marker.style.left = `${positionPercent}%`;
    marker.style.bottom = '0';
    marker.style.width = '20px';
    marker.style.height = '60px';
    marker.style.display = 'flex';
    marker.style.flexDirection = 'column';
    marker.style.alignItems = 'center';
    marker.style.pointerEvents = 'none';
    marker.style.zIndex = '9999';
    marker.style.transform = 'translateX(-50%)';

    const line = document.createElement('div');
    line.style.width = '3px';
    line.style.height = '40px';
    line.style.backgroundColor = '#FF4081';
    line.style.borderRadius = '1px';

    const label = document.createElement('div');
    label.innerText = 'Start';
    label.style.color = '#FF4081';
    label.style.fontSize = '10px';
    label.style.fontWeight = 'bold';
    label.style.marginBottom = '2px';

    marker.appendChild(label);
    marker.appendChild(line);

    container.style.position = 'relative';
    container.appendChild(marker);
}

function formatDuration(seconds) {
    if (seconds < 60) {
        return `${seconds}s`;
    }

    const h = Math.floor(seconds / 3600);
    const m = Math.floor((seconds % 3600) / 60);
    const s = seconds % 60;

    let parts = [];
    if (h > 0) parts.push(`${h}h`);
    if (m > 0) parts.push(`${m}m`);
    if (s > 0 || parts.length === 0) parts.push(`${s}s`);

    return parts.join(" ");
}

function formatTime(seconds) {
    const h = String(Math.floor(seconds / 3600)).padStart(2, "0");
    const m = String(Math.floor((seconds % 3600) / 60)).padStart(2, "0");
    const s = String(seconds % 60).padStart(2, "0");
    return `${h}:${m}:${s}`;
}

function parseTime(str) {
    const [h, m, s] = str.split(":").map(Number);
    return h * 3600 + m * 60 + s;
}

document.body.addEventListener("click", (e) => {
    if (e.target.id === "yt-cut-send") {
        const videoId = new URLSearchParams(window.location.search).get("v");
        const rows = document.querySelectorAll("#yt-cut-table tr");
        const parts = [];

        rows.forEach((row) => {
            const cb = row.querySelector("input[type='checkbox']");
            if (cb && cb.checked) {
                const text = row.cells[1].textContent.replace(/\(.*?\)/, "").trim();
                const [from, to] = text.split(" - ");
                parts.push({
                    start: parseTime(from),
                    end: parseTime(to),
                });
            }
        });

        const payload = {
            id: videoId,
            parts: parts,
        };

        alert(JSON.stringify(payload));

        fetch("http://localhost:5000/api/cut", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });
    }

    if (e.target.id === "yt-cut-delete") {
        const rows = document.querySelectorAll("#yt-cut-table tr");
        rows.forEach((row) => {
            const cb = row.querySelector("input[type='checkbox']");
            if (cb && cb.checked) {
                const times = row.cells[1].textContent.split(" - ");
                document.getElementById(`highlight-${parseTime(times[0])}-${parseTime(times[1])}`)?.remove();
                row.remove();
            }
        });
    }

    if (e.target.id === "yt-cut-toggle-select") {
        const rows = document.querySelectorAll("#yt-cut-table tr");
        const allSelected = [...rows].every(row => row.querySelector("input")?.checked);
        rows.forEach((row) => {
            const cb = row.querySelector("input");
            if (cb) cb.checked = !allSelected;
        });
        e.target.textContent = allSelected ? "Select All" : "Select None";
    }
});

setInterval(injectButtons, 1000);
