/*
    LeaveManagementDB full setup script
    SQL Server 2019+
*/
SET NOCOUNT ON;
GO

USE master;
GO

IF DB_ID('LeaveManagementDB') IS NOT NULL
BEGIN
    ALTER DATABASE LeaveManagementDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE LeaveManagementDB;
END
GO

CREATE DATABASE LeaveManagementDB;
GO

USE LeaveManagementDB;
GO

/* =========================
   TABLES
   ========================= */

IF OBJECT_ID('dbo.AuditLogs', 'U') IS NOT NULL DROP TABLE dbo.AuditLogs;
GO
IF OBJECT_ID('dbo.Holidays', 'U') IS NOT NULL DROP TABLE dbo.Holidays;
GO
IF OBJECT_ID('dbo.LeaveRequests', 'U') IS NOT NULL DROP TABLE dbo.LeaveRequests;
GO
IF OBJECT_ID('dbo.LeaveAllocations', 'U') IS NOT NULL DROP TABLE dbo.LeaveAllocations;
GO
IF OBJECT_ID('dbo.LeaveTypes', 'U') IS NOT NULL DROP TABLE dbo.LeaveTypes;
GO

IF OBJECT_ID('dbo.AspNetUserTokens', 'U') IS NOT NULL DROP TABLE dbo.AspNetUserTokens;
IF OBJECT_ID('dbo.AspNetUserRoles', 'U') IS NOT NULL DROP TABLE dbo.AspNetUserRoles;
IF OBJECT_ID('dbo.AspNetUserClaims', 'U') IS NOT NULL DROP TABLE dbo.AspNetUserClaims;
IF OBJECT_ID('dbo.AspNetRoleClaims', 'U') IS NOT NULL DROP TABLE dbo.AspNetRoleClaims;
IF OBJECT_ID('dbo.AspNetUserLogins', 'U') IS NOT NULL DROP TABLE dbo.AspNetUserLogins;
IF OBJECT_ID('dbo.AspNetRoles', 'U') IS NOT NULL DROP TABLE dbo.AspNetRoles;
IF OBJECT_ID('dbo.AspNetUsers', 'U') IS NOT NULL DROP TABLE dbo.AspNetUsers;
IF OBJECT_ID('dbo.Departments', 'U') IS NOT NULL DROP TABLE dbo.Departments;
GO

CREATE TABLE Departments (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE AspNetRoles (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
    Name NVARCHAR(256) NULL,
    NormalizedName NVARCHAR(256) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL
);
GO

CREATE TABLE AspNetUsers (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
    UserName NVARCHAR(256) NULL,
    NormalizedUserName NVARCHAR(256) NULL,
    Email NVARCHAR(256) NULL,
    NormalizedEmail NVARCHAR(256) NULL,
    EmailConfirmed BIT NOT NULL,
    PasswordHash NVARCHAR(MAX) NULL,
    SecurityStamp NVARCHAR(MAX) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL,
    PhoneNumber NVARCHAR(MAX) NULL,
    PhoneNumberConfirmed BIT NOT NULL,
    TwoFactorEnabled BIT NOT NULL,
    LockoutEnd DATETIMEOFFSET NULL,
    LockoutEnabled BIT NOT NULL,
    AccessFailedCount INT NOT NULL,
    FullName NVARCHAR(150) NOT NULL,
    DepartmentId INT NULL,
    ManagerId NVARCHAR(450) NULL,
    DateOfJoining DATETIME NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    ProfilePicture NVARCHAR(255) NULL,
    CONSTRAINT FK_AspNetUsers_Departments_DepartmentId FOREIGN KEY (DepartmentId) REFERENCES Departments(Id),
    CONSTRAINT FK_AspNetUsers_AspNetUsers_ManagerId FOREIGN KEY (ManagerId) REFERENCES AspNetUsers(Id)
);
GO

CREATE TABLE AspNetUserClaims (
    Id INT NOT NULL IDENTITY PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL,
    ClaimType NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL,
    CONSTRAINT FK_AspNetUserClaims_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);
GO

CREATE TABLE AspNetUserLogins (
    LoginProvider NVARCHAR(450) NOT NULL,
    ProviderKey NVARCHAR(450) NOT NULL,
    ProviderDisplayName NVARCHAR(MAX) NULL,
    UserId NVARCHAR(450) NOT NULL,
    CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey),
    CONSTRAINT FK_AspNetUserLogins_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);
GO

