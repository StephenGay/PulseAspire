using Pulse.Models.Production;

namespace Pulse.Models.Api
{
    /// <summary>
    /// Centralized API endpoint constants for Pulse.ApiService.
    /// Organize by feature/module for maintainability.
    /// </summary>
    public static class ApiEndpoints
    {
        private const string ApiPrefix = "";

        #region Security Endpoints

            public static class Security
            {
                private const string Prefix = $"{ApiPrefix}/Security";

                public const string Login = $"{Prefix}/login";
                public const string Register = $"{Prefix}/register";
                public const string UpdateUser = $"{Prefix}/UpdateUser";
                public const string Logout = $"{Prefix}/logout";
                public const string RefreshToken = $"{Prefix}/refresh-token";
                public const string CreateRole = $"{Prefix}/roles";
                public const string GetAllUsers = $"{Prefix}/users";
                public const string AssignRole = $"{Prefix}/assign-role";
                public const string RemoveRole = $"{Prefix}/remove-role";
                public const string GetRoles = $"{Prefix}/roles";
                public const string ChangePassword = $"{Prefix}/change-password";

                public static class Permissions
                {
                    private const string PermissionsPrefix = $"{Prefix}/permissions";
                    public const string GetAll = $"{PermissionsPrefix}/GetAll";
                    public const string Create = $"{PermissionsPrefix}/Create";
                    public const string Update = $"{PermissionsPrefix}/Update";
                    //public const string Delete = $"{PermissionsPrefix}/Delete";
                    public static string GetByRoleID(string roleId) => $"{PermissionsPrefix}/GetByRole/{roleId}";
                    public static string GetAssignedRoles(int permissionId) => $"{PermissionsPrefix}/GetAssignedRoles/{permissionId}";
                    public const string AssignToRole = $"{PermissionsPrefix}/AssignToRole";
                    public static string RemoveFromRole(string roleId, int permissionId) => $"{PermissionsPrefix}/RemoveFromRole/{roleId}/{permissionId}";
                    public static string GetByCategory(string category) => $"{PermissionsPrefix}/GetByCategory/{Uri.EscapeDataString(category)}";

                    public static class Categories
                    {
                        private const string CategoriesPrefix = $"{PermissionsPrefix}/categories";
                        public const string GetAll = $"{CategoriesPrefix}/GetAll";
                        public const string Create = $"{CategoriesPrefix}/Create";
                        //public const string Update = $"{CategoriesPrefix}/Update";
                        //public const string Delete = $"{CategoriesPrefix}/Delete";
                    }
                }

                public static class Roles
                {
                    private const string RolesPrefix = $"{Prefix}/Roles";
                    public const string GetAll = $"{RolesPrefix}/GetAll";
                    public const string Create = $"{RolesPrefix}/Add";

                }
            }

        #endregion

        #region AI Endpoints


        public static class PulseAi
        {
            private const string Prefix = $"{ApiPrefix}/PulseAI";

            public static class EndpointsTables
            {
                private const string TablesPrefix = $"{Prefix}/Tables";
                public const string SendRequest = $"{TablesPrefix}/SendRequest";
            }
            public static class EndpointsFlapper
            {
                private const string FlapperPrefix = $"{Prefix}/Flapper";
                public const string SendRequest = $"{FlapperPrefix}/SendRequest";
                public const string ChatSession = $"{FlapperPrefix}/ChatSession";
                public const string OllamaChatSession = $"{FlapperPrefix}/OllamaChatSession";
                public static string GetUserConversationHistory(string userId) => $"{FlapperPrefix}/Conversations/UserHistory/{userId}";
                public static string GetConversationMessages(Guid conversationId) => $"{FlapperPrefix}/Conversations/GetMessages/{conversationId}";
            }
            public static class EndpointsAli
            {
                private const string AliPrefix = $"{Prefix}/Ali";
                public const string SendRequest = $"{AliPrefix}/SendRequest";

