<script setup lang="ts">
import { onMounted, computed, ref, watch } from 'vue'
import { useRouter, RouterLink } from 'vue-router'
import { useCategoriesStore } from '@/stores/categories.store'
import { categoriesApi } from '@/api/categories.api'

const showHowItWorks = ref(false)

const router = useRouter()
const categoriesStore = useCategoriesStore()

onMounted(() => {
  categoriesStore.fetchTree()
})

const bentoCategories = computed(() => categoriesStore.topCategories.slice(0, 4))

const coverImages = ref<Record<number, string>>({})

watch(bentoCategories, async (cats) => {
  if (!cats.length) return
  const results = await Promise.all(
    cats.map(cat => categoriesApi.getPhotos(cat.id, { pageSize: 1 }).catch(() => null))
  )
  for (let i = 0; i < cats.length; i++) {
    const firstPhoto = results[i]?.items?.[0]
    if (firstPhoto) {
      coverImages.value[cats[i].id] = firstPhoto.thumbnailUrl
    }
  }
}, { immediate: true })

const decades = [
  { year: '1920s', label: 'Lata międzywojenne', dateFrom: '1920', dateTo: '1929', img: 'https://lh3.googleusercontent.com/aida-public/AB6AXuCOsUqjKwlrpy5dOn1CeigljtKA09VXmSyRmaoS8kybikoVyhQjTmToKNNAaSOSHz-DxZnKxdsYy3Tk3qi9TTvwFSw60-bD4Qf7Rho10Jq75eaI-hwVcch7rRBD82lSwAKsyZjrGjNIkt-CvP0pRphhtTSXYU91SEtwoygnkP0X-_coxhU5yfFmLSvglqcURbmfFuDMlwWwNEre27Xjws0M1oLiz13eDcy1EzJ5H0TZD5FiJXD1LAmLQWExpoN9ocLYBb1beC3bnDOD', filter: 'grayscale' },
  { year: '1950s', label: 'Czas odbudowy', dateFrom: '1950', dateTo: '1959', img: 'https://lh3.googleusercontent.com/aida-public/AB6AXuDSw44O0fG3uAxvu64oHuWk0YLhspRBeGekN32Vq8LDG5-u7Yp0pNWjhdLclzqt5kvZckNk_pFdfxmbmUD_UwByDn0gHMgcn3KV3LH9emj7IHNzn5hy-qXAlNqhkZATPFHxONheC4I73Xr68TW0XP_cg_Tf-7VnosbU_uKpxvzk1SxS2pMtZaaRn7xeRuxaNB75s6Hd6FZZfsPrImd1e2Xq5iIK_TMJwFjg0Ifsnl9XbAcA5r1I146hEb_3Ni_TFTbWi_ori_2SYUNH', filter: 'sepia' },
  { year: '1970s', label: 'Kultura i wolność', dateFrom: '1970', dateTo: '1979', img: 'https://lh3.googleusercontent.com/aida-public/AB6AXuANzAt2DmCCunmTiCaki7uSeiASeFUOELX7oYO8WlbNBN0pvlYeRYZhs4bvlwraXBp0e8kPFjooDdbT8xR79vNZN2haMEXvUwS67By7mU9tLTwGj62QQAhmFcDGFmdJsaeGCwbMek5CDC-jHFJuDvW_r0ffqfMOzP1InUeyWpxOKk5uiRxrrxlo0mqj9z6yh0F2iN1_GXcZPlpA20t0exjowSnkKVKE2JVqMZUI6QNo0jC-eWcOfa4ZDhE3okh6PaLaNLOIsmHR83Jq', filter: '' },
  { year: '1990s', label: 'Przełom cyfrowy', dateFrom: '1990', dateTo: '1999', img: 'https://lh3.googleusercontent.com/aida-public/AB6AXuCeG0KGopHjZpuebjt7Hm2jUtPXziOJR-rW83Rknnd-S6KhUFCGn-Jmh143rjSNlUSCdosFHlCj57eG4Tz5wV3H1bqjIxOsYr445iEwxg7J_8FfnUjtQYqf-RdH0B1ogAvVcJnPFoPBJTF7jHPRSjuPx2eLgZlqNjHP12Z6T5KBOGV35Xntw4n_w4cFCakLk4U-7FEpAb_ZF9AEfLv0QroCjhk1UqQbXfMD6jhjv2-grY4PZLzhLvrEMChiFn6nIFeGVGnZAN8B1lX1', filter: '' },
  { year: 'Dziś', label: 'Nowoczesne archiwum', dateFrom: '2000', dateTo: '2030', img: 'https://lh3.googleusercontent.com/aida-public/AB6AXuAwmICZ917ne8cIQoDE7d6G-KdV1zjGjsvndqCExaD9s-X9jY7ujII4H4se29w-JKh_eWOL4pACr3Uj6HrB1XahzqFuXE7m8lTvMBTIA0o1qmZeNCk03mkv8bUG8ItAR0sipOUpm6iWaSBgKJimsId0Z69kuYXKWnIXlFuT3M_yFoFmcVjJ0oFKVn5qvwe4lXR7Npw9fKOl2V4lK0qftLk4h6DaYJfj0arwftmFyDgEgOT8-rtQsN7f1NLg42VB57M3umPF11zy3LZM', filter: '' }
]

