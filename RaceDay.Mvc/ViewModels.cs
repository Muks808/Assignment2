using System.Text.Json.Serialization;
using Assignment2.DTOs;

namespace RaceDay.Mvc;

public record Identity([property: JsonPropertyName("userID")] int Id, string FirstName, string LastName, string Role);
public record Category([property: JsonPropertyName("categoryID")] int Id,
    [property: JsonPropertyName("categoryName")] string Name, string Description);
public record Race([property: JsonPropertyName("eventID")] int Id,
    [property: JsonPropertyName("eventName")] string Name, string Description, DateTime EventDate,
    TimeSpan StartTime, string Location, string Province, decimal EntryFee, int? MaxParticipants,
    string Status, decimal DistanceKm, string EventType, string? BannerUrl, int EnrolmentCount,
    [property: JsonPropertyName("categoryOptions")] Category[]? Categories);
public record Entry([property: JsonPropertyName("enrollmentID")] int Id, string Status,
    Race Event, Category? Category, Identity? Participant, RaceResult? Result);
public record RaceResult([property: JsonPropertyName("resultID")] int Id, TimeSpan? FinishTime, int? Position,
    [property: JsonPropertyName("resultStatus")] string Status, Race? Event, string? Category, int TotalFinishers)
{
    public string Placing => Status == "Finished" ? $"{Position} of {TotalFinishers}" : "—";
}
public record Profile([property: JsonPropertyName("userID")] int Id, string FirstName, string LastName,
    string Email, string? Phone, string? ProfileImageUrl);
public record RaceList(Race[] Races, string? Type, DateTime FromDate);
public record ManageRace(Race Race, Entry[] Entries, Category[] Categories);
public record RaceEditor(int Id, CreateEventDto Fields, string? BannerUrl, Category[] Categories);