                public static class CustomPrompts
                {
                    private const string CustomPromptsPrefix = $"{AliPrefix}/CustomPrompts";
                    public const string GetAll = $"{CustomPromptsPrefix}/GetAll";
                    public const string Add = $"{CustomPromptsPrefix}/Add";

                }

                public static class Analysis
                {
                    private const string AnalysisPrefix = $"{AliPrefix}/Analysis";
                    public const string AddToQueue = $"{AnalysisPrefix}/AddToQueue";

                }
            }
        }

        public static class Ai
        {
            private const string Prefix = $"{ApiPrefix}/AI";

            public const string Schema = $"{Prefix}/schema";
            public const string Examples = $"{Prefix}/examples";
            public const string AiQuery = $"{Prefix}/aiquery";
            public const string SavedQueries = $"{Prefix}/savedqueries";
            public const string ContextualPrompt = $"{Prefix}/ContextualPrompt";
            public const string Execute = $"{Prefix}/execute";
            public const string ExecuteActionSql = $"{Prefix}/ExecuteAiUpdateInsertQry";

            /// <summary>
            /// Vote on a saved query usefulness.
            /// Usage: {SavedQueries}/vote/{queryId}/{vote}
            /// </summary>
            public static string Vote(int queryId, string vote) =>
                $"{SavedQueries}/vote/{queryId}/{Uri.EscapeDataString(vote)}";
        }

        #endregion

        #region User Endpoints

        public static class User
        {
            private const string Prefix = $"{ApiPrefix}/User";

            public const string Profile = $"{Prefix}/profile";
            public const string SendWelcomeEmail = $"{ApiPrefix}/Security/SendWelcomeEmail";
            public const string SendForcePasswordResetEmail = $"{ApiPrefix}/Security/ForcePasswordReset";

            public static class Settings
            {
                private const string SettingsPrefix = $"{Prefix}/Settings";

                public static string GetSettings(string userId) => $"{SettingsPrefix}/GetSettings/{userId}";
                public const string Update = $"{SettingsPrefix}/Update";
            }
            
            public static class SpeedDial
            {
                private const string SpeedDialPrefix = $"{Prefix}/SpeedDial";
                public static string GetByUserId(string userId) => $"{SpeedDialPrefix}/GetByUserId/{userId}";
                public const string Add = $"{SpeedDialPrefix}/Add/";
                /// <summary>
                /// Delete a speed dial item.
                /// Usage: {Delete}/{itemId}
                /// </summary>
                public static string Delete(int itemId) => $"{SpeedDialPrefix}/Delete/{itemId}";
            }
            public static class Favourites
            {
                private const string FavouritePrefix = $"{Prefix}/Favourites";

                public static class SavedQueries
                {
                    private const string QueriesPrefix = $"{FavouritePrefix}/Queries";

                    public static string GetUserFavourites(string userId) => $"{QueriesPrefix}/GetByUserId/{userId}";
                    public const string Add = $"{QueriesPrefix}/Add";

                    /// <summary>
                    /// Delete a saved favourite query.
                    /// Usage: {Delete}/{queryId}
                    /// </summary>
                    public static string Delete(int queryId) => $"{QueriesPrefix}/Delete/{queryId}";
                }
            }
        }

        #endregion

        #region Company Endpoints

        public static class Companies
        {
            private const string Prefix = $"{ApiPrefix}/Companies";
            public static string GetById(int companyId) => $"{Prefix}/GetById/{companyId}";
        }
        #endregion

            #region Division Endpoints

            public static class Divisions
        {
            private const string Prefix = $"{ApiPrefix}/Divisions";
            public const string GetActive = $"{Prefix}/GetActive";

            public static class WithDivisionID
            {
                private const string WithDivisionIDPrefix = $"{Prefix}/WithDivisionID";

