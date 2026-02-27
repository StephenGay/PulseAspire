/**
 * Machine Icon Library — Realistic SVG factory equipment
 * 
 * Usage:
 *   getMachineIcon('lathe', 48)           → SVG string for floor plan node
 *   getMachineIcon('conveyor', 32)        → smaller icon for sidebar/picker
 *   MACHINE_TYPES                         → array of all available types
 *   getMachineLabel(type)                 → human-readable label
 *
 * Each icon is a self-contained inline SVG with class hooks for CSS animation:
 *   .machine-svg           — root SVG element
 *   .machine-body          — main body shape (for glow/color)
 *   .machine-moving-part   — parts that animate when running
 *   .machine-indicator     — status LED dot
 */

const MACHINE_ICONS = {

    /* ━━━ CNC Lathe / Machine Tool ━━━ */
    lathe: (s) => `<svg class="machine-svg" width="${s}" height="${s}" viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg">
        <rect class="machine-body" x="6" y="18" width="52" height="32" rx="4" fill="#1e293b" stroke="#475569" stroke-width="1.5"/>
        <rect x="10" y="22" width="22" height="10" rx="2" fill="#0f172a" stroke="#334155" stroke-width="1"/>
        <rect x="34" y="22" width="20" height="10" rx="2" fill="#0f172a" stroke="#334155" stroke-width="1"/>
        <circle class="machine-moving-part" cx="21" cy="40" r="5" fill="#334155" stroke="#64748b" stroke-width="1.5"/>
        <circle cx="21" cy="40" r="2" fill="#0f172a"/>
        <circle class="machine-moving-part" cx="44" cy="40" r="5" fill="#334155" stroke="#64748b" stroke-width="1.5"/>
        <circle cx="44" cy="40" r="2" fill="#0f172a"/>
        <rect x="14" y="14" width="8" height="6" rx="1" fill="#334155" stroke="#475569" stroke-width="1"/>
        <rect x="15" y="15" width="6" height="4" rx="1" fill="#0ea5e9" opacity="0.3"/>
        <line x1="36" y1="26" x2="50" y2="26" stroke="#64748b" stroke-width="1" stroke-dasharray="2 2"/>
        <circle class="machine-indicator" cx="52" cy="22" r="2.5" fill="#64748b"/>
    </svg>`,

    /* ━━━ Conveyor Belt ━━━ */
    conveyor: (s) => `<svg class="machine-svg" width="${s}" height="${s}" viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg">
        <rect class="machine-body" x="4" y="24" width="56" height="20" rx="10" fill="#1e293b" stroke="#475569" stroke-width="1.5"/>
        <circle cx="14" cy="34" r="7" fill="#334155" stroke="#64748b" stroke-width="1.5"/>
        <circle cx="14" cy="34" r="3" fill="#0f172a"/>
        <circle cx="50" cy="34" r="7" fill="#334155" stroke="#64748b" stroke-width="1.5"/>
        <circle cx="50" cy="34" r="3" fill="#0f172a"/>
        <line x1="14" y1="27" x2="50" y2="27" stroke="#475569" stroke-width="1.5"/>
        <line x1="14" y1="41" x2="50" y2="41" stroke="#475569" stroke-width="1.5"/>
        <g class="machine-moving-part">
            <rect x="22" y="25.5" width="3" height="3" rx="0.5" fill="#64748b" opacity="0.6"/>
            <rect x="30" y="25.5" width="3" height="3" rx="0.5" fill="#64748b" opacity="0.6"/>
            <rect x="38" y="25.5" width="3" height="3" rx="0.5" fill="#64748b" opacity="0.6"/>
        </g>
        <rect x="26" y="16" width="12" height="9" rx="2" fill="#334155" stroke="#475569" stroke-width="1"/>
        <rect x="28" y="18" width="8" height="5" rx="1" fill="#f59e0b" opacity="0.15"/>
        <circle class="machine-indicator" cx="55" cy="20" r="2.5" fill="#64748b"/>
    </svg>`,

    /* ━━━ Industrial Door / Entry Gate ━━━ */
    door: (s) => `<svg class="machine-svg" width="${s}" height="${s}" viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg">
        <rect class="machine-body" x="12" y="8" width="40" height="50" rx="3" fill="#1e293b" stroke="#475569" stroke-width="1.5"/>
        <rect x="16" y="12" width="32" height="42" rx="2" fill="#0f172a" stroke="#334155" stroke-width="1"/>
        <g class="machine-moving-part">
            <rect x="16" y="12" width="32" height="21" rx="2" fill="#334155" opacity="0.4"/>
            <line x1="16" y1="33" x2="48" y2="33" stroke="#64748b" stroke-width="1" stroke-dasharray="3 2"/>
        </g>
        <rect x="42" y="30" width="3" height="6" rx="1" fill="#64748b"/>
        <rect x="24" y="4" width="16" height="6" rx="2" fill="#334155" stroke="#475569" stroke-width="1"/>
        <circle class="machine-indicator" cx="32" cy="7" r="2" fill="#64748b"/>
        <circle cx="28" cy="7" r="1" fill="#ef4444" opacity="0.4"/>
        <circle cx="36" cy="7" r="1" fill="#22c55e" opacity="0.4"/>
    </svg>`,

    /* ━━━ Camera / Sensor ━━━ */
    sensor: (s) => `<svg class="machine-svg" width="${s}" height="${s}" viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg">
        <rect x="26" y="34" width="12" height="22" rx="2" fill="#334155" stroke="#475569" stroke-width="1"/>
        <circle class="machine-body" cx="32" cy="26" r="16" fill="#1e293b" stroke="#475569" stroke-width="1.5"/>
        <circle cx="32" cy="26" r="11" fill="#0f172a" stroke="#334155" stroke-width="1"/>
        <circle cx="32" cy="26" r="6" fill="#1e293b" stroke="#475569" stroke-width="1"/>
        <circle cx="32" cy="26" r="3" fill="#0ea5e9" opacity="0.3"/>
        <g class="machine-moving-part" opacity="0.4">
            <path d="M32 8 L28 4 L36 4 Z" fill="#64748b"/>
        </g>
        <circle class="machine-indicator" cx="44" cy="14" r="2.5" fill="#64748b"/>
    </svg>`,

    /* ━━━ Generic Machine / Motor ━━━ */
    machine: (s) => `<svg class="machine-svg" width="${s}" height="${s}" viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg">
        <rect class="machine-body" x="8" y="16" width="48" height="36" rx="4" fill="#1e293b" stroke="#475569" stroke-width="1.5"/>
        <rect x="12" y="20" width="20" height="14" rx="2" fill="#0f172a" stroke="#334155" stroke-width="1"/>
        <g class="machine-moving-part">
            <circle cx="22" cy="27" r="4" fill="none" stroke="#64748b" stroke-width="1.5"/>
            <line x1="22" y1="23" x2="22" y2="31" stroke="#64748b" stroke-width="1"/>
            <line x1="18" y1="27" x2="26" y2="27" stroke="#64748b" stroke-width="1"/>
        </g>
        <rect x="36" y="20" width="16" height="6" rx="1" fill="#334155"/>
        <rect x="36" y="28" width="16" height="6" rx="1" fill="#334155"/>
        <rect x="38" y="21" width="12" height="4" rx="1" fill="#0ea5e9" opacity="0.15"/>
        <rect x="38" y="29" width="8" height="4" rx="1" fill="#22c55e" opacity="0.15"/>
        <rect x="12" y="40" width="40" height="8" rx="2" fill="#0f172a" stroke="#334155" stroke-width="1"/>
        <circle cx="18" cy="44" r="2" fill="#475569"/>
        <circle cx="26" cy="44" r="2" fill="#475569"/>
        <circle cx="34" cy="44" r="2" fill="#475569"/>
        <circle class="machine-indicator" cx="50" cy="20" r="2.5" fill="#64748b"/>
    </svg>`,

    /* ━━━ Forklift / Vehicle ━━━ */
    forklift: (s) => `<svg class="machine-svg" width="${s}" height="${s}" viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg">
        <rect class="machine-body" x="18" y="18" width="28" height="26" rx="4" fill="#1e293b" stroke="#475569" stroke-width="1.5"/>
        <rect x="8" y="20" width="12" height="4" rx="1" fill="#334155" stroke="#475569" stroke-width="1"/>
        <rect x="6" y="16" width="4" height="30" rx="1" fill="#475569"/>
        <rect x="6" y="14" width="14" height="3" rx="1" fill="#334155" stroke="#475569" stroke-width="1"/>
        <g class="machine-moving-part">
            <rect x="7" y="18" width="2" height="8" rx="0.5" fill="#f59e0b" opacity="0.4"/>
        </g>
        <rect x="22" y="22" width="20" height="12" rx="2" fill="#0f172a" stroke="#334155" stroke-width="1"/>
        <rect x="24" y="24" width="8" height="8" rx="1" fill="#0ea5e9" opacity="0.15"/>
        <circle cx="22" cy="48" r="5" fill="#334155" stroke="#64748b" stroke-width="1.5"/>
        <circle cx="22" cy="48" r="2" fill="#0f172a"/>
        <circle cx="42" cy="48" r="5" fill="#334155" stroke="#64748b" stroke-width="1.5"/>
        <circle cx="42" cy="48" r="2" fill="#0f172a"/>
        <circle class="machine-indicator" cx="42" cy="22" r="2.5" fill="#64748b"/>
    </svg>`,

    /* ━━━ Warehouse / Storage ━━━ */
    warehouse: (s) => `<svg class="machine-svg" width="${s}" height="${s}" viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg">
        <path class="machine-body" d="M8 24L32 8L56 24V56H8V24Z" fill="#1e293b" stroke="#475569" stroke-width="1.5"/>
        <rect x="14" y="28" width="16" height="12" rx="1" fill="#0f172a" stroke="#334155" stroke-width="1"/>
        <rect x="34" y="28" width="16" height="12" rx="1" fill="#0f172a" stroke="#334155" stroke-width="1"/>
        <line x1="14" y1="34" x2="30" y2="34" stroke="#334155" stroke-width="0.5"/>
        <line x1="34" y1="34" x2="50" y2="34" stroke="#334155" stroke-width="0.5"/>
        <rect x="24" y="42" width="16" height="14" rx="2" fill="#0f172a" stroke="#334155" stroke-width="1"/>
        <g class="machine-moving-part">
            <line x1="32" y1="44" x2="32" y2="54" stroke="#475569" stroke-width="1"/>
        </g>
        <circle class="machine-indicator" cx="32" cy="18" r="2.5" fill="#64748b"/>
    </svg>`,

    /* ━━━ Fire / Hazard Detector ━━━ */
    fire: (s) => `<svg class="machine-svg" width="${s}" height="${s}" viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg">
        <circle class="machine-body" cx="32" cy="32" r="22" fill="#1e293b" stroke="#475569" stroke-width="1.5"/>
        <circle cx="32" cy="32" r="16" fill="#0f172a" stroke="#334155" stroke-width="1"/>
        <g class="machine-moving-part">
            <path d="M32 20C32 20 24 28 24 34C24 38.4 27.6 42 32 42C36.4 42 40 38.4 40 34C40 28 32 20 32 20Z" fill="#ef4444" opacity="0.25"/>
            <path d="M32 26C32 26 28 30 28 34C28 36.2 29.8 38 32 38C34.2 38 36 36.2 36 34C36 30 32 26 32 26Z" fill="#f59e0b" opacity="0.3"/>
        </g>
        <circle class="machine-indicator" cx="48" cy="16" r="2.5" fill="#64748b"/>
    </svg>`,

    /* ━━━ Light / Illumination ━━━ */
    light: (s) => `<svg class="machine-svg" width="${s}" height="${s}" viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg">
        <rect x="28" y="40" width="8" height="12" rx="2" fill="#334155" stroke="#475569" stroke-width="1"/>
        <rect x="26" y="50" width="12" height="4" rx="2" fill="#334155" stroke="#475569" stroke-width="1"/>
        <path class="machine-body" d="M20 28C20 21.4 25.4 16 32 16C38.6 16 44 21.4 44 28C44 33 41 37 37 39H27C23 37 20 33 20 28Z" fill="#1e293b" stroke="#475569" stroke-width="1.5"/>
        <path d="M24 28C24 23.6 27.6 20 32 20C36.4 20 40 23.6 40 28C40 31.5 38 34.5 35 36H29C26 34.5 24 31.5 24 28Z" fill="#0f172a"/>
        <g class="machine-moving-part" opacity="0.3">
            <line x1="32" y1="8" x2="32" y2="13" stroke="#f59e0b" stroke-width="1.5" stroke-linecap="round"/>
            <line x1="18" y1="14" x2="21" y2="18" stroke="#f59e0b" stroke-width="1.5" stroke-linecap="round"/>
            <line x1="46" y1="14" x2="43" y2="18" stroke="#f59e0b" stroke-width="1.5" stroke-linecap="round"/>
            <line x1="14" y1="28" x2="18" y2="28" stroke="#f59e0b" stroke-width="1.5" stroke-linecap="round"/>
            <line x1="46" y1="28" x2="50" y2="28" stroke="#f59e0b" stroke-width="1.5" stroke-linecap="round"/>
        </g>
        <circle class="machine-indicator" cx="32" cy="28" r="3" fill="#64748b"/>
    </svg>`,

    /* ━━━ Security Guard / Access ━━━ */
    guard: (s) => `<svg class="machine-svg" width="${s}" height="${s}" viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg">
        <path class="machine-body" d="M32 6L14 16V32C14 45 22 54 32 58C42 54 50 45 50 32V16L32 6Z" fill="#1e293b" stroke="#475569" stroke-width="1.5"/>
        <path d="M32 12L18 20V32C18 42 24 50 32 54C40 50 46 42 46 32V20L32 12Z" fill="#0f172a" stroke="#334155" stroke-width="0.75"/>
        <g class="machine-moving-part">
            <path d="M28 30L31 33L38 26" stroke="#22c55e" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" opacity="0.5"/>
        </g>
        <circle class="machine-indicator" cx="32" cy="22" r="2.5" fill="#64748b"/>
    </svg>`,

    /* ━━━ Pump / Compressor ━━━ */
    pump: (s) => `<svg class="machine-svg" width="${s}" height="${s}" viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg">
        <rect class="machine-body" x="14" y="22" width="36" height="24" rx="4" fill="#1e293b" stroke="#475569" stroke-width="1.5"/>
        <circle cx="32" cy="34" r="9" fill="#0f172a" stroke="#334155" stroke-width="1.5"/>
        <g class="machine-moving-part">
            <line x1="32" y1="25" x2="32" y2="43" stroke="#64748b" stroke-width="1.5"/>
            <line x1="23" y1="34" x2="41" y2="34" stroke="#64748b" stroke-width="1.5"/>
            <line x1="26" y1="28" x2="38" y2="40" stroke="#64748b" stroke-width="1"/>
            <line x1="38" y1="28" x2="26" y2="40" stroke="#64748b" stroke-width="1"/>
        </g>
        <rect x="4" y="30" width="12" height="8" rx="2" fill="#334155" stroke="#475569" stroke-width="1"/>
        <rect x="48" y="30" width="12" height="8" rx="2" fill="#334155" stroke="#475569" stroke-width="1"/>
        <line x1="4" y1="34" x2="0" y2="34" stroke="#475569" stroke-width="2" stroke-linecap="round"/>
        <line x1="60" y1="34" x2="64" y2="34" stroke="#475569" stroke-width="2" stroke-linecap="round"/>
        <circle class="machine-indicator" cx="50" cy="22" r="2.5" fill="#64748b"/>
    </svg>`,

    /* ━━━ Robotic Arm ━━━ */
    robot: (s) => `<svg class="machine-svg" width="${s}" height="${s}" viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg">
        <rect x="24" y="48" width="16" height="10" rx="3" fill="#334155" stroke="#475569" stroke-width="1.5"/>
        <rect class="machine-body" x="28" y="30" width="8" height="20" rx="2" fill="#1e293b" stroke="#475569" stroke-width="1.5"/>
        <g class="machine-moving-part">
            <rect x="20" y="18" width="24" height="14" rx="3" fill="#1e293b" stroke="#475569" stroke-width="1.5"/>
            <rect x="10" y="10" width="14" height="10" rx="2" fill="#334155" stroke="#475569" stroke-width="1"/>
            <line x1="24" y1="15" x2="20" y2="22" stroke="#64748b" stroke-width="2" stroke-linecap="round"/>
            <circle cx="17" cy="10" r="3" fill="#475569" stroke="#64748b" stroke-width="1"/>
        </g>
        <rect x="30" y="20" width="4" height="4" rx="1" fill="#0ea5e9" opacity="0.3"/>
        <circle class="machine-indicator" cx="40" cy="20" r="2.5" fill="#64748b"/>
    </svg>`,

    /* ━━━ Welding Station ━━━ */
    welder: (s) => `<svg class="machine-svg" width="${s}" height="${s}" viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg">
        <rect class="machine-body" x="10" y="24" width="44" height="28" rx="4" fill="#1e293b" stroke="#475569" stroke-width="1.5"/>
        <rect x="14" y="28" width="18" height="10" rx="2" fill="#0f172a" stroke="#334155" stroke-width="1"/>
        <rect x="34" y="28" width="16" height="10" rx="2" fill="#0f172a" stroke="#334155" stroke-width="1"/>
        <rect x="16" y="30" width="4" height="6" rx="1" fill="#f59e0b" opacity="0.2"/>
        <rect x="22" y="30" width="4" height="6" rx="1" fill="#ef4444" opacity="0.2"/>
        <g class="machine-moving-part">
            <line x1="32" y1="8" x2="32" y2="24" stroke="#64748b" stroke-width="2" stroke-linecap="round"/>
            <circle cx="32" cy="8" r="3" fill="#f59e0b" opacity="0.4"/>
            <circle cx="32" cy="8" r="1.5" fill="#ffffff" opacity="0.3"/>
        </g>
        <rect x="16" y="42" width="32" height="6" rx="1" fill="#0f172a" stroke="#334155" stroke-width="0.75"/>
        <circle class="machine-indicator" cx="48" cy="28" r="2.5" fill="#64748b"/>
    </svg>`,

    /* ━━━ Press / Stamping Machine ━━━ */
    press: (s) => `<svg class="machine-svg" width="${s}" height="${s}" viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg">
        <rect x="14" y="6" width="36" height="10" rx="3" fill="#334155" stroke="#475569" stroke-width="1.5"/>
        <rect class="machine-body" x="10" y="40" width="44" height="18" rx="4" fill="#1e293b" stroke="#475569" stroke-width="1.5"/>
        <rect x="18" y="44" width="28" height="10" rx="2" fill="#0f172a" stroke="#334155" stroke-width="1"/>
        <g class="machine-moving-part">
            <rect x="28" y="16" width="8" height="24" rx="2" fill="#475569" stroke="#64748b" stroke-width="1"/>
            <rect x="24" y="36" width="16" height="6" rx="2" fill="#334155" stroke="#475569" stroke-width="1"/>
        </g>
        <rect x="8" y="14" width="6" height="32" rx="2" fill="#334155" stroke="#475569" stroke-width="1"/>
        <rect x="50" y="14" width="6" height="32" rx="2" fill="#334155" stroke="#475569" stroke-width="1"/>
        <circle class="machine-indicator" cx="50" cy="10" r="2.5" fill="#64748b"/>
    </svg>`,

    /* ━━━ Oven / Furnace ━━━ */
    oven: (s) => `<svg class="machine-svg" width="${s}" height="${s}" viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg">
        <rect class="machine-body" x="8" y="14" width="48" height="40" rx="5" fill="#1e293b" stroke="#475569" stroke-width="1.5"/>
        <rect x="12" y="18" width="40" height="24" rx="3" fill="#0f172a" stroke="#334155" stroke-width="1"/>
        <g class="machine-moving-part">
            <rect x="18" y="36" width="6" height="4" rx="1" fill="#ef4444" opacity="0.4"/>
            <rect x="26" y="34" width="6" height="6" rx="1" fill="#f59e0b" opacity="0.35"/>
            <rect x="34" y="36" width="6" height="4" rx="1" fill="#ef4444" opacity="0.4"/>
            <rect x="42" y="35" width="6" height="5" rx="1" fill="#f59e0b" opacity="0.3"/>
        </g>
        <rect x="12" y="46" width="40" height="4" rx="1" fill="#334155"/>
        <circle cx="16" cy="48" r="1.5" fill="#64748b"/>
        <circle cx="22" cy="48" r="1.5" fill="#64748b"/>
        <circle cx="28" cy="48" r="1.5" fill="#64748b"/>
        <rect x="36" y="46" width="12" height="4" rx="1" fill="#0ea5e9" opacity="0.15"/>
        <line x1="14" y1="30" x2="50" y2="30" stroke="#334155" stroke-width="0.5" stroke-dasharray="2 2"/>
        <circle class="machine-indicator" cx="50" cy="18" r="2.5" fill="#64748b"/>
    </svg>`,
};

