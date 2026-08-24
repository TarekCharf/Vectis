import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ImportDepenseComponent } from './import-depense.component';

describe('ImportDepenseComponent', () => {
  let component: ImportDepenseComponent;
  let fixture: ComponentFixture<ImportDepenseComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ImportDepenseComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ImportDepenseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
