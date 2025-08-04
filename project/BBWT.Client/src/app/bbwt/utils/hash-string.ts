export const getKeyFromHashString = (hash: string): number => {
  return Number(hash.split("-")[0]);
};