/* ━━━ Machine type registry ━━━ */
const MACHINE_TYPES = [
    { key: 'machine', label: 'Machine', category: 'Equipment' },
    { key: 'lathe', label: 'CNC Lathe', category: 'Equipment' },
    { key: 'conveyor', label: 'Conveyor', category: 'Equipment' },
    { key: 'press', label: 'Press', category: 'Equipment' },
    { key: 'welder', label: 'Welder', category: 'Equipment' },
    { key: 'pump', label: 'Pump', category: 'Equipment' },
    { key: 'robot', label: 'Robot Arm', category: 'Equipment' },
    { key: 'oven', label: 'Oven / Furnace', category: 'Equipment' },
    { key: 'sensor', label: 'Camera / Sensor', category: 'Monitoring' },
    { key: 'fire', label: 'Fire Detector', category: 'Monitoring' },
    { key: 'light', label: 'Light', category: 'Monitoring' },
    { key: 'guard', label: 'Security', category: 'Access' },
    { key: 'door', label: 'Door / Gate', category: 'Access' },
    { key: 'forklift', label: 'Forklift', category: 'Vehicles' },
    { key: 'warehouse', label: 'Warehouse', category: 'Zones' },
];

function getMachineIcon(type, size = 48) {
    const fn = MACHINE_ICONS[type] || MACHINE_ICONS['machine'];
    return fn(size);
}

