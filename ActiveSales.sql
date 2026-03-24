USE MovieShopDB
GO

DROP TABLE IF EXISTS Transactions; 
DROP TABLE IF EXISTS ActiveSales;
DROP TABLE IF EXISTS Events;
DROP TABLE IF EXISTS Equipment;
DROP TABLE IF EXISTS Movies;


CREATE TABLE Movies(
	ID INT IDENTITY(1, 1) PRIMARY KEY,
	Title NVARCHAR(255) NOT NULL,
	Description NVARCHAR(MAX) NOT NULL,
	Rating FLOAT NOT NULL,
	Price DECIMAL(15, 2) NOT NULL,
	ImageUrl NVARCHAR(500)
)

CREATE TABLE ActiveSales(
	ID INT IDENTITY(1, 1) PRIMARY KEY,
	MovieID INT NOT NULL,
	DiscountPercentage DECIMAL(5, 2) NOT NULL,
	StartTime DATETIME NOT NULL,
	EndTime DATETIME NOT NULL,

	FOREIGN KEY (MovieID) REFERENCES Movies(ID)
)


-- Make sure User 1 exists
IF NOT EXISTS (SELECT * FROM Users WHERE ID = 1)
    INSERT INTO Users (Username, Email, PasswordHash, Balance) 
    VALUES ('Test', 'test@test.com', 'pwd', 100.00);

-- Make sure Movie 1 exists
IF NOT EXISTS (SELECT * FROM Movies WHERE ID = 1)
    INSERT INTO Movies (Title, Description, Rating, Price) 
    VALUES ('Inception', 'A dream within a dream', 8.8, 45.00);

-------SALE LOGIC

-- started yesterday and expires 5 days from now
INSERT INTO ActiveSales (MovieID, DiscountPercentage, StartTime, EndTime)
VALUES (1, 20.00, DATEADD(day, -1, GETDATE()), DATEADD(day, 5, GETDATE()));

--  happened last week and ended 2 days ago
INSERT INTO ActiveSales (MovieID, DiscountPercentage, StartTime, EndTime)
VALUES (1, 50.00, DATEADD(day, -10, GETDATE()), DATEADD(day, -2, GETDATE()));

--to see if it expires in one minute when i refresh
INSERT INTO ActiveSales (MovieID, DiscountPercentage, StartTime, EndTime)
VALUES (1, 99.00, GETDATE(), DATEADD(minute, 1, GETDATE()));
