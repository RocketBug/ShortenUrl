import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ManageUrlsComponent } from './manage-urls/manage-urls.component';
import { LoginComponent } from './login/login.component';
import { AuthGuard } from './_helpers/auth.guard';
import { RouteShortUrlComponent } from './route-short-url/route-short-url.component';

const routes: Routes = [
    {
        path: '',
        component: ManageUrlsComponent,
        canActivate: [AuthGuard],
    },
    { path: 'login', component: LoginComponent },
    { path: ':dynamicSegment', component: RouteShortUrlComponent },
    { path: '**', redirectTo: '' },
];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule],
})
export class AppRoutingModule {}
