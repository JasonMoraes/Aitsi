<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'

const props = withDefaults(defineProps<{
  modelValue?: string
  compact?: boolean
  autofocus?: boolean
}>(), { modelValue: '', compact: false, autofocus: false })

const emit = defineEmits<{
  'update:modelValue': [value: string]
  search: [query: string]
}>()

const { t } = useI18n()

const inputRef = ref<HTMLInputElement | null>(null)
const localValue = ref(props.modelValue)
let debounceTimer: ReturnType<typeof setTimeout> | undefined

watch(() => props.modelValue, (newVal) => {
  localValue.value = newVal
})

function onInput(event: Event) {
  const value = (event.target as HTMLInputElement).value
  localValue.value = value

  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    emit('update:modelValue', value)
  }, 300)
}

function onSubmit() {
  clearTimeout(debounceTimer)
  emit('update:modelValue', localValue.value)
  emit('search', localValue.value)
}

function onClear() {
  localValue.value = ''
  emit('update:modelValue', '')
  emit('search', '')
  inputRef.value?.focus()
}

onMounted(() => {
  if (props.autofocus) {
    inputRef.value?.focus()
  }
})
</script>

<template>
  <form
    class="search-bar"
    role="search"
    aria-label="Wyszukiwanie zdjęć"
    @submit.prevent="onSubmit"
  >
    <div class="search-bar__wrapper">
      <input
        ref="inputRef"
        type="search"
        class="search-bar__input"
        :value="localValue"
        :placeholder="t('search.placeholder')"
        :aria-label="t('search.placeholder')"
        @input="onInput"
      />

      <button
        v-if="localValue"
        type="button"
        class="search-bar__clear"
        :aria-label="t('search.clear')"
        @click="onClear"
      >
        &times;
      </button>
    </div>

    <button type="submit" class="btn btn-primary">
      {{ t('search.button') }}
    </button>
  </form>
</template>

<style scoped>
.search-bar {
  display: flex;
  gap: 12px;
}

.search-bar__wrapper {
  position: relative;
  flex: 1;
}

.search-bar__input {
  width: 100%;
  height: 48px;
  padding: 8px 40px 8px 16px;
  border: 2px solid transparent;
  border-radius: var(--radius-md);
  background: var(--surface-container-low);
  color: var(--on-surface);
  font-size: var(--title-md);
  font-family: var(--font-body);
  transition: background var(--transition-normal), border-color var(--transition-normal);
}

.search-bar__input::placeholder {
  color: var(--on-surface-variant);
}

.search-bar__input:focus {
  outline: none;
  background: var(--surface-container-lowest);
  border-color: var(--primary);
}

.search-bar__input::-webkit-search-cancel-button {
  -webkit-appearance: none;
  appearance: none;
}

.search-bar__clear {
  position: absolute;
  top: 50%;
  right: 8px;
  transform: translateY(-50%);
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  padding: 0;
  border: none;
  border-radius: var(--radius-sm);
  background: transparent;
  color: var(--on-surface-variant);
  font-size: 1.2rem;
  line-height: 1;
  cursor: pointer;
  transition: color var(--transition-fast);
}

.search-bar__clear:hover {
  color: var(--on-surface);
}
</style>
