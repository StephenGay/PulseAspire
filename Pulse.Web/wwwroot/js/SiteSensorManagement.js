/* ===== Toast ===== */
function showToast(message, type) {
    type = type || 'success';
    var toast = document.getElementById('toast');
    toast.textContent = message;
    toast.className = 'toast ' + type + ' show';
    setTimeout(function () { toast.className = 'toast'; }, 3000);
}

/* ===== Time Formatting ===== */
function timeAgo(dateStr) {
    if (!dateStr) return 'Never';
    var d = new Date(dateStr);
    var now = new Date();
    var diff = Math.floor((now - d) / 1000);
    if (diff < 60) return diff + 's ago';
    if (diff < 3600) return Math.floor(diff / 60) + 'm ago';
    if (diff < 86400) return Math.floor(diff / 3600) + 'h ago';
    return Math.floor(diff / 86400) + 'd ago';
}

/* ===== Debounce ===== */
function debounce(fn, delay) {
    var timer;
    return function () {
        var args = arguments;
        var context = this;
        clearTimeout(timer);
        timer = setTimeout(function () { fn.apply(context, args); }, delay);
    };
}

/* ===== Network Scan (browser-based) ===== */
var scanRunning = false;
var discoveredSensors = [];

function startNetworkScan() {
    if (scanRunning) return;
    scanRunning = true;
    var btn = document.getElementById('scanBtn');
    btn.disabled = true;
    btn.innerHTML = '<span>⏳</span> Scanning...';

    var progress = document.getElementById('scanProgress');
    progress.classList.add('active');
    var log = document.getElementById('scanLog');
    var bar = document.getElementById('scanBarFill');
    log.innerHTML = '';
    bar.style.width = '0%';
    discoveredSensors = [];

    addScanLog('info', 'Starting network scan...');

    // Call server-side scan API which simulates discovery
    fetch('/api/sensors/scan', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
    })
        .then(function (r) { return r.json(); })
        .then(function (data) {
            if (data.error) {
                addScanLog('warn', 'Error: ' + data.error);
                scanDone();
                return;
            }
            // Animate discovery messages
            var steps = data.scan_steps || [];
            var i = 0;
            function nextStep() {
                if (i >= steps.length) {
                    bar.style.width = '100%';
                    addScanLog('info', '--- Scan complete. Found ' + (data.discovered || []).length + ' sensor(s). ---');
                    if ((data.discovered || []).length > 0) {
                        addScanLog('found', 'New sensors added! Reloading page...');
                        setTimeout(function () { location.reload(); }, 1200);
                    }
                    scanDone();
                    return;
                }
                var step = steps[i];
                bar.style.width = Math.round(((i + 1) / steps.length) * 100) + '%';
                addScanLog(step.type || 'info', step.message);
                i++;
                setTimeout(nextStep, 400 + Math.random() * 300);
            }
            setTimeout(nextStep, 500);
        })
        .catch(function (err) {
            addScanLog('warn', 'Scan failed: ' + err.message);
            scanDone();
        });
}

function addScanLog(type, msg) {
    var log = document.getElementById('scanLog');
    var line = document.createElement('div');
    line.className = type;
    var ts = new Date().toLocaleTimeString();
    line.textContent = '[' + ts + '] ' + msg;
    log.appendChild(line);
    log.scrollTop = log.scrollHeight;
}

function scanDone() {
    scanRunning = false;
    var btn = document.getElementById('scanBtn');
    btn.disabled = false;
    btn.innerHTML = '<span>🔍</span> Scan Network';
}

/* ===== Add Sensor Modal ===== */
function openAddSensorModal(type) {
    document.getElementById('addSensorModal').classList.add('open');
    if (type) {
        document.getElementById('sensorType').value = type;
    }
    updateSensorForm();
}

function closeAddSensorModal() {
    document.getElementById('addSensorModal').classList.remove('open');
}

