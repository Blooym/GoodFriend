export const isValidContentIDHash = (id: string) => id.length > 64;
export const isValidIDUint = (id: any) => !Number.isNaN(parseInt(id, 10));
export const isValidGUID = (id: string) => id.match(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i);
