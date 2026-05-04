/**
 * Optimized Image Loader
 * - Uses Intersection Observer for efficient lazy loading
 * - Preloads images in viewport + 200px margin
 * - Handles loading states and errors
 * - Falls back to immediate loading if IO not supported
 */

(function() {
    'use strict';

    // Configuration
    const CONFIG = {
        rootMargin: '200px 0px', // Start loading 200px before entering viewport
        threshold: 0.01,
        enableLogging: false // Set to true for debugging
    };

    /**
     * Load an image and update its container state
     */
    function loadImage(img) {
        const container = img.closest('[data-image-container]');
        const actualSrc = img.getAttribute('data-src');
        
        if (!actualSrc || img.classList.contains('loaded') || img.classList.contains('error')) {
            return;
        }

        if (CONFIG.enableLogging) {
            console.log('Loading image:', actualSrc);
        }

        // Set src to trigger browser load
        img.src = actualSrc;

        // Handle successful load
        img.addEventListener('load', function onLoad() {
            img.classList.add('loaded');
            if (container) {
                container.setAttribute('data-loaded', 'true');
            }
            img.removeEventListener('load', onLoad);
            
            if (CONFIG.enableLogging) {
                console.log('Image loaded:', actualSrc);
            }
        }, { once: true });

        // Handle error
        img.addEventListener('error', function onError() {
            img.classList.add('error');
            if (container) {
                container.setAttribute('data-loaded', 'true');
            }
            img.removeEventListener('error', onError);
            
            if (CONFIG.enableLogging) {
                console.warn('Image failed to load:', actualSrc);
            }
        }, { once: true });
    }

    /**
     * Initialize lazy loading with Intersection Observer
     */
    function initializeLazyLoading() {
        const images = document.querySelectorAll('[data-optimized-image]');
        
        if (images.length === 0) {
            return;
        }

        // Check for Intersection Observer support
        if (!('IntersectionObserver' in window)) {
            if (CONFIG.enableLogging) {
                console.warn('IntersectionObserver not supported, loading all images immediately');
            }
            // Fallback: load all images immediately
            images.forEach(loadImage);
            return;
        }

        // Create observer
        const observer = new IntersectionObserver(function(entries) {
            entries.forEach(function(entry) {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    loadImage(img);
                    observer.unobserve(img);
                }
            });
        }, {
            rootMargin: CONFIG.rootMargin,
            threshold: CONFIG.threshold
        });

        // Observe all images
        images.forEach(function(img) {
            // Load eager images immediately
            if (img.getAttribute('loading') === 'eager') {
                loadImage(img);
            } else {
                observer.observe(img);
            }
        });

        if (CONFIG.enableLogging) {
            console.log('Lazy loading initialized for', images.length, 'images');
        }
    }

    /**
     * Reinitialize for dynamically added images
     */
    function reinitialize() {
        initializeLazyLoading();
    }

    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeLazyLoading);
    } else {
        initializeLazyLoading();
    }

    // Expose reinitialize function globally for SPA-like behavior
    window.OptimizedImageLoader = {
        reinitialize: reinitialize,
        loadImage: loadImage
    };
})();
