import { Component, OnInit } from '@angular/core';
import {
    FormBuilder,
    FormControl,
    FormGroup,
    Validators,
} from '@angular/forms';
import { UrlManagementService } from '../_services/url-management.service';
import { AuthenticationService } from '../_services/authentication.service';
import { User } from '../_models/user';
import { CreateUrlDto } from '../_models/createUrlDto';
import { MatTableDataSource } from '@angular/material/table';
import { UrlResponseDto } from '../_models/urlResponseDto';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';
import { snackBarConfigs } from '../_helpers/snackBarConfigs';

@Component({
    selector: 'app-manage-urls',
    templateUrl: './manage-urls.component.html',
    styleUrls: ['./manage-urls.component.scss'],
})
export class ManageUrlsComponent implements OnInit {
    createShortUrlForm: FormGroup;
    user: User;

    successSnackBarConfig: MatSnackBarConfig = new snackBarConfigs()
        .successConfig;
    errorSnackBarConfig: MatSnackBarConfig = new snackBarConfigs().failedConfig;

    displayedColumns: string[] = [
        'Original Url',
        'Short Url',
        'Expiry',
        'Actions',
    ];
    dataSource: MatTableDataSource<UrlResponseDto>;

    constructor(
        private _formBuilder: FormBuilder,
        private _urlManagementService: UrlManagementService,
        private _authenticationService: AuthenticationService,
        private _snackBar: MatSnackBar
    ) {
        this.dataSource = new MatTableDataSource<UrlResponseDto>();
    }

    ngOnInit(): void {
        this.createShortUrlForm = this._formBuilder.group({
            originalUrl: new FormControl('', [
                Validators.required,
                Validators.maxLength(250),
            ]),
            validMinutes: new FormControl(10, [
                Validators.required,
                Validators.min(1),
                Validators.max(60),
            ]),
        });
        this.user = this._authenticationService.userValue;
        this.getAllUrls();
    }

    getAllUrls(): void {
        this._urlManagementService
            .getAllUrls(this.user.id)
            .subscribe(urlResponse => {
                this.dataSource.data = urlResponse;
            });
    }

    onDeleteItem(item: UrlResponseDto): void {
        this._urlManagementService.archiveUrl(item.token).subscribe(() => {
            this._snackBar.open(
                `Link Deleted Successfully`,
                `Dismiss`,
                this.successSnackBarConfig
            );
            this.getAllUrls();
        });
    }

    submitForm(): void {
        if (this.createShortUrlForm.invalid) {
            return;
        }
        const urlDto: CreateUrlDto = {
            LongUrl: this.createShortUrlForm.value.originalUrl,
            userId: this.user.id,
            validMinutes: this.createShortUrlForm.value.validMinutes,
        };
        this._urlManagementService.createShortUrl(urlDto).subscribe(res => {
            this.getAllUrls();
            this.clearForm();
            this._snackBar.open(
                `Short Link Created Successfully`,
                `Dismiss`,
                this.successSnackBarConfig
            );
        });
    }

    showCopiedLinkSnackBar(): void {
        this._snackBar.open(
            `Copied to clipboard`,
            `Dismiss`,
            this.successSnackBarConfig
        );
    }

    clearForm(): void {
        this.createShortUrlForm.reset({
            originalUrl: '',
            validMinutes: 10,
        });
    }

    logoutUser(): void {
        this._authenticationService.logout();
    }
}
