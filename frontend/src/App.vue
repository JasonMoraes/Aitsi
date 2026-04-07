<script setup lang="ts">
import { useRoute } from 'vue-router'
import AppHeader from '@/components/layout/AppHeader.vue'
import AppFooter from '@/components/layout/AppFooter.vue'
import AppToast from '@/components/common/AppToast.vue'
import { useAnnouncer } from '@/composables/useAnnouncer'

const route = useRoute()
const { announcement, politeness } = useAnnouncer()
</script>

<template>
  <a href="#main-content" class="skip-link">Przejdź do treści</a>
  <AppHeader />
  <main id="main-content" tabindex="-1">
    <router-view v-slot="{ Component }">
      <transition name="page" mode="out-in">
        <component :is="Component" />
      </transition>
    </router-view>
  </main>
  <AppFooter v-if="!route.meta.hideFooter" />

  <div class="sr-only" :aria-live="politeness" aria-atomic="true" role="status">
    {{ announcement }}
  </div>

  <AppToast />
</template>

<style>
.skip-link {
  position: absolute;
  top: -100%;
  left: 8px;
  padding: 8px 16px;
  background: var(--primary);
  color: var(--on-primary);
  border-radius: var(--radius-md);
  font-family: var(--font-label);
  font-weight: 600;
  text-decoration: none;
  z-index: 9999;
  transition: top 0.1s;
}

.skip-link:focus {
  top: 8px;
}

main {
  outline: none;
  min-height: calc(100vh - 56px - 96px);
  min-height: calc(100dvh - 56px - 96px);
}

.page-enter-active,
.page-leave-active {
  transition: opacity var(--transition-normal, 250ms) ease;
}

.page-enter-from,
.page-leave-to {
  opacity: 0;
}

@media (prefers-reduced-motion: reduce) {
  .page-enter-active,
  .page-leave-active {
    transition: none;
  }
}
</style>
