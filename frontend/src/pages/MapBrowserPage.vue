<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { RouterLink } from 'vue-router'
import { LMap, LTileLayer, LMarker, LPopup } from '@vue-leaflet/vue-leaflet'
import 'leaflet/dist/leaflet.css'
import { photosApi } from '@/api/photos.api'
import { useCategoriesStore } from '@/stores/categories.store'
import { useToastStore } from '@/stores/toast.store'
import type { PhotoSummary } from '@/types'

const { t } = useI18n()
const categoriesStore = useCategoriesStore()
const toastStore = useToastStore()

/* ---- Map state ---- */
const mapCenter = ref<[number, number]>([52.2297, 21.0122])
const mapZoom = ref(13)
const legendOpen = ref(false)

/* ---- Filters ---- */
const searchQuery = ref('')
const activeCategoryId = ref<number | null>(null)
const yearFrom = ref(1850)
const yearTo = ref(new Date().getFullYear()anow resolv)
const sortDir = ref<'desc' | 'asc'>('desc')

/* ---- Results ---- */
const photos = ref<PhotoSummary[]>([])
const isLoading = ref(false)
const currentPage = ref(1)
const totalCount = ref(0)
const totalPages = ref(0)
const PAGE_SIZE = 12

/* ---- Computed ---- */
const photosWithLocation = computed(() =>
  photos.value.filter(p => p.location)
)

const hasMore = computed(() => currentPage.value < totalPages.value)

const rangeLeftPct = computed(() =>
  ((yearFrom.value - 1850) / (2025 - 1850)) * 100
)
const rangeRightPct = computed(() =>
  ((yearTo.value - 1850) / (2025 - 1850)) * 100
)

const activeCategory = computed(() =>
  activeCategoryId.value
    ? categoriesStore.flatMap.get(activeCategoryId.value) ?? null
    : null
)

const footerLabel = computed(() => {
  const parts: string[] = []
  if (activeCategory.value) parts.push(activeCategory.value.name)
  parts.push(`${yearFrom.value}–${yearTo.value}`)
  return parts.join(', ')
})

/* ---- API ---- */
async function fetchPhotos(append = false) {
  isLoading.value = true
  try {
    const res = await photosApi.search({
      q: searchQuery.value || undefined,
      categoryId: activeCategoryId.value ?? undefined,
      dateFrom: String(yearFrom.value),
      dateTo: String(yearTo.value),
      sortBy: 'date',
      sortDir: sortDir.value,
      page: currentPage.value,
      pageSize: PAGE_SIZE,
    })
    photos.value = append ? [...photos.value, ...res.items] : res.items
    totalCount.value = res.totalCount
    totalPages.value = res.totalPages
  } finally {
    isLoading.value = false
  }
}

/* ---- Handlers ---- */
let searchTimer: ReturnType<typeof setTimeout> | undefined
let rangeTimer: ReturnType<typeof setTimeout> | undefined

function resetAndFetch() {
  currentPage.value = 1
  fetchPhotos()
}

function handleSearchInput() {
  clearTimeout(searchTimer)
  searchTimer = setTimeout(resetAndFetch, 400)
}

function handleSearchSubmit() {
  clearTimeout(searchTimer)
  resetAndFetch()
}

function selectCategory(id: number | null) {
  activeCategoryId.value = id
  resetAndFetch()
}

function toggleSort() {
  sortDir.value = sortDir.value === 'desc' ? 'asc' : 'desc'
  resetAndFetch()
}

function handleFromInput() {
  if (yearFrom.value > yearTo.value) yearFrom.value = yearTo.value
  clearTimeout(rangeTimer)
  rangeTimer = setTimeout(resetAndFetch, 300)
}

function handleToInput() {
  if (yearTo.value < yearFrom.value) yearTo.value = yearFrom.value
  clearTimeout(rangeTimer)
  rangeTimer = setTimeout(resetAndFetch, 300)
}

function handleLoadMore() {
  if (!hasMore.value) return
  currentPage.value++
  fetchPhotos(true)
}

function handleMyLocation() {
  if (!navigator.geolocation) {
    toastStore.show('Geolokalizacja nie jest wspierana', 'error')
    return
  }
  navigator.geolocation.getCurrentPosition(
    (pos) => {
      mapCenter.value = [pos.coords.latitude, pos.coords.longitude]
      mapZoom.value = 14
    },
    () => toastStore.show('Nie udało się pobrać lokalizacji', 'error')
  )
}

