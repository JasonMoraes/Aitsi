<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import type { PhotoSummary } from '@/types'

defineProps<{
  photos: PhotoSummary[]
  loading?: boolean
}>()

defineEmits<{
  edit: [photoId: number]
  delete: [photoId: number]
}>()

const { t } = useI18n()

function formatDate(date: string): string {
  if (date.length === 4) return date
  if (date.length === 7) return date
  return date.slice(0, 10)
}
</script>

<template>
  <div class="photo-table-wrapper">
    <!-- Loading state -->
    <div v-if="loading" class="loading-state" role="status">
      <p>{{ t('common.loading') }}</p>
    </div>

    <!-- Empty state -->
    <div v-else-if="photos.length === 0" class="empty-state">
      <p>Brak zdjęć</p>
    </div>

    <!-- Table -->
    <table v-else class="photo-table" role="table">
      <thead>
        <tr>
          <th>Miniatura</th>
          <th>Tytuł</th>
          <th>Autor</th>
          <th>Data</th>
          <th>Kategoria</th>
          <th>Akcje</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="photo in photos" :key="photo.id">
          <td data-label="Miniatura">
            <img
              :src="photo.thumbnailUrl"
              :alt="photo.title"
              class="photo-thumbnail"
              width="60"
              height="60"
              loading="lazy"
            />
          </td>
          <td data-label="Tytuł">{{ photo.title }}</td>
          <td data-label="Autor">{{ photo.author.displayName }}</td>
          <td data-label="Data">{{ formatDate(photo.date) }}</td>
          <td data-label="Kategoria">{{ photo.category.name }}</td>
          <td data-label="Akcje">
            <div class="action-group">
              <button
                class="action-btn action-edit"
                @click="$emit('edit', photo.id)"
              >
                {{ t('admin.editPhoto') }}
              </button>
              <button
                class="action-btn action-delete"
                @click="$emit('delete', photo.id)"
              >
                {{ t('admin.deletePhoto') }}
              </button>
            </div>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
.photo-table-wrapper {
  width: 100%;
  overflow-x: auto;
}

.loading-state,
.empty-state {
  text-align: center;
  padding: 32px 16px;
  color: var(--on-surface-variant);
  font-size: var(--label-lg);
}

.photo-table {
  width: 100%;
  border-collapse: collapse;
}

.photo-table thead {
  position: sticky;
  top: 0;
  background-color: var(--surface-container-lowest);
  z-index: var(--z-sticky);
}

.photo-table thead th {
  text-align: left;
  padding: 12px;
  border-bottom: 2px solid var(--ghost-border);
  font-size: var(--label-lg);
  color: var(--on-surface-variant);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  font-weight: 600;
}

.photo-table tbody td {
  padding: 12px;
  border-bottom: 1px solid var(--ghost-border);
  font-size: var(--label-lg);
  color: var(--on-surface);
  vertical-align: middle;
}

.photo-thumbnail {
  width: 60px;
  height: 60px;
  object-fit: cover;
  border-radius: var(--radius-md);
  display: block;
}

/* Action buttons */
.action-group {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.action-btn {
  padding: 6px 12px;
  border-radius: var(--radius-md);
  font-size: var(--label-lg);
  font-weight: 600;
  font-family: var(--font-body);
  cursor: pointer;
  border: 1px solid transparent;
  min-height: 44px;
  white-space: nowrap;
  transition: background-color var(--transition-fast), opacity var(--transition-fast);
}

.action-btn:hover {
  opacity: 0.85;
}

.action-btn:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring);
}

.action-edit {
  background-color: transparent;
  border-color: var(--ghost-border);
  color: var(--on-surface);
}

.action-edit:hover {
  background-color: var(--surface-container-low);
}

.action-delete {
  background-color: var(--error);
  color: var(--on-error);
}

/* Responsive: card layout on mobile */
@media (max-width: 768px) {
  .photo-table thead {
    display: none;
  }

  .photo-table,
  .photo-table tbody,
  .photo-table tr,
  .photo-table td {
    display: block;
    width: 100%;
  }

  .photo-table tr {
    margin-bottom: 16px;
    border: 1px solid var(--ghost-border);
    border-radius: var(--radius-lg);
    padding: 12px;
    background-color: var(--surface-container-lowest);
  }

  .photo-table td {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 0;
    border-bottom: none;
  }

  .photo-table td:not(:last-child) {
    border-bottom: 1px solid var(--ghost-border);
  }

  .photo-table td::before {
    content: attr(data-label);
    font-weight: 600;
    font-size: var(--label-lg);
    color: var(--on-surface-variant);
    text-transform: uppercase;
    letter-spacing: 0.05em;
    margin-right: 12px;
    flex-shrink: 0;
  }

  .photo-thumbnail {
    width: 48px;
    height: 48px;
  }
}
</style>
