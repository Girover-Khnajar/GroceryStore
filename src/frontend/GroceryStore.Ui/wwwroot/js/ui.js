// Shared UI helpers: theme toggle + AOS init
// Loaded by both index.html and admin.html

(function () {
    const THEME_KEY = 'theme';

    // Keep slider timers per element (safe to re-init)
    const bannerSliderTimers = new WeakMap();

    function getSystemTheme() {
        const prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
        return prefersDark ? 'dark' : 'light';
    }

    function getSavedTheme() {
        const theme = localStorage.getItem(THEME_KEY);
        return theme === 'dark' || theme === 'light' ? theme : null;
    }

    function setMetaThemeColor(theme) {
        const meta = document.querySelector('meta[name="theme-color"]');
        if (!meta) return;
        meta.setAttribute('content', theme === 'dark' ? '#0b1220' : '#f8f9fa');
    }

    function applyTheme(theme, { persist } = { persist: false }) {
        const normalized = theme === 'dark' ? 'dark' : 'light';
        document.documentElement.dataset.theme = normalized;
        document.documentElement.style.colorScheme = normalized;

        if (persist) {
            localStorage.setItem(THEME_KEY, normalized);
        }

        setMetaThemeColor(normalized);
        updateThemeToggleUI(normalized);

        document.dispatchEvent(new CustomEvent('theme:change', { detail: { theme: normalized } }));
    }

    function updateThemeToggleUI(theme) {
        const toggle = document.getElementById('themeToggle');
        if (!toggle) return;

        const isDark = theme === 'dark';
        toggle.setAttribute('aria-pressed', isDark ? 'true' : 'false');
        toggle.setAttribute('aria-label', isDark ? 'تبديل إلى الوضع الفاتح' : 'تبديل إلى الوضع الداكن');
        toggle.setAttribute('title', isDark ? 'Light mode' : 'Dark mode');
    }

    function initTheme() {
        const saved = getSavedTheme();
        const initial = saved || document.documentElement.dataset.theme || getSystemTheme();
        applyTheme(initial, { persist: false });

        // If user hasn't explicitly chosen, follow system changes.
        if (!saved && window.matchMedia) {
            const mq = window.matchMedia('(prefers-color-scheme: dark)');
            mq.addEventListener?.('change', () => {
                if (!getSavedTheme()) {
                    applyTheme(getSystemTheme(), { persist: false });
                }
            });
        }
    }

    function initThemeToggle() {
        const toggle = document.getElementById('themeToggle');
        if (!toggle) return;

        toggle.addEventListener('click', () => {
            const current = document.documentElement.dataset.theme === 'dark' ? 'dark' : 'light';
            const next = current === 'dark' ? 'light' : 'dark';
            applyTheme(next, { persist: true });
        });

        updateThemeToggleUI(document.documentElement.dataset.theme);
    }

    function initAOS() {
        if (!window.AOS) return;

        const prefersReducedMotion = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

        window.AOS.init({
            duration: 750,
            easing: 'ease-out-cubic',
            once: false,
            mirror: true,
            offset: 80,
            disable: prefersReducedMotion
        });
    }

    function refreshAOS() {
        if (!window.AOS) return;
        try {
            window.AOS.refreshHard();
        } catch {
            window.AOS.refresh();
        }
    }

    function initBannerSlider(rootId = 'bannerSlider', intervalMs = 5000) {
        const slider = document.getElementById(rootId);
        if (!slider) return;

        const slides = Array.from(slider.querySelectorAll('.banner-slide'));
        if (slides.length <= 1) {
            // Ensure at least one visible
            slides[0]?.classList.add('active');
            return;
        }

        const existingTimer = bannerSliderTimers.get(slider);
        if (existingTimer) {
            clearInterval(existingTimer);
            bannerSliderTimers.delete(slider);
        }

        let index = slides.findIndex(s => s.classList.contains('active'));
        if (index < 0) {
            index = 0;
            slides.forEach((s, i) => s.classList.toggle('active', i === 0));
        }

        const timerId = setInterval(() => {
            slides[index]?.classList.remove('active');
            index = (index + 1) % slides.length;
            slides[index]?.classList.add('active');
        }, Math.max(1000, Number(intervalMs) || 5000));

        bannerSliderTimers.set(slider, timerId);
    }

    window.UI = {
        applyTheme,
        initTheme,
        initThemeToggle,
        initAOS,
        refreshAOS,
        initBannerSlider
    };

    // Boot
    document.addEventListener('DOMContentLoaded', () => {
        initTheme();
        initThemeToggle();
        initAOS();
        refreshAOS();

        // Banner slider (if present on the current page)
        initBannerSlider();
    });
})();