function openStreetView() {
  const [lat, lng] = mapCenter.value
  window.open(`https://www.google.com/maps/@${lat},${lng},15z`, '_blank')
}

function centerOnPhoto(photo: PhotoSummary) {
  if (photo.location) {
    mapCenter.value = [photo.location.lat, photo.location.lng]
    mapZoom.value = 15
  }
}

function zoomIn() { mapZoom.value = Math.min(mapZoom.value + 1, 18) }
function zoomOut() { mapZoom.value = Math.max(mapZoom.value - 1, 3) }

/* ---- Init ---- */
onMounted(() => {
  categoriesStore.fetchTree()
  fetchPhotos()
})
</script>

<template>
  <div class="map-browser">
    <!-- Main split layout -->
    <main class="map-browser__content">
      <!-- Left: Map -->
      <section class="map-browser__map">
        <l-map
          :zoom="mapZoom"
          :center="mapCenter"
          :use-global-leaflet="false"
          :options="{ scrollWheelZoom: true, zoomControl: false }"
          class="map-browser__leaflet"
        >
          <l-tile-layer
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
            attribution="&copy; <a href='https://www.openstreetmap.org/copyright'>OpenStreetMap</a>"
            layer-type="base"
            name="OpenStreetMap"
          />
          <l-marker
            v-for="photo in photosWithLocation"
            :key="photo.id"
            :lat-lng="[photo.location!.lat, photo.location!.lng]"
          >
            <l-popup>
              <div class="map-browser__popup">
                <img :src="photo.thumbnailUrl" :alt="photo.title" class="map-browser__popup-thumb" />
                <p class="map-browser__popup-name">{{ photo.title }}</p>
                <p class="map-browser__popup-count">{{ photo.date }}</p>
                <RouterLink :to="`/zdjecie/${photo.id}`" class="map-browser__popup-link">
                  {{ t('mapBrowser.details') }}
                </RouterLink>
              </div>
            </l-popup>
          </l-marker>
        </l-map>

        <!-- Map controls overlay -->
        <div class="map-browser__map-controls">
          <div class="map-browser__zoom-group">
            <button class="map-browser__zoom-btn" @click="zoomIn" aria-label="Przybliz">
              <span class="material-symbols-outlined">add</span>
            </button>
            <div class="map-browser__zoom-divider"></div>
            <button class="map-browser__zoom-btn" @click="zoomOut" aria-label="Oddal">
              <span class="material-symbols-outlined">remove</span>
            </button>
          </div>
          <button class="map-browser__location-btn" aria-label="Moja lokalizacja" @click="handleMyLocation">
            <span class="material-symbols-outlined">my_location</span>
          </button>
        </div>

        <!-- Legend overlay -->
        <div v-if="legendOpen" class="map-browser__legend">
          <div class="map-browser__legend-header">
            <h4>{{ t('mapBrowser.legend') }}</h4>
            <button class="map-browser__legend-close" @click="legendOpen = false">
              <span class="material-symbols-outlined">close</span>
            </button>
          </div>
          <div class="map-browser__legend-item">
            <span class="material-symbols-outlined" style="color: var(--primary)">location_on</span>
            <span>Zdjęcie archiwalne z lokalizacją</span>
          </div>
          <div class="map-browser__legend-item">
            <span class="map-browser__legend-count">{{ photosWithLocation.length }}</span>
            <span>Zdjęć na mapie</span>
          </div>
        </div>

        <!-- Map footer info -->
        <div class="map-browser__map-footer">
          <div class="map-browser__map-footer-inner">
            <div class="map-browser__map-footer-info">
              <div class="map-browser__map-footer-icon-wrap">
                <span class="material-symbols-outlined">explore</span>
              </div>
              <div>
                <p class="map-browser__map-footer-label">{{ t('mapBrowser.currentFocus') }}</p>
                <p class="map-browser__map-footer-title">{{ footerLabel }}</p>
              </div>
            </div>
            <div class="map-browser__map-footer-actions">
              <button class="map-browser__map-footer-btn" @click="legendOpen = !legendOpen">{{ t('mapBrowser.legend') }}</button>
              <button class="map-browser__map-footer-btn map-browser__map-footer-btn--primary" @click="openStreetView">{{ t('mapBrowser.streetView') }}</button>
            </div>
          </div>
        </div>
      </section>

      <!-- Right: Browser -->
      <section class="map-browser__browser">
        <div class="map-browser__browser-header">
          <header>
            <h2 class="map-browser__browser-title">{{ t('mapBrowser.title') }}</h2>
            <p class="map-browser__browser-desc">{{ t('mapBrowser.description') }}</p>
          </header>

          <!-- Search -->
          <form class="map-browser__browser-search" @submit.prevent="handleSearchSubmit">
            <input
              v-model="searchQuery"
              type="text"
              class="map-browser__browser-search-input"
              :placeholder="t('mapBrowser.searchPlaceholder')"
              @input="handleSearchInput"
            />
            <span class="material-symbols-outlined map-browser__browser-search-icon">search</span>
          </form>

          <!-- Category pills -->
          <div class="map-browser__pills">
            <button
              class="map-browser__pill"
              :class="{ 'map-browser__pill--active': activeCategoryId === null }"
              @click="selectCategory(null)"
            >
              <span
                class="material-symbols-outlined map-browser__pill-icon"
                :style="activeCategoryId === null ? `font-variation-settings: 'FILL' 1;` : ''"
              >grid_view</span>
              {{ t('mapBrowser.allItems') }}
            </button>
            <button
              v-for="cat in categoriesStore.topCategories"
              :key="cat.id"
              class="map-browser__pill"
              :class="{ 'map-browser__pill--active': activeCategoryId === cat.id }"
              @click="selectCategory(cat.id)"
            >
              {{ cat.name }}
            </button>
          </div>

          <!-- Chronological range -->
          <div class="map-browser__range">
            <div class="map-browser__range-header">
              <span class="map-browser__range-label">{{ t('mapBrowser.chronologicalRange') }}</span>
              <span class="map-browser__range-value">{{ yearFrom }} -- {{ yearTo }}</span>
            </div>
            <div class="map-browser__range-track-wrap">
              <div class="map-browser__range-track">
                <div
                  class="map-browser__range-fill"
                  :style="{ left: rangeLeftPct + '%', right: (100 - rangeRightPct) + '%' }"
                />
              </div>
              <input
                type="range"
                min="1850" max="2025" step="1"
                :value="yearFrom"
                class="map-browser__range-input"
                @input="yearFrom = Number(($event.target as HTMLInputElement).value); handleFromInput()"
              />
              <input
                type="range"
                min="1850" max="2025" step="1"
                :value="yearTo"
                class="map-browser__range-input"
                @input="yearTo = Number(($event.target as HTMLInputElement).value); handleToInput()"
              />
            </div>
            <div class="map-browser__range-labels">
              <span>1850</span>
              <span>1900</span>
              <span>1950</span>
              <span>2000</span>
            </div>
          </div>
        </div>

        <!-- Results -->
        <div class="map-browser__results">
          <div class="map-browser__results-header">
            <h3 class="map-browser__results-title">
              {{ t('mapBrowser.recentlyDiscovered') }}
              <span class="map-browser__results-count">({{ totalCount }} {{ t('mapBrowser.results') }})</span>
            </h3>
            <button class="map-browser__sort-btn" @click="toggleSort">
              {{ t('mapBrowser.sortByDate') }}
              <span
                class="material-symbols-outlined map-browser__sort-icon"
                :style="{ transform: sortDir === 'asc' ? 'rotate(180deg)' : '', transition: 'transform 0.2s' }"
              >expand_more</span>
            </button>
          </div>

          <!-- Loading -->
          <div v-if="isLoading && !photos.length" class="map-browser__loading">
            {{ t('common.loading') }}
          </div>

          <!-- Empty -->
          <div v-else-if="!photos.length" class="map-browser__empty">
            Brak wynikow dla wybranych filtrow
          </div>

          <!-- Grid -->
          <div v-else class="map-browser__grid">
            <template v-for="(photo, idx) in photos" :key="photo.id">
              <!-- Featured card (first result) -->
              <RouterLink
                v-if="idx === 0"
                :to="`/zdjecie/${photo.id}`"
                class="map-browser__card map-browser__card--featured"
                @mouseenter="centerOnPhoto(photo)"
              >
                <div class="map-browser__card-image map-browser__card-image--featured">
                  <img :src="photo.thumbnailUrl" :alt="photo.title" />
                </div>
                <div class="map-browser__card-body map-browser__card-body--featured">
                  <div class="map-browser__featured-top">
                    <span class="map-browser__card-date">{{ photo.date }} -- {{ photo.category.name }}</span>
                    <span class="material-symbols-outlined map-browser__featured-star" style="font-variation-settings: 'FILL' 1;">star</span>
                  </div>
                  <h4 class="map-browser__card-title map-browser__card-title--featured">{{ photo.title }}</h4>
                  <p class="map-browser__card-desc">{{ photo.description }}</p>
                  <div class="map-browser__card-footer">
                    <div class="map-browser__featured-meta">
                      <span class="map-browser__featured-meta-item">
                        <span class="material-symbols-outlined map-browser__card-loc-icon">camera</span>
                        {{ photo.author.displayName }}
                      </span>
                      <span v-if="photo.location" class="map-browser__featured-meta-item">
                        <span class="material-symbols-outlined map-browser__card-loc-icon">location_on</span>
                        {{ photo.location.label || '' }}
                      </span>
                    </div>
                    <span class="map-browser__details-btn">{{ t('mapBrowser.details') }}</span>
                  </div>
                </div>
              </RouterLink>

              <!-- Regular card -->
              <RouterLink
                v-else
                :to="`/zdjecie/${photo.id}`"
                class="map-browser__card"
                @mouseenter="centerOnPhoto(photo)"
              >
                <div class="map-browser__card-image">
                  <img :src="photo.thumbnailUrl" :alt="photo.title" />
                  <span class="map-browser__card-badge map-browser__card-badge--primary">
                    {{ photo.category.name }}
                  </span>
                </div>
                <div class="map-browser__card-body">
                  <span class="map-browser__card-date">{{ photo.date }}</span>
                  <h4 class="map-browser__card-title">{{ photo.title }}</h4>
                  <p class="map-browser__card-desc">{{ photo.description }}</p>
                  <div class="map-browser__card-footer">
                    <span v-if="photo.location" class="map-browser__card-location">
                      <span class="material-symbols-outlined map-browser__card-loc-icon">location_on</span>
                      {{ photo.location.label || '' }}
                    </span>
                    <span class="material-symbols-outlined map-browser__card-arrow">arrow_forward</span>
                  </div>
                </div>
              </RouterLink>
            </template>
          </div>

          <div v-if="hasMore" class="map-browser__load-more-wrap">
            <button class="map-browser__load-more" :disabled="isLoading" @click="handleLoadMore">
              {{ isLoading ? t('common.loading') : t('mapBrowser.loadMore') }}
              <span class="material-symbols-outlined map-browser__load-more-icon">refresh</span>
            </button>
          </div>
        </div>
      </section>
    </main>

    <!-- Mobile bottom nav -->
    <nav class="map-browser__mobile-nav" aria-label="Nawigacja mobilna">
      <RouterLink to="/przegladaj" class="map-browser__mobile-link">
        <span class="material-symbols-outlined">search</span>
        <span class="map-browser__mobile-label">Przeglądaj</span>
      </RouterLink>
      <RouterLink to="/mapa" class="map-browser__mobile-link map-browser__mobile-link--active">
        <span class="material-symbols-outlined" style="font-variation-settings: 'FILL' 1;">map</span>
        <span class="map-browser__mobile-label">Mapa</span>
      </RouterLink>
      <RouterLink to="/dodaj-zdjecie" class="map-browser__mobile-link">
        <span class="material-symbols-outlined">add_a_photo</span>
        <span class="map-browser__mobile-label">Dodaj</span>
      </RouterLink>
      <RouterLink to="/moje-zdjecia" class="map-browser__mobile-link">
        <span class="material-symbols-outlined">person</span>
        <span class="map-browser__mobile-label">Profil</span>
      </RouterLink>
    </nav>
  </div>
