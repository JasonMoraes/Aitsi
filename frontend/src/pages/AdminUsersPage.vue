<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useToastStore } from '@/stores/toast.store'
import { adminApi } from '@/api/admin.api'
import type { User } from '@/types'
import UserTable from '@/components/admin/UserTable.vue'
import BlockUserDialog from '@/components/admin/BlockUserDialog.vue'

const { t } = useI18n()
const toastStore = useToastStore()

const loading = ref(false)
const blockDialogOpen = ref(false)
const blockTarget = ref<User | null>(null)
const users = ref<User[]>([])

async function loadUsers() {
  loading.value = true
  try {
    const res = await adminApi.getUsers()
    users.value = res.items
  } catch {
    toastStore.show(t('common.error'), 'error')
  } finally {
    loading.value = false
  }
}

onMounted(loadUsers)

function openBlockDialog(userId: number) {
  const user = users.value.find((u) => u.id === userId)
  if (user) {
    blockTarget.value = user
    blockDialogOpen.value = true
  }
}

async function handleBlock(reason: string) {
  if (!blockTarget.value) return
  try {
    await adminApi.blockUser(blockTarget.value.id, { reason })
    const user = users.value.find((u) => u.id === blockTarget.value!.id)
    if (user) user.isBlocked = true
    toastStore.show('Użytkownik zablokowany', 'success')
  } catch {
    toastStore.show(t('common.error'), 'error')
  }
  blockDialogOpen.value = false
  blockTarget.value = null
}

async function handleUnblock(userId: number) {
  try {
    await adminApi.unblockUser(userId)
    const user = users.value.find((u) => u.id === userId)
    if (user) user.isBlocked = false
    toastStore.show('Użytkownik odblokowany', 'success')
  } catch {
    toastStore.show(t('common.error'), 'error')
  }
}
</script>

<template>
  <div class="container">
    <h1>{{ t('admin.users') }}</h1>

    <UserTable
      :users="users"
      :loading="loading"
      @block="openBlockDialog"
      @unblock="handleUnblock"
    />

    <BlockUserDialog
      :open="blockDialogOpen"
      :user-name="blockTarget?.displayName"
      @confirm="handleBlock"
      @cancel="blockDialogOpen = false"
    />
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

.container h1 {
  font-family: var(--font-headline);
  font-size: var(--headline-lg);
  font-weight: 400;
  color: var(--on-surface);
  margin-bottom: 32px;
}
</style>
