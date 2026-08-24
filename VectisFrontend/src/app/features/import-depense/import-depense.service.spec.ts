import { TestBed } from '@angular/core/testing';

import { ImportDepenseService } from './import-depense.service';

describe('ImportDepenseService', () => {
  let service: ImportDepenseService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ImportDepenseService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
