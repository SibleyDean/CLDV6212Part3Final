------------------------------------------------------------
-- SELECT THE DATABASE TO USE
------------------------------------------------------------
USE ABCRetailersDB;
GO


------------------------------------------------------------
-- DROP TABLES IF THEY ALREADY EXIST (SAFE RESET)
------------------------------------------------------------
IF OBJECT_ID('dbo.Orders', 'U') IS NOT NULL DROP TABLE dbo.Orders;
IF OBJECT_ID('dbo.Cart', 'U') IS NOT NULL DROP TABLE dbo.Cart;
IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL DROP TABLE dbo.Products;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO


------------------------------------------------------------
-- USERS TABLE (LOGIN + ROLE)
------------------------------------------------------------
CREATE TABLE dbo.Users
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserName NVARCHAR(100) NOT NULL UNIQUE,
    Email NVARCHAR(256) NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(50) NOT NULL DEFAULT 'Customer',
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL
);
GO


------------------------------------------------------------
-- PRODUCTS TABLE
-- Add sample products later if needed
------------------------------------------------------------
CREATE TABLE dbo.Products
(
    Id NVARCHAR(50) PRIMARY KEY,
    ProductName NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Price DECIMAL(18,2) NOT NULL,
    StockAvailable INT NOT NULL DEFAULT 0,
    ImageUrl NVARCHAR(1024) NULL
);
GO


------------------------------------------------------------
-- CART TABLE
------------------------------------------------------------
CREATE TABLE dbo.Cart
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ProductId NVARCHAR(50) NOT NULL,
    Quantity INT NOT NULL CHECK (Quantity > 0),
    DateAdded DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Cart_User
        FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
);
GO

CREATE INDEX IX_Cart_UserId ON dbo.Cart(UserId);
GO


------------------------------------------------------------
-- ORDERS TABLE
------------------------------------------------------------
CREATE TABLE dbo.Orders
(
    Id NVARCHAR(100) PRIMARY KEY,
    UserId INT NOT NULL,
    Username NVARCHAR(100) NOT NULL,
    ProductId NVARCHAR(100) NOT NULL,
    ProductName NVARCHAR(255) NULL,
    OrderDateUtc DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    Quantity INT NOT NULL CHECK (Quantity > 0),
    UnitPrice DECIMAL(18,2) NOT NULL,
    TotalAmount AS (UnitPrice * Quantity) PERSISTED,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Submitted',

    CONSTRAINT FK_Orders_User
        FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
);
GO

CREATE INDEX IX_Orders_UserId ON dbo.Orders(UserId);
GO

CREATE INDEX IX_Orders_Status ON dbo.Orders(Status);
GO


------------------------------------------------------------
-- OPTIONAL SAMPLE DATA (safe to run)
-- Replace hashes with real BCrypt hashes before production
------------------------------------------------------------

-- Sample Users (use real BCrypt hash later)
INSERT INTO dbo.Users (UserName, Email, PasswordHash, Role)
VALUES
('admin_user', 'admin@example.com', 'PLACEHOLDER_ADMIN_HASH', 'Admin'),
('john_doe',  'john@example.com',  'PLACEHOLDER_USER_HASH',  'Customer');
GO


-- Sample Products
INSERT INTO dbo.Products (Id, ProductName, Description, Price, StockAvailable)
VALUES
('P1001', 'Laptop Bag', 'Durable laptop bag', 499.00, 10),
('P1002', 'Wireless Mouse', 'Ergonomic mouse', 199.00, 20),
('P1003', 'Keyboard', 'USB keyboard', 299.00, 15);
GO


-- Sample Cart (assign to john_doe)
INSERT INTO dbo.Cart (UserId, ProductId, Quantity)
SELECT Id, 'P1001', 2 FROM dbo.Users WHERE UserName = 'john_doe';

INSERT INTO dbo.Cart (UserId, ProductId, Quantity)
SELECT Id, 'P1002', 1 FROM dbo.Users WHERE UserName = 'john_doe';
GO


------------------------------------------------------------
-- CHECK YOUR TABLES
------------------------------------------------------------
SELECT * FROM dbo.Users;
SELECT * FROM dbo.Products;
SELECT * FROM dbo.Cart;
SELECT * FROM dbo.Orders;
GO
