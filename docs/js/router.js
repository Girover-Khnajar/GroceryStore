// ============================================
// HASH-BASED ROUTER SYSTEM
// ============================================

const Router = {
    routes: {},
    currentRoute: null,

    // Initialize Router
    init() {
        window.addEventListener('hashchange', () => this.handleRoute());
        window.addEventListener('load', () => this.handleRoute());
    },

    // Register Route
    register(path, handler) {
        this.routes[path] = handler;
    },

    // Handle Route Changes
    handleRoute() {
        const hash = window.location.hash.slice(1) || 'home';
        const [route, ...params] = hash.split('/');
        
        this.currentRoute = route;
        this.updateNavigation();

        // Stop home banner slider when leaving home
        if (route !== 'home' && window.App?.stopBannerSlider) {
            window.App.stopBannerSlider();
        }
        
        if (this.routes[route]) {
            this.routes[route](...params);
        } else {
            this.routes['home']();
        }

        // Refresh AOS after dynamic renders
        if (window.UI?.refreshAOS) {
            window.UI.refreshAOS();
        } else if (window.AOS?.refreshHard) {
            window.AOS.refreshHard();
        }

        // Scroll to top
        window.scrollTo({ top: 0, behavior: 'smooth' });
    },

    // Update Navigation Active State
    updateNavigation() {
        const navLinks = document.querySelectorAll('.nav-link');
        navLinks.forEach(link => {
            const href = link.getAttribute('href').slice(1);
            link.classList.toggle('active', href === this.currentRoute);
        });
    },

    // Navigate to Route
    navigate(path) {
        window.location.hash = path;
    }
};
