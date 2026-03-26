USE MovieShopDB
GO

DROP TABLE IF EXISTS Transactions; 
DROP TABLE IF EXISTS ActiveSales;
DROP TABLE IF EXISTS Events;
DROP TABLE IF EXISTS Equipment;
DROP TABLE IF EXISTS Users;
DROP TABLE IF EXISTS Movies;


CREATE TABLE Users(
	ID INT IDENTITY(1,1) PRIMARY KEY,
	Username NVARCHAR(100) NOT NULL UNIQUE,
	Email NVARCHAR(255) NOT NULL UNIQUE,
	PasswordHash NVARCHAR(255) NOT NULL,
	Balance DECIMAL(15, 2) NOT NULL
)

CREATE TABLE Movies(
	ID INT IDENTITY(1, 1) PRIMARY KEY,
	Title NVARCHAR(255) NOT NULL,
	Description NVARCHAR(MAX) NOT NULL,
	Rating FLOAT NOT NULL,
	Price DECIMAL(15, 2) NOT NULL,
	ImageUrl NVARCHAR(500)
)

CREATE TABLE Equipment(
	ID INT IDENTITY(1, 1) PRIMARY KEY,
	SellerID INT NOT NULL,
	Title NVARCHAR(255) NOT NULL,
	Category NVARCHAR(100) NOT NULL,
	Description NVARCHAR(MAX) NULL,
	Condition NVARCHAR(50) NOT NULL,
	Price DECIMAL(15, 2) NOT NULL,
	ImageUrl NVARCHAR(500) NULL,
	Status NVARCHAR(50) NOT NULL,

	FOREIGN KEY (SellerID) REFERENCES Users(ID)
)

CREATE TABLE Events(
	ID INT IDENTITY(1, 1) PRIMARY KEY,
	MovieID INT NOT NULL,
	Title NVARCHAR(255) NOT NULL,
	Description NVARCHAR(MAX) NOT NULL,
	Date DATETIME NOT NULL,
	Location NVARCHAR(255) NOT NULL,
	TicketPrice DECIMAL(15, 2) NOT NULL,
	PosterUrl NVARCHAR(500) NOT NULL,

	FOREIGN KEY(MovieID) REFERENCES Movies(ID)
)

CREATE TABLE Transactions (
    TransactionID INT IDENTITY(1,1) PRIMARY KEY,
    BuyerID INT NOT NULL,
	SellerID INT NULL, --NULL IF BUYING MOVIE/TICKET
	EquipmentID INT NULL,
	MovieID INT NULL,
	EventID INT NULL,
	Amount DECIMAL(15, 2) NOT NULL,
	Type NVARCHAR(50) NOT NULL,
	Status NVARCHAR(50) NOT NULL,
	Timestamp DATETIME NOT NULL,
	ShippingAddress NVARCHAR(500) NULL,

	FOREIGN KEY (BuyerID) REFERENCES Users(ID),
	FOREIGN KEY (SellerID) REFERENCES Users(ID),
	FOREIGN KEY (EquipmentID) REFERENCES Equipment(ID),
    FOREIGN KEY (MovieID) REFERENCES Movies(ID),
    FOREIGN KEY (EventID) REFERENCES [Events](ID)
);

CREATE TABLE ActiveSales(
	ID INT IDENTITY(1, 1) PRIMARY KEY,
	MovieID INT NOT NULL,
	DiscountPercentage DECIMAL(5, 2) NOT NULL,
	StartTime DATETIME NOT NULL,
	EndTime DATETIME NOT NULL,

	FOREIGN KEY (MovieID) REFERENCES Movies(ID)
)


USE MovieShopDB;
GO

-- Make sure User 1 exists
IF NOT EXISTS (SELECT * FROM Users WHERE ID = 1)
    INSERT INTO Users (Username, Email, PasswordHash, Balance) 
    VALUES ('Test', 'test@test.com', 'pwd', 100.00);

-- Make sure Movie 1 exists
IF NOT EXISTS (SELECT * FROM Movies WHERE ID = 1)
    INSERT INTO Movies (Title, Description, Rating, Price) 
    VALUES ('Inception', 'A dream within a dream', 8.8, 45.00);


SELECT * FROM Transactions


USE MovieShopDB;
GO

-- started yesterday and expires 5 days from now
INSERT INTO ActiveSales (MovieID, DiscountPercentage, StartTime, EndTime)
VALUES (1, 20.00, DATEADD(day, -1, GETDATE()), DATEADD(day, 5, GETDATE()));

--  happened last week and ended 2 days ago
INSERT INTO ActiveSales (MovieID, DiscountPercentage, StartTime, EndTime)
VALUES (1, 50.00, DATEADD(day, -10, GETDATE()), DATEADD(day, -2, GETDATE()));

--to see if it expires in one minute when i refresh
INSERT INTO ActiveSales (MovieID, DiscountPercentage, StartTime, EndTime)
VALUES (1, 99.00, GETDATE(), DATEADD(minute, 1, GETDATE()));


select * from Equipment