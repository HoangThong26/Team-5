import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // Lấy token từ localStorage (phải trùng tên với lúc bạn lưu khi login)
  const token = localStorage.getItem('token'); 
  console.log('Interceptor đang chạy, token tìm thấy:', token);

  if (token) {
    const cloned = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    return next(cloned);
  }

  return next(req);
};