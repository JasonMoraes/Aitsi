<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useCategoriesStore } from '@/stores/categories.store'
import { useToastStore } from '@/stores/toast.store'
import { photosApi } from '@/api/photos.api'
import ImageDropzone from '@/components/forms/ImageDropzone.vue'
import DatePrecisionPicker from '@/components/forms/DatePrecisionPicker.vue'
import CategorySelect from '@/components/forms/CategorySelect.vue'

const props = defineProps<{
  id: string
}>()

const router = useRouter()
const { t } = useI18n()
const categoriesStore = useCategoriesStore()
const toastStore = useToastStore()

const submitting = ref(false)
const fileError = ref('')
const loading = ref(true)
const existingImageUrl = ref('')

const form = reactive({
  file: null as File | null,
  title: '',
  description: '',
  categoryId: undefined as number | undefined,
  date: '',
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

onMounted(async () => {
  await categoriesStore.fetchTree()

  try {
    const photo = await photosApi.getById(Number(props.id))
    form.title = photo.title
    form.description = photo.description ?? ''
    form.categoryId = photo.category.id
    form.date = photo.date
    form.lat = photo.location?.lat
    form.lng = photo.location?.lng
    form.locationLabel = photo.locationLabel ?? ''
    form.technique = photo.technique ?? ''
    form.quote = photo.quote ?? ''
    form.inventoryNumber = photo.inventoryNumber ?? ''
    form.originalFormat = photo.originalFormat ?? ''
    form.license = photo.license ?? ''
    form.digitization = photo.digitization ?? ''
    form.tags = photo.tags?.join(', ') ?? ''
    existingImageUrl.value = photo.imageUrl
  } catch {
    toastStore.show(t('common.error'), 'error')
  }

  loading.value = false
})

async function handleSubmit() {
  fileError.value = ''
  submitting.value = true

  try {
    await photosApi.update(Number(props.id), {
      title: form.title,
      description: form.description,
      categoryId: form.categoryId!,
      date: form.date,
      lat: form.lat,
      lng: form.lng,
      locationLabel: form.locationLabel,
      technique: form.technique || undefined,
      quote: form.quote || undefined,
      inventoryNumber: form.inventoryNumber || undefined,
      originalFormat: form.originalFormat || undefined,
      license: form.license || undefined,
      digitization: form.digitization || undefined,
      tags: form.tags || undefined,
    })
    toastStore.show('Zdjęcie zostało zaktualizowane!', 'success')
    router.push({ name: 'my-photos' })
  } catch (e) {
    toastStore.show(t('common.error'), 'error')
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="container">
    <h1 class="page-title">Edytuj zdjęcie</h1>

    <div v-if="loading" class="loading-state">
      <p>{{ t('common.loading') }}</p>
    </div>

    <form v-else class="upload-form" @submit.prevent="handleSubmit">
      <ImageDropzone
        v-model="form.file"
        :preview-url="existingImageUrl"
        @error="fileError = $event"
      />
      <p v-if="fileError" class="field-error" role="alert">{{ fileError }}</p>

      <div class="form-field">
        <label for="title">{{ t('upload.photoTitle') }} *</label>
        <input
          id="title"
          v-model="form.title"
          type="text"
          required
          maxlength="200"
        />
      </div>

      <div class="form-field">
        <label for="description">{{ t('upload.description') }}</label>
        <textarea
          id="description"
          v-model="form.description"
          rows="4"
          maxlength="2000"
        ></textarea>
      </div>

      <CategorySelect
        v-model="form.categoryId"
        :categories="categoriesStore.tree"
        required
      />

      <DatePrecisionPicker v-model="form.date" />

      <div class="form-field">
        <label>{{ t('upload.location') }} *</label>
        <div class="location-inputs">
          <div>
            <label for="lat">Szerokość geogr.</label>
            <input
              id="lat"
              v-model.number="form.lat"
              type="number"
              step="any"
              min="-90"
              max="90"
              required
            />
          </div>
          <div>
            <label for="lng">Długość geogr.</label>
            <input
              id="lng"
              v-model.number="form.lng"
              type="number"
              step="any"
              min="-180"
              max="180"
              required
            />
          </div>
        </div>
        <div class="form-field" style="margin-top: 12px">
          <label for="locationLabel">Opis lokalizacji</label>
          <input
            id="locationLabel"
            v-model="form.locationLabel"
            type="text"
            placeholder="np. Kraków, Rynek Główny"
          />
        </div>
      </div>

      <div class="form-field">
        <label for="ep-quote">Cytat / historia</label>
        <textarea
          id="ep-quote"
          v-model="form.quote"
          rows="2"
          maxlength="1000"
          placeholder="Anegdota, wspomnienie lub cytat związany ze zdjęciem..."
        ></textarea>
      </div>

      <div class="form-field">
        <label for="ep-tags">Tagi</label>
        <input
          id="ep-tags"
          v-model="form.tags"
          type="text"
          placeholder="np. Architektura, Transport, Dwudziestolecie"
        />
        <span style="font-size: var(--label-sm, 0.75rem); color: var(--on-surface-variant); margin-top: 4px;">Oddziel tagi przecinkami</span>
      </div>

      <details class="form-details">
        <summary class="form-details-summary">Metadane techniczne</summary>
        <div class="form-details-grid">
          <div class="form-field">
            <label for="ep-technique">Technika</label>
            <input id="ep-technique" v-model="form.technique" type="text" placeholder="np. Sepia, Srebro" />
          </div>
          <div class="form-field">
            <label for="ep-format">Format oryginału</label>
            <input id="ep-format" v-model="form.originalFormat" type="text" placeholder="np. 13 x 18 cm" />
          </div>
          <div class="form-field">
            <label for="ep-inventory">Nr inwentarzowy</label>
            <input id="ep-inventory" v-model="form.inventoryNumber" type="text" placeholder="np. HA/2023/X-102" />
          </div>
          <div class="form-field">
            <label for="ep-license">Licencja</label>
            <input id="ep-license" v-model="form.license" type="text" placeholder="np. CC BY-NC 4.0" />
          </div>
          <div class="form-field">
            <label for="ep-digitization">Digitalizacja</label>
            <input id="ep-digitization" v-model="form.digitization" type="text" placeholder="np. 600 DPI, TIFF" />
          </div>
        </div>
      </details>

      <button type="submit" class="submit-btn" :disabled="submitting">
        {{ submitting ? t('common.loading') : t('common.save') }}
      </button>
    </form>
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

.page-title {
  font-family: var(--font-headline);
  font-size: var(--headline-lg);
  font-weight: 400;
  color: var(--on-surface);
  margin-bottom: 32px;
}

.loading-state {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 64px 16px;
  color: var(--on-surface-variant);
}

.upload-form {
  max-width: 700px;
  display: flex;
  flex-direction: column;
  gap: 24px;
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
.form-field textarea,
.form-field select {
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
.form-field textarea:focus,
.form-field select:focus {
  outline: none;
  background-color: var(--surface-container-lowest);
  border-color: var(--primary);
  box-shadow: var(--focus-ring);
}

.form-field textarea {
  resize: vertical;
  min-height: 100px;
}

.location-inputs {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.location-inputs label {
  font-weight: 500;
  font-size: var(--label-lg);
  color: var(--on-surface-variant);
  text-transform: none;
  letter-spacing: normal;
  margin-bottom: 6px;
  display: block;
  font-family: var(--font-label);
}

.location-inputs input {
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

.location-inputs input:focus {
  outline: none;
  background-color: var(--surface-container-lowest);
  border-color: var(--primary);
  box-shadow: var(--focus-ring);
}

.field-error {
  color: var(--error);
  font-size: var(--label-lg);
  margin-top: -4px;
}

.submit-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background-color: var(--primary);
  color: var(--on-primary);
  padding: 12px 32px;
  border: none;
  border-radius: var(--radius-lg);
  font-weight: 500;
  font-size: var(--title-lg);
  font-family: var(--font-body);
  min-height: 48px;
  cursor: pointer;
  transition: background-color var(--transition-fast);
}

.submit-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.submit-btn:not(:disabled):hover {
  background-color: var(--primary-container);
}

.submit-btn:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring);
}

@media (max-width: 480px) {
  .location-inputs {
    grid-template-columns: 1fr;
  }
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
