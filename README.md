# pmd-web-application
This is my Project for Year 2 Sem 2 ASPJ (Application Security &amp; Project) Module

As part of NYP's Application Security & Project Module, I am tasked to develop a security solution that covers the development of a secure web application using ASP.NET

In this project, I developed an integrated & secure ASP.NET MVC Web Application that is geared towards combating issues regarding PMD Safety in Singapore.
The Project has a strong focus on Application Security and the finished project is secure against the OWASP Top 10, i.e. Parameterized Query was used to prevent SQL Injection, exception handling to prevent disclosing sensitive codes to the attackers etc.

List of Features :
Login
- Login (Including 2FA, External Authentication, Lockout Account upon multiple wrong attempts)
- Registration (Recaptcha, Password Hashed + Salted in database)
- Forget Password

Manage
- Update Profile Picture - File Header Signature check to ensure only valid JPG/PNG files are uploaded
- Change Password

Insurance Claim
- Claim by User - Smart PDF Reader that extracts the content of a pre-filled PDF Claim Form, validate the content, before accepting into the system
- Manage Claims - Admin is able to approve/deny claims

Super Admin Management
- CRUD of Roles (Aid in roles-based access)
- User Management (Register/delete new user, lock/unlock user, revoke/assign roles, reset user password)
- Security Configurations (Set password policy - password ageing, min length etc)
- View System Logs (Errors, Warnings and Info are logged and can be downloaded as PDF, CSV or Excel file)

Language Used : C#(Primary) HTML, CSS, Javascript/Jquery
Framework : ASP.NET MVC, Identity Framework, Entity Framework
