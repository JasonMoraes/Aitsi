<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useCategoriesStore } from '@/stores/categories.store'
import { usePhotosStore } from '@/stores/photos.store'
import { useToastStore } from '@/stores/toast.store'
import ImageDropzone from './ImageDropzone.vue'
import DatePrecisionPicker from './DatePrecisionPicker.vue'
import CategorySelect from './CategorySelect.vue'

const emit = defineEmits<{
  success: []
  cancel: []
}>()

const { t } = useI18n()
const categoriesStore = useCategoriesStore()
const photosStore = usePhotosStore()
const toastStore = useToastStore()

const submitting = ref(false)
const fileError = ref('')

const form = reactive({
  file: null as File | null,
  title: '',
  description: '',
  categoryId: undefined as number | undefined,
  date: '1960',
  lat: undefined as number | undefined,
  lng: undefined as number | undefined,
  locationLabel: '',
  technique: '',
  quote: '',
  inventoryNumber: '',
  originalFormat: '',
  license: '',
  digitization: '',
  tags: '',
})

function resetForm() {
  form.file = null
  form.title = ''
  form.description = ''
  form.categoryId = undefined
  form.date = '1960'
  form.lat = undefined
  form.lng = undefined
  form.locationLabel = ''
  form.technique = ''
  form.quote = ''
  form.inventoryNumber = ''
  form.originalFormat = ''
  form.license = ''
  form.digitization = ''
  form.tags = ''
  fileError.value = ''
}

async function handleSubmit() {
  if (!form.file) {
    fileError.value = 'Zdjęcie jest wymagane'
    return
  }
  if (!form.title.trim()) {
    toastStore.show('Tytuł jest wymagany', 'error')
    return
  }
  fileError.value = ''
  submitting.value = true

  try {
    const formData = new FormData()
    formData.append('file', form.file)
    formData.append('title', form.title)
    formData.append('description', form.description)
    if (form.categoryId !== undefined) formData.append('categoryId', String(form.categoryId))
    formData.append('date', form.date)
    if (form.lat !== undefined) formData.append('lat', String(form.lat))
    if (form.lng !== undefined) formData.append('lng', String(form.lng))
    formData.append('locationLabel', form.locationLabel)
    if (form.technique) formData.append('technique', form.technique)
    if (form.quote) formData.append('quote', form.quote)
    if (form.inventoryNumber) formData.append('inventoryNumber', form.inventoryNumber)
    if (form.originalFormat) formData.append('originalFormat', form.originalFormat)
    if (form.license) formData.append('license', form.license)
    if (form.digitization) formData.append('digitization', form.digitization)
    if (form.tags) formData.append('tags', form.tags)

    await photosStore.uploadPhoto(formData)
    toastStore.show('Zdjęcie zostało dodane!', 'success')
    resetForm()
    emit('success')
  } catch {
    toastStore.show('Wystąpił błąd podczas dodawania zdjęcia', 'error')
  } finally {
    submitting.value = false
  }
}

onMounted(() => {
  categoriesStore.fetchTree()
})
</script>

