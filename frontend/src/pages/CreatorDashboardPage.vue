<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRouter, RouterLink } from 'vue-router'
import { useToastStore } from '@/stores/toast.store'
import { usePhotosStore } from '@/stores/photos.store'
import PhotoUploadForm from '@/components/forms/PhotoUploadForm.vue'

const { t } = useI18n()
const router = useRouter()
const toastStore = useToastStore()
const photosStore = usePhotosStore()

const showModal = ref(false)

function handleUploadSuccess() {
  showModal.value = false
  fetchMyPhotos()
}

// My photos from API
interface MaterialCard {
  id: number
  title: string
  date: string
  thumbnailUrl: string
}

const materials = ref<MaterialCard[]>([])
const stats = reactive({ all: 0 })

async function fetchMyPhotos() {
  try {
    await photosStore.fetchPhotos({ pageSize: 20 })
    materials.value = photosStore.items.map(p => ({
      id: p.id,
      title: p.title,
      date: p.date?.substring(0, 4) ?? '',
      thumbnailUrl: p.thumbnailUrl,
    }))
    stats.all = photosStore.pagination.totalCount
  } catch { /* ignore */ }
}

const sidebarLinks = [
  { icon: 'home', label: t('adminDashboard.sideHome'), active: false, route: '/przegladaj' },
  { icon: 'account_balance', label: t('adminDashboard.sideArchive'), active: true, route: '/panel-tworcy' },
  { icon: 'upload_file', label: t('adminDashboard.sideUploads'), active: false, route: '/moje-zdjecia' },
  { icon: 'gavel', label: t('adminDashboard.sideModeration'), active: false, route: '/admin' },
  { icon: 'history', label: t('adminDashboard.sideAuditLog'), active: false, route: '/panel-tworcy' },
]

async function deleteMaterial(id: number) {
  try {
    await photosStore.deletePhoto(id)
    materials.value = materials.value.filter(m => m.id !== id)
    stats.all--
    toastStore.show('Materiał został usunięty', 'success')
  } catch {
    toastStore.show('Nie udało się usunąć materiału', 'error')
  }
}

onMounted(() => {
  fetchMyPhotos()
})
</script>

