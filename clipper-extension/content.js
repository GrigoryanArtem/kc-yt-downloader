class VideoCutter {
    constructor() {
        this.startTime = null;
        this.endTime = null;
        this.segments = [];
        this.isInitialized = false;

        this.init();
    }

    init() {
        if (this.isInitialized) return;

        this.injectStylesheet();
        this.setupMutationObserver();
        this.setupEventListeners();
        this.isInitialized = true;
    }

    injectStylesheet() {
        if (document.getElementById('yt-cut-stylesheet')) return;

        const link = document.createElement('link');
        link.id = 'yt-cut-stylesheet';
        link.rel = 'stylesheet';
        link.href = chrome.runtime.getURL('style.css');
        document.head.appendChild(link);
    }

    setupMutationObserver() {
        const observer = new MutationObserver(() => {
            if (document.querySelector('.ytp-left-controls') && !document.getElementById('cut-start-btn')) {
                this.injectUI();
            }
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    }

    setupEventListeners() {
        document.body.addEventListener('click', this.handlePanelClick.bind(this));
    }

    injectUI() {
        this.injectControlButtons();
        this.injectPanel();
    }

    injectControlButtons() {
        const controls = document.querySelector('.ytp-left-controls');
        if (!controls || document.getElementById('cut-start-btn')) return;

        const startBtn = this.createButton({
            id: 'cut-start-btn',
            text: 'Cut Start',
            onClick: this.handleStartCut.bind(this)
        });

        const endBtn = this.createButton({
            id: 'cut-end-btn',
            text: 'Cut End',
            onClick: this.handleEndCut.bind(this)
        });

        controls.appendChild(startBtn);
        controls.appendChild(endBtn);
    }

    createButton({ id, text, onClick }) {
        const btn = document.createElement('button');
        btn.id = id;
        btn.className = 'yt-cut-btn';
        btn.textContent = text;
        btn.addEventListener('click', onClick);
        return btn;
    }

    injectPanel() {
        if (document.getElementById('yt-cut-panel')) return;

        const panel = document.createElement('div');
        panel.id = 'yt-cut-panel';
        panel.innerHTML = `
      <div id="yt-cut-panel-header">Segments</div>
      <table id="yt-cut-table"></table>
      <div id="yt-cut-panel-buttons">
        <button id="yt-cut-toggle-select">All</button>
        <button id="yt-cut-send">Send</button>
      </div>
    `;
        document.body.appendChild(panel);
        this.makePanelDraggable(panel, panel.querySelector('#yt-cut-panel-header'));
    }

    makePanelDraggable(panel, handle) {
        let offsetX = 0;
        let offsetY = 0;
        let isDragging = false;

        const handleMouseDown = (e) => {
            isDragging = true;
            offsetX = e.clientX - panel.offsetLeft;
            offsetY = e.clientY - panel.offsetTop;
            document.body.style.userSelect = 'none';
        };

        const handleMouseMove = (e) => {
            if (!isDragging) return;
            panel.style.left = `${e.clientX - offsetX}px`;
            panel.style.top = `${e.clientY - offsetY}px`;
            panel.style.right = 'auto';
        };

        const handleMouseUp = () => {
            isDragging = false;
            document.body.style.userSelect = '';
        };

        handle.addEventListener('mousedown', handleMouseDown);
        document.addEventListener('mousemove', handleMouseMove);
        document.addEventListener('mouseup', handleMouseUp);

        panel.addEventListener('unload', () => {
            handle.removeEventListener('mousedown', handleMouseDown);
            document.removeEventListener('mousemove', handleMouseMove);
            document.removeEventListener('mouseup', handleMouseUp);
        });
    }

    handleStartCut() {
        this.removeHighlightBar();
        const video = this.getVideoElement();
        this.startTime = Math.floor(video.currentTime);
        this.createStartMarker((this.startTime / video.duration) * 100);
    }

    handleEndCut() {
        const marker = document.getElementById('yt-cut-start-flag');
        if (marker) marker.remove();

        const video = this.getVideoElement();
        this.endTime = Math.floor(video.currentTime);

        this.addHighlightBar(this.startTime, this.endTime, `highlight-${this.startTime}-${this.endTime}`);
        this.addSegmentToTable(this.startTime, this.endTime);
    }

    getVideoElement() {
        return document.querySelector('video');
    }

    createStartMarker(positionPercent) {
        const container = document.querySelector('.ytp-progress-bar-container');
        if (!container) return;

        this.removeElementById('yt-cut-start-flag');

        const marker = document.createElement('div');
        marker.id = 'yt-cut-start-flag';
        Object.assign(marker.style, {
            position: 'absolute',
            left: `${positionPercent}%`,
            bottom: '0',
            width: '20px',
            height: '60px',
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            pointerEvents: 'none',
            zIndex: '9999',
            transform: 'translateX(-50%)'
        });

        const line = document.createElement('div');
        Object.assign(line.style, {
            width: '3px',
            height: '40px',
            backgroundColor: '#FF4081',
            borderRadius: '1px'
        });

        const label = document.createElement('div');
        label.textContent = 'Start';
        Object.assign(label.style, {
            color: '#FF4081',
            fontSize: '10px',
            fontWeight: 'bold',
            marginBottom: '2px'
        });

        marker.appendChild(label);
        marker.appendChild(line);
        container.style.position = 'relative';
        container.appendChild(marker);
    }

    addHighlightBar(startSec, endSec, id = 'yt-cut-highlight') {
        const video = this.getVideoElement();
        const duration = video?.duration;
        if (!duration) return;

        const container = document.querySelector('.ytp-progress-bar-container');
        if (!container) return;

        this.removeElementById(id);

        const bar = document.createElement('div');
        bar.id = id;
        const percentStart = (startSec / duration) * 100;
        const percentEnd = (endSec / duration) * 100;

        Object.assign(bar.style, {
            position: 'absolute',
            top: '-6px',
            height: '18px',
            background: 'rgba(137, 100, 255, 0.9)',
            pointerEvents: 'none',
            zIndex: '10',
            left: `${percentStart}%`,
            width: `${percentEnd - percentStart}%`,
            borderRadius: '2px'
        });

        container.style.position = 'relative';
        container.appendChild(bar);
    }

    removeHighlightBar() {
        this.removeElementById('yt-cut-highlight');
    }

    removeElementById(id) {
        const el = document.getElementById(id);
        if (el) el.remove();
    }

    addSegmentToTable(start, end) {
        const videoId = new URLSearchParams(window.location.search).get('v');
        const rowId = `cut-row-${start}-${end}`;
        const table = document.getElementById('yt-cut-table');

        if (document.getElementById(rowId)) return;

        const row = document.createElement('tr');
        row.id = rowId;
        row.dataset.start = start;
        row.dataset.end = end;

        const checkboxCell = document.createElement('td');
        const checkbox = document.createElement('input');
        checkbox.type = 'checkbox';
        checkbox.checked = true;
        checkbox.addEventListener('change', (e) => this.handleCheckboxChange(e));
        checkboxCell.appendChild(checkbox);

        const textCell = document.createElement('td');
        const duration = this.formatDuration(end - start);
        textCell.textContent = `${this.formatTime(start)} - ${this.formatTime(end)} (${duration})`;

        const deleteCell = document.createElement('td');
        deleteCell.innerHTML = '&times;';
        Object.assign(deleteCell.style, {
            cursor: 'pointer',
            color: '#f33',
            fontWeight: 'bold',
            fontSize: '18px',
            textAlign: 'center',
            width: '16px',
            userSelect: 'none'
        });
        deleteCell.title = 'Remove segment';

        deleteCell.addEventListener('click', () => {
            this.removeElementById(`highlight-${start}-${end}`);
            row.remove();
            this.segments = this.segments.filter(seg => seg.start !== start || seg.end !== end);
        });

        row.appendChild(checkboxCell);
        row.appendChild(textCell);
        row.appendChild(deleteCell);
        table.appendChild(row);

        this.segments.push({ start, end, videoId, rowId });
    }

    handleCheckboxChange(e) {
        const row = e.target.closest('tr');
        const start = row.dataset.start;
        const end = row.dataset.end;
        const highlight = document.getElementById(`highlight-${start}-${end}`);

        if (highlight) {
            highlight.style.background = e.target.checked
                ? 'rgba(137, 100, 255, 0.9)'
                : 'rgba(255, 255, 255, 0.3)';
        }
    }

    handlePanelClick(e) {
        switch (e.target.id) {
            case 'yt-cut-send':
                this.handleSendSegments();
                break;
            case 'yt-cut-toggle-select':
                this.handleToggleSelect();
                break;
        }
    }

    handleSendSegments() {
        const videoId = new URLSearchParams(window.location.search).get('v');
        const rows = document.querySelectorAll('#yt-cut-table tr');
        const parts = [];

        rows.forEach((row) => {
            const cb = row.querySelector('input[type="checkbox"]');
            if (cb?.checked) {
                const text = row.cells[1].textContent.replace(/\(.*?\)/, '').trim();
                const [from, to] = text.split(' - ');
                parts.push({
                    start: this.parseTime(from),
                    end: this.parseTime(to),
                });
            }
        });

        const payload = {
            id: videoId,
            parts: parts,
        };

        const json = JSON.stringify(payload);

        fetch('http://localhost:5000/api/cut', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: json
        })
            .catch(error => {
                console.error('Error:', error);
                alert('Failed to send segments. Try to restart desktop app.');
            });
    }

    handleToggleSelect() {
        const rows = document.querySelectorAll('#yt-cut-table tr');
        const allSelected = [...rows].every(row => row.querySelector('input')?.checked);

        rows.forEach((row) => {
            const cb = row.querySelector('input');
            if (cb) {
                cb.checked = !allSelected;
                cb.dispatchEvent(new Event('change'));
            }
        });

        const toggleBtn = document.getElementById('yt-cut-toggle-select');
        toggleBtn.textContent = allSelected ? 'All' : 'None';
    }

    formatDuration(seconds) {
        if (seconds < 60) {
            return `${seconds}s`;
        }

        const h = Math.floor(seconds / 3600);
        const m = Math.floor((seconds % 3600) / 60);
        const s = seconds % 60;

        const parts = [];
        if (h > 0) parts.push(`${h}h`);
        if (m > 0) parts.push(`${m}m`);
        if (s > 0 || parts.length === 0) parts.push(`${s}s`);

        return parts.join(' ');
    }

    formatTime(seconds) {
        const h = String(Math.floor(seconds / 3600)).padStart(2, '0');
        const m = String(Math.floor((seconds % 3600) / 60)).padStart(2, '0');
        const s = String(seconds % 60).padStart(2, '0');
        return `${h}:${m}:${s}`;
    }

    parseTime(str) {
        const [h, m, s] = str.split(':').map(Number);
        return h * 3600 + m * 60 + s;
    }
}

if (document.readyState === 'complete' || document.readyState === 'interactive') {
    new VideoCutter();
} else {
    document.addEventListener('DOMContentLoaded', () => new VideoCutter());
}