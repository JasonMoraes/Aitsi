<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/stores/auth.store'
import { authApi } from '@/api/auth.api'
import { useToastStore } from '@/stores/toast.store'

const router = useRouter()
const { t } = useI18n()
const authStore = useAuthStore()
const toastStore = useToastStore()

const loading = ref(false)
const error = ref('')

// --- Facebook ---
async function handleFbToken(accessToken: string) {
  try {
    const result = await authApi.loginWithFacebook(accessToken)
    authStore.setAuth(result.token, result.user)
    toastStore.show('Zalogowano pomyslnie!', 'success')
    router.push({ name: 'browse' })
  } catch (e: any) {
    try {
      const body = await e?.response?.json()
      error.value = body?.message ?? t('common.error')
    } catch {
      error.value = t('common.error')
    }
  }
  loading.value = false
}

function loginWithFacebook() {
  loading.value = true
  error.value = ''

  window.FB.login((response) => {
    if (response.authResponse) {
      handleFbToken(response.authResponse.accessToken)
    } else {
      error.value = 'Logowanie anulowane'
      loading.value = false
    }
  }, { scope: 'public_profile' })
}

// --- Google ---
const googleBtnRef = ref<HTMLElement | null>(null)

onMounted(() => {
  const checkGoogle = setInterval(() => {
    if (window.google?.accounts?.id && googleBtnRef.value) {
      clearInterval(checkGoogle)
      window.google.accounts.id.initialize({
        client_id: '951114717412-mbnurf4ap3kh9egfi2lbpilcmnprfp21.apps.googleusercontent.com',
        callback: async (response) => {
          loading.value = true
          try {
            const result = await authApi.loginWithGoogle(response.credential)
            authStore.setAuth(result.token, result.user)
            toastStore.show('Zalogowano pomyslnie!', 'success')
            router.push({ name: 'browse' })
          } catch (e: any) {
            try {
              const body = await e?.response?.json()
              error.value = body?.message ?? t('common.error')
            } catch {
              error.value = t('common.error')
            }
          }
          loading.value = false
        }
      })
      window.google.accounts.id.renderButton(googleBtnRef.value, {
        theme: 'outline',
        size: 'large',
        text: 'signin_with',
        width: 400,
        shape: 'rectangular'
      })
    }
  }, 100)
})
</script>

<template>
  <div class="login-page">
    <div class="login-content">
      <h1 class="login__title">{{ t('auth.loginTitle') }}</h1>
      <p class="login__sub">{{ t('auth.loginSubtitle') }}</p>

      <div class="login__buttons">
        <div ref="googleBtnRef" class="login__google-btn"></div>

        <div class="login__divider" aria-hidden="true">
          <span class="login__divider-line"></span>
          <span class="login__divider-text">lub</span>
          <span class="login__divider-line"></span>
        </div>

        <button class="login__btn" :disabled="loading" @click="loginWithFacebook">
          <svg width="20" height="20" viewBox="0 0 24 24" aria-hidden="true">
            <path fill="currentColor" d="M24 12.073c0-6.627-5.373-12-12-12S0 5.446 0 12.073c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z"/>
          </svg>
          {{ t('auth.loginFacebook') }}
        </button>
      </div>

      <p v-if="error" class="login__error" role="alert">{{ error }}</p>
    </div>
  </div>
</template>

<style scoped>
.login-page {
  min-height: 70vh;
  padding: 16px;
}

.login-content {
  max-width: 400px;
  margin: 0 auto;
  padding-top: 80px;
}

.login__title {
  font-family: var(--font-headline);
  font-size: var(--headline-lg);
  color: var(--on-surface);
  margin-bottom: 8px;
}

.login__sub {
  color: var(--on-surface-variant);
  margin-bottom: 32px;
  line-height: 1.6;
}

.login__buttons {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.login__btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  width: 100%;
  min-height: 48px;
  padding: 12px 16px;
  background: var(--surface-container-lowest);
  color: var(--on-surface);
  border: 1px solid var(--ghost-border);
  border-radius: var(--radius-lg);
  font-size: var(--body-md);
  font-weight: 500;
  font-family: inherit;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.login__btn:not(:disabled):hover {
  border-color: var(--primary);
}

.login__btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.login__btn:focus-visible {
  box-shadow: var(--focus-ring);
}

.login__divider {
  display: flex;
  align-items: center;
  gap: 12px;
}

.login__divider-line {
  flex: 1;
  height: 1px;
  background: var(--outline-variant);
}

.login__divider-text {
  font-size: var(--label-lg);
  color: var(--on-surface-variant);
}

.login__error {
  margin-top: 20px;
  color: var(--error);
  font-size: var(--label-lg);
  font-weight: 500;
}

@media (max-width: 480px) {
  .login-content { padding-top: 48px; }
  .login__title { font-size: var(--headline-md); }
}
</style>
