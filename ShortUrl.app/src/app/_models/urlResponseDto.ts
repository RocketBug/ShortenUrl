export class UrlResponseDto {
    id: number;
    originalUrl: string;
    shortenUrl: string;
    token: string;
    expiryDate: Date;
    isExpired: boolean;
}
