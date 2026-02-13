// ============================================
// ADMIN PANEL LOGIC
// ============================================

const Admin = {
    currentPage: 'dashboard',
    editingItem: null,

    // Initialize Admin
    init() {
        this.checkAuth();
        this.setupEventListeners();
    },

    // Check Authentication
    checkAuth() {
        const isLoggedIn = sessionStorage.getItem('admin_logged_in');
        const loginScreen = document.getElementById('loginScreen');
        const dashboard = document.getElementById('adminDashboard');

        if (isLoggedIn) {
            loginScreen.style.display = 'none';
            dashboard.style.display = 'grid';
            this.loadPage('dashboard');
        } else {
            loginScreen.style.display = 'flex';
            dashboard.style.display = 'none';
        }
    },

    // Setup Event Listeners
    setupEventListeners() {
        // Login Form
        const loginForm = document.getElementById('loginForm');
        loginForm?.addEventListener('submit', (e) => {
            e.preventDefault();
            this.handleLogin();
        });

        // Logout Button
        const logoutBtn = document.getElementById('logoutBtn');
        logoutBtn?.addEventListener('click', () => this.handleLogout());

        // Menu Items
        const menuItems = document.querySelectorAll('.menu-item');
        menuItems.forEach(item => {
            item.addEventListener('click', (e) => {
                e.preventDefault();
                const page = item.dataset.page;
                this.loadPage(page);
                
                // Update active state
                menuItems.forEach(m => m.classList.remove('active'));
                item.classList.add('active');
            });
        });

        // Modal Close
        const modalClose = document.getElementById('modalClose');
        modalClose?.addEventListener('click', () => this.closeModal());

        const modal = document.getElementById('modal');
        modal?.addEventListener('click', (e) => {
            if (e.target === modal) {
                this.closeModal();
            }
        });
    },

    // Handle Login
    handleLogin() {
        const username = document.getElementById('username').value;
        const password = document.getElementById('password').value;

        if (DataManager.verifyAdmin(username, password)) {
            sessionStorage.setItem('admin_logged_in', 'true');
            this.showToast('تم تسجيل الدخول بنجاح', 'success');
            this.checkAuth();
        } else {
            this.showToast('اسم المستخدم أو كلمة المرور غير صحيحة', 'error');
        }
    },

    // Handle Logout
    handleLogout() {
        sessionStorage.removeItem('admin_logged_in');
        this.showToast('تم تسجيل الخروج', 'success');
        window.location.reload();
    },

    // Load Page
    loadPage(page) {
        this.currentPage = page;
        const pageTitle = document.getElementById('pageTitle');
        const adminContent = document.getElementById('adminContent');

        const pageTitles = {
            dashboard: 'الإحصائيات',
            products: 'إدارة المنتجات',
            categories: 'إدارة الأقسام',
            brands: 'إدارة الماركات',
            banners: 'إدارة البانرات',
            settings: 'الإعدادات'
        };

        pageTitle.textContent = pageTitles[page] || 'لوحة التحكم';

        switch (page) {
            case 'dashboard':
                this.renderDashboard(adminContent);
                break;
            case 'products':
                this.renderProducts(adminContent);
                break;
            case 'categories':
                this.renderCategories(adminContent);
                break;
            case 'brands':
                this.renderBrands(adminContent);
                break;
            case 'banners':
                this.renderBanners(adminContent);
                break;
            case 'settings':
                this.renderSettings(adminContent);
                break;
        }

        window.UI?.refreshAOS?.();
    },

    // Render Dashboard
    renderDashboard(container) {
        const products = DataManager.getProducts();
        const categories = DataManager.getCategories();
        const brands = DataManager.getBrands();
        const banners = DataManager.getBanners();

        container.innerHTML = `
            <div class="stats-grid">
                <div class="stat-card" data-aos="fade-up" data-aos-delay="0">
                    <div class="stat-icon products">
                        <i class="fas fa-box-open"></i>
                    </div>
                    <div class="stat-details">
                        <h3>${products.length}</h3>
                        <p>المنتجات</p>
                    </div>
                </div>
                <div class="stat-card" data-aos="fade-up" data-aos-delay="80">
                    <div class="stat-icon categories">
                        <i class="fas fa-th-large"></i>
                    </div>
                    <div class="stat-details">
                        <h3>${categories.length}</h3>
                        <p>الأقسام</p>
                    </div>
                </div>
                <div class="stat-card" data-aos="fade-up" data-aos-delay="160">
                    <div class="stat-icon brands">
                        <i class="fas fa-tag"></i>
                    </div>
                    <div class="stat-details">
                        <h3>${brands.length}</h3>
                        <p>الماركات</p>
                    </div>
                </div>
                <div class="stat-card" data-aos="fade-up" data-aos-delay="240">
                    <div class="stat-icon banners">
                        <i class="fas fa-images"></i>
                    </div>
                    <div class="stat-details">
                        <h3>${banners.length}</h3>
                        <p>البانرات</p>
                    </div>
                </div>
            </div>

            <div class="table-card" data-aos="fade-up" style="margin-top:2rem;">
                <div class="table-header">
                    <h3>آخر المنتجات المضافة</h3>
                </div>
                <div class="table-wrapper">
                    <table class="data-table">
                        <thead>
                            <tr>
                                <th>الصورة</th>
                                <th>الاسم</th>
                                <th>القسم</th>
                                <th>السعر</th>
                                <th>الحالة</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${products.slice(-5).reverse().map(p => {
                                const cat = DataManager.getCategoryById(p.categoryId);
                                return `
                                    <tr>
                                        <td><img src="${p.images?.[0] || 'images/ui/product-placeholder.svg'}" class="table-image" alt="${p.name}" loading="lazy" onerror="this.onerror=null;this.src='images/ui/product-placeholder.svg';"></td>
                                        <td>${p.name}</td>
                                        <td>${cat?.name || '-'}</td>
                                        <td>${p.price} ${p.currency}/${p.unit}</td>
                                        <td><span class="badge ${p.isActive ? 'badge-success' : 'badge-danger'}">${p.isActive ? 'نشط' : 'غير نشط'}</span></td>
                                    </tr>
                                `;
                            }).join('')}
                        </tbody>
                    </table>
                </div>
            </div>
        `;
    },

    // Render Products Management
    renderProducts(container) {
        const products = DataManager.getProducts();
        
        container.innerHTML = `
            <div class="table-card">
                <div class="table-header">
                    <h3>جميع المنتجات (${products.length})</h3>
                    <button class="btn-add" onclick="Admin.openProductModal()">
                        <i class="fas fa-plus"></i>
                        إضافة منتج جديد
                    </button>
                </div>
                <div class="table-wrapper">
                    <table class="data-table">
                        <thead>
                            <tr>
                                <th>الصورة</th>
                                <th>الاسم</th>
                                <th>القسم</th>
                                <th>السعر</th>
                                <th>الحالة</th>
                                <th>الإجراءات</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${products.map(p => {
                                const cat = DataManager.getCategoryById(p.categoryId);
                                return `
                                    <tr>
                                        <td><img src="${p.images?.[0] || 'images/ui/product-placeholder.svg'}" class="table-image" alt="${p.name}" loading="lazy" onerror="this.onerror=null;this.src='images/ui/product-placeholder.svg';"></td>
                                        <td>${p.name}</td>
                                        <td>${cat?.name || '-'}</td>
                                        <td>${p.price} ${p.currency}/${p.unit}</td>
                                        <td><span class="badge ${p.isActive ? 'badge-success' : 'badge-danger'}">${p.isActive ? 'نشط' : 'غير نشط'}</span></td>
                                        <td>
                                            <div class="table-actions">
                                                <button class="btn-icon btn-edit" onclick="Admin.editProduct(${p.id})">
                                                    <i class="fas fa-edit"></i>
                                                </button>
                                                <button class="btn-icon btn-delete" onclick="Admin.deleteProduct(${p.id})">
                                                    <i class="fas fa-trash"></i>
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                `;
                            }).join('')}
                        </tbody>
                    </table>
                </div>
            </div>
        `;
    },

    // Render Categories Management
    renderCategories(container) {
        const categories = DataManager.getAll(DataManager.KEYS.CATEGORIES);
        
        container.innerHTML = `
            <div class="table-card">
                <div class="table-header">
                    <h3>جميع الأقسام (${categories.length})</h3>
                    <button class="btn-add" onclick="Admin.openCategoryModal()">
                        <i class="fas fa-plus"></i>
                        إضافة قسم جديد
                    </button>
                </div>
                <div class="table-wrapper">
                    <table class="data-table">
                        <thead>
                            <tr>
                                <th>الصورة</th>
                                <th>الاسم</th>
                                <th>الحالة</th>
                                <th>الإجراءات</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${categories.map(c => `
                                <tr>
                                    <td><img src="${c.image || 'images/ui/category-placeholder.svg'}" class="table-image" alt="${c.name}" loading="lazy" onerror="this.onerror=null;this.src='images/ui/category-placeholder.svg';"></td>
                                    <td>${c.name}</td>
                                    <td><span class="badge ${c.isActive ? 'badge-success' : 'badge-danger'}">${c.isActive ? 'نشط' : 'غير نشط'}</span></td>
                                    <td>
                                        <div class="table-actions">
                                            <button class="btn-icon btn-edit" onclick="Admin.editCategory(${c.id})">
                                                <i class="fas fa-edit"></i>
                                            </button>
                                            <button class="btn-icon btn-delete" onclick="Admin.deleteCategory(${c.id})">
                                                <i class="fas fa-trash"></i>
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>
                </div>
            </div>
        `;
    },

    // Render Brands Management
    renderBrands(container) {
        const brands = DataManager.getAll(DataManager.KEYS.BRANDS);
        
        container.innerHTML = `
            <div class="table-card">
                <div class="table-header">
                    <h3>جميع الماركات (${brands.length})</h3>
                    <button class="btn-add" onclick="Admin.openBrandModal()">
                        <i class="fas fa-plus"></i>
                        إضافة ماركة جديدة
                    </button>
                </div>
                <div class="table-wrapper">
                    <table class="data-table">
                        <thead>
                            <tr>
                                <th>الاسم</th>
                                <th>الحالة</th>
                                <th>الإجراءات</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${brands.map(b => `
                                <tr>
                                    <td>${b.name}</td>
                                    <td><span class="badge ${b.isActive ? 'badge-success' : 'badge-danger'}">${b.isActive ? 'نشط' : 'غير نشط'}</span></td>
                                    <td>
                                        <div class="table-actions">
                                            <button class="btn-icon btn-edit" onclick="Admin.editBrand(${b.id})">
                                                <i class="fas fa-edit"></i>
                                            </button>
                                            <button class="btn-icon btn-delete" onclick="Admin.deleteBrand(${b.id})">
                                                <i class="fas fa-trash"></i>
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>
                </div>
            </div>
        `;
    },

    // Render Banners Management
    renderBanners(container) {
        const banners = DataManager.getAll(DataManager.KEYS.BANNERS);
        
        container.innerHTML = `
            <div class="table-card">
                <div class="table-header">
                    <h3>جميع البانرات (${banners.length})</h3>
                    <button class="btn-add" onclick="Admin.openBannerModal()">
                        <i class="fas fa-plus"></i>
                        إضافة بانر جديد
                    </button>
                </div>
                <div class="table-wrapper">
                    <table class="data-table">
                        <thead>
                            <tr>
                                <th>الصورة</th>
                                <th>العنوان</th>
                                <th>الموضع</th>
                                <th>الحالة</th>
                                <th>الإجراءات</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${banners.map(b => `
                                <tr>
                                    <td><img src="${(b.images && b.images[0]) || b.imageUrl || 'images/ui/banner-placeholder.svg'}" class="table-image" alt="${b.title}" loading="lazy" onerror="this.onerror=null;this.src='images/ui/banner-placeholder.svg';"></td>
                                    <td>${b.title}</td>
                                    <td>${b.placement}</td>
                                    <td><span class="badge ${b.isActive ? 'badge-success' : 'badge-danger'}">${b.isActive ? 'نشط' : 'غير نشط'}</span></td>
                                    <td>
                                        <div class="table-actions">
                                            <button class="btn-icon btn-edit" onclick="Admin.editBanner(${b.id})">
                                                <i class="fas fa-edit"></i>
                                            </button>
                                            <button class="btn-icon btn-delete" onclick="Admin.deleteBanner(${b.id})">
                                                <i class="fas fa-trash"></i>
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>
                </div>
            </div>
        `;
    },

    // Render Settings
    renderSettings(container) {
        const settings = DataManager.getSettings();
        
        container.innerHTML = `
            <div class="table-card">
                <div class="table-header">
                    <h3>إعدادات المتجر</h3>
                </div>
                <div style="padding:2rem;">
                    <form id="settingsForm" class="modal-form">
                        <div class="form-group">
                            <label><i class="fas fa-store"></i> اسم المتجر</label>
                            <input type="text" name="storeName" value="${settings.storeName}" required>
                        </div>
                        <div class="form-row">
                            <div class="form-group">
                                <label><i class="fas fa-phone"></i> الهاتف</label>
                                <input type="text" name="phone" value="${settings.phone}" required>
                            </div>
                            <div class="form-group">
                                <label><i class="fab fa-whatsapp"></i> واتساب</label>
                                <input type="text" name="whatsappNumber" value="${settings.whatsappNumber}" required>
                            </div>
                        </div>
                        <div class="form-group">
                            <label><i class="fas fa-envelope"></i> البريد الإلكتروني</label>
                            <input type="email" name="email" value="${settings.email}" required>
                        </div>
                        <div class="form-group">
                            <label><i class="fas fa-map-marker-alt"></i> العنوان</label>
                            <input type="text" name="address" value="${settings.address}" required>
                        </div>
                        <div class="form-group">
                            <label><i class="fas fa-map"></i> رابط خريطة Google</label>
                            <input type="url" name="googleMapsUrl" value="${settings.googleMapsUrl}" required>
                        </div>
                        <div class="form-group">
                            <label><i class="fas fa-clock"></i> ساعات العمل</label>
                            <textarea name="openingHours" rows="3" required>${settings.openingHours}</textarea>
                        </div>
                        <div class="form-actions">
                            <button type="submit" class="btn-submit">
                                <i class="fas fa-save"></i>
                                حفظ التغييرات
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        `;

        // Handle Settings Form Submit
        document.getElementById('settingsForm').addEventListener('submit', (e) => {
            e.preventDefault();
            const formData = new FormData(e.target);
            const settings = Object.fromEntries(formData);
            DataManager.saveSettings(settings);
            this.showToast('تم حفظ الإعدادات بنجاح', 'success');
        });
    },

    // Product Modal
    openProductModal(productId = null) {
        const isEdit = productId !== null;
        const product = isEdit ? DataManager.getProductById(productId) : null;
        const categories = DataManager.getCategories();
        const brands = DataManager.getBrands();

        const modalTitle = document.getElementById('modalTitle');
        const modalBody = document.getElementById('modalBody');
        
        modalTitle.textContent = isEdit ? 'تعديل المنتج' : 'إضافة منتج جديد';

        modalBody.innerHTML = `
            <form id="productForm" class="modal-form">
                <div class="form-group">
                    <label>اسم المنتج</label>
                    <input type="text" name="name" value="${product?.name || ''}" required>
                </div>
                <div class="form-row">
                    <div class="form-group">
                        <label>القسم</label>
                        <select name="categoryId" required>
                            <option value="">اختر القسم</option>
                            ${categories.map(c => `
                                <option value="${c.id}" ${product?.categoryId === c.id ? 'selected' : ''}>${c.name}</option>
                            `).join('')}
                        </select>
                    </div>
                    <div class="form-group">
                        <label>الماركة</label>
                        <select name="brandId">
                            <option value="">اختر الماركة (اختياري)</option>
                            ${brands.map(b => `
                                <option value="${b.id}" ${product?.brandId === b.id ? 'selected' : ''}>${b.name}</option>
                            `).join('')}
                        </select>
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-group">
                        <label>السعر</label>
                        <input type="number" step="0.01" name="price" value="${product?.price || ''}" required>
                    </div>
                    <div class="form-group">
                        <label>الوحدة</label>
                        <input type="text" name="unit" value="${product?.unit || 'kg'}" required>
                    </div>
                </div>
                <div class="form-group">
                    <label>الوصف</label>
                    <textarea name="description" rows="3">${product?.description || ''}</textarea>
                </div>
                <div class="form-group">
                    <label>رابط الصورة (Base64 أو URL)</label>
                    <input type="text" name="imageUrl" value="${product?.images?.[0] || ''}" required>
                </div>
                <div class="form-row">
                    <div class="form-group">
                        <label>
                            <input type="checkbox" name="isActive" ${product?.isActive !== false ? 'checked' : ''}>
                            نشط
                        </label>
                    </div>
                    <div class="form-group">
                        <label>
                            <input type="checkbox" name="isFeatured" ${product?.isFeatured ? 'checked' : ''}>
                            مميز
                        </label>
                    </div>
                </div>
                <div class="form-actions">
                    <button type="button" class="btn-secondary" onclick="Admin.closeModal()">إلغاء</button>
                    <button type="submit" class="btn-submit">
                        <i class="fas fa-save"></i>
                        ${isEdit ? 'حفظ التعديلات' : 'إضافة المنتج'}
                    </button>
                </div>
            </form>
        `;

        document.getElementById('productForm').addEventListener('submit', (e) => {
            e.preventDefault();
            this.saveProduct(e.target, isEdit, product);
        });

        this.openModal();
    },

    saveProduct(form, isEdit, existingProduct) {
        const formData = new FormData(form);
        const product = {
            name: formData.get('name'),
            slug: this.slugify(formData.get('name')),
            categoryId: parseInt(formData.get('categoryId')),
            brandId: formData.get('brandId') ? parseInt(formData.get('brandId')) : null,
            price: parseFloat(formData.get('price')),
            currency: 'IQD',
            unit: formData.get('unit'),
            description: formData.get('description'),
            images: [formData.get('imageUrl')],
            isActive: formData.get('isActive') === 'on',
            isFeatured: formData.get('isFeatured') === 'on',
            sku: `PRD${Date.now()}`,
            createdAt: new Date().toISOString()
        };

        if (isEdit) {
            product.id = existingProduct.id;
            DataManager.update(DataManager.KEYS.PRODUCTS, product);
            this.showToast('تم تحديث المنتج بنجاح', 'success');
        } else {
            DataManager.add(DataManager.KEYS.PRODUCTS, product);
            this.showToast('تم إضافة المنتج بنجاح', 'success');
        }

        this.closeModal();
        this.loadPage('products');
    },

    editProduct(id) {
        this.openProductModal(id);
    },

    deleteProduct(id) {
        if (confirm('هل أنت متأكد من حذف هذا المنتج؟')) {
            DataManager.delete(DataManager.KEYS.PRODUCTS, id);
            this.showToast('تم حذف المنتج بنجاح', 'success');
            this.loadPage('products');
        }
    },

    // Category Modal
    openCategoryModal(categoryId = null) {
        const isEdit = categoryId !== null;
        const category = isEdit ? DataManager.getCategoryById(categoryId) : null;

        const modalTitle = document.getElementById('modalTitle');
        const modalBody = document.getElementById('modalBody');
        
        modalTitle.textContent = isEdit ? 'تعديل القسم' : 'إضافة قسم جديد';

        modalBody.innerHTML = `
            <form id="categoryForm" class="modal-form">
                <div class="form-group">
                    <label>اسم القسم</label>
                    <input type="text" name="name" value="${category?.name || ''}" required>
                </div>
                <div class="form-group">
                    <label>رابط الصورة (Base64 أو URL)</label>
                    <input type="text" name="image" value="${category?.image || ''}" required>
                </div>
                <div class="form-group">
                    <label>
                        <input type="checkbox" name="isActive" ${category?.isActive !== false ? 'checked' : ''}>
                        نشط
                    </label>
                </div>
                <div class="form-actions">
                    <button type="button" class="btn-secondary" onclick="Admin.closeModal()">إلغاء</button>
                    <button type="submit" class="btn-submit">
                        <i class="fas fa-save"></i>
                        ${isEdit ? 'حفظ التعديلات' : 'إضافة القسم'}
                    </button>
                </div>
            </form>
        `;

        document.getElementById('categoryForm').addEventListener('submit', (e) => {
            e.preventDefault();
            this.saveCategory(e.target, isEdit, category);
        });

        this.openModal();
    },

    saveCategory(form, isEdit, existingCategory) {
        const formData = new FormData(form);
        const category = {
            name: formData.get('name'),
            slug: this.slugify(formData.get('name')),
            image: formData.get('image'),
            isActive: formData.get('isActive') === 'on',
            displayOrder: 1
        };

        if (isEdit) {
            category.id = existingCategory.id;
            DataManager.update(DataManager.KEYS.CATEGORIES, category);
            this.showToast('تم تحديث القسم بنجاح', 'success');
        } else {
            DataManager.add(DataManager.KEYS.CATEGORIES, category);
            this.showToast('تم إضافة القسم بنجاح', 'success');
        }

        this.closeModal();
        this.loadPage('categories');
    },

    editCategory(id) {
        this.openCategoryModal(id);
    },

    deleteCategory(id) {
        if (confirm('هل أنت متأكد من حذف هذا القسم؟')) {
            DataManager.delete(DataManager.KEYS.CATEGORIES, id);
            this.showToast('تم حذف القسم بنجاح', 'success');
            this.loadPage('categories');
        }
    },

    // Brand Modal
    openBrandModal(brandId = null) {
        const isEdit = brandId !== null;
        const brand = isEdit ? DataManager.getBrandById(brandId) : null;

        const modalTitle = document.getElementById('modalTitle');
        const modalBody = document.getElementById('modalBody');
        
        modalTitle.textContent = isEdit ? 'تعديل الماركة' : 'إضافة ماركة جديدة';

        modalBody.innerHTML = `
            <form id="brandForm" class="modal-form">
                <div class="form-group">
                    <label>اسم الماركة</label>
                    <input type="text" name="name" value="${brand?.name || ''}" required>
                </div>
                <div class="form-group">
                    <label>
                        <input type="checkbox" name="isActive" ${brand?.isActive !== false ? 'checked' : ''}>
                        نشط
                    </label>
                </div>
                <div class="form-actions">
                    <button type="button" class="btn-secondary" onclick="Admin.closeModal()">إلغاء</button>
                    <button type="submit" class="btn-submit">
                        <i class="fas fa-save"></i>
                        ${isEdit ? 'حفظ التعديلات' : 'إضافة الماركة'}
                    </button>
                </div>
            </form>
        `;

        document.getElementById('brandForm').addEventListener('submit', (e) => {
            e.preventDefault();
            this.saveBrand(e.target, isEdit, brand);
        });

        this.openModal();
    },

    saveBrand(form, isEdit, existingBrand) {
        const formData = new FormData(form);
        const brand = {
            name: formData.get('name'),
            slug: this.slugify(formData.get('name')),
            isActive: formData.get('isActive') === 'on'
        };

        if (isEdit) {
            brand.id = existingBrand.id;
            DataManager.update(DataManager.KEYS.BRANDS, brand);
            this.showToast('تم تحديث الماركة بنجاح', 'success');
        } else {
            DataManager.add(DataManager.KEYS.BRANDS, brand);
            this.showToast('تم إضافة الماركة بنجاح', 'success');
        }

        this.closeModal();
        this.loadPage('brands');
    },

    editBrand(id) {
        this.openBrandModal(id);
    },

    deleteBrand(id) {
        if (confirm('هل أنت متأكد من حذف هذه الماركة؟')) {
            DataManager.delete(DataManager.KEYS.BRANDS, id);
            this.showToast('تم حذف الماركة بنجاح', 'success');
            this.loadPage('brands');
        }
    },

    // Banner Modal
    openBannerModal(bannerId = null) {
        const isEdit = bannerId !== null;
        const banners = DataManager.getAll(DataManager.KEYS.BANNERS);
        const banner = isEdit ? banners.find(b => b.id === bannerId) : null;

        const modalTitle = document.getElementById('modalTitle');
        const modalBody = document.getElementById('modalBody');
        
        modalTitle.textContent = isEdit ? 'تعديل البانر' : 'إضافة بانر جديد';

        const bannerImagesValue = Array.isArray(banner?.images) && banner.images.length > 0
            ? banner.images.join('\n')
            : (banner?.imageUrl || '');

        modalBody.innerHTML = `
            <form id="bannerForm" class="modal-form">
                <div class="form-group">
                    <label>العنوان</label>
                    <input type="text" name="title" value="${banner?.title || ''}" required>
                </div>
                <div class="form-group">
                    <label>روابط الصور (URL) — كل رابط بسطر</label>
                    <textarea name="images" rows="4" required placeholder="https://...\nhttps://...">${bannerImagesValue}</textarea>
                </div>
                <div class="form-group">
                    <label>رابط التوجيه (اختياري)</label>
                    <input type="text" name="linkUrl" value="${banner?.linkUrl || ''}">
                </div>
                <div class="form-group">
                    <label>الموضع</label>
                    <select name="placement" required>
                        <option value="HomeHero" ${banner?.placement === 'HomeHero' ? 'selected' : ''}>الصفحة الرئيسية</option>
                        <option value="Sidebar" ${banner?.placement === 'Sidebar' ? 'selected' : ''}>الشريط الجانبي</option>
                    </select>
                </div>
                <div class="form-group">
                    <label>
                        <input type="checkbox" name="isActive" ${banner?.isActive !== false ? 'checked' : ''}>
                        نشط
                    </label>
                </div>
                <div class="form-actions">
                    <button type="button" class="btn-secondary" onclick="Admin.closeModal()">إلغاء</button>
                    <button type="submit" class="btn-submit">
                        <i class="fas fa-save"></i>
                        ${isEdit ? 'حفظ التعديلات' : 'إضافة البانر'}
                    </button>
                </div>
            </form>
        `;

        document.getElementById('bannerForm').addEventListener('submit', (e) => {
            e.preventDefault();
            this.saveBanner(e.target, isEdit, banner);
        });

        this.openModal();
    },

    saveBanner(form, isEdit, existingBanner) {
        const formData = new FormData(form);

        const imagesRaw = (formData.get('images') || '').toString();
        const images = imagesRaw
            .split(/\r?\n/)
            .map(s => s.trim())
            .filter(Boolean);

        const banner = {
            title: formData.get('title'),
            images,
            imageUrl: images[0] || 'images/ui/banner-placeholder.svg',
            linkUrl: formData.get('linkUrl'),
            placement: formData.get('placement'),
            isActive: formData.get('isActive') === 'on',
            startDate: new Date().toISOString(),
            endDate: null,
            displayOrder: 1
        };

        if (isEdit) {
            banner.id = existingBanner.id;
            DataManager.update(DataManager.KEYS.BANNERS, banner);
            this.showToast('تم تحديث البانر بنجاح', 'success');
        } else {
            DataManager.add(DataManager.KEYS.BANNERS, banner);
            this.showToast('تم إضافة البانر بنجاح', 'success');
        }

        this.closeModal();
        this.loadPage('banners');
    },

    editBanner(id) {
        this.openBannerModal(id);
    },

    deleteBanner(id) {
        if (confirm('هل أنت متأكد من حذف هذا البانر؟')) {
            DataManager.delete(DataManager.KEYS.BANNERS, id);
            this.showToast('تم حذف البانر بنجاح', 'success');
            this.loadPage('banners');
        }
    },

    // Modal Controls
    openModal() {
        document.getElementById('modal').classList.add('show');
    },

    closeModal() {
        document.getElementById('modal').classList.remove('show');
    },

    // Utilities
    slugify(text) {
        return text
            .toLowerCase()
            .replace(/[^\w\s-]/g, '')
            .replace(/[\s_-]+/g, '-')
            .replace(/^-+|-+$/g, '');
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
    }
};

// Initialize Admin Panel
document.addEventListener('DOMContentLoaded', () => {
    Admin.init();
});
