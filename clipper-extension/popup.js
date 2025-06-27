const checkbox = document.getElementById("toggle-enable");

chrome.storage.local.get(["ytc_enabled"], (result) => {
    const enabled = result.ytc_enabled ?? true;
    checkbox.checked = enabled;
});

checkbox.addEventListener("change", (e) => {
    const enabled = e.target.checked;
    const id = chrome.runtime.id;

    chrome.management.get(id, function (ex) {
        chrome.management.setEnabled(id, enabled);
    });
});