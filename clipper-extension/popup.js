const checkbox = document.getElementById("toggle-enable");

chrome.storage.local.get(["ytc_enabled"], (result) => {
    checkbox.checked = result.ytc_enabled ?? true;
});

checkbox.addEventListener('change', (e) => {
    chrome.storage.local.set({ ytc_enabled: e.target.checked });
});