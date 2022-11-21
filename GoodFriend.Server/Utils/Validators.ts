const isValidContentIDHash = (id: string) => id.length > 64;
const isValidIDUint = (id: any) => !Number.isNaN(parseInt(id, 10));

export { isValidContentIDHash, isValidIDUint };