<template>
  <form class="photo-upload-form" @submit.prevent="handleSubmit">
    <ImageDropzone v-model="form.file" @error="fileError = $event" />
    <p v-if="fileError" class="field-error" role="alert">{{ fileError }}</p>

    <div class="form-field">
      <label for="pu-title">{{ t('upload.photoTitle') }} *</label>
      <input
        id="pu-title"
        v-model="form.title"
        type="text"
        required
        maxlength="200"
      />
    </div>

    <DatePrecisionPicker v-model="form.date" />

    <div class="form-field">
      <label for="pu-location">{{ t('upload.location') }}</label>
      <input
        id="pu-location"
        v-model="form.locationLabel"
        type="text"
        placeholder="np. Kraków, Rynek Główny"
      />
    </div>

    <div class="location-coords">
      <div class="form-field">
        <label for="pu-lat">Szerokość geogr.</label>
        <input
          id="pu-lat"
          v-model.number="form.lat"
          type="number"
          step="any"
          min="-90"
          max="90"
          placeholder="np. 50.0614"
        />
      </div>
      <div class="form-field">
        <label for="pu-lng">Długość geogr.</label>
        <input
          id="pu-lng"
          v-model.number="form.lng"
          type="number"
          step="any"
          min="-180"
          max="180"
          placeholder="np. 19.9372"
        />
      </div>
    </div>

    <CategorySelect
      v-model="form.categoryId"
      :categories="categoriesStore.tree"
      required
    />

    <div class="form-field">
      <label for="pu-desc">{{ t('upload.description') }}</label>
      <textarea
        id="pu-desc"
        v-model="form.description"
        rows="4"
        maxlength="2000"
      ></textarea>
    </div>

    <div class="form-field">
      <label for="pu-quote">Cytat / historia</label>
      <textarea
        id="pu-quote"
        v-model="form.quote"
        rows="2"
        maxlength="1000"
        placeholder="Anegdota, wspomnienie lub cytat związany ze zdjęciem..."
      ></textarea>
    </div>

    <div class="form-field">
      <label for="pu-tags">Tagi</label>
      <input
        id="pu-tags"
        v-model="form.tags"
        type="text"
        placeholder="np. Architektura, Transport, Dwudziestolecie"
      />
      <span class="form-hint">Oddziel tagi przecinkami</span>
    </div>

    <details class="form-details">
      <summary class="form-details-summary">Metadane techniczne</summary>
      <div class="form-details-grid">
        <div class="form-field">
          <label for="pu-technique">Technika</label>
          <input id="pu-technique" v-model="form.technique" type="text" placeholder="np. Sepia, Srebro" />
        </div>
        <div class="form-field">
          <label for="pu-format">Format oryginału</label>
          <input id="pu-format" v-model="form.originalFormat" type="text" placeholder="np. 13 x 18 cm" />
        </div>
        <div class="form-field">
          <label for="pu-inventory">Nr inwentarzowy</label>
          <input id="pu-inventory" v-model="form.inventoryNumber" type="text" placeholder="np. HA/2023/X-102" />
        </div>
        <div class="form-field">
          <label for="pu-license">Licencja</label>
          <input id="pu-license" v-model="form.license" type="text" placeholder="np. CC BY-NC 4.0" />
        </div>
        <div class="form-field">
          <label for="pu-digitization">Digitalizacja</label>
          <input id="pu-digitization" v-model="form.digitization" type="text" placeholder="np. 600 DPI, TIFF" />
        </div>
      </div>
    </details>

    <div class="form-actions">
      <button type="button" class="btn-cancel" @click="$emit('cancel')">
        {{ t('common.cancel') }}
      </button>
      <button type="submit" class="btn-submit" :disabled="submitting">
        {{ submitting ? t('common.loading') : t('upload.submit') }}
      </button>
    </div>
  </form>
</template>

<style scoped>
.photo-upload-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.form-field {
  display: flex;
  flex-direction: column;
}

.form-field label {
  display: block;
  font-size: var(--label-lg);
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.03em;
  color: var(--on-surface-variant);
  margin-bottom: 6px;
  font-family: var(--font-label);
}

.form-field input,
.form-field textarea {
  width: 100%;
  padding: 8px 12px;
  border: 2px solid transparent;
  border-radius: var(--radius-md);
  background-color: var(--surface-container-low);
  color: var(--on-surface);
  font-size: var(--body-md);
  font-family: var(--font-body);
  min-height: 44px;
  transition: border-color var(--transition-fast), background-color var(--transition-fast);
}

.form-field input:focus,
.form-field textarea:focus {
  outline: none;
  background-color: var(--surface-container-lowest);
  border-color: var(--primary);
  box-shadow: var(--focus-ring);
}

.form-field textarea {
  resize: vertical;
  min-height: 100px;
}

.location-coords {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

@media (max-width: 480px) {
  .location-coords {
    grid-template-columns: 1fr;
  }
}

.field-error {
  color: var(--error);
  font-size: var(--label-lg);
  margin: -4px 0 0;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding-top: 8px;
}

.btn-cancel {
  padding: 12px 24px;
  background: var(--surface-container-high);
  color: var(--on-surface-variant);
  border: none;
  border-radius: var(--radius-lg);
  font-family: var(--font-label);
  font-weight: 600;
  font-size: var(--body-sm);
  cursor: pointer;
  min-height: 44px;
  transition: background var(--transition-fast);
}

.btn-cancel:hover {
  background: var(--surface-dim);
}

.btn-submit {
  padding: 12px 24px;
  background: var(--primary);
  color: var(--on-primary);
  border: none;
  border-radius: var(--radius-lg);
  font-family: var(--font-label);
  font-weight: 700;
  font-size: var(--body-sm);
  cursor: pointer;
  min-height: 44px;
  transition: opacity var(--transition-fast);
}

.btn-submit:hover:not(:disabled) {
  opacity: 0.9;
}

.btn-submit:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.form-hint {
  font-size: var(--label-sm, 0.75rem);
  color: var(--on-surface-variant);
  margin-top: 4px;
  font-family: var(--font-body);
}

.form-details {
  border: 1px solid var(--outline-variant, #ccc);
  border-radius: var(--radius-md);
  overflow: hidden;
}

.form-details-summary {
  padding: 12px 16px;
  font-family: var(--font-label);
  font-weight: 600;
  font-size: var(--body-sm);
  color: var(--on-surface-variant);
  cursor: pointer;
  background: var(--surface-container-low);
  transition: background var(--transition-fast);
}

.form-details-summary:hover {
  background: var(--surface-container-high);
}

.form-details-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
  padding: 16px;
}

@media (max-width: 480px) {
  .form-details-grid {
    grid-template-columns: 1fr;
  }
}
</style>