function goToArchive() {
  router.push({ name: 'browse' })
}

function goToUpload() {
  router.push({ name: 'upload-photo' })
}

function goToBrowse() {
  router.push({ name: 'browse' })
}
</script>

<template>
  <div class="landing">
    <main>
      <!-- ============================================================
           HERO — Editorial asymmetric layout
           ============================================================ -->
      <section class="hero">
        <div class="hero__inner">
          <div class="hero__content">
            <span class="hero__badge">Pamięć Pokoleń</span>
            <h1 class="hero__title">
              Odkryj historię <br/>
              <span class="hero__title-accent">swojej społeczności</span>
            </h1>
            <p class="hero__subtitle">
              Przeglądaj tysiące archiwalnych fotografii, listów i wspomnień, które tworzą unikalny portret naszej wspólnej tożsamości.
            </p>
            <div class="hero__actions">
              <button class="btn btn-primary hero__cta" @click="goToArchive">
                Rozpocznij zwiedzanie
              </button>
              <button class="btn btn-secondary hero__cta" @click="goToUpload">
                <span class="material-symbols-outlined">add_a_photo</span>
                Dodaj własne wspomnienie
              </button>
            </div>
          </div>

          <div class="hero__images">
            <div class="hero__image-main">
              <img
                src="https://lh3.googleusercontent.com/aida-public/AB6AXuCI1hbCONudFr5Q1A52CXcTlV957OzST5XnK4gfEBd4XF4mwEFtKLOcIF_1owzy0DqA4aFYvl-S9_AiKOVgQJ7ctQtayqrIQDE_kQ2RPGqjpx_8omJPwfAXbVkp9K1Bf1aRZ0FpszXpIUBi7qOOxOQKX6nByBJLE6gfCr8V1Pxjbk3smYF6s2AOQ_lbupLLe9lx4NGcadNk10AP1J1ZiCoPaTJy9YtUD_VqTxLjYdUBf1jQR2gslAQfYVaOdP50m9devxywcw9N0uFR"
                alt="Historyczne zdjecie spolecznosci"
              />
            </div>
            <div class="hero__image-secondary">
              <img
                src="https://lh3.googleusercontent.com/aida-public/AB6AXuCWg1ET3lpqHqwuujHw1e0QfoIOcnwlTDEUIKaBX4I9tELjxa2zfFn0qE6O248eanDRPAFcN2k4OiG8i4M_R8-UiECOQvWnawVxSDVLuuUMG3NrJFhwsdLXeS9WwlH3Gb_Fa9v0dPoswxmx-sBaGg0OorjzvBswtD8cn-cd9FYaOyn33C92cED-hm3aSqKkh5OJsvgMxKj0fHxgeKykZIX3jsyCvrZmuAISBhyX7u8vIh_ymik4qGyLbjtJK6Ovbvy2Z8OFuSYp1ho6"
                alt="Archiwalny portret"
              />
            </div>
          </div>
        </div>
      </section>

      <!-- ============================================================
           TIMELINE — "Journey through time"
           ============================================================ -->
      <section class="timeline">
        <div class="timeline__inner">
          <div class="timeline__header">
            <div>
              <h2 class="timeline__title">Podróż przez czas</h2>
            </div>
            <button class="timeline__see-all" @click="goToBrowse">Zobacz całą oś czasu</button>
          </div>

          <div class="timeline__track">
            <div class="timeline__line" aria-hidden="true"></div>
            <div class="timeline__grid">
              <RouterLink
                v-for="decade in decades"
                :key="decade.year"
                :to="{ name: 'browse', query: { dateFrom: decade.dateFrom, dateTo: decade.dateTo } }"
                class="timeline__item"
              >
                <div class="timeline__card">
                  <img :src="decade.img" :alt="decade.year" :class="['timeline__card-img', decade.filter ? `timeline__card-img--${decade.filter}` : '']" />
                </div>
                <div class="timeline__dot" aria-hidden="true"></div>
                <span class="timeline__year">{{ decade.year }}</span>
                <span class="timeline__label">{{ decade.label }}</span>
              </RouterLink>
            </div>
          </div>
        </div>
      </section>

      <!-- ============================================================
           FEATURED COLLECTIONS — Bento Grid
           ============================================================ -->
      <section class="collections">
        <div class="collections__inner">
          <div class="collections__header">
            <h2 class="collections__title">Wyróżnione Kolekcje</h2>
            <p class="collections__subtitle">Starannie dobrane zbiory tematyczne przygotowane przez naszych kuratorów i społeczność.</p>
          </div>

          <div class="bento" v-if="bentoCategories.length">
            <!-- Main card -->
            <RouterLink
              v-if="bentoCategories[0]"
              :to="{ name: 'browse-category', params: { categoryId: bentoCategories[0].id } }"
              class="bento__card bento__card--main"
              :class="{ 'bento__card--no-img': !coverImages[bentoCategories[0].id] }"
            >
              <img v-if="coverImages[bentoCategories[0].id]" :src="coverImages[bentoCategories[0].id]" :alt="bentoCategories[0].name" class="bento__img" />
              <div class="bento__gradient bento__gradient--primary"></div>
              <div class="bento__content bento__content--large">
                <span class="bento__badge">Najpopularniejsze</span>
                <h3 class="bento__card-title bento__card-title--large">{{ bentoCategories[0].name }}</h3>
              </div>
            </RouterLink>

            <!-- Small card top-right -->
            <RouterLink
              v-if="bentoCategories[1]"
              :to="{ name: 'browse-category', params: { categoryId: bentoCategories[1].id } }"
              class="bento__card bento__card--sm-top"
              :class="{ 'bento__card--no-img': !coverImages[bentoCategories[1].id] }"
            >
              <img v-if="coverImages[bentoCategories[1].id]" :src="coverImages[bentoCategories[1].id]" :alt="bentoCategories[1].name" class="bento__img" />
              <div class="bento__gradient bento__gradient--dark"></div>
              <div class="bento__content">
                <h3 class="bento__card-title">{{ bentoCategories[1].name }}</h3>
              </div>
            </RouterLink>

            <!-- Small card mid-right -->
            <RouterLink
              v-if="bentoCategories[2]"
              :to="{ name: 'browse-category', params: { categoryId: bentoCategories[2].id } }"
              class="bento__card bento__card--sm-mid"
              :class="{ 'bento__card--no-img': !coverImages[bentoCategories[2].id] }"
            >
              <img v-if="coverImages[bentoCategories[2].id]" :src="coverImages[bentoCategories[2].id]" :alt="bentoCategories[2].name" class="bento__img" />
              <div class="bento__gradient bento__gradient--dark"></div>
              <div class="bento__content">
                <h3 class="bento__card-title">{{ bentoCategories[2].name }}</h3>
              </div>
            </RouterLink>

            <!-- Wide card bottom -->
            <RouterLink
              v-if="bentoCategories[3]"
              :to="{ name: 'browse-category', params: { categoryId: bentoCategories[3].id } }"
              class="bento__card bento__card--wide"
              :class="{ 'bento__card--no-img': !coverImages[bentoCategories[3].id] }"
            >
              <img v-if="coverImages[bentoCategories[3].id]" :src="coverImages[bentoCategories[3].id]" :alt="bentoCategories[3].name" class="bento__img" />
              <div class="bento__gradient bento__gradient--primary"></div>
              <div class="bento__content bento__content--wide">
                <h3 class="bento__card-title bento__card-title--med">{{ bentoCategories[3].name }}</h3>
              </div>
            </RouterLink>
          </div>

          <!-- Empty state when no categories exist yet -->
          <div v-else-if="!categoriesStore.isLoading" class="collections__empty">
            <span class="material-symbols-outlined">photo_library</span>
            <p>Kolekcje pojawią się tutaj po dodaniu kategorii i zdjęć.</p>
            <RouterLink to="/admin/kategorie" class="btn btn-outline">Zarządzaj kategoriami</RouterLink>
          </div>
        </div>
      </section>

      <!-- ============================================================
           CTA — Community Contribution
           ============================================================ -->
      <section class="cta-section">
        <div class="cta-section__inner">
          <div class="cta-section__card">
            <span class="material-symbols-outlined cta-section__icon">history_edu</span>
            <h2 class="cta-section__title">Zostań współtwórcą historii</h2>
            <p class="cta-section__text">
              Każda fotografia, każda opowieść i każdy drobny przedmiot ma znaczenie. Nie pozwól, aby Twoje wspomnienia wyblakły – podziel się nimi z przyszłymi pokoleniami.
            </p>
            <div class="cta-section__actions">
              <button class="btn btn-primary cta-section__btn" @click="goToUpload">
                Dodaj materiały teraz
              </button>
              <button class="btn btn-outline cta-section__btn" @click="showHowItWorks = true">
                Jak to działa?
              </button>
            </div>
            <p class="cta-section__disclaimer">
              Twoje dane są u nas bezpieczne. Działamy zgodnie z najwyższymi standardami cyfrowej archiwizacji.
            </p>
          </div>
        </div>
      </section>
    </main>

    <!-- ============================================================
         FOOTER
         ============================================================ -->
    <footer class="lfooter">
      <div class="lfooter__inner">
        <div class="lfooter__grid">
          <div class="lfooter__brand-col">
            <div class="lfooter__brand">Cyfrowe Archiwum</div>
            <p class="lfooter__desc">
              Niezależna platforma cyfrowa dedykowana zachowaniu dziedzictwa kulturowego i społecznego lokalnych społeczności. Razem budujemy żywe muzeum wspomnień.
            </p>
          </div>

          <div class="lfooter__links-col">
            <h4 class="lfooter__links-heading">Eksploruj</h4>
            <ul class="lfooter__links">
              <li><RouterLink to="/przegladaj">Wszystkie kolekcje</RouterLink></li>
              <li><RouterLink to="/mapa">Interaktywna mapa</RouterLink></li>
              <li><RouterLink to="/kroniki">Kroniki społeczności</RouterLink></li>
              <li><RouterLink to="/przegladaj">Przeglądaj archiwum</RouterLink></li>
            </ul>
          </div>
        </div>

        <div class="lfooter__copyright">
          <p>© 2026 Cyfrowe Archiwum. Wszystkie prawa zastrzeżone. Projekt realizowany w trosce o wspólną pamięć.</p>
        </div>
      </div>
    </footer>

    <!-- ============================================================
         HOW IT WORKS — Modal
         ============================================================ -->
    <Teleport to="body">
      <div
        v-if="showHowItWorks"
        class="dialog-overlay"
        @click.self="showHowItWorks = false"
        @keydown.escape="showHowItWorks = false"
      >
        <div class="dialog how-dialog" role="dialog" aria-modal="true" aria-label="Jak to działa?">
          <div class="how-dialog__header">
            <span class="material-symbols-outlined how-dialog__icon">lightbulb</span>
            <h2>Jak to działa?</h2>
          </div>

          <ol class="how-dialog__steps">
            <li>
              <strong>Przeglądaj archiwum</strong>
              <span>Szukaj zdjęć po kategoriach, dekadach lub na interaktywnej mapie.</span>
            </li>
            <li>
              <strong>Załóż konto</strong>
              <span>Darmowa rejestracja pozwala dodawać własne materiały.</span>
            </li>
            <li>
              <strong>Dodaj wspomnienie</strong>
              <span>Prześlij zdjęcie, opisz je i oznacz na mapie — to zajmuje chwilę.</span>
            </li>
            <li>
              <strong>Buduj historię razem z nami</strong>
              <span>Twoje materiały trafiają do wspólnego archiwum, dostępnego dla wszystkich.</span>
            </li>
          </ol>

          <div class="how-dialog__actions">
            <button class="btn btn-primary how-dialog__btn" @click="showHowItWorks = false">
              Rozumiem!
            </button>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- ============================================================
         MOBILE BOTTOM NAV
         ============================================================ -->
    <nav class="mobile-bottom-nav" aria-label="Nawigacja mobilna">
      <RouterLink to="/przegladaj" class="mobile-bottom-nav__item">
        <span class="material-symbols-outlined">search</span>
        <span class="mobile-bottom-nav__label">Przeglądaj</span>
      </RouterLink>
      <RouterLink to="/mapa" class="mobile-bottom-nav__item">
        <span class="material-symbols-outlined">map</span>
        <span class="mobile-bottom-nav__label">Mapa</span>
      </RouterLink>
      <RouterLink to="/dodaj-zdjecie" class="mobile-bottom-nav__item">
        <span class="material-symbols-outlined">add_a_photo</span>
        <span class="mobile-bottom-nav__label">Dodaj</span>
      </RouterLink>
      <RouterLink to="/moje-zdjecia" class="mobile-bottom-nav__item">
        <span class="material-symbols-outlined">person</span>
        <span class="mobile-bottom-nav__label">Profil</span>
      </RouterLink>
    </nav>
  </div>
