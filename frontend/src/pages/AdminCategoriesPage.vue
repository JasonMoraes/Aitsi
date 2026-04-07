<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import type { CategoryNode, CategoryCreatePayload } from '@/types'
import { categoriesApi } from '@/api/categories.api'
import CategoryForm from '@/components/categories/CategoryForm.vue'

const { t } = useI18n()

const showCreateForm = ref(false)
const editTarget = ref<CategoryNode | null>(null)
const deleteTarget = ref<CategoryNode | null>(null)
const categories = ref<CategoryNode[]>([])
const isLoading = ref(false)

async function fetchCategories() {
  isLoading.value = true
  try {
    categories.value = await categoriesApi.getTree()
  } finally {
    isLoading.value = false
  }
}

onMounted(fetchCategories)

// Flatten categories with depth for display
interface FlatCategory {
  id: number
  name: string
  description?: string
  depth: number
  parentId?: number | null
  children: CategoryNode[]
}

function flatten(
  nodes: CategoryNode[],
  depth = 0,
): FlatCategory[] {
  const result: FlatCategory[] = []
  for (const node of nodes) {
    result.push({
      id: node.id,
      name: node.name,
      description: node.description,
      depth,
      parentId: node.parentId,
      children: node.children ?? [],
    })
    if (node.children?.length > 0) {
      result.push(...flatten(node.children, depth + 1))
    }
  }
  return result
}

const flatCategories = computed(() => flatten(categories.value))

function editCategory(cat: FlatCategory) {
  const findNode = (nodes: CategoryNode[], id: number): CategoryNode | null => {
    for (const node of nodes) {
      if (node.id === id) return node
      if (node.children?.length) {
        const found = findNode(node.children, id)
        if (found) return found
      }
    }
    return null
  }
  editTarget.value = findNode(categories.value, cat.id)
}

function confirmDeleteCategory(cat: FlatCategory) {
  deleteTarget.value = {
    id: cat.id,
    name: cat.name,
    description: cat.description,
    parentId: cat.parentId,
    children: cat.children,
  }
}

async function handleDelete() {
  if (!deleteTarget.value) return
  await categoriesApi.delete(deleteTarget.value.id)
  deleteTarget.value = null
  await fetchCategories()
}

async function handleFormSubmit(data: CategoryCreatePayload) {
  if (editTarget.value) {
    await categoriesApi.update(editTarget.value.id, data)
  } else {
    await categoriesApi.create(data)
  }
  closeForm()
  await fetchCategories()
}

function closeForm() {
  showCreateForm.value = false
  editTarget.value = null
}
</script>

<template>
  <div class="container">
    <div class="page-header">
      <h1>{{ t('admin.categories') }}</h1>
      <button class="add-btn" @click="showCreateForm = true">
        Dodaj kategorię
      </button>
    </div>

    <!-- Category tree view -->
    <div class="categories-list">
      <div
        v-for="cat in flatCategories"
        :key="cat.id"
        class="category-item"
        :style="{ paddingLeft: cat.depth * 24 + 16 + 'px' }"
      >
        <div class="category-info">
          <span class="category-name">{{ cat.name }}</span>
          <span v-if="cat.description" class="category-desc">{{ cat.description }}</span>
        </div>
        <div class="category-actions">
          <button class="btn-edit" @click="editCategory(cat)">
            {{ t('common.edit') }}
          </button>
          <button class="btn-danger-text" @click="confirmDeleteCategory(cat)">
            {{ t('common.delete') }}
          </button>
        </div>
      </div>

      <div v-if="flatCategories.length === 0" class="empty-state">
        <p>Brak kategorii</p>
      </div>
    </div>

    <!-- Create/Edit form modal -->
    <Teleport to="body">
      <div
        v-if="showCreateForm || editTarget"
        class="dialog-overlay"
        @click.self="closeForm"
        @keydown.escape="closeForm"
      >
        <CategoryForm
          :category="editTarget ?? undefined"
          :categories="categories"
          @submit="handleFormSubmit"
          @cancel="closeForm"
        />
      </div>
    </Teleport>

    <!-- Delete confirmation dialog -->
    <Teleport to="body">
      <div
        v-if="deleteTarget"
        class="confirm-dialog-overlay"
        @click.self="deleteTarget = null"
        @keydown.escape="deleteTarget = null"
      >
        <div
          class="confirm-dialog"
          role="alertdialog"
          aria-label="Potwierdź usunięcie kategorii"
        >
          <h2>Usunąć kategorię "{{ deleteTarget.name }}"?</h2>
          <p v-if="deleteTarget.children.length > 0">
            Ta kategoria zawiera podkategorie. Usunięcie jej spowoduje usunięcie wszystkich podkategorii.
          </p>
          <p v-else>Tej operacji nie można cofnąć.</p>
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
  border: none;
  font-family: var(--font-body);
  font-weight: 500;
  font-size: var(--label-lg);
  min-height: 44px;
  cursor: pointer;
  transition: background-color var(--transition-fast);
}

.add-btn:hover {
  background-color: var(--primary-container);
}

.add-btn:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring);
}

/* Categories list */
.categories-list {
  border: none;
  border-radius: var(--radius-lg);
  overflow: hidden;
  background-color: var(--surface-container-lowest);
  box-shadow: var(--shadow-sm);
}

.category-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  padding-right: 16px;
  border-bottom: 1px solid var(--ghost-border);
  min-height: 44px;
}

.category-item:last-child {
  border-bottom: none;
}

.category-info {
  flex: 1;
  min-width: 0;
  display: flex;
  align-items: baseline;
  gap: 12px;
  flex-wrap: wrap;
}

.category-name {
  font-weight: 600;
  color: var(--on-surface);
  font-size: var(--label-lg);
}

.category-desc {
  color: var(--on-surface-variant);
  font-size: var(--label-lg);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.category-actions {
  display: flex;
  gap: 8px;
  flex-shrink: 0;
}

.btn-edit {
  padding: 4px 12px;
  border: 1px solid var(--ghost-border);
  border-radius: var(--radius-md);
  background-color: transparent;
  color: var(--on-surface);
  font-size: var(--label-lg);
  font-family: var(--font-body);
  font-weight: 500;
  min-height: 32px;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.btn-edit:hover {
  background-color: var(--surface-container-low);
}

.btn-danger-text {
  padding: 4px 12px;
  border: 1px solid transparent;
  border-radius: var(--radius-md);
  background-color: transparent;
  color: var(--error);
  font-size: var(--label-lg);
  font-family: var(--font-body);
  font-weight: 500;
  min-height: 32px;
  cursor: pointer;
  transition: background-color var(--transition-fast);
}

.btn-danger-text:hover {
  background-color: var(--surface-container-low);
}

.btn-edit:focus-visible,
.btn-danger-text:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring);
}

.empty-state {
  text-align: center;
  padding: 40px 16px;
  color: var(--on-surface-variant);
  font-size: var(--label-lg);
}

/* Responsive */
@media (max-width: 640px) {
  .category-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 8px;
  }

  .category-actions {
    align-self: flex-end;
  }

  .category-info {
    flex-direction: column;
    gap: 4px;
  }
}
</style>

<style>
/* Dialog styles for teleported content -- unscoped */
.dialog-overlay {
  position: fixed;
  inset: 0;
  z-index: var(--z-modal);
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: var(--overlay);
  padding: 16px;
}

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
