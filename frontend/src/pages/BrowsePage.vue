<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { usePhotosStore } from '@/stores/photos.store'
import { useCategoriesStore } from '@/stores/categories.store'
import { usePhotoSearch } from '@/composables/usePhotoSearch'
import PhotoSearchBar from '@/components/photos/PhotoSearchBar.vue'
import PhotoGrid from '@/components/photos/PhotoGrid.vue'
import PhotoFilters from '@/components/photos/PhotoFilters.vue'
import AppPagination from '@/components/common/AppPagination.vue'

const props = defineProps<{
  categoryId?: string
}>()

const route = useRoute()
const { t } = useI18n()
const { initFromUrl, syncToUrl } = usePhotoSearch()
const photosStore = usePhotosStore()
const categoriesStore = useCategoriesStore()

const searchQuery = ref('')
const filtersOpen = ref(false)

const filters = ref({
  categoryId: undefined as number | undefined,
  dateFrom: '',
  dateTo: '',
  tag: '',
  sortBy: 'relevance' as 'date' | 'createdAt' | 'relevance',
  sortDir: 'desc' as 'asc' | 'desc',
})

const currentPage = ref(1)

const activeFilterCount = computed(() => {
  let count = 0
  if (filters.value.categoryId) count++
  if (filters.value.dateFrom) count++
  if (filters.value.dateTo) count++
  if (filters.value.tag) count++
  if (filters.value.sortBy !== 'relevance') count++
  return count
})

function handleSearch(query: string) {
  searchQuery.value = query
  currentPage.value = 1
  syncToUrl({ q: query || undefined, categoryId: filters.value.categoryId ?? undefined, dateFrom: filters.value.dateFrom || undefined, dateTo: filters.value.dateTo || undefined, sortBy: filters.value.sortBy, sortDir: filters.value.sortDir, page: 1 })
  photosStore.fetchPhotos({ q: query, ...filters.value, page: 1 })
}

function resetFilters() {
  filters.value = { categoryId: undefined, dateFrom: '', dateTo: '', tag: '', sortBy: 'relevance', sortDir: 'desc' }
  currentPage.value = 1
  photosStore.fetchPhotos({ q: searchQuery.value, ...filters.value, page: 1 })
}

function goToPage(page: number) {
  currentPage.value = page
  photosStore.fetchPhotos({ ...filters.value, q: searchQuery.value, page })
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

watch(filters, () => {
  currentPage.value = 1
  photosStore.fetchPhotos({ q: searchQuery.value, ...filters.value, page: 1 })
}, { deep: true })

onMounted(() => {
  if (route.query.q) searchQuery.value = String(route.query.q)
  if (route.query.sortBy) filters.value.sortBy = String(route.query.sortBy) as any
  if (route.query.sortDir) filters.value.sortDir = String(route.query.sortDir) as any
  if (route.query.dateFrom) filters.value.dateFrom = String(route.query.dateFrom)
  if (route.query.dateTo) filters.value.dateTo = String(route.query.dateTo)
  if (route.query.tag) filters.value.tag = String(route.query.tag)
  if (props.categoryId) filters.value.categoryId = Number(props.categoryId)
  else if (route.query.categoryId) filters.value.categoryId = Number(route.query.categoryId)
  if (route.query.page) currentPage.value = Number(route.query.page)
  initFromUrl()

  photosStore.fetchPhotos({ q: searchQuery.value, ...filters.value, page: currentPage.value })
  categoriesStore.fetchTree()
})

watch(filtersOpen, (open) => {
  document.body.style.overflow = open ? 'hidden' : ''
})
</script>

<template>
  <div class="browse container">
    <h1 class="browse__title">Przeglądaj archiwum</h1>

    <PhotoSearchBar v-model="searchQuery" autofocus @search="handleSearch" />

    <div class="browse__layout">
      <button class="browse__filters-toggle" type="button" @click="filtersOpen = !filtersOpen">
        Filtry
        <span v-if="activeFilterCount"> ({{ activeFilterCount }})</span>
      </button>

      <PhotoFilters
        class="browse__sidebar"
        :class="{ 'is-open': filtersOpen }"
        v-model:categoryId="filters.categoryId"
        v-model:dateFrom="filters.dateFrom"
        v-model:dateTo="filters.dateTo"
        v-model:tag="filters.tag"
        v-model:sortBy="filters.sortBy"
        v-model:sortDir="filters.sortDir"
        :categories="categoriesStore.tree"
        @reset="resetFilters"
      />

      <div v-if="filtersOpen" class="browse__overlay" @click="filtersOpen = false" />

      <div class="browse__results">
        <p class="browse__count" aria-live="polite">
          {{ t('search.results', { count: photosStore.pagination.totalCount }) }}
        </p>

        <PhotoGrid :photos="photosStore.items" :loading="photosStore.isLoading" :empty-message="t('search.noResults')" />

        <AppPagination
          v-if="photosStore.pagination.totalPages > 1"
          :page="currentPage"
          :total-pages="photosStore.pagination.totalPages"
          :total-count="photosStore.pagination.totalCount"
          @update:page="goToPage"
        />
      </div>
    </div>
  </div>
</template>

<style scoped>
.browse {
  padding-top: 48px;
  padding-bottom: 32px;
}

.browse__title {
  font-family: var(--font-headline);
  font-size: var(--headline-md);
  margin: 0 0 24px;
}

.browse__layout {
  display: grid;
  grid-template-columns: 260px 1fr;
  gap: 32px;
  margin-top: 24px;
  align-items: start;
}

.browse__filters-toggle {
  display: none;
  width: 100%;
  padding: 12px 0;
  background: transparent;
  border: none;
  border-bottom: 1px solid var(--ghost-border);
  color: var(--on-surface);
  font-size: var(--label-lg);
  font-weight: 500;
  cursor: pointer;
  text-align: left;
}

.browse__sidebar {
  background: var(--surface-container-low);
  border-radius: var(--radius-lg);
  padding: 24px;
}

.browse__overlay { display: none; }

.browse__results { min-width: 0; }

.browse__count {
  font-size: var(--label-lg);
  color: var(--on-surface-variant);
  margin-bottom: 16px;
}

@media (max-width: 768px) {
  .browse__layout { grid-template-columns: 1fr; }

  .browse__filters-toggle { display: block; }

  .browse__sidebar {
    display: none;
    position: fixed;
    inset: 0;
    z-index: var(--z-modal);
    background: var(--surface-container-lowest);
    overflow-y: auto;
    padding: 24px;
    border-radius: 0;
  }

  .browse__sidebar.is-open { display: block; }

  .browse__overlay {
    display: block;
    position: fixed;
    inset: 0;
    z-index: calc(var(--z-modal) - 1);
    background: var(--overlay);
  }
}
</style>
