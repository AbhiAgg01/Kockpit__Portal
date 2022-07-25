CREATE TABLE "tblRole"
(
	Id SERIAL PRIMARY KEY,
	RoleName TEXT NOT NULL,
	CreatedOn DATE,
	ActiveStatus BOOLEAN,
	LastUpdateDate DATE
);

INSERT INTO "tblRole"(RoleName, CreatedOn, ActiveStatus, LastUpdateDate)
VALUES
 ('SUPERADMIN', current_date, true, current_date)
,('EMPLOYEE', current_date, true, current_date)
,('ADMIN', current_date, true, current_date)
,('SUBADMIN', current_date, true, current_date)
	
CREATE TABLE "tblUsers"
(
	Id SERIAL PRIMARY KEY,
	EmailId TEXT NOT NULL,
	Password TEXT NOT NULL,
	CompanyName TEXT NOT NULL,
	SchemaName TEXT NOT NULL,
	ExpiredOn DATE,
	Logo TEXT,
	Role TEXT NOT NULL,
	IsSuperAdmin BOOLEAN,
	SocketId TEXT,
	EmpCode Text,
	ContactNo1 TEXT,
	ContactNO2 TEXt,
	CreatedBy INT NOT NULL,
	CreatedOn DATE,
	DeviceId TEXT,
	ActiveStatus BOOLEAN,
	LastUpdateDate DATE
);

INSERT INTO "tblUsers"(emailid, password, companyname, schemaname, expiredon, logo, role, issuperadmin, socketid, createdby, createdon, activestatus, lastupdatedate)
VALUES ('admin@kockpit.in', 'admin', 'Kockpit', 'public', NULL, '~/img/logofull.svg', 'SUPERADMIN', true, NULL, 0, current_date, true, current_date);

CREATE TABLE "tblProject"
(
	Id SERIAL PRIMARY KEY,
	ProjectName TEXT NOT NULL,
	Description TEXT,
	Version TEXT NOT NULL,
	ProjectType TEXT NOT NULL,
	ProjectImage TEXT,
	ProjectVideo TEXT,
	ProjectStartUpLink TEXT,
	IsPro BOOLEAN,
	IsChat BOOLEAN,
	CreatedOn DATE,
	ActiveStatus BOOLEAN,
	LastUpdateDate DATE
);

CREATE TABLE "tblModule"
(
	Id SERIAL PRIMARY KEY,
	ProjectId INT NOT NULL,
	ModuleName TEXT NOT NULL,
	Description TEXT,
	IsDefault BOOLEAN,
	CreatedOn DATE,
	ActiveStatus BOOLEAN,
	LastUpdateDate DATE
);

CREATE TABLE "tblFormCategory"
(
	Id SERIAL PRIMARY KEY,
	ProjectId INT NOT NULL,
	CategoryName TEXT NOT NULL,
	CreatedOn DATE,
	ActiveStatus BOOLEAN,
	LastUpdateDate DATE
);

CREATE TABLE "tblFormMaster"
(
	Id SERIAL PRIMARY KEY,
	ProjectId INT NOT NULL,
	CategoryId INT NOT NULL,
	FormName TEXT NOT NULL,
	Description TEXT,
	ActionName TEXT NOT NULL,
	ControllerName TEXT NOT NULL,
	PageCode TEXT NOT NULL,
	LinkIcon TEXT NOT NULL,
	CreatedOn DATE,
	ActiveStatus BOOLEAN,
	LastUpdateDate DATE
);

CREATE TABLE "tblFormDetail"
(
	Id SERIAL PRIMARY KEY,
	FormId INT NOT NULL,
	ModuleId INT NOT NULL,
);

CREATE TABLE "tblUserModule"
(
	Id SERIAL PRIMARY KEY,
	UserId INT NOT NULL,
	ProjectId INT NOT NULL,
	ModuleId INT NOT NULL,
	CreatedOn DATE,
	ActiveStatus BOOLEAN,
	LastUpdateDate DATE
);

CREATE TABLE "tblEmployeeProject"
(
	Id SERIAL PRIMARY KEY,
	UserId INT NOT NULL,
	ProjectId INT NOT NULL,
	CreatedOn DATE,
	ActiveStatus BOOLEAN,
	LastUpdateDate DATE
);


