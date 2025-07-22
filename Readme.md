Create MS SQL DB: Discount



Run script:



CREATE TABLE DiscountCodes (

&nbsp;   Id INT IDENTITY(1,1) PRIMARY KEY,

&nbsp;   Code NVARCHAR(10) NOT NULL UNIQUE,

&nbsp;   IsUsed BIT NOT NULL DEFAULT 0

);



Use Postman to run WebSocket:





ws://localhost:<ip>/ws



Messages:



{ "Count": 2000 } - Generate 2000 discount codes



{ "Code": "PWKQLTP1" } - Consume code

