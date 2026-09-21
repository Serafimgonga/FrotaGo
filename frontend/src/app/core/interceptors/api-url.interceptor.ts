import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export const apiUrlInterceptor: HttpInterceptorFn = (req, next) => {
  if (environment.apiUrl && req.url.startsWith('/api')) {
    const baseUrl = environment.apiUrl.replace(/\/+$/, '');
    const apiReq = req.clone({
      url: `${baseUrl}${req.url}`
    });
    return next(apiReq);
  }
  return next(req);
};