</template>

<style scoped>
/* ==========================================================================
   MapBrowserPage — Archive Map Browser
   ========================================================================== */

.map-browser {
  display: flex;
  flex-direction: column;
  height: calc(100vh - 56px);
  background: var(--surface);
  color: var(--on-surface);
  font-family: var(--font-body);
  overflow: hidden;
}

/* --------------------------------------------------------------------------
   Main split content
   -------------------------------------------------------------------------- */
.map-browser__content {
  display: flex;
  flex-direction: column;
  flex: 1;
  overflow: hidden;
}

@media (min-width: 768px) {
  .map-browser__content {
    flex-direction: row;
  }
}

/* --------------------------------------------------------------------------
   Map section
   -------------------------------------------------------------------------- */
.map-browser__map {
  position: relative;
  width: 100%;
  height: 50vh;
  flex-shrink: 0;
  overflow: hidden;
  background: var(--surface-container-highest);
}

@media (min-width: 768px) {
  .map-browser__map {
    width: 50%;
    height: auto;
  }
}

@media (min-width: 1024px) {
  .map-browser__map {
    width: 60%;
  }
}

.map-browser__leaflet {
  width: 100%;
  height: 100%;
  z-index: 1;
}

/* Popup styling */
.map-browser__popup {
  text-align: center;
  min-width: 140px;
}