</template>

<style scoped>
/* ==========================================================================
   LANDING PAGE — Modern Heritage Editorial
   ========================================================================== */

.landing {
  background: var(--surface);
}

/* ==========================================================================
   HERO
   ========================================================================== */
.hero {
  background: var(--surface);
  overflow: hidden;
  padding: 96px 0 128px;
}

@media (max-width: 768px) {
  .hero { padding: 48px 0 64px; }
}

.hero__inner {
  max-width: var(--container-max);
  margin: 0 auto;
  padding: 0 32px;
  display: grid;
  grid-template-columns: 1fr;
  gap: 48px;
  align-items: center;
}

@media (min-width: 768px) {
  .hero__inner {
    grid-template-columns: 7fr 5fr;
    gap: 48px;
  }
}

.hero__content {
  display: flex;
  flex-direction: column;
  gap: 32px;
}

.hero__badge {
  display: inline-block;
  align-self: flex-start;
  padding: 6px 16px;
  background: rgba(160, 65, 0, 0.1);
  color: var(--secondary);
  font-family: var(--font-label);
  font-weight: 700;
  font-size: var(--label-sm);
  letter-spacing: 0.1em;
  text-transform: uppercase;
  border-radius: var(--radius-sm);
}

.hero__title {
  font-family: var(--font-headline);
  font-size: clamp(2.5rem, 7vw, 5rem);
  font-weight: 500;
  font-style: italic;
  line-height: 1.1;
  color: var(--on-surface);
  margin: 0;
}

