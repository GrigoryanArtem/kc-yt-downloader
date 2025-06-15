let start = null;
let end = null;

function injectButtons() {
    const controls = document.querySelector(".ytp-left-controls");
    if (!controls || document.getElementById("cut-start-btn")) return;

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

        addHighlightBar(start, end);

        const videoId = new URLSearchParams(window.location.search).get("v");
        const data = { videoId, start, end };

        fetch("http://localhost:5000/api/cut", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        });
    };

    controls.appendChild(startBtn);
    controls.appendChild(endBtn);
}

function addHighlightBar(startSec, endSec) {
    const video = document.querySelector("video");
    const duration = video?.duration;
    if (!duration) return;

    const container = document.querySelector(".ytp-progress-bar-container");
    if (!container) return;

    removeHighlightBar();

    const bar = document.createElement("div");
    bar.id = "yt-cut-highlight";

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

setInterval(injectButtons, 1000);


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