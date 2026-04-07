<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/stores/auth.store'

const { t } = useI18n()
const authStore = useAuthStore()
</script>

<template>
  <nav role="navigation" aria-label="Menu główne">
    <ul class="nav-list">
      <li>
        <router-link to="/przegladaj" class="nav-link">
          {{ t('nav.browse') }}
        </router-link>
      </li>
      <li>
        <router-link to="/kroniki" class="nav-link">
          {{ t('nav.feed') }}
        </router-link>
      </li>
      <li>
        <router-link to="/mapa" class="nav-link">
          {{ t('nav.map') }}
        </router-link>
      </li>
      <li v-if="authStore.isCreator">
        <router-link to="/panel-tworcy" class="nav-link">
          {{ t('nav.creatorDashboard') }}
        </router-link>
      </li>
      <li v-if="authStore.isAdmin">
        <router-link to="/admin" class="nav-link">
          {{ t('nav.admin') }}
        </router-link>
      </li>
    </ul>
  </nav>
</template>

<style scoped>
.nav-list {
  display: flex;
  align-items: center;
  gap: 4px;
  list-style: none;
  margin: 0;
  padding: 0;
}

.nav-link {
  display: inline-flex;
  align-items: center;
  min-height: 36px;
  padding: 4px 12px;
  color: var(--on-surface-variant);
  text-decoration: none;
  font-family: var(--font-label);
  font-size: var(--label-lg);
  font-weight: 500;
  letter-spacing: 0.03em;
  border-radius: var(--radius-md);
  transition: color var(--transition-fast), background var(--transition-fast);
}

.nav-link:hover {
  color: var(--primary);
  background: var(--surface-container);
}

.nav-link:focus-visible {
  outline: var(--focus-ring);
  outline-offset: 2px;
}

.nav-link.router-link-active {
  color: var(--primary);
  background: transparent;
  box-shadow: inset 0 -2px 0 var(--primary);
}
</style>