.hero__title-accent {
  font-style: normal;
  color: var(--primary);
}

.hero__subtitle {
  font-family: var(--font-body);
  font-size: clamp(1.125rem, 2vw, 1.5rem);
  color: var(--on-surface-variant);
  max-width: 640px;
  line-height: 1.6;
  margin: 0;
}

.hero__actions {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
  padding-top: 8px;
}

.hero__cta {
  padding: 16px 32px;
  border-radius: var(--radius-lg);
  font-size: var(--body-md);
}

.hero__images {
  position: relative;
  display: none;
}

@media (min-width: 768px) {
  .hero__images { display: block; }
}

.hero__image-main {
  aspect-ratio: 4 / 5;
  border-radius: var(--radius-xl);
  overflow: hidden;
  box-shadow: var(--shadow-lg);
  transform: rotate(3deg);
  position: relative;
  z-index: 2;
  border: 8px solid var(--surface-container-lowest);
}

.hero__image-main img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  filter: grayscale(100%) sepia(0.3);
}

.hero__image-secondary {
  position: absolute;
  bottom: -40px;
  left: -40px;
  width: 256px;
  aspect-ratio: 1;
  border-radius: var(--radius-xl);
  overflow: hidden;
  box-shadow: var(--shadow-md);
  transform: rotate(-6deg);
  z-index: 3;
  border: 4px solid var(--surface-container-lowest);
}

