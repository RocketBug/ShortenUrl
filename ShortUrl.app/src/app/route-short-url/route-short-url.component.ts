import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { UrlManagementService } from '../_services/url-management.service';
import { ThemePalette } from '@angular/material/core';
import { ProgressSpinnerMode } from '@angular/material/progress-spinner';

@Component({
    selector: 'app-route-short-url',
    templateUrl: './route-short-url.component.html',
    styleUrls: ['./route-short-url.component.scss'],
})
export class RouteShortUrlComponent implements OnInit {
    dynamicSegment: string;
    color: ThemePalette = 'primary';
    mode: ProgressSpinnerMode = 'indeterminate';
    isLinkNotFound = false;

    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private urlManagement: UrlManagementService
    ) {}

    ngOnInit(): void {
        this.dynamicSegment =
            this.route.snapshot.paramMap.get('dynamicSegment');
        this.urlManagement.redirectToOriginalUrl(this.dynamicSegment).subscribe(
            originalRoute => {
                window.location.href = originalRoute;
            },
            (error: Error) => {
                this.isLinkNotFound = true;
            }
        );
    }
}
