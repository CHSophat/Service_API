namespace ApartmentManagementSystem.API.Configuration
{
    /// <summary>
    /// Centralized API endpoints configuration for all client applications
    /// Provides a single source of truth for all API endpoints
    /// </summary>
    public static class ApiEndpoints
    {
        /// <summary>
        /// Base API versioning
        /// </summary>
        public const string ApiVersion = "v1";
        public const string ApiBaseRoute = $"/api/{ApiVersion}";

        #region Authentication Endpoints
        /// <summary>
        /// Authentication endpoints
        /// </summary>
        public static class Auth
        {
            public const string Base = $"{ApiBaseRoute}/auth";
            public const string Login = $"{Base}/login";
            public const string Signup = $"{Base}/signup";
            public const string Logout = $"{Base}/logout";
            public const string Refresh = $"{Base}/refresh";
            public const string Verify2FA = $"{Base}/verify-2fa";
            public const string ResendOTP = $"{Base}/resend-otp";
            public const string ForgotPassword = $"{Base}/forgot-password";
            public const string ResetPassword = $"{Base}/reset-password";
            public const string ValidateToken = $"{Base}/validate-token";
            public const string GoogleLogin = $"{Base}/google";
            public const string FacebookLogin = $"{Base}/facebook";
        }
        #endregion

        #region User Endpoints
        /// <summary>
        /// User management endpoints
        /// </summary>
        public static class Users
        {
            public const string Base = $"{ApiBaseRoute}/users";
            public const string Profile = $"{Base}/profile";
            public const string UpdateProfile = $"{Base}/profile";
            public const string Avatar = $"{Base}/profile/avatar";
            public const string Documents = $"{Base}/documents";
            public const string Addresses = $"{Base}/addresses";
            public const string Preferences = $"{Base}/preferences";
            public const string ActivityLog = $"{Base}/activity-log";
            public static string GetById(string id) => $"{Base}/{id}";
        }
        #endregion

        #region Property/Unit Endpoints
        /// <summary>
        /// Property and unit management endpoints
        /// </summary>
        public static class Properties
        {
            public const string Base = $"{ApiBaseRoute}/properties";
            public const string List = $"{Base}";
            public const string Search = $"{Base}/search";
            public const string Create = $"{Base}";
            public const string Statistics = $"{Base}/statistics";
            public static string GetById(string id) => $"{Base}/{id}";
            public static string Update(string id) => $"{Base}/{id}";
            public static string Delete(string id) => $"{Base}/{id}";
            public static string GetUnits(string id) => $"{Base}/{id}/units";
            public static string GetImages(string id) => $"{Base}/{id}/images";
        }
        #endregion

        #region Unit/Apartment Endpoints
        /// <summary>
        /// Unit/apartment endpoints
        /// </summary>
        public static class Units
        {
            public const string Base = $"{ApiBaseRoute}/units";
            public const string List = $"{Base}";
            public const string Search = $"{Base}/search";
            public const string Create = $"{Base}";
            public static string GetById(string id) => $"{Base}/{id}";
            public static string Update(string id) => $"{Base}/{id}";
            public static string Delete(string id) => $"{Base}/{id}";
            public static string GetImages(string id) => $"{Base}/{id}/images";
            public static string GetTenants(string id) => $"{Base}/{id}/tenants";
            public static string GetLeases(string id) => $"{Base}/{id}/leases";
        }
        #endregion

        #region Lease Endpoints
        /// <summary>
        /// Lease management endpoints
        /// </summary>
        public static class Leases
        {
            public const string Base = $"{ApiBaseRoute}/leases";
            public const string List = $"{Base}";
            public const string Create = $"{Base}";
            public const string Active = $"{Base}/active";
            public const string Expiring = $"{Base}/expiring";
            public static string GetById(string id) => $"{Base}/{id}";
            public static string Update(string id) => $"{Base}/{id}";
            public static string Delete(string id) => $"{Base}/{id}";
            public static string Renew(string id) => $"{Base}/{id}/renew";
            public static string GetDocuments(string id) => $"{Base}/{id}/documents";
            public static string TerminateLease(string id) => $"{Base}/{id}/terminate";
        }
        #endregion

        #region Payment Endpoints
        /// <summary>
        /// Payment management endpoints
        /// </summary>
        public static class Payments
        {
            public const string Base = $"{ApiBaseRoute}/payments";
            public const string List = $"{Base}";
            public const string Create = $"{Base}";
            public const string History = $"{Base}/history";
            public const string Methods = $"{Base}/methods";
            public const string BakongQR = $"{Base}/bakong-qr";
            public const string Invoice = $"{Base}/invoice";
            public const string Statistics = $"{Base}/statistics";
            public const string Outstanding = $"{Base}/outstanding";
            public static string GetById(string id) => $"{Base}/{id}";
            public static string Confirm(string id) => $"{Base}/{id}/confirm";
            public static string Cancel(string id) => $"{Base}/{id}/cancel";
            public static string GetReceipt(string id) => $"{Base}/{id}/receipt";
        }
        #endregion

        #region Maintenance Endpoints
        /// <summary>
        /// Maintenance request and ticket management endpoints
        /// </summary>
        public static class Maintenance
        {
            public const string Base = $"{ApiBaseRoute}/maintenance";
            
            // Requests
            public const string RequestsBase = $"{Base}/requests";
            public const string RequestList = $"{RequestsBase}";
            public const string RequestCreate = $"{RequestsBase}";
            public const string MyRequests = $"{RequestsBase}/my-requests";
            public static string RequestGetById(string id) => $"{RequestsBase}/{id}";
            public static string RequestUpdate(string id) => $"{RequestsBase}/{id}";
            public static string RequestDelete(string id) => $"{RequestsBase}/{id}";
            public static string RequestAssign(string id) => $"{RequestsBase}/{id}/assign";

            // Tickets
            public const string TicketsBase = $"{Base}/tickets";
            public const string TicketList = $"{TicketsBase}";
            public const string TicketCreate = $"{TicketsBase}";
            public static string TicketGetById(string id) => $"{TicketsBase}/{id}";
            public static string TicketUpdate(string id) => $"{TicketsBase}/{id}";
            public static string TicketClose(string id) => $"{TicketsBase}/{id}/close";

            // Schedule
            public const string Schedule = $"{Base}/schedule";
            public const string Technicians = $"{Base}/technicians";
        }
        #endregion

        #region Communication Endpoints
        /// <summary>
        /// Communication, messaging, and notification endpoints
        /// </summary>
        public static class Communication
        {
            public const string Base = $"{ApiBaseRoute}/communication";
            
            // Conversations
            public const string ConversationsBase = $"{Base}/conversations";
            public const string ConversationList = $"{ConversationsBase}";
            public const string ConversationCreate = $"{ConversationsBase}";
            public static string ConversationGetById(string id) => $"{ConversationsBase}/{id}";
            public static string ConversationMessages(string id) => $"{ConversationsBase}/{id}/messages";
            public static string SendMessage(string id) => $"{ConversationsBase}/{id}/messages";

            // Notifications
            public const string NotificationsBase = $"{Base}/notifications";
            public const string NotificationList = $"{NotificationsBase}";
            public const string UnreadCount = $"{NotificationsBase}/unread-count";
            public static string MarkAsRead(string id) => $"{NotificationsBase}/{id}/read";
            public static string MarkAsUnread(string id) => $"{NotificationsBase}/{id}/unread";
            public const string MarkAllAsRead = $"{NotificationsBase}/mark-all-read";

            // Announcements
            public const string AnnouncementsBase = $"{Base}/announcements";
            public const string AnnouncementList = $"{AnnouncementsBase}";
            public static string GetAnnouncement(string id) => $"{AnnouncementsBase}/{id}";
        }
        #endregion

        #region Dashboard Endpoints
        /// <summary>
        /// Dashboard statistics and overview endpoints
        /// </summary>
        public static class Dashboard
        {
            public const string Base = $"{ApiBaseRoute}/dashboard";
            public const string TenantStats = $"{Base}/tenant/stats";
            public const string OwnerStats = $"{Base}/owner/stats";
            public const string AdminStats = $"{Base}/admin/stats";
            public const string PropertyOverview = $"{Base}/property-overview";
            public const string RevenueAnalytics = $"{Base}/revenue-analytics";
            public const string OccupancyRate = $"{Base}/occupancy-rate";
        }
        #endregion

        #region Admin Endpoints
        /// <summary>
        /// Administrative endpoints
        /// </summary>
        public static class Admin
        {
            public const string Base = $"{ApiBaseRoute}/admin";
            public const string UserManagement = $"{Base}/users";
            public const string PropertyManagement = $"{Base}/properties";
            public const string Reports = $"{Base}/reports";
            public const string AuditLogs = $"{Base}/audit-logs";
            public const string SystemSettings = $"{Base}/system-settings";
            public static string GetReport(string reportType) => $"{Reports}?type={reportType}";
        }
        #endregion

        #region Upload Endpoints
        /// <summary>
        /// File upload endpoints
        /// </summary>
        public static class Uploads
        {
            public const string Base = $"{ApiBaseRoute}/uploads";
            public const string ProfileImage = $"{Base}/profile-image";
            public const string PropertyImage = $"{Base}/property-image";
            public const string Document = $"{Base}/document";
            public const string Video = $"{Base}/video";
        }
        #endregion

        #region Health Check
        /// <summary>
        /// Health check and monitoring endpoints
        /// </summary>
        public static class Health
        {
            public const string Base = "/health";
            public const string Status = $"{Base}/status";
            public const string Ready = $"{Base}/ready";
            public const string Live = $"{Base}/live";
        }
        #endregion
    }
}
