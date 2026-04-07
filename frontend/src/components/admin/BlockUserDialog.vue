<script setup lang="ts">
import { ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'

const props = defineProps<{
  open: boolean
  userName?: string
}>()

const emit = defineEmits<{
  confirm: [reason: string]
  cancel: []
}>()

const { t } = useI18n()

const reason = ref('')

// Reset reason when dialog opens
watch(
  () => props.open,
  (isOpen) => {
    if (isOpen) {
      reason.value = ''
    }
  },
)

function handleConfirm() {
  if (reason.value.trim()) {
    emit('confirm', reason.value.trim())
  }
}
</script>

<template>
  <Teleport to="body">
    <div
      v-if="open"
      class="dialog-overlay"
      @click.self="$emit('cancel')"
      @keydown.escape="$emit('cancel')"
    >
      <div
        class="dialog"
        role="alertdialog"
        aria-modal="true"
        :aria-label="'Zablokuj ' + userName"
      >
        <h2>Zablokuj użytkownika {{ userName }}</h2>
        <p>Zablokowany użytkownik nie będzie mógł dodawać nowych materiałów.</p>

        <div class="form-field">
          <label for="block-reason">Powód blokady *</label>
          <textarea
            id="block-reason"
            v-model="reason"
            rows="3"
            required
            placeholder="Opisz powód blokady..."
          ></textarea>
        </div>

        <div class="dialog-actions">
          <button class="btn-secondary" @click="$emit('cancel')">
            {{ t('common.cancel') }}
          </button>
          <button
            class="btn-danger"
            :disabled="!reason.trim()"
            @click="handleConfirm"
          >
            {{ t('admin.blockUser') }}
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<style>
/* Dialog styles are unscoped since they are teleported to body */
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

.dialog {
  max-width: 480px;
  width: 100%;
  background-color: var(--surface-container-lowest);
  border: 1px solid var(--ghost-border);
  border-radius: var(--radius-lg);
  padding: 24px;
  box-shadow: var(--shadow-lg);
}

.dialog h2 {
  font-family: var(--font-headline);
  font-size: var(--title-lg);
  font-weight: 400;
  color: var(--on-surface);
  margin: 0 0 8px;
}

.dialog p {
  color: var(--on-surface-variant);
  font-size: var(--label-lg);
  margin: 0 0 24px;
  line-height: 1.5;
}

.dialog .form-field {
  display: flex;
  flex-direction: column;
  margin-bottom: 24px;
}

.dialog .form-field label {
  display: block;
  font-weight: 500;
  margin-bottom: 4px;
  font-size: var(--label-lg);
  color: var(--on-surface-variant);
}

.dialog .form-field textarea {
  width: 100%;
  padding: 8px 12px;
  border: 2px solid transparent;
  border-radius: var(--radius-md);
  background-color: var(--surface-container-low);
  color: var(--on-surface);
  font-size: var(--body-md);
  font-family: var(--font-body);
  resize: vertical;
  min-height: 80px;
}

.dialog .form-field textarea:focus {
  outline: none;
  background-color: var(--surface-container-lowest);
  border-color: var(--primary);
  box-shadow: var(--focus-ring);
}

.dialog-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.dialog-actions .btn-secondary {
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

.dialog-actions .btn-secondary:hover {
  background-color: var(--surface-container-low);
}

.dialog-actions .btn-danger {
  padding: 8px 16px;
  border: none;
  border-radius: var(--radius-lg);
  background-color: var(--error);
  color: var(--on-error);
  font-size: var(--label-lg);
  font-family: var(--font-body);
  font-weight: 600;
  cursor: pointer;
  min-height: 44px;
  transition: opacity var(--transition-fast);
}

.dialog-actions .btn-danger:hover:not(:disabled) {
  opacity: 0.9;
}

.dialog-actions .btn-danger:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.dialog-actions button:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring);
}
</style>
