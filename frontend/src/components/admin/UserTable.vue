<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import type { User } from '@/types'

defineProps<{
  users: User[]
  loading?: boolean
}>()

defineEmits<{
  block: [userId: number]
  unblock: [userId: number]
}>()

const { t } = useI18n()

function roleBadgeClass(role: string): string {
  switch (role) {
    case 'admin':
      return 'role-badge role-admin'
    case 'creator':
      return 'role-badge role-creator'
    default:
      return 'role-badge role-viewer'
  }
}
</script>

<template>
  <div class="user-table-wrapper">
    <!-- Loading state -->
    <div v-if="loading" class="loading-state" role="status">
      <p>{{ t('common.loading') }}</p>
    </div>

    <!-- Empty state -->
    <div v-else-if="users.length === 0" class="empty-state">
      <p>Brak użytkowników</p>
    </div>

    <!-- Table -->
    <table v-else class="user-table" role="table">
      <thead>
        <tr>
          <th>Imię</th>
          <th>Email</th>
          <th>Rola</th>
          <th>Status</th>
          <th>Akcje</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="user in users"
          :key="user.id"
          :class="{ 'blocked-row': user.isBlocked }"
          :data-label-name="user.displayName"
        >
          <td data-label="Imię">{{ user.displayName }}</td>
          <td data-label="Email">{{ user.email }}</td>
          <td data-label="Rola">
            <span :class="roleBadgeClass(user.role)">{{ user.role }}</span>
          </td>
          <td data-label="Status">
            <span v-if="user.isBlocked" class="status-blocked">Zablokowany</span>
            <span v-else class="status-active">Aktywny</span>
          </td>
          <td data-label="Akcje">
            <button
              v-if="user.isBlocked"
              class="action-btn action-unblock"
              @click="$emit('unblock', user.id)"
            >
              {{ t('admin.unblockUser') }}
            </button>
            <button
              v-else
              class="action-btn action-block"
              @click="$emit('block', user.id)"
            >
              {{ t('admin.blockUser') }}
            </button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
.user-table-wrapper {
  width: 100%;
  overflow-x: auto;
}

.loading-state,
.empty-state {
  text-align: center;
  padding: 32px 16px;
  color: var(--on-surface-variant);
  font-size: var(--label-lg);
}

.user-table {
  width: 100%;
  border-collapse: collapse;
}

.user-table thead {
  position: sticky;
  top: 0;
  background-color: var(--surface-container-lowest);
  z-index: var(--z-sticky);
}

.user-table thead th {
  text-align: left;
  padding: 12px;
  border-bottom: 2px solid var(--ghost-border);
  font-size: var(--label-lg);
  color: var(--on-surface-variant);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  font-weight: 600;
}

.user-table tbody td {
  padding: 12px;
  border-bottom: 1px solid var(--ghost-border);
  font-size: var(--label-lg);
  color: var(--on-surface);
}

.blocked-row td {
  opacity: 0.6;
}

/* Role badges */
.role-badge {
  display: inline-block;
  padding: 2px 8px;
  border: 1px solid var(--ghost-border);
  border-radius: var(--radius-md);
  font-size: var(--label-lg);
  font-weight: 600;
  text-transform: capitalize;
}

.role-admin {
  border-color: var(--error);
  color: var(--error);
}

.role-creator {
  border-color: var(--primary);
  color: var(--primary);
}

.role-viewer {
  border-color: var(--ghost-border);
  color: var(--on-surface-variant);
}

/* Status */
.status-blocked {
  color: var(--error);
  font-weight: 600;
  font-size: var(--label-lg);
}

.status-active {
  color: var(--primary);
  font-weight: 600;
  font-size: var(--label-lg);
}

/* Action buttons */
.action-btn {
  padding: 6px 12px;
  border-radius: var(--radius-md);
  font-size: var(--label-lg);
  font-weight: 600;
  font-family: var(--font-body);
  cursor: pointer;
  border: 1px solid transparent;
  min-height: 44px;
  transition: background-color var(--transition-fast), opacity var(--transition-fast);
}

.action-btn:hover {
  opacity: 0.85;
}

.action-btn:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring);
}

.action-block {
  background-color: var(--error);
  color: var(--on-error);
}

.action-unblock {
  background-color: transparent;
  border-color: var(--ghost-border);
  color: var(--on-surface);
}

.action-unblock:hover {
  background-color: var(--surface-container-low);
}

/* Responsive: card layout on mobile */
@media (max-width: 768px) {
  .user-table thead {
    display: none;
  }

  .user-table,
  .user-table tbody,
  .user-table tr,
  .user-table td {
    display: block;
    width: 100%;
  }

  .user-table tr {
    margin-bottom: 16px;
    border: 1px solid var(--ghost-border);
    border-radius: var(--radius-lg);
    padding: 12px;
    background-color: var(--surface-container-lowest);
  }

  .user-table td {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 0;
    border-bottom: none;
  }

  .user-table td:not(:last-child) {
    border-bottom: 1px solid var(--ghost-border);
  }

  .user-table td::before {
    content: attr(data-label);
    font-weight: 600;
    font-size: var(--label-lg);
    color: var(--on-surface-variant);
    text-transform: uppercase;
    letter-spacing: 0.05em;
    margin-right: 12px;
    flex-shrink: 0;
  }
}
</style>
