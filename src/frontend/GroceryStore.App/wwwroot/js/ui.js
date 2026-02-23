// ── Theme ─────────────────────────────────────────────────────────────────────
function initTheme() {
    const t = localStorage.getItem('theme') || 'light';
    document.documentElement.setAttribute('data-theme', t);
}

function toggleTheme() {
    const current = document.documentElement.getAttribute('data-theme') || 'light';
    const next    = current === 'light' ? 'dark' : 'light';
    document.documentElement.setAttribute('data-theme', next);
    localStorage.setItem('theme', next);
}

// ── Navbar scroll effect ──────────────────────────────────────────────────────
window.addEventListener('scroll', function () {
    const nav = document.getElementById('navbar');
    if (nav) nav.classList.toggle('scrolled', window.scrollY > 60);
});

// ── Scroll to top ─────────────────────────────────────────────────────────────
function scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// ── Focus helper used after modals open ──────────────────────────────────────
function focusElement(selector) {
    const el = document.querySelector(selector);
    if (el) el.focus();
}