.hero__image-secondary img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  filter: grayscale(100%);
}

/* ==========================================================================
   TIMELINE
   ========================================================================== */
.timeline {
  background: var(--surface-container-low);
  padding: 96px 0;
}

.timeline__inner {
  max-width: var(--container-max);
  margin: 0 auto;
  padding: 0 32px;
}

.timeline__header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  margin-bottom: 64px;
  flex-wrap: wrap;
  gap: 16px;
}

.timeline__title {
  font-family: var(--font-headline);
  font-size: clamp(2rem, 4vw, 3rem);
  font-style: italic;
  margin: 0 0 8px;
}

.timeline__subtitle {
  font-family: var(--font-body);
  color: var(--on-surface-variant);
  margin: 0;
}

.timeline__see-all {
  background: none;
  border: none;
  color: var(--primary);
  font-family: var(--font-label);
  font-weight: 700;
  font-size: var(--label-lg);
  text-decoration: underline;
  text-underline-offset: 8px;
  text-decoration-color: rgba(0, 70, 74, 0.2);
  cursor: pointer;
  transition: text-decoration-color var(--transition-fast);
}

.timeline__see-all:hover {
  text-decoration-color: var(--primary);
}

.timeline__track {
  position: relative;
}

.timeline__line {
  display: none;
  position: absolute;
  top: 50%;
  left: 0;
  width: 100%;
  height: 1px;
  background: rgba(190, 200, 201, 0.3);
  transform: translateY(-50%);
}