function getMachineLabel(type) {
    const entry = MACHINE_TYPES.find(t => t.key === type);
    return entry ? entry.label : 'Machine';
}

/* ━━━ CSS Animations (injected once) ━━━ */
(function injectAnimationStyles() {
    if (document.getElementById('machine-animation-css')) return;
    const style = document.createElement('style');
    style.id = 'machine-animation-css';
    style.textContent = `
        /* === RUNNING STATE: spinning/moving parts === */
        .machine-state-on .machine-moving-part {
            animation: machine-spin 2s linear infinite;
            transform-origin: center;
        }
        .machine-state-on .machine-indicator { fill: #22c55e; }
        .machine-state-on .machine-body { filter: drop-shadow(0 0 4px rgba(34,197,94,0.2)); }

        /* === IDLE / OFF: dim === */
        .machine-state-off .machine-svg,
        .machine-state-idle .machine-svg,
        .machine-state-unknown .machine-svg {
            opacity: 0.55;
        }
        .machine-state-off .machine-indicator,
        .machine-state-idle .machine-indicator { fill: #4b5563; }

        /* === ALERT: urgent pulse === */
        .machine-state-alert .machine-indicator { fill: #ef4444; }
        .machine-state-alert .machine-body {
            animation: machine-alert-glow 1s ease-in-out infinite alternate;
        }
        .machine-state-alert .machine-moving-part {
            animation: machine-alert-shake 0.3s ease-in-out infinite;
            transform-origin: center;
        }

        /* === WARNING: amber pulse === */
        .machine-state-warning .machine-indicator { fill: #f59e0b; }
        .machine-state-warning .machine-body {
            animation: machine-warn-glow 1.5s ease-in-out infinite alternate;
        }
        .machine-state-warning .machine-moving-part {
            animation: machine-spin 4s linear infinite;
            transform-origin: center;
        }

        /* === ANOMALY ACTIVE: intense flash === */
        .machine-anomaly-active .machine-body {
            animation: machine-anomaly-flash 0.6s ease-in-out infinite !important;
        }
        .machine-anomaly-active .machine-indicator {
            fill: #ef4444 !important;
            animation: machine-indicator-blink 0.4s infinite !important;
        }
        .machine-anomaly-active .machine-moving-part {
            animation: machine-alert-shake 0.15s ease-in-out infinite !important;
            transform-origin: center;
        }

        /* === Keyframes === */
        @keyframes machine-spin {
            from { transform: rotate(0deg); }
            to { transform: rotate(360deg); }
        }

        @keyframes machine-alert-glow {
            0% { filter: drop-shadow(0 0 3px rgba(239,68,68,0.3)); }
            100% { filter: drop-shadow(0 0 12px rgba(239,68,68,0.6)); }
        }

        @keyframes machine-warn-glow {
            0% { filter: drop-shadow(0 0 2px rgba(245,158,11,0.2)); }
            100% { filter: drop-shadow(0 0 8px rgba(245,158,11,0.4)); }
        }

        @keyframes machine-alert-shake {
            0%, 100% { transform: translateX(0); }
            25% { transform: translateX(-1.5px); }
            75% { transform: translateX(1.5px); }
        }

        @keyframes machine-anomaly-flash {
            0%, 100% { filter: drop-shadow(0 0 6px rgba(239,68,68,0.5)) brightness(1); }
            50% { filter: drop-shadow(0 0 18px rgba(239,68,68,0.8)) brightness(1.3); }
        }

        @keyframes machine-indicator-blink {
            0%, 100% { opacity: 1; }
            50% { opacity: 0.2; }
        }
    `;
    document.head.appendChild(style);
})();


