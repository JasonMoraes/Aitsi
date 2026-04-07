<script setup lang="ts">
import { computed, toRef } from 'vue'
import { useI18n } from 'vue-i18n'
import { usePagination } from '@/composables/usePagination'
import { useBreakpoint } from '@/composables/useBreakpoint'

const props = defineProps<{
  page: number
  totalPages: number
  totalCount: number
}>()

const emit = defineEmits<{
  'update:page': [page: number]
}>()

const { t } = useI18n()
const { isMobile } = useBreakpoint()

const pageRef = toRef(props, 'page')
const totalPagesRef = toRef(props, 'totalPages')

const { visiblePages, hasPrev, hasNext } = usePagination(pageRef, totalPagesRef)

const infoText = computed(() =>
  `${t('pagination.page', { page: props.page, total: props.totalPages })} (${t('pagination.count', { count: props.totalCount })})`
)

function goToPage(p: number) {
  if (p >= 1 && p <= props.totalPages && p !== props.page) {
    emit('update:page', p)
  }
}
</script>

<template>
  <nav v-if="totalPages > 1" class="pagination" aria-label="Nawigacja między stronami">
    <p class="pagination__info" aria-live="polite">{{ infoText }}</p>

    <div class="pagination__controls">
      <button
        v-if="!isMobile"
        class="pagination__btn"
        :disabled="!hasPrev"
        :aria-label="t('pagination.first')"
        @click="goToPage(1)"
      >
        {{ t('pagination.first') }}
      </button>

      <button
        class="pagination__btn"
        :disabled="!hasPrev"
        :aria-label="t('pagination.previous')"
        @click="goToPage(page - 1)"
      >
        {{ t('pagination.previous') }}
      </button>

      <template v-for="(p, index) in visiblePages" :key="index">
        <span v-if="p === '...'" class="pagination__ellipsis" aria-hidden="true">...</span>
        <button
          v-else
          class="pagination__btn pagination__btn--page"
          :class="{ 'pagination__btn--active': p === page }"
          :aria-current="p === page ? 'page' : undefined"
          :aria-label="`Strona ${p}`"
          @click="goToPage(p as number)"
        >
          {{ p }}
        </button>
      </template>

      <button
        class="pagination__btn"
        :disabled="!hasNext"
        :aria-label="t('pagination.next')"
        @click="goToPage(page + 1)"
      >
        {{ t('pagination.next') }}
      </button>

      <button
        v-if="!isMobile"
        class="pagination__btn"
        :disabled="!hasNext"
        :aria-label="t('pagination.last')"
        @click="goToPage(totalPages)"
      >
        {{ t('pagination.last') }}
      </button>
    </div>
  </nav>
</template>

<style scoped>
.pagination {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  padding: 24px 0;
}

.pagination__info {
  color: var(--on-surface-variant);
  font-family: var(--font-label);
  font-size: var(--label-lg);
  margin: 0;
}

.pagination__controls {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 4px;
}

.pagination__btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 40px;
  min-height: 40px;
  padding: 6px 10px;
  border: 1px solid var(--ghost-border);
  border-radius: var(--radius-md);
  background: var(--surface-container-lowest);
  color: var(--on-surface);
  font-size: var(--label-lg);
  font-family: var(--font-body);
  font-weight: 500;
  cursor: pointer;
  transition: all var(--transition-fast);
  white-space: nowrap;
}

.pagination__btn:hover:not(:disabled) {
  color: var(--primary);
  background: rgba(0, 70, 74, 0.05);
}

.pagination__btn:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring);
}

.pagination__btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.pagination__btn--active {
  background: var(--primary);
  color: var(--on-primary);
  border-color: var(--primary);
}

.pagination__btn--active:hover:not(:disabled) {
  background: var(--primary-container);
  color: var(--on-primary);
  border-color: var(--primary-container);
}

.pagination__ellipsis {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 40px;
  min-height: 40px;
  color: var(--on-surface-variant);
  font-size: var(--label-lg);
  user-select: none;
}
</style>
