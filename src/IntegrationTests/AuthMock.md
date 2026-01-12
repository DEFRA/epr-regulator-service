### Authentication: Test Authentication Handler vs Real Azure AD B2C

Mini-ADR

**Context:**

The microservice uses Azure AD B2C for authentication via Microsoft.Identity.Web.

During integration testing, the authentication middleware attempts to:

1. Fetch OpenID Connect discovery document from B2C endpoint
2. Validate tokens and establish authenticated sessions
3. Challenge unauthenticated requests by redirecting to B2C login

This creates a problem: integration tests fail because there's no real B2C instance available, and the OIDC configuration endpoint cannot be reached.

**Options Considered:**

1. **Replace authentication with test handler in WebApplicationFactory** ✅ CHOSEN
   - Override `ConfigureServices` to remove real authentication
   - Add custom `TestAuthHandler` that returns authenticated user with test claims
   - Clean separation between test and production code
   - Standard ASP.NET Core testing pattern

2. **Allow auto-redirect = false**
   - Configure test client to not follow redirects
   - Tests receive 401/302 responses for protected pages

3. **Stub OIDC endpoints with WireMock**
   - Create WireMock stubs for `.well-known/openid-configuration`, JWKS, token endpoints
   - Provide valid OIDC discovery document and signing keys
   - More realistic but significantly more complex
   - Heavy maintenance burden for minimal benefit

4. **Conditional registration in startup code**
   - Add environment check in production `Program.cs`/`ServiceProviderExtension.cs`
   - Skip authentication when environment is "Testing"
   - Invasive to production code
   - Violates separation of concerns
   - Risk of creating security hole in production auth

**Decision:**

Use **Option 1: Test Authentication Handler** in WebApplicationFactory.
