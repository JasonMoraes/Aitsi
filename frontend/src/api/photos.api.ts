import { api } from './client'
import type { PaginatedResponse, PhotoSummary, PhotoDetail, PhotoSearchParams, PhotoUpdatePayload } from '@/types'

const API_BASE = import.meta.env.VITE_API_BASE_URL || '/api'

interface BackendPhoto {
  id: number
  title: string
  description?: string
  imagePath: string
  thumbnailPath: string
  lat?: number
  lng?: number
  locationLabel?: string
  date: string
  datePrecision: string
  technique?: string
  inventoryNumber?: string
  originalFormat?: string
  license?: string
  digitization?: string
  quote?: string
  createdAt: string
  authorId: number
  categoryId: number
  author: { id: number; displayName: string }
  category: { id: number; name: string }
  tags: { id: number; name: string }[]
}

function toImageUrl(path: string): string {
  if (!path) return 'https://placehold.co/1200x900/ccc/666?text=Brak+zdjecia'
  if (path.startsWith('http')) return path
  return `${API_BASE.replace('/api', '')}${path}`
}

function toSummary(p: BackendPhoto): PhotoSummary {
  return {
    id: p.id,
    title: p.title,
    description: p.description,
    thumbnailUrl: toImageUrl(p.thumbnailPath),
    location: p.lat != null && p.lng != null
      ? { lat: p.lat, lng: p.lng, label: p.locationLabel }
      : undefined,
    date: p.date,
    datePrecision: p.datePrecision as PhotoSummary['datePrecision'],
    category: { id: p.category.id, name: p.category.name },
    author: { id: p.author.id, displayName: p.author.displayName },
    createdAt: p.createdAt,
  }
}

function toDetail(p: BackendPhoto): PhotoDetail {
  return {
    ...toSummary(p),
    imageUrl: toImageUrl(p.imagePath),
    locationLabel: p.locationLabel,
    technique: p.technique,
    inventoryNumber: p.inventoryNumber,
    originalFormat: p.originalFormat,
    license: p.license,
    digitization: p.digitization,
    quote: p.quote,
    tags: p.tags?.map(t => t.name) ?? [],
  }
}

export const photosApi = {
  async search(params: PhotoSearchParams): Promise<PaginatedResponse<PhotoSummary>> {
    const searchParams: Record<string, string> = {}
    if (params.q) searchParams.q = params.q
    if (params.categoryId) searchParams.categoryId = String(params.categoryId)
    if (params.dateFrom) searchParams.dateFrom = params.dateFrom
    if (params.dateTo) searchParams.dateTo = params.dateTo
    if (params.tag) searchParams.tag = params.tag
    if (params.sortBy) searchParams.sortBy = params.sortBy
    if (params.sortDir) searchParams.sortDir = params.sortDir
    if (params.page) searchParams.page = String(params.page)
    if (params.pageSize) searchParams.pageSize = String(params.pageSize)

    const res: { items: BackendPhoto[]; page: number; pageSize: number; totalCount: number; totalPages: number } =
      await api.get('photos', { searchParams }).json()

    return {
      items: res.items.map(toSummary),
      page: res.page,
      pageSize: res.pageSize,
      totalCount: res.totalCount,
      totalPages: res.totalPages,
    }
  },

  async getById(id: number): Promise<PhotoDetail> {
    const p: BackendPhoto = await api.get(`photos/${id}`).json()
    return toDetail(p)
  },

  imageUrl(id: number): string {
    return `${API_BASE}/photos/${id}/image`
  },

  thumbnailUrl(id: number): string {
    return `${API_BASE}/photos/${id}/thumbnail`
  },

  async upload(formData: FormData): Promise<PhotoDetail> {
    const p: BackendPhoto = await api.post('photos', { body: formData }).json()
    return toDetail(p)
  },

  async update(id: number, data: PhotoUpdatePayload): Promise<PhotoDetail> {
    const p: BackendPhoto = await api.put(`photos/${id}`, { json: data }).json()
    return toDetail(p)
  },

  async delete(id: number): Promise<void> {
    await api.delete(`photos/${id}`)
  },

  async getMyPhotos(params?: { page?: number; pageSize?: number }): Promise<PaginatedResponse<PhotoSummary>> {
    const searchParams: Record<string, string> = {}
    if (params?.page) searchParams.page = String(params.page)
    if (params?.pageSize) searchParams.pageSize = String(params.pageSize)

    const res: { items: BackendPhoto[]; page: number; pageSize: number; totalCount: number; totalPages: number } =
      await api.get('photos/my', { searchParams }).json()

    return {
      items: res.items.map(toSummary),
      page: res.page,
      pageSize: res.pageSize,
      totalCount: res.totalCount,
      totalPages: res.totalPages,
    }
  },
}