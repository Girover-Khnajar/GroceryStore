// ============================================
// MAIN APPLICATION LOGIC
// ============================================

const App = {
    // State
    state: {
        currentPage: 1,
        itemsPerPage: 12,
        selectedCategory: null,
        selectedBrand: null,
        searchQuery: '',
        sortBy: 'newest'
    },

    // Initialize App
    init() {
        this.setupRouter();
        this.setupEventListeners();
        Router.init();
    },

    // Setup Routes
    setupRouter() {
        Router.register('home', () => this.renderHome());
        Router.register('categories', () => this.renderCategories());
        Router.register('category', (slug) => this.renderCategoryProducts(slug));
        Router.register('products', () => this.renderProducts());
        Router.register('product', (slug) => this.renderProductDetail(slug));
        Router.register('contact', () => this.renderContact());
    },

    // Setup Event Listeners
    setupEventListeners() {
        // Mobile Menu Toggle
        const menuToggle = document.getElementById('menuToggle');
        const navMenu = document.getElementById('navMenu');
        menuToggle?.addEventListener('click', () => {
            navMenu.classList.toggle('active');
        });

        // Search Toggle
        const searchToggle = document.getElementById('searchToggle');
        const searchOverlay = document.getElementById('searchOverlay');
        const searchClose = document.getElementById('searchClose');
        const globalSearch = document.getElementById('globalSearch');

        searchToggle?.addEventListener('click', () => {
            searchOverlay.classList.add('active');
            globalSearch.focus();
        });

        searchClose?.addEventListener('click', () => {
            searchOverlay.classList.remove('active');
        });

        searchOverlay?.addEventListener('click', (e) => {
            if (e.target === searchOverlay) {
                searchOverlay.classList.remove('active');
            }
        });

        // Global Search
        globalSearch?.addEventListener('input', (e) => {
            this.handleGlobalSearch(e.target.value);
        });

        // Navbar Scroll Effect
        let lastScroll = 0;
        window.addEventListener('scroll', () => {
            const navbar = document.getElementById('navbar');
            const currentScroll = window.pageYOffset;
            
            if (currentScroll > 100) {
                navbar.classList.add('scrolled');
            } else {
                navbar.classList.remove('scrolled');
            }
            
            lastScroll = currentScroll;
        });
    },

    // Global Search Handler
    handleGlobalSearch(query) {
        const searchResults = document.getElementById('searchResults');
        
        if (!query.trim()) {
            searchResults.innerHTML = '<p style="text-align:center;color:var(--text-muted);padding:2rem;">ابحث عن المنتجات...</p>';
            return;
        }

        const products = DataManager.searchProducts(query);
        const categories = DataManager.getCategories().filter(c => 
            c.name.toLowerCase().includes(query.toLowerCase())
        );

        let html = '';

        if (categories.length > 0) {
            html += '<h4 style="padding:1rem 0;color:var(--text-light);font-size:0.875rem;text-transform:uppercase;">الأقسام</h4>';
            categories.forEach(cat => {
                html += `
                    <div class="search-result-item" onclick="Router.navigate('category/${cat.slug}')">
                        <img src="${cat.image}" alt="${cat.name}">
                        <div class="search-result-info">
                            <h4>${cat.name}</h4>
                            <p>قسم</p>
                        </div>
                    </div>
                `;
            });
        }

        if (products.length > 0) {
            html += '<h4 style="padding:1rem 0;color:var(--text-light);font-size:0.875rem;text-transform:uppercase;">المنتجات</h4>';
            products.slice(0, 10).forEach(product => {
                const category = DataManager.getCategoryById(product.categoryId);
                html += `
                    <div class="search-result-item" onclick="Router.navigate('product/${product.slug}')">
                        <img src="${product.images[0]}" alt="${product.name}">
                        <div class="search-result-info">
                            <h4>${product.name}</h4>
                            <p>${category?.name || ''} • ${product.price} ${product.currency}/${product.unit}</p>
                        </div>
                    </div>
                `;
            });
        }

        if (categories.length === 0 && products.length === 0) {
            html = '<div class="empty-state"><i class="fas fa-search"></i><h3>لا توجد نتائج</h3><p>حاول البحث بكلمات أخرى</p></div>';
        }

        searchResults.innerHTML = html;
    },

    // Render Home Page
    renderHome() {
        const app = document.getElementById('app');
        const banners = DataManager.getBanners();
        const categories = DataManager.getCategories().slice(0, 5);
        const featuredProducts = DataManager.getFeaturedProducts();

        app.innerHTML = `
            <!-- Hero Section -->
            <section class="hero">
                <div class="container">
                    <div class="hero-content">
                        <div class="hero-text">
                            <h1>مرحباً بك في متجر البقالة</h1>
                            <p>أفضل المنتجات الطازجة والعروض الحصرية بين يديك</p>
                            <div class="hero-actions">
                                <button class="btn btn-primary" onclick="Router.navigate('products')">
                                    <i class="fas fa-shopping-basket"></i>
                                    تسوق الآن
                                </button>
                                <button class="btn btn-outline" onclick="Router.navigate('categories')">
                                    <i class="fas fa-th-large"></i>
                                    الأقسام
                                </button>
                            </div>
                        </div>
                        <div class="hero-image">
                            <img src="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='600' height='600'%3E%3Crect fill='%23ffffff' width='600' height='600' rx='20'/%3E%3Ctext x='50%25' y='50%25' dominant-baseline='middle' text-anchor='middle' font-family='Arial' font-size='200' fill='%232ecc71'%3E🛒%3C/text%3E%3C/svg%3E" alt="Shopping">
                        </div>
                    </div>
                </div>
            </section>

            <!-- Featured Categories -->
            <section class="section">
                <div class="container">
                    <h2 class="section-title">الأقسام المميزة</h2>
                    <div class="categories-grid">
                        ${categories.map(cat => `
                            <div class="category-card" onclick="Router.navigate('category/${cat.slug}')">
                                <div class="category-image">
                                    <img src="${cat.image}" alt="${cat.name}">
                                </div>
                                <div class="category-info">
                                    <h3>${cat.name}</h3>
                                    <p>اكتشف المزيد</p>
                                </div>
                            </div>
                        `).join('')}
                    </div>
                    <div style="text-align:center;margin-top:2rem;">
                        <button class="btn btn-outline" onclick="Router.navigate('categories')">
                            عرض جميع الأقسام
                            <i class="fas fa-arrow-left"></i>
                        </button>
                    </div>
                </div>
            </section>

            <!-- Featured Products -->
            <section class="section" style="background:white;">
                <div class="container">
                    <h2 class="section-title">المنتجات المميزة</h2>
                    <div class="products-grid">
                        ${this.renderProductCards(featuredProducts)}
                    </div>
                    <div style="text-align:center;margin-top:2rem;">
                        <button class="btn btn-primary" onclick="Router.navigate('products')">
                            عرض جميع المنتجات
                            <i class="fas fa-arrow-left"></i>
                        </button>
                    </div>
                </div>
            </section>
        `;

        // Auto-rotate banners if exists
        if (banners.length > 1) {
            this.startBannerSlider();
        }
    },

    // Render Categories Page
    renderCategories() {
        const app = document.getElementById('app');
        const categories = DataManager.getCategories();

        app.innerHTML = `
            <section class="section">
                <div class="container">
                    <h1 class="section-title">جميع الأقسام</h1>
                    <div class="categories-grid">
                        ${categories.map(cat => {
                            const productCount = DataManager.getProductsByCategory(cat.id).length;
                            return `
                                <div class="category-card" onclick="Router.navigate('category/${cat.slug}')">
                                    <div class="category-image">
                                        <img src="${cat.image}" alt="${cat.name}">
                                    </div>
                                    <div class="category-info">
                                        <h3>${cat.name}</h3>
                                        <p>${productCount} منتج</p>
                                    </div>
                                </div>
                            `;
                        }).join('')}
                    </div>
                </div>
            </section>
        `;
    },

    // Render Category Products
    renderCategoryProducts(slug) {
        const app = document.getElementById('app');
        const categories = DataManager.getCategories();
        const category = categories.find(c => c.slug === slug);
        
        if (!category) {
            app.innerHTML = '<div class="empty-state"><i class="fas fa-exclamation-triangle"></i><h3>القسم غير موجود</h3></div>';
            return;
        }

        const products = DataManager.getProductsByCategory(category.id);

        app.innerHTML = `
            <section class="section">
                <div class="container">
                    <div style="margin-bottom:2rem;">
                        <button class="btn btn-outline" onclick="Router.navigate('categories')" style="margin-bottom:1rem;">
                            <i class="fas fa-arrow-right"></i>
                            العودة للأقسام
                        </button>
                        <h1 class="section-title">${category.name}</h1>
                    </div>
                    ${products.length > 0 ? `
                        <div class="products-grid">
                            ${this.renderProductCards(products)}
                        </div>
                    ` : `
                        <div class="empty-state">
                            <i class="fas fa-box-open"></i>
                            <h3>لا توجد منتجات في هذا القسم</h3>
                            <p>سيتم إضافة منتجات قريباً</p>
                        </div>
                    `}
                </div>
            </section>
        `;
    },

    // Render Products Page
    renderProducts() {
        const app = document.getElementById('app');
        const products = DataManager.getProducts().filter(p => p.isActive);
        const categories = DataManager.getCategories();
        const brands = DataManager.getBrands();

        app.innerHTML = `
            <section class="section">
                <div class="container">
                    <h1 class="section-title">جميع المنتجات</h1>
                    
                    <div class="products-header">
                        <div class="filters-bar">
                            <button class="filter-btn active" onclick="App.filterByCategory(null)">
                                <i class="fas fa-border-all"></i>
                                الكل
                            </button>
                            ${categories.map(cat => `
                                <button class="filter-btn" onclick="App.filterByCategory(${cat.id})">
                                    ${cat.name}
                                </button>
                            `).join('')}
                        </div>
                    </div>

                    <div class="products-grid" id="productsGrid">
                        ${this.renderProductCards(products)}
                    </div>

                    ${products.length > this.state.itemsPerPage ? `
                        <div class="pagination" id="pagination"></div>
                    ` : ''}
                </div>
            </section>
        `;

        this.renderPagination(products.length);
    },

    // Filter Products by Category
    filterByCategory(categoryId) {
        this.state.selectedCategory = categoryId;
        this.state.currentPage = 1;
        
        const products = categoryId 
            ? DataManager.getProductsByCategory(categoryId)
            : DataManager.getProducts().filter(p => p.isActive);
        
        const productsGrid = document.getElementById('productsGrid');
        productsGrid.innerHTML = this.renderProductCards(products);
        
        // Update active filter button
        document.querySelectorAll('.filter-btn').forEach((btn, index) => {
            btn.classList.toggle('active', index === (categoryId === null ? 0 : categoryId));
        });
        
        this.renderPagination(products.length);
    },

    // Render Product Cards
    renderProductCards(products) {
        return products.map(product => {
            const category = DataManager.getCategoryById(product.categoryId);
            const settings = DataManager.getSettings();
            
            return `
                <div class="product-card">
                    ${product.isFeatured ? '<span class="product-badge">مميز</span>' : ''}
                    <div class="product-image" onclick="Router.navigate('product/${product.slug}')">
                        <img src="${product.images[0]}" alt="${product.name}">
                    </div>
                    <div class="product-info">
                        <div class="product-category">${category?.name || ''}</div>
                        <h3 class="product-name">${product.name}</h3>
                        <div class="product-price">
                            <span class="price-current">${product.price}</span>
                            <span class="price-unit">${product.currency}/${product.unit}</span>
                        </div>
                        <div class="product-actions">
                            <button class="btn-whatsapp" onclick="App.sendWhatsApp('${product.name}', ${product.price}, '${product.unit}')">
                                <i class="fab fa-whatsapp"></i>
                                واتساب
                            </button>
                            <button class="btn-details" onclick="Router.navigate('product/${product.slug}')">
                                <i class="fas fa-info-circle"></i>
                            </button>
                        </div>
                    </div>
                </div>
            `;
        }).join('');
    },

    // Render Product Detail
    renderProductDetail(slug) {
        const app = document.getElementById('app');
        const products = DataManager.getProducts();
        const product = products.find(p => p.slug === slug);
        
        if (!product) {
            app.innerHTML = '<div class="empty-state"><i class="fas fa-exclamation-triangle"></i><h3>المنتج غير موجود</h3></div>';
            return;
        }

        const category = DataManager.getCategoryById(product.categoryId);
        const brand = DataManager.getBrandById(product.brandId);

        app.innerHTML = `
            <section class="section">
                <div class="container">
                    <button class="btn btn-outline" onclick="Router.navigate('products')" style="margin-bottom:2rem;">
                        <i class="fas fa-arrow-right"></i>
                        العودة للمنتجات
                    </button>

                    <div class="product-detail">
                        <div class="product-gallery">
                            <div class="main-image" id="mainImage">
                                <img src="${product.images[0]}" alt="${product.name}">
                            </div>
                            ${product.images.length > 1 ? `
                                <div class="thumbnail-images">
                                    ${product.images.map((img, i) => `
                                        <div class="thumbnail ${i === 0 ? 'active' : ''}" onclick="App.changeImage('${img}', this)">
                                            <img src="${img}" alt="${product.name}">
                                        </div>
                                    `).join('')}
                                </div>
                            ` : ''}
                        </div>

                        <div class="product-details-info">
                            <div class="product-meta">
                                ${category ? `<span class="meta-badge">${category.name}</span>` : ''}
                                ${brand ? `<span class="meta-badge">${brand.name}</span>` : ''}
                                ${product.sku ? `<span class="meta-badge">${product.sku}</span>` : ''}
                            </div>

                            <h1 class="product-title">${product.name}</h1>

                            <div class="product-price-large">
                                <span class="price-large">${product.price}</span>
                                <span class="price-unit">${product.currency}/${product.unit}</span>
                            </div>

                            ${product.description ? `
                                <p class="product-description">${product.description}</p>
                            ` : ''}

                            <div class="product-quantity">
                                <label>الكمية:</label>
                                <div class="quantity-control">
                                    <button onclick="App.decreaseQuantity()">
                                        <i class="fas fa-minus"></i>
                                    </button>
                                    <input type="number" id="quantity" class="quantity-input" value="1" min="1" max="100">
                                    <button onclick="App.increaseQuantity()">
                                        <i class="fas fa-plus"></i>
                                    </button>
                                </div>
                            </div>

                            <button class="btn-whatsapp-large" onclick="App.sendWhatsAppWithQuantity('${product.name}', ${product.price}, '${product.unit}')">
                                <i class="fab fa-whatsapp"></i>
                                اطلب عبر واتساب
                            </button>
                        </div>
                    </div>
                </div>
            </section>
        `;
    },

    // Render Contact Page
    renderContact() {
        const app = document.getElementById('app');
        const settings = DataManager.getSettings();

        app.innerHTML = `
            <section class="section">
                <div class="container">
                    <h1 class="section-title">تواصل معنا</h1>
                    
                    <div class="contact-grid">
                        <div class="contact-info-card">
                            <div class="contact-item">
                                <div class="contact-icon">
                                    <i class="fas fa-phone"></i>
                                </div>
                                <div class="contact-details">
                                    <h4>الهاتف</h4>
                                    <p dir="ltr"><a href="tel:${settings.phone}">${settings.phone}</a></p>
                                </div>
                            </div>

                            <div class="contact-item">
                                <div class="contact-icon">
                                    <i class="fab fa-whatsapp"></i>
                                </div>
                                <div class="contact-details">
                                    <h4>واتساب</h4>
                                    <p dir="ltr"><a href="https://wa.me/${settings.whatsappNumber}" target="_blank">${settings.phone}</a></p>
                                </div>
                            </div>

                            <div class="contact-item">
                                <div class="contact-icon">
                                    <i class="fas fa-envelope"></i>
                                </div>
                                <div class="contact-details">
                                    <h4>البريد الإلكتروني</h4>
                                    <p><a href="mailto:${settings.email}">${settings.email}</a></p>
                                </div>
                            </div>

                            <div class="contact-item">
                                <div class="contact-icon">
                                    <i class="fas fa-map-marker-alt"></i>
                                </div>
                                <div class="contact-details">
                                    <h4>العنوان</h4>
                                    <p>${settings.address}</p>
                                </div>
                            </div>

                            <div class="contact-item">
                                <div class="contact-icon">
                                    <i class="fas fa-clock"></i>
                                </div>
                                <div class="contact-details">
                                    <h4>ساعات العمل</h4>
                                    <p style="white-space:pre-line;">${settings.openingHours}</p>
                                </div>
                            </div>
                        </div>

                        <div class="map-container">
                            <iframe src="${settings.googleMapsUrl}" allowfullscreen="" loading="lazy"></iframe>
                        </div>
                    </div>
                </div>
            </section>
        `;
    },

    // Pagination
    renderPagination(totalItems) {
        const totalPages = Math.ceil(totalItems / this.state.itemsPerPage);
        const pagination = document.getElementById('pagination');
        
        if (!pagination || totalPages <= 1) return;

        let html = `
            <button class="page-btn" onclick="App.goToPage(${this.state.currentPage - 1})" ${this.state.currentPage === 1 ? 'disabled' : ''}>
                <i class="fas fa-chevron-right"></i>
            </button>
        `;

        for (let i = 1; i <= totalPages; i++) {
            html += `
                <button class="page-btn ${i === this.state.currentPage ? 'active' : ''}" onclick="App.goToPage(${i})">
                    ${i}
                </button>
            `;
        }

        html += `
            <button class="page-btn" onclick="App.goToPage(${this.state.currentPage + 1})" ${this.state.currentPage === totalPages ? 'disabled' : ''}>
                <i class="fas fa-chevron-left"></i>
            </button>
        `;

        pagination.innerHTML = html;
    },

    goToPage(page) {
        this.state.currentPage = page;
        this.renderProducts();
    },

    // WhatsApp Functions
    sendWhatsApp(productName, price, unit) {
        const settings = DataManager.getSettings();
        const message = `مرحباً، أريد طلب: ${productName} (${price} ${settings.currency || 'IQD'}/${unit})`;
        const whatsappUrl = `https://wa.me/${settings.whatsappNumber}?text=${encodeURIComponent(message)}`;
        window.open(whatsappUrl, '_blank');
    },

    sendWhatsAppWithQuantity(productName, price, unit) {
        const quantity = document.getElementById('quantity').value;
        const settings = DataManager.getSettings();
        const total = (price * quantity).toFixed(2);
        const message = `مرحباً، أريد طلب:\n${productName}\nالكمية: ${quantity} ${unit}\nالسعر الإجمالي: ${total} ${settings.currency || 'IQD'}`;
        const whatsappUrl = `https://wa.me/${settings.whatsappNumber}?text=${encodeURIComponent(message)}`;
        window.open(whatsappUrl, '_blank');
    },

    // Utility Functions
    changeImage(imgSrc, element) {
        document.querySelector('#mainImage img').src = imgSrc;
        document.querySelectorAll('.thumbnail').forEach(t => t.classList.remove('active'));
        element.classList.add('active');
    },

    increaseQuantity() {
        const input = document.getElementById('quantity');
        input.value = parseInt(input.value) + 1;
    },

    decreaseQuantity() {
        const input = document.getElementById('quantity');
        if (parseInt(input.value) > 1) {
            input.value = parseInt(input.value) - 1;
        }
    },

    showToast(message, type = 'success') {
        const toast = document.getElementById('toast');
        const toastMessage = document.getElementById('toastMessage');
        const icon = toast.querySelector('i');
        
        toastMessage.textContent = message;
        
        if (type === 'success') {
            icon.className = 'fas fa-check-circle';
        } else if (type === 'error') {
            icon.className = 'fas fa-exclamation-circle';
        }
        
        toast.classList.add('show');
        
        setTimeout(() => {
            toast.classList.remove('show');
        }, 3000);
    },

    showLoading() {
        document.getElementById('loadingSpinner').classList.add('show');
    },

    hideLoading() {
        document.getElementById('loadingSpinner').classList.remove('show');
    }
};

// Initialize App
document.addEventListener('DOMContentLoaded', () => {
    App.init();
});
