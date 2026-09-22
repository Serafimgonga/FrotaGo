export const environment = {
  production: true,
  apiUrl: (typeof window !== 'undefined' && (window as any).__env?.apiUrl) || 'https://frotago-api.onrender.com',
  hubUrl: (typeof window !== 'undefined' && (window as any).__env?.hubUrl) || 'https://frotago-api.onrender.com/hubs/gps'
};