@media (min-width: 768px) {
  .timeline__line { display: block; }
}

.timeline__grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 32px;
}

@media (min-width: 640px) {
  .timeline__grid { grid-template-columns: repeat(2, 1fr); }
}

@media (min-width: 1024px) {
  .timeline__grid { grid-template-columns: repeat(5, 1fr); }
}

.timeline__item {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  text-decoration: none;
  color: inherit;
}

.timeline__card {
  width: 100%;
  aspect-ratio: 1;
  background: var(--surface-container-lowest);
  border-radius: var(--radius-lg);
  overflow: hidden;
  margin-bottom: 24px;
  border: 1px solid rgba(190, 200, 201, 0.1);
  box-shadow: var(--shadow-sm);
  transition: transform var(--transition-normal);
}

.timeline__item:hover .timeline__card {
  transform: translateY(-8px);
}

.timeline__card-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.timeline__card-img--grayscale {
  filter: grayscale(100%);
}

.timeline__card-img--sepia {
  filter: sepia(0.2);
}

.timeline__dot {
  display: none;
  width: 16px;
  height: 16px;
  background: var(--primary);
  border-radius: var(--radius-full);
  border: 4px solid var(--surface-container-low);
  margin-bottom: 16px;
  z-index: 2;
}

@media (min-width: 768px) {
  .timeline__dot { display: block; }
}

.timeline__year {
  font-family: var(--font-headline);
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--on-surface);
}

.timeline__label {
  font-family: var(--font-label);
  font-size: var(--body-sm);
  color: var(--on-surface-variant);
}

/* ==========================================================================
   COLLECTIONS — Bento Grid
   ========================================================================== */
.collections {
  background: var(--surface);
  padding: 96px 0;
}

.collections__inner {
  max-width: var(--container-max);
  margin: 0 auto;
  padding: 0 32px;
}

.collections__header {
  text-align: center;
  margin-bottom: 80px;
}

.collections__title {
  font-family: var(--font-headline);
  font-size: clamp(2rem, 4vw, 3rem);
  font-style: italic;
  margin: 0 0 16px;
}

.collections__subtitle {
  font-family: var(--font-body);
  color: var(--on-surface-variant);
  max-width: 560px;
  margin: 0 auto;
}

.bento {
  display: grid;
  grid-template-columns: 1fr;
  grid-auto-rows: 280px;
  gap: 24px;
}

@media (min-width: 768px) {
  .bento {
    grid-template-columns: repeat(4, 1fr);
    grid-template-rows: 1fr 1fr;
    height: 600px;
  }
}

.bento__card {
  position: relative;
  overflow: hidden;
  border-radius: var(--radius-xl);
  cursor: pointer;
}

@media (min-width: 768px) {
  .bento__card--main {
    grid-column: span 2;
    grid-row: span 2;
  }
  .bento__card--wide {
    grid-column: span 2;
  }
}

