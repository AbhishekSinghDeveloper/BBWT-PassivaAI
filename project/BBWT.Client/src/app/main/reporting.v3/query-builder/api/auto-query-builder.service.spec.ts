import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

import { AutoQueryBuilderService, IAutoQueryRequest, IAutoQueryResponse, IQueryValidationRequest, IQueryValidationResponse } from './auto-query-builder.service';
import { HttpResponsesHandlersFactory } from '@bbwt/modules/data-service';

describe('AutoQueryBuilderService', () => {
    let service: AutoQueryBuilderService;
    let httpMock: HttpTestingController;
    let mockHandlersFactory: jasmine.SpyObj<HttpResponsesHandlersFactory>;

    beforeEach(() => {
        mockHandlersFactory = jasmine.createSpyObj('HttpResponsesHandlersFactory', ['create']);

        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [
                AutoQueryBuilderService,
                { provide: HttpResponsesHandlersFactory, useValue: mockHandlersFactory }
            ]
        });

        service = TestBed.inject(AutoQueryBuilderService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    describe('generateQuery', () => {
        it('should generate query successfully', async () => {
            // Arrange
            const request: IAutoQueryRequest = {
                tableSetId: 'table-set-1',
                description: 'Get all users with their orders',
                maxTables: 5,
                includeRelatedTables: true
            };

            const expectedResponse: IAutoQueryResponse = {
                queryId: 'query-123',
                sql: 'SELECT u.*, o.* FROM users u LEFT JOIN orders o ON u.id = o.user_id',
                tables: ['users', 'orders'],
                confidence: 0.95,
                suggestions: ['Consider adding WHERE clause for date filtering']
            };

            // Act
            const resultPromise = service.generateQuery(request);

            // Assert
            const req = httpMock.expectOne('api/reporting3/query/auto/generate');
            expect(req.request.method).toBe('POST');
            expect(req.request.body).toEqual(request);

            req.flush(expectedResponse);
            const result = await resultPromise;
            expect(result).toEqual(expectedResponse);
        });

        it('should handle minimal request parameters', async () => {
            // Arrange
            const request: IAutoQueryRequest = {
                tableSetId: 'table-set-1',
                description: 'Simple query'
            };

            const expectedResponse: IAutoQueryResponse = {
                queryId: 'query-456',
                sql: 'SELECT * FROM table1',
                tables: ['table1'],
                confidence: 0.8
            };

            // Act
            const resultPromise = service.generateQuery(request);

            // Assert
            const req = httpMock.expectOne('api/reporting3/query/auto/generate');
            expect(req.request.method).toBe('POST');
            expect(req.request.body).toEqual(request);

            req.flush(expectedResponse);
            const result = await resultPromise;
            expect(result).toEqual(expectedResponse);
        });
    });

    describe('validateQuery', () => {
        it('should validate query successfully when valid', async () => {
            // Arrange
            const request: IQueryValidationRequest = {
                sql: 'SELECT * FROM users WHERE id = 1',
                tableSetId: 'table-set-1'
            };

            const expectedResponse: IQueryValidationResponse = {
                isValid: true,
                errors: [],
                warnings: ['Consider using specific column names instead of *']
            };

            // Act
            const resultPromise = service.validateQuery(request);

            // Assert
            const req = httpMock.expectOne('api/reporting3/query/auto/validate');
            expect(req.request.method).toBe('POST');
            expect(req.request.body).toEqual(request);

            req.flush(expectedResponse);
            const result = await resultPromise;
            expect(result).toEqual(expectedResponse);
        });

        it('should validate query and return errors when invalid', async () => {
            // Arrange
            const request: IQueryValidationRequest = {
                sql: 'SELECT * FROM nonexistent_table',
                tableSetId: 'table-set-1'
            };

            const expectedResponse: IQueryValidationResponse = {
                isValid: false,
                errors: ['Table "nonexistent_table" does not exist'],
                warnings: []
            };

            // Act
            const resultPromise = service.validateQuery(request);

            // Assert
            const req = httpMock.expectOne('api/reporting3/query/auto/validate');
            expect(req.request.method).toBe('POST');
            expect(req.request.body).toEqual(request);

            req.flush(expectedResponse);
            const result = await resultPromise;
            expect(result).toEqual(expectedResponse);
        });
    });

    describe('getSuggestions', () => {
        it('should get suggestions for partial description', async () => {
            // Arrange
            const tableSetId = 'table-set-1';
            const partialDescription = 'users with ord';
            const expectedSuggestions = [
                'users with orders',
                'users with order history',
                'users with order details'
            ];

            // Act
            const resultPromise = service.getSuggestions(tableSetId, partialDescription);

            // Assert
            const req = httpMock.expectOne(`api/reporting3/query/auto/suggestions/${tableSetId}?q=${encodeURIComponent(partialDescription)}`);
            expect(req.request.method).toBe('GET');

            req.flush(expectedSuggestions);
            const result = await resultPromise;
            expect(result).toEqual(expectedSuggestions);
        });
    });

    describe('getAvailableTables', () => {
        it('should get available tables for table set', async () => {
            // Arrange
            const tableSetId = 'table-set-1';
            const expectedTables = ['users', 'orders', 'products', 'categories'];

            // Act
            const resultPromise = service.getAvailableTables(tableSetId);

            // Assert
            const req = httpMock.expectOne(`api/reporting3/query/auto/tables/${tableSetId}`);
            expect(req.request.method).toBe('GET');

            req.flush(expectedTables);
            const result = await resultPromise;
            expect(result).toEqual(expectedTables);
        });
    });
});
