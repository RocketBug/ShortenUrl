import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RouteShortUrlComponent } from './route-short-url.component';

describe('RouteShortUrlComponent', () => {
  let component: RouteShortUrlComponent;
  let fixture: ComponentFixture<RouteShortUrlComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [RouteShortUrlComponent]
    });
    fixture = TestBed.createComponent(RouteShortUrlComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