.bento__img {
  position: absolute;
  inset: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 700ms ease;
}

.bento__card--no-img {
  background: linear-gradient(135deg, var(--primary) 0%, color-mix(in srgb, var(--primary) 60%, black) 100%);
}

.collections__empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
  padding: 48px 24px;
  text-align: center;
  color: var(--on-surface-variant);
  border: 2px dashed var(--ghost-border);
  border-radius: var(--radius-lg);
}

.collections__empty .material-symbols-outlined {
  font-size: 48px;
  opacity: 0.4;
}

.bento__card:hover .bento__img {
  transform: scale(1.05);
}

.bento__gradient {
  position: absolute;
  inset: 0;
}

.bento__gradient--primary {
  background: linear-gradient(to top, rgba(0, 70, 74, 0.9), rgba(0, 70, 74, 0.2) 50%, transparent);
}

.bento__gradient--dark {
  background: linear-gradient(to top, rgba(28, 28, 23, 0.8), transparent);
}

.bento__content {
  position: absolute;
  bottom: 0;
  left: 0;
  padding: 24px;
}

.bento__content--large {
  padding: 32px;
}

.bento__content--wide {
  padding: 32px;
}

.bento__badge {
  display: inline-block;
  background: var(--secondary-container);
  color: var(--on-secondary-container);
  font-family: var(--font-label);
  font-size: var(--label-sm);
  font-weight: 700;
  padding: 4px 12px;
  border-radius: var(--radius-full);
  margin-bottom: 12px;
}

.bento__card-title {
  font-family: var(--font-headline);
  color: var(--on-primary);
  margin: 0;
  font-size: 1.25rem;
  font-weight: 500;
}

.bento__card-title--large {
  font-size: 1.875rem;
  margin-bottom: 8px;
}

.bento__card-title--med {
  font-size: 1.5rem;
  margin-bottom: 4px;
}

.bento__card-desc {
  font-family: var(--font-body);
  color: rgba(255, 255, 255, 0.8);
  font-size: var(--body-sm);
  margin: 0;
}

/* ==========================================================================
   CTA SECTION
   ========================================================================== */
.cta-section {
  background: var(--surface-container-highest);
  padding: 96px 0;
}

.cta-section__inner {
  max-width: 896px;
  margin: 0 auto;
  padding: 0 32px;
}

.cta-section__card {
  background: var(--surface-container-lowest);
  padding: 80px 48px;
  border-radius: var(--radius-2xl);
  box-shadow: var(--shadow-sm);
  border: 1px solid var(--ghost-border);
  text-align: center;
}

@media (max-width: 640px) {
  .cta-section__card { padding: 48px 24px; }
}

.cta-section__icon {
  color: var(--secondary);
  font-size: 48px;
  margin-bottom: 24px;
}

.cta-section__title {
  font-family: var(--font-headline);
  font-size: clamp(1.75rem, 3vw, 2.5rem);
  font-style: italic;
  margin: 0 0 24px;
}

.cta-section__text {
  font-family: var(--font-body);
  font-size: var(--body-lg);
  color: var(--on-surface-variant);
  max-width: 640px;
  margin: 0 auto 40px;
  line-height: 1.6;
}

.cta-section__actions {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
}

@media (min-width: 640px) {
  .cta-section__actions {
    flex-direction: row;
    justify-content: center;
  }
}

.cta-section__btn {
  padding: 20px 40px;
  border-radius: var(--radius-xl);
  font-size: var(--title-lg);
}

.cta-section__disclaimer {
  margin-top: 32px;
  font-family: var(--font-label);
  font-size: var(--body-sm);
  color: rgba(63, 73, 73, 0.7);
  font-style: italic;
}

/* ==========================================================================
   FOOTER
   ========================================================================== */
.lfooter {
  background: var(--surface-container-low);
  padding-top: 80px;
  padding-bottom: 40px;
  border-top: 1px solid rgba(190, 200, 201, 0.1);
}

.lfooter__inner {
  max-width: var(--container-max);
  margin: 0 auto;
  padding: 0 32px;
}

.lfooter__grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 48px;
  margin-bottom: 64px;
}

@media (min-width: 768px) {
  .lfooter__grid {
    grid-template-columns: 2fr 1fr;
  }
}