// ===== DATA =====
const assets = [{ "anomaly_count_24h": 2, "asset_id": "hm-vdb-cast-01", "current_state": "idle", "description": "PU casting station - 6-head carousel mould", "floor_x": 10.0, "floor_y": 20.0, "icon": "press", "id": 123, "last_event_at": "2026-02-17T07:11:17", "name": "Casting Bay #1", "work_items": [{ "priority": "high", "product_code": "PU-75-500", "product_name": "PU Casting - 75x500 Nip Roller", "quantity": 120, "quantity_completed": 95, "status": "in_progress", "unit": "pcs", "wo_id": "WO-HM-001", "wo_title": "PU Rollers 75x500 - Sappi Tugela" }, { "priority": "urgent", "product_code": "PU-60-300", "product_name": "PU Casting - 60x300 Guide", "quantity": 200, "quantity_completed": 0, "status": "queued", "unit": "pcs", "wo_id": "WO-HM-003", "wo_title": "PU Conveyor Rollers 60x300 - Anglo American" }] }, { "anomaly_count_24h": 0, "asset_id": "hm-vdb-cast-02", "current_state": "alert", "description": "Rubber casting station - manual pour moulding", "floor_x": 10.0, "floor_y": 45.0, "icon": "press", "id": 124, "last_event_at": "2026-02-17T07:12:17", "name": "Casting Bay #2", "work_items": [{ "priority": "normal", "product_code": "RB-100-800", "product_name": "Rubber Casting - 100x800 Feed", "quantity": 80, "quantity_completed": 60, "status": "in_progress", "unit": "pcs", "wo_id": "WO-HM-002", "wo_title": "Rubber Rollers 100x800 - ArcelorMittal" }, { "priority": "high", "product_code": "RC-150-1200-CAST", "product_name": "Re-Cast Rubber Sleeve", "quantity": 24, "quantity_completed": 14, "status": "in_progress", "unit": "pcs", "wo_id": "WO-HM-004", "wo_title": "Rubber Sleeve Re-Cover - Mondi" }] }, { "anomaly_count_24h": 2, "asset_id": "hm-vdb-oven-01", "current_state": "idle", "description": "Walk-in curing oven - 180C max - PU rollers", "floor_x": 30.0, "floor_y": 20.0, "icon": "oven", "id": 125, "last_event_at": "2026-02-17T07:06:17", "name": "Curing Oven #1", "work_items": [{ "priority": "high", "product_code": "PU-75-500-CURE", "product_name": "PU Curing - 180C / 4hr", "quantity": 120, "quantity_completed": 80, "status": "in_progress", "unit": "pcs", "wo_id": "WO-HM-001", "wo_title": "PU Rollers 75x500 - Sappi Tugela" }, { "priority": "urgent", "product_code": "PU-60-300-CURE", "product_name": "PU Curing - 180C / 3hr", "quantity": 200, "quantity_completed": 0, "status": "queued", "unit": "pcs", "wo_id": "WO-HM-003", "wo_title": "PU Conveyor Rollers 60x300 - Anglo American" }] }, { "anomaly_count_24h": 4, "asset_id": "hm-vdb-oven-02", "current_state": "on", "description": "Tunnel oven - continuous cure - rubber compound", "floor_x": 30.0, "floor_y": 45.0, "icon": "oven", "id": 126, "last_event_at": "2026-02-17T07:08:19", "name": "Curing Oven #2", "work_items": [{ "priority": "normal", "product_code": "RB-100-800-CURE", "product_name": "Rubber Curing - 150C / 6hr", "quantity": 80, "quantity_completed": 45, "status": "in_progress", "unit": "pcs", "wo_id": "WO-HM-002", "wo_title": "Rubber Rollers 100x800 - ArcelorMittal" }, { "priority": "high", "product_code": "RC-150-1200-CURE", "product_name": "Cure Sleeve - 150C", "quantity": 24, "quantity_completed": 8, "status": "in_progress", "unit": "pcs", "wo_id": "WO-HM-004", "wo_title": "Rubber Sleeve Re-Cover - Mondi" }] }, { "anomaly_count_24h": 5, "asset_id": "hm-vdb-lathe-01", "current_state": "on", "description": "CNC roller lathe - precision OD turning \u0026 profiling", "floor_x": 50.0, "floor_y": 15.0, "icon": "lathe", "id": 127, "last_event_at": "2026-02-17T07:07:17", "name": "Lathe #1", "work_items": [{ "priority": "high", "product_code": "PU-75-500-TURN", "product_name": "OD Turning \u0026 Profile", "quantity": 120, "quantity_completed": 62, "status": "in_progress", "unit": "pcs", "wo_id": "WO-HM-001", "wo_title": "PU Rollers 75x500 - Sappi Tugela" }, { "priority": "urgent", "product_code": "PU-60-300-GRIND", "product_name": "Precision OD Grind", "quantity": 200, "quantity_completed": 0, "status": "queued", "unit": "pcs", "wo_id": "WO-HM-003", "wo_title": "PU Conveyor Rollers 60x300 - Anglo American" }, { "priority": "high", "product_code": "RC-150-1200-STRIP", "product_name": "Strip Old Rubber", "quantity": 24, "quantity_completed": 20, "status": "in_progress", "unit": "pcs", "wo_id": "WO-HM-004", "wo_title": "Rubber Sleeve Re-Cover - Mondi" }] }, { "anomaly_count_24h": 8, "asset_id": "hm-vdb-lathe-02", "current_state": "on", "description": "Manual gap-bed lathe - engineering \u0026 re-covers", "floor_x": 50.0, "floor_y": 40.0, "icon": "lathe", "id": 128, "last_event_at": "2026-02-17T07:13:20", "name": "Lathe #2", "work_items": [{ "priority": "normal", "product_code": "RB-100-800-TURN", "product_name": "OD Turning - Crowned", "quantity": 80, "quantity_completed": 18, "status": "in_progress", "unit": "pcs", "wo_id": "WO-HM-002", "wo_title": "Rubber Rollers 100x800 - ArcelorMittal" }, { "priority": "high", "product_code": "RC-150-1200-TURN", "product_name": "Crown Profile Turn", "quantity": 24, "quantity_completed": 4, "status": "in_progress", "unit": "pcs", "wo_id": "WO-HM-004", "wo_title": "Rubber Sleeve Re-Cover - Mondi" }] }, { "anomaly_count_24h": 3, "asset_id": "hm-vdb-rib-01", "current_state": "on", "description": "Helical rib cutting head - spiral groove pattern", "floor_x": 50.0, "floor_y": 65.0, "icon": "machine", "id": 129, "last_event_at": "2026-02-17T07:05:19", "name": "Ribbing Machine", "work_items": [{ "priority": "normal", "product_code": "RB-100-800-RIB", "product_name": "Helical Rib Cut", "quantity": 80, "quantity_completed": 30, "status": "in_progress", "unit": "pcs", "wo_id": "WO-HM-002", "wo_title": "Rubber Rollers 100x800 - ArcelorMittal" }] }, { "anomaly_count_24h": 3, "asset_id": "hm-vdb-sand-01", "current_state": "on", "description": "Cylindrical roller sander - 80 to 400 grit", "floor_x": 70.0, "floor_y": 20.0, "icon": "machine", "id": 130, "last_event_at": "2026-02-17T07:14:20", "name": "Sanding Station #1", "work_items": [{ "priority": "high", "product_code": "PU-75-500-SAND", "product_name": "Finish Sanding 320 grit", "quantity": 120, "quantity_completed": 48, "status": "in_progress", "unit": "pcs", "wo_id": "WO-HM-001", "wo_title": "PU Rollers 75x500 - Sappi Tugela" }, { "priority": "high", "product_code": "RC-150-1200-SAND", "product_name": "Final Polish 400 grit", "quantity": 24, "quantity_completed": 0, "status": "queued", "unit": "pcs", "wo_id": "WO-HM-004", "wo_title": "Rubber Sleeve Re-Cover - Mondi" }] }, { "anomaly_count_24h": 7, "asset_id": "hm-vdb-sand-02", "current_state": "on", "description": "Fine finish sander - mirror polish capability", "floor_x": 70.0, "floor_y": 45.0, "icon": "machine", "id": 131, "last_event_at": "2026-02-17T07:11:19", "name": "Sanding Station #2", "work_items": [{ "priority": "normal", "product_code": "RB-100-800-SAND", "product_name": "Finish Sand 240 grit", "quantity": 80, "quantity_completed": 0, "status": "queued", "unit": "pcs", "wo_id": "WO-HM-002", "wo_title": "Rubber Rollers 100x800 - ArcelorMittal" }] }, { "anomaly_count_24h": 4, "asset_id": "hm-vdb-flow-01", "current_state": "on", "description": "Roller conveyor flow line - casting to dispatch", "floor_x": 45.0, "floor_y": 85.0, "icon": "conveyor", "id": 132, "last_event_at": "2026-02-17T06:38:17", "name": "Flow Line", "work_items": [] }, { "anomaly_count_24h": 5, "asset_id": "hm-vdb-qc-01", "current_state": "on", "description": "Hardness testing, OD measurement, visual check", "floor_x": 85.0, "floor_y": 25.0, "icon": "sensor", "id": 133, "last_event_at": "2026-02-17T07:07:17", "name": "QC Inspection Table", "work_items": [{ "priority": "high", "product_code": "PU-75-500-QC", "product_name": "QC Hardness \u0026 OD Check", "quantity": 120, "quantity_completed": 35, "status": "in_progress", "unit": "pcs", "wo_id": "WO-HM-001", "wo_title": "PU Rollers 75x500 - Sappi Tugela" }] }, { "anomaly_count_24h": 7, "asset_id": "hm-vdb-door-01", "current_state": "on", "description": "Main dispatch roller shutter - truck access", "floor_x": 90.0, "floor_y": 55.0, "icon": "door", "id": 134, "last_event_at": "2026-02-17T07:12:17", "name": "Dispatch Bay Door", "work_items": [] }];
const edges = [{ "cpu_percent": 29.8, "edge_id": "hm-vdb-floor-cam", "last_heartbeat": "2026-02-17T07:15:05", "memory_percent": 52.6, "name": "Production Floor Cam", "software_version": "2.4.1", "status": "online" }, { "cpu_percent": 23.4, "edge_id": "hm-vdb-oven-cam", "last_heartbeat": "2026-02-17T07:13:07", "memory_percent": 60.3, "name": "Oven Bay Cam", "software_version": "2.4.1", "status": "offline" }, { "cpu_percent": 27.6, "edge_id": "hm-vdb-dispatch-cam", "last_heartbeat": "2026-02-17T07:15:08", "memory_percent": 50.2, "name": "Dispatch \u0026 QC Cam", "software_version": "2.4.1", "status": "online" }, { "cpu_percent": 37.2, "edge_id": "hm-vdb-gate-cam", "last_heartbeat": "2026-02-17T07:15:15", "memory_percent": 60.2, "name": "Security Gate Cam", "software_version": "2.3.9", "status": "online" }];
const zoneMappings = [{ "asset_id": "hm-vdb-cast-01", "edge_id": "hm-vdb-floor-cam", "zone_id": "zone_cast1", "zone_name": "Casting Bay 1" }, { "asset_id": "hm-vdb-cast-02", "edge_id": "hm-vdb-floor-cam", "zone_id": "zone_cast2", "zone_name": "Casting Bay 2" }, { "asset_id": "hm-vdb-door-01", "edge_id": "hm-vdb-dispatch-cam", "zone_id": "zone_disp", "zone_name": "Dispatch Door" }, { "asset_id": "hm-vdb-flow-01", "edge_id": "hm-vdb-dispatch-cam", "zone_id": "zone_flow", "zone_name": "Flow Line" }, { "asset_id": "hm-vdb-lathe-01", "edge_id": "hm-vdb-floor-cam", "zone_id": "zone_lathe1", "zone_name": "Lathe 1" }, { "asset_id": "hm-vdb-lathe-02", "edge_id": "hm-vdb-floor-cam", "zone_id": "zone_lathe2", "zone_name": "Lathe 2" }, { "asset_id": "hm-vdb-oven-01", "edge_id": "hm-vdb-oven-cam", "zone_id": "zone_oven1", "zone_name": "Curing Oven 1" }, { "asset_id": "hm-vdb-oven-02", "edge_id": "hm-vdb-oven-cam", "zone_id": "zone_oven2", "zone_name": "Curing Oven 2" }, { "asset_id": "hm-vdb-qc-01", "edge_id": "hm-vdb-dispatch-cam", "zone_id": "zone_qc", "zone_name": "QC Table" }, { "asset_id": "hm-vdb-rib-01", "edge_id": "hm-vdb-floor-cam", "zone_id": "zone_rib1", "zone_name": "Ribbing Machine" }, { "asset_id": "hm-vdb-sand-01", "edge_id": "hm-vdb-oven-cam", "zone_id": "zone_sand1", "zone_name": "Sanding Station 1" }, { "asset_id": "hm-vdb-sand-02", "edge_id": "hm-vdb-oven-cam", "zone_id": "zone_sand2", "zone_name": "Sanding Station 2" }];
const workOrders = [{ "created_at": "Feb 16, 08:18", "description": "120x polyurethane nip rollers 75mm OD x 500mm face for Sappi Tugela paper mill. Shore 85A, smooth finish.", "done_count": 0, "due_date": "Feb 16, 16:18", "item_count": 5, "line_items": [{ "asset_id": "hm-vdb-cast-01", "id": 9, "product_code": "PU-75-500", "product_name": "PU Casting - 75x500 Nip Roller", "quality_notes": "", "quality_status": "passed", "quantity": 120, "quantity_completed": 95, "quantity_failed": 2, "quantity_rework": 3, "status": "in_progress", "tracking_id": "TRK-HM001-CAST", "unit": "pcs" }, { "asset_id": "hm-vdb-oven-01", "id": 10, "product_code": "PU-75-500-CURE", "product_name": "PU Curing - 180C / 4hr", "quality_notes": "", "quality_status": "passed", "quantity": 120, "quantity_completed": 80, "quantity_failed": 1, "quantity_rework": 2, "status": "in_progress", "tracking_id": "TRK-HM001-CURE", "unit": "pcs" }, { "asset_id": "hm-vdb-lathe-01", "id": 11, "product_code": "PU-75-500-TURN", "product_name": "OD Turning \u0026 Profile", "quality_notes": "", "quality_status": "passed", "quantity": 120, "quantity_completed": 62, "quantity_failed": 0, "quantity_rework": 1, "status": "in_progress", "tracking_id": "TRK-HM001-TURN", "unit": "pcs" }, { "asset_id": "hm-vdb-sand-01", "id": 12, "product_code": "PU-75-500-SAND", "product_name": "Finish Sanding 320 grit", "quality_notes": "", "quality_status": "pending", "quantity": 120, "quantity_completed": 48, "quantity_failed": 0, "quantity_rework": 0, "status": "in_progress", "tracking_id": "TRK-HM001-SAND", "unit": "pcs" }, { "asset_id": "hm-vdb-qc-01", "id": 13, "product_code": "PU-75-500-QC", "product_name": "QC Hardness \u0026 OD Check", "quality_notes": "", "quality_status": "pending", "quantity": 120, "quantity_completed": 35, "quantity_failed": 0, "quantity_rework": 0, "status": "in_progress", "tracking_id": "TRK-HM001-QC01", "unit": "pcs" }], "priority": "high", "progress_pct": 53.3, "status": "in_progress", "title": "PU Rollers 75x500 - Sappi Tugela", "work_order_id": "WO-HM-001" }, { "created_at": "Feb 16, 08:18", "description": "80x rubber feed rollers 100mm OD x 800mm face for ArcelorMittal Vanderbijlpark. Ribbed pattern, Shore 65A.", "done_count": 0, "due_date": "Feb 17, 00:18", "item_count": 5, "line_items": [{ "asset_id": "hm-vdb-cast-02", "id": 14, "product_code": "RB-100-800", "product_name": "Rubber Casting - 100x800 Feed", "quality_notes": "", "quality_status": "passed", "quantity": 80, "quantity_completed": 60, "quantity_failed": 3, "quantity_rework": 2, "status": "in_progress", "tracking_id": "TRK-HM002-CAST", "unit": "pcs" }, { "asset_id": "hm-vdb-oven-02", "id": 15, "product_code": "RB-100-800-CURE", "product_name": "Rubber Curing - 150C / 6hr", "quality_notes": "", "quality_status": "passed", "quantity": 80, "quantity_completed": 45, "quantity_failed": 1, "quantity_rework": 1, "status": "in_progress", "tracking_id": "TRK-HM002-CURE", "unit": "pcs" }, { "asset_id": "hm-vdb-rib-01", "id": 16, "product_code": "RB-100-800-RIB", "product_name": "Helical Rib Cut", "quality_notes": "", "quality_status": "passed", "quantity": 80, "quantity_completed": 30, "quantity_failed": 0, "quantity_rework": 2, "status": "in_progress", "tracking_id": "TRK-HM002-HRIB", "unit": "pcs" }, { "asset_id": "hm-vdb-lathe-02", "id": 17, "product_code": "RB-100-800-TURN", "product_name": "OD Turning - Crowned", "quality_notes": "", "quality_status": "pending", "quantity": 80, "quantity_completed": 18, "quantity_failed": 0, "quantity_rework": 0, "status": "in_progress", "tracking_id": "TRK-HM002-TURN", "unit": "pcs" }, { "asset_id": "hm-vdb-sand-02", "id": 18, "product_code": "RB-100-800-SAND", "product_name": "Finish Sand 240 grit", "quality_notes": "", "quality_status": "pending", "quantity": 80, "quantity_completed": 0, "quantity_failed": 0, "quantity_rework": 0, "status": "queued", "tracking_id": "TRK-HM002-SAND", "unit": "pcs" }], "priority": "normal", "progress_pct": 38.2, "status": "in_progress", "title": "Rubber Rollers 100x800 - ArcelorMittal", "work_order_id": "WO-HM-002" }, { "created_at": "Feb 16, 08:18", "description": "200x polyurethane conveyor guide rollers 60mm OD x 300mm. Shore 92A, precision ground.", "done_count": 0, "due_date": "Feb 17, 08:18", "item_count": 3, "line_items": [{ "asset_id": "hm-vdb-cast-01", "id": 19, "product_code": "PU-60-300", "product_name": "PU Casting - 60x300 Guide", "quality_notes": "", "quality_status": "pending", "quantity": 200, "quantity_completed": 0, "quantity_failed": 0, "quantity_rework": 0, "status": "queued", "tracking_id": "TRK-HM003-CAST", "unit": "pcs" }, { "asset_id": "hm-vdb-oven-01", "id": 20, "product_code": "PU-60-300-CURE", "product_name": "PU Curing - 180C / 3hr", "quality_notes": "", "quality_status": "pending", "quantity": 200, "quantity_completed": 0, "quantity_failed": 0, "quantity_rework": 0, "status": "queued", "tracking_id": "TRK-HM003-CURE", "unit": "pcs" }, { "asset_id": "hm-vdb-lathe-01", "id": 21, "product_code": "PU-60-300-GRIND", "product_name": "Precision OD Grind", "quality_notes": "", "quality_status": "pending", "quantity": 200, "quantity_completed": 0, "quantity_failed": 0, "quantity_rework": 0, "status": "queued", "tracking_id": "TRK-HM003-GRND", "unit": "pcs" }], "priority": "urgent", "progress_pct": 0.0, "status": "pending", "title": "PU Conveyor Rollers 60x300 - Anglo American", "work_order_id": "WO-HM-003" }, { "created_at": "Feb 16, 08:18", "description": "Re-cover 24x existing steel cores with new rubber sleeves 150mm OD x 1200mm. Shore 55A, crowned profile.", "done_count": 0, "due_date": "Feb 16, 12:18", "item_count": 5, "line_items": [{ "asset_id": "hm-vdb-lathe-01", "id": 22, "product_code": "RC-150-1200-STRIP", "product_name": "Strip Old Rubber", "quality_notes": "", "quality_status": "passed", "quantity": 24, "quantity_completed": 20, "quantity_failed": 1, "quantity_rework": 0, "status": "in_progress", "tracking_id": "TRK-HM004-STRP", "unit": "pcs" }, { "asset_id": "hm-vdb-cast-02", "id": 23, "product_code": "RC-150-1200-CAST", "product_name": "Re-Cast Rubber Sleeve", "quality_notes": "", "quality_status": "passed", "quantity": 24, "quantity_completed": 14, "quantity_failed": 0, "quantity_rework": 1, "status": "in_progress", "tracking_id": "TRK-HM004-RSLV", "unit": "pcs" }, { "asset_id": "hm-vdb-oven-02", "id": 24, "product_code": "RC-150-1200-CURE", "product_name": "Cure Sleeve - 150C", "quality_notes": "", "quality_status": "pending", "quantity": 24, "quantity_completed": 8, "quantity_failed": 0, "quantity_rework": 0, "status": "in_progress", "tracking_id": "TRK-HM004-CURE", "unit": "pcs" }, { "asset_id": "hm-vdb-lathe-02", "id": 25, "product_code": "RC-150-1200-TURN", "product_name": "Crown Profile Turn", "quality_notes": "", "quality_status": "pending", "quantity": 24, "quantity_completed": 4, "quantity_failed": 0, "quantity_rework": 0, "status": "in_progress", "tracking_id": "TRK-HM004-CRWN", "unit": "pcs" }, { "asset_id": "hm-vdb-sand-01", "id": 26, "product_code": "RC-150-1200-SAND", "product_name": "Final Polish 400 grit", "quality_notes": "", "quality_status": "pending", "quantity": 24, "quantity_completed": 0, "quantity_failed": 0, "quantity_rework": 0, "status": "queued", "tracking_id": "TRK-HM004-PLSH", "unit": "pcs" }], "priority": "high", "progress_pct": 38.3, "status": "in_progress", "title": "Rubber Sleeve Re-Cover - Mondi", "work_order_id": "WO-HM-004" }];
const anomalyAssets = new Map();
let showZones = false;
let alertFilterOn = false;

