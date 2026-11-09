CREATE DATABASE RaceDayDB;
GO
USE RaceDayDB;
GO

CREATE TABLE Users
(
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    Password VARCHAR(150) NOT NULL,
    Phone VARCHAR(20) NOT NULL,
    Role VARCHAR(150) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE Organisers
(
    OrganiserID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL UNIQUE,
    OrganisationName VARCHAR(100) NOT NULL,
    ContactNumber VARCHAR(100) NOT NULL,
    Website VARCHAR(100),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Organisers_Users FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

CREATE TABLE Participants
(
    ParticipantID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL UNIQUE,
    DateOfBirth DATE NOT NULL,
    Gender VARCHAR(10) NOT NULL,
    EmergencyContact VARCHAR(20) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Participants_Users FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

CREATE TABLE Events
(
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID INT NOT NULL,
    EventName VARCHAR(150) NOT NULL,
    Description VARCHAR(1000),
    EventDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    Location VARCHAR(150) NOT NULL,
    Province VARCHAR(150) NOT NULL,
    EntryFee DECIMAL(10,2) NOT NULL,
    MaxParticipants INT,
    Status VARCHAR(150) NOT NULL,
    CONSTRAINT FK_Events_Organisers FOREIGN KEY (OrganiserID) REFERENCES Organisers(OrganiserID),
    CONSTRAINT CK_Events_EntryFee CHECK (EntryFee >= 0),
    CONSTRAINT CK_Events_MaxParticipants CHECK (MaxParticipants IS NULL OR MaxParticipants > 0)
);
GO

CREATE TABLE Categories
(
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName VARCHAR(100) NOT NULL UNIQUE,
    Description VARCHAR(200)
);
GO

CREATE TABLE EventCategories
(
    CategoryID INT NOT NULL,
    EventID INT NOT NULL,
    CONSTRAINT PK_EventCategories PRIMARY KEY (CategoryID, EventID),
    CONSTRAINT FK_EventCategories_Categories FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
    CONSTRAINT FK_EventCategories_Events FOREIGN KEY (EventID) REFERENCES Events(EventID)
);
GO

CREATE TABLE Routes
(
    RouteID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL UNIQUE,
    RouteName VARCHAR(100) NOT NULL,
    DistanceKm DECIMAL(8,2) NOT NULL,
    ElevationGainM DECIMAL(6,2),
    StartPoint VARCHAR(150) NOT NULL,
    FinishPoint VARCHAR(150) NOT NULL,
    RouteUrl VARCHAR(500),
    CONSTRAINT FK_Routes_Events FOREIGN KEY (EventID) REFERENCES Events(EventID),
    CONSTRAINT CK_Routes_Distance CHECK (DistanceKm > 0)
);
GO

CREATE TABLE EventEnrollments
(
    EnrollmentID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    ParticipantID INT NOT NULL,
    EnrolmentDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    RaceNumber VARCHAR(20),
    Status VARCHAR(20) NOT NULL,
    CONSTRAINT FK_EventEnrollments_Events FOREIGN KEY (EventID) REFERENCES Events(EventID),
    CONSTRAINT FK_EventEnrollments_Participants FOREIGN KEY (ParticipantID) REFERENCES Participants(ParticipantID),
    CONSTRAINT UQ_EventEnrollments_EventParticipant UNIQUE(EventID, ParticipantID)
);
GO

CREATE TABLE Results
(
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrollmentID INT NOT NULL UNIQUE,
    FinishTime TIME,
    Position INT,
    AveragePace VARCHAR(20),
    ResultStatus VARCHAR(20) NOT NULL,
    CONSTRAINT FK_Results_EventEnrollments FOREIGN KEY (EnrollmentID) REFERENCES EventEnrollments(EnrollmentID),
    CONSTRAINT CK_Results_Position CHECK (Position IS NULL OR Position > 0)
);
GO

CREATE TABLE EventWeather
(
    WeatherID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    ForecastDate DATE NOT NULL,
    TemperatureC DECIMAL(6,2),
    Condition VARCHAR(200) NOT NULL,
    WindSpeedKmh DECIMAL(6,2),
    RainChancePercent INT,
    RecordedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_EventWeather_Events FOREIGN KEY (EventID) REFERENCES Events(EventID),
    CONSTRAINT CK_EventWeather_Rain CHECK (RainChancePercent IS NULL OR RainChancePercent BETWEEN 0 AND 100)
);
GO

INSERT INTO Categories (CategoryName, Description) VALUES
('Marathon', 'Long-distance running event'),
('Half Marathon', '21 kilometre running event'),
('Fun Run', 'Short recreational running event');
GO