.lfooter__brand {
  font-family: var(--font-headline);
  font-size: 1.875rem;
  font-weight: 700;
  color: var(--primary);
  margin-bottom: 24px;
}

.lfooter__desc {
  font-family: var(--font-body);
  color: var(--on-surface-variant);
  max-width: 384px;
  line-height: 1.6;
  margin-bottom: 24px;
}

.lfooter__socials {
  display: flex;
  gap: 16px;
}

.lfooter__social-link {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  border-radius: var(--radius-full);
  background: rgba(0, 70, 74, 0.05);
  color: var(--primary);
  text-decoration: none;
  transition: all var(--transition-fast);
}

.lfooter__social-link:hover {
  background: var(--primary);
  color: var(--on-primary);
}

.lfooter__social-link .material-symbols-outlined {
  font-size: 18px;
}

.lfooter__links-heading {
  font-family: var(--font-label);
  font-weight: 700;
  font-size: var(--body-sm);
  text-transform: uppercase;
  letter-spacing: 0.08em;
  color: var(--on-surface);
  margin: 0 0 24px;
}

.lfooter__links {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.lfooter__links a {
  font-family: var(--font-body);
  color: var(--on-surface-variant);
  text-decoration: none;
  transition: color var(--transition-fast);
}

.lfooter__links a:hover {
  color: var(--primary);
}

.lfooter__copyright {
  padding-top: 40px;
  border-top: 1px solid rgba(190, 200, 201, 0.1);
}

.lfooter__copyright p {
  font-family: var(--font-label);
  font-size: var(--body-sm);
  color: rgba(63, 73, 73, 0.6);
  margin: 0;
}

/* ==========================================================================
   MOBILE BOTTOM NAV
   ========================================================================== */
.mobile-bottom-nav {
  display: flex;
  position: fixed;
  bottom: 0;
  left: 0;
  width: 100%;
  z-index: var(--z-sticky);
  background: rgba(252, 249, 241, 0.8);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  border-top: 1px solid var(--ghost-border);
  justify-content: space-around;
  align-items: center;
  padding: 8px 16px 24px;
  box-shadow: 0 -4px 20px rgba(0, 0, 0, 0.04);
}

@media (min-width: 768px) {
  .mobile-bottom-nav { display: none; }
}

.mobile-bottom-nav__item {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: rgba(28, 28, 23, 0.6);
  text-decoration: none;
  padding: 4px 16px;
  border-radius: var(--radius-lg);
  transition: all var(--transition-fast);
}

.mobile-bottom-nav__item--active {
  color: var(--primary);
  background: rgba(0, 70, 74, 0.05);
}

.mobile-bottom-nav__label {
  font-family: var(--font-label);
  font-size: 12px;
  font-weight: 600;
}
</style>

<style>
/* Dialog base — unscoped because Teleported to body */
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

/* How It Works dialog */
.how-dialog {
  max-width: 520px;
}

.how-dialog__header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 24px;
}

.how-dialog__header h2 {
  margin: 0;
}

.how-dialog__icon {
  color: var(--secondary);
  font-size: 32px;
}

.how-dialog__steps {
  margin: 0 0 28px;
  padding-left: 24px;
  display: flex;
  flex-direction: column;
  gap: 20px;
  list-style: none;
  counter-reset: step;
}

.how-dialog__steps li {
  counter-increment: step;
  display: flex;
  flex-direction: column;
  gap: 4px;
  position: relative;
  padding-left: 36px;
}

.how-dialog__steps li::before {
  content: counter(step);
  position: absolute;
  left: 0;
  top: 0;
  width: 28px;
  height: 28px;
  border-radius: var(--radius-full);
  background: var(--primary);
  color: var(--on-primary);
  font-family: var(--font-label);
  font-size: var(--label-lg);
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
}

.how-dialog__steps li strong {
  font-family: var(--font-headline);
  font-size: var(--title-lg);
  font-weight: 500;
  color: var(--on-surface);
}

.how-dialog__steps li span {
  font-family: var(--font-body);
  font-size: var(--body-md);
  color: var(--on-surface-variant);
  line-height: 1.5;
}

.how-dialog__actions {
  display: flex;
  justify-content: center;
}

.how-dialog__btn {
  padding: 12px 32px;
  border-radius: var(--radius-lg);
  font-size: var(--label-lg);
}
</style>