// ===== HELPERS =====
function timeAgo(isoStr) {
    if (!isoStr) return 'No events';
    const d = new Date(isoStr.endsWith('Z') ? isoStr : isoStr + 'Z');
    const diff = (Date.now() - d.getTime()) / 1000;
    if (diff < 60) return 'Just now';
    if (diff < 3600) return Math.floor(diff / 60) + 'm ago';
    if (diff < 86400) return Math.floor(diff / 3600) + 'h ago';
    return Math.floor(diff / 86400) + 'd ago';
}

function getAssetIcon(icon) {
    return getMachineIcon(icon, 36);
}

// Inject SVG icons into all asset nodes (replaces Jinja emojis)
function renderAssetIcons() {
    document.querySelectorAll('.asset-node').forEach(node => {
        const iconEl = node.querySelector('.node-icon');
        if (iconEl) iconEl.innerHTML = getMachineIcon(node.dataset.icon, 38);
    });
}

// ===== ZONE OVERLAYS =====
function buildZoneOverlays() {
    const canvas = document.getElementById('floorCanvas');
    // Group assets by rough proximity to create zone rectangles
    const zoneGroups = {};
    zoneMappings.forEach(zm => {
        const key = zm.zone_name || zm.zone_id;
        if (!zoneGroups[key]) zoneGroups[key] = { name: key, assets: [], hasAnomaly: false };
        zoneGroups[key].assets.push(zm.asset_id);
    });

    // For each asset, find its position
    const assetPos = {};
    assets.forEach(a => { assetPos[a.asset_id] = { x: a.floor_x, y: a.floor_y, anomalies: a.anomaly_count_24h || 0 }; });

    // Create zone overlays based on asset positions
    Object.values(zoneGroups).forEach(group => {
        let minX = 100, maxX = 0, minY = 100, maxY = 0;
        let hasAnomaly = false;
        let found = false;
        group.assets.forEach(aid => {
            const pos = assetPos[aid];
            if (pos) {
                found = true;
                minX = Math.min(minX, pos.x);
                maxX = Math.max(maxX, pos.x);
                minY = Math.min(minY, pos.y);
                maxY = Math.max(maxY, pos.y);
                if (pos.anomalies > 0) hasAnomaly = true;
            }
        });
        if (!found) return;

        // Add padding
        const pad = 5;
        minX = Math.max(0, minX - pad);
        maxX = Math.min(100, maxX + pad);
        minY = Math.max(0, minY - pad);
        maxY = Math.min(100, maxY + pad);

        // Ensure minimum size
        if (maxX - minX < 8) { minX -= 4; maxX += 4; }
        if (maxY - minY < 8) { minY -= 4; maxY += 4; }

        const div = document.createElement('div');
        div.className = 'zone-overlay' + (hasAnomaly ? ' hot' : '');
        div.dataset.zoneName = group.name;
        div.style.left = minX + '%';
        div.style.top = minY + '%';
        div.style.width = (maxX - minX) + '%';
        div.style.height = (maxY - minY) + '%';
        div.innerHTML = '<span class="zone-label">' + group.name + '</span>';
        canvas.insertBefore(div, canvas.firstChild);
    });
}