.map-browser__popup-thumb {
  width: 100%;
  max-width: 160px;
  aspect-ratio: 4/3;
  object-fit: cover;
  border-radius: var(--radius-md);
  margin-bottom: 0.5rem;
}

.map-browser__popup-name {
  font-family: var(--font-label);
  font-size: var(--body-sm);
  font-weight: 700;
  color: var(--on-surface);
  margin: 0 0 0.125rem;
}

.map-browser__popup-count {
  font-family: var(--font-label);
  font-size: 0.625rem;
  font-style: italic;
  color: var(--on-surface-variant);
  margin: 0;
}

.map-browser__popup-link {
  display: inline-block;
  margin-top: 0.5rem;
  font-family: var(--font-label);
  font-size: var(--label-sm);
  font-weight: 700;
  color: var(--primary);
  text-decoration: none;
}

.map-browser__popup-link:hover {
  text-decoration: underline;
}

/* Map controls overlay */
.map-browser__map-controls {
  position: absolute;
  top: 1.5rem;
  left: 1.5rem;
  z-index: 10;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.map-browser__zoom-group {
  background: var(--surface-container-lowest);
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-lg);
  border: 1px solid var(--ghost-border);
  display: flex;
  flex-direction: column;
  gap: 0;
  overflow: hidden;
}

