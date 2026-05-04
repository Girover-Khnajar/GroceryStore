/**
 * Gallery – client-side companion for Admin/Gallery.cshtml
 * Mirrors the Blazor Gallery.razor behaviour.
 */
window.Gallery = (function () {
    'use strict';

    // ── State ────────────────────────────────────────────────────────
    var S = {
        viewMode:   'library',   // 'library' | 'entity'
        libraryImages: [],
        entityImages:  [],
        categories:    [],
        products:      [],
        search:        '',
        selected:      new Set(),
        target:        'category', // 'category' | 'product'
        entityId:      '',         // guid string, '' = none
        assignSetFirstPrimary: true,
        previews:      [],         // [{name, dataUrl, file}]
        uploading:     false,
        uploadedCount: 0,
        lightboxIndex: -1,
        draggingId:    null,
        dragTargetId:  null,
        busyAssign:    false,
        busyGenerateThumbs: false
    };

    // ── DOM cache ────────────────────────────────────────────────────
    var $grid, $empty, $bulkBar, $bulkCount, $searchInput, $clearSearchBtn,
        $selectAllCheck, $previewArea, $previewStrip, $queuedCount,
        $uploadProgress, $uploadProgressText, $uploadProgressBar,
        $uploadBtn, $clearBtn, $lightbox, $lbImage, $lbCaption, $lbCounter,
        $btnLibrary, $btnEntity, $entitySelect, $entityLabel, $entityInfo,
        $targetSelect, $deselectBtn, $assignBtn, $unassignBtn,
        $assignPrimaryCheck, $dropZone, $fileInput, $loading, $thumbStatsBadge;

    // ── Helpers ──────────────────────────────────────────────────────
    function esc(s) {
        var d = document.createElement('div');
        d.textContent = s || '';
        return d.innerHTML;
    }

    function formatSize(bytes) {
        if (bytes >= 1048576) return (bytes / 1048576).toFixed(1) + ' MB';
        if (bytes >= 1024)    return (bytes / 1024).toFixed(0)    + ' KB';
        return bytes + ' B';
    }

    function formatDate(iso) {
        if (!iso) return '';
        var d = new Date(iso);
        return d.getFullYear() + '-'
             + String(d.getMonth() + 1).padStart(2, '0') + '-'
             + String(d.getDate()).padStart(2, '0');
    }

    function toast(msg, type) {
        if (typeof showToast === 'function') { showToast(msg, type || 'success'); return; }
        alert(msg);
    }

    // ── Computed ─────────────────────────────────────────────────────
    function currentImages() {
        return S.viewMode === 'library' ? S.libraryImages : S.entityImages;
    }

    function filtered() {
        var imgs = currentImages();
        if (!S.search) return imgs;
        var s = S.search.toLowerCase();
        return imgs.filter(function (i) {
            return (i.originalFileName || '').toLowerCase().indexOf(s) >= 0;
        });
    }

    // ── Initialisation ──────────────────────────────────────────────
    function init(data) {
        S.libraryImages = (data.images || []).filter(function (i) { return !i.isDeleted; });
        S.categories    = data.categories || [];
        S.products      = data.products   || [];

        $grid               = document.getElementById('galleryGrid');
        $empty              = document.getElementById('emptyState');
        $bulkBar            = document.getElementById('bulkBar');
        $bulkCount          = document.getElementById('bulkCount');
        $searchInput        = document.getElementById('searchInput');
        $clearSearchBtn     = document.getElementById('clearSearchBtn');
        $selectAllCheck     = document.getElementById('selectAllCheck');
        $previewArea        = document.getElementById('previewArea');
        $previewStrip       = document.getElementById('previewStrip');
        $queuedCount        = document.getElementById('queuedCount');
        $uploadProgress     = document.getElementById('uploadProgress');
        $uploadProgressText = document.getElementById('uploadProgressText');
        $uploadProgressBar  = document.getElementById('uploadProgressBar');
        $uploadBtn          = document.getElementById('uploadBtn');
        $clearBtn           = document.getElementById('clearBtn');
        $lightbox           = document.getElementById('lightbox');
        $lbImage            = document.getElementById('lbImage');
        $lbCaption          = document.getElementById('lbCaption');
        $lbCounter          = document.getElementById('lbCounter');
        $btnLibrary         = document.getElementById('btnLibrary');
        $btnEntity          = document.getElementById('btnEntity');
        $entitySelect       = document.getElementById('entitySelect');
        $entityLabel        = document.getElementById('entityLabel');
        $entityInfo         = document.getElementById('entityInfo');
        $targetSelect       = document.getElementById('targetSelect');
        $deselectBtn        = document.getElementById('deselectBtn');
        $assignBtn          = document.getElementById('assignBtn');
        $unassignBtn        = document.getElementById('unassignBtn');
        $assignPrimaryCheck = document.getElementById('assignPrimaryCheck');
        $dropZone           = document.getElementById('dropZone');
        $fileInput          = document.getElementById('fileInput');
        $loading            = document.getElementById('galleryLoading');
        $thumbStatsBadge    = document.getElementById('thumbStatsBadge');

        bindEvents();
        populateEntitySelect();
        renderGrid();
        updateBulkBar();
        loadThumbnailStats();
    }

    function bindEvents() {
        // Drop zone drag-over / leave / drop
        $dropZone.addEventListener('dragover',  function (e) { e.preventDefault(); $dropZone.classList.add('drag-over'); });
        $dropZone.addEventListener('dragleave', function ()  { $dropZone.classList.remove('drag-over'); });
        $dropZone.addEventListener('drop', function (e) {
            e.preventDefault();
            $dropZone.classList.remove('drag-over');
            handleFiles(e.dataTransfer.files);
        });

        // File input
        $fileInput.addEventListener('change', function () {
            handleFiles(this.files);
            this.value = '';
        });

        // Keyboard (lightbox)
        document.addEventListener('keydown', function (e) {
            if (S.lightboxIndex >= 0) {
                if (e.key === 'Escape')     closeLightbox();
                if (e.key === 'ArrowLeft')  prevLightbox();
                if (e.key === 'ArrowRight') nextLightbox();
            }
        });

        // Grid drag-and-drop (delegated)
        $grid.addEventListener('dragstart', function (e) {
            var card = e.target.closest('.gallery-card');
            if (card && S.viewMode === 'entity') {
                S.draggingId = card.dataset.id;
                card.classList.add('dragging');
            }
        });
        $grid.addEventListener('dragover', function (e) {
            e.preventDefault();
            var card = e.target.closest('.gallery-card');
            if (card && S.viewMode === 'entity') {
                S.dragTargetId = card.dataset.id;
                $grid.querySelectorAll('.gallery-card').forEach(function (c) { c.classList.remove('drag-target'); });
                if (S.draggingId !== card.dataset.id) card.classList.add('drag-target');
            }
        });
        $grid.addEventListener('drop', function (e) {
            e.preventDefault();
            var card = e.target.closest('.gallery-card');
            if (card && S.viewMode === 'entity') doDragDrop(card.dataset.id);
        });
        $grid.addEventListener('dragend', function () {
            S.draggingId = null;
            S.dragTargetId = null;
            $grid.querySelectorAll('.gallery-card').forEach(function (c) {
                c.classList.remove('dragging', 'drag-target');
            });
        });
    }

    // ── Rendering ────────────────────────────────────────────────────
    function renderGrid() {
        var items = filtered();
        if ($loading) $loading.style.display = 'none';

        if (items.length === 0) {
            $grid.style.display = 'none';
            $empty.style.display = '';
            return;
        }
        $grid.style.display = '';
        $empty.style.display = 'none';

        var isEntity = S.viewMode === 'entity';

        $grid.innerHTML = items.map(function (img, idx) {
            var sel       = S.selected.has(img.imageId);
            var isPrimary = !!img.isPrimary;
            var name      = esc(img.originalFileName || '');
            var showPri   = isEntity && isPrimary;
            var showDrag  = isEntity;
            var showSetP  = isEntity && S.target === 'product' && S.entityId && !isPrimary;

            return '<div class="gallery-card' + (sel ? ' selected' : '') + (showPri ? ' is-primary' : '') + '"'
                 + ' data-id="' + img.imageId + '"'
                 + (showDrag ? ' draggable="true"' : '') + '>'
                 + (showPri  ? '<span class="primary-badge"><i class="fas fa-star" style="margin-right:.2rem;"></i>Primary</span>' : '')
                 + (showDrag ? '<i class="fas fa-grip-vertical drag-handle" title="Drag to reorder"></i>' : '')
                 + '<input class="select-check" type="checkbox"' + (sel ? ' checked' : '')
                 + ' onclick="event.stopPropagation();Gallery.toggleSelect(\'' + img.imageId + '\')" />'
                 + '<div class="gallery-img-wrap" onclick="Gallery.openLightbox(' + idx + ')">'
                 + '<img src="' + esc(img.url) + '" alt="' + name + '" loading="lazy" />'
                 + '</div>'
                 + '<div class="gallery-meta">'
                 + '<div class="gallery-filename" title="' + name + '">' + name + '</div>'
                 + '<div class="gallery-info">'
                 + '<span>' + formatDate(img.createdOnUtc) + '</span>'
                 + (img.contentType ? '<span class="info-badge">' + img.contentType.replace('image/', '').toUpperCase() + '</span>' : '')
                 + (img.fileSizeBytes > 0 ? '<span class="info-badge">' + formatSize(img.fileSizeBytes) + '</span>' : '')
                 + '</div>'
                 + '<div class="gallery-actions">'
                 + (showSetP ? '<button class="btn btn-secondary btn-sm" title="Set primary (entity)" onclick="Gallery.setPrimary(\'' + img.imageId + '\')"><i class="fas fa-star"></i></button>' : '')
                 + '<button class="btn btn-secondary btn-sm" title="Copy URL" onclick="Gallery.copyUrl(\'' + esc(img.url) + '\')"><i class="fas fa-link"></i></button>'
                 + '<button class="btn btn-danger btn-sm" title="Delete from library" onclick="Gallery.deleteImage(\'' + img.imageId + '\')"><i class="fas fa-trash"></i></button>'
                 + '</div></div></div>';
        }).join('');
    }

    function updateBulkBar() {
        var count = S.selected.size;

        // Bulk bar visibility
        if (count > 0) {
            $bulkBar.style.display = '';
            $bulkCount.textContent = count + ' selected';
        } else {
            $bulkBar.style.display = 'none';
        }

        // Select-all checkbox
        var items = filtered();
        $selectAllCheck.checked = items.length > 0 && count === items.length;

        // Deselect button
        $deselectBtn.disabled = count === 0;

        // Assign / Unassign
        var canAssign = count > 0 && S.entityId && !S.busyAssign;
        $assignBtn.disabled   = !canAssign;
        $unassignBtn.disabled = !canAssign;
    }

    function updateEntityInfo() {
        if (S.viewMode === 'entity') {
            $entityInfo.style.display = '';
            $entityInfo.textContent = S.entityId
                ? 'Viewing: ' + S.target.charAt(0).toUpperCase() + S.target.slice(1) + ' · Entity images: ' + S.entityImages.length
                : 'No entity selected';
        } else {
            $entityInfo.style.display = 'none';
        }
    }

    function populateEntitySelect() {
        var items = S.target === 'category' ? S.categories : S.products;
        var label = S.target === 'category' ? 'Category' : 'Product';
        $entityLabel.textContent = label;
        $entitySelect.innerHTML = '<option value="">Select ' + label.toLowerCase() + '…</option>'
            + items.map(function (it) {
                return '<option value="' + it.id + '">' + esc(it.name) + '</option>';
            }).join('');
        S.entityId = '';
        updateEntityInfo();
    }

    // ── View-mode switching ──────────────────────────────────────────
    function switchView(mode) {
        S.viewMode = mode;
        S.selected.clear();
        S.search = '';
        $searchInput.value = '';
        $clearSearchBtn.style.display = 'none';

        if (mode === 'library') {
            $btnLibrary.className = 'btn btn-primary btn-sm';
            $btnEntity.className  = 'btn btn-secondary btn-sm';
        } else {
            $btnLibrary.className = 'btn btn-secondary btn-sm';
            $btnEntity.className  = 'btn btn-primary btn-sm';
            reloadEntityImages();
        }

        renderGrid();
        updateBulkBar();
        updateEntityInfo();
    }

    // ── Search ───────────────────────────────────────────────────────
    function onSearch(val) {
        S.search = val;
        $clearSearchBtn.style.display = val ? '' : 'none';
        renderGrid();
        updateBulkBar();
    }

    function clearSearch() {
        S.search = '';
        $searchInput.value = '';
        $clearSearchBtn.style.display = 'none';
        renderGrid();
        updateBulkBar();
    }

    // ── Selection ────────────────────────────────────────────────────
    function toggleSelect(id) {
        if (S.selected.has(id)) S.selected.delete(id);
        else S.selected.add(id);
        renderGrid();
        updateBulkBar();
    }

    function toggleSelectAll(checked) {
        if (checked) {
            filtered().forEach(function (img) { S.selected.add(img.imageId); });
        } else {
            S.selected.clear();
        }
        renderGrid();
        updateBulkBar();
    }

    function deselectAll() {
        S.selected.clear();
        renderGrid();
        updateBulkBar();
    }

    // ── File picking / preview ───────────────────────────────────────
    function handleFiles(fileList) {
        Array.from(fileList).forEach(function (file) {
            if (S.previews.some(function (p) { return p.name === file.name; })) return;
            if (!file.type.startsWith('image/')) return;
            var reader = new FileReader();
            reader.onload = function (e) {
                S.previews.push({ name: file.name, dataUrl: e.target.result, file: file });
                renderPreviews();
            };
            reader.readAsDataURL(file);
        });
    }

    function renderPreviews() {
        if (S.previews.length === 0) {
            $previewArea.style.display = 'none';
            return;
        }
        $previewArea.style.display = '';
        $queuedCount.textContent = S.previews.length + ' file' + (S.previews.length === 1 ? '' : 's') + ' queued';
        $previewStrip.innerHTML = S.previews.map(function (pv, idx) {
            return '<div class="preview-thumb">'
                 + '<img src="' + pv.dataUrl + '" alt="' + esc(pv.name) + '" />'
                 + '<button class="rm" onclick="Gallery.removePreview(' + idx + ')"' + (S.uploading ? ' disabled' : '') + '>'
                 + '<i class="fas fa-times"></i></button></div>';
        }).join('');
    }

    function removePreview(idx) {
        S.previews.splice(idx, 1);
        renderPreviews();
    }

    function clearPreviews() {
        S.previews = [];
        renderPreviews();
    }

    // ── Upload ───────────────────────────────────────────────────────
    async function upload() {
        if (S.previews.length === 0 || S.uploading) return;
        S.uploading     = true;
        S.uploadedCount = 0;
        $uploadProgress.style.display = '';
        $uploadBtn.innerHTML  = '<i class="fas fa-spinner fa-spin"></i> <span>Uploading\u2026</span>';
        $uploadBtn.disabled   = true;
        $clearBtn.disabled    = true;

        var total = S.previews.length;
        for (var i = 0; i < S.previews.length; i++) {
            var pv = S.previews[i];
            var fd = new FormData();
            fd.append('file', pv.file);
            try {
                var resp = await fetch('/Admin/Gallery/Upload', { method: 'POST', body: fd });
                if (!resp.ok) {
                    var err = {};
                    try { err = await resp.json(); } catch(_){}
                    toast(err.error || 'Upload failed', 'error');
                }
            } catch (ex) {
                toast('Upload error: ' + ex.message, 'error');
            }
            S.uploadedCount++;
            $uploadProgressText.textContent  = 'Uploading ' + S.uploadedCount + ' of ' + total + '\u2026';
            $uploadProgressBar.style.width   = (S.uploadedCount * 100 / total) + '%';
        }

        S.uploading = false;
        S.previews  = [];
        renderPreviews();
        $uploadProgress.style.display = 'none';
        $uploadBtn.innerHTML = '<i class="fas fa-upload"></i> <span>Upload to library</span>';
        $uploadBtn.disabled  = false;
        $clearBtn.disabled   = false;
        toast('Uploaded to library.');
        await reloadLibrary();
    }

    // ── Library reload ───────────────────────────────────────────────
    async function reloadLibrary() {
        try {
            var resp = await fetch('/Admin/Gallery/Images');
            if (resp.ok) {
                var data = await resp.json();
                S.libraryImages = (data || []).filter(function (i) { return !i.isDeleted; });
                if (S.viewMode === 'library') renderGrid();
                updateBulkBar();
            }
        } catch (_) {
            toast('Failed to reload images', 'error');
        }
    }

    // ── Target / entity changes ──────────────────────────────────────
    function onTargetChange(val) {
        S.target = val;
        S.entityId = '';
        S.entityImages = [];
        populateEntitySelect();
        if (S.viewMode === 'entity') {
            renderGrid();
            updateBulkBar();
        }
    }

    function onEntityChange(val) {
        S.entityId = val;
        if (S.viewMode === 'entity') reloadEntityImages();
        updateBulkBar();
    }

    async function reloadEntityImages() {
        if (S.viewMode !== 'entity') return;
        if (!S.entityId) {
            S.entityImages = [];
            renderGrid();
            updateBulkBar();
            updateEntityInfo();
            return;
        }
        // Entity image listing (not yet supported without dedicated endpoints)
        S.entityImages = [];
        renderGrid();
        updateBulkBar();
        updateEntityInfo();
    }

    // ── Delete ───────────────────────────────────────────────────────
    async function deleteImage(id) {
        if (!confirm('Delete this image?')) return;
        try {
            var resp = await fetch('/Admin/Gallery/Delete/' + id, { method: 'POST' });
            if (resp.ok) {
                toast('Deleted.');
                S.selected.delete(id);
                if (S.lightboxIndex >= 0) {
                    var items = filtered();
                    if (!items.some(function (i) { return i.imageId === id; }))
                        closeLightbox();
                }
                await reloadLibrary();
                if (S.viewMode === 'entity') await reloadEntityImages();
            } else {
                toast('Delete failed.', 'error');
            }
        } catch (ex) {
            toast('Delete error: ' + ex.message, 'error');
        }
    }

    async function bulkDelete() {
        var ids = Array.from(S.selected);
        if (ids.length === 0) return;
        if (!confirm('Delete ' + ids.length + ' selected image(s)?')) return;
        try {
            var resp = await fetch('/Admin/Gallery/BulkDelete', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(ids)
            });
            if (resp.ok) {
                toast('Deleted selected images from library.');
                S.selected.clear();
                await reloadLibrary();
                if (S.viewMode === 'entity') await reloadEntityImages();
            }
        } catch (ex) {
            toast('Bulk delete error: ' + ex.message, 'error');
        }
    }

    async function generateMissingThumbnails() {
        if (S.busyGenerateThumbs) return;

        if (!confirm('Generate thumbnails for original images that do not have thumbnails yet?'))
            return;

        var btn = document.getElementById('generateThumbsBtn');
        var previousHtml = btn ? btn.innerHTML : '';

        S.busyGenerateThumbs = true;
        if (btn) {
            btn.disabled = true;
            btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Generating...';
        }

        try {
            var resp = await fetch('/Admin/Gallery/GenerateMissingThumbnails', {
                method: 'POST'
            });

            var data = await resp.json();
            if (!resp.ok) {
                toast(data.error || 'Thumbnail generation failed.', 'error');
                return;
            }

            toast(
                'Done. Generated: ' + (data.generated || 0)
                + ', Existing: ' + (data.skippedExisting || 0)
                + ', Unsupported: ' + (data.skippedUnsupported || 0)
                + ', Failed: ' + (data.failed || 0),
                'success'
            );

            await reloadLibrary();
            await loadThumbnailStats();
            if (S.viewMode === 'entity') await reloadEntityImages();
        } catch (ex) {
            toast('Generate thumbnails error: ' + ex.message, 'error');
        } finally {
            S.busyGenerateThumbs = false;
            if (btn) {
                btn.disabled = false;
                btn.innerHTML = previousHtml;
            }
        }
    }

    async function loadThumbnailStats() {
        if (!$thumbStatsBadge) return;

        try {
            var resp = await fetch('/Admin/Gallery/ThumbnailStats');
            if (!resp.ok) {
                $thumbStatsBadge.style.display = 'none';
                return;
            }

            var data = await resp.json();
            var missing = data.missing || 0;

            if (missing > 0) {
                $thumbStatsBadge.style.display = '';
                $thumbStatsBadge.textContent = 'Missing: ' + missing;
                $thumbStatsBadge.style.background = '#fee2e2';
                $thumbStatsBadge.style.color = '#b91c1c';
            } else {
                $thumbStatsBadge.style.display = '';
                $thumbStatsBadge.textContent = 'All thumbnails ready';
                $thumbStatsBadge.style.background = '#dcfce7';
                $thumbStatsBadge.style.color = '#166534';
            }
        } catch (_) {
            $thumbStatsBadge.style.display = 'none';
        }
    }

    // ── Assign / Unassign ────────────────────────────────────────────
    async function assignSelected() {
        if (S.selected.size === 0 || !S.entityId || S.busyAssign) return;
        S.busyAssign = true;
        updateBulkBar();
        try {
            var res = await fetch('/Admin/Gallery/Assign', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    target:     S.target,
                    entityId:   S.entityId,
                    imageIds:   Array.from(S.selected),
                    makePrimary: S.assignSetFirstPrimary
                })
            });
            var data = await res.json();
            if (res.ok) {
                if (S.target === 'category') {
                    toast('Assigned image to category.');
                } else {
                    toast('Assigned ' + data.assigned + ' image(s) to product.');
                }
                S.selected.clear();
                if (S.viewMode === 'entity') await reloadEntityImages();
            } else {
                toast('Assign failed: ' + (data.error || res.statusText), 'error');
            }
        } catch (ex) {
            toast('Assign error: ' + ex.message, 'error');
        }
        S.busyAssign = false;
        updateBulkBar();
    }

    async function unassignSelected() {
        if (S.selected.size === 0 || !S.entityId || S.busyAssign) return;
        S.busyAssign = true;
        updateBulkBar();
        try {
            var res = await fetch('/Admin/Gallery/Unassign', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    target:   S.target,
                    entityId: S.entityId,
                    imageIds: Array.from(S.selected)
                })
            });
            var data = await res.json();
            if (res.ok) {
                if (S.target === 'category') {
                    toast('Removed image from category.');
                } else {
                    toast('Removed ' + data.removed + ' image(s) from product.');
                }
                S.selected.clear();
                if (S.viewMode === 'entity') await reloadEntityImages();
            } else {
                toast('Unassign failed: ' + (data.error || res.statusText), 'error');
            }
        } catch (ex) {
            toast('Unassign error: ' + ex.message, 'error');
        }
        S.busyAssign = false;
        updateBulkBar();
    }

    async function setPrimary(imageId) {
        if (!S.entityId) return;
        try {
            var res = await fetch('/Admin/Gallery/SetPrimary', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    target:   S.target,
                    entityId: S.entityId,
                    imageId:  imageId
                })
            });
            var data = await res.json();
            if (res.ok) {
                if (S.target === 'category') {
                    toast('Category image updated.');
                } else {
                    toast('Primary image updated.');
                }
                if (S.viewMode === 'entity') await reloadEntityImages();
            } else {
                toast('Set primary failed: ' + (data.error || res.statusText), 'error');
            }
        } catch (ex) {
            toast('Set primary error: ' + ex.message, 'error');
        }
    }

    function onAssignPrimaryChange(checked) {
        S.assignSetFirstPrimary = checked;
    }

    // ── Drag reorder (entity view) ───────────────────────────────────
    async function doDragDrop(targetId) {
        if (!S.draggingId || S.draggingId === targetId) {
            S.draggingId = null;
            S.dragTargetId = null;
            return;
        }
        toast('Reorder is not yet implemented in this build.', 'info');
        S.draggingId   = null;
        S.dragTargetId = null;
    }

    // ── Copy URL ─────────────────────────────────────────────────────
    function copyUrl(url) {
        navigator.clipboard.writeText(url).then(function () {
            toast('Copied URL.');
        }).catch(function () {
            toast('Copy manually: ' + url, 'info');
        });
    }

    // ── Lightbox ─────────────────────────────────────────────────────
    function openLightbox(idx) {
        var items = filtered();
        if (idx < 0 || idx >= items.length) return;
        S.lightboxIndex = idx;
        renderLightbox();
    }

    function closeLightbox() {
        S.lightboxIndex = -1;
        $lightbox.style.display = 'none';
    }

    function prevLightbox() {
        var items = filtered();
        if (items.length === 0) return;
        S.lightboxIndex = (S.lightboxIndex - 1 + items.length) % items.length;
        renderLightbox();
    }

    function nextLightbox() {
        var items = filtered();
        if (items.length === 0) return;
        S.lightboxIndex = (S.lightboxIndex + 1) % items.length;
        renderLightbox();
    }

    function renderLightbox() {
        if (S.lightboxIndex < 0) return;
        var items = filtered();
        if (S.lightboxIndex >= items.length) { closeLightbox(); return; }
        var img = items[S.lightboxIndex];
        $lbImage.src          = img.url;
        $lbImage.alt          = img.originalFileName || '';
        $lbCaption.textContent = img.originalFileName || '';
        $lbCounter.textContent = (S.lightboxIndex + 1) + ' / ' + items.length;
        $lightbox.style.display = 'flex';
    }

    // ── Public API ───────────────────────────────────────────────────
    return {
        init:                init,
        switchView:          switchView,
        onSearch:            onSearch,
        clearSearch:         clearSearch,
        toggleSelect:        toggleSelect,
        toggleSelectAll:     toggleSelectAll,
        deselectAll:         deselectAll,
        upload:              upload,
        clearPreviews:       clearPreviews,
        removePreview:       removePreview,
        deleteImage:         deleteImage,
        bulkDelete:          bulkDelete,
        openLightbox:        openLightbox,
        closeLightbox:       closeLightbox,
        prevLightbox:        prevLightbox,
        nextLightbox:        nextLightbox,
        copyUrl:             copyUrl,
        onTargetChange:      onTargetChange,
        onEntityChange:      onEntityChange,
        assignSelected:      assignSelected,
        unassignSelected:    unassignSelected,
        setPrimary:          setPrimary,
        onAssignPrimaryChange: onAssignPrimaryChange,
        generateMissingThumbnails: generateMissingThumbnails
    };
})();

// Auto-init when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    if (window.__galleryData) Gallery.init(window.__galleryData);
});