function updateSensorForm() {
    var type = document.getElementById('sensorType').value;
    var grpIp = document.getElementById('grpIp');
    var grpPort = document.getElementById('grpPort');
    var grpUrl = document.getElementById('grpUrl');
    var urlHint = document.getElementById('urlHint');
    var portInput = document.getElementById('sensorPort');

    // Show/hide fields based on type
    grpIp.style.display = 'block';
    grpPort.style.display = 'block';
    grpUrl.style.display = 'block';

    switch (type) {
        case 'rtsp':
            portInput.placeholder = '554';
            urlHint.textContent = 'e.g. rtsp://user:pass@192.168.1.100:554/stream1';
            break;
        case 'camera':
        case 'onvif':
            portInput.placeholder = '80';
            urlHint.textContent = 'e.g. http://192.168.1.100/snapshot or ONVIF endpoint';
            break;
        case 'mqtt':
            portInput.placeholder = '1883';
            urlHint.textContent = 'e.g. mqtt://broker:1883/topic/sensor1';
            grpIp.querySelector('label').textContent = 'Broker IP';
            break;
        case 'http':
            portInput.placeholder = '80';
            urlHint.textContent = 'e.g. http://192.168.1.50/api/data';
            break;
        case 'usb':
            grpIp.style.display = 'none';
            grpPort.style.display = 'none';
            urlHint.textContent = 'e.g. /dev/video0 or USB device index (0, 1, ...)';
            break;
        case 'audio':
            grpIp.style.display = 'none';
            grpPort.style.display = 'none';
            urlHint.textContent = 'e.g. hw:0,0 or audio device index';
            break;
        case 'gpio':
            grpPort.style.display = 'none';
            grpUrl.style.display = 'none';
            grpIp.querySelector('label').textContent = 'GPIO Pin(s)';
            break;
    }
}

function submitSensor(e) {
    if (e) e.preventDefault();
    var payload = {
        sensor_type: document.getElementById('sensorType').value,
        name: document.getElementById('sensorName').value,
        ip_address: document.getElementById('sensorIp').value || null,
        port: document.getElementById('sensorPort').value ? parseInt(document.getElementById('sensorPort').value) : null,
        url: document.getElementById('sensorUrl').value || null,
        edge_id: document.getElementById('sensorEdge').value || null,
        zone_name: document.getElementById('sensorZone').value || null,
    };
    if (!payload.name) {
        showToast('Sensor name is required', 'error');
        return;
    }
    fetch('/api/sensors', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    })
        .then(function (r) { return r.json(); })
        .then(function (data) {
            if (data.error) {
                showToast(data.error, 'error');
            } else {
                showToast('Sensor added: ' + payload.name, 'success');
                closeAddSensorModal();
                setTimeout(function () { location.reload(); }, 500);
            }
        })
        .catch(function (err) { showToast('Failed: ' + err.message, 'error'); });
}

/* ===== Sensor Actions ===== */
function activateSensor(sensorId) {
    fetch('/api/sensors/' + sensorId + '/activate', { method: 'POST' })
        .then(function (r) { return r.json(); })
        .then(function (data) {
            showToast(data.status === 'ok' ? 'Sensor activated!' : (data.error || 'Error'), data.status === 'ok' ? 'success' : 'error');
            if (data.status === 'ok') setTimeout(function () { location.reload(); }, 500);
        });
}

function testSensor(sensorId) {
    showToast('Testing sensor connection...', 'info');
    fetch('/api/sensors/' + sensorId + '/test', { method: 'POST' })
        .then(function (r) { return r.json(); })
        .then(function (data) {
            showToast(data.reachable ? '✓ Sensor is reachable!' : '✗ Sensor not reachable', data.reachable ? 'success' : 'error');
        });
}

function removeSensor(sensorId) {
    if (!confirm('Remove this sensor point?')) return;
    fetch('/api/sensors/' + sensorId, { method: 'DELETE' })
        .then(function (r) { return r.json(); })
        .then(function (data) {
            showToast('Sensor removed', 'success');
            setTimeout(function () { location.reload(); }, 400);
        });
}

function editSensor(sensorId) {
    // For now, redirect to setup with edit modal
    showToast('Edit coming soon — use remove + re-add for now', 'info');
}

function openSetupGuide() {
    document.getElementById('setupGuideModal').classList.add('open');
}

// Close modals on overlay click
document.querySelectorAll('.modal-overlay').forEach(function (el) {
    el.addEventListener('click', function (e) {
        if (e.target === el) el.classList.remove('open');
    });
});

// Init form
updateSensorForm();