.map-browser__zoom-btn {
  padding: 0.5rem;
  background: none;
  border: none;
  color: var(--primary);
  cursor: pointer;
  transition: background var(--transition-fast);
}

.map-browser__zoom-btn:hover {
  background: var(--surface-container);
}

.map-browser__zoom-divider {
  height: 1px;
  background: var(--outline-variant);
  opacity: 0.2;
  margin: 0 0.5rem;
}

.map-browser__location-btn {
  background: var(--surface-container-lowest);
  border: 1px solid var(--ghost-border);
  border-radius: var(--radius-xl);
  padding: 0.75rem;
  color: var(--primary);
  cursor: pointer;
  box-shadow: var(--shadow-lg);
  transition: background var(--transition-fast);
}

.map-browser__location-btn:hover {
  background: var(--surface-container);
}

/* Legend overlay */
.map-browser__legend {
  position: absolute;
  bottom: 6rem;
  left: 1.5rem;
  z-index: 15;
  background: var(--surface-container-lowest);
  border-radius: var(--radius-xl);
  padding: 1.25rem;
  box-shadow: var(--shadow-lg);
  border: 1px solid var(--ghost-border);
  min-width: 220px;
}

.map-browser__legend-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.75rem;
}

.map-browser__legend-header h4 {
  font-family: var(--font-label);
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  font-size: var(--label-sm);
  color: var(--on-surface-variant);
  margin: 0;
}

.map-browser__legend-close {
  background: none;
  border: none;
  color: var(--on-surface-variant);
  cursor: pointer;
  padding: 0;
  display: flex;
}

.map-browser__legend-close .material-symbols-outlined {
  font-size: 1.125rem;
}

.map-browser__legend-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: var(--body-sm);
  color: var(--on-surface);
  margin-bottom: 0.375rem;
}

.map-browser__legend-item:last-child {
  margin-bottom: 0;
}

.map-browser__legend-item .material-symbols-outlined {
  font-size: 1.25rem;
}

.map-browser__legend-count {
  font-weight: 700;
  color: var(--primary);
  min-width: 1.5rem;
  text-align: center;
}

/* Map footer info bar */
.map-browser__map-footer {
  position: absolute;
  bottom: 1.5rem;
  left: 1.5rem;
  right: 1.5rem;
  z-index: 10;
}

.map-browser__map-footer-inner {
  background: var(--surface-container-lowest);
  opacity: 0.95;
  backdrop-filter: blur(8px);
  padding: 1rem 1.5rem;
  border-radius: var(--radius-2xl);
  border: 1px solid var(--ghost-border);
  box-shadow: var(--shadow-lg);
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  flex-wrap: wrap;
}

