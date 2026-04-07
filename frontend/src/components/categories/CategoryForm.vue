<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import { useI18n } from 'vue-i18n'
import type { CategoryNode, CategoryCreatePayload } from '@/types'

const props = defineProps<{
  category?: CategoryNode
  categories: CategoryNode[]
}>()

const emit = defineEmits<{
  submit: [data: CategoryCreatePayload]
  cancel: []
}>()

const { t } = useI18n()

const name = ref('')
const parentId = ref<number | null>(null)
const description = ref('')

// Populate fields when editing
watch(
  () => props.category,
  (cat) => {
    if (cat) {
      name.value = cat.name
      parentId.value = cat.parentId ?? null
      description.value = cat.description ?? ''
    } else {
      name.value = ''
      parentId.value = null
      description.value = ''
    }
  },
  { immediate: true },
)

interface FlatOption {
  id: number
  name: string
  depth: number
}

// Flatten categories for the parent select, with indentation
function flattenCategories(nodes: CategoryNode[], depth = 0): FlatOption[] {
  const result: FlatOption[] = []
  for (const node of nodes) {
    // Exclude the category being edited and its descendants
    if (props.category && node.id === props.category.id) continue
    result.push({ id: node.id, name: node.name, depth })
    if (node.children.length > 0) {
      result.push(...flattenCategories(node.children, depth + 1))
    }
  }
  return result
}

const flatOptions = computed(() => flattenCategories(props.categories))

function indentLabel(option: FlatOption): string {
  const indent = '\u00A0\u00A0\u00A0\u00A0'.repeat(option.depth)
  return indent + option.name
}

function handleSubmit() {
  if (!name.value.trim()) return

  const payload: CategoryCreatePayload = {
    name: name.value.trim(),
    parentId: parentId.value,
    description: description.value.trim() || undefined,
  }

  emit('submit', payload)
}

const isEditing = computed(() => !!props.category)
</script>

<template>
  <form class="category-form" @submit.prevent="handleSubmit" @click.stop>
    <h2>{{ isEditing ? 'Edytuj kategorię' : 'Nowa kategoria' }}</h2>

    <div class="form-field">
      <label for="cat-name">Nazwa *</label>
      <input
        id="cat-name"
        v-model="name"
        type="text"
        required
        maxlength="200"
        placeholder="Nazwa kategorii"
      />
    </div>

    <div class="form-field">
      <label for="cat-parent">Kategoria nadrzędna</label>
      <select id="cat-parent" v-model="parentId">
        <option :value="null">Brak (kategoria główna)</option>
        <option
          v-for="option in flatOptions"
          :key="option.id"
          :value="option.id"
        >
          {{ indentLabel(option) }}
        </option>
      </select>
    </div>

    <div class="form-field">
      <label for="cat-desc">Opis</label>
      <textarea
        id="cat-desc"
        v-model="description"
        rows="3"
        maxlength="1000"
        placeholder="Opcjonalny opis kategorii"
      ></textarea>
    </div>

    <div class="form-actions">
      <button type="button" class="btn-cancel" @click="$emit('cancel')">
        {{ t('common.cancel') }}
      </button>
      <button type="submit" class="btn-submit" :disabled="!name.trim()">
        {{ t('common.save') }}
      </button>
    </div>
  </form>
</template>

<style scoped>
.category-form {
  max-width: 480px;
  width: 100%;
  background-color: var(--surface-container-lowest);
  border: 1px solid var(--ghost-border);
  border-radius: var(--radius-lg);
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 20px;
  box-shadow: var(--shadow-sm);
}

.category-form h2 {
  font-family: var(--font-headline);
  font-size: var(--headline-md);
  font-weight: 400;
  color: var(--on-surface);
  margin: 0;
}

.form-field {
  display: flex;
  flex-direction: column;
}

.form-field label {
  display: block;
  font-weight: 500;
  margin-bottom: 4px;
  font-size: var(--label-lg);
  color: var(--on-surface-variant);
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
  min-height: 80px;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.btn-cancel {
  padding: 8px 16px;
  border: 1px solid var(--ghost-border);
  border-radius: var(--radius-lg);
  background-color: transparent;
  color: var(--on-surface);
  font-size: var(--label-lg);
  font-family: var(--font-body);
  font-weight: 500;
  cursor: pointer;
  min-height: 44px;
  transition: background-color var(--transition-fast);
}

.btn-cancel:hover {
  background-color: var(--surface-container-low);
}

.btn-submit {
  background-color: var(--primary);
  color: var(--on-primary);
  padding: 8px 16px;
  border: none;
  border-radius: var(--radius-lg);
  font-weight: 600;
  font-size: var(--label-lg);
  font-family: var(--font-body);
  cursor: pointer;
  min-height: 44px;
  transition: background-color var(--transition-fast);
}

.btn-submit:hover:not(:disabled) {
  background-color: var(--primary-container);
}

.btn-submit:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-cancel:focus-visible,
.btn-submit:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring);
}
</style>
