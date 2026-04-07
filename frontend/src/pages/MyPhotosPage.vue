<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { usePhotosStore } from '@/stores/photos.store'
import { useToastStore } from '@/stores/toast.store'
import PhotoGrid from '@/components/photos/PhotoGrid.vue'

const router = useRouter()
const { t } = useI18n()
const photosStore = usePhotosStore()
const toastStore = useToastStore()

const deleteTarget = ref<number | null>(null)

function editPhoto(photoId: number) {
  router.push({ name: 'edit-photo', params: { id: photoId } })
}

function confirmDelete(photoId: number) {
  deleteTarget.value = photoId
}

async function handleDelete() {
  if (deleteTarget.value === null) return

  await photosStore.deletePhoto(deleteTarget.value)
  toastStore.show('Zdjęcie zostało usunięte', 'success')
  deleteTarget.value = null
}

onMounted(() => {
  photosStore.fetchMyPhotos()
})
</script>

<template>
  <div class="container">
    <div class="page-header">
      <h1>{{ t('nav.myPhotos') }}</h1>
      <router-link to="/dodaj-zdjecie" class="add-btn">
        {{ t('nav.upload') }}
      </router-link>
    </div>

    <PhotoGrid
      :photos="photosStore.myPhotos"
      :loading="photosStore.isLoading"
      show-actions
      @edit="editPhoto"
      @delete="confirmDelete"
    />

    <!-- Delete confirmation dialog -->
    <Teleport to="body">
      <div
        v-if="deleteTarget !== null"
        class="confirm-dialog-overlay"
        @click.self="deleteTarget = null"
      >
        <div
          class="confirm-dialog"
          role="alertdialog"
          aria-label="Potwierdź usunięcie"
        >
          <h2>Usunąć zdjęcie?</h2>
          <p>Tej operacji nie można cofnąć.</p>
          <div class="confirm-actions">
            <button class="btn-cancel" @click="deleteTarget = null">
              {{ t('common.cancel') }}
            </button>
            <button class="btn-danger" @click="handleDelete">
              {{ t('common.delete') }}
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
.container {
  max-width: var(--container-max);
  margin: 0 auto;
  padding: 32px 16px;
}

@media (min-width: 768px) {
  .container {
    padding: 32px 24px;
  }
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 32px;
  flex-wrap: wrap;
  gap: 12px;
}

.page-header h1 {
  font-family: var(--font-headline);
  font-size: var(--headline-lg);
  font-weight: 400;
  color: var(--on-surface);
}

.add-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  background-color: var(--primary);
  color: var(--on-primary);
  padding: 10px 24px;
  border-radius: var(--radius-lg);
  text-decoration: none;
  font-family: var(--font-body);
  font-weight: 500;
  font-size: var(--label-lg);
  min-height: 44px;
  transition: background-color var(--transition-fast);
}

.add-btn:hover {
  background-color: var(--primary-container);
  color: var(--on-primary);
}

.add-btn:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring);
}
</style>

<style>
/* Dialog styles are unscoped since they are teleported to body */
.confirm-dialog-overlay {
  position: fixed;
  inset: 0;
  z-index: var(--z-modal);
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: var(--overlay);
  padding: 16px;
}

.confirm-dialog {
  max-width: 400px;
  width: 100%;
  background-color: var(--surface-container-lowest);
  border: 1px solid var(--ghost-border);
  border-radius: var(--radius-lg);
  padding: 24px;
  box-shadow: var(--shadow-lg);
}

.confirm-dialog h2 {
  font-family: var(--font-headline);
  font-size: var(--title-lg);
  font-weight: 400;
  color: var(--on-surface);
  margin-bottom: 8px;
}

.confirm-dialog p {
  color: var(--on-surface-variant);
  font-size: var(--label-lg);
  margin-bottom: 24px;
  line-height: 1.5;
}

.confirm-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.confirm-actions .btn-cancel {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 10px 24px;
  border: 1px solid var(--ghost-border);
  border-radius: var(--radius-lg);
  background-color: transparent;
  color: var(--on-surface);
  font-size: var(--label-lg);
  font-family: var(--font-body);
  font-weight: 500;
  min-height: 44px;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.confirm-actions .btn-cancel:hover {
  background-color: var(--surface-container-low);
}

.confirm-actions .btn-danger {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 10px 24px;
  border: none;
  border-radius: var(--radius-lg);
  background-color: var(--error);
  color: var(--on-error);
  font-size: var(--label-lg);
  font-family: var(--font-body);
  font-weight: 500;
  min-height: 44px;
  cursor: pointer;
  transition: opacity var(--transition-fast);
}

.confirm-actions .btn-danger:hover {
  opacity: 0.9;
}

.confirm-actions button:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring);
}
</style>
