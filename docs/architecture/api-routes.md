# API routes

All business routes are prefixed with `/api/v1`. Operational endpoints are unversioned. Errors use RFC 9457 problem details (400 validation/domain rule, 404 not found).

| Method | Route | Use case |
| --- | --- | --- |
| GET | `/health` | Health (includes PostgreSQL) |
| GET | `/openapi/v1.json` | OpenAPI document |
| POST | `/api/v1/customers` | Create Customer |
| GET | `/api/v1/customers/{customerId}` | Retrieve Customer |
| POST | `/api/v1/customers/{customerId}/tenants` | Create Tenant |
| POST | `/api/v1/tenants/{tenantId}/facilities` | Create Facility |
| POST | `/api/v1/facilities/{facilityId}/units` | Create Unit |
| POST | `/api/v1/solutions` | Register Solution |
| GET | `/api/v1/solutions/{solutionId}` | Retrieve Solution |
| POST | `/api/v1/solutions/{solutionId}/items` | Associate Application / Gateway / Component |
| POST | `/api/v1/solutions/{solutionId}/items/{applicationId}/versions` | Register application version |
| POST | `/api/v1/solutions/{solutionId}/workloads` | Register workload (with served versions) |
| POST | `/api/v1/customers/{customerId}/agreements` | Record Agreement |
| POST | `/api/v1/customers/{customerId}/entitlements` | Record Entitlement |
| PUT | `/api/v1/contexts/{level}/{contextId}/solutions/{solutionId}/availability` | Make Solution available in a context |
| PUT | `/api/v1/contexts/{level}/{contextId}/solutions/{solutionId}/configuration` | Define contextual configuration (entries, version/workload selection) |
| GET | `/api/v1/contexts/{level}/{contextId}/solutions/{solutionId}/configuration/effective` | Resolve effective configuration |
| GET | `/api/v1/contexts/{level}/{contextId}/solutions/{solutionId}/target` | Resolve the configured workload |

`{level}` is `tenant`, `facility` or `unit` (case-insensitive).
