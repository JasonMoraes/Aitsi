<script setup lang="ts">
import { ref, computed } from 'vue'
import { useI18n } from 'vue-i18n'

const props = withDefaults(defineProps<{
  modelValue?: File | null
  maxSizeMb?: number
  previewUrl?: string
}>(), { maxSizeMb: 20 })

const emit = defineEmits<{
  'update:modelValue': [file: File | null]
  error: [message: string]
}>()

const { t } = useI18n()

const isDragging = ref(false)
const fileInput = ref<HTMLInputElement | null>(null)

const previewSrc = computed(() => {
  if (props.modelValue) {
    return URL.createObjectURL(props.modelValue)
  }
  return props.previewUrl ?? null
})

const hasPreview = computed(() => !!previewSrc.value)

function openFileDialog() {
  fileInput.value?.click()
}

function handleFileSelect(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (file) {
    validateAndEmit(file)
  }
  // Reset input so the same file can be re-selected
  input.value = ''
}

function handleDrop(event: DragEvent) {
  isDragging.value = false
  const file = event.dataTransfer?.files?.[0]
  if (file) {
    validateAndEmit(file)
  }
}

function handleDragOver(_event: DragEvent) {
  isDragging.value = true
}

function handleDragLeave() {
  isDragging.value = false
}

function validateAndEmit(file: File) {
  const allowedTypes = ['image/jpeg', 'image/png', 'image/webp']
  if (!allowedTypes.includes(file.type)) {
    emit('error', 'Dozwolone formaty: JPG, PNG, WebP')
    return
  }

  const maxBytes = props.maxSizeMb * 1024 * 1024
  if (file.size > maxBytes) {
    emit('error', `Maksymalny rozmiar pliku: ${props.maxSizeMb}MB`)
    return
  }

  emit('update:modelValue', file)
}

function removeFile() {
  emit('update:modelValue', null)
}
</script>

<template>
  <div
    class="dropzone"
    :class="{ 'dropzone--dragging': isDragging, 'dropzone--has-preview': hasPreview }"
    @dragover.prevent="handleDragOver"
    @dragleave.prevent="handleDragLeave"
    @drop.prevent="handleDrop"
  >
    <!-- Preview state -->
    <div v-if="hasPreview" class="dropzone__preview">
      <img :src="previewSrc!" :alt="modelValue?.name ?? 'Podgląd zdjęcia'" />
      <button
        type="button"
        class="dropzone__remove"
        aria-label="Usuń zdjęcie"
        @click="removeFile"
      >
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
          <line x1="18" y1="6" x2="6" y2="18" />
          <line x1="6" y1="6" x2="18" y2="18" />
        </svg>
      </button>
    </div>

    <!-- Empty state -->
    <div v-else class="dropzone__empty" @click="openFileDialog">
      <svg class="dropzone__icon" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" aria-hidden="true">
        <rect x="3" y="3" width="18" height="18" rx="2" />
        <circle cx="8.5" cy="8.5" r="1.5" />
        <polyline points="21 15 16 10 5 21" />
      </svg>
      <p class="dropzone__text">
        {{ t('upload.dropzone') }}
        <button type="button" class="dropzone__select-btn">{{ t('upload.selectFile') }}</button>
      </p>
      <p class="dropzone__hint">
        {{ t('upload.formats') }} &middot; {{ t('upload.maxSize', { size: maxSizeMb }) }}
      </p>
    </div>

    <input
      ref="fileInput"
      type="file"
      accept="image/jpeg,image/png,image/webp"
      class="dropzone__input"
      @change="handleFileSelect"
    />
  </div>
</template>

<style scoped>
.dropzone {
  border: 2px dashed var(--ghost-border);
  border-radius: var(--radius-lg);
  padding: 40px;
  text-align: center;
  transition: border-color var(--transition-fast), background-color var(--transition-fast);
  cursor: pointer;
}

.dropzone--dragging {
  border-color: var(--primary);
  background-color: var(--surface-container-low);
}

.dropzone--has-preview {
  padding: 16px;
  cursor: default;
}

.dropzone__input {
  position: absolute;
  width: 1px;
  height: 1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}

.dropzone__empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
}

.dropzone__icon {
  color: var(--on-surface-variant);
}

.dropzone__text {
  color: var(--on-surface-variant);
  font-size: var(--body-md);
}

.dropzone__select-btn {
  background: none;
  border: none;
  color: var(--primary);
  font-weight: 600;
  cursor: pointer;
  text-decoration: underline;
  font-size: inherit;
  font-family: var(--font-body);
  padding: 0;
  min-height: 44px;
}

.dropzone__select-btn:hover {
  color: var(--primary-container);
}

.dropzone__select-btn:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring);
}

.dropzone__hint {
  color: var(--on-surface-variant);
  font-size: var(--label-lg);
}

.dropzone__preview {
  position: relative;
  display: inline-block;
}

.dropzone__preview img {
  max-height: 300px;
  max-width: 100%;
  object-fit: contain;
  border-radius: var(--radius-lg);
}

.dropzone__remove {
  position: absolute;
  top: 8px;
  right: 8px;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: rgba(0, 0, 0, 0.6);
  color: #fff;
  border: none;
  border-radius: 50%;
  cursor: pointer;
  transition: background-color var(--transition-fast);
}

.dropzone__remove:hover {
  background-color: var(--error);
}

.dropzone__remove:focus-visible {
  outline: 2px solid #fff;
  outline-offset: 2px;
}
</style>