                public static class Factory
                {
                    private const string FactorySuffix = "/Factory";
                    public static class WorkCentres
                    {
                        private const string WorkCentreSuffix = $"{FactorySuffix}/WorkCentres";
                        public static string GetAll(string divisionId) => $"{WithDivisionIDPrefix}/{Uri.EscapeDataString(divisionId)}{WorkCentreSuffix}/GetAll";
                        public static string GetWorkCentreZones(string divisionId, int workCentreId) => $"{WithDivisionIDPrefix}/{Uri.EscapeDataString(divisionId)}{WorkCentreSuffix}/{workCentreId}/GetWCZones";
                        public static string Create(string divisionId) => $"{WithDivisionIDPrefix}/{Uri.EscapeDataString(divisionId)}{WorkCentreSuffix}/Create";
                        public static string Update(string divisionId) => $"{WithDivisionIDPrefix}/{Uri.EscapeDataString(divisionId)}{WorkCentreSuffix}/Update";
                        
                    }

                    public static class FactoryLayout
                    {
                        private const string FactoryLayoutSuffix = $"{FactorySuffix}/FactoryLayout";
                        public static string SaveLayout(string divisionId,string parentId) => $"{WithDivisionIDPrefix}/{Uri.EscapeDataString(divisionId)}{FactoryLayoutSuffix}/{Uri.EscapeDataString(parentId)}/SaveLayout";

                        public static class Zones
                        {
                            private const string ZonesSuffix = $"{FactoryLayoutSuffix}/Zones";
                            public static string GetAll(string divisionId, string parentId) => $"{WithDivisionIDPrefix}/{Uri.EscapeDataString(divisionId)}{ZonesSuffix}/{Uri.EscapeDataString(parentId)}/GetAll";
                            public static string Create(string divisionId) => $"{WithDivisionIDPrefix}/{Uri.EscapeDataString(divisionId)}{ZonesSuffix}/Create";
                            public static string Update(string divisionId) => $"{WithDivisionIDPrefix}/{Uri.EscapeDataString(divisionId)}{ZonesSuffix}/Update";
                            public static string Save(string divisionId) => $"{WithDivisionIDPrefix}/{Uri.EscapeDataString(divisionId)}{ZonesSuffix}/Save";
                        }
                    }
                }
            }

                public static class Equipment
            {
                private const string EquipmentPrefix = $"{Prefix}/Equipment";

                public static class Capabilities
                {
                    private const string CapabilitiesPrefix = $"{EquipmentPrefix}/Capabilities";

                    public const string Add = $"{CapabilitiesPrefix}/Add";
                    public const string Update = $"{CapabilitiesPrefix}/Update";
                    public const string Delete = $"{CapabilitiesPrefix}/Delete";
                }
            }

            public static class WorkCentre
            {
                private const string WorkCentrePrefix = $"{Prefix}/WorkCentre";

                public static class Functions
                {
                    private const string FunctionsPrefix = $"{WorkCentrePrefix}/Functions";

                    public const string Add = $"{FunctionsPrefix}/Add";
                    public const string Update = $"{FunctionsPrefix}/Update";
                    public const string Delete = $"{FunctionsPrefix}/Delete";
                }
            }

            public static class Production
            {
                private const string ProductionPrefix = $"{Prefix}/Production";

                public static class WorkOrder
                {
                    /// <summary>
                    /// Get production plan for work order.
                    /// Usage: {GetProductionPlan(workOrderNumber)}
                    /// </summary>
                    public static string GetProductionPlan(int workOrderNumber) =>
                        $"{ProductionPrefix}/WorkOrder/{workOrderNumber}/GetProductionPlan";

                    /// <summary>
                    /// Create production plan for work order.
                    /// Usage: {CreateProductionPlan(workOrderNumber)}
                    /// </summary>
                    public static string CreateProductionPlan(int workOrderNumber) =>
                        $"{ProductionPrefix}/WorkOrder/{workOrderNumber}/CreateProductionPlan";
                    public static string RemoveProductionPlan(int workOrderNumber) =>
                        $"{ProductionPrefix}/WorkOrder/{workOrderNumber}/RemoveProductionPlan";
                }
            }

            public static class WIP
            {
                private const string WipPrefix = $"{Prefix}/WIP";

                /// <summary>
                /// Update a production plan item.
                /// Usage: {UpdateProductionPlanItem(planItemId)}
                /// </summary>
                public static string UpdateProductionPlanItem(int planItemId) =>
                    $"{WipPrefix}/UpdateProductionPlanItem/{planItemId}";
            }
        }