.map-browser__map-footer-info {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.map-browser__map-footer-icon-wrap {
  background: rgba(0, 70, 74, 0.1);
  padding: 0.5rem;
  border-radius: var(--radius-full);
  color: var(--primary);
  display: flex;
}

.map-browser__map-footer-label {
  font-family: var(--font-label);
  font-size: var(--label-sm);
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.15em;
  color: var(--on-surface-variant);
  margin: 0 0 0.125rem;
}

.map-browser__map-footer-title {
  font-family: var(--font-headline);
  font-size: var(--body-lg);
  font-weight: 700;
  color: var(--primary);
  margin: 0;
}

.map-browser__map-footer-actions {
  display: flex;
  gap: 0.5rem;
}

.map-browser__map-footer-btn {
  padding: 0.5rem 1rem;
  font-family: var(--font-label);
  font-size: var(--body-sm);
  border: none;
  background: none;
  color: var(--primary);
  cursor: pointer;
  border-radius: var(--radius-lg);
  transition: background var(--transition-fast);
}

.map-browser__map-footer-btn:hover {
  background: rgba(0, 70, 74, 0.05);
}

.map-browser__map-footer-btn--primary {
  background: var(--primary);
  color: var(--on-primary);
}

.map-browser__map-footer-btn--primary:hover {
  opacity: 0.9;
}

/* --------------------------------------------------------------------------
   Browser section
   -------------------------------------------------------------------------- */
.map-browser__browser {
  width: 100%;
  flex: 1;
  overflow-y: auto;
  border-left: 1px solid var(--ghost-border);
  box-shadow: -10px 0 30px rgba(0, 0, 0, 0.02);
  z-index: 20;
  background: var(--surface);
}

@media (min-width: 768px) {
  .map-browser__browser {
    width: 50%;
    flex: none;
  }
}

@media (min-width: 1024px) {
  .map-browser__browser {
    width: 40%;
  }
}

.map-browser__browser-header {
  padding: 2rem;
  background: color-mix(in srgb, var(--surface-container-low) 50%, var(--surface));
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.map-browser__browser-title {
  font-family: var(--font-headline);
  font-size: var(--headline-lg);
  font-weight: 700;
  color: var(--on-surface);
  margin: 0 0 0.5rem;
}

.map-browser__browser-desc {
  font-size: var(--body-sm);
  color: var(--on-surface-variant);
  line-height: 1.6;
  margin: 0;
}

/* Browser search */
.map-browser__browser-search {
  position: relative;
}

.map-browser__browser-search-input {
  width: 100%;
  background: var(--surface-container-lowest);
  border: none;
  padding: 1rem 3rem 1rem 1.5rem;
  border-radius: var(--radius-xl);
  box-shadow: 0 2px 15px rgba(0, 0, 0, 0.03);
  font-family: var(--font-body);
  font-size: var(--body-md);
  color: var(--on-surface);
  outline: none;
  box-sizing: border-box;
}

.map-browser__browser-search-input:focus {
  box-shadow: var(--focus-ring);
}

.map-browser__browser-search-input::placeholder {
  color: var(--on-surface-variant);
  opacity: 0.5;
}

.map-browser__browser-search-icon {
  position: absolute;
  right: 1.5rem;
  top: 50%;
  transform: translateY(-50%);
  color: var(--primary);
}

/* Category pills */
.map-browser__pills {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.map-browser__pill {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1.25rem;
  border-radius: var(--radius-full);
  font-family: var(--font-label);
  font-size: var(--body-sm);
  border: 1px solid var(--ghost-border);
  background: var(--surface-container-lowest);
  color: var(--on-surface-variant);
  cursor: pointer;
  transition: all var(--transition-fast);
}

.map-browser__pill:hover {
  color: var(--primary);
}

.map-browser__pill--active {
  background: var(--primary);
  color: var(--on-primary);
  border-color: var(--primary);
}

.map-browser__pill-icon {
  font-size: 1rem !important;
}

/* Chronological range */
.map-browser__range {
  padding-top: 0.5rem;
}

.map-browser__range-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  margin-bottom: 1rem;
}

.map-browser__range-label {
  font-family: var(--font-label);
  font-size: var(--label-sm);
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.15em;
  color: var(--on-surface-variant);
}

.map-browser__range-value {
  font-family: var(--font-headline);
  font-size: var(--body-lg);
  font-weight: 700;
  font-style: italic;
  color: var(--primary);
}

.map-browser__range-track-wrap {
  position: relative;
  height: 1.5rem;
  display: flex;
  align-items: center;
}

.map-browser__range-track {
  position: absolute;
  left: 0;
  right: 0;
  height: 0.375rem;
  background: color-mix(in srgb, var(--outline-variant) 30%, transparent);
  border-radius: var(--radius-full);
}

.map-browser__range-fill {
  position: absolute;
  height: 100%;
  background: var(--primary);
  border-radius: var(--radius-full);
}

.map-browser__range-input {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  -webkit-appearance: none;
  appearance: none;
  background: transparent;
  pointer-events: none;
  margin: 0;
  z-index: 3;
}

.map-browser__range-input::-webkit-slider-thumb {
  -webkit-appearance: none;
  pointer-events: all;
  width: 1rem;
  height: 1rem;
  border-radius: 50%;
  background: var(--surface-container-lowest);
  border: 2px solid var(--primary);
  cursor: pointer;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
  position: relative;
  z-index: 5;
}

.map-browser__range-input::-moz-range-thumb {
  pointer-events: all;
  width: 1rem;
  height: 1rem;
  border-radius: 50%;
  background: var(--surface-container-lowest);
  border: 2px solid var(--primary);
  cursor: pointer;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

.map-browser__range-input:focus::-webkit-slider-thumb {
  box-shadow: var(--focus-ring);
}

.map-browser__range-labels {
  display: flex;
  justify-content: space-between;
  margin-top: 0.5rem;
  font-family: var(--font-label);
  font-size: 0.625rem;
  color: var(--on-surface-variant);
  opacity: 0.6;
  font-weight: 500;
}

/* --------------------------------------------------------------------------
   Results grid
   -------------------------------------------------------------------------- */
.map-browser__results {
  padding: 2rem;
}

.map-browser__results-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1.5rem;
}

.map-browser__results-title {
  font-family: var(--font-headline);
  font-size: var(--title-lg);
  font-weight: 700;
  color: var(--on-surface);
  margin: 0;
}

.map-browser__results-count {
  font-family: var(--font-body);
  font-size: var(--body-sm);
  font-weight: 400;
  color: var(--on-surface-variant);
  margin-left: 0.5rem;
}

.map-browser__sort-btn {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  background: none;
  border: none;
  font-family: var(--font-label);
  font-size: var(--label-sm);
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  color: var(--primary);
  cursor: pointer;
}

.map-browser__sort-icon {
  font-size: 1rem !important;
}

.map-browser__loading,
.map-browser__empty {
  text-align: center;
  padding: 3rem 1rem;
  color: var(--on-surface-variant);
  font-size: var(--body-md);
}

.map-browser__grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1.5rem;
}

@media (min-width: 640px) {
  .map-browser__grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

/* Card */
.map-browser__card {
  display: flex;
  flex-direction: column;
  background: var(--surface-container-lowest);
  border-radius: var(--radius-xl);
  overflow: hidden;
  box-shadow: var(--shadow-sm);
  text-decoration: none;
  color: inherit;
  transition: box-shadow var(--transition-normal);
}

.map-browser__card:hover {
  box-shadow: var(--shadow-lg);
}

.map-browser__card:hover .map-browser__card-title {
  color: var(--primary);
}

.map-browser__card:hover img {
  transform: scale(1.1);
}

.map-browser__card-image {
  aspect-ratio: 4/3;
  overflow: hidden;
  position: relative;
}

.map-browser__card-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.7s ease;
}

.map-browser__card-badge {
  position: absolute;
  top: 0.75rem;
  right: 0.75rem;
  padding: 0.25rem 0.5rem;
  border-radius: var(--radius-sm);
  font-family: var(--font-label);
  font-size: 0.625rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.15em;
  backdrop-filter: blur(8px);
}

.map-browser__card-badge--primary {
  background: rgba(0, 70, 74, 0.8);
  color: var(--on-primary);
}

.map-browser__card-badge--secondary {
  background: rgba(160, 65, 0, 0.8);
  color: var(--on-secondary);
}

.map-browser__card-body {
  padding: 1.25rem;
  display: flex;
  flex-direction: column;
  flex-grow: 1;
}

.map-browser__card-date {
  font-family: var(--font-label);
  font-size: 0.625rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.15em;
  color: var(--secondary);
  margin-bottom: 0.25rem;
}

.map-browser__card-title {
  font-family: var(--font-headline);
  font-size: var(--body-lg);
  font-weight: 700;
  line-height: 1.3;
  color: var(--on-surface);
  margin: 0 0 0.5rem;
  transition: color var(--transition-fast);
}

.map-browser__card-desc {
  font-size: var(--body-sm);
  color: var(--on-surface-variant);
  line-height: 1.6;
  margin: 0 0 1rem;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.map-browser__card-footer {
  margin-top: auto;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.map-browser__card-location {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  font-size: var(--label-sm);
  color: var(--on-surface-variant);
}

.map-browser__card-loc-icon {
  font-size: 0.875rem !important;
}

.map-browser__card-arrow {
  color: var(--primary);
}

/* Featured card */
.map-browser__card--featured {
  grid-column: 1 / -1;
  border: 1px solid rgba(0, 70, 74, 0.05);
}

@media (min-width: 640px) {
  .map-browser__card--featured {
    flex-direction: row;
  }
}

.map-browser__card-image--featured {
  aspect-ratio: 16/10;
}

@media (min-width: 640px) {
  .map-browser__card-image--featured {
    width: 40%;
    aspect-ratio: auto;
    flex-shrink: 0;
  }
}

@media (min-width: 640px) {
  .map-browser__card-body--featured {
    width: 60%;
  }
}

.map-browser__card-body--featured {
  padding: 1.5rem;
}

.map-browser__featured-top {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 0.5rem;
}

.map-browser__featured-star {
  color: var(--primary);
}

.map-browser__card-title--featured {
  font-size: var(--headline-sm);
}

.map-browser__featured-meta {
  display: flex;
  gap: 1rem;
}

.map-browser__featured-meta-item {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  font-size: var(--label-sm);
  font-weight: 500;
  color: var(--on-surface-variant);
}

.map-browser__details-btn {
  background: rgba(0, 70, 74, 0.05);
  color: var(--primary);
  padding: 0.5rem 1rem;
  border: none;
  border-radius: var(--radius-lg);
  font-family: var(--font-label);
  font-size: var(--label-sm);
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.map-browser__details-btn:hover {
  background: var(--primary);
  color: var(--on-primary);
}

/* Load more */
.map-browser__load-more-wrap {
  display: flex;
  justify-content: center;
  margin-top: 3rem;
  margin-bottom: 6rem;
}

.map-browser__load-more {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: var(--surface-container-low);
  border: 1px solid var(--ghost-border);
  color: var(--on-surface-variant);
  padding: 0.75rem 2rem;
  border-radius: var(--radius-full);
  font-family: var(--font-label);
  font-size: var(--body-sm);
  cursor: pointer;
  transition: background var(--transition-fast);
}

.map-browser__load-more:hover:not(:disabled) {
  background: var(--surface-container);
}

.map-browser__load-more:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.map-browser__load-more-icon {
  font-size: 1rem !important;
}

/* --------------------------------------------------------------------------
   Mobile bottom nav
   -------------------------------------------------------------------------- */
.map-browser__mobile-nav {
  position: fixed;
  bottom: 0;
  left: 0;
  width: 100%;
  z-index: var(--z-sticky);
  background: var(--surface);
  opacity: 0.8;
  backdrop-filter: blur(12px);
  display: flex;
  justify-content: space-around;
  align-items: center;
  padding: 0.5rem 1rem;
  padding-bottom: calc(0.5rem + env(safe-area-inset-bottom, 0));
  border-top: 1px solid var(--ghost-border);
  box-shadow: 0 -4px 20px rgba(0, 0, 0, 0.04);
}

@media (min-width: 768px) {
  .map-browser__mobile-nav {
    display: none;
  }
}

.map-browser__mobile-link {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-decoration: none;
  color: var(--on-surface);
  opacity: 0.6;
  padding: 0.25rem 1rem;
  border-radius: var(--radius-xl);
  transition: all var(--transition-fast);
}

.map-browser__mobile-link--active {
  color: var(--primary);
  opacity: 1;
  background: rgba(0, 70, 74, 0.05);
}

.map-browser__mobile-label {
  font-family: var(--font-body);
  font-size: 0.75rem;
  font-weight: 600;
}

/* --------------------------------------------------------------------------
   Leaflet style overrides (unscoped via :deep)
   -------------------------------------------------------------------------- */
.map-browser__map :deep(.leaflet-control-attribution) {
  font-size: 0.625rem;
  background: var(--surface-container-lowest);
  opacity: 0.8;
}

.map-browser__map :deep(.leaflet-popup-content-wrapper) {
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-lg);
  border: 1px solid var(--ghost-border);
}

.map-browser__map :deep(.leaflet-popup-tip) {
  box-shadow: none;
}
</style>