CREATE TABLE "tblPlan"
(
	Id SERIAL PRIMARY KEY,
	PlanName TEXT,
	Description TEXT,
	ProjectId INT NOT NULL,
	NoOfUsers INT NOT NULL,
	ValidityDays INT NOT NULL,
	IsFree BOOLEAN,
	PlanPrice TEXT NOT NULL,
	iswarranty boolean,
	warrantyindays int,
	CreatedOn DATE,
	ActiveStatus BOOLEAN,
	LastUpdateDate DATE
)

CREATE TABLE "tblPlanRequest"
(
	Id SERIAL PRIMARY KEY,
	ClientId INT NOT NULL,
	PurchaseDate DATE NOT NULL,
	PaymentMethod TEXT,
	PaymentMode TEXT,
	PaymentTransactionNo TEXT,
	IsApproved BOOLEAN,
	Remarks TEXT,
	CreatedOn DATE,
	ActiveStatus BOOLEAN,
	LastUpdateDate DATE
)

CREATE TABLE "tblPlanRequestDetail"
(
	Id SERIAL PRIMARY KEY,
	RequestId INT NOT NULL,
	PlanId INT NOT NULL,
	NoOfUsers INT NOT NULL,
)


CREATE TABLE "tblPlanPurchase"
(
	Id SERIAL PRIMARY KEY,
	SubscriptionId TEXT NOT NULL,
	ClientId INT NOT NULL,
	PurchaseDate DATE NOT NULL,
	PaymentMethod TEXT,
	PaymentMode TEXT,
	PaymentTransactionNo TEXT,
	CreatedOn DATE,
	ActiveStatus BOOLEAN,
	LastUpdateDate DATE
)

CREATE TABLE "tblPlanPurchaseDetail"
(
	Id SERIAL PRIMARY KEY,
	SubscriptionId TEXT NOT NULL,
	PlanId INT NOT NULL,
	LicenseKey TEXT,
	ActivationDate DATE,
	ExpiryDate DATE,
	UserId INT,
	IsRenew BOOLEAN,
	IsExpired BOOLEAN,
	TransferFrom TEXT,
	DeviceId TEXT,
	IsWarranty Bool,
	WarrantyStartDate date,
	WarrantyEndDate date,
	CreatedOn DATE,
	ActiveStatus BOOLEAN,
	LastUpdateDate DATE
)


CREATE TABLE "tblPlanPurchaseHistory"
(
	Id SERIAL PRIMARY KEY,
	PlanPurchaseDetailId INT NOT NULL,
	ActivationDate DATE,
	ExpiryDate DATE,
	UserId INT
)


CREATE TABLE "tblProjectAssign"
(
	Id SERIAL PRIMARY KEY,
	ProjectId INT NOT NULL,
	EmpId INT NOT NULL,
	AssignDate DATETIME NOT NULL,
	CreatedOn DATE,
	ActiveStatus BOOLEAN,
	LastUpdateDate DATE
);

CREATE TABLE "tblTicketLevel"
(
	Id SERIAL PRIMARY KEY,
	LevelName TEXT NOT NULL,
	CreatedOn DATE,
	ActiveStatus BOOLEAN,
	LastUpdateDate DATE
);

CREATE TABLE "tblTicket"
(
	Id SERIAL PRIMARY KEY,
	UniqueId TEXT NOT NULL,
	UserId INT NOT NULL,
	ProjectId INT NOT NULL,
	IssueId INT NOT NULL,
	LevelId INT NOT NULL,
	TicketSubject TEXT NOT NULL,
	Description TEXT NOT NULL,
	AssignedTo INT,
	AssignDate DATE,
	ClosingDate DATE,
	Closedby INT ,
	TicketStatus TEXT NOT NULL,
	CusRating TEXT,
	EmpRating TEXT,
	SupportRep TEXT,
	CreatedOn DATE,
	ActiveStatus BOOLEAN,
	LastUpdateDate DATE
);

CREATE TABLE "tblTicketHistory"
(
	Id SERIAL PRIMARY KEY,
	TicketId INT NOT NULL,
	UserId INT NOT NULL,
	AssignedTo INT,
	TicketStatus TEXT NOT NULL,
	Remarks TEXT ,
	Solution TEXT,
	Attachment TEXT,
	TicketStatusDate DATE
);


CREATE TABLE "tblAMC"
(
	Id SERIAL PRIMARY KEY,
	ClientId INT,
	PlanId INT,
	SubscriptionId TEXT,
	AMCStartDate DATE, 
	AMCEndDate DATE,
	BudgetedManDays TEXT,
	ActiveStatus boolean,
	CreatedOn DATE,
	LastUpdateDate DATE
);