// ===== TOOLBAR TOGGLES =====
function toggleZones() {
    showZones = !showZones;
    document.querySelectorAll('.zone-overlay').forEach(z => {
        z.style.display = showZones ? '' : 'none';
    });
    document.getElementById('btnShowZones').classList.toggle('active-filter', showZones);
}

function toggleAlertFilter() {
    alertFilterOn = !alertFilterOn;
    document.querySelectorAll('.asset-node').forEach(node => {
        if (alertFilterOn) {
            const state = node.dataset.state;
            const anomalies = parseInt(node.dataset.anomalies) || 0;
            node.style.opacity = (state === 'alert' || state === 'warning' || anomalies > 0) ? '' : '0.2';
        } else {
            node.style.opacity = '';
        }
    });
    document.getElementById('btnFilterAlerts').classList.toggle('active-filter', alertFilterOn);
}

// ===== TOOLTIP =====
const tooltip = document.getElementById('assetTooltip');
function initTooltips() {
    document.querySelectorAll('.asset-node').forEach(node => {
        node.addEventListener('mouseenter', () => {
            const id = parseInt(node.dataset.assetId);
            const asset = assets.find(a => a.id === id);
            if (!asset) return;
            document.getElementById('ttIcon').innerHTML = getMachineIcon(asset.icon, 28);
            document.getElementById('ttName').textContent = asset.name;
            const stateEl = document.getElementById('ttState');
            stateEl.textContent = (asset.current_state || 'unknown').toUpperCase();
            stateEl.className = 'tt-val ' + (asset.current_state || '');
            document.getElementById('ttTime').textContent = timeAgo(asset.last_event_at);
            document.getElementById('ttDesc').textContent = node.dataset.desc || '';
            const aCount = parseInt(node.dataset.anomalies) || 0;
            const aRow = document.getElementById('ttAnomalyRow');
            if (aCount > 0) {
                aRow.style.display = '';
                document.getElementById('ttAnomalies').textContent = aCount;
            } else {
                aRow.style.display = 'none';
            }

            // Work order items for this asset
            const woDiv = document.getElementById('ttWorkItems');
            const woList = document.getElementById('ttWorkItemsList');
            const wi = asset.work_items || [];
            if (wi.length > 0) {
                woDiv.style.display = '';
                woList.innerHTML = wi.map(w =>
                    `<div style="display:flex;justify-content:space-between;padding:1px 0;">` +
                    `<span style="color:var(--text)">${w.product_name}</span>` +
                    `<span style="color:var(--text-muted)">${w.quantity_completed}/${w.quantity} ${w.unit}</span></div>`
                ).join('');
            } else {
                woDiv.style.display = 'none';
            }

            tooltip.classList.add('show');
        });
        node.addEventListener('mousemove', (e) => {
            const rect = document.getElementById('floorPlan').getBoundingClientRect();
            let x = e.clientX - rect.left + 14;
            let y = e.clientY - rect.top - 10;
            // Keep tooltip within bounds
            if (x + 200 > rect.width) x = e.clientX - rect.left - 200;
            if (y + 120 > rect.height) y = e.clientY - rect.top - 120;
            tooltip.style.left = x + 'px';
            tooltip.style.top = y + 'px';
        });
        node.addEventListener('mouseleave', () => tooltip.classList.remove('show'));
    });
}