CREATE TABLE AspNetUserRoles (
    UserId NVARCHAR(450) NOT NULL,
    RoleId NVARCHAR(450) NOT NULL,
    CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_AspNetUserRoles_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_AspNetUserRoles_AspNetRoles_RoleId FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);
GO

CREATE TABLE AspNetUserTokens (
    UserId NVARCHAR(450) NOT NULL,
    LoginProvider NVARCHAR(450) NOT NULL,
    Name NVARCHAR(450) NOT NULL,
    Value NVARCHAR(MAX) NULL,
    CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name),
    CONSTRAINT FK_AspNetUserTokens_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);
GO

CREATE TABLE AspNetRoleClaims (
    Id INT NOT NULL IDENTITY PRIMARY KEY,
    RoleId NVARCHAR(450) NOT NULL,
    ClaimType NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL,
    CONSTRAINT FK_AspNetRoleClaims_AspNetRoles_RoleId FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);
GO

CREATE TABLE LeaveTypes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    DefaultDays INT NOT NULL,
    Description NVARCHAR(255) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE LeaveAllocations (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(450) NOT NULL,
    LeaveTypeId INT NOT NULL,
    NumberOfDays INT NOT NULL,
    UsedDays INT NOT NULL DEFAULT 0,
    Period INT NOT NULL,
    DateCreated DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id),
    FOREIGN KEY (LeaveTypeId) REFERENCES LeaveTypes(Id)
);
GO

CREATE TABLE LeaveRequests (
    Id INT PRIMARY KEY IDENTITY(1,1),
    RequestingEmployeeId NVARCHAR(450) NOT NULL,
    LeaveTypeId INT NOT NULL,
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    NumberOfDays INT NOT NULL,
    Reason NVARCHAR(500) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    ApprovedById NVARCHAR(450) NULL,
    ReviewComments NVARCHAR(500) NULL,
    DateActioned DATETIME NULL,
    DateRequested DATETIME NOT NULL DEFAULT GETDATE(),
    Cancelled BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (RequestingEmployeeId) REFERENCES AspNetUsers(Id),
    FOREIGN KEY (LeaveTypeId) REFERENCES LeaveTypes(Id)
);
GO

