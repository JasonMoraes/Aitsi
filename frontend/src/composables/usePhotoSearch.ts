import { useRouter, useRoute } from 'vue-router'
import { usePhotosStore } from '@/stores/photos.store'
import type { PhotoSearchParams } from '@/types'

export function usePhotoSearch() {
  const router = useRouter()
  const route = useRoute()
  const store = usePhotosStore()

  function readFromUrl(): Partial<PhotoSearchParams> {
    const q = route.query
    return {
      ...(q.q && { q: String(q.q) }),
      ...(q.categoryId && { categoryId: Number(q.categoryId) }),
      ...(q.lat && { lat: Number(q.lat) }),
      ...(q.lng && { lng: Number(q.lng) }),
      ...(q.radius && { radius: Number(q.radius) }),
      ...(q.dateFrom && { dateFrom: String(q.dateFrom) }),
      ...(q.dateTo && { dateTo: String(q.dateTo) }),
      ...(q.tag && { tag: String(q.tag) }),
      ...(q.sortBy && { sortBy: String(q.sortBy) as PhotoSearchParams['sortBy'] }),
      ...(q.sortDir && { sortDir: String(q.sortDir) as PhotoSearchParams['sortDir'] }),
      ...(q.page && { page: Number(q.page) }),
    }
  }

  function syncToUrl(params: PhotoSearchParams) {
    const query: Record<string, string> = {}
    if (params.q) query.q = params.q
    if (params.categoryId) query.categoryId = String(params.categoryId)
    if (params.lat) query.lat = String(params.lat)
    if (params.lng) query.lng = String(params.lng)
    if (params.radius) query.radius = String(params.radius)
    if (params.dateFrom) query.dateFrom = params.dateFrom
    if (params.dateTo) query.dateTo = params.dateTo
    if (params.tag) query.tag = params.tag
    if (params.sortBy && params.sortBy !== 'relevance') query.sortBy = params.sortBy
    if (params.sortDir && params.sortDir !== 'desc') query.sortDir = params.sortDir
    if (params.page && params.page > 1) query.page = String(params.page)
    router.replace({ query })
  }

  function initFromUrl() {
    const urlParams = readFromUrl()
    if (Object.keys(urlParams).length > 0) {
      store.setSearchParams(urlParams)
    }
  }

  return { readFromUrl, syncToUrl, initFromUrl }
}