// ===== HIGHLIGHT ASSET (from sidebar click) =====
function highlightAsset(assetId) {
    const node = document.querySelector(`.asset-node[data-asset-id="${assetId}"]`);
    if (!node) return;
    node.style.transition = 'transform 0.3s, box-shadow 0.3s';
    node.style.transform = 'translate(-50%, -50%) scale(1.25)';
    node.style.boxShadow = '0 0 30px var(--primary-glow)';
    node.style.zIndex = '50';
    setTimeout(() => {
        node.style.transform = '';
        node.style.boxShadow = '';
        node.style.zIndex = '';
    }, 1200);
}

// ===== FLOOR LOCK =====
const ADMIN_PIN = '1212';
let floorLocked = true; // starts locked
let autoLockTimer = null;
const AUTO_LOCK_MS = 120000; // 2 minutes of inactivity

function applyLockState() {
    const btn = document.getElementById('btnLock');
    const floorPlan = document.getElementById('floorPlan');
    if (floorLocked) {
        btn.className = 'tool-btn lock-btn locked';
        btn.innerHTML = '🔒 Locked';
        floorPlan.classList.add('floor-locked');
    } else {
        btn.className = 'tool-btn lock-btn unlocked';
        btn.innerHTML = '🔓 Unlocked';
        floorPlan.classList.remove('floor-locked');
        resetAutoLock();
    }
}

function resetAutoLock() {
    clearTimeout(autoLockTimer);
    if (!floorLocked) {
        autoLockTimer = setTimeout(() => {
            floorLocked = true;
            applyLockState();
        }, AUTO_LOCK_MS);
    }
}

function toggleFloorLock() {
    if (floorLocked) {
        // Show PIN modal to unlock
        openPinModal();
    } else {
        // Lock immediately (no PIN needed)
        floorLocked = true;
        clearTimeout(autoLockTimer);
        applyLockState();
    }
}

function openPinModal() {
    const overlay = document.getElementById('pinOverlay');
    const input = document.getElementById('pinInput');
    const error = document.getElementById('pinError');
    overlay.classList.remove('hidden');
    input.value = '';
    input.classList.remove('shake');
    error.textContent = '';
    setTimeout(() => input.focus(), 100);
}

function closePinModal() {
    document.getElementById('pinOverlay').classList.add('hidden');
}

function submitPin() {
    const input = document.getElementById('pinInput');
    const error = document.getElementById('pinError');
    if (input.value === ADMIN_PIN) {
        floorLocked = false;
        closePinModal();
        applyLockState();
    } else {
        error.textContent = 'Incorrect PIN';
        input.classList.add('shake');
        input.value = '';
        setTimeout(() => input.classList.remove('shake'), 500);
    }
}

// Allow Enter key in PIN input
document.addEventListener('DOMContentLoaded', () => {
    const pinInput = document.getElementById('pinInput');
    if (pinInput) {
        pinInput.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') submitPin();
            if (e.key === 'Escape') closePinModal();
        });
    }
});

// Guard for Edit Layout link
function checkUnlocked(e) {
    if (floorLocked) {
        e.preventDefault();
        openPinModal();
        return false;
    }
    return true;
}

// ===== DRAG AND DROP =====
let draggedNode = null;
let hasDragged = false;
let dragStart = { x: 0, y: 0 };
const canvas = document.getElementById('floorCanvas');

function initDragAndDrop() {
    document.querySelectorAll('.asset-node').forEach(node => {
        node.addEventListener('mousedown', (e) => {
            if (e.button !== 0) return;
            if (floorLocked) return; // block drag when locked
            e.preventDefault();
            draggedNode = node;
            hasDragged = false;
            dragStart = { x: e.clientX, y: e.clientY };
            node.classList.add('dragging');
            resetAutoLock();
        });

        node.addEventListener('dblclick', () => {
            window.location.href = '/asset/' + node.dataset.assetId;
        });
    });

    document.addEventListener('mousemove', (e) => {
        if (!draggedNode) return;
        const dx = e.clientX - dragStart.x;
        const dy = e.clientY - dragStart.y;
        if (Math.abs(dx) > 5 || Math.abs(dy) > 5) hasDragged = true;

        const rect = canvas.getBoundingClientRect();
        let x = ((e.clientX - rect.left) / rect.width) * 100;
        let y = ((e.clientY - rect.top) / rect.height) * 100;
        x = Math.max(4, Math.min(96, x));
        y = Math.max(4, Math.min(96, y));
        draggedNode.style.left = x + '%';
        draggedNode.style.top = y + '%';
    });

    document.addEventListener('mouseup', async (e) => {
        if (!draggedNode) return;
        draggedNode.classList.remove('dragging');
        const assetId = draggedNode.dataset.assetId;

        if (hasDragged) {
            const x = parseFloat(draggedNode.style.left);
            const y = parseFloat(draggedNode.style.top);
            await savePos(assetId, x, y);
            resetAutoLock();
        } else {
            if (!floorLocked) showCtxMenu(draggedNode, e);
        }
        draggedNode = null;
    });

    document.addEventListener('click', (e) => {
        const menu = document.getElementById('assetMenu');
        if (!menu.contains(e.target) && !e.target.closest('.asset-node')) {
            menu.classList.remove('show');
        }
    });
}