<template>
  <div class="creator">
    <!-- ============================
         SIDEBAR (Desktop only)
         ============================ -->
    <aside class="creator__sidebar">
      <div class="creator__sidebar-profile">
        <div class="creator__sidebar-avatar">
          <span class="material-symbols-outlined">shield_person</span>
        </div>
        <div>
          <h2 class="creator__sidebar-name">{{ t('adminDashboard.archiveManager') }}</h2>
          <p class="creator__sidebar-role">{{ t('adminDashboard.seniorCurator') }}</p>
        </div>
      </div>

      <nav class="creator__sidebar-nav">
        <RouterLink
          v-for="link in sidebarLinks"
          :key="link.label"
          :to="link.route"
          class="creator__sidebar-link"
          :class="{ 'creator__sidebar-link--active': link.active }"
        >
          <span
            class="material-symbols-outlined"
            :style="link.active ? `font-variation-settings: 'FILL' 1` : ''"
          >{{ link.icon }}</span>
          <span>{{ link.label }}</span>
        </RouterLink>
      </nav>

      <button class="creator__sidebar-cta" @click="router.push({ name: 'upload-photo' })">
        <span class="material-symbols-outlined">add</span>
        {{ t('adminDashboard.newCollection') }}
      </button>

      <div class="creator__sidebar-footer">
        <a href="#" class="creator__sidebar-link" @click.prevent>
          <span class="material-symbols-outlined">help</span>
          <span>{{ t('adminDashboard.helpCenter') }}</span>
        </a>
      </div>
    </aside>

    <!-- ============================
         MAIN CONTENT
         ============================ -->
    <main class="creator__main">
      <!-- Dashboard Canvas -->
      <div class="creator__canvas">
        <!-- Header -->
        <div class="creator__header">
          <div>
            <span class="creator__badge">{{ t('creatorDashboard.badge') }}</span>
            <h1 class="creator__title">{{ t('creatorDashboard.title') }}</h1>
            <p class="creator__description">{{ t('creatorDashboard.description') }}</p>
          </div>
          <button class="creator__add-btn" @click="showModal = true">
            <span class="material-symbols-outlined">add</span>
            {{ t('creatorDashboard.addPhoto') }}
          </button>
        </div>

        <!-- Stats -->
        <div class="creator__stats">
          <div class="creator__stat">
            <span class="creator__stat-label">{{ t('creatorDashboard.statsAll') }}</span>
            <h3 class="creator__stat-value">{{ stats.all }}</h3>
          </div>
        </div>

        <!-- Materials Grid -->
        <div class="creator__grid">
          <div
            v-for="card in materials"
            :key="card.id"
            class="creator__card"
          >
            <div class="creator__card-thumb">
              <img :src="card.thumbnailUrl" :alt="card.title" />
            </div>
            <div class="creator__card-body">
              <h4 class="creator__card-title">{{ card.title }}</h4>
              <p class="creator__card-date">{{ card.date }}</p>
              <div class="creator__card-actions">
                <RouterLink
                  :to="{ name: 'photo-detail', params: { id: card.id } }"
                  class="creator__card-action creator__card-action--primary"
                >
                  <span class="material-symbols-outlined">visibility</span>
                  Podgląd
                </RouterLink>
                <button class="creator__card-action creator__card-action--error" @click="deleteMaterial(card.id)">
                  <span class="material-symbols-outlined">delete</span>
                  {{ t('common.delete') }}
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </main>

    <!-- ============================
         UPLOAD MODAL
         ============================ -->
    <div v-if="showModal" class="creator__overlay" @click.self="showModal = false">
      <div class="creator__modal">
        <button class="creator__modal-close" @click="showModal = false" aria-label="Zamknij">
          <span class="material-symbols-outlined">close</span>
        </button>
        <div class="creator__modal-inner">
          <h2 class="creator__modal-title">{{ t('creatorDashboard.modalTitle') }}</h2>
          <PhotoUploadForm @success="handleUploadSuccess" @cancel="showModal = false" />
        </div>
      </div>
    </div>

    <!-- ============================
         MOBILE BOTTOM NAV
         ============================ -->
    <nav class="creator__mobile-nav" aria-label="Nawigacja mobilna">
      <RouterLink to="/przegladaj" class="creator__mobile-nav-item">
        <span class="material-symbols-outlined">search</span>
        <span class="creator__mobile-nav-label">Przeglądaj</span>
      </RouterLink>
      <RouterLink to="/mapa" class="creator__mobile-nav-item">
        <span class="material-symbols-outlined">map</span>
        <span class="creator__mobile-nav-label">Mapa</span>
      </RouterLink>
      <RouterLink to="/dodaj-zdjecie" class="creator__mobile-nav-item">
        <span class="material-symbols-outlined">add_a_photo</span>
        <span class="creator__mobile-nav-label">Dodaj</span>
      </RouterLink>
      <RouterLink to="/moje-zdjecia" class="creator__mobile-nav-item">
        <span class="material-symbols-outlined">person</span>
        <span class="creator__mobile-nav-label">Profil</span>
      </RouterLink>
    </nav>
  </div>
</template>

<style scoped>
/* ==========================================================================
   CREATOR DASHBOARD — Panel Tworcy
   ========================================================================== */

.creator {
  display: flex;
  min-height: calc(100vh - 56px);
  background: var(--surface);
}

/* ==========================================================================
   SIDEBAR
   ========================================================================== */

.creator__sidebar {
  display: none;
  flex-direction: column;
  width: 256px;
  height: calc(100vh - 56px);
  position: fixed;
  left: 0;
  top: 56px;
  padding: 16px;
  gap: 24px;
  background: var(--surface-variant);
  border-right: 1px solid var(--ghost-border);
  z-index: 50;
  overflow-y: auto;
}

@media (min-width: 768px) {
  .creator__sidebar {
    display: flex;
  }
}

.creator__sidebar-profile {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 0 8px;
}

