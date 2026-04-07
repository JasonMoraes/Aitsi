<script setup lang="ts">
import { LMap, LTileLayer, LMarker, LPopup } from '@vue-leaflet/vue-leaflet'
import 'leaflet/dist/leaflet.css'

const props = withDefaults(defineProps<{
  lat: number
  lng: number
  label?: string
  height?: string
}>(), {
  height: '250px'
})
</script>

<template>
  <div class="location-display" aria-label="Mapa lokalizacji zdjęcia">
    <div class="location-display__map" :style="{ height: props.height }">
      <l-map
        :zoom="14"
        :center="[props.lat, props.lng]"
        :use-global-leaflet="false"
        :options="{ scrollWheelZoom: false }"
      >
        <l-tile-layer
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          attribution="&copy; <a href='https://www.openstreetmap.org/copyright'>OpenStreetMap</a>"
          layer-type="base"
          name="OpenStreetMap"
        />
        <l-marker :lat-lng="[props.lat, props.lng]">
          <l-popup v-if="props.label">{{ props.label }}</l-popup>
        </l-marker>
      </l-map>
    </div>
    <p v-if="props.label || props.lat" class="location-display__coords">
      <span v-if="props.label">{{ props.label }} &middot; </span>
      <span>{{ props.lat.toFixed(5) }}, {{ props.lng.toFixed(5) }}</span>
    </p>
  </div>
</template>

<style scoped>
.location-display__map {
  border-radius: var(--radius-lg);
  overflow: hidden;
  border: 1px solid var(--ghost-border);
}

.location-display__coords {
  font-size: var(--label-lg);
  color: var(--on-surface-variant);
  margin: 8px 0 0;
}
</style>
