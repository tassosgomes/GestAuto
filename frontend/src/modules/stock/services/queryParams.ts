type PaginationInput = {
  page?: number;
  size?: number;
  includeCompat?: boolean;
};

export const compactParams = (params: Record<string, unknown>) =>
  Object.fromEntries(
    Object.entries(params).filter(([, value]) => value !== undefined && value !== null && value !== ''),
  );

export const buildPaginationParams = ({ page, size, includeCompat }: PaginationInput) => {
  const safePage = page ?? 1;
  const safeSize = size ?? 10;

  const base: Record<string, number> = {
    _page: safePage,
    _size: safeSize,
  };

  if (includeCompat) {
    base.page = safePage;
    base.pageSize = safeSize;
  }

  return base;
};
