 RaceDay Platform (Part 3)

A full-stack, container-ready event management and enrolment platform built with ASP.NET Core MVC, Web API, and Entity Framework Core.

 Features
- **Role-Based Access Control**: Distinct interfaces and permissions for Organisers and Participants using JWT authentication.
- **Session Management**: Secure storage of user roles and tokens in ASP.NET Core Session.
- **RESTful API Integration**: The MVC frontend communicates exclusively with the Web API for all data operations (strict separation of concerns).
- **Participant Enrolment**: Secure, token-authenticated enrolment flow for race categories.
- **Organiser Dashboard**: Manage events and view platform data.
- **Cloud Storage Ready**: Azure Blob Storage integration architecture for event banner uploads.
- **Containerization**: Fully Dockerized application structure with `docker-compose.yml` for seamless orchestration.

 Tech Stack
- **Frontend**: ASP.NET Core MVC, Bootstrap 5
- **Backend**: ASP.NET Core Web API, Entity Framework Core
- **Database**: SQL Server
- **Authentication**: JWT (JSON Web Tokens)
- **DevOps**: Docker, Docker Compose

Running with Docker (Prerequisites)
1. Ensure Docker Desktop is installed and running.
2. Open a terminal in the solution root directory.
3. Run the following command:
   ```bash
   docker-compose up --build
4.Access the applications:
MVC Website: http://localhost:5001
API Swagger: http://localhost:5000/swagger