<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount, nextTick } from 'vue'
import { useI18n } from 'vue-i18n'
import type { CategoryNode } from '@/types'

const props = withDefaults(defineProps<{
  modelValue?: number
  categories: CategoryNode[]
  required?: boolean
  error?: string
}>(), { required: false })

const emit = defineEmits<{
  'update:modelValue': [categoryId: number]
}>()

const { t } = useI18n()

const isOpen = ref(false)
const triggerRef = ref<HTMLButtonElement | null>(null)
const dropdownRef = ref<HTMLDivElement | null>(null)
const dropdownStyle = ref<Record<string, string>>({})

// Flatten category tree with depth info
interface FlatCategory {
  id: number
  name: string
  depth: number
}

function flattenCategories(nodes: CategoryNode[], depth = 0): FlatCategory[] {
  const result: FlatCategory[] = []
  for (const node of nodes) {
    result.push({ id: node.id, name: node.name, depth })
    if (node.children?.length) {
      result.push(...flattenCategories(node.children, depth + 1))
    }
  }
  return result
}

const flatCategories = computed(() => flattenCategories(props.categories))

// Find selected category path
const selectedLabel = computed(() => {
  if (!props.modelValue) return null

  function findPath(nodes: CategoryNode[], path: string[]): string[] | null {
    for (const node of nodes) {
      const current = [...path, node.name]
      if (node.id === props.modelValue) return current
      if (node.children?.length) {
        const found = findPath(node.children, current)
        if (found) return found
      }
    }
    return null
  }

  const path = findPath(props.categories, [])
  return path ? path.join(' > ') : null
})

function toggleDropdown() {
  if (isOpen.value) {
    isOpen.value = false
  } else {
    openDropdown()
  }
}

function openDropdown() {
  isOpen.value = true
  nextTick(() => {
    positionDropdown()
  })
}

function positionDropdown() {
  if (!triggerRef.value) return
  const rect = triggerRef.value.getBoundingClientRect()
  dropdownStyle.value = {
    position: 'fixed',
    top: `${rect.bottom + 4}px`,
    left: `${rect.left}px`,
    width: `${rect.width}px`,
  }
}

function selectCategory(id: number) {
  emit('update:modelValue', id)
  isOpen.value = false
  triggerRef.value?.focus()
}

function handleClickOutside(event: MouseEvent) {
  const target = event.target as Node
  if (
    triggerRef.value?.contains(target) ||
    dropdownRef.value?.contains(target)
  ) {
    return
  }
  isOpen.value = false
}

function handleKeydown(event: KeyboardEvent) {
  if (event.key === 'Escape') {
    isOpen.value = false
    triggerRef.value?.focus()
  }
}

onMounted(() => {
  document.addEventListener('mousedown', handleClickOutside)
  document.addEventListener('keydown', handleKeydown)
})

onBeforeUnmount(() => {
  document.removeEventListener('mousedown', handleClickOutside)
  document.removeEventListener('keydown', handleKeydown)
})
</script>

<template>
  <div class="category-select">
    <label class="category-label">
      {{ t('upload.category') }}
      <span v-if="required" aria-hidden="true"> *</span>
    </label>

    <button
      ref="triggerRef"
      type="button"
      class="category-trigger"
      :class="{ 'category-trigger--error': error }"
      :aria-expanded="isOpen"
      aria-haspopup="listbox"
      @click="toggleDropdown"
    >
      <span :class="{ 'category-trigger__placeholder': !selectedLabel }">
        {{ selectedLabel ?? 'Wybierz kategorię...' }}
      </span>
      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
        <polyline points="6 9 12 15 18 9" />
      </svg>
    </button>

    <Teleport to="body">
      <div
        v-if="isOpen"
        ref="dropdownRef"
        class="category-dropdown"
        :style="dropdownStyle"
        role="listbox"
        :aria-label="t('upload.category')"
      >
        <button
          v-for="cat in flatCategories"
          :key="cat.id"
          type="button"
          class="category-item"
          :class="{ 'category-item--selected': modelValue === cat.id }"
          :style="{ paddingLeft: `${12 + cat.depth * 16}px` }"
          role="option"
          :aria-selected="modelValue === cat.id"
          @click="selectCategory(cat.id)"
        >
          {{ cat.name }}
        </button>
      </div>
    </Teleport>

    <p v-if="error" class="category-error" role="alert">{{ error }}</p>
  </div>
</template>

<style scoped>
.category-select {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.category-label {
  font-weight: 500;
  font-size: var(--label-lg);
  color: var(--on-surface-variant);
}

.category-trigger {
  width: 100%;
  text-align: left;
  padding: 8px 12px;
  border: 1px solid var(--ghost-border);
  border-radius: var(--radius-md);
  background-color: var(--surface-container-low);
  color: var(--on-surface);
  font-size: var(--body-md);
  font-family: var(--font-body);
  cursor: pointer;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 8px;
  min-height: 44px;
  transition: border-color var(--transition-fast);
}

.category-trigger:focus-visible {
  outline: none;
  border-color: var(--primary);
  box-shadow: var(--focus-ring);
}

.category-trigger--error {
  border-color: var(--error);
}

.category-trigger__placeholder {
  color: var(--on-surface-variant);
}

.category-error {
  color: var(--error);
  font-size: var(--label-lg);
  margin: 0;
}
</style>

<style>
/* Dropdown is teleported to body, so unscoped */
.category-dropdown {
  z-index: var(--z-dropdown);
  background-color: var(--surface-container-lowest);
  border: 1px solid var(--ghost-border);
  border-radius: var(--radius-lg);
  max-height: 300px;
  overflow-y: auto;
  padding: 4px 0;
  box-shadow: var(--shadow-md);
}

.category-item {
  display: block;
  width: 100%;
  text-align: left;
  padding: 8px 12px;
  border: none;
  background: transparent;
  color: var(--on-surface);
  font-size: var(--label-lg);
  font-family: var(--font-body);
  cursor: pointer;
  min-height: 44px;
  display: flex;
  align-items: center;
  transition: background-color var(--transition-fast);
}

.category-item:hover {
  background-color: var(--surface-container-low);
}

.category-item--selected {
  font-weight: 600;
  color: var(--primary);
}

.category-item:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring);
}
</style>