        #endregion

        #region Customer Endpoints

        public static class Customers
        {
            private const string Prefix = $"{ApiPrefix}/Customers";

            public static class Details
            {
                /// <summary>
                /// Update customer master file.
                /// Usage: {UpdateMasterFile(fullClientId)}
                /// </summary>
                public static string UpdateMasterFile(string fullClientId) =>
                    $"{Prefix}/Details/{Uri.EscapeDataString(fullClientId)}/Update/MasterFile";

                /// <summary>
                /// Get customer sales history.
                /// Usage: {GetSales(clientId)}
                /// </summary>
                public static string GetSales(string clientId) =>
                    $"{Prefix}/Details/{Uri.EscapeDataString(clientId)}/Sales";

                /// <summary>
                /// Get customer current statistics.
                /// Usage: {GetCurrentStats(clientId)}
                /// </summary>
                public static string GetCurrentStats(string clientId) =>
                    $"{Prefix}/Details/{Uri.EscapeDataString(clientId)}/CurrentStats";

                /// <summary>
                /// Get customer roller specifications.
                /// Usage: {GetRollerSpecifications(clientId)}
                /// </summary>
                public static string GetRollerSpecifications(string clientId) =>
                    $"{Prefix}/Details/{Uri.EscapeDataString(clientId)}/RollerSpecifications";
            }

            public static class RollerSpecifications 
            {
                private const string RollerSpecificationsPrefix = $"{Prefix}/RollerSpecifications";
                public static string Get3DModel(string rollerSpecId) => $"/3DRoller/{rollerSpecId}";
                public static string GetActiveRollerShafts(string rollerSpecId) => $"{RollerSpecificationsPrefix}/GetActiveRollerShafts/{Uri.EscapeDataString(rollerSpecId)}";
                public static string GetAllDetailsBySpecID(string rollerSpecId) => $"{RollerSpecificationsPrefix}/AllDetailsBySpecID/{Uri.EscapeDataString(rollerSpecId)}";
                public static string GetWorkOrdersByRollNumber(int ClientRollerID) => $"{RollerSpecificationsPrefix}/GetWOByRollerID/{ClientRollerID}";
                public static string GetWorkOrdersBySpecificationID(string rollerSpecId) => $"{RollerSpecificationsPrefix}/GetWOBySpecID/{Uri.EscapeDataString(rollerSpecId)}";
            }
        
        }

        #endregion

        #region Production Endpoints

        public static class Production
        {
            private const string Prefix = $"{ApiPrefix}/Production";
            public static class WorkTypes
            {
                private const string WorkTypesPrefix = $"{Prefix}/WorkTypes";
                public const string GetAllActive = $"{WorkTypesPrefix}/GetAllActive";

                public static class Division
                {
                    private const string DivisionPrefix = $"{WorkTypesPrefix}/Division";
                    public static string GetAllActive(string divisionId) => $"{DivisionPrefix}/{Uri.EscapeDataString(divisionId)}/GetAllActive";
                    public static string Add(string divisionId) => $"{DivisionPrefix}/{Uri.EscapeDataString(divisionId)}/Add";
                }
            }
            ////public static class ProductionPlanning
            ////{
            ////    private const string ProductionPlanItemsPrefix = $"{Prefix}/ProductionPlanItems";
            ////    public static string Update(int planItemId) => $"{ProductionPlanItemsPrefix}/Update/{planItemId}";
            ////}
        }

        #endregion

        #region Default Endpoints

        public static class Default
        {
            private const string Prefix = $"{ApiPrefix}/default";

            public const string Health = $"{Prefix}/health";
        }

        #endregion

        #region Utilities Endpoints

        public static class Utilities
        {
            private const string Prefix = $"{ApiPrefix}/Utilities";
            public const string SendEmail = $"{Prefix}/SendEmail";
            public const string ExportPdf = $"{Prefix}/pdf/export";

        }
        #endregion
    }
}