using KockpitPortal.Models.PlanManagement;
using KockpitPortal.Models.SuperAdmin;
using KockpitPortal.Models.Support;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;

namespace KockpitPortal.DataAccessLayer
{
    public class DataPostgres
    {
        DataMethodPostgres LocalDM;
        DataSet _localDS;
        string _Query = string.Empty;
        private string _ConnectionStr = string.Empty;
        private string strQuery;

        private string _Schema = string.Empty;

        public DataPostgres(string ConnectionStr)
        {
            this._ConnectionStr = ConnectionStr;
            LocalDM = new DataMethodPostgres(this._ConnectionStr);
        }

        public DataPostgres(DataSet LocalDS, string ConnectionStr, string Schema = "")
        {
            this._ConnectionStr = ConnectionStr;
            LocalDM = new DataMethodPostgres(this._ConnectionStr, LocalDS);
            this._localDS = LocalDS;
            this._Schema = Schema;
        }

        public void ExecuteSelectCommand(string strQuery, string strTableAlias)
        {
            LocalDM.ExecuteSelectReaderCommand(strQuery, null, strTableAlias, true);
        }
        public bool ExecuteNonQuery(string strQuery)
        {
            return LocalDM.ExecuteNonQueryCommand(strQuery, null, true);
        }
        public bool ExecuteNonQuery(string strQuery, NpgsqlParameter[] sqlpara)
        {
            return LocalDM.ExecuteNonQueryCommand(strQuery, sqlpara, true);
        }

        //public void AuthenticateLogin(string strEmailId, string strPassword, string strTableAlias, string strSchema = "")
        //{
        //    strQuery = $@"SELECT * FROM ""Users"" WHERE Username = '{strEmailId}' AND Password = '{strPassword}'";
        //    LocalDM.ExecuteSelectQuery(strQuery, strTableAlias);
        //}

        #region [Authentication]
        public void GetLogo(string strSchemaName, string strTableAlias)
        {
            strQuery = $@"SELECT ""logo"" FROM ""tblUsers"" where ""schemaname"" = '{strSchemaName}' AND ""logo"" != '' AND ActiveStatus = true limit 1;";
            LocalDM.ExecuteSelectQuery(strQuery, strTableAlias);
        }

        public void AuthenticateLogin(string strEmailId, string strPassword, string strTableAlias, string strSchema = "")
        {
            if (!string.IsNullOrEmpty(strSchema))
                strQuery = $@"SELECT * FROM ""tblUsers"" WHERE EmailId = '{strEmailId}' AND Password = '{strPassword}' AND SchemaName = '{strSchema}' AND ActiveStatus = true";
            else
                strQuery = $@"SELECT * FROM ""tblUsers"" WHERE EmailId = '{strEmailId}' AND Password = '{strPassword}' AND ActiveStatus = true";
            LocalDM.ExecuteSelectQuery(strQuery, strTableAlias);
        }

        public void CheckDomainByUser(string strEmailId, string strSchema, string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblUsers"" WHERE EmailId = '{strEmailId}' AND SchemaName = '{strSchema}' AND ActiveStatus = true";
            LocalDM.ExecuteSelectQuery(strQuery, strTableAlias);
        }

        public Tuple<bool, int> UserSelectByEmail(string strEmailId)
        {
            strQuery = $@"SELECT * FROM ""tblUsers"" WHERE EmailId = '{strEmailId}' AND ActiveStatus = true";
            LocalDM.ExecuteSelectQuery(strQuery, "tUserSelectByEmail");
            if (this._localDS.Tables["tUserSelectByEmail"] != null && this._localDS.Tables["tUserSelectByEmail"].Rows.Count > 0)
                return new Tuple<bool, int>(true,Convert.ToInt32(this._localDS.Tables["tUserSelectByEmail"].Rows[0]["Id"].ToString()));
            else
                return new Tuple<bool, int>(false, 0);
        }

        public void GetUsersinfo(int userId, string strTableAlias)
        {
            string strQuery = $@"SELECT * FROM ""tblUsers"" WHERE id = {userId}";
            LocalDM.ExecuteSelectQuery(strQuery, strTableAlias);
        }

        public bool UpdateUserProfile(int userId, string contact1, string contact2, string address, string image)
        {
            string strQuery = $@"UPDATE ""tblUsers"" SET contactno1 = @contact1, contactno2 = @contact2, address = @address, logo = @logo, lastupdatedate = current_date where id = @id";

            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                new Npgsql.NpgsqlParameter("@id",userId),
                new Npgsql.NpgsqlParameter("@contact1", (object)contact1 ?? DBNull.Value),
                new Npgsql.NpgsqlParameter("@contact2", (object)contact2 ?? DBNull.Value),
                new Npgsql.NpgsqlParameter("@address",(object) address ?? DBNull.Value),
                new Npgsql.NpgsqlParameter("@logo", (object)image ?? DBNull.Value),
            };
            return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters);
        }

        public void VerifyPassword(int userId, string password, string strTableAlias)
        {
            string strQuery = $@"SELECT * FROM ""tblUsers"" WHERE id = {userId} AND password = '{password}'";
            LocalDM.ExecuteSelectQuery(strQuery, strTableAlias);
        }
        public bool UpdatePassword(int id, string password)
        {
            strQuery = $@"UPDATE ""tblUsers"" SET password = '{password}' WHERE id = {id}";
            return LocalDM.ExecuteNonQueryCommand(strQuery);
        }

        public void AuthenticateLicense(string strEmailId, string strLicenseKey, string strTableAlias)
        {
            string sRole = string.Empty;
            strQuery = $@"SELECT Role FROM ""tblUsers"" Where EmailId = '{strEmailId}' AND ActiveStatus = true";
            LocalDM.ExecuteSelectCommand(strQuery, "tAuthenticateLicenseRole");
            if (_localDS.Tables["tAuthenticateLicenseRole"] != null &&
                _localDS.Tables["tAuthenticateLicenseRole"].Rows.Count > 0)
            {
                sRole = this._localDS.Tables["tAuthenticateLicenseRole"].Rows[0]["Role"].ToString().Trim();
            }

            if(sRole == "ADMIN")
            {
                strQuery = $@"select a.* ,b.CompanyName, b.Logo, 
                        b.Id as CompanyId, b.CompanyName AS ParentCompany, b.SocketId, b.SchemaName AS CompanySchema,
                        b.EmpCode, b.ContactNo1, b.ContactNo2, b.SchemaName, b.Role
                        from ""tblPlanPurchaseDetail"" a
                        inner join ""tblUsers"" b ON a.UserId = b.Id
                        Where b.EmailId = '{strEmailId}' and a.LicenseKey = '{strLicenseKey}' and (a.IsExpired is null or a.IsExpired = false)";
            }
            else if(sRole == "SUBADMIN")
            {
                strQuery = $@"select a.* ,b.CompanyName, b.Logo, 
                        c.Id as CompanyId, c.CompanyName AS ParentCompany, c.SocketId, c.SchemaName AS CompanySchema,
                        b.EmpCode, b.ContactNo1, b.ContactNo2, b.SchemaName, b.Role
                        from ""tblPlanPurchaseDetail"" a
                        inner join ""tblUsers"" b ON a.UserId = b.Id
                        left join ""tblUsers"" c ON b.CreatedBy = c.Id
                        Where b.EmailId = '{strEmailId}' and a.LicenseKey = '{strLicenseKey}' and (a.IsExpired is null or a.IsExpired = false)";
            }
            LocalDM.ExecuteSelectQuery(strQuery, strTableAlias);
        }

        public void AuthenticateCompanyDomain(string strCompanDomain, string strTableAlias)
        {
            strQuery = $@"select * from ""tblUsers"" 
                        Where SchemaName = '{strCompanDomain}' AND ActiveStatus = true";
            LocalDM.ExecuteSelectQuery(strQuery, strTableAlias);
        }


        public bool UpdateLicenseDeviceId(string strLicenseKey, string strDeviceId)
        {
            strQuery = $@"UPDATE ""tblPlanPurchaseDetail"" SET DeviceId = '{strDeviceId}' WHERE LicenseKey = '{strLicenseKey}'";
            return LocalDM.ExecuteNonQueryCommand(strQuery);
        }

        public bool RemoveLicenseDeviceId(string strEmailId, string strLicenseKey)
        {
            strQuery = $@"UPDATE ""tblPlanPurchaseDetail"" SET DeviceId = '' WHERE LicenseKey = '{strLicenseKey}'";
            return LocalDM.ExecuteNonQueryCommand(strQuery);
        }

        public bool ChangePostgreSqlPassword(string strUsername, string strNewpassword)
        {
            strQuery = $@"ALTER USER ""{strUsername}"" WITH PASSWORD '{strNewpassword}';";
            return LocalDM.ExecuteNonQueryCommand(strQuery, null, true);
        }

        public bool ChangePassword(int nId, string strNewpassword)
        {
            strQuery = $@"UPDATE ""tblUsers"" SET Password = '{strNewpassword}' WHERE Id = {nId}";
            return LocalDM.ExecuteNonQueryCommand(strQuery, null, true);
        }

        public void ValidateOldPassword(string strEmailId, string strPassword, string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblUsers"" WHERE EmailId = '{strEmailId}' AND Password = '{strPassword}' AND ActiveStatus = true";
            LocalDM.ExecuteSelectQuery(strQuery, strTableAlias);
        }

        public void GetUsersEmpCodeByEmail(string emailId, string strTableAlias)
        {
            string strQuery = $@"SELECT empcode FROM ""tblUsers"" WHERE emailid = '{emailId}' and empcode <> '' and empcode is not null";
            LocalDM.ExecuteSelectQuery(strQuery, strTableAlias);
        }

        #endregion


        #region [Dashboard]
        public void ClientDashboard(int nUserId, bool IsAdmin, string strTableAlias)
        {
            strQuery = $@"
                        SELECT c.* , a.LicenseKey
                        FROM ""tblPlanPurchaseDetail"" a
                        INNER JOIN ""tblPlan"" b ON a.PlanId = b.Id
                        INNER JOIN ""tblProject"" c ON b.ProjectId = c.Id
                        WHERE a.UserId = {nUserId} AND(a.IsExpired is null or a.IsExpired = false)
                        AND c.ActiveStatus = true";
                    
            if(IsAdmin)
            {
                strQuery += $@" UNION
                        SELECT x.*, '' AS LicenseKey
                        FROM ""tblProject"" x
                        WHERE x.Id NOT IN
                        (
                            SELECT c.Id
                            FROM ""tblPlanPurchaseDetail"" a
                            INNER JOIN ""tblPlan"" b ON a.PlanId = b.Id
                            INNER JOIN ""tblProject"" c ON b.ProjectId = c.Id
                            WHERE a.UserId = {nUserId} AND (a.IsExpired is null or a.IsExpired = false) AND c.ActiveStatus = true
                        ) AND x.ActiveStatus = true";
            }   


            strQuery += $@" ;SELECT * FROM ""tblNotification""
                            WHERE isread = false AND refrenceid = {nUserId} 
                            order by createdon desc limit 3;
        
                        SELECT a.*,b.projectname,c.planname FROM ""tblCart"" a
                        INNER JOIN ""tblProject"" b
                            ON a.offeringid = b.id
                        INNER JOIN ""tblPlan"" c
                            ON a.planid = c.id
                        AND a.userid = {nUserId} order by a.createdon desc;";
            LocalDM.ExecuteSelectQuery(strQuery, strTableAlias);
        }

        public void SuperAdminDashboard(int nUserId,string strTableAlias)
        {
            strQuery = $@"
            Select * from ""tblUsers"" Where Role = 'ADMIN' and ActiveStatus = true;
            Select * from ""tblUsers"" Where Role = 'SUPPORTMANAGER' and ActiveStatus = true;
            Select * from ""tblUsers"" Where Role = 'SUPPORTREPRESENTATIVE' and ActiveStatus = true;
            Select COUNT(*) from ""tblPlanPurchaseDetail"";
            Select COUNT(*) from ""tblPlanPurchaseDetail"" Where UserId Is not null and(IsExpired = false or IsExpired is null);
            Select COUNT(*) from ""tblPlanPurchaseDetail"" Where UserId Is null and(IsExpired = false or IsExpired is null);
            Select COUNT(*) from ""tblPlanPurchaseDetail"" Where IsExpired = true;
            SELECT COUNT(*) FROM ""tblPlanRequest"" WHERE IsApproved is NULL;
            SELECT * FROM ""tblNotification"" WHERE isread = false AND refrenceid = {nUserId} order by createdon desc limit 3;
            ";

            LocalDM.ExecuteSelectCommand(strQuery, strTableAlias);
        }
        #endregion

        #region Service
        public void GetService(string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblService"" WHERE ActiveStatus = true;";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias);
        }

        public void GetIssueService(int nUserId, string strTableAlias)
        {
            strQuery = $@"SELECT d.id ,d.servicename 
                                from  ""tblAMC"" e
                            INNER JOIN ""tblService"" d
                                ON e.serviceid = d.id
                    WHERE e.clientid = {nUserId} and (e.isexpired is null or e.isexpired = false)
                    and d.activestatus is true ";

            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public bool SaveServiceTicket(string ticketSubject, string uniqueId, string ticketDiscription, string attachment, int ticketLevel, int userId, int projectId, int issueId, string Scenario, DateTime dateTime)
        {
            int licenseid = 0;
            strQuery = $@"INSERT INTO ""tblTicket""(uniqueid,userid,licenseid,levelid,ticketsubject,description,ticketstatus,createdon,activestatus, lastupdatedate,issueid,scenario,isalreadyopen,amcid)
                        VALUES(@UniqueId,@UserId, @LicenseId, @LevelId, @TicketSubject,@Description, @TicketStatus, @createdon ,@ActiveStatus,@lastupdatedate,@IssueId, @Scenario,@isalreadyopen,@AmcId) RETURNING id; ";
            Npgsql.NpgsqlParameter[] npgsqlParameters1 = {
                        new Npgsql.NpgsqlParameter("@UniqueId",uniqueId),
                        new Npgsql.NpgsqlParameter("@UserId", userId),
                        new Npgsql.NpgsqlParameter("@LicenseId", licenseid),
                        new Npgsql.NpgsqlParameter("@LevelId", ticketLevel),
                        new Npgsql.NpgsqlParameter("@TicketSubject", ticketSubject),
                        new Npgsql.NpgsqlParameter("@Description", ticketDiscription),
                        new Npgsql.NpgsqlParameter("@TicketStatus","Open"),

                        new Npgsql.NpgsqlParameter("@ActiveStatus", true),
                        new Npgsql.NpgsqlParameter("@IssueId",issueId),
                        new Npgsql.NpgsqlParameter("@createdon",dateTime),
                        new Npgsql.NpgsqlParameter("@lastupdatedate",dateTime),

                        new Npgsql.NpgsqlParameter("@Scenario",Scenario),
                        new Npgsql.NpgsqlParameter("@isalreadyopen",false),
                        new Npgsql.NpgsqlParameter("@AmcId", projectId),

            };
            LocalDM.ExecuteSelectCommand(strQuery, npgsqlParameters1, "tTicketId");
            if (this._localDS.Tables["tTicketId"] != null && this._localDS.Tables["tTicketId"].Rows.Count > 0)
            {
                var ticketId = Convert.ToInt32(this._localDS.Tables["tTicketId"].Rows[0][0].ToString().Trim());
                //code to maintain history
                strQuery = $@"INSERT INTO ""tblTicketHistory""(ticketid,userid,  ticketstatus,remarks,attachment, ticketstatusdate,action)
                            VALUES (@TicketId,@userId, @TicketStatus, @remarks,@attachment,@ticketstatusdate ,@action)";
                attachment = attachment == null ? "" : attachment;
                Npgsql.NpgsqlParameter[] npgsqlParameters = {
                        new Npgsql.NpgsqlParameter("@TicketId", ticketId),
                        new Npgsql.NpgsqlParameter("@userId",userId),
                        new Npgsql.NpgsqlParameter("@TicketStatus", "Open"),
                        new Npgsql.NpgsqlParameter("@remarks","Ticket raised by client"),
                        new Npgsql.NpgsqlParameter("@attachment",attachment),
                        new Npgsql.NpgsqlParameter("@ticketstatusdate",dateTime),
                        new Npgsql.NpgsqlParameter("@action","Ticket logged"),
                    };
                return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters);
            }
            else
            {
                return false;
            }
        }

        public void GetAllServicePurchasedByClients(string strTableAlias)
        {
            string strQuery = $@"
                        SELECT x.* from
                        (SELECT distinct on (b.id) b.subscriptionId,b.id, a.CompanyName, a.EmailId, a.Logo, b.planid,c.ServiceName as planname,
                         b.ClientId, '' as PurchaseDate,
                        c.Id AS ProjectId, c.ServiceName,'' as iswarranty,'' as warrantyenddate,
                        0 AS TotalKeys,
                        0 AS UsedKeys,
                        0 AS UnusedKeys,
                        0 AS ExpiredKeys,
                        0 AS TransferedKeys
                       --b.Id, b.LicenseKey, e.ValidityDays, d.CompanyName AS AssignTo, b.ActivationDate, b.ExpiryDate, b.IsExpired
                       ,b.isexpired as AMCExpired,b.amcstartdate,b.amcenddate, b.amcenddate::date  - current_date::date as remainingdays, b.iscancelled as AMCCancelled,b.reissue,b.id as amcid
                        FROM  ""tblUsers"" a 
                        INNER JOIN ""tblAMC"" b on a.id = b.clientid
                        INNER JOIN ""tblService"" c ON b.ServiceId = c.Id and b.ServiceId is not null
                        WHERE b.isexpired = false OR b.isexpired is null 
                        ORDER BY  b.id desc)x order by x.clientid,x.planid,x.amcid desc; ";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void GetServiceForSupport(string strTableAlias)
        {
            string strQuery = $@"SELECT DISTINCT d.id ,d.servicename 
                                from  ""tblAMC"" e
                            INNER JOIN ""tblService"" d
                                ON e.serviceid = d.id";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }


        #endregion

        #region [Project]
        public void ProjectGetAll(string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblProject"" WHERE ActiveStatus = true;";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias);
        }
        
        public void ProjectCategoryAll(string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblOfferingCategory"" WHERE ActiveStatus = true;";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias);
        }
        public void ProjectGetById(int nId, string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblProject"" WHERE ActiveStatus = true AND Id = @Id;";
            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                new Npgsql.NpgsqlParameter("@Id", nId),
            };
            LocalDM.ExecuteSelectCommand(strQuery, npgsqlParameters, strTableAlias);
        }
        public void ProjectCheckDuplicacy(tblProject project, string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblProject"" WHERE ProjectName = @ProjectName AND Id != @Id;";
            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                new Npgsql.NpgsqlParameter("@Id",project.Id),
                new Npgsql.NpgsqlParameter("@ProjectName",project.ProjectName),
            };
            LocalDM.ExecuteSelectCommand(strQuery, npgsqlParameters, strTableAlias);
        }
        public bool ProjectUpsert(tblProject project)
        {
            if (project.Id == 0)
            {
                strQuery = $@"INSERT INTO ""tblProject""(OfferingCategory, ProjectName, Description, Version, ProjectType, ProjectImage, ProjectVideo,ProjectIcon, ProjectStartUpLink, CreatedOn, ActiveStatus, LastUpdateDate,IsPro,IsChat,)
		                    VALUES(@OfferingCategory, @ProjectName, @Description, @Version, @ProjectType, @ProjectImage, @ProjectVideo,@ProjectIcon, @ProjectStartUpLink, @CreatedOn, @ActiveStatus, current_date,@IsPro,@IsChat);";
            }
            else
            {
                strQuery = $@"UPDATE ""tblProject""
		                    SET 
                            OfferingCategory = @OfferingCategory,
		                    ProjectName = @ProjectName, 
		                    Description = @Description, 
		                    Version = @Version, 
		                    ProjectType = @ProjectType, 
		                    ProjectImage = @ProjectImage, 
                            ProjectVideo = @ProjectVideo,
                            ProjectStartUpLink = @ProjectStartUpLink,
		                    CreatedOn = @CreatedOn, 
		                    ActiveStatus = @ActiveStatus, 
		                    LastUpdateDate = current_date,
                            IsPro = @IsPro,
                            IsChat = @IsChat,
                            ProjectIcon = @ProjectIcon
		                    WHERE Id = @Id;";
            }

            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                new Npgsql.NpgsqlParameter("@Id",project.Id),
                new Npgsql.NpgsqlParameter("@OfferingCategory",project.OfferingCategory),
                new Npgsql.NpgsqlParameter("@ProjectName",project.ProjectName),
                new Npgsql.NpgsqlParameter("@Description",string.IsNullOrEmpty(project.Description) ? "" : project.Description),
                new Npgsql.NpgsqlParameter("@Version",project.Version),
                new Npgsql.NpgsqlParameter("@ProjectType",project.ProjectType),
                new Npgsql.NpgsqlParameter("@ProjectImage",project.ProjectImage),
                new Npgsql.NpgsqlParameter("@ProjectVideo",project.ProjectVideo),
                new Npgsql.NpgsqlParameter("@ProjectStartUpLink",project.ProjectStartUpLink),
                new Npgsql.NpgsqlParameter("@CreatedOn",project.CreatedOn),
                new Npgsql.NpgsqlParameter("@ActiveStatus",project.ActiveStatus),
                new Npgsql.NpgsqlParameter("@IsPro",project.IsPro),
                new Npgsql.NpgsqlParameter("@IsChat",project.IsChat),
                new Npgsql.NpgsqlParameter("@ProjectIcon",project.ProjectIcon),
            };
            return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters);
        }
        #endregion


        #region[tblOfferingCategory]

        public void GetOfferingCategory(string strTableAlias)
        {
            string strQuery = $@"select * from ""tblOfferingCategory"";";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }
        public bool OfferingCategoryUpsert(tblOfferingCategory offeringCategory)
        {
            if (offeringCategory.Id == 0)
            {
                strQuery = $@"INSERT INTO ""tblOfferingCategory""(offeringcategory, description, activestatus, createdon, lastupdatedate,ispro)
		                    VALUES(@offeringcategory, @description, @activestatus, current_date,current_date,@ispro);";
            }
            else
            {
                strQuery = $@"UPDATE ""tblOfferingCategory""
		                    SET 
		                    offeringcategory = @offeringcategory, 
		                    description = @description, 
		                    activestatus = @activestatus, 
		                    lastupdatedate = current_date,
                            ispro = @ispro
		                    WHERE Id = @Id;";
            }

            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                new Npgsql.NpgsqlParameter("@Id",offeringCategory.Id),
                new Npgsql.NpgsqlParameter("@offeringcategory",offeringCategory.OfferingCategory),
                new Npgsql.NpgsqlParameter("@description",offeringCategory.Description),
                new Npgsql.NpgsqlParameter("@activestatus",offeringCategory.ActiveStatus),
                new Npgsql.NpgsqlParameter("@ispro",offeringCategory.IsPro),
            };
            return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters);
        }

        public bool DeleteOfferingCategory(int nId)
        {
            strQuery = $@"delete from ""tblOfferingCategory"" where id = {nId}";
            return LocalDM.ExecuteNonQueryCommand(strQuery, null, true);
        }

        #endregion
        #region[tblService]

        public void GetServices(string strTableAlias)
        {
            string strQuery = $@"select * from ""tblService"";";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }
        public bool ServiceUpsert(tblService service)
        {
            if (service.Id == 0)
            {
                strQuery = $@"INSERT INTO ""tblService""(servicename, description, activestatus, createdon, lastupdatedOn)
		                    VALUES(@servicename, @description, @activestatus, current_date,current_date);";
            }
            else
            {
                strQuery = $@"UPDATE ""tblService""
		                    SET 
		                    servicename = @servicename, 
		                    description = @description, 
		                    activestatus = @activestatus, 
		                    lastupdatedOn = current_date
		                    WHERE Id = @Id;";
            }

            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                new Npgsql.NpgsqlParameter("@Id",service.Id),
                new Npgsql.NpgsqlParameter("@servicename",service.ServiceName),
                new Npgsql.NpgsqlParameter("@description",service.Description),
                new Npgsql.NpgsqlParameter("@activestatus",service.ActiveStatus),
            };
            return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters);
        }

        public bool DeleteService(int nId)
        {
            strQuery = $@"delete from ""tblService"" where id = {nId}";
            return LocalDM.ExecuteNonQueryCommand(strQuery, null, true);
        }


        #endregion


        #region [tblModule]
        public void ModuleGetAll(string strTableAlias)
        {
            strQuery = $@"SELECT a.*,b.ProjectName  FROM ""tblModule"" a inner join ""tblProject"" b ON a.ProjectId = b.Id WHERE a.ActiveStatus = true";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }
        public void ModuleGetById(int nId, string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblModule"" WHERE ActiveStatus = true AND Id = {nId};";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }
        public void ModuleCheckDuplicacy(tblModule oModel, string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblModule"" WHERE ModuleName = '{oModel.ModuleName}' AND ActiveStatus = true AND Id != {oModel.Id} AND ProjectId = {oModel.ProjectId}";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }
        public bool ModuleUpsert(tblModule oModel)
        {
            if (oModel.Id != 0)
            {
                strQuery = $@"UPDATE ""tblModule"" 
                        SET ProjectId = @ProjectId,
                        ModuleName = @ModuleName, Description = @Description, IsDefault = @IsDefault,
                        LastUpdateDate = current_date
                        WHERE Id = @Id";
            }
            else
            {
                strQuery = $@"INSERT INTO ""tblModule"" (ProjectId, ModuleName, Description, IsDefault, ActiveStatus, CreatedOn, LastUpdateDate) 
                            VALUES (@ProjectId, @ModuleName, @Description, @IsDefault, @ActiveStatus, @CreatedOn, current_date)";
            }

            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                new Npgsql.NpgsqlParameter("@Id",oModel.Id),
                new Npgsql.NpgsqlParameter("@ProjectId", oModel.ProjectId),
                new Npgsql.NpgsqlParameter("@ModuleName", oModel.ModuleName),
                new Npgsql.NpgsqlParameter("@Description", oModel.Description),
                new Npgsql.NpgsqlParameter("@IsDefault", oModel.IsDefault),
                new Npgsql.NpgsqlParameter("@ActiveStatus", oModel.ActiveStatus),
                new Npgsql.NpgsqlParameter("@CreatedOn", oModel.CreatedOn),
            };
            return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters);
        }
        #endregion


        #region [tblFormCategory]
        public void FormCategoryGetAll(string strTableAlias)
        {
            strQuery = $@"SELECT a.*, b.ProjectName  FROM ""tblFormCategory"" a inner join ""tblProject"" b ON a.ProjectId = b.Id WHERE a.ActiveStatus = true";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }
        public void FormCategoryGetById(int nId, string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblFormCategory"" WHERE ActiveStatus = true AND Id = {nId};";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }
        public void FormCategoryCheckDuplicacy(tblFormCategory oModel, string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblFormCategory"" WHERE CategoryName = '{oModel.CategoryName}' AND ActiveStatus = true AND Id != {oModel.Id} AND ProjectId = {oModel.ProjectId}";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }
        public bool FormCategoryUpsert(tblFormCategory oModel)
        {
            if (oModel.Id != 0)
            {
                strQuery = $@"UPDATE ""tblFormCategory"" 
                        SET ProjectId = @ProjectId,
                        CategoryName = @CategoryName, LastUpdateDate = current_date 
                        WHERE Id = @Id";
            }
            else
            {
                strQuery = $@"INSERT INTO ""tblFormCategory"" (ProjectId, CategoryName, ActiveStatus, CreatedOn, LastUpdateDate) 
                            VALUES (@ProjectId, @CategoryName, @ActiveStatus, @CreatedOn, current_date)";
            }

            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                new Npgsql.NpgsqlParameter("@Id",oModel.Id),
                new Npgsql.NpgsqlParameter("@ProjectId", oModel.ProjectId),
                new Npgsql.NpgsqlParameter("@CategoryName", oModel.CategoryName),
                new Npgsql.NpgsqlParameter("@ActiveStatus", oModel.ActiveStatus),
                new Npgsql.NpgsqlParameter("@CreatedOn", oModel.CreatedOn),
            };
            return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters);
        }
        #endregion


        #region [tblForm]
        public void FormGetAll(string strTableAlias)
        {
            strQuery = $@"SELECT a.Id, a.ProjectId, d.ProjectName, 
                        a.CategoryId, e.CategoryName,
                        a.FormName, a.Description, a.ActionName, a.ControllerName, a.PageCode, a.LinkIcon,
                        string_agg(c.ModuleName::text, ',') AS ModuleName,
                        string_agg(c.Id::text, ',') AS ModuleIds
                        FROM ""tblFormMaster"" a
                        INNER JOIN ""tblFormDetail"" b ON a.Id = b.FormId
                        INNER JOIN ""tblModule"" c ON b.ModuleId = c.Id
                        INNER JOIN ""tblProject"" d ON a.ProjectId = d.Id
                        INNER JOIN ""tblFormCategory"" e ON a.CategoryId = e.Id
                        WHERE a.ActiveStatus = true
                        GROUP BY a.Id, d.ProjectName, e.CategoryName, a.FormName, a.Description, a.ActionName, a.ControllerName";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }
        public void FormCheckDuplicacy(tblFormMaster oModel, string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblFormMaster"" WHERE FormName = '{oModel.FormName}' AND ActiveStatus = true AND Id != {oModel.Id} AND ProjectId = {oModel.ProjectId} ";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }
        public bool FormUpdate(tblFormMaster oModel, List<int> liModules)
        {
            bool lretval = false;
            strQuery = $@"DELETE FROM ""tblFormDetail"" WHERE FormId = {oModel.Id} ";
            lretval = LocalDM.ExecuteNonQueryCommand(strQuery, null, true);
            if (lretval)
            {
                strQuery = $@"UPDATE ""tblFormMaster"" SET 
                            ProjectId = @ProjectId, 
                            CategoryId = @CategoryId, 
                            FormName = @FormName, 
                            Description = @Description,
                            ActionName = @ActionName,
                            ControllerName = @ControllerName,
                            PageCode = @PageCode,
                            LinkIcon = @LinkIcon
                            WHERE Id = @Id";
                Npgsql.NpgsqlParameter[] npgsqlParameters = {
                    new Npgsql.NpgsqlParameter("@Id",oModel.Id),
                    new Npgsql.NpgsqlParameter("@CategoryId",oModel.CategoryId),
                    new Npgsql.NpgsqlParameter("@FormName",oModel.FormName),
                    new Npgsql.NpgsqlParameter("@Description",oModel.Description),
                    new Npgsql.NpgsqlParameter("@ActionName",oModel.ActionName),
                    new Npgsql.NpgsqlParameter("@ControllerName",oModel.ControllerName),
                    new Npgsql.NpgsqlParameter("@PageCode",oModel.PageCode),
                    new Npgsql.NpgsqlParameter("@LinkIcon",oModel.LinkIcon),
                };
                lretval = LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters, true);
                if (lretval)
                {
                    int nFormId = oModel.Id;
                    strQuery = $@"INSERT INTO ""tblFormDetail"" (FormId, ModuleId) VALUES ";
                    for (int i = 0; i < liModules.Count; i++)
                    {
                        strQuery += (i == liModules.Count - 1) ? $@" ({nFormId},{liModules[i]})" : $@" ({nFormId},{liModules[i]}),";
                    }

                    lretval = LocalDM.ExecuteNonQueryCommand(strQuery, null, true);
                }
            }

            return lretval;
        }
        public bool FormInsert(tblFormMaster oModel, List<int> liModules)
        {
            bool lretval = false;

            strQuery = $@"INSERT INTO ""tblFormMaster"" (ProjectId, CategoryId, FormName, Description, ActionName, ControllerName, PageCode, LinkIcon,  ActiveStatus, CreatedOn) 
                            VALUES (@ProjectId, @CategoryId, @FormName, @Description, @ActionName, @ControllerName, @PageCode, @LinkIcon, @ActiveStatus, @CreatedOn)";
            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                new Npgsql.NpgsqlParameter("@ProjectId",oModel.ProjectId),
                new Npgsql.NpgsqlParameter("@CategoryId",oModel.CategoryId),
                new Npgsql.NpgsqlParameter("@FormName",oModel.FormName),
                new Npgsql.NpgsqlParameter("@Description",oModel.Description),
                new Npgsql.NpgsqlParameter("@ActionName",oModel.ActionName),
                new Npgsql.NpgsqlParameter("@ControllerName",oModel.ControllerName),
                new Npgsql.NpgsqlParameter("@ActiveStatus",oModel.ActiveStatus),
                new Npgsql.NpgsqlParameter("@CreatedOn",oModel.CreatedOn),
                new Npgsql.NpgsqlParameter("@PageCode",oModel.PageCode),
                new Npgsql.NpgsqlParameter("@LinkIcon",oModel.LinkIcon),
            };
            lretval = LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters, true);
            if (lretval)
            {
                int nFormId = 0;
                strQuery = $@"SELECT Id FROM ""tblFormMaster"" WHERE ActiveStatus = true ORDER BY Id DESC limit 1;";
                LocalDM.ExecuteSelectCommand(strQuery, null, "tLastId", true);
                if (this._localDS.Tables["tLastId"] != null && this._localDS.Tables["tLastId"].Rows.Count > 0)
                {
                    nFormId = Convert.ToInt32(this._localDS.Tables["tLastId"].Rows[0]["Id"].ToString().Trim());
                }

                strQuery = $@"INSERT INTO ""tblFormDetail"" (FormId, ModuleId) VALUES ";
                for (int i = 0; i < liModules.Count; i++)
                {
                    strQuery += (i == liModules.Count - 1) ? $@" ({nFormId},{liModules[i]})" : $@" ({nFormId},{liModules[i]}),";
                }

                lretval = LocalDM.ExecuteNonQueryCommand(strQuery, null, true);
            }

            return lretval;
        }
        public bool FormRemove(int nId)
        {
            strQuery = $@"UPDATE ""tblFormMaster"" SET ActiveStatus = @ActiveStatus WHERE Id = @Id";
            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                new Npgsql.NpgsqlParameter("@Id",nId),
                new Npgsql.NpgsqlParameter("@ActiveStatus",false),
            };
            return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters, true);
        }

        public void FormCategoryByProject(int nProjectId, string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblFormCategory"" WHERE ProjectId = {nProjectId} AND ActiveStatus = true";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias);
        }
        public void FormModuleByProject(int nProjectId, string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblModule"" WHERE ProjectId = {nProjectId} AND ActiveStatus = true";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias);
        }

        #endregion


        #region [tblUsers]
        public bool ClientUserCreate(string strEmailId, string strPassword)
        {
            strQuery = $@"CREATE USER ""{strEmailId}"" WITH PASSWORD '{strPassword}';";
            return LocalDM.ExecuteNonQueryCommand(strQuery, null, true);
        }
        public bool ClientUserSchemaCreate(string strSchemaName, string strEmailId)
        {
            bool lretval = false;
            strQuery = $@"CREATE SCHEMA IF NOT EXISTS ""{strSchemaName}"" AUTHORIZATION ""{strEmailId}"";";
            lretval = LocalDM.ExecuteNonQueryCommand(strQuery, null, true);
            if (lretval)
            {
                strQuery = $@"GRANT USAGE ON SCHEMA ""{strSchemaName}"" TO ""{strEmailId}"";
                        ALTER USER ""{strEmailId}"" SET search_path = ""{strSchemaName}""";
                lretval = LocalDM.ExecuteNonQueryCommand(strQuery, null, true);
            }

            return lretval;
        }
        public bool ClientUserSchemaStructure(string strQuery)
        {
            return LocalDM.ExecuteNonQueryCommand(strQuery, null, true);
        }
        public bool ClientUserInsert(string strEmailId, string strPassword, string strCompanyName,
            string strSchemaName, DateTime dtExpiredOn, DateTime dtCreatedOn, bool lActive,
            string strLogo, string strSocketId, string strRole, int nCreatedBy, string empCode, string contact1, string contact2, string address)
        {
            strQuery = $@"INSERT INTO ""tblUsers""(EmailId, Password, CompanyName, SchemaName, ExpiredOn, CreatedOn, ActiveStatus, logo, IsSuperAdmin, SocketId, Role, CreatedBy, LastUpdateDate,empcode,contactno1,contactno2,address)
                    VALUES(@EmailId,@Password,@CompanyName,@SchemaName,@ExpiredOn,@CreatedOn,@ActiveStatus, @logo, @IsSuperAdmin, @SocketId, @Role, @CreatedBy, current_date,@empcode,@contact1,@contact2,@address)";
            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                new Npgsql.NpgsqlParameter("@EmailId",strEmailId.Trim()),
                new Npgsql.NpgsqlParameter("@Password",strPassword),
                new Npgsql.NpgsqlParameter("@CompanyName",strCompanyName),
                new Npgsql.NpgsqlParameter("@SchemaName",strSchemaName),
                new Npgsql.NpgsqlParameter("@ExpiredOn",dtExpiredOn),
                new Npgsql.NpgsqlParameter("@CreatedOn",dtCreatedOn),
                new Npgsql.NpgsqlParameter("@ActiveStatus",lActive),
                new Npgsql.NpgsqlParameter("@logo",strLogo),
                new Npgsql.NpgsqlParameter("@IsSuperAdmin",false),
                new Npgsql.NpgsqlParameter("@SocketId",strSocketId),
                new Npgsql.NpgsqlParameter("@Role",strRole),
                new Npgsql.NpgsqlParameter("@CreatedBy",nCreatedBy),
                new Npgsql.NpgsqlParameter("@empcode",(object)empCode?? DBNull.Value),
                new Npgsql.NpgsqlParameter("@contact1",(object)contact1 ?? DBNull.Value),
                new Npgsql.NpgsqlParameter("@contact2",(object)contact2 ?? DBNull.Value),
                new Npgsql.NpgsqlParameter("@address",(object)address ?? DBNull.Value)
                
            };

            return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters, true);
        }
        public bool ClientUserRename(string strOldEmailId, string strEmailId, string strPassword)
        {
            strQuery = $@"ALTER USER ""{strOldEmailId}"" RENAME TO ""{strEmailId}"";
                        ALTER USER ""{strEmailId}"" WITH PASSWORD '{strPassword}';";
            return LocalDM.ExecuteNonQueryCommand(strQuery, null, true);
        }
        public bool ClientUserSchemaRename(string strOldName, string strSchemaName, string strEmailId)
        {
            strQuery = $@"ALTER SCHEMA ""{strOldName}"" RENAME TO ""{strSchemaName}"";
                          ALTER SCHEMA ""{strSchemaName}"" OWNER TO ""{strEmailId}"";";
            return LocalDM.ExecuteNonQueryCommand(strQuery, null, true);
        }

        public void GetSupportManagerById(int id, string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblUsers"" Where role = 'SUPPORTMANAGER' AND id <> {id}";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void GetSupportManager(string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblUsers"" Where role = 'SUPPORTMANAGER'";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public bool ClientUserUpdate(int nId, DateTime dtExpiredOn, bool lActive, string strLogo, string role, 
            string empCode, string contact1, string contact2, string address)
        {
            strQuery = $@"UPDATE ""tblUsers""
                        SET
                        ExpiredOn = @ExpiredOn, 
                        ActiveStatus = @ActiveStatus, 
                        logo = @logo,
                        role = @role,
                        empcode = @empCode,
                        contactno1 = @contact1,
                        contactno2 = @contact2,
                        address = @address
                        WHERE Id = @Id";
            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                new Npgsql.NpgsqlParameter("@Id",nId),
                new Npgsql.NpgsqlParameter("@ExpiredOn",dtExpiredOn),
                new Npgsql.NpgsqlParameter("@ActiveStatus",lActive),
                new Npgsql.NpgsqlParameter("@logo",strLogo),
                new Npgsql.NpgsqlParameter("@empCode",(object)empCode ?? DBNull.Value),
                new Npgsql.NpgsqlParameter("@contact1",(object) contact1  ?? DBNull.Value),
                new Npgsql.NpgsqlParameter("@contact2",(object)contact2?? DBNull.Value),
                new Npgsql.NpgsqlParameter("@address",(object)address?? DBNull.Value),
                new Npgsql.NpgsqlParameter("@role", (object) role ?? DBNull.Value),
            };

            return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters, true);
        }
        public bool ClientSubUserUpdate(int nId, string strEmailId, string strPassword)
        {
            strQuery = $@"UPDATE ""tblUsers"" SET
                        EmailId = @EmailId, 
                        Password = @Password
                        WHERE Id = @Id";
            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                new Npgsql.NpgsqlParameter("@Id",nId),
                new Npgsql.NpgsqlParameter("@EmailId",strEmailId),
                new Npgsql.NpgsqlParameter("@Password",strPassword),
            };

            return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters, true);
        }
        public void ClientUserGetAll(int nCreatedBy, string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblUsers"" Where IsSuperAdmin = false AND CreatedBy = {nCreatedBy} AND Role='ADMIN'";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }
        public void EmployeeUserGetAll(int nCreatedBy, string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblUsers"" Where IsSuperAdmin = false AND CreatedBy = {nCreatedBy} AND Role IN('SUPPORTMANAGER','SUPPORTREPRESENTATIVE')";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }
        public void SubUserGetAll(int nCreatedBy, string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblUsers"" Where IsSuperAdmin = false AND CreatedBy = {nCreatedBy} AND Role='SUBADMIN' OR Id = {nCreatedBy}";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }
        public void ClientUserGetAll(string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblUsers"" Where role = 'ADMIN' and activestatus = true";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void GetSubUsers(int nCreatedBy, string strTableAlias)
        {
            strQuery = $@"select b.id,b.emailid,b.password,b.companyname,b.schemaname,b.expiredon,b.role,b.issuperadmin,
                                b.socketid,b.createdby,b.createdon,b.activestatus,
                                b.lastupdatedate,b.empcode,b.contactno1,b.contactno2,
                                b.address,
                                CASE WHEN (b.logo is null  OR b.logo = '') then a.logo 
                                else b.logo end as logo
                            from ""tblUsers"" a
                        left join ""tblUsers"" b
                            on a.id = b.createdby
                        where a.id = {nCreatedBy}
                        Union
                        select b.id,b.emailid,b.password,b.companyname,b.schemaname,
                            b.expiredon,b.role,b.issuperadmin,
                            b.socketid,b.createdby,b.createdon,b.activestatus,
                            b.lastupdatedate,b.empcode,b.contactno1,b.contactno2,
                            b.address,b.logo 
                        from ""tblUsers"" b where id = {nCreatedBy}";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void GetSubUsersWithEmail(int nCreatedBy, string strTableAlias)
        {
            strQuery = $@"select b.id,CONCAT (b.companyname, ' ( ',b.emailid,' ) ') as companyName 
                            from ""tblUsers"" a
                        left join ""tblUsers"" b
                            on a.id = b.createdby
                        where a.id = {nCreatedBy}
                        Union
                        select  b.id,CONCAT (b.companyname, ' ( ',b.emailid,' ) ') as companyName  
                        from ""tblUsers"" b where id = {nCreatedBy}";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void ClientUserGetById(int nId, string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblUsers"" WHERE Id = {nId}";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }
        public bool ClientUserCanRemove(string strSchemaName)
        {
            strQuery = $@"SELECT COUNT(*) FROM ""{strSchemaName}"".""tblCompanyName"";";
            LocalDM.ExecuteSelectCommand(strQuery, null, "tComCount", true);
            if (this._localDS.Tables["tComCount"] != null && this._localDS.Tables["tComCount"].Rows.Count > 0)
            {
                var nCount = Convert.ToInt32(this._localDS.Tables["tComCount"].Rows[0][0].ToString());
                return nCount > 0 ? false : true;
            }
            else
            {
                return false;
            }
        }

        public bool ClientUserSchemaRemove(string strSchemaName = "", string strEmailId = "")
        {
            bool lretval = true;
            if (!string.IsNullOrEmpty(strSchemaName))
            {
                strQuery = $@"DROP SCHEMA ""{strSchemaName}"" CASCADE;";
                lretval = LocalDM.ExecuteNonQueryCommand(strQuery, null, true);
            }

            if (lretval && !string.IsNullOrEmpty(strEmailId))
            {
                strQuery = $@"DROP USER ""{strEmailId}"";";
                lretval = LocalDM.ExecuteNonQueryCommand(strQuery, null, true);
            }
            return lretval;
        }
        public bool ClientUserRemove(int nClientId)
        {
            bool lretval = true;
            if (lretval)
            {
                strQuery = $@"DELETE FROM ""tblUsers"" WHERE Id = {nClientId};";
                lretval = LocalDM.ExecuteNonQueryCommand(strQuery, null, true);
            }

            if (lretval)
            {
                strQuery = $@"DELETE FROM ""tblUsers"" WHERE CreatedBy = {nClientId};";
                lretval = LocalDM.ExecuteNonQueryCommand(strQuery, null, true);
            }

            if (lretval)
            {
                strQuery = $@"DELETE FROM ""tblUserModule"" WHERE UserId = {nClientId}; ";
                lretval = LocalDM.ExecuteNonQueryCommand(strQuery, null, true);
            }

            return lretval;
        }
        public Tuple<bool, string> ClientUserCheckDuplicacy(int nId, string strEmailId,
            string strSchemaName = "", string strCompanyName = "", string empcode = "", string contactno1 = "", string contactno2 = "")
        {
            if (!string.IsNullOrEmpty(strCompanyName))
            {
                strQuery = $@"select * from ""tblUsers"" where companyname = '{strCompanyName}' and id != {nId} and activestatus = true";
                LocalDM.ExecuteSelectCommand(strQuery, null, "tDuplicateCompany", true);
                if (this._localDS.Tables["tDuplicateCompany"] != null
                    && this._localDS.Tables["tDuplicateCompany"].Rows.Count > 0)
                {
                    return new Tuple<bool, string>(false, "Company Already Exists..");
                }
            }

            strQuery = $@"select * from ""tblUsers"" where emailid = '{strEmailId}' and id != {nId} and activestatus = true";
            LocalDM.ExecuteSelectCommand(strQuery, null, "tDuplicateEmail", true);
            if (this._localDS.Tables["tDuplicateEmail"] != null
                && this._localDS.Tables["tDuplicateEmail"].Rows.Count > 0)
            {
                return new Tuple<bool, string>(false, "Email Already Exists..");
            }

            if (!string.IsNullOrEmpty(strSchemaName))
            {
                strQuery = $@"select * from ""tblUsers"" where schemaname = '{strSchemaName}' and id != {nId} and activestatus = true";
                LocalDM.ExecuteSelectCommand(strQuery, null, "tDuplicateCompany", true);
                if (this._localDS.Tables["tDuplicateCompany"] != null
                    && this._localDS.Tables["tDuplicateCompany"].Rows.Count > 0)
                {
                    return new Tuple<bool, string>(false, "Schema Already Exists..");
                }
            }

            if (!string.IsNullOrEmpty(empcode))
            {
                strQuery = $@"select * from ""tblUsers"" where empcode = '{empcode}' and id != {nId} and activestatus = true";
                LocalDM.ExecuteSelectCommand(strQuery, null, "tDuplicateEmpcode", true);
                if (this._localDS.Tables["tDuplicateEmpcode"] != null
                    && this._localDS.Tables["tDuplicateEmpcode"].Rows.Count > 0)
                {
                    return new Tuple<bool, string>(false, "Code Already Exists..");
                }
            }

            if (!string.IsNullOrEmpty(contactno1) || !string.IsNullOrEmpty(contactno2))
            {
                strQuery = $@"select * from ""tblUsers"" where (contactno1 = '{contactno1}' OR contactno2='{contactno2}' )and id != {nId} and activestatus = true";
                LocalDM.ExecuteSelectCommand(strQuery, null, "tDuplicatecontact", true);
                if (this._localDS.Tables["tDuplicatecontact"] != null
                    && this._localDS.Tables["tDuplicatecontact"].Rows.Count > 0)
                {
                    return new Tuple<bool, string>(false, "Contact Number Already Exists..");
                }
            }
            return new Tuple<bool, string>(true, string.Empty);
        }
        #endregion


        #region [ClientProject]
        public void UserProjects(string strTableAlias, string strRole)
        {
            strQuery = $@"select a.* , c.ProjectName
                    from ""tblUsers"" a
                    LEFT JOIN ""tblUserModule"" b ON a.Id = b.UserId
                    LEFT JOIN ""tblProject"" c ON c.Id = b.ProjectId
                    Where Role = '{strRole}'";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias);
        }
        #endregion


        #region [tblPlan]
        public void PlanGetAll(string strTableAlias)
        {
            strQuery = $@"SELECT a.*,b.ProjectName  FROM ""tblPlan"" a inner join ""tblProject"" b ON a.ProjectId = b.Id WHERE a.ActiveStatus = true";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }
        public void PlanGetAllByProject(int projectId, string strTableAlias)
        {
            string strQuery = $@"SELECT a.*,b.ProjectName  FROM ""tblPlan"" a inner join ""tblProject"" b ON a.ProjectId = b.Id WHERE a.ActiveStatus = true and b.id = {projectId}";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void PlanGetById(string nId, string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblPlan"" WHERE ActiveStatus = true AND Id IN ({nId});";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }
        public void PlanCheckDuplicacy(tblPlan oModel, string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblPlan"" WHERE PlanName = '{oModel.PlanName}' AND ActiveStatus = true AND Id != {oModel.Id} AND ProjectId = {oModel.ProjectId}";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }
        public bool PlanUpsert(tblPlan oModel)
        {
            if (oModel.Id != 0)
            {
                strQuery = $@"UPDATE ""tblPlan"" 
                        SET ProjectId = @ProjectId,
                        PlanName = @PlanName, Description = @Description, NoOfUsers = @NoOfUsers,
                        ValidityDays = @ValidityDays, IsFree = @IsFree, PlanPrice = @PlanPrice,
                        ActiveStatus = @ActiveStatus, LastUpdateDate = current_date,
                        iswarranty = @iswarranty,warrantyindays = @warrantyindays
                        WHERE Id = @Id";
            }
            else
            {
                strQuery = $@"INSERT INTO ""tblPlan"" (ProjectId, PlanName, Description, NoOfUsers, ValidityDays, IsFree, PlanPrice, ActiveStatus, CreatedOn, LastUpdateDate,iswarranty, warrantyindays) 
                            VALUES (@ProjectId, @PlanName, @Description, @NoOfUsers, @ValidityDays, @IsFree, @PlanPrice, @ActiveStatus, @CreatedOn, current_date,@iswarranty,@warrantyindays)";
            }

            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                new Npgsql.NpgsqlParameter("@Id",oModel.Id),
                new Npgsql.NpgsqlParameter("@ProjectId", oModel.ProjectId),
                new Npgsql.NpgsqlParameter("@PlanName", oModel.PlanName),
                new Npgsql.NpgsqlParameter("@Description", oModel.Description),
                new Npgsql.NpgsqlParameter("@NoOfUsers", oModel.NoOfUsers),
                new Npgsql.NpgsqlParameter("@IsFree", oModel.IsFree),
                new Npgsql.NpgsqlParameter("@ValidityDays", oModel.ValidityDays),
                new Npgsql.NpgsqlParameter("@PlanPrice", oModel.PlanPrice),
                new Npgsql.NpgsqlParameter("@ActiveStatus", oModel.ActiveStatus),
                new Npgsql.NpgsqlParameter("@CreatedOn", oModel.CreatedOn),
                new Npgsql.NpgsqlParameter("@iswarranty",oModel.IsWarranty),
                new Npgsql.NpgsqlParameter("@warrantyindays", (object)oModel.WarrantyInDays?? DBNull.Value)
            };
            return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters);
        }
        #endregion


        #region [tblPlanPurchaseRequest]
        public bool PlanPurchaseRequestInsert(tblPlanRequest planRequest, List<tblPlanRequestDetail> requestDetails)
        {
            bool lretval = false;
            int nRecordId = 0;
            strQuery = $@"INSERT INTO ""tblPlanRequest""(ClientId,PurchaseDate,PaymentMethod,PaymentMode,PaymentTransactionNo,IsApproved,Remarks,CreatedOn,ActiveStatus,LastUpdateDate)
                        VALUES(@ClientId,@PurchaseDate,@PaymentMethod,@PaymentMode,@PaymentTransactionNo,NULL,@Remarks,@CreatedOn,@ActiveStatus,current_date) RETURNING id;";
            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                new Npgsql.NpgsqlParameter("@ClientId", planRequest.ClientId),
                new Npgsql.NpgsqlParameter("@PurchaseDate", planRequest.PurchaseDate),
                new Npgsql.NpgsqlParameter("@PaymentMethod", planRequest.PaymentMethod),
                new Npgsql.NpgsqlParameter("@PaymentMode", planRequest.PaymentMode),
                new Npgsql.NpgsqlParameter("@PaymentTransactionNo", planRequest.PaymentTransactionNo),
                new Npgsql.NpgsqlParameter("@Remarks", planRequest.Remarks),
                new Npgsql.NpgsqlParameter("@CreatedOn", planRequest.CreatedOn),
                new Npgsql.NpgsqlParameter("@ActiveStatus", planRequest.ActiveStatus),
            };
            LocalDM.ExecuteSelectCommand(strQuery, npgsqlParameters, "tMaster");
            if (this._localDS.Tables["tMaster"] != null && this._localDS.Tables["tMaster"].Rows.Count > 0)
            {
                lretval = true;
                nRecordId = Convert.ToInt32(this._localDS.Tables["tMaster"].Rows[0][0].ToString().Trim());
                foreach (var item in requestDetails)
                {
                    var objPlanRequests = new tblPlanRequestDetail
                    {
                        RequestId = nRecordId,
                        PlanId = item.PlanId,
                        NoOfUsers = item.NoOfUsers
                    };
                    strQuery = $@"INSERT INTO ""tblPlanRequestDetail""(RequestId,PlanId,NoOfUsers)
                        VALUES(@RequestId, @PlanId, @NoOfUsers)";
                    Npgsql.NpgsqlParameter[] npgsqlParameters1 = {
                        new Npgsql.NpgsqlParameter("@RequestId", objPlanRequests.RequestId),
                        new Npgsql.NpgsqlParameter("@PlanId", objPlanRequests.PlanId),
                        new Npgsql.NpgsqlParameter("@NoOfUsers", objPlanRequests.NoOfUsers),
                    };
                    lretval = LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters1);
                }
            }
            else
            {
                lretval = false;
            }
            return lretval;
        }

        public bool PlanPurchaseRequestUpdate(tblPlanRequest planRequest)
        {
            strQuery = $@"UPDATE ""tblPlanRequest""
                        SET 
                        IsApproved = @IsApproved,
                        Remarks = @Remarks,
                        LastUpdateDate = current_date
                        WHERE Id = @Id";
            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                new Npgsql.NpgsqlParameter("@Id", planRequest.Id),
                new Npgsql.NpgsqlParameter("@IsApproved", planRequest.IsApproved),
                new Npgsql.NpgsqlParameter("@Remarks", planRequest.Remarks),
            };
            return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters);
        }

        public void PlanPurchaseRequestGetAll(string strTableAlias, int nClientId = 0)
        {
            //strQuery = $@"select c.CompanyName, c.EmailId, 
            //                a.*, 
            //                b.PlanId, b.NoOfUsers, 
            //                d.PlanName, d.Description, d.ValidityDays, d.PlanPrice, 
            //                e.ProjectName, e.ProjectType, e.ProjectImage
            //                from ""tblPlanRequest"" a
            //                inner join ""tblUsers"" c ON a.ClientId = c.Id
            //                inner join ""tblPlanRequestDetail"" b ON a.Id = b.RequestId
            //                inner join ""tblPlan"" d ON b.PlanId = d.Id
            //                inner join ""tblProject"" e ON d.ProjectId = e.Id";

            strQuery = $@"SELECT c.CompanyName, c.EmailId, c.Logo, a.* 
                        FROM ""tblPlanRequest"" a 
                        INNER JOIN ""tblUsers"" c ON a.ClientId = c.Id";
            if(nClientId != 0)
            {
                strQuery += $@" WHERE a.ClientId = {nClientId}";
            }
            else
            {
                strQuery += $@" WHERE IsApproved is NULL";
            }
            LocalDM.ExecuteSelectCommand(strQuery, strTableAlias);
        }

        public void PlanPurchaseRequestGetById(int nId, string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblPlanRequest"" WHERE Id = {nId}";
            LocalDM.ExecuteSelectCommand(strQuery, strTableAlias);
        }

        public void GetNoOfRequestAgainstRequestId(int nId, string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblPlanRequestDetail"" WHERE RequestId = {nId}";
            LocalDM.ExecuteSelectCommand(strQuery, strTableAlias);
        }

        public void PlanPurchaseRequestDetailByRequestId(int requestId, string strTableAlias)
        {
            strQuery = $@"select b.RequestId, b.PlanId, b.NoOfUsers, 
                        d.PlanName, d.Description, d.ValidityDays, d.PlanPrice,d.iswarranty,d.warrantyindays ,
                        e.ProjectName, e.ProjectType, e.ProjectImage
                        from ""tblPlanRequestDetail"" b
                        inner join ""tblPlan"" d ON b.PlanId = d.Id
                        inner join ""tblProject"" e ON d.ProjectId = e.Id
                        where b.requestid = {requestId}";
            LocalDM.ExecuteSelectCommand(strQuery, strTableAlias);
        }

        public void PlanPurchaseRequestDetailByRequestDetailId(int nId, string strTableAlias)
        {
            strQuery = $@"select b.RequestId, b.PlanId, b.NoOfUsers, 
                        d.PlanName, d.Description, d.ValidityDays, d.PlanPrice,d.iswarranty,d.warrantyindays ,
                        e.ProjectName, e.ProjectType, e.ProjectImage
                        from ""tblPlanRequestDetail"" b
                        inner join ""tblPlan"" d ON b.PlanId = d.Id
                        inner join ""tblProject"" e ON d.ProjectId = e.Id
                        where b.id = {nId}";
            LocalDM.ExecuteSelectCommand(strQuery, strTableAlias);
        }

        #endregion


        #region [tblPlanPurchase]
        public bool PlanPurchaseInsert(tblPlanPurchase planPurchase, List<tblPlanPurchaseDetail> purchaseDetails)
        {
            bool lretval = false;
            int nRecordId = 0;
            strQuery = $@"INSERT INTO ""tblPlanPurchase""(SubscriptionId,ClientId,PurchaseDate,PaymentMethod,PaymentMode,PaymentTransactionNo,CreatedOn,ActiveStatus,LastUpdateDate)
                        VALUES(@SubscriptionId,@ClientId,@PurchaseDate,@PaymentMethod,@PaymentMode,@PaymentTransactionNo,@CreatedOn,@ActiveStatus,current_date) RETURNING id;";
            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                new Npgsql.NpgsqlParameter("@SubscriptionId", planPurchase.SubscriptionId),
                new Npgsql.NpgsqlParameter("@ClientId", planPurchase.ClientId),
                new Npgsql.NpgsqlParameter("@PurchaseDate", planPurchase.PurchaseDate),
                new Npgsql.NpgsqlParameter("@PaymentMethod", planPurchase.PaymentMethod),
                new Npgsql.NpgsqlParameter("@PaymentMode", planPurchase.PaymentMode),
                new Npgsql.NpgsqlParameter("@PaymentTransactionNo", planPurchase.PaymentTransactionNo),
                new Npgsql.NpgsqlParameter("@CreatedOn", planPurchase.CreatedOn),
                new Npgsql.NpgsqlParameter("@ActiveStatus", planPurchase.ActiveStatus),
            };
            LocalDM.ExecuteSelectCommand(strQuery, npgsqlParameters, "tMaster");
            if (this._localDS.Tables["tMaster"] != null && this._localDS.Tables["tMaster"].Rows.Count > 0)
            {
                lretval = true;
                nRecordId = Convert.ToInt32(this._localDS.Tables["tMaster"].Rows[0][0].ToString().Trim());
                foreach (var item in purchaseDetails)
                {
                    var objPlanRequests = new tblPlanPurchaseDetail
                    {
                        SubscriptionId = planPurchase.SubscriptionId,
                        PlanId = item.PlanId,
                        LicenseKey = item.LicenseKey,
                        CreatedOn = item.CreatedOn,
                        ActiveStatus = true,
                        IsWarranty = item.IsWarranty,
                        WarrantyStartDate = item.WarrantyStartDate,
                        WarrantyEndDate = item.WarrantyEndDate
                    };
                    strQuery = $@"INSERT INTO ""tblPlanPurchaseDetail""(SubscriptionId,PlanId,LicenseKey,ActivationDate,ExpiryDate,UserId,IsRenew,IsExpired,CreatedOn,ActiveStatus,LastUpdateDate, DeviceId,iswarranty,warrantystartdate,warrantyenddate,istransfered)
                        VALUES(@SubscriptionId, @PlanId, @LicenseKey, NULL,NULL,NULL,NULL,NULL,@CreatedOn,@ActiveStatus,Current_date, NULL,@iswarranty,@warrantystartdate,@warrantyenddate,false)";
                    Npgsql.NpgsqlParameter[] npgsqlParameters1 = {
                        new Npgsql.NpgsqlParameter("@SubscriptionId", objPlanRequests.SubscriptionId),
                        new Npgsql.NpgsqlParameter("@PlanId", objPlanRequests.PlanId),
                        new Npgsql.NpgsqlParameter("@LicenseKey", objPlanRequests.LicenseKey),
                        new Npgsql.NpgsqlParameter("@CreatedOn", objPlanRequests.CreatedOn),
                        new Npgsql.NpgsqlParameter("@ActiveStatus", objPlanRequests.ActiveStatus),
                        new Npgsql.NpgsqlParameter("@iswarranty",objPlanRequests.IsWarranty),
                        new Npgsql.NpgsqlParameter("@warrantystartdate", objPlanRequests.WarrantyStartDate ),
                        new Npgsql.NpgsqlParameter("@warrantyenddate",objPlanRequests.WarrantyEndDate)
                    };
                    lretval = LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters1);
                }
            }
            else
            {
                lretval = false;
            }
            return lretval;
        }

        public bool ManualPlanPurchaseInsert(tblPlanPurchase planPurchase, List<tblPlanPurchaseDetail> purchaseDetails)
        {
            bool lretval = false;
            int nRecordId = 0;
            int nDetailId = 0;
            strQuery = $@"INSERT INTO ""tblPlanPurchase""(SubscriptionId,ClientId,PurchaseDate,PaymentMethod,PaymentMode,PaymentTransactionNo,CreatedOn,ActiveStatus,LastUpdateDate)
                        VALUES(@SubscriptionId,@ClientId,@PurchaseDate,@PaymentMethod,@PaymentMode,@PaymentTransactionNo,@CreatedOn,@ActiveStatus,current_date) RETURNING id;";
            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                new Npgsql.NpgsqlParameter("@SubscriptionId", planPurchase.SubscriptionId),
                new Npgsql.NpgsqlParameter("@ClientId", planPurchase.ClientId),
                new Npgsql.NpgsqlParameter("@PurchaseDate", planPurchase.PurchaseDate),
                new Npgsql.NpgsqlParameter("@PaymentMethod", planPurchase.PaymentMethod),
                new Npgsql.NpgsqlParameter("@PaymentMode", planPurchase.PaymentMode),
                new Npgsql.NpgsqlParameter("@PaymentTransactionNo", planPurchase.PaymentTransactionNo),
                new Npgsql.NpgsqlParameter("@CreatedOn", planPurchase.CreatedOn),
                new Npgsql.NpgsqlParameter("@ActiveStatus", planPurchase.ActiveStatus),
            };
            LocalDM.ExecuteSelectCommand(strQuery, npgsqlParameters, "tMaster");
            if (this._localDS.Tables["tMaster"] != null && this._localDS.Tables["tMaster"].Rows.Count > 0)
            {
                lretval = true;
                nRecordId = Convert.ToInt32(this._localDS.Tables["tMaster"].Rows[0][0].ToString().Trim());
                foreach (var item in purchaseDetails)
                {
                    var objPlanRequests = new tblPlanPurchaseDetail
                    {
                        SubscriptionId = planPurchase.SubscriptionId,
                        PlanId = item.PlanId,
                        LicenseKey = item.LicenseKey,
                        CreatedOn = item.CreatedOn,
                        ActiveStatus = true,
                        IsWarranty = false,
                        WarrantyStartDate = item.WarrantyStartDate,
                        WarrantyEndDate = item.WarrantyEndDate,
                        ActivationDate = DateTime.Now,
                        ExpiryDate = item.ExpiryDate,
                        UserId = item.UserId,
                        IsExpired = item.IsExpired
                    };

                    strQuery = $@"INSERT INTO ""tblPlanPurchaseDetail""(SubscriptionId,PlanId,LicenseKey,ActivationDate,ExpiryDate,UserId,IsRenew,IsExpired,CreatedOn,ActiveStatus,LastUpdateDate, DeviceId,iswarranty,warrantystartdate,warrantyenddate,istransfered)
                        VALUES(@SubscriptionId, @PlanId, @LicenseKey, @activationDate,@ExpiryDate,@UserId,NULL,@IsExpired,@CreatedOn,@ActiveStatus,Current_date, NULL,@iswarranty,@warrantystartdate,@warrantyenddate,false)RETURNING id;";
                    Npgsql.NpgsqlParameter[] npgsqlParameters1 = {
                        new Npgsql.NpgsqlParameter("@SubscriptionId", objPlanRequests.SubscriptionId),
                        new Npgsql.NpgsqlParameter("@PlanId", objPlanRequests.PlanId),
                        new Npgsql.NpgsqlParameter("@LicenseKey", objPlanRequests.LicenseKey),
                        new Npgsql.NpgsqlParameter("@CreatedOn", objPlanRequests.CreatedOn),
                        new Npgsql.NpgsqlParameter("@ActiveStatus", objPlanRequests.ActiveStatus),
                        new Npgsql.NpgsqlParameter("@iswarranty",objPlanRequests.IsWarranty),
                        new Npgsql.NpgsqlParameter("@warrantystartdate", objPlanRequests.WarrantyStartDate ),
                        new Npgsql.NpgsqlParameter("@warrantyenddate",objPlanRequests.WarrantyEndDate),
                        new Npgsql.NpgsqlParameter("@activationDate",objPlanRequests.ActivationDate),
                        new Npgsql.NpgsqlParameter("@ExpiryDate",objPlanRequests.ExpiryDate),
                        new Npgsql.NpgsqlParameter("@UserId",objPlanRequests.UserId),
                        new Npgsql.NpgsqlParameter("@IsExpired",objPlanRequests.IsExpired)
                    };
                    LocalDM.ExecuteSelectCommand(strQuery, npgsqlParameters1, "tMasterDetail");
                    if (this._localDS.Tables["tMaster"] != null && this._localDS.Tables["tMasterDetail"].Rows.Count > 0)
                    {
                        //code to maintain history
                        nDetailId = Convert.ToInt32(this._localDS.Tables["tMasterDetail"].Rows[0][0].ToString().Trim());

                        strQuery = $@"INSERT INTO ""tblPlanPurchaseHistory""(PlanPurchaseDetailId, ActivationDate, ExpiryDate, UserId)
                            VALUES (@PlanPurchaseDetailId, @ActivationDate, @ExpiryDate, @UserId)";

                        Npgsql.NpgsqlParameter[] npgsqlParameters2 = {
                        new Npgsql.NpgsqlParameter("@PlanPurchaseDetailId", nDetailId),
                        new Npgsql.NpgsqlParameter("@ActivationDate", DateTime.Now),
                        new Npgsql.NpgsqlParameter("@ExpiryDate", purchaseDetails[0].ExpiryDate),
                        new Npgsql.NpgsqlParameter("@UserId", purchaseDetails[0].UserId),
                        };
                        lretval = LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters2);
                    }
                }
            }
            else
            {
                lretval = false;
            }
            return lretval;
        }


        public void PlanPurchaseGetAll(string strTableAlias, int nClientId = 0)
        {
            strQuery = $@"SELECT 
                            c.CompanyName, c.EmailId, c.Logo, 
                            a.SubscriptionId, a.ClientId, a.PurchaseDate,
                            f.Id AS ProjectId, f.ProjectName,b.istransfered,
                            (SELECT COUNT(*) FROM ""tblPlanPurchaseDetail"" WHERE SubscriptionId = a.SubscriptionId) AS TotalKeys,
                            (SELECT COUNT(*) FROM ""tblPlanPurchaseDetail"" WHERE SubscriptionId = a.SubscriptionId AND UserId IS NOT NULL) AS UsedKeys,
                            (SELECT COUNT(*) FROM ""tblPlanPurchaseDetail"" WHERE SubscriptionId = a.SubscriptionId AND UserId IS NULL) AS UnusedKeys,
                            (SELECT COUNT(*) FROM ""tblPlanPurchaseDetail"" WHERE SubscriptionId = a.SubscriptionId AND IsExpired = true AND istransfered = false) AS ExpiredKeys,
                            (SELECT COUNT(*) FROM ""tblPlanPurchaseDetail"" WHERE SubscriptionId = a.SubscriptionId AND IsExpired = true AND istransfered = true) AS TransferedKeys,
                            b.Id, b.LicenseKey, e.ValidityDays, 
                        CASE WHEN 
                                d.companyname is not null then  CONCAT(d.CompanyName, ' (',d.EmailId,')') end AS AssignTo, 
                            g.id as amcid, g.amcenddate::date  - current_date::date as remainingdays,g.amcstartdate,g.amcenddate,
                            b.ActivationDate, b.ExpiryDate, b.IsExpired
                        FROM ""tblPlanPurchase"" a
                        INNER JOIN ""tblUsers"" c ON a.ClientId = c.Id
                        INNER JOIN ""tblPlanPurchaseDetail"" b ON a.SubscriptionId = b.SubscriptionId
                        INNER JOIN ""tblPlan"" e ON b.PlanId = e.Id
                        INNER JOIN ""tblProject"" f ON e.ProjectId = f.Id
                        LEFT OUTER JOIN ""tblUsers"" d ON b.UserId = d.Id 
                        left join ""tblAMC"" g on b.SubscriptionId = g.subscriptionid
                            where(g.reissue = false OR g.reissue is null)";
            if (nClientId != 0)
            {
                strQuery += $@" AND a.ClientId = {nClientId} AND b.activestatus = true";
            }
            LocalDM.ExecuteSelectCommand(strQuery, strTableAlias);
        }


        public void GetSuperAdminDetails(string strTableAlias)
        {
            strQuery = $@"select * from ""tblUsers"" WHERE role = 'SUPERADMIN'";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void PlanPurchaseCheckLicenseForUser(int nUserId, int nProjectId, string strTableAlias)
        {
            strQuery = $@"select * from ""tblPlanPurchaseDetail"" a
                        inner join ""tblPlan"" b On a.PlanId = b.Id
                        inner join ""tblProject"" c On b.ProjectId = c.Id
                        Where a.UserId = {nUserId} and c.Id = {nProjectId} and a.IsExpired = false";
            LocalDM.ExecuteSelectCommand(strQuery, strTableAlias);
        }

        public bool PlanPurchaseAssignLicense(tblPlanPurchaseDetail purchaseDetail)
        {
            strQuery = $@"UPDATE ""tblPlanPurchaseDetail"" SET 
                        ActivationDate = @ActivationDate, 
                        ExpiryDate = @ExpiryDate,
                        UserId = @UserId,
                        IsExpired = false
                        WHERE Id = @Id";
            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                        new Npgsql.NpgsqlParameter("@Id", purchaseDetail.Id),
                        new Npgsql.NpgsqlParameter("@ActivationDate", purchaseDetail.ActivationDate),
                        new Npgsql.NpgsqlParameter("@ExpiryDate", purchaseDetail.ExpiryDate),
                        new Npgsql.NpgsqlParameter("@UserId", purchaseDetail.UserId),
                        new Npgsql.NpgsqlParameter("@IsExpired", purchaseDetail.IsExpired),
                    };

            var res = LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters);
            if (res)
            {
                //code to maintain history
                strQuery = $@"INSERT INTO ""tblPlanPurchaseHistory""(PlanPurchaseDetailId, ActivationDate, ExpiryDate, UserId)
                            VALUES (@PlanPurchaseDetailId, @ActivationDate, @ExpiryDate, @UserId)";
                Npgsql.NpgsqlParameter[] npgsqlParameters1 = {
                        new Npgsql.NpgsqlParameter("@PlanPurchaseDetailId", purchaseDetail.Id),
                        new Npgsql.NpgsqlParameter("@ActivationDate", purchaseDetail.ActivationDate),
                        new Npgsql.NpgsqlParameter("@ExpiryDate", purchaseDetail.ExpiryDate),
                        new Npgsql.NpgsqlParameter("@UserId", purchaseDetail.UserId),
                    };
                res = LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters1);
            }

            return res;
        }

        public void PlanPurchaseGetLicenseInfo(int nId, string strTableAlias)
        {
            strQuery = $@"select a.*, b.* , c.IsChat
                        from ""tblPlanPurchaseDetail"" a
                        inner join ""tblPlan"" b On a.PlanId = b.Id
                        inner join ""tblProject"" c On b.ProjectId = c.Id
                        Where a.Id = {nId}";
            LocalDM.ExecuteSelectCommand(strQuery, strTableAlias);
        }

        public bool PlanPurchaseExpireLicense(int nId)
        {
            strQuery = $@"UPDATE ""tblPlanPurchaseDetail"" SET 
                        IsExpired = true,
                        IsTransfered = true
                        WHERE Id = @Id";
            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                        new Npgsql.NpgsqlParameter("@Id", nId),
                    };
            return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters);
        }
        public bool PlanPurchaseTransferLicense(tblPlanPurchaseDetail objPlanRequests)
        {
            strQuery = $@"INSERT INTO ""tblPlanPurchaseDetail""(SubscriptionId,PlanId,LicenseKey,ActivationDate,ExpiryDate,UserId,IsRenew,IsExpired,CreatedOn,ActiveStatus,LastUpdateDate,TransferFrom, DeviceId,iswarranty,warrantystartdate,warrantyenddate,istransfered)
                        VALUES(@SubscriptionId, @PlanId, @LicenseKey, @ActivationDate,@ExpiryDate,@UserId,NULL,NULL,@CreatedOn,@ActiveStatus,Current_date, @TransferFrom, @DeviceId,@iswarranty,@warrantystartdate,@warrantyenddate,@istransfered) RETURNING id;";
            Npgsql.NpgsqlParameter[] npgsqlParameters1 = {
                        new Npgsql.NpgsqlParameter("@SubscriptionId", objPlanRequests.SubscriptionId),
                        new Npgsql.NpgsqlParameter("@PlanId", objPlanRequests.PlanId),
                        new Npgsql.NpgsqlParameter("@LicenseKey", objPlanRequests.LicenseKey),
                        new Npgsql.NpgsqlParameter("@ActivationDate", objPlanRequests.ActivationDate),
                        new Npgsql.NpgsqlParameter("@ExpiryDate", objPlanRequests.ExpiryDate),
                        new Npgsql.NpgsqlParameter("@UserId", objPlanRequests.UserId),
                        new Npgsql.NpgsqlParameter("@TransferFrom", objPlanRequests.TransferFrom),
                        new Npgsql.NpgsqlParameter("@DeviceId", objPlanRequests.DeviceId),
                        new Npgsql.NpgsqlParameter("@CreatedOn", objPlanRequests.CreatedOn),
                        new Npgsql.NpgsqlParameter("@ActiveStatus", objPlanRequests.ActiveStatus),
                        new Npgsql.NpgsqlParameter("@iswarranty",objPlanRequests.IsWarranty),
                        new Npgsql.NpgsqlParameter("@warrantystartdate",objPlanRequests.WarrantyStartDate),
                        new Npgsql.NpgsqlParameter("@warrantyenddate",objPlanRequests.WarrantyEndDate),
                        new Npgsql.NpgsqlParameter("@istransfered",objPlanRequests.IsTransfered)
                    };
            LocalDM.ExecuteSelectCommand(strQuery, npgsqlParameters1, "tScopeValue");
            if (this._localDS.Tables["tScopeValue"]!=null && this._localDS.Tables["tScopeValue"].Rows.Count > 0)
            {
                var PlanPurchaseDetailId = Convert.ToInt32(this._localDS.Tables["tScopeValue"].Rows[0][0].ToString().Trim());
                //code to maintain history
                strQuery = $@"INSERT INTO ""tblPlanPurchaseHistory""(PlanPurchaseDetailId, ActivationDate, ExpiryDate, UserId)
                            VALUES (@PlanPurchaseDetailId, @ActivationDate, @ExpiryDate, @UserId)";
                Npgsql.NpgsqlParameter[] npgsqlParameters = {
                        new Npgsql.NpgsqlParameter("@PlanPurchaseDetailId", PlanPurchaseDetailId),
                        new Npgsql.NpgsqlParameter("@ActivationDate", objPlanRequests.ActivationDate),
                        new Npgsql.NpgsqlParameter("@ExpiryDate", objPlanRequests.ExpiryDate),
                        new Npgsql.NpgsqlParameter("@UserId", objPlanRequests.UserId),
                    };
                return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters);
            }
            else
            {
                return false;
            }
        }

        public void PlanPurchaseGetAllActiveLicense(string strTableAlias)
        {
            strQuery = $@"select a.*, b.EmailId, b.EmpCode,
                            c.CompanyName, b.CompanyName as Username 
                            from ""tblPlanPurchaseDetail"" a
                            inner join ""tblUsers"" b on a.UserId = b.Id
                            left join ""tblUsers"" c on b.CreatedBy = c.Id
                            WHERE ActivationDate is not null
                            and (IsExpired = false or IsExpired is null)
                            and a.ActiveStatus = true";

            LocalDM.ExecuteSelectCommand(strQuery, strTableAlias);
        }

        public void PlanPurchaseGetAllActiveWarranty(string strTableAlias)
        {
            strQuery = $@"  select * from ""tblPlanPurchaseDetail"" where 
                            (iswarranty = true or iswarranty is null)
                            and activeStatus = true";
            LocalDM.ExecuteSelectCommand(strQuery, strTableAlias);
        }

        public bool PlanPurchaseExpireWarranty(int nId)
        {
            strQuery = $@"UPDATE ""tblPlanPurchaseDetail"" SET 
                        iswarranty = false
                        WHERE Id = @Id";
            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                        new Npgsql.NpgsqlParameter("@Id", nId),
                    };
            return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters);
        }


        public void GetAllUnExpiredAMC(string strTableAlias)
        {
            strQuery = $@"  select * from ""tblAMC"" where 
                            (isexpired = false or isexpired is null)
                            and activeStatus = true";
            LocalDM.ExecuteSelectCommand(strQuery, strTableAlias);
        }

        public bool ExpiredAMC(int nId)
        {
            strQuery = $@"UPDATE ""tblAMC"" SET 
                        isexpired = true
                        WHERE Id = @Id";
            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                        new Npgsql.NpgsqlParameter("@Id", nId),
                    };
            return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters);
        }

        public void IsNotificationSendToday(int purchaseDetailId,string strTableAlias)
        {
            strQuery = $@"  select * from ""tblNotificationHistory"" where 
                            planpurchasedetailid={purchaseDetailId}
                            and createdon = current_date";
            LocalDM.ExecuteSelectCommand(strQuery, strTableAlias);
        }

        public bool InsertNotificationHistory(int purchaseDetailId)
        {
            strQuery = $@"INSERT INTO ""tblNotificationHistory""(planpurchasedetailid, isnotificationsend, createdon)
                    VALUES(@planpurchasedetailid,true,current_date)";
            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                new Npgsql.NpgsqlParameter("@planpurchasedetailid",purchaseDetailId),
            };
            return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters, true);
        }

        public void GetFAQByOffering(int nId, string strTableAlias)
        {
            strQuery = $@"select * from
                        ""tblOfferingFAQ"" 
                        Where offeringid = {nId}";
            LocalDM.ExecuteSelectCommand(strQuery, strTableAlias);
        }

        public void PlanPurchaseGetAllActiveLicenseByUser(int nUserId, string strTableAlias)
        {
            strQuery = $@"select a.*, b.ProjectId, c.ProjectStartUpLink, c.IsPro
                        from ""tblPlanPurchaseDetail"" a
                        inner join ""tblPlan"" b on b.Id = a.PlanId
                        inner join ""tblProject"" c On b.ProjectId = c.Id
                        WHERE
                        a.ActivationDate is not null
                        and (a.IsExpired = false or a.IsExpired is null)
                        and a.ActiveStatus = true
                        and a.UserId = {nUserId}";
            LocalDM.ExecuteSelectCommand(strQuery, strTableAlias);
        }

        public void PlanPurchaseGetActiveDeviceOnLicense(int nUserId, string sKey, string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblPlanPurchaseDetail"" 
                        WHERE UserId = {nUserId} AND LicenseKey = '{sKey}'";
            LocalDM.ExecuteSelectCommand(strQuery, strTableAlias);
        }

        #endregion


        #region [HelpSupport]
        public void GetAllEmployee(string strTableAlias)
        {
            strQuery = $@"SELECT id, companyname FROM ""tblUsers"" Where IsSuperAdmin = false  AND Role='SUPPORTREPRESENTATIVE'";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void GetIssueType(string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblTicketIssue"" where activestatus = true ";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void GetAllUnAssignedOfferings(int nEmployeeId, string strTableAlias)
        {
            strQuery = $@"select * from ""tblProject"" a
                           LEFT JOIN ""tblProjectAssign"" b on a.id = b.projectid
                        WHERE b.empid IS NULL OR b.empid = {nEmployeeId}";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public bool DeleteExistingEntryOfAssignEmoployee(int nEmpId)
        {
            strQuery = $@"delete from ""tblProjectAssign"" where empid = {nEmpId}";
            return LocalDM.ExecuteNonQueryCommand(strQuery, null, true);
        }

        public bool AssignOfferings(int projectId, int empId)
        {
            strQuery = $@"INSERT INTO ""tblProjectAssign""(projectid, empid, assigndate, createdon, activestatus, lastupdatedate)
                    VALUES(@projectid,@empid,@assigndate,@createdon,@activestatus,@lastupdatedate)";
            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                new Npgsql.NpgsqlParameter("@projectid",projectId),
                new Npgsql.NpgsqlParameter("@empid",empId),
                new Npgsql.NpgsqlParameter("@assigndate",DateTime.Now),
                new Npgsql.NpgsqlParameter("@createdon",DateTime.Now),
                new Npgsql.NpgsqlParameter("@activestatus",true),
                new Npgsql.NpgsqlParameter("@lastupdatedate",DateTime.Now),
            };
            return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters, true);
        }

        public void GetPlans(int nUserId, string strTableAlias)
        {
            strQuery = $@"SELECT d.id,a.subscriptionid,d.projectname 
                                from ""tblPlanPurchaseDetail"" a 
                            INNER JOIN ""tblPlanPurchase"" b
                                ON a.subscriptionid = b.subscriptionid
                            INNER JOIN ""tblPlan"" c
                                ON a.planid = c.id
                            INNER JOIN ""tblProject"" d
                                ON c.projectid = d.id
                            left join ""tblAMC"" e 
                                on a.subscriptionid = e.subscriptionid
                    WHERE a.userid = {nUserId} AND a.activestatus = true
                    and (a.isexpired is null or a.isexpired = false)
                    and(e.isexpired is null or e.isexpired = false)
                    and d.activestatus is true and c.activestatus = true";

            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        

        public void GetTicketsforClient(int nUserId, string strTableAlias)
        {
            string strQuery = $@"
                        select a.id,a.userid,a.uniqueid,a.ticketsubject,a.description,a.assignedto,a.ticketstatus,b.levelname,a.levelid,a.issueid,a.scenario,a.createdon,c.companyname, d.subscriptionid,a.resolution,e.attachment
                        from ""tblTicket"" a
                        JOIN ""tblTicketLevel"" b
                         ON a.levelid = b.id
                        JOIN ""tblUsers"" c
                            on c.id = a.userid
                        JOIN ""tblPlanPurchaseDetail"" d
                            on a.licenseid = d.id
                        JOIN ""tblTicketHistory"" e
                            on a.id = e.ticketid
                        where a.userid = {nUserId} AND  e.remarks = 'Ticket raised by client'
                        union 
						SELECT distinct a.id,a.userid,a.uniqueid,a.ticketsubject,a.description,a.assignedto,a.ticketstatus,b.levelname,a.levelid,a.issueid,a.scenario,a.createdon,c.companyname,
						d.serviceid::text || '-Service' as subscriptionid,a.resolution,e.attachment
                        from ""tblTicket"" a
                        JOIN ""tblTicketLevel"" b
                         ON a.levelid = b.id
                          JOIN ""tblUsers"" c
                            on c.id = a.userid
                         JOIN ""tblAMC"" d
                            on a.amcid = d.id
                        JOIN ""tblTicketHistory"" e
                            on a.id = e.ticketid
                         where a.userid = {nUserId} AND  e.remarks = 'Ticket raised by client'";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void GetTicketsforSupportPerson(string strTableAlias)
        {
            strQuery = $@"SELECT a.id,a.userid,a.uniqueid,a.ticketsubject,a.description,a.assignedto,a.ticketstatus,b.levelname,a.levelid,a.issueid,a.scenario,a.createdon,c.companyname,d.subscriptionid,a.resolution,e.attachment
                        from ""tblTicket"" a
                        JOIN ""tblTicketLevel"" b
                         ON a.levelid= b.id
                          JOIN ""tblUsers"" c
                            on c.id = a.userid
                         JOIN ""tblPlanPurchaseDetail"" d
                            on a.licenseid = d.id
                        JOIN ""tblTicketHistory"" e
                            on a.id = e.ticketid
                        where e.remarks = 'Ticket raised by client'
                        union 
						SELECT distinct a.id,a.userid,a.uniqueid,a.ticketsubject,a.description,a.assignedto,a.ticketstatus,b.levelname,a.levelid,a.issueid,a.scenario,a.createdon,c.companyname,
						d.serviceid::text as subscriptionid,a.resolution,e.attachment
                        from ""tblTicket"" a
                        JOIN ""tblTicketLevel"" b
                         ON a.levelid = b.id
                          JOIN ""tblUsers"" c
                            on c.id = a.userid
                         JOIN ""tblAMC"" d
                            on a.amcid = d.id
                        JOIN ""tblTicketHistory"" e
                            on a.id = e.ticketid
                        where e.remarks = 'Ticket raised by client'";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void GetSupportManagerEmail(string strTableAlias)
        {
            string strQuery = $@"SELECT id,emailid,contactno1,companyname from ""tblUsers"" WHERE role = 'SUPPORTMANAGER'";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void GetLastInsertedId(string strTableAlias)
        {
            string strQuery = $@"SELECT id FROM ""tblTicket"" ORDER BY id DESC LIMIT 1";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }


        public void GetLicense(int nUserId, string subscriptionId, string strTableAlias)
        {

            strQuery = $@"	select a.id as projectId,b.emailid as clientEmail,b.companyname,d.projectname,a.planid from 
                            ""tblPlanPurchaseDetail"" a
                            INNER JOIN ""tblUsers"" b
                                on a.userid = b.id
                            INNER JOIN ""tblPlan"" c
                                on c.id = a.planid
                            INNER JOIN ""tblProject"" d
                                on c.projectid = d.id
                    where a.subscriptionid = '{subscriptionId}' and a.userid = {nUserId} and (a.isexpired IS NULL OR a.isexpired= false)";

            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void GetAMC(int nUserId, string subscriptionId, string strTableAlias)
        {   
            strQuery = $@"select am.id as projectId,b.emailid as clientEmail,b.companyname,
	                    s.servicename as projectname
	                    from  ""tblUsers"" b 
                        Inner join ""tblAMC"" am
                        on b.id = am.clientid
                        left join ""tblService"" s
                        on am.serviceid = s.id
                    where am.serviceid = '{subscriptionId}'
                    and am.clientid = {nUserId} and(am.isexpired IS NULL OR am.isexpired = false)";

            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }


        public bool SaveTicketLevel(string ticketLevelName,string strTableAlias)
        {
            strQuery = $@"INSERT INTO ""tblTicketLevel""
                        (levelname,createdon,activestatus,lastupdatedate)
                            SELECT @ticketLevel, @createdOn,@activeStatus,@lastUpdateDate
                            WHERE
                            NOT EXISTS (
                           SELECT id FROM ""tblTicketLevel"" WHERE levelname = '{ticketLevelName}'
                           );
                        SELECT id from ""tblTicketLevel"" WHERE levelname = '{ticketLevelName}'";
            Npgsql.NpgsqlParameter[] npgsqlParameters1 = {
                        new Npgsql.NpgsqlParameter("@ticketLevel", ticketLevelName),
                        new Npgsql.NpgsqlParameter("@createdOn", DateTime.Now),
                        new Npgsql.NpgsqlParameter("@activeStatus", true),
                        new Npgsql.NpgsqlParameter("@lastUpdateDate", DateTime.Now),
            };
            return LocalDM.ExecuteSelectCommand(strQuery, npgsqlParameters1, strTableAlias, true);
        }

        public bool SaveTicket(string ticketSubject, string uniqueId, string ticketDiscription, string attachment, int ticketLevel, int userId, int projectId, int issueId, string Scenario, DateTime dateTime)
        {
            strQuery = $@"INSERT INTO ""tblTicket""(uniqueid,userid,licenseid,levelid,ticketsubject,description,ticketstatus,createdon,activestatus, lastupdatedate,issueid,scenario,isalreadyopen)
                        VALUES(@UniqueId,@UserId, @LicenseId, @LevelId, @TicketSubject,@Description, @TicketStatus, @createdon ,@ActiveStatus,@lastupdatedate,@IssueId, @Scenario,@isalreadyopen) RETURNING id; ";
            Npgsql.NpgsqlParameter[] npgsqlParameters1 = {
                        new Npgsql.NpgsqlParameter("@UniqueId",uniqueId),
                        new Npgsql.NpgsqlParameter("@UserId", userId),
                        new Npgsql.NpgsqlParameter("@LicenseId", projectId),
                        new Npgsql.NpgsqlParameter("@LevelId", ticketLevel),
                        new Npgsql.NpgsqlParameter("@TicketSubject", ticketSubject),
                        new Npgsql.NpgsqlParameter("@Description", ticketDiscription),
                        new Npgsql.NpgsqlParameter("@TicketStatus","Open"),

                        new Npgsql.NpgsqlParameter("@ActiveStatus", true),
                        new Npgsql.NpgsqlParameter("@IssueId",issueId),
                        new Npgsql.NpgsqlParameter("@createdon",dateTime),
                        new Npgsql.NpgsqlParameter("@lastupdatedate",dateTime),

                        new Npgsql.NpgsqlParameter("@Scenario",Scenario),
                        new Npgsql.NpgsqlParameter("@isalreadyopen",false)

            };
            LocalDM.ExecuteSelectCommand(strQuery, npgsqlParameters1, "tTicketId");
            if (this._localDS.Tables["tTicketId"] != null && this._localDS.Tables["tTicketId"].Rows.Count > 0)
            {
                var ticketId = Convert.ToInt32(this._localDS.Tables["tTicketId"].Rows[0][0].ToString().Trim());
                //code to maintain history
                strQuery = $@"INSERT INTO ""tblTicketHistory""(ticketid,userid,  ticketstatus,remarks,attachment, ticketstatusdate,action)
                            VALUES (@TicketId,@userId, @TicketStatus, @remarks,@attachment,@ticketstatusdate ,@action)";
                attachment = attachment == null ? "" : attachment;
                Npgsql.NpgsqlParameter[] npgsqlParameters = {
                        new Npgsql.NpgsqlParameter("@TicketId", ticketId),
                        new Npgsql.NpgsqlParameter("@userId",userId),
                        new Npgsql.NpgsqlParameter("@TicketStatus", "Open"),
                        new Npgsql.NpgsqlParameter("@remarks","Ticket raised by client"),
                        new Npgsql.NpgsqlParameter("@attachment",attachment),
                        new Npgsql.NpgsqlParameter("@ticketstatusdate",dateTime),
                        new Npgsql.NpgsqlParameter("@action","Ticket logged"),
                    };
                return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters);
            }
            else
            {
                return false;
            }
        }

        

        public bool UpdateTicketStatus(string status, int empId, string remarks, int ticketId, int userId,DateTime dateTime)
        {
            strQuery = $@"Update ""tblTicket"" SET assignedto = @assignedto,assigndate= @assigndate, ticketstatus = @status,lastupdatedate = @lastupdatedate
                            WHERE id = {ticketId}";
            Npgsql.NpgsqlParameter[] npgsqlParameters1 = {
                        new Npgsql.NpgsqlParameter("@assignedto", empId),
                        new Npgsql.NpgsqlParameter("@status", status),
                        new Npgsql.NpgsqlParameter("@assigndate", dateTime),
                        new Npgsql.NpgsqlParameter("@lastupdatedate", dateTime),


            };
            if (LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters1))
            {
                strQuery = $@"INSERT INTO ""tblTicketHistory""(ticketid,userid,assignedto,ticketstatus,remarks,ticketstatusdate,action)
                            VALUES(@ticketid,@userid,@assignedto,@status,@remarks,@ticketstatusdate,@action)";

                Npgsql.NpgsqlParameter[] npgsqlParameters = {
                        new Npgsql.NpgsqlParameter("@ticketid",ticketId),
                        new Npgsql.NpgsqlParameter("@userid",userId),
                        new Npgsql.NpgsqlParameter("@assignedto", empId),
                        new Npgsql.NpgsqlParameter("@status", status),
                        new Npgsql.NpgsqlParameter("@remarks", remarks),
                        new Npgsql.NpgsqlParameter("@action","Ticket assigned "),
                        new Npgsql.NpgsqlParameter("@ticketstatusdate",dateTime)
                };
                return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters);
            }
            else
            {
                return false;
            }
        }

        public bool UpdateTicketBySupport(int levelId, string resolution, int ticketId,DateTime dateTime)
        {
            strQuery = $@"Update ""tblTicket"" SET resolution = @resolution, levelid = @levelId, lastupdatedate = @lastupdatedate
                            WHERE id = {ticketId}";
            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                        new Npgsql.NpgsqlParameter("@resolution",(object)resolution ?? DBNull.Value ),
                        new Npgsql.NpgsqlParameter("@levelId",  levelId),
                        new Npgsql.NpgsqlParameter("@lastupdatedate",  dateTime),
            };
            return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters);
        }

        public bool CloseTicket(string status, int empId, string remarks, int ticketId,DateTime dateTime)
        {
            strQuery = $@"Update ""tblTicket"" SET ticketstatus = @status,lastupdatedate = @lastupdatedate, closingdate= now()::timestamp(0),closedby=@closedby, resolution = @resolution
                            WHERE id = {ticketId}";
            Npgsql.NpgsqlParameter[] npgsqlParameters1 = {
                        new Npgsql.NpgsqlParameter("@status", status),
                        new Npgsql.NpgsqlParameter("@closedby",empId),
                        new Npgsql.NpgsqlParameter("@resolution",remarks),
                        new Npgsql.NpgsqlParameter("@lastupdatedate",dateTime),
            };

            if (LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters1))
            {
                strQuery = $@"INSERT  INTO ""tblTicketHistory""(ticketid,userid,assignedto, ticketstatus,remarks,ticketstatusdate,action)
                                VALUES(@ticketid,@userid,@assignedto,@ticketstatus,@remarks,now()::timestamp(0),@action)";
                Npgsql.NpgsqlParameter[] npgsqlParameters = {

                        new Npgsql.NpgsqlParameter("@ticketid",ticketId),
                        new Npgsql.NpgsqlParameter("@userid",empId),
                        new Npgsql.NpgsqlParameter("@assignedto", empId),
                        new Npgsql.NpgsqlParameter("@ticketstatus", status),
                        new Npgsql.NpgsqlParameter("@remarks", "Ticket closed by support team"),
                        new Npgsql.NpgsqlParameter("@action","Ticket is closed")
                };
                return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters);
            }
            else
            {
                return false;
            }
        }

        public void GetAllPlansPurchasedByClients(string strTableAlias)
        {
            string strQuery = $@"
                        SELECT x.* from
                        (SELECT distinct on (b.subscriptionId,g.id) b.subscriptionId,g.id, c.CompanyName, c.EmailId, c.Logo, b.planid,e.planname,
                         a.ClientId, a.PurchaseDate,
                        f.Id AS ProjectId, f.ProjectName,b.iswarranty,b.warrantyenddate,
                        (SELECT COUNT(*) FROM ""tblPlanPurchaseDetail"" WHERE SubscriptionId = a.SubscriptionId) AS TotalKeys,
                        (SELECT COUNT(*) FROM ""tblPlanPurchaseDetail"" WHERE SubscriptionId = a.SubscriptionId AND UserId IS NOT NULL) AS UsedKeys,
                        (SELECT COUNT(*) FROM ""tblPlanPurchaseDetail"" WHERE SubscriptionId = a.SubscriptionId AND UserId IS NULL) AS UnusedKeys,
                        (SELECT COUNT(*) FROM ""tblPlanPurchaseDetail"" WHERE SubscriptionId = a.SubscriptionId AND IsExpired = true and IsTransfered = false) AS ExpiredKeys,
                        (SELECT COUNT(*) FROM ""tblPlanPurchaseDetail"" WHERE SubscriptionId = a.SubscriptionId AND IsExpired = true and IsTransfered = true) AS TransferedKeys
                       --b.Id, b.LicenseKey, e.ValidityDays, d.CompanyName AS AssignTo, b.ActivationDate, b.ExpiryDate, b.IsExpired
                       ,g.isexpired as AMCExpired,g.amcstartdate,g.amcenddate, g.amcenddate::date  - current_date::date as remainingdays, g.iscancelled as AMCCancelled,g.reissue,g.id as amcid
                        FROM ""tblPlanPurchase"" a
                       INNER JOIN ""tblUsers"" c ON a.ClientId = c.Id
                        INNER JOIN ""tblPlanPurchaseDetail"" b ON a.SubscriptionId = b.SubscriptionId
                        INNER JOIN ""tblPlan"" e ON b.PlanId = e.Id
                        INNER JOIN ""tblProject"" f ON e.ProjectId = f.Id
                        LEFT OUTER JOIN ""tblUsers"" d ON b.UserId = d.Id
                        LEFT JOIN ""tblAMC"" g 
                            ON a.ClientId = g.clientid and e.id = g.planid and a.subscriptionid = g.subscriptionid
                            WHERE b.isexpired = false OR b.isexpired is null
                            	ORDER BY b.subscriptionId, g.id desc)x order by x.clientid,x.planid,x.amcid desc;";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        

        public void GetAMCPurchasedByClient(int nClientId, int nPlanId, string subscriptionId,int amc, string strTableAlias)
        {
            string strQuery = $@"SELECT * FROM ""tblAMC"" WHERE clientid = {nClientId} AND planid = {nPlanId} AND subscriptionid = '{subscriptionId}'
              AND ({amc} = 0 OR id = {amc}  )";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public bool UpdateAMC(string subscriptionid, int planId, int clientId)
        {
            string strQuery = $@"UPDATE ""tblAMC"" SET reissue = true where subscriptionid= '{subscriptionid}' AND planid= {planId} AND clientid = {clientId} ";
            return LocalDM.ExecuteNonQueryCommand(strQuery, null);
        }

        public bool AMCUpsert(tblAMC oModel)
        {
            if (oModel.Id != 0)
            {
                strQuery = $@"UPDATE ""tblAMC"" 
                        SET amcstartdate = @AMCStartDate,
                        amcenddate = @AMCEndDate, budgetedmandays = @BudgetedManDays,
                        ActiveStatus = @ActiveStatus, LastUpdateDate = current_date,
                        serviceid=@ServiceId
                        iscancelled =@iscancelled WHERE Id = @Id";
            }
            else
            {
                strQuery = $@"INSERT INTO ""tblAMC"" ( clientid, planid, amcstartdate, amcenddate, budgetedmandays, activestatus, createdon, lastupdatedate, subscriptionid,isexpired,iscancelled,reissue,serviceid) 
                            VALUES (@ClientId, @PlanId, @AMCStartDate, @AMCEndDate, @BudgetedManDays, @ActiveStatus, current_date, current_date,@SubscriptionID, false,false,false,@IsService)";
            }

            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                new Npgsql.NpgsqlParameter("@Id",oModel.Id),
                new Npgsql.NpgsqlParameter("@ClientId", oModel.ClientId),
                new Npgsql.NpgsqlParameter("@PlanId", oModel.PlanId),
                new Npgsql.NpgsqlParameter("@AMCStartDate", oModel.AMCStartDate),
                new Npgsql.NpgsqlParameter("@AMCEndDate", oModel.AMCEndDate),
                new Npgsql.NpgsqlParameter("@BudgetedManDays", oModel.BudgetedManDays),
                new Npgsql.NpgsqlParameter("@ActiveStatus", oModel.ActiveStatus),
                new Npgsql.NpgsqlParameter("@SubscriptionID", oModel.SubscriptionId),
                new Npgsql.NpgsqlParameter("@ActiveStatus", oModel.ActiveStatus),
                new NpgsqlParameter("@iscancelled",oModel.IsCancelled),
                new NpgsqlParameter("@IsService",oModel.IsService),
                new NpgsqlParameter("@IsOfferings",oModel.IsOfferings)
            };
            return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters);
        }

        public void GetClientIdBySubAdmin(int sId, string strTableAlias)
        {
            string strQuery = $@"select t2.id from ""tblUsers"" t1, ""tblUsers"" t2 WHERE t1.createdby = t2.id AND t1.id = {sId}";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void CheckWarrantyOfPlan(string subscriptionId, int nUserId, string strTableAlias)
        {
            string strQuery = $@"select * from ""tblPlanPurchaseDetail"" where subscriptionid = '{subscriptionId}' 
                AND userid = {nUserId}
                and iswarranty = true and current_date between warrantystartdate and warrantyenddate";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void CheckAMCForSubAdmin(int nUserId, int planId, string subscriptionId, string strTableAlias)
        {
            string strQuery = $@"select * from ""tblAMC"" a
	                            JOIN ""tblUsers"" b
	                                on a.clientid = b.createdby
                                where current_date between a.amcstartdate and a.amcenddate and
                                a.planid = {planId} and a.subscriptionid = '{subscriptionId}' AND a.activestatus = true 
                                AND a.isexpired = false AND a.iscancelled = false
				                AND b.id = {nUserId}";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void CheckAMC(int clientid, int planId, string subscriptionId, string strTableAlias)
        {
            string strQuery = $@"
                
                                Select a.id from ""tblAMC"" a
	                            JOIN ""tblUsers"" b
	                                on a.clientid = b.createdby
                                where current_date between a.amcstartdate and a.amcenddate and
                                a.planid = {planId} and a.subscriptionid = '{subscriptionId}' AND a.activestatus = true 
                                AND a.isexpired = false AND a.iscancelled = false
				                AND b.id = {clientid}
                                UNION
                                select id from ""tblAMC"" 
                                    where current_date between amcstartdate and amcenddate and
                                    clientid = {clientid} and planid = {planId} and subscriptionid = '{subscriptionId}' AND activestatus = true
                                    AND isexpired = false AND iscancelled = false";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void CheckServiceAMC(int clientid, int planId, string subscriptionId, string strTableAlias)
        {
            string strQuery = $@"
                
                                Select a.id from ""tblAMC"" a
	                            JOIN ""tblUsers"" b
	                                on a.clientid = b.createdby
                                where current_date between a.amcstartdate and a.amcenddate and
                                a.id = {planId} and a.serviceid = '{subscriptionId}' AND a.activestatus = true 
                                AND a.isexpired = false AND a.iscancelled = false
				                AND b.id = {clientid}
                                UNION
                                select id from ""tblAMC"" 
                                    where current_date between amcstartdate and amcenddate and
                                    clientid = {clientid} and id = {planId} and serviceid = '{subscriptionId}' AND activestatus = true
                                    AND isexpired = false AND iscancelled = false";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        //ye hai
        public bool AddComment(int tId, int nUserId, int nAssignedTo, string remarks, string attachment,DateTime dateTime)
        {
            string strQuery = $@"INSERT INTO ""tblTicketHistory""(ticketid,userid,assignedto,ticketstatus,remarks,attachment,ticketstatusdate,action)
            VALUES(@Id,@userId,@assignedTo,@ticketStatus,@remarks,@attachment,@ticketstatusdate,@action)";
            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                new Npgsql.NpgsqlParameter("@Id",tId),
                new Npgsql.NpgsqlParameter("@userId",nUserId ),
                new Npgsql.NpgsqlParameter("@assignedTo", nAssignedTo),
                new Npgsql.NpgsqlParameter("@ticketStatus", "Open"),
                new Npgsql.NpgsqlParameter("@remarks", remarks),
                new Npgsql.NpgsqlParameter("@attachment", (object)attachment ?? DBNull.Value),
                new Npgsql.NpgsqlParameter("@action", "Comment added on ticket"),
                new Npgsql.NpgsqlParameter("@ticketstatusdate", dateTime),
            };
            return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters);
        }

        public void GetTicketUserName(int tId, string strTableAlias)
        {
            string strQuery = $@"select a.*,b.companyname,b.contactno1,b.emailid from ""tblTicket"" a 
                                JOIN ""tblUsers""  b on a.userid = b.id
                                WHERE a.id = {tId}";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void GetPlansForSupport(string strTableAlias)
        {
            string strQuery = $@"SELECT DISTINCT a.subscriptionid,d.projectname from ""tblPlanPurchaseDetail""
                            a INNER JOIN ""tblPlanPurchase"" b
                              ON a.subscriptionid = b.subscriptionid
                            INNER JOIN ""tblPlan"" c
                             ON a.planid = c.id
                            INNER JOIN ""tblProject"" d
                             ON c.projectid = d.id
                            union
							 SELECT DISTINCT d.id::text as subscriptionid,d.servicename  as projectname
                                from  ""tblAMC"" e
                            INNER JOIN ""tblService"" d
                                ON e.serviceid = d.id";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }


        

        public void TimelineByTicketId(int ticketId, string strTableAlias)
        {
            strQuery = $@"select a.description, a.resolution,c.companyname as username, 
                        CASE WHEN
                                (c.role = 'SUBADMIN' and c.logo is null) 
                                        then (select u.logo from 
                                                ""tblUsers"" u 
                                            where u.id = c.createdby)
                                else c.logo
                        end as logo,
                        d.companyname as assignedUser, b.* 
                          from ""tblTicket"" a 
                          join ""tblTicketHistory"" b
                            on a.id = b.ticketid
                          join ""tblUsers"" c
                            on c.id = b.userid
                          left join ""tblUsers"" d
                            on d.id = b.assignedto
                          where a.id = {ticketId}
                        order by ticketstatusdate asc";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void GetNotificationForSupport(int nUserId, string strTableAlias)
        {
            string strQuery = $@" SELECT * FROM ""tblNotification"" WHERE isread = false AND refrenceid = {nUserId} order by createdon desc limit 3";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void GetSupportManagerDashboard(int nUserId, string strTableAlias)
        {
            string strQuery = $@"
            SELECT COUNT(*) from ""tblTicket"" where ticketstatus = 'Open' AND assignedto is null;
            SELECT COUNT(*) from ""tblTicket"" where ticketstatus = 'Open' AND assignedto = { nUserId };
            SELECT COUNT(*) from ""tblTicket"" where ticketstatus = 'Close' AND assignedto = { nUserId };
            SELECT COUNT(*) from ""tblTicket"";";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void GetSupportRepDashboard(int nUserId, string strTableAlias)
        {
            string strQuery = $@"
            SELECT COUNT(*) from ""tblTicket"" where ticketstatus = 'Open' AND assignedto = { nUserId };
            SELECT COUNT(*) from ""tblTicket"" where ticketstatus = 'Close' AND assignedto = { nUserId };   
            SELECT COUNT(*) from ""tblTicket"" ;
            SELECT COUNT(*) from ""tblTicket"" where ticketstatus = 'Open' AND assignedto is null;";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void GetSubAdminDashboard(int nUserId, string strTableAlias)
        {
            string strQuery = $@"
            SELECT COUNT(*) from ""tblTicket"" where ticketstatus = 'Open' AND assignedto = { nUserId };
            SELECT COUNT(*) from ""tblTicket"" where ticketstatus = 'Close' AND assignedto = { nUserId };   
            SELECT COUNT(*) from ""tblTicket"" where userid={nUserId} ;
            SELECT COUNT(*) from ""tblTicket"" where ticketstatus = 'Open' AND userid={nUserId} AND assignedto is null;";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void GetClientsForAmc(string strTableAlias)
        {
            string strQuery = $@"select id,companyname from ""tblUsers"" 
                                where role = 'ADMIN'";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void GetTicketIssue(string strTableAlias)
        {
            string strQuery = $@"select * from ""tblTicketIssue"";";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public bool TicketIssueUpsert(tblTicketIssue issue)
        {
            if (issue.Id == 0)
            {
                strQuery = $@"INSERT INTO ""tblTicketIssue""(issuetype, discription, activestatus, createdon, lastupdatedate)
		                    VALUES(@issueType, @discription, @activestatus, current_date,current_date);";
            }
            else
            {
                strQuery = $@"UPDATE ""tblTicketIssue""
		                    SET 
		                    issuetype = @issueType, 
		                    discription = @discription, 
		                    activestatus = @activestatus, 
		                    lastupdatedate = current_date
		                    WHERE Id = @Id;";
            }

            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                new Npgsql.NpgsqlParameter("@Id",issue.Id),
                new Npgsql.NpgsqlParameter("@issueType",issue.Issue),
                new Npgsql.NpgsqlParameter("@discription",issue.Description),
                new Npgsql.NpgsqlParameter("@activestatus",issue.ActiveStatus),
            };
            return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters);
        }

        public bool DeleteTicketIssue(int nId)
        {
            strQuery = $@"delete from ""tblTicketIssue"" where id = {nId}";
            return LocalDM.ExecuteNonQueryCommand(strQuery, null, true);
        }

        #endregion

        #region [Notification]

        public void GetNotification(int nUserId, string strTableAlias)
        {
            strQuery = $@"SELECT * FROM ""tblNotification"" Where refrenceid = {nUserId}";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public bool UpdateNotification(int? nId)
        {
            string strQuery = $@"UPDATE ""tblNotification"" SET isread = true WHERE id ={nId} ";
            return LocalDM.ExecuteNonQueryCommand(strQuery, null);
        }

        //ye
        public bool InsertNotification(string subject, string message, string type,DateTime dateTime, int refrenceId)
        {

            string strQuery = $@"INSERT INTO ""tblNotification""(subject,message,type,isread,createdon,refrenceid)
            VALUES(@subject,@message,@type,false,@createdon, @refrenceId)";
            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                new Npgsql.NpgsqlParameter("@subject",subject ),
                new Npgsql.NpgsqlParameter("@message", message),
                new Npgsql.NpgsqlParameter("@type", type),
                new Npgsql.NpgsqlParameter("@createdon",dateTime),
                new Npgsql.NpgsqlParameter("@refrenceId", refrenceId),
            };
            return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters);
        }

        public bool DeleteNotification(int nId)
        {
            strQuery = $@"delete from ""tblNotification"" where id = {nId}";
            return LocalDM.ExecuteNonQueryCommand(strQuery, null, true);
        }

        public bool DeleteNotifications(int nUserId)
        {
            strQuery = $@"delete from ""tblNotification"" where refrenceid = {nUserId}";
            return LocalDM.ExecuteNonQueryCommand(strQuery, null, true);
        }
        #endregion

        #region [tblCart]
        public bool AddProductToCart(int userId, int offeringId, int planId, int price)
        {
            strQuery = $@"INSERT INTO ""tblCart"" (offeringid, planid, userid, totallicense, price, CreatedOn, LastUpdateDate)
                    VALUES(@offeringId,@planId,@userId,1,@price,current_date, current_date)";
            Npgsql.NpgsqlParameter[] npgsqlParameters = {
                new Npgsql.NpgsqlParameter("@offeringId",offeringId),
                new Npgsql.NpgsqlParameter("@planId",planId),
                new Npgsql.NpgsqlParameter("@userId",userId),
                new Npgsql.NpgsqlParameter("@price",price),
            };
            return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters, true);
        }

        public void CheckDuplicateEntryCart(int planId, int offeringId, int userId, string strTableAlias)
        {
            string strQuery = $@"
                SELECT * FROM ""tblCart"" WHERE offeringid = {offeringId} AND planid = {planId} AND userid = {userId}";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void GetProductsFromCart(int userId, string strTableAlias)
        {
            string strQuery = $@" SELECT a.*,b.projectname,c.planname FROM ""tblCart"" a
            INNER JOIN ""tblProject"" b
                ON a.offeringid = b.id
            INNER JOIN ""tblPlan"" c
                ON a.planid = c.id
            AND a.userid = {userId} order by a.createdon desc";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public bool UpdateCart(int cId, int totalLicense)
        {
            string strQuery = $@"UPDATE ""tblCart"" SET totallicense ={totalLicense}, lastupdatedate = current_date WHERE id = {cId} ";
            return LocalDM.ExecuteNonQueryCommand(strQuery, null);
        }

        public bool RemoveProductsFromCart(int cId)
        {
            string strQuery = $@"DELETE FROM ""tblCart"" WHERE id = {cId}";
            return LocalDM.ExecuteNonQueryCommand(strQuery, null, true);
        }

        public void GetAllProducts(int userId, string strTableAlias)
        {
            string strQuery = $@" SELECT a.*,b.projectname,c.planname FROM ""tblCart"" a
            INNER JOIN ""tblProject"" b
                ON a.offeringid = b.id
            INNER JOIN ""tblPlan"" c
                ON a.planid = c.id
            AND a.userid = {userId}";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public bool RemoveAllProductsForUsers(int nUserId)
        {
            string strQuery = $@"DELETE FROM ""tblCart"" WHERE userid = {nUserId}";
            return LocalDM.ExecuteNonQueryCommand(strQuery, null, true);
        }

        #endregion

        #region [tblMobileAppDevice]
        public bool MobileAppDeviceInsert(string sUsername, string sLicense, string sDevice, string sDomain, string OfferingCategory)
        {
            strQuery = $@"SELECT * FROM ""tblMobileAppDevice"" WHERE Username = '{sUsername}' AND License = '{sLicense}' AND Device = '{sDevice}'";
            LocalDM.ExecuteSelectCommand(strQuery, "tCheckDevice");
            if (this._localDS.Tables["tCheckDevice"] != null && this._localDS.Tables["tCheckDevice"].Rows.Count > 0)
            {
                strQuery = $@"UPDATE ""tblMobileAppDevice"" 
                        SET IsAllowed = true
                        WHERE Username = '{sUsername}' AND License = '{sLicense}' AND Device = '{sDevice}' AND OfferingCategory = '{OfferingCategory}'";
                return LocalDM.ExecuteNonQueryCommand(strQuery);
            }
            else
            {
                strQuery = $@"INSERT INTO ""tblMobileAppDevice"" (Username, License, Device, IsAllowed, LastUpdateDate, Domain, OfferingCategory)
                    VALUES(@Username, @License, @Device, @IsAllowed, current_date, @Domain, @OfferingCategory)";
                Npgsql.NpgsqlParameter[] npgsqlParameters = {
                    new Npgsql.NpgsqlParameter("@Username",sUsername),
                    new Npgsql.NpgsqlParameter("@License",sLicense),
                    new Npgsql.NpgsqlParameter("@Device",sDevice),
                    new Npgsql.NpgsqlParameter("@Domain",sDomain),
                    new Npgsql.NpgsqlParameter("@IsAllowed",true),
                    new Npgsql.NpgsqlParameter("@OfferingCategory",OfferingCategory),
                };
                return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters, true);
            }
        }

        public bool MobileAppDeviceUpdate(string sUsername, string sDevice, string sDomain, string OfferingCategory, string Token)
        {
            strQuery = $@"UPDATE ""tblMobileAppDevice"" 
                        SET Token = '{Token}'
                        WHERE Username = '{sUsername}' AND Device = '{sDevice}' AND OfferingCategory = '{OfferingCategory}' AND Domain = '{sDomain}'";
            return LocalDM.ExecuteNonQueryCommand(strQuery);
        }

        public bool MobileAppDeviceCheckExists(string sUsername, string sDevice, string sDomain, string OfferingCategory)
        {
            strQuery = $@"SELECT * FROM ""tblMobileAppDevice"" WHERE Username = '{sUsername}' AND Device = '{sDevice}' AND OfferingCategory = '{OfferingCategory}' AND Domain = '{sDomain}'";
            LocalDM.ExecuteSelectCommand(strQuery, null, "tMobileAppDeviceCheckExists", true);
            if (this._localDS.Tables["tMobileAppDeviceCheckExists"] != null &&
                this._localDS.Tables["tMobileAppDeviceCheckExists"].Rows.Count > 0)
                return true;
            else
                return false;
        }

        public void MobileAppDeviceGetByUserLicense(string sUsername, string sLicense, string sDevice, string strTableAlias)
        {
            string strQuery = $@"SELECT * FROM  ""tblMobileAppDevice"" 
                            WHERE Username = '{sUsername}' AND License = '{sLicense}' AND Device = '{sDevice}'";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void MobileAppDeviceGetAllDevice(string sUsername, string sLicense, string strTableAlias)
        {
            string strQuery = $@"SELECT * FROM  ""tblMobileAppDevice"" 
                            WHERE Username = '{sUsername}' AND License = '{sLicense}'";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void MobileAppDeviceGetAllDeviceByUsernameAndDomain(string sUsername,
            string sDomain,
            string sOfferingCategory, string strTableAlias)
        {
            if (string.IsNullOrEmpty(sUsername))
            {
                strQuery = $@"SELECT * FROM  ""tblMobileAppDevice"" 
                            WHERE OfferingCategory = '{sOfferingCategory}'
                            AND Domain = '{sDomain}'";
            }
            else
            {
                strQuery = $@"SELECT * FROM  ""tblMobileAppDevice"" 
                            WHERE Username = '{sUsername}' AND OfferingCategory = '{sOfferingCategory}'
                            AND Domain = '{sDomain}'";
            }
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public void MobileAppDeviceGetByUser(string sUsername, string sLicense, string strTableAlias) 
        {
            string strQuery = $@"SELECT * FROM  ""tblMobileAppDevice"" 
                                WHERE Username = '{sUsername}' AND License = '{sLicense}' AND IsAllowed = true";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }

        public bool MobileAppDeviceRemoveDeviceId(string sUsername, string sLicense, string Device)
        {
            strQuery = $@"UPDATE ""tblMobileAppDevice"" 
                        SET IsAllowed = false
                        WHERE Username = '{sUsername}' AND License = '{sLicense}' AND Device = '{Device}'";
            return LocalDM.ExecuteNonQueryCommand(strQuery);
        }
        public bool MobileAppDeviceRemoveDevice(string sUsername, string sDevice, string sDomain, string sOfferingCategory)
        {
            strQuery = $@"DELETE FROM ""tblMobileAppDevice"" 
                        WHERE Username = '{sUsername}' AND Domain = '{sDomain}' AND Device = '{sDevice}' AND OfferingCategory = '{sOfferingCategory}'";
            return LocalDM.ExecuteNonQueryCommand(strQuery);
        }
        public bool MobileAppDeviceRemoveDeviceAll(string sUsername, string sDomain, string sOfferingCategory)
        {
            strQuery = $@"DELETE FROM ""tblMobileAppDevice"" 
                        WHERE Username = '{sUsername}' AND Domain = '{sDomain}' AND OfferingCategory = '{sOfferingCategory}'";
            return LocalDM.ExecuteNonQueryCommand(strQuery);
        }
        #endregion

        #region [MobileAppApiAuthenticate]
        public string GetLicenseKeyByUserId(int nUserId, string sOfferingCat)
        {
            strQuery = $@"select LicenseKey
                        from ""tblPlanPurchaseDetail"" a
                        inner join ""tblPlan"" b on a.PlanId = b.Id
                        inner join ""tblProject"" c ON b.ProjectId = c.Id
                        Where UserId = {nUserId} AND c.OfferingCategory = '{sOfferingCat}' and (a.IsExpired is null or a.IsExpired = false)";
            LocalDM.ExecuteSelectQuery(strQuery, "tGetLicenseKeyByUserId");

            if(this._localDS.Tables["tGetLicenseKeyByUserId"] != null && this._localDS.Tables["tGetLicenseKeyByUserId"].Rows.Count > 0)
            {
                return this._localDS.Tables["tGetLicenseKeyByUserId"].Rows[0][0].ToString().Trim();
            }
            else
            {
                return string.Empty;
            }
        }
        #endregion


        #region Nucleus

        public void NucleusDeviceGetByUser(int userid,  string strTableAlias)
        {
            string strQuery = $@"SELECT a.id,emailid,deviceid,activationdate,licensekey FROM ""tblUsers"" a inner join ""tblPlanPurchaseDetail"" b
                    on a.id = b.userid WHERE a.id= {userid} and b.isexpired=false";
            LocalDM.ExecuteSelectCommand(strQuery, null, strTableAlias, true);
        }
        public bool NucleusDeviceInsert(string sUsername, string sLicense, string sDevice, string sDomain, string OfferingCategory)
        {
            strQuery = $@"SELECT * FROM ""tblNucleusUser"" WHERE Username = '{sUsername}' AND License = '{sLicense}' AND Device = '{sDevice}'";
            LocalDM.ExecuteSelectCommand(strQuery, "tCheckDevice");
            if (this._localDS.Tables["tCheckDevice"] != null && this._localDS.Tables["tCheckDevice"].Rows.Count > 0)
            {
                strQuery = $@"UPDATE ""tblNucleusUser"" 
                        SET IsAllowed = true
                        WHERE Username = '{sUsername}' AND License = '{sLicense}' AND Device = '{sDevice}' AND OfferingCategory = '{OfferingCategory}'";
                return LocalDM.ExecuteNonQueryCommand(strQuery);
            }
            else
            {
                strQuery = $@"INSERT INTO ""tblNucleusUser"" (Username, License, Device, IsAllowed, LastUpdateDate, Domain, OfferingCategory)
                    VALUES(@Username, @License, @Device, @IsAllowed, current_date, @Domain, @OfferingCategory)";
                Npgsql.NpgsqlParameter[] npgsqlParameters = {
                    new Npgsql.NpgsqlParameter("@Username",sUsername),
                    new Npgsql.NpgsqlParameter("@License",sLicense),
                    new Npgsql.NpgsqlParameter("@Device",sDevice),
                    new Npgsql.NpgsqlParameter("@Domain",sDomain),
                    new Npgsql.NpgsqlParameter("@IsAllowed",true),
                    new Npgsql.NpgsqlParameter("@OfferingCategory",OfferingCategory),
                };
                return LocalDM.ExecuteNonQueryCommand(strQuery, npgsqlParameters, true);
            }
        }

        public void NucleusCheckUser(string sUsername,string sLicense,string strTableAlias)
        {
            strQuery = $@"select * from ""tblUsers"" a inner join ""tblPlanPurchaseDetail"" b on a.id=b.userid
                        Where a.emailid = '{sUsername}' AND b.deviceid = '{sLicense}' and (b.IsExpired is null or b.IsExpired = false)";
            LocalDM.ExecuteSelectQuery(strQuery, strTableAlias);
        }

        public bool NucleusUpdatePassword(string sUsername, string sDevice,string sPwd)
        {
            strQuery = $@"update ""tblUsers"" a
                        set password = '{sPwd}'
                        from  ""tblPlanPurchaseDetail"" b
                        Where a.id = b.userid and a.emailid = '{sUsername}' AND b.deviceid = '{sDevice}'";
            return LocalDM.ExecuteNonQueryCommand(strQuery);
        }
        #endregion

        public void CheckAccessIdExits(string EmpCode, string Domain, string strTableAlias)
        {
            strQuery = $@"select * from ""tblUsers"" a 
                        Where a.empcode = '{EmpCode}' AND a.schemaname = '{Domain}'";
            LocalDM.ExecuteSelectQuery(strQuery, strTableAlias);
        }
    }
}
