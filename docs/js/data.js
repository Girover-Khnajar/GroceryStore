// ============================================
// DATA MANAGEMENT SYSTEM
// ============================================

const DataManager = {
    // Storage Keys
    KEYS: {
        PRODUCTS: 'grocery_products',
        CATEGORIES: 'grocery_categories',
        BRANDS: 'grocery_brands',
        BANNERS: 'grocery_banners',
        SETTINGS: 'grocery_settings',
        ADMIN: 'grocery_admin'
    },

    // Initialize Data
    init() {
        if (!localStorage.getItem(this.KEYS.CATEGORIES)) {
            this.seedData();
        }

        // Keep older localStorage data compatible with new UI features.
        this.migrateData();
    },

    // Migrate persisted data to newer formats
    migrateData() {
        this.migrateProductImagesToInternetUrls();
        this.migrateCategoryImagesToInternetUrls();
        this.migrateBannerImagesToArray();
    },

    buildInternetProductImageUrl(product) {
        const seedSource = (product?.slug || product?.sku || product?.name || String(product?.id || '')).trim();
        const seed = seedSource ? encodeURIComponent(seedSource) : String(Date.now());

        // Deterministic internet image (stable per seed). Uses an external placeholder-photo service.
        return `https://picsum.photos/seed/${seed}/900/900`;
    },

    buildInternetCategoryImageUrl(category) {
        const seedSource = (category?.slug || category?.name || String(category?.id || '')).trim();
        const seed = seedSource ? encodeURIComponent(`category-${seedSource}`) : String(Date.now());

        // Wide-ish placeholder photo, stable per category.
        return `https://picsum.photos/seed/${seed}/900/675`;
    },

    migrateProductImagesToInternetUrls() {
        const products = this.getAll(this.KEYS.PRODUCTS);
        if (!Array.isArray(products) || products.length === 0) return;

        let changed = false;

        const updated = products.map((product) => {
            const existingImages = Array.isArray(product?.images) ? product.images.filter(Boolean) : [];
            const firstImage = existingImages[0] || '';
            const firstIsInternet = typeof firstImage === 'string' && /^https?:\/\//i.test(firstImage);

            if (firstIsInternet) return product;

            const internetUrl = this.buildInternetProductImageUrl(product);
            changed = true;
            return {
                ...product,
                images: [internetUrl, ...existingImages]
            };
        });

        if (changed) {
            this.save(this.KEYS.PRODUCTS, updated);
        }
    },

    migrateCategoryImagesToInternetUrls() {
        const categories = this.getAll(this.KEYS.CATEGORIES);
        if (!Array.isArray(categories) || categories.length === 0) return;

        let changed = false;

        const updated = categories.map((category) => {
            const current = category?.image || '';
            const isInternet = typeof current === 'string' && /^https?:\/\//i.test(current);
            if (isInternet) return category;

            const internetUrl = this.buildInternetCategoryImageUrl(category);
            changed = true;
            return {
                ...category,
                image: internetUrl
            };
        });

        if (changed) {
            this.save(this.KEYS.CATEGORIES, updated);
        }
    },

    migrateBannerImagesToArray() {
        const banners = this.getAll(this.KEYS.BANNERS);
        if (!Array.isArray(banners) || banners.length === 0) return;

        let changed = false;

        const updated = banners.map((banner) => {
            const existingImages = Array.isArray(banner?.images) ? banner.images.filter(Boolean) : [];
            const legacyImage = banner?.imageUrl ? [banner.imageUrl] : [];

            // If already has an images array and a valid imageUrl, keep.
            const baseImages = existingImages.length > 0 ? existingImages : legacyImage;

            // Build fallback images for the banner slider.
            const seedSource = (banner?.id ? String(banner.id) : '') || (banner?.title || '').trim() || (banner?.placement || '');
            const seed = encodeURIComponent(seedSource);
            const ensureThreeImages = (images) => {
                const unique = new Set(images);
                const out = [...unique];
                // Only force 3 images for HomeHero banners (banner slider)
                if ((banner?.placement || '') !== 'HomeHero') return out;

                for (let i = out.length; i < 3; i++) {
                    out.push(`https://picsum.photos/seed/banner-${seed}-${i + 1}/1200/400`);
                }
                return out;
            };

            const images = ensureThreeImages(baseImages);
            if (images.length === 0) {
                // No legacy image exists, generate for HomeHero.
                if ((banner?.placement || '') === 'HomeHero') {
                    const generated = ensureThreeImages([]);
                    changed = true;
                    return {
                        ...banner,
                        images: generated,
                        imageUrl: generated[0]
                    };
                }

                return banner;
            }

            const nextImageUrl = banner?.imageUrl || images[0];
            const needsChange =
                !Array.isArray(banner?.images) ||
                banner.images.length !== images.length ||
                images.some((img, idx) => banner.images?.[idx] !== img) ||
                banner?.imageUrl !== nextImageUrl;

            if (!needsChange) return banner;

            changed = true;
            return {
                ...banner,
                images,
                imageUrl: nextImageUrl
            };
        });

        if (changed) {
            this.save(this.KEYS.BANNERS, updated);
        }
    },

    // Seed Initial Data
    seedData() {
        // Admin Credentials
        this.saveAdmin({
            username: 'admin',
            password: 'admin123'
        });

        // Categories
        const categories = [
            {
                id: 1,
                name: 'خضروات طازجة',
                slug: 'vegetables',
                image: 'https://picsum.photos/seed/category-vegetables/900/675',
                isActive: true,
                displayOrder: 1
            },
            {
                id: 2,
                name: 'فواكه',
                slug: 'fruits',
                image: 'https://picsum.photos/seed/category-fruits/900/675',
                isActive: true,
                displayOrder: 2
            },
            {
                id: 3,
                name: 'منتجات الألبان',
                slug: 'dairy',
                image: 'https://picsum.photos/seed/category-dairy/900/675',
                isActive: true,
                displayOrder: 3
            },
            {
                id: 4,
                name: 'مشروبات',
                slug: 'beverages',
                image: 'https://picsum.photos/seed/category-beverages/900/675',
                isActive: true,
                displayOrder: 4
            },
            {
                id: 5,
                name: 'معلبات',
                slug: 'canned',
                image: 'https://picsum.photos/seed/category-canned/900/675',
                isActive: true,
                displayOrder: 5
            }
        ];
        localStorage.setItem(this.KEYS.CATEGORIES, JSON.stringify(categories));

        // Brands
        const brands = [
            { id: 1, name: 'Fresh Valley', slug: 'fresh-valley', isActive: true },
            { id: 2, name: 'Organic Choice', slug: 'organic-choice', isActive: true },
            { id: 3, name: 'Pure Delight', slug: 'pure-delight', isActive: true }
        ];
        localStorage.setItem(this.KEYS.BRANDS, JSON.stringify(brands));

        // Products
        const products = [
            {
                id: 1,
                name: 'طماطم طازجة',
                slug: 'fresh-tomatoes',
                categoryId: 1,
                brandId: 1,
                price: 2.50,
                currency: 'IQD',
                unit: 'kg',
                sku: 'VEG001',
                description: 'طماطم طازجة ومنتقاة بعناية من المزارع المحلية',
                isActive: true,
                isFeatured: true,
                images: [
                    'https://picsum.photos/seed/fresh-tomatoes/900/900',
                    'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="400" height="400"%3E%3Crect fill="%23ff6b6b" width="400" height="400"/%3E%3Ctext x="50%25" y="50%25" dominant-baseline="middle" text-anchor="middle" font-family="Arial" font-size="64" fill="white"%3E🍅%3C/text%3E%3C/svg%3E'
                ],
                createdAt: new Date().toISOString()
            },
            {
                id: 2,
                name: 'خيار عضوي',
                slug: 'organic-cucumber',
                categoryId: 1,
                brandId: 2,
                price: 1.80,
                currency: 'IQD',
                unit: 'kg',
                sku: 'VEG002',
                description: 'خيار عضوي بدون مبيدات',
                isActive: true,
                isFeatured: false,
                images: [
                    'https://picsum.photos/seed/organic-cucumber/900/900',
                    'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="400" height="400"%3E%3Crect fill="%232ecc71" width="400" height="400"/%3E%3Ctext x="50%25" y="50%25" dominant-baseline="middle" text-anchor="middle" font-family="Arial" font-size="64" fill="white"%3E🥒%3C/text%3E%3C/svg%3E'
                ],
                createdAt: new Date().toISOString()
            },
            {
                id: 3,
                name: 'تفاح أحمر',
                slug: 'red-apple',
                categoryId: 2,
                brandId: 1,
                price: 3.20,
                currency: 'IQD',
                unit: 'kg',
                sku: 'FRT001',
                description: 'تفاح أحمر لذيذ وطازج',
                isActive: true,
                isFeatured: true,
                images: [
                    'https://picsum.photos/seed/red-apple/900/900',
                    'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="400" height="400"%3E%3Crect fill="%23e74c3c" width="400" height="400"/%3E%3Ctext x="50%25" y="50%25" dominant-baseline="middle" text-anchor="middle" font-family="Arial" font-size="64" fill="white"%3E🍎%3C/text%3E%3C/svg%3E'
                ],
                createdAt: new Date().toISOString()
            },
            {
                id: 4,
                name: 'موز',
                slug: 'banana',
                categoryId: 2,
                brandId: 1,
                price: 2.00,
                currency: 'IQD',
                unit: 'kg',
                sku: 'FRT002',
                description: 'موز طازج غني بالبوتاسيوم',
                isActive: true,
                isFeatured: true,
                images: [
                    'https://picsum.photos/seed/banana/900/900',
                    'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="400" height="400"%3E%3Crect fill="%23f1c40f" width="400" height="400"/%3E%3Ctext x="50%25" y="50%25" dominant-baseline="middle" text-anchor="middle" font-family="Arial" font-size="64" fill="white"%3E🍌%3C/text%3E%3C/svg%3E'
                ],
                createdAt: new Date().toISOString()
            },
            {
                id: 5,
                name: 'حليب كامل الدسم',
                slug: 'whole-milk',
                categoryId: 3,
                brandId: 3,
                price: 1.50,
                currency: 'IQD',
                unit: 'لتر',
                sku: 'DRY001',
                description: 'حليب طازج كامل الدسم',
                isActive: true,
                isFeatured: false,
                images: [
                    'https://picsum.photos/seed/whole-milk/900/900',
                    'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="400" height="400"%3E%3Crect fill="%23ecf0f1" width="400" height="400"/%3E%3Ctext x="50%25" y="50%25" dominant-baseline="middle" text-anchor="middle" font-family="Arial" font-size="64" fill="%233498db"%3E🥛%3C/text%3E%3C/svg%3E'
                ],
                createdAt: new Date().toISOString()
            },
            {
                id: 6,
                name: 'جبنة شيدر',
                slug: 'cheddar-cheese',
                categoryId: 3,
                brandId: 3,
                price: 4.00,
                currency: 'IQD',
                unit: 'قطعة',
                sku: 'DRY002',
                description: 'جبنة شيدر عالية الجودة',
                isActive: true,
                isFeatured: false,
                images: [
                    'https://picsum.photos/seed/cheddar-cheese/900/900',
                    'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="400" height="400"%3E%3Crect fill="%23f39c12" width="400" height="400"/%3E%3Ctext x="50%25" y="50%25" dominant-baseline="middle" text-anchor="middle" font-family="Arial" font-size="64" fill="white"%3E🧀%3C/text%3E%3C/svg%3E'
                ],
                createdAt: new Date().toISOString()
            },
            {
                id: 7,
                name: 'عصير برتقال',
                slug: 'orange-juice',
                categoryId: 4,
                brandId: 2,
                price: 2.80,
                currency: 'IQD',
                unit: 'لتر',
                sku: 'BEV001',
                description: 'عصير برتقال طبيعي 100%',
                isActive: true,
                isFeatured: true,
                images: [
                    'https://picsum.photos/seed/orange-juice/900/900',
                    'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="400" height="400"%3E%3Crect fill="%23e67e22" width="400" height="400"/%3E%3Ctext x="50%25" y="50%25" dominant-baseline="middle" text-anchor="middle" font-family="Arial" font-size="64" fill="white"%3E🍊%3C/text%3E%3C/svg%3E'
                ],
                createdAt: new Date().toISOString()
            },
            {
                id: 8,
                name: 'ماء معدني',
                slug: 'mineral-water',
                categoryId: 4,
                brandId: 3,
                price: 0.50,
                currency: 'IQD',
                unit: 'قطعة',
                sku: 'BEV002',
                description: 'ماء معدني نقي',
                isActive: true,
                isFeatured: false,
                images: [
                    'https://picsum.photos/seed/mineral-water/900/900',
                    'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="400" height="400"%3E%3Crect fill="%233498db" width="400" height="400"/%3E%3Ctext x="50%25" y="50%25" dominant-baseline="middle" text-anchor="middle" font-family="Arial" font-size="64" fill="white"%3E💧%3C/text%3E%3C/svg%3E'
                ],
                createdAt: new Date().toISOString()
            }
        ];
        localStorage.setItem(this.KEYS.PRODUCTS, JSON.stringify(products));

        // Banners
        const banners = [
            {
                id: 1,
                title: 'عروض الأسبوع',
                images: [
                    'https://picsum.photos/seed/banner-weekly-1/1200/400',
                    'https://picsum.photos/seed/banner-weekly-2/1200/400',
                    'https://picsum.photos/seed/banner-weekly-3/1200/400'
                ],
                imageUrl: 'https://picsum.photos/seed/banner-weekly-1/1200/400',
                linkUrl: '#products',
                placement: 'HomeHero',
                startDate: new Date().toISOString(),
                endDate: null,
                isActive: true,
                displayOrder: 1
            },
            {
                id: 2,
                title: 'منتجات طازجة يومياً',
                images: [
                    'https://picsum.photos/seed/banner-fresh-1/1200/400',
                    'https://picsum.photos/seed/banner-fresh-2/1200/400',
                    'https://picsum.photos/seed/banner-fresh-3/1200/400'
                ],
                imageUrl: 'https://picsum.photos/seed/banner-fresh-1/1200/400',
                linkUrl: '#categories',
                placement: 'HomeHero',
                startDate: new Date().toISOString(),
                endDate: null,
                isActive: true,
                displayOrder: 2
            }
        ];
        localStorage.setItem(this.KEYS.BANNERS, JSON.stringify(banners));

        // Settings
        const settings = {
            storeName: 'متجر البقالة الطازجة',
            phone: '+964 750 123 4567',
            whatsappNumber: '+9647501234567',
            email: 'info@grocery.com',
            address: 'أربيل، كردستان العراق',
            googleMapsUrl: 'https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d25617.15!2d44.0094!3d36.1900!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x0%3A0x0!2zMzbCsDExJzI0LjAiTiA0NMKwMDAnMzMuOCJF!5e0!3m2!1sen!2siq!4v1234567890',
            openingHours: 'السبت - الخميس: 8 صباحاً - 10 مساءً\nالجمعة: 9 صباحاً - 11 مساءً'
        };
        localStorage.setItem(this.KEYS.SETTINGS, JSON.stringify(settings));
    },

    // Generic CRUD Operations
    getAll(key) {
        const data = localStorage.getItem(key);
        return data ? JSON.parse(data) : [];
    },

    getById(key, id) {
        const items = this.getAll(key);
        return items.find(item => item.id === parseInt(id));
    },

    save(key, items) {
        localStorage.setItem(key, JSON.stringify(items));
    },

    add(key, item) {
        const items = this.getAll(key);
        const maxId = items.length > 0 ? Math.max(...items.map(i => i.id)) : 0;
        item.id = maxId + 1;
        items.push(item);
        this.save(key, items);
        return item;
    },

    update(key, item) {
        const items = this.getAll(key);
        const index = items.findIndex(i => i.id === item.id);
        if (index !== -1) {
            items[index] = item;
            this.save(key, items);
            return true;
        }
        return false;
    },

    delete(key, id) {
        const items = this.getAll(key);
        const filtered = items.filter(item => item.id !== parseInt(id));
        this.save(key, filtered);
        return filtered.length < items.length;
    },

    // Specific Methods
    getProducts() {
        return this.getAll(this.KEYS.PRODUCTS);
    },

    getProductById(id) {
        return this.getById(this.KEYS.PRODUCTS, id);
    },

    getProductsByCategory(categoryId) {
        return this.getProducts().filter(p => p.categoryId === parseInt(categoryId) && p.isActive);
    },

    getFeaturedProducts(limit = 6) {
        return this.getProducts().filter(p => p.isFeatured && p.isActive).slice(0, limit);
    },

    searchProducts(query) {
        const products = this.getProducts();
        const lowerQuery = query.toLowerCase();
        return products.filter(p => 
            p.isActive && (
                p.name.toLowerCase().includes(lowerQuery) ||
                p.sku.toLowerCase().includes(lowerQuery) ||
                (p.description && p.description.toLowerCase().includes(lowerQuery))
            )
        );
    },

    getCategories() {
        return this.getAll(this.KEYS.CATEGORIES).filter(c => c.isActive);
    },

    getCategoryById(id) {
        return this.getById(this.KEYS.CATEGORIES, id);
    },

    getBrands() {
        return this.getAll(this.KEYS.BRANDS).filter(b => b.isActive);
    },

    getBrandById(id) {
        return this.getById(this.KEYS.BRANDS, id);
    },

    getBanners() {
        return this.getAll(this.KEYS.BANNERS)
            .filter(b => b.isActive)
            .sort((a, b) => a.displayOrder - b.displayOrder);
    },

    getSettings() {
        const settings = localStorage.getItem(this.KEYS.SETTINGS);
        return settings ? JSON.parse(settings) : null;
    },

    saveSettings(settings) {
        localStorage.setItem(this.KEYS.SETTINGS, JSON.stringify(settings));
    },

    // Admin
    getAdmin() {
        const admin = localStorage.getItem(this.KEYS.ADMIN);
        return admin ? JSON.parse(admin) : null;
    },

    saveAdmin(admin) {
        localStorage.setItem(this.KEYS.ADMIN, JSON.stringify(admin));
    },

    verifyAdmin(username, password) {
        const admin = this.getAdmin();
        return admin && admin.username === username && admin.password === password;
    }
};

// Initialize on load
DataManager.init();