function showCtxMenu(node, event) {
    const id = node.dataset.assetId;
    const menu = document.getElementById('assetMenu');
    const rect = canvas.getBoundingClientRect();
    menu.style.left = (event.clientX - rect.left + 10) + 'px';
    menu.style.top = (event.clientY - rect.top + 10) + 'px';
    document.getElementById('menuTimeline').href = '/asset/' + id;
    document.getElementById('menuConfigure').href = '/asset/' + id + '/config';
    document.getElementById('menuZones').href = '/asset/' + id + '/config';
    menu.classList.add('show');
}

async function savePos(assetId, floorX, floorY) {
    try {
        await fetch('/api/v1/assets/' + assetId + '/position', {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include',
            body: JSON.stringify({ floor_x: floorX, floor_y: floorY })
        });
    } catch (err) {
        console.error('Position save error:', err);
    }
}

// ===== ANOMALY POLLING =====
async function pollAnomalies() {
    try {
        const res = await fetch('/api/v1/anomalies/recent?minutes=1');
        if (!res.ok) return;
        const data = await res.json();
        const recent = data.anomalies || [];
        lastUpdateTs = Date.now();

        const activeZones = new Set();
        const activeEdges = new Set();
        recent.forEach(a => {
            if (a.zone_id) activeZones.add(a.zone_id);
            if (a.zone_name) activeZones.add(a.zone_name);
            if (a.edge_id) activeEdges.add(a.edge_id);
        });

        document.querySelectorAll('.asset-node').forEach(node => {
            const id = parseInt(node.dataset.assetId);
            const asset = assets.find(a => a.id === id);
            if (!asset) return;

            let hit = false;
            activeZones.forEach(z => {
                if (asset.asset_id?.includes(z)) hit = true;
                if (asset.name?.toLowerCase().includes(z.toLowerCase())) hit = true;
            });
            if (!hit) {
                activeEdges.forEach(eid => {
                    if (asset.asset_id?.startsWith(eid + '_')) hit = true;
                });
            }

            if (hit) {
                node.classList.add('anomaly-active', 'machine-anomaly-active');
                anomalyAssets.set(id, Date.now());
            } else {
                const last = anomalyAssets.get(id);
                if (!last || Date.now() - last > 5000) {
                    node.classList.remove('anomaly-active', 'machine-anomaly-active');
                    anomalyAssets.delete(id);
                }
            }
        });
    } catch (err) {
        console.error('Poll error:', err);
    }
}

// ===== LIVE INDICATOR BLINK + LAST UPDATE =====
let lastUpdateTs = Date.now();
function updateLastUpdateTime() {
    const el = document.getElementById('lastUpdateTime');
    if (!el) return;
    const now = new Date(lastUpdateTs);
    const hh = String(now.getHours()).padStart(2, '0');
    const mm = String(now.getMinutes()).padStart(2, '0');
    const ss = String(now.getSeconds()).padStart(2, '0');
    el.textContent = hh + ':' + mm + ':' + ss;
}
function blinkLive() {
    const el = document.getElementById('liveIndicator');
    if (!el) return;
    setInterval(() => {
        el.style.opacity = el.style.opacity === '0.3' ? '1' : '0.3';
    }, 1000);
    updateLastUpdateTime();
    setInterval(updateLastUpdateTime, 1000);
}

// ===== PRODUCTION DRILL-DOWN =====
function toggleProdSection(sectionId, iconEl) {
    const section = document.getElementById(sectionId);
    if (!section) return;
    const isHidden = section.style.display === 'none';
    section.style.display = isHidden ? '' : 'none';
    if (iconEl) iconEl.classList.toggle('collapsed', !isHidden);
}
function toggleMachineSub(subId) {
    const sub = document.getElementById(subId);
    if (!sub) return;
    sub.classList.toggle('open');
}

// ===== WORK ORDER EXPAND / NAVIGATE =====
function toggleWoCard(cardEl) {
    cardEl.classList.toggle('expanded');
}

function navigateToAsset(assetId) {
    // Find asset node on floor and highlight it
    const assetObj = assets.find(a => a.asset_id === assetId);
    if (assetObj) {
        highlightAsset(assetObj.id);
    }
}

function viewFullOrder(woId) {
    // Navigate to production page with order context
    window.location.href = '/production?wo=' + encodeURIComponent(woId);
}

function exportWoFinance(woId) {
    // Switch to Finance tab and highlight the order
    const finTab = document.querySelector('.drawer-tab[data-panel="panelFinance"]');
    if (finTab) switchDrawerTab(finTab);
    const drawer = document.getElementById('sidebarDrawer');
    if (!drawer.classList.contains('open')) toggleDrawer();
    // Highlight the row briefly
    const row = document.querySelector('.fin-wo-row[onclick*="' + woId + '"]');
    if (row) {
        row.style.background = 'rgba(59,130,246,0.15)';
        setTimeout(() => { row.style.background = ''; }, 1500);
    }
}

function switchToProductionTab(woId) {
    const prodTab = document.querySelector('.drawer-tab[data-panel="panelProduction"]');
    if (prodTab) switchDrawerTab(prodTab);
    // Expand the matching WO card
    const card = document.querySelector('.wo-card[data-wo-id="' + woId + '"]');
    if (card && !card.classList.contains('expanded')) {
        card.classList.add('expanded');
        card.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }
}

function openProductionPanel() {
    const drawer = document.getElementById('sidebarDrawer');
    if (!drawer.classList.contains('open')) toggleDrawer();
    const prodTab = document.querySelector('.drawer-tab[data-panel="panelProduction"]');
    if (prodTab) switchDrawerTab(prodTab);
}

function openFinanceSetup() {
    window.location.href = '/integrations';
}

// Compute finance summary from work orders
function computeFinanceSummary() {
    let totalItems = 0, completedItems = 0;
    workOrders.forEach(wo => {
        (wo.line_items || []).forEach(item => {
            totalItems += item.quantity || 0;
            completedItems += item.quantity_completed || 0;
        });
    });
    // Placeholder unit economics (will be configured per-site)
    const unitRevenue = 125;  // revenue per unit
    const unitCost = 78;      // cost per unit
    const estRevenue = completedItems * unitRevenue;
    const estCost = completedItems * unitCost;
    const margin = estRevenue - estCost;

    const fmt = (v) => v >= 1000 ? (v / 1000).toFixed(1) + 'k' : v.toString();

    const revEl = document.getElementById('finRevenue');
    const costEl = document.getElementById('finCost');
    const marginEl = document.getElementById('finMargin');
    if (revEl) revEl.textContent = completedItems > 0 ? 'R' + fmt(estRevenue) : '--';
    if (costEl) costEl.textContent = completedItems > 0 ? 'R' + fmt(estCost) : '--';
    if (marginEl) marginEl.textContent = completedItems > 0 ? 'R' + fmt(margin) : '--';
}

// ===== DRAWER =====
function toggleDrawer() {
    const drawer = document.getElementById('sidebarDrawer');
    const toggle = document.getElementById('drawerToggle');
    drawer.classList.toggle('open');
    if (drawer.classList.contains('open')) {
        toggle.querySelector('.drawer-icon').textContent = '▶';
    } else {
        toggle.querySelector('.drawer-icon').textContent = '◀';
    }
}

function switchDrawerTab(btn) {
    document.querySelectorAll('.drawer-tab').forEach(t => t.classList.remove('active'));
    document.querySelectorAll('.drawer-panel').forEach(p => p.classList.remove('active'));
    btn.classList.add('active');
    const panel = document.getElementById(btn.dataset.panel);
    if (panel) panel.classList.add('active');
}

// ===== INIT =====
renderAssetIcons();
// Inject SVG mini-icons into sidebar asset rows
document.querySelectorAll('.asset-row .row-icon[data-icon]').forEach(el => {
    el.innerHTML = getMachineIcon(el.dataset.icon, 18);
    el.style.lineHeight = '0';
});
buildZoneOverlays();
document.getElementById('btnShowZones').classList.add('active-filter');
applyLockState();
initDragAndDrop();
initTooltips();
blinkLive();
computeFinanceSummary();
setInterval(pollAnomalies, 2000);
pollAnomalies();
/*setTimeout(() => location.reload(), 90000);*/

// ===== Location Switcher =====
//function toggleLocationDropdown(e) {
//    e.stopPropagation();
//    var sw = document.getElementById('locationSwitcher');
//    if (sw) sw.classList.toggle('open');
//}
//document.addEventListener('click', function () {
//    var sw = document.getElementById('locationSwitcher');
//    if (sw) sw.classList.remove('open');
//});