.creator__sidebar-avatar {
  width: 40px;
  height: 40px;
  border-radius: var(--radius-full);
  background: var(--primary);
  color: var(--on-primary);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.creator__sidebar-name {
  font-family: var(--font-headline);
  font-size: var(--body-lg);
  font-weight: 700;
  color: var(--on-surface);
  margin: 0;
}

.creator__sidebar-role {
  font-family: var(--font-label);
  font-size: 10px;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  color: var(--on-surface-variant);
  margin: 0;
}

.creator__sidebar-cta {
  width: 100%;
  padding: 12px 16px;
  background: var(--secondary);
  color: var(--on-secondary);
  border: none;
  border-radius: var(--radius-lg);
  font-family: var(--font-label);
  font-weight: 600;
  font-size: var(--body-sm);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  transition: opacity var(--transition-fast);
}

.creator__sidebar-cta:hover {
  opacity: 0.9;
}

.creator__sidebar-nav {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.creator__sidebar-link {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 16px;
  border-radius: var(--radius-lg);
  text-decoration: none;
  font-family: var(--font-body);
  font-size: var(--body-sm);
  font-weight: 500;
  letter-spacing: 0.02em;
  color: var(--on-surface);
  opacity: 0.8;
  transition: background var(--transition-fast), opacity var(--transition-fast);
}

.creator__sidebar-link:hover {
  background: var(--surface-container-low);
  opacity: 1;
}

.creator__sidebar-link--active {
  background: var(--surface);
  color: var(--primary);
  font-weight: 700;
  opacity: 1;
  box-shadow: var(--shadow-sm);
  border-top-right-radius: 0;
  border-bottom-right-radius: 0;
}

.creator__sidebar-footer {
  padding-top: 24px;
  border-top: 1px solid rgba(190, 200, 201, 0.3);
}

/* ==========================================================================
   MAIN
   ========================================================================== */

.creator__main {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

@media (min-width: 768px) {
  .creator__main {
    margin-left: 256px;
  }
}

/* ==========================================================================
   CANVAS
   ========================================================================== */

.creator__canvas {
  padding: 32px;
  max-width: var(--container-max);
  margin: 0 auto;
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 40px;
}

@media (max-width: 768px) {
  .creator__canvas {
    padding: 16px;
    gap: 24px;
  }
}

/* ==========================================================================
   HEADER
   ========================================================================== */

.creator__header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 24px;
  flex-wrap: wrap;
}

.creator__badge {
  display: inline-block;
  padding: 4px 12px;
  background: var(--primary);
  color: var(--on-primary);
  border-radius: var(--radius-full);
  font-family: var(--font-label);
  font-size: var(--label-sm);
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin-bottom: 12px;
}

.creator__title {
  font-family: var(--font-headline);
  font-size: clamp(1.5rem, 3vw, 2rem);
  font-weight: 700;
  color: var(--on-surface);
  margin: 0 0 8px;
}

.creator__description {
  font-family: var(--font-body);
  font-size: var(--body-md);
  color: var(--on-surface-variant);
  margin: 0;
  max-width: 560px;
}

.creator__add-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 24px;
  background: var(--primary);
  color: var(--on-primary);
  border: none;
  border-radius: var(--radius-lg);
  font-family: var(--font-label);
  font-weight: 700;
  font-size: var(--body-sm);
  cursor: pointer;
  transition: opacity var(--transition-fast);
  white-space: nowrap;
  flex-shrink: 0;
}

.creator__add-btn:hover {
  opacity: 0.9;
}

/* ==========================================================================
   STATS
   ========================================================================== */

.creator__stats {
  display: grid;
  grid-template-columns: 1fr;
  gap: 16px;
}

@media (min-width: 640px) {
  .creator__stats {
    grid-template-columns: repeat(3, 1fr);
  }
}

.creator__stat {
  padding: 24px;
  border-radius: var(--radius-xl);
  background: var(--surface-container-lowest);
  border: 1px solid rgba(190, 200, 201, 0.1);
}

.creator__stat--primary {
  background: var(--primary);
  color: var(--on-primary);
  border-color: var(--primary-container);
}

.creator__stat--secondary {
  background: var(--secondary);
  color: var(--on-secondary);
  border-color: var(--secondary-container);
}

.creator__stat-label {
  font-family: var(--font-label);
  font-size: var(--label-sm);
  text-transform: uppercase;
  letter-spacing: 0.1em;
  font-weight: 600;
}

.creator__stat:not(.creator__stat--primary):not(.creator__stat--secondary) .creator__stat-label {
  color: var(--on-surface-variant);
}

.creator__stat--primary .creator__stat-label,
.creator__stat--secondary .creator__stat-label {
  opacity: 0.8;
}

.creator__stat-value {
  font-family: var(--font-headline);
  font-size: clamp(1.875rem, 3vw, 2.25rem);
  font-weight: 700;
  margin: 8px 0 0;
}

.creator__stat:not(.creator__stat--primary):not(.creator__stat--secondary) .creator__stat-value {
  color: var(--on-surface);
}

/* ==========================================================================
   MATERIALS GRID
   ========================================================================== */

.creator__grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 24px;
}

