import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { CreateUrlDto } from '../_models/createUrlDto';
import { UrlResponseDto } from '../_models/urlResponseDto';

@Injectable({
    providedIn: 'root',
})
export class UrlManagementService {
    constructor(private http: HttpClient) {}

    getAllUrls(userId: string): Observable<UrlResponseDto[]> {
        return this.http.get<UrlResponseDto[]>(
            `${environment.apiUrl}/url/${userId}/all-urls`
        );
    }

    createShortUrl(createUrlDto: CreateUrlDto): Observable<any> {
        return this.http.post<any>(
            `${environment.apiUrl}/url/shorten`,
            createUrlDto
        );
    }

    redirectToOriginalUrl(urlToken: string): Observable<string> {
        return this.http.get<string>(
            `${environment.apiUrl}/url/${urlToken}/redirect`
        );
    }

    archiveUrl(urlToken: string): Observable<any> {
        return this.http.delete<any>(
            `${environment.apiUrl}/url/${urlToken}/archive`
        );
    }
}
