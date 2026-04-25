// ═══════════════════════════════════════════════════════════════════════════
// FRESH GROCERY STORE — UI Scripts
// ═══════════════════════════════════════════════════════════════════════════

// ── Theme ─────────────────────────────────────────────────────────────────
function initTheme() {
    var t = localStorage.getItem('theme') || 'light';
    document.documentElement.setAttribute('data-theme', t);
}

function toggleTheme() {
    var current = document.documentElement.getAttribute('data-theme') || 'light';
    var next = current === 'light' ? 'dark' : 'light';
    document.documentElement.setAttribute('data-theme', next);
    localStorage.setItem('theme', next);
}

// ── Navbar scroll effect ──────────────────────────────────────────────────
window.addEventListener('scroll', function () {
    var nav = document.getElementById('navbar');
    if (nav) nav.classList.toggle('scrolled', window.scrollY > 60);
});

// ── Mobile menu toggle ───────────────────────────────────────────────────
function toggleMenu() {
    var menu = document.getElementById('navMenu');
    if (menu) menu.classList.toggle('active');
}

// ── Search overlay ───────────────────────────────────────────────────────
function openSearch() {
    var overlay = document.getElementById('searchOverlay');
    if (overlay) {
        overlay.classList.add('active');
        var input = overlay.querySelector('input');
        if (input) input.focus();
    }
}

function closeSearch() {
    var overlay = document.getElementById('searchOverlay');
    if (overlay) overlay.classList.remove('active');
}

// ── Admin sidebar toggle ─────────────────────────────────────────────────
function toggleSidebar() {
    var sidebar = document.getElementById('adminSidebar');
    var overlay = document.getElementById('sidebarOverlay');
    if (sidebar) sidebar.classList.toggle('show');
    if (overlay) overlay.classList.toggle('show');
}

// ── Toast notifications ──────────────────────────────────────────────────
function showToast(message, type) {
    var toast = document.getElementById('toast');
    if (!toast) return;
    var iconMap = {
        success: 'fa-check-circle',
        error: 'fa-exclamation-circle',
        warning: 'fa-exclamation-triangle',
        info: 'fa-info-circle'
    };
    toast.querySelector('i').className = 'fas ' + (iconMap[type] || iconMap.success);
    toast.querySelector('span').textContent = message;
    toast.classList.add('show');
    setTimeout(function () { toast.classList.remove('show'); }, 5000);
}

// ── Scroll to top ─────────────────────────────────────────────────────────
function scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// ── Focus helper ──────────────────────────────────────────────────────────
function focusElement(selector) {
    var el = document.querySelector(selector);
    if (el) el.focus();
}

// ── On DOMContentLoaded ──────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', function () {
    // Show toast from TempData
    var toastEl = document.getElementById('toast');
    if (toastEl && toastEl.dataset.message) {
        showToast(toastEl.dataset.message, toastEl.dataset.type || 'success');
    }

    // Search overlay: close on backdrop click
    var searchOverlay = document.getElementById('searchOverlay');
    if (searchOverlay) {
        searchOverlay.addEventListener('click', function (e) {
            if (e.target === searchOverlay) closeSearch();
        });

        // Submit search form on Enter
        var searchInput = searchOverlay.querySelector('input[name="search"]');
        if (searchInput) {
            searchInput.addEventListener('keydown', function (e) {
                if (e.key === 'Enter') {
                    var form = searchInput.closest('form');
                    if (form) form.submit();
                }
            });
        }
    }

    // Close search on Escape key
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') closeSearch();
    });

    // Set active nav link based on current URL
    var currentPath = window.location.pathname.toLowerCase();
    document.querySelectorAll('.nav-link').forEach(function (link) {
        var href = link.getAttribute('href');
        if (!href) return;
        href = href.toLowerCase();
        if (href === '/' && currentPath === '/') {
            link.classList.add('active');
        } else if (href !== '/' && currentPath.startsWith(href)) {
            link.classList.add('active');
        }
    });

    // Set active admin menu item
    document.querySelectorAll('.menu-item').forEach(function (link) {
        var href = link.getAttribute('href');
        if (!href) return;
        href = href.toLowerCase();
        if (currentPath.startsWith(href)) {
            link.classList.add('active');
        }
    });
});
