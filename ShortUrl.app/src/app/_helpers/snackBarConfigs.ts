import { MatSnackBarConfig } from '@angular/material/snack-bar';

export class snackBarConfigs {
    successConfig: MatSnackBarConfig = {
        horizontalPosition: 'center',
        verticalPosition: 'bottom',
        duration: 2500,
        panelClass: ['success-snackbar'],
    };

    failedConfig: MatSnackBarConfig = {
        horizontalPosition: 'center',
        verticalPosition: 'bottom',
        duration: 2500,
        panelClass: ['failed-snackbar'],
    };
}
