<script setup lang="ts">
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
</script>

<template>
  <div class="not-found">
    <div class="not-found__inner">
      <span class="not-found__code" aria-hidden="true">404</span>
      <h1 class="not-found__title">{{ t('common.notFound') }}</h1>
      <p class="not-found__message">{{ t('common.notFoundMessage') }}</p>
      <RouterLink to="/przegladaj" class="btn btn-primary">
        Wróć na stronę główną
      </RouterLink>
    </div>
  </div>
</template>

<style scoped>
.not-found {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 70vh;
  padding: 48px 16px;
  text-align: center;
}

.not-found__inner {
  position: relative;
  max-width: 500px;
}

.not-found__code {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  font-family: var(--font-headline);
  font-size: clamp(6rem, 18vw, 12rem);
  line-height: 1;
  color: var(--on-surface);
  opacity: 0.05;
  z-index: -1;
  user-select: none;
}

.not-found__title {
  position: relative;
  font-family: var(--font-headline);
  font-size: var(--headline-lg);
  color: var(--on-surface);
  margin-bottom: 8px;
}

.not-found__message {
  position: relative;
  color: var(--on-surface-variant);
  line-height: 1.6;
  margin: 0 auto 32px;
  max-width: 380px;
}
</style>
