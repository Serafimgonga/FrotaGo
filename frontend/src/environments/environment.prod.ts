export const environment = {
  production: true,
  apiUrl: (typeof window !== 'undefined' && (window as any).__env?.apiUrl) || '',
  hubUrl: (typeof window !== 'undefined' && (window as any).__env?.hubUrl) || '/hubs/gps'
};