CREATE TABLE Holidays (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(150) NOT NULL,
    HolidayDate DATETIME NOT NULL,
    Description NVARCHAR(255) NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE AuditLogs (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Action NVARCHAR(200) NOT NULL,
    EntityName NVARCHAR(100) NOT NULL,
    EntityId NVARCHAR(100) NOT NULL,
    OldValues NVARCHAR(MAX) NULL,
    NewValues NVARCHAR(MAX) NULL,
    PerformedBy NVARCHAR(150) NOT NULL,
    IPAddress NVARCHAR(50) NULL,
    Timestamp DATETIME NOT NULL DEFAULT GETDATE()
);
GO

CREATE INDEX IX_AspNetUsers_DepartmentId ON AspNetUsers(DepartmentId);
CREATE INDEX IX_AspNetUsers_ManagerId ON AspNetUsers(ManagerId);
CREATE INDEX IX_LeaveRequests_Status ON LeaveRequests(Status);
CREATE INDEX IX_LeaveRequests_RequestingEmployeeId ON LeaveRequests(RequestingEmployeeId);
GO

/* =========================
   STORED PROCEDURES
   ========================= */
IF OBJECT_ID('dbo.sp_GetLeaveRequests', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_GetLeaveRequests;
GO
CREATE PROCEDURE sp_GetLeaveRequests
    @Status NVARCHAR(50) = NULL,
    @DepartmentId INT = NULL,
    @LeaveTypeId INT = NULL,
    @Year INT = NULL,
    @UserId NVARCHAR(450) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @PageNumber <= 0 SET @PageNumber = 1;
        IF @PageSize <= 0 SET @PageSize = 10;

        ;WITH Filtered AS (
            SELECT
                lr.Id, lr.StartDate, lr.EndDate, lr.NumberOfDays, lr.Status, lr.DateRequested, lr.Cancelled,
                u.FullName AS EmployeeName, d.Name AS DepartmentName, lt.Name AS LeaveTypeName,
                COUNT(1) OVER() AS TotalCount
            FROM LeaveRequests lr
            INNER JOIN AspNetUsers u ON u.Id = lr.RequestingEmployeeId
            LEFT JOIN Departments d ON d.Id = u.DepartmentId
            INNER JOIN LeaveTypes lt ON lt.Id = lr.LeaveTypeId
            WHERE (@Status IS NULL OR lr.Status = @Status)
              AND (@DepartmentId IS NULL OR u.DepartmentId = @DepartmentId)
              AND (@LeaveTypeId IS NULL OR lr.LeaveTypeId = @LeaveTypeId)
              AND (@Year IS NULL OR YEAR(lr.StartDate) = @Year)
              AND (@UserId IS NULL OR lr.RequestingEmployeeId = @UserId)
        )
        SELECT *
        FROM Filtered
        ORDER BY DateRequested DESC
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

IF OBJECT_ID('dbo.sp_GetLeaveRequestsByEmployee', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_GetLeaveRequestsByEmployee;
GO
CREATE PROCEDURE sp_GetLeaveRequestsByEmployee
    @UserId NVARCHAR(450),
    @Year INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @UserId IS NULL OR LTRIM(RTRIM(@UserId)) = ''
            THROW 50001, 'UserId is required.', 1;

        SELECT lr.Id, lr.StartDate, lr.EndDate, lr.NumberOfDays, lr.Status, lr.DateRequested, lr.Reason,
               lt.Name AS LeaveTypeName
        FROM LeaveRequests lr
        INNER JOIN LeaveTypes lt ON lt.Id = lr.LeaveTypeId
        WHERE lr.RequestingEmployeeId = @UserId
          AND (@Year IS NULL OR YEAR(lr.StartDate) = @Year)
        ORDER BY lr.DateRequested DESC;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

IF OBJECT_ID('dbo.sp_GetPendingRequestsForManager', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_GetPendingRequestsForManager;
GO
CREATE PROCEDURE sp_GetPendingRequestsForManager
    @ManagerId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @ManagerId IS NULL OR LTRIM(RTRIM(@ManagerId)) = ''
            THROW 50002, 'ManagerId is required.', 1;

        SELECT lr.Id, u.FullName AS EmployeeName, d.Name AS DepartmentName, lt.Name AS LeaveTypeName,
               lr.StartDate, lr.EndDate, lr.NumberOfDays, lr.DateRequested
        FROM LeaveRequests lr
        INNER JOIN AspNetUsers u ON u.Id = lr.RequestingEmployeeId
        LEFT JOIN Departments d ON d.Id = u.DepartmentId
        INNER JOIN LeaveTypes lt ON lt.Id = lr.LeaveTypeId
        WHERE lr.Status = 'Pending'
          AND u.ManagerId = @ManagerId
          AND lr.Cancelled = 0
        ORDER BY lr.DateRequested DESC;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

IF OBJECT_ID('dbo.sp_ApplyLeave', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_ApplyLeave;
GO
CREATE PROCEDURE sp_ApplyLeave
    @UserId NVARCHAR(450),
    @LeaveTypeId INT,
    @StartDate DATETIME,
    @EndDate DATETIME,
    @NumberOfDays INT,
    @Reason NVARCHAR(500),
    @Result INT OUTPUT,
    @Message NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @UserId IS NULL OR LTRIM(RTRIM(@UserId)) = '' OR @LeaveTypeId <= 0 OR @NumberOfDays <= 0
        BEGIN
            SET @Result = 0;
            SET @Message = 'Invalid request parameters.';
            RETURN;
        END

        IF @EndDate < @StartDate
        BEGIN
            SET @Result = 0;
            SET @Message = 'EndDate must be greater than or equal to StartDate.';
            RETURN;
        END

        IF EXISTS (
            SELECT 1 FROM LeaveRequests
            WHERE RequestingEmployeeId = @UserId
              AND Status IN ('Pending', 'Approved')
              AND Cancelled = 0
              AND (@StartDate <= EndDate AND @EndDate >= StartDate)
        )
        BEGIN
            SET @Result = 0;
            SET @Message = 'Overlapping leave request found.';
            RETURN;
        END

        IF EXISTS (
            SELECT 1 FROM Holidays
            WHERE CAST(HolidayDate AS DATE) BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE)
        )
        BEGIN
            SET @Result = 0;
            SET @Message = 'Requested date range includes a public holiday.';
            RETURN;
        END

        DECLARE @Period INT = YEAR(@StartDate);
        DECLARE @Remaining INT;
        SELECT @Remaining = (NumberOfDays - UsedDays)
        FROM LeaveAllocations
        WHERE UserId = @UserId AND LeaveTypeId = @LeaveTypeId AND Period = @Period;

        IF @Remaining IS NULL OR @Remaining < @NumberOfDays
        BEGIN
            SET @Result = 0;
            SET @Message = 'Insufficient leave balance.';
            RETURN;
        END

        INSERT INTO LeaveRequests (RequestingEmployeeId, LeaveTypeId, StartDate, EndDate, NumberOfDays, Reason, Status, DateRequested, Cancelled)
        VALUES (@UserId, @LeaveTypeId, @StartDate, @EndDate, @NumberOfDays, @Reason, 'Pending', GETDATE(), 0);

        SET @Result = 1;
        SET @Message = 'Leave request submitted successfully.';
    END TRY
    BEGIN CATCH
        SET @Result = 0;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END
GO

IF OBJECT_ID('dbo.sp_ApproveLeave', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_ApproveLeave;
GO
CREATE PROCEDURE sp_ApproveLeave
    @LeaveRequestId INT,
    @ApprovedById NVARCHAR(450),
    @Comments NVARCHAR(500) = NULL,
    @Result INT OUTPUT,
    @Message NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @UserId NVARCHAR(450), @LeaveTypeId INT, @Days INT, @Period INT, @RequesterManagerId NVARCHAR(450);
        DECLARE @IsAdmin BIT = 0;
        SELECT @IsAdmin =
            CASE WHEN EXISTS (
                SELECT 1
                FROM AspNetUserRoles ur
                INNER JOIN AspNetRoles r ON r.Id = ur.RoleId
                WHERE ur.UserId = @ApprovedById AND r.NormalizedName = 'ADMIN'
            ) THEN 1 ELSE 0 END;

        SELECT
            @UserId = lr.RequestingEmployeeId,
            @LeaveTypeId = lr.LeaveTypeId,
            @Days = lr.NumberOfDays,
            @Period = YEAR(lr.StartDate),
            @RequesterManagerId = u.ManagerId
        FROM LeaveRequests lr
        INNER JOIN AspNetUsers u ON u.Id = lr.RequestingEmployeeId
        WHERE lr.Id = @LeaveRequestId AND lr.Status = 'Pending' AND lr.Cancelled = 0;

        IF @UserId IS NULL
        BEGIN
            SET @Result = 0;
            SET @Message = 'Pending leave request not found.';
            RETURN;
        END

        IF @IsAdmin = 0 AND ISNULL(@RequesterManagerId, '') <> @ApprovedById
        BEGIN
            SET @Result = 0;
            SET @Message = 'You are not authorized to approve this leave request.';
            RETURN;
        END

        UPDATE LeaveRequests
        SET Status = 'Approved', ApprovedById = @ApprovedById, ReviewComments = @Comments, DateActioned = GETDATE()
        WHERE Id = @LeaveRequestId AND Status = 'Pending' AND Cancelled = 0;

        UPDATE LeaveAllocations
        SET UsedDays = UsedDays + @Days
        WHERE UserId = @UserId AND LeaveTypeId = @LeaveTypeId AND Period = @Period;

        INSERT INTO AuditLogs (Action, EntityName, EntityId, NewValues, PerformedBy, IPAddress, Timestamp)
        VALUES ('Approve Leave', 'LeaveRequest', CAST(@LeaveRequestId AS NVARCHAR(100)), @Comments, @ApprovedById, NULL, GETDATE());

        SET @Result = 1;
        SET @Message = 'Leave approved successfully.';
    END TRY
    BEGIN CATCH
        SET @Result = 0;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END
GO

IF OBJECT_ID('dbo.sp_RejectLeave', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_RejectLeave;
GO
CREATE PROCEDURE sp_RejectLeave
    @LeaveRequestId INT,
    @RejectedById NVARCHAR(450),
    @Comments NVARCHAR(500),
    @Result INT OUTPUT,
    @Message NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @RequesterManagerId NVARCHAR(450), @RequestExists BIT = 0, @IsAdmin BIT = 0;
        SELECT @IsAdmin =
            CASE WHEN EXISTS (
                SELECT 1
                FROM AspNetUserRoles ur
                INNER JOIN AspNetRoles r ON r.Id = ur.RoleId
                WHERE ur.UserId = @RejectedById AND r.NormalizedName = 'ADMIN'
            ) THEN 1 ELSE 0 END;

        SELECT
            @RequestExists = 1,
            @RequesterManagerId = u.ManagerId
        FROM LeaveRequests lr
        INNER JOIN AspNetUsers u ON u.Id = lr.RequestingEmployeeId
        WHERE lr.Id = @LeaveRequestId AND lr.Status = 'Pending' AND lr.Cancelled = 0;

        IF @RequestExists = 0
        BEGIN
            SET @Result = 0;
            SET @Message = 'Pending leave request not found.';
            RETURN;
        END

        IF @IsAdmin = 0 AND ISNULL(@RequesterManagerId, '') <> @RejectedById
        BEGIN
            SET @Result = 0;
            SET @Message = 'You are not authorized to reject this leave request.';
            RETURN;
        END

        UPDATE LeaveRequests
        SET Status = 'Rejected', ApprovedById = @RejectedById, ReviewComments = @Comments, DateActioned = GETDATE()
        WHERE Id = @LeaveRequestId AND Status = 'Pending' AND Cancelled = 0;

        INSERT INTO AuditLogs (Action, EntityName, EntityId, NewValues, PerformedBy, IPAddress, Timestamp)
        VALUES ('Reject Leave', 'LeaveRequest', CAST(@LeaveRequestId AS NVARCHAR(100)), @Comments, @RejectedById, NULL, GETDATE());

        SET @Result = 1;
        SET @Message = 'Leave rejected successfully.';
    END TRY
    BEGIN CATCH
        SET @Result = 0;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END
GO

IF OBJECT_ID('dbo.sp_CancelLeave', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_CancelLeave;
GO
CREATE PROCEDURE sp_CancelLeave
    @LeaveRequestId INT,
    @UserId NVARCHAR(450),
    @Result INT OUTPUT,
    @Message NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @Status NVARCHAR(50), @StartDate DATETIME, @Days INT, @LeaveTypeId INT, @Period INT;
        SELECT @Status = Status, @StartDate = StartDate, @Days = NumberOfDays, @LeaveTypeId = LeaveTypeId, @Period = YEAR(StartDate)
        FROM LeaveRequests
        WHERE Id = @LeaveRequestId AND RequestingEmployeeId = @UserId AND Cancelled = 0;

        IF @Status IS NULL
        BEGIN
            SET @Result = 0;
            SET @Message = 'Leave request not found for user.';
            RETURN;
        END

        IF @Status = 'Pending' OR (@Status = 'Approved' AND CAST(@StartDate AS DATE) > CAST(GETDATE() AS DATE))
        BEGIN
            IF @Status = 'Approved'
            BEGIN
                UPDATE LeaveAllocations
                SET UsedDays = CASE WHEN UsedDays >= @Days THEN UsedDays - @Days ELSE 0 END
                WHERE UserId = @UserId AND LeaveTypeId = @LeaveTypeId AND Period = @Period;
            END

            UPDATE LeaveRequests SET Status = 'Cancelled', Cancelled = 1, DateActioned = GETDATE() WHERE Id = @LeaveRequestId;
            SET @Result = 1;
            SET @Message = 'Leave cancelled successfully.';
        END
        ELSE
        BEGIN
            SET @Result = 0;
            SET @Message = 'This leave request cannot be cancelled.';
        END
    END TRY
    BEGIN CATCH
        SET @Result = 0;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END
GO

IF OBJECT_ID('dbo.sp_GetLeaveBalance', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_GetLeaveBalance;
GO
CREATE PROCEDURE sp_GetLeaveBalance
    @UserId NVARCHAR(450),
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @UserId IS NULL OR LTRIM(RTRIM(@UserId)) = ''
            THROW 50003, 'UserId is required.', 1;

        SELECT lt.Id AS LeaveTypeId, lt.Name AS LeaveTypeName,
               ISNULL(la.NumberOfDays, lt.DefaultDays) AS TotalDays,
               ISNULL(la.UsedDays, 0) AS UsedDays,
               ISNULL(la.NumberOfDays, lt.DefaultDays) - ISNULL(la.UsedDays, 0) AS RemainingDays
        FROM LeaveTypes lt
        LEFT JOIN LeaveAllocations la
            ON la.LeaveTypeId = lt.Id
           AND la.UserId = @UserId
           AND la.Period = @Year
        WHERE lt.IsActive = 1
        ORDER BY lt.Name;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

IF OBJECT_ID('dbo.sp_AllocateLeave', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_AllocateLeave;
GO
CREATE PROCEDURE sp_AllocateLeave
    @UserId NVARCHAR(450),
    @LeaveTypeId INT,
    @NumberOfDays INT,
    @Year INT,
    @Result INT OUTPUT,
    @Message NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @UserId IS NULL OR @LeaveTypeId <= 0 OR @NumberOfDays <= 0 OR @Year <= 0
        BEGIN
            SET @Result = 0;
            SET @Message = 'Invalid allocation input.';
            RETURN;
        END

        IF EXISTS (SELECT 1 FROM LeaveAllocations WHERE UserId = @UserId AND LeaveTypeId = @LeaveTypeId AND Period = @Year)
        BEGIN
            UPDATE LeaveAllocations
            SET NumberOfDays = @NumberOfDays
            WHERE UserId = @UserId AND LeaveTypeId = @LeaveTypeId AND Period = @Year;
        END
        ELSE
        BEGIN
            INSERT INTO LeaveAllocations (UserId, LeaveTypeId, NumberOfDays, UsedDays, Period, DateCreated)
            VALUES (@UserId, @LeaveTypeId, @NumberOfDays, 0, @Year, GETDATE());
        END

        INSERT INTO AuditLogs (Action, EntityName, EntityId, NewValues, PerformedBy, IPAddress, Timestamp)
        VALUES ('Allocate Leave', 'LeaveAllocation', CONCAT(@UserId, ':', @LeaveTypeId, ':', @Year), CAST(@NumberOfDays AS NVARCHAR(20)), 'SYSTEM', NULL, GETDATE());

        SET @Result = 1;
        SET @Message = 'Leave allocation processed successfully.';
    END TRY
    BEGIN CATCH
        SET @Result = 0;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END
GO

IF OBJECT_ID('dbo.sp_GetAdminDashboardStats', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_GetAdminDashboardStats;
GO
CREATE PROCEDURE sp_GetAdminDashboardStats
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT
            (SELECT COUNT(1) FROM AspNetUsers WHERE IsActive = 1) AS TotalEmployees,
            (SELECT COUNT(1) FROM LeaveRequests WHERE Status = 'Pending' AND Cancelled = 0) AS TotalPendingRequests,
            (SELECT COUNT(1) FROM LeaveRequests WHERE Status = 'Approved' AND MONTH(DateActioned) = MONTH(GETDATE()) AND YEAR(DateActioned) = YEAR(GETDATE())) AS TotalApprovedThisMonth,
            (SELECT COUNT(1) FROM LeaveRequests WHERE Status = 'Rejected' AND MONTH(DateActioned) = MONTH(GETDATE()) AND YEAR(DateActioned) = YEAR(GETDATE())) AS TotalRejectedThisMonth;

        SELECT d.Name AS DepartmentName, COUNT(1) AS RequestCount
        FROM LeaveRequests lr
        INNER JOIN AspNetUsers u ON u.Id = lr.RequestingEmployeeId
        LEFT JOIN Departments d ON d.Id = u.DepartmentId
        WHERE YEAR(lr.StartDate) = @Year
        GROUP BY d.Name
        ORDER BY RequestCount DESC;

        SELECT MONTH(StartDate) AS [Month], COUNT(1) AS TotalRequests
        FROM LeaveRequests
        WHERE YEAR(StartDate) = @Year
        GROUP BY MONTH(StartDate)
        ORDER BY [Month];
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

IF OBJECT_ID('dbo.sp_GetManagerDashboardStats', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_GetManagerDashboardStats;
GO
CREATE PROCEDURE sp_GetManagerDashboardStats
    @ManagerId NVARCHAR(450),
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT
            (SELECT COUNT(1) FROM AspNetUsers WHERE ManagerId = @ManagerId AND IsActive = 1) AS TeamSize,
            (SELECT COUNT(1)
             FROM LeaveRequests lr
             INNER JOIN AspNetUsers u ON u.Id = lr.RequestingEmployeeId
             WHERE u.ManagerId = @ManagerId AND lr.Status = 'Pending' AND lr.Cancelled = 0) AS PendingApprovals,
            (SELECT COUNT(1)
             FROM LeaveRequests lr
             INNER JOIN AspNetUsers u ON u.Id = lr.RequestingEmployeeId
             WHERE u.ManagerId = @ManagerId
               AND lr.Status = 'Approved'
               AND MONTH(lr.DateActioned) = MONTH(GETDATE())
               AND YEAR(lr.DateActioned) = YEAR(GETDATE())) AS ApprovedThisMonth;

        SELECT lt.Name AS LeaveTypeName, SUM(la.NumberOfDays - la.UsedDays) AS RemainingDays
        FROM LeaveAllocations la
        INNER JOIN AspNetUsers u ON u.Id = la.UserId
        INNER JOIN LeaveTypes lt ON lt.Id = la.LeaveTypeId
        WHERE u.ManagerId = @ManagerId AND la.Period = @Year
        GROUP BY lt.Name
        ORDER BY lt.Name;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

IF OBJECT_ID('dbo.sp_GetLeaveReport', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_GetLeaveReport;
GO
CREATE PROCEDURE sp_GetLeaveReport
    @DepartmentId INT = NULL,
    @LeaveTypeId INT = NULL,
    @Status NVARCHAR(50) = NULL,
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL,
    @Year INT = NULL,
    @ManagerId NVARCHAR(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT u.FullName AS EmployeeName, d.Name AS DepartmentName, lt.Name AS LeaveTypeName,
               lr.StartDate, lr.EndDate, lr.NumberOfDays, lr.Status,
               appr.FullName AS ApprovedBy
        FROM LeaveRequests lr
        INNER JOIN AspNetUsers u ON u.Id = lr.RequestingEmployeeId
        LEFT JOIN AspNetUsers appr ON appr.Id = lr.ApprovedById
        LEFT JOIN Departments d ON d.Id = u.DepartmentId
        INNER JOIN LeaveTypes lt ON lt.Id = lr.LeaveTypeId
        WHERE (@DepartmentId IS NULL OR u.DepartmentId = @DepartmentId)
          AND (@LeaveTypeId IS NULL OR lr.LeaveTypeId = @LeaveTypeId)
          AND (@Status IS NULL OR lr.Status = @Status)
          AND (@FromDate IS NULL OR lr.StartDate >= @FromDate)
          AND (@ToDate IS NULL OR lr.EndDate <= @ToDate)
          AND (@Year IS NULL OR YEAR(lr.StartDate) = @Year)
          AND (@ManagerId IS NULL OR u.ManagerId = @ManagerId)
        ORDER BY lr.DateRequested DESC;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* =========================
   SEED DATA
   ========================= */
INSERT INTO Departments (Name, Description, IsActive)
SELECT v.Name, v.Description, v.IsActive
FROM (VALUES
    ('IT Department', 'Handles software and infrastructure', CAST(1 AS BIT)),
    ('HR Department', 'Handles people operations', CAST(1 AS BIT)),
    ('Finance Department', 'Handles accounting and payroll', CAST(1 AS BIT))
) AS v(Name, Description, IsActive)
WHERE NOT EXISTS
(
    SELECT 1
    FROM Departments d
    WHERE d.Name = v.Name
);
GO

INSERT INTO LeaveTypes (Name, DefaultDays, Description, IsActive)
VALUES
('Casual Leave', 12, 'Casual leave for personal work', 1),
('Sick Leave', 10, 'Sick leave entitlement', 1),
('Annual Leave', 15, 'Annual paid leave', 1),
('Maternity Leave', 90, 'Maternity leave entitlement', 1),
('Paternity Leave', 15, 'Paternity leave entitlement', 1);
GO

INSERT INTO Holidays (Name, HolidayDate, Description)
VALUES
('Republic Day', '2025-01-26', 'National holiday'),
('Holi', '2025-03-14', 'Festival holiday'),
('Good Friday', '2025-04-18', 'Religious holiday'),
('Independence Day', '2025-08-15', 'National holiday'),
('Gandhi Jayanti', '2025-10-02', 'National holiday'),
('Diwali', '2025-10-20', 'Festival holiday'),
('Christmas', '2025-12-25', 'Festival holiday');
GO

DECLARE @AdminRoleId NVARCHAR(450) = NEWID();
DECLARE @ManagerRoleId NVARCHAR(450) = NEWID();
DECLARE @EmployeeRoleId NVARCHAR(450) = NEWID();

INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
VALUES
(@AdminRoleId, 'Admin', 'ADMIN', NEWID()),
(@ManagerRoleId, 'Manager', 'MANAGER', NEWID()),
(@EmployeeRoleId, 'Employee', 'EMPLOYEE', NEWID());
GO

DECLARE @PasswordHashAdmin NVARCHAR(MAX) = 'AQAAAAIAAYagAAAAEAdminReplaceWithIdentityHash123==';
DECLARE @PasswordHashManager NVARCHAR(MAX) = 'AQAAAAIAAYagAAAAEManagerReplaceWithIdentityHash123==';
DECLARE @PasswordHashEmployee NVARCHAR(MAX) = 'AQAAAAIAAYagAAAAEEmployeeReplaceWithIdentityHash123==';

DECLARE @AdminId NVARCHAR(450) = NEWID();
DECLARE @Manager1Id NVARCHAR(450) = NEWID();
DECLARE @Manager2Id NVARCHAR(450) = NEWID();
DECLARE @Emp1Id NVARCHAR(450) = NEWID();
DECLARE @Emp2Id NVARCHAR(450) = NEWID();
DECLARE @Emp3Id NVARCHAR(450) = NEWID();
DECLARE @Emp4Id NVARCHAR(450) = NEWID();
DECLARE @Emp5Id NVARCHAR(450) = NEWID();

INSERT INTO AspNetUsers
(Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, FullName, DepartmentId, ManagerId, DateOfJoining, IsActive)
VALUES
(@AdminId, 'admin@lms.com', 'ADMIN@LMS.COM', 'admin@lms.com', 'ADMIN@LMS.COM', 1, @PasswordHashAdmin, NEWID(), NEWID(), '9000000000', 1, 0, 1, 0, 'System Admin', 1, NULL, '2022-01-10', 1),
(@Manager1Id, 'manager1@lms.com', 'MANAGER1@LMS.COM', 'manager1@lms.com', 'MANAGER1@LMS.COM', 1, @PasswordHashManager, NEWID(), NEWID(), '9000000001', 1, 0, 1, 0, 'Manager One', 1, @AdminId, '2022-02-10', 1),
(@Manager2Id, 'manager2@lms.com', 'MANAGER2@LMS.COM', 'manager2@lms.com', 'MANAGER2@LMS.COM', 1, @PasswordHashManager, NEWID(), NEWID(), '9000000002', 1, 0, 1, 0, 'Manager Two', 2, @AdminId, '2022-03-10', 1),
(@Emp1Id, 'employee1@lms.com', 'EMPLOYEE1@LMS.COM', 'employee1@lms.com', 'EMPLOYEE1@LMS.COM', 1, @PasswordHashEmployee, NEWID(), NEWID(), '9000000003', 1, 0, 1, 0, 'Employee One', 1, @Manager1Id, '2023-01-01', 1),
(@Emp2Id, 'employee2@lms.com', 'EMPLOYEE2@LMS.COM', 'employee2@lms.com', 'EMPLOYEE2@LMS.COM', 1, @PasswordHashEmployee, NEWID(), NEWID(), '9000000004', 1, 0, 1, 0, 'Employee Two', 1, @Manager1Id, '2023-02-01', 1),
(@Emp3Id, 'employee3@lms.com', 'EMPLOYEE3@LMS.COM', 'employee3@lms.com', 'EMPLOYEE3@LMS.COM', 1, @PasswordHashEmployee, NEWID(), NEWID(), '9000000005', 1, 0, 1, 0, 'Employee Three', 2, @Manager2Id, '2023-03-01', 1),
(@Emp4Id, 'employee4@lms.com', 'EMPLOYEE4@LMS.COM', 'employee4@lms.com', 'EMPLOYEE4@LMS.COM', 1, @PasswordHashEmployee, NEWID(), NEWID(), '9000000006', 1, 0, 1, 0, 'Employee Four', 3, @Manager2Id, '2023-04-01', 1),
(@Emp5Id, 'employee5@lms.com', 'EMPLOYEE5@LMS.COM', 'employee5@lms.com', 'EMPLOYEE5@LMS.COM', 1, @PasswordHashEmployee, NEWID(), NEWID(), '9000000007', 1, 0, 1, 0, 'Employee Five', 3, @Manager2Id, '2023-05-01', 1);
GO

DECLARE @AdminRole NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'Admin');
DECLARE @ManagerRole NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'Manager');
DECLARE @EmployeeRole NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'Employee');

INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT Id, @AdminRole FROM AspNetUsers WHERE Email = 'admin@lms.com';
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT Id, @ManagerRole FROM AspNetUsers WHERE Email IN ('manager1@lms.com','manager2@lms.com');
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT Id, @EmployeeRole FROM AspNetUsers WHERE Email LIKE 'employee%@lms.com';
GO

DECLARE @Year INT = 2025;
INSERT INTO LeaveAllocations (UserId, LeaveTypeId, NumberOfDays, UsedDays, Period, DateCreated)
SELECT u.Id, lt.Id, lt.DefaultDays, 0, @Year, GETDATE()
FROM AspNetUsers u
CROSS JOIN LeaveTypes lt
WHERE u.IsActive = 1;
GO

PRINT 'LeaveManagementDB full setup script executed successfully.';
GO