create table "tblTicketIssue"
(	id serial primary key,
 	issuetype TEXT,
 	createdon date,
 	activestatus boolean,
 	lastupdatedate date
							 );
insert into "tblTicketIssue"(issuetype,createdon,activestatus,lastupdatedate)
values('LOGGING ISSUE',current_date, true,current_date);

--- ALTER COMMAND

alter table "tblUsers"
add EmpCode Text,
add ContactNo1 TEXT,
add ContactNO2 TEXt,

alter table "tblPlan" 
add iswarranty boolean,
add warrantyindays int

alter table "tblPlanPurchaseDetail"
add IsWarranty Bool,
add WarrantyStartDate date,
add WarrantyEndDate date

DROP TABLE "tblTicket"
DROP TABLE "tblTicketHistory"
CREATE TABLE "tblTicket"
(
	Id SERIAL PRIMARY KEY,
	UniqueId TEXT NOT NULL,
	UserId INT NOT NULL,
	ProjectId INT NOT NULL,
	IssueId INT NOT NULL,
	LevelId INT NOT NULL,
	TicketSubject TEXT NOT NULL,
	Description TEXT NOT NULL,
	AssignedTo INT,
	AssignDate DATE,
	ClosingDate DATE,
	Closedby INT ,
	TicketStatus TEXT NOT NULL,
	CusRating TEXT,
	EmpRating TEXT,
	SupportRep TEXT,
	CreatedOn DATE,
	ActiveStatus BOOLEAN,
	LastUpdateDate DATE
);

CREATE TABLE "tblTicketHistory"
(
	Id SERIAL PRIMARY KEY,
	TicketId INT NOT NULL,
	UserId INT NOT NULL,
	AssignedTo INT,
	TicketStatus TEXT NOT NULL,
	Remarks TEXT ,
	Solution TEXT,
	Attachment TEXT,
	TicketStatusDate DATE
);

--------------------------------07 Mar
create table "tblNotification"
(
	id serial primary key,
	subject Text,
	message Text,
	Type Text,
	IsRead boolean,
	Createdon timestamp
)

alter table "tblNotification" add RefrenceId int;
ALTER TABLE "tblTicket" 
  RENAME projectid TO licenseid;

  
alter table "tblUsers" add address TEXT
  
alter table "tblTicketHistory"
	alter column ticketstatusdate type timestamp

ALTER TABLE "tblTicket" 
  ALTER COLUMN assigndate TYPE TimeStamp,
  ALTER COLUMN closingdate type timestamp,
  alter column createdon type timestamp,
  alter column lastupdatedate type timestamp

  alter table "tblTicketHistory"
add action TEXT

create table "tblCart"(
	id serial primary key,
    offeringId int,
	planId int,
    UserId int, 
	TotalLicense int,
	Price int,
	CreatedOn Date,
	LastUpdateDate Date
)

Update "tblPlan" Set IsWarranty = false

alter table "tblAMC" add iscancelled bool, add remarks text, add isexpired bool,add reissue bool;

-----
CREATE TABLE "tblMobileAppDevice"
(
	id serial primary key,
	Username Text,
	License Text,
	Device Text,
	IsAllowed boolean,
	LastUpdateDate Date
)
create table "tblFAQ"(
id SERIAL PRIMARY KEY,
frequentquestion text,
frequentanswer text,
createdon date,
lastupdatedate date
)

alter table "tblTicketIssue"
add discription text

create table "tblNotificationHistory"
(
	id serial,
	planpurchasedetailid int,
	isnotificationsend bool,
	createdon date
)


alter table "tblPlanPurchaseDetail"
add isTransfered bool

Create table "tblOfferingFAQ"(
id serial,
offeringId int,
Question text,
Ans text,
createdon date,
Activestatus bool
)add discription text


Create Table "tblOfferingCategory"
(
	Id Serial,
	OfferingCategory TEXT,
	CreatedOn DATE,
	ActiveStatus BOOLEAN,
	LastUpdateDate DATE
)

INSERT INTO "tblOfferingCategory"(OfferingCategory, CreatedOn, ActiveStatus, LastUpdateDate)
VALUES
('KONNECT', current_date, true, current_date),
('BXRAY', current_date, true, current_date),
('NUCLEUS', current_date, true, current_date)

Alter table "tblProject"
Add OfferingCategory TEXT

create table "tblService" (
	id serial,
	ServiceName text,
	Description text,
	createdon date,
	lastupdatedOn date,
	ActiveStatus bool
)