> **Project Overview:**
> A production-grade ASP.NET Core RESTful API built with Clean Architecture and SQL Server via ADO.NET for high-performance data operations. It features a robust security layer implementing JWT authentication with Refresh Token Rotation, sliding-window Rate Limiting to prevent brute-force attacks, BCrypt encryption, and comprehensive audit logging.

---

### Why Implement These Practices? (The Business & Technical Value)

* **Clean Architecture:** Separates concerns (API controllers, business logic, and data access layers) to ensure high maintainability, testability, and long-term scalability as the project grows.
* **JWT & Refresh Token Rotation:** Secures user sessions efficiently without overloading the database, while neutralizing token-theft risks through immediate revocation upon reuse detection.
* **Rate Limiting:** Protects server resources and endpoints against automated bot attacks, denial of service (DoS), and brute-force credential stuffing.
* **ADO.NET & SQL Server:** Provides precise, high-performance database querying and data integrity control through parameterized execution.
* **Auditing & Logging:** Guarantees full traceability of security events, failed attempts, and potential breaches for rapid incident response and debugging.
