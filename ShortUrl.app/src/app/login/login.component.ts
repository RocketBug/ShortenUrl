import { Component, OnInit } from '@angular/core';
import {
    FormBuilder,
    FormControl,
    FormGroup,
    Validators,
} from '@angular/forms';
import { AuthenticationService } from '../_services/authentication.service';
import { first } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';
import { snackBarConfigs } from '../_helpers/snackBarConfigs';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss'],
})
export class LoginComponent implements OnInit {
    authForm: FormGroup;
    submitted: boolean;
    error: string;
    loading: boolean;

    errorSnackBarConfig = new snackBarConfigs().failedConfig;

    constructor(
        private _formBuilder: FormBuilder,
        private _route: ActivatedRoute,
        private _router: Router,
        private _authenticationService: AuthenticationService,
        private _snackBar: MatSnackBar
    ) {}

    ngOnInit(): void {
        this.authForm = this._formBuilder.group({
            email: new FormControl('', [Validators.required, Validators.email]),
            password: new FormControl('', [Validators.required]),
        });
    }

    submitForm(value: string): void {
        this.submitted = true;
        if (this.authForm.invalid) {
            return;
        }

        this.error = '';
        this.loading = true;
        if (value === 'register') {
            this._authenticationService
                .register(
                    this.authForm.value.email,
                    this.authForm.value.password
                )
                .pipe(first())
                .subscribe({
                    next: () => {
                        const returnUrl =
                            this._route.snapshot.queryParams['returnUrl'] ||
                            '/';
                        this._router.navigate([returnUrl]);
                    },
                    error: error => {
                        this.error = error;
                        this.loading = false;
                        this._snackBar.open(
                            `Looks like there was an issue registering`,
                            `Dismiss`,
                            this.errorSnackBarConfig
                        );
                    },
                });
        }

        if (value === 'login') {
            this._authenticationService
                .login(this.authForm.value.email, this.authForm.value.password)
                .pipe(first())
                .subscribe({
                    next: () => {
                        const returnUrl =
                            this._route.snapshot.queryParams['returnUrl'] ||
                            '/';
                        this._router.navigate([returnUrl]);
                    },
                    error: error => {
                        this.error = error;
                        this.loading = false;
                        this._snackBar.open(
                            `Looks like there was an issue logging, Please check you email and password`,
                            `Dismiss`,
                            this.errorSnackBarConfig
                        );
                    },
                });
        }
    }
}