@media (min-width: 768px) {
  .creator__grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

.creator__card {
  display: flex;
  gap: 16px;
  background: var(--surface-container-lowest);
  border-radius: var(--radius-xl);
  overflow: hidden;
  transition: background var(--transition-fast), box-shadow var(--transition-fast);
  border: 1px solid rgba(190, 200, 201, 0.1);
}

.creator__card:hover {
  background: #fff;
  box-shadow: var(--shadow-md);
}

@media (max-width: 480px) {
  .creator__card {
    flex-direction: column;
  }
}

.creator__card-thumb {
  width: 160px;
  min-height: 160px;
  position: relative;
  flex-shrink: 0;
  overflow: hidden;
}

@media (max-width: 480px) {
  .creator__card-thumb {
    width: 100%;
    height: 180px;
  }
}

.creator__card-thumb img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.creator__card-badge {
  position: absolute;
  top: 8px;
  left: 8px;
  padding: 4px 8px;
  border-radius: var(--radius-sm);
  font-family: var(--font-label);
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.creator__card-badge--published {
  background: var(--primary);
  color: var(--on-primary);
}

.creator__card-badge--moderation {
  background: var(--secondary);
  color: var(--on-secondary);
}

.creator__card-body {
  flex: 1;
  padding: 16px 16px 16px 0;
  display: flex;
  flex-direction: column;
  min-width: 0;
}

@media (max-width: 480px) {
  .creator__card-body {
    padding: 0 16px 16px;
  }
}

.creator__card-title {
  font-family: var(--font-body);
  font-weight: 700;
  font-size: var(--body-lg);
  color: var(--on-surface);
  margin: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.creator__card-date {
  font-family: var(--font-label);
  font-size: var(--label-sm);
  color: var(--on-surface-variant);
  margin: 4px 0 0;
}

.creator__card-submitted {
  font-family: var(--font-label);
  font-size: var(--label-sm);
  color: var(--on-surface-variant);
  margin: 8px 0 0;
}

.creator__card-actions {
  display: flex;
  gap: 12px;
  align-items: center;
  margin-top: auto;
  padding-top: 12px;
}

.creator__card-action {
  display: flex;
  align-items: center;
  gap: 4px;
  background: none;
  border: none;
  font-family: var(--font-label);
  font-size: var(--body-sm);
  font-weight: 700;
  cursor: pointer;
  text-decoration: none;
  transition: opacity var(--transition-fast);
}

.creator__card-action:hover {
  text-decoration: underline;
}

.creator__card-action--primary {
  color: var(--primary);
}

.creator__card-action--error {
  color: var(--error);
}

.creator__card-action .material-symbols-outlined {
  font-size: 1rem;
}

/* ==========================================================================
   MODAL
   ========================================================================== */

.creator__overlay {
  position: fixed;
  inset: 0;
  z-index: var(--z-modal, 300);
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 16px;
}

.creator__modal {
  background: var(--surface);
  border-radius: var(--radius-xl);
  width: 100%;
  max-width: 900px;
  max-height: 90vh;
  overflow-y: auto;
  position: relative;
  box-shadow: var(--shadow-lg);
}

.creator__modal-close {
  position: absolute;
  top: 16px;
  right: 16px;
  background: none;
  border: none;
  color: var(--on-surface-variant);
  cursor: pointer;
  padding: 4px;
  border-radius: var(--radius-full);
  z-index: 1;
  transition: background var(--transition-fast);
}

.creator__modal-close:hover {
  background: var(--surface-container-high);
}

.creator__modal-inner {
  padding: 32px;
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.creator__modal-title {
  font-family: var(--font-headline);
  font-size: var(--headline-md);
  font-weight: 700;
  color: var(--on-surface);
  margin: 0;
}

/* ==========================================================================
   MOBILE BOTTOM NAV
   ========================================================================== */

.creator__mobile-nav {
  display: flex;
  position: fixed;
  bottom: 0;
  left: 0;
  width: 100%;
  z-index: 50;
  justify-content: space-around;
  align-items: center;
  padding: 8px 16px 24px;
  background: rgba(252, 249, 241, 0.8);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  border-top: 1px solid var(--ghost-border);
  box-shadow: 0 -4px 20px rgba(0, 0, 0, 0.04);
}

@media (min-width: 768px) {
  .creator__mobile-nav {
    display: none;
  }
}

.creator__mobile-nav-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 4px 16px;
  border-radius: var(--radius-xl);
  text-decoration: none;
  color: var(--on-surface);
  opacity: 0.6;
  transition: all var(--transition-fast);
  background: none;
  border: none;
  cursor: pointer;
  font: inherit;
}

.creator__mobile-nav-item--active {
  color: var(--primary);
  background: rgba(0, 70, 74, 0.05);
  opacity: 1;
}

.creator__mobile-nav-label {
  font-family: var(--font-label);
  font-size: 12px;
  font-weight: 600;
}
</style>