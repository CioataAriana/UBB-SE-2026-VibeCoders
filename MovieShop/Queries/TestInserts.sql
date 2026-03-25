DBCC CHECKIDENT('Equipment', RESEED, 0) WITH NO_INFOMSGS;

-- have at least 1 user
IF NOT EXISTS (SELECT * FROM Users WHERE ID = 1)
    INSERT INTO Users (Username, Email, PasswordHash, Balance) 
    VALUES ('Test', 'test@test.com', 'pwd', 100.00);

-- have at least 1 movie
IF NOT EXISTS (SELECT * FROM Movies WHERE ID = 1)
    INSERT INTO Movies (Title, Description, Rating, Price, ImageUrl) 
    VALUES ('Inception', 'A dream within a dream', 8.8, 45.00, '<none>');

-- started yesterday and expires 5 days from now
INSERT INTO ActiveSales (MovieID, DiscountPercentage, StartTime, EndTime)
VALUES (1, 20.00, DATEADD(day, -1, GETDATE()), DATEADD(day, 5, GETDATE()));

--  happened last week and ended 2 days ago
INSERT INTO ActiveSales (MovieID, DiscountPercentage, StartTime, EndTime)
VALUES (1, 50.00, DATEADD(day, -10, GETDATE()), DATEADD(day, -2, GETDATE()));

--to see if it expires in one minute when i refresh
INSERT INTO ActiveSales (MovieID, DiscountPercentage, StartTime, EndTime)
VALUES (1, 99.00, GETDATE(), DATEADD(minute, 1, GETDATE()));

-- seller 1
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'dummy1')
BEGIN
	INSERT INTO Users(Username, Email, PasswordHash, Balance)
	VALUES ('dummy1', 'dummy1@gmail.com', 'pass1', 0.00);
END

-- seller 2
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'dummy2')
BEGIN
	INSERT INTO Users(Username, Email, PasswordHash, Balance)
	VALUES ('dummy2', 'dummy2@gmail.com', 'pass2', 50.00);
END

-- add equipment based on seller id

DECLARE @Seller1 INT = (SELECT ID FROM Users WHERE Username = 'dummy1');
DECLARE @Seller2 INT = (SELECT ID FROM Users WHERE Username = 'dummy2');

INSERT INTO Equipment(SellerID, Title, Category, Description, Condition, Price, ImageUrl, Status)
VALUES
	(@Seller1, 'Canon EOS 2000D Kit', 'Cameras', '24.1 MP APS-C CMOS sensor. Perfect entry-level DSLR for student films, includes 18-55mm IS II Lens and 1080p cinematic video mode.', 'Good', 1200.00, 'https://static0.pocketlintimages.com/wordpress/wp-content/uploads/wm/143700-cameras-review-hands-on-canon-eos-2000d-review-image1-xploy5pbva.jpg', 'Available'),
    
    (@Seller1, 'Rode NTG Shotgun Mic', 'Audio', 'Professional directional condenser microphone. Super-cardioid polar pattern, ideal for isolating dialogue on noisy film sets.', 'New', 1200.00, 'https://fstudio.vtexassets.com/arquivos/ids/750303-1200-1200', 'Sold'),
    
    (@Seller2, 'Blackmagic Pocket Cinema 6K', 'Cameras', 'EF Mount, Super 35 HDR sensor, 13 stops of dynamic range and dual native ISO up to 25,600 for incredible low light performance.', 'Like New', 9500.00, 'https://images.blackmagicdesign.com/images/products/blackmagicpocketcinemacamera/main/pocket-6k-g2-xl.jpg', 'Available'),
    
    (@Seller1, 'DJI RS 3 Pro Gimbal', 'Stabilization', 'Carbon fiber construction, 4.5kg (10lbs) tested payload. Automated axis locks and LiDAR focusing for professional solo cinematographers.', 'New', 4200.00, 'https://m.media-amazon.com/images/I/61S6h1S-z3L._AC_SL1500_.jpg', 'Available'),
    
    (@Seller2, 'Atomos Ninja V+ Monitor', 'Monitoring', '5-inch 4K HDMI Recording Monitor. 1000 nits brightness for outdoor use, supports ProRes RAW recording directly from camera sensor.', 'Used', 2800.00, 'https://m.media-amazon.com/images/I/71N-W-vV6NL._AC_SL1500_.jpg', 'Available');