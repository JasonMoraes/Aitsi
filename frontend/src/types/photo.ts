export interface PhotoLocation {
  lat: number
  lng: number
  label?: string
}

export type DatePrecision = 'year' | 'month' | 'day'

export interface PhotoSummary {
  id: number
  title: string
  description?: string
  thumbnailUrl: string
  location?: PhotoLocation
  date: string
  datePrecision: DatePrecision
  category: { id: number; name: string }
  author: { id: number; displayName: string }
  createdAt: string
}

export interface PhotoContributor {
  name: string
  role: string
  avatarUrl?: string
  quote?: string
}

export interface PhotoDetail extends PhotoSummary {
  imageUrl: string
  locationLabel?: string
  technique?: string
  inventoryNumber?: string
  originalFormat?: string
  license?: string
  digitization?: string
  quote?: string
  contributor?: PhotoContributor
  tags?: string[]
  relatedPhotos?: PhotoSummary[]
}

export interface PhotoSearchParams {
  q?: string
  categoryId?: number
  lat?: number
  lng?: number
  radius?: number
  dateFrom?: string
  dateTo?: string
  tag?: string
  sortBy?: 'date' | 'createdAt' | 'relevance'
  sortDir?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export interface PhotoCreatePayload {
  file: File
  title: string
  description?: string
  categoryId: number
  lat: number
  lng: number
  locationLabel?: string
  date: string
}

export interface PhotoUpdatePayload {
  title?: string
  description?: string
  categoryId?: number
  lat?: number
  lng?: number
  locationLabel?: string
  date?: string
  technique?: string
  quote?: string
  inventoryNumber?: string
  originalFormat?: string
  license?: string
  digitization?: string
  tags?: string
}
