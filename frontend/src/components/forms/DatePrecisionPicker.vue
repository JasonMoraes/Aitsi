<script setup lang="ts">
import { ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'

const props = defineProps<{
  modelValue?: string
}>()

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

const { t } = useI18n()

type Precision = 'year' | 'month' | 'day'

const precision = ref<Precision>('year')
const year = ref<number>(1960)
const month = ref<number>(1)
const day = ref<number>(1)

const currentYear = new Date().getFullYear()

const months = [
  'Styczeń', 'Luty', 'Marzec', 'Kwiecień', 'Maj', 'Czerwiec',
  'Lipiec', 'Sierpień', 'Wrzesień', 'Październik', 'Listopad', 'Grudzień',
]

// Parse initial modelValue
if (props.modelValue) {
  const parts = props.modelValue.split('-')
  if (parts.length >= 1 && parts[0]) {
    year.value = parseInt(parts[0], 10)
  }
  if (parts.length >= 2 && parts[1]) {
    month.value = parseInt(parts[1], 10)
    precision.value = 'month'
  }
  if (parts.length >= 3 && parts[2]) {
    day.value = parseInt(parts[2], 10)
    precision.value = 'day'
  }
}

function emitValue() {
  const y = String(year.value).padStart(4, '0')
  if (precision.value === 'year') {
    emit('update:modelValue', y)
    return
  }
  const m = String(month.value).padStart(2, '0')
  if (precision.value === 'month') {
    emit('update:modelValue', `${y}-${m}`)
    return
  }
  const d = String(day.value).padStart(2, '0')
  emit('update:modelValue', `${y}-${m}-${d}`)
}

watch([precision, year, month, day], () => {
  emitValue()
})
</script>

<template>
  <fieldset class="date-precision-picker">
    <legend class="picker-legend">{{ t('upload.date') }}</legend>

    <div class="precision-selector" role="radiogroup" :aria-label="t('upload.date')">
      <label
        v-for="opt in (['year', 'month', 'day'] as Precision[])"
        :key="opt"
        class="precision-option"
        :class="{ 'precision-option--selected': precision === opt }"
      >
        <input
          type="radio"
          name="date-precision"
          :value="opt"
          v-model="precision"
          class="sr-only"
        />
        {{ t(`datePrecision.${opt}`) }}
      </label>
    </div>

    <div class="date-inputs">
      <div class="date-field">
        <label for="date-year">Rok</label>
        <input
          id="date-year"
          v-model.number="year"
          type="number"
          :min="1800"
          :max="currentYear"
          required
        />
      </div>

      <div v-if="precision === 'month' || precision === 'day'" class="date-field">
        <label for="date-month">Miesiąc</label>
        <select id="date-month" v-model.number="month">
          <option
            v-for="(name, idx) in months"
            :key="idx"
            :value="idx + 1"
          >
            {{ name }}
          </option>
        </select>
      </div>

      <div v-if="precision === 'day'" class="date-field">
        <label for="date-day">Dzień</label>
        <input
          id="date-day"
          v-model.number="day"
          type="number"
          :min="1"
          :max="31"
          required
        />
      </div>
    </div>
  </fieldset>
</template>

<style scoped>
.date-precision-picker {
  border: none;
  padding: 0;
  margin: 0;
}

.picker-legend {
  font-weight: 600;
  font-size: var(--label-lg);
  color: var(--on-surface);
  margin-bottom: 12px;
}

.precision-selector {
  display: flex;
  gap: 8px;
  margin-bottom: 16px;
  flex-wrap: wrap;
}

.precision-option {
  padding: 8px 12px;
  border: 1px solid var(--ghost-border);
  border-radius: var(--radius-md);
  cursor: pointer;
  font-size: var(--label-lg);
  color: var(--on-surface-variant);
  transition: background-color var(--transition-fast), color var(--transition-fast), border-color var(--transition-fast);
  user-select: none;
  min-height: 44px;
  display: inline-flex;
  align-items: center;
}

.precision-option:hover {
  background-color: var(--surface-container-low);
}

.precision-option--selected {
  background-color: var(--primary);
  color: var(--on-primary);
  border-color: var(--primary);
}

.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border-width: 0;
}

.date-inputs {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}

.date-field {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.date-field label {
  font-size: var(--label-lg);
  font-weight: 500;
  color: var(--on-surface-variant);
}

.date-field input,
.date-field select {
  padding: 8px 12px;
  border: 2px solid transparent;
  border-radius: var(--radius-md);
  background-color: var(--surface-container-low);
  color: var(--on-surface);
  font-size: var(--body-md);
  font-family: var(--font-body);
  min-width: 100px;
  min-height: 44px;
}

.date-field input:focus,
.date-field select:focus {
  outline: none;
  background-color: var(--surface-container-lowest);
  border-color: var(--primary);
  box-shadow: var(--focus-ring);
}
</style>
