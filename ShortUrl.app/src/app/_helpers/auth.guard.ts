import { inject } from '@angular/core';
import {
    ActivatedRouteSnapshot,
    CanActivateFn,
    Router,
    RouterStateSnapshot,
} from '@angular/router';
import { AuthenticationService } from '../_services/authentication.service';

export const AuthGuard: CanActivateFn = (
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
) => {
    const authService = inject(AuthenticationService);
    const router = inject(Router);
    const user = authService.userValue;

    if (user) {
        return true;
    }

    router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
    return false;
    // https://jasonwatmore.com/post/2022/11/15/angular-14-jwt-authentication-example-tutorial#user-service-ts
};
