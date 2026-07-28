namespace FrotaGo.Mobile.Core.Helpers;

public static class ApiEndpoints
{
    public const string BaseUrl = "https://app.frotago.com/api";
    public const string SignalRHubUrl = "https://app.frotago.com/hubs/tracking";

    public static class Auth
    {
        public const string Login = "/auth/login";
        public const string RefreshToken = "/auth/refresh";
    }

    public static class Mobile
    {
        public const string Profile = "/mobile/profile";
        public const string TodayLessons = "/mobile/lessons/today";
        public static string LessonDetails(Guid id) => $"/mobile/lessons/{id}";
        public const string StartLesson = "/mobile/lessons/start";
        public const string FinishLesson = "/mobile/lessons/finish";
        public const string StartTracking = "/mobile/tracking/start";
        public const string SendTelemetry = "/mobile/tracking/telemetry";
        public const string EndTracking = "/mobile/tracking/end";
    }
}
