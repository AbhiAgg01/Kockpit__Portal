--
-- PostgreSQL database dump
--

-- Dumped from database version 9.6.6
-- Dumped by pg_dump version 9.6.6

-- Started on 2021-05-25 14:16:41

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 1 (class 3079 OID 12387)
-- Name: plpgsql; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS plpgsql WITH SCHEMA pg_catalog;


--
-- TOC entry 2580 (class 0 OID 0)
-- Dependencies: 1
-- Name: EXTENSION plpgsql; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION plpgsql IS 'PL/pgSQL procedural language';


SET search_path = public, pg_catalog;

SET default_tablespace = '';

SET default_with_oids = false;

--
-- TOC entry 185 (class 1259 OID 16395)
-- Name: BU Shared Percentage; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."BU Shared Percentage" (
    "PostingDate" text NOT NULL,
    "BU_Name" text NOT NULL,
    "Percentage" double precision
);


ALTER TABLE "{1}"."BU Shared Percentage" OWNER TO "{0}";

--
-- TOC entry 186 (class 1259 OID 16401)
-- Name: BU Shared Percentage INS; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."BU Shared Percentage INS" (
    "PostingDate" date NOT NULL,
    "BU_Name" text NOT NULL,
    "Percentage" double precision
);


ALTER TABLE "{1}"."BU Shared Percentage INS" OWNER TO "{0}";

--
-- TOC entry 187 (class 1259 OID 16407)
-- Name: ChartofAccounts; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."ChartofAccounts" (
    "GL Account No" text,
    "Account Description" text,
    "Income Balance" text,
    "PLReportHeader" text,
    "BSReportHeader" text,
    "CFReportHeader" text,
    "RatioComponent" text,
    "DBName" text,
    "EntityName" text,
    "Account Type" text,
    "IsNegativePolarity" boolean DEFAULT false NOT NULL,
    "UpdatedAt" timestamp
);


ALTER TABLE "{1}"."ChartofAccounts" OWNER TO "{0}";

--
-- TOC entry 277 (class 1259 OID 457851)
-- Name: ChartofAccountsBackup; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."ChartofAccountsBackup" (
    "GL Account No" text,
    "Account Description" text,
    "Income Balance" text,
    "PLReportHeader" text,
    "BSReportHeader" text,
    "CFReportHeader" text,
    "RatioComponent" text,
    "DBName" text,
    "EntityName" text,
    "Account Type" text
);


ALTER TABLE "{1}"."ChartofAccountsBackup" OWNER TO "{0}";

--
-- TOC entry 282 (class 1259 OID 1375467)
-- Name: Cluster/SUBBU Corporate Shared Percentage; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."Cluster/SUBBU Corporate Shared Percentage" (
    "PostingDate" text,
    "SUBBU_Name" text,
    "Percentage" double precision,
    "BU_Name" text,
    "SUBBU_Code" numeric
);


ALTER TABLE "{1}"."Cluster/SUBBU Corporate Shared Percentage" OWNER TO "{0}";

--
-- TOC entry 188 (class 1259 OID 16413)
-- Name: Cluster/SUBBU Shared Percentage; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."Cluster/SUBBU Shared Percentage" (
    "PostingDate" text NOT NULL,
    "SUBBU_Name" text NOT NULL,
    "Percentage" double precision,
    "BU_Name" text,
    "SUBBU_Code" numeric
);


ALTER TABLE "{1}"."Cluster/SUBBU Shared Percentage" OWNER TO "{0}";

--
-- TOC entry 189 (class 1259 OID 16419)
-- Name: Cluster/SUBBU Shared Region Percentage; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."Cluster/SUBBU Shared Region Percentage" (
    "BU" text NOT NULL,
    "OTBRANCH Code" text NOT NULL,
    "Percentage" double precision,
    "PostingDate" date NOT NULL
);


ALTER TABLE "{1}"."Cluster/SUBBU Shared Region Percentage" OWNER TO "{0}";

--
-- TOC entry 190 (class 1259 OID 16425)
-- Name: CompanyDetail; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."CompanyDetail" (
    "DBName" text,
    "EntityName" text,
    "StartDate" date,
    "EndDate" date,
    "CompanyName" text
);


ALTER TABLE "{1}"."CompanyDetail" OWNER TO "{0}";

--
-- TOC entry 279 (class 1259 OID 1001076)
-- Name: ConditionalMapping; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."ConditionalMapping" (
    "GLAccount" text NOT NULL,
    "Alternate Mapping" text NOT NULL,
    "Particulars" text NOT NULL,
    "Type" text NOT NULL,
    "Cluster" text
);


ALTER TABLE "{1}"."ConditionalMapping" OWNER TO "{0}";

--
-- TOC entry 281 (class 1259 OID 1374547)
-- Name: EUS Shared Percentage; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."EUS Shared Percentage" (
    "PostingDate" text,
    "SUBBU_Name" text,
    "Percentage" double precision,
    "BU_Name" text,
    "SUBBU_Code" numeric
);


ALTER TABLE "{1}"."EUS Shared Percentage" OWNER TO "{0}";

--
-- TOC entry 191 (class 1259 OID 16431)
-- Name: GETMONTH; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW "{1}"."GETMONTH" AS
 SELECT to_char(to_timestamp((s.m)::text, 'MM'::text), 'Month'::text) AS "MonthName",
    to_char(to_timestamp((s.m)::text, 'MM'::text), 'MM'::text) AS "MonthNumber"
   FROM generate_series(1, 12, 1) s(m);


ALTER TABLE "{1}"."GETMONTH" OWNER TO "{0}";

--
-- TOC entry 192 (class 1259 OID 16435)
-- Name: GETYEAR; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW "{1}"."GETYEAR" AS
 SELECT s.s
   FROM generate_series(((date_part('year'::text, ('now'::text)::date))::integer - 10), (date_part('year'::text, ('now'::text)::date))::integer) s(s);


ALTER TABLE "{1}"."GETYEAR" OWNER TO "{0}";

--
-- TOC entry 193 (class 1259 OID 16439)
-- Name: GETYEAR1; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW "{1}"."GETYEAR1" AS
 SELECT "Year"."Year"
   FROM generate_series(((date_part('year'::text, ('now'::text)::date))::integer - 10), (date_part('year'::text, ('now'::text)::date))::integer) "Year"("Year");


ALTER TABLE "{1}"."GETYEAR1" OWNER TO "{0}";

--
-- TOC entry 194 (class 1259 OID 16443)
-- Name: Login; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."Login" (
    id integer NOT NULL,
    "Username" text,
    "Password" text,
    "EmailID" text,
    "IsActive" boolean
);


ALTER TABLE "{1}"."Login" OWNER TO "{0}";

--
-- TOC entry 278 (class 1259 OID 797673)
-- Name: PosNegFlagChanger; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."PosNegFlagChanger" (
    "GLAccount" text NOT NULL,
    "Neg Head" text NOT NULL,
    "Pos Head" text NOT NULL,
    "Particulars" text NOT NULL
);


ALTER TABLE "{1}"."PosNegFlagChanger" OWNER TO "{0}";

--
-- TOC entry 275 (class 1259 OID 326624)
-- Name: RESalary1920; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."RESalary1920" (
    "GLAccount" integer NOT NULL,
    "GLRangeCategory" text,
    "Link_SUBBU" integer NOT NULL,
    "Amount" double precision NOT NULL,
    "BU" text,
    "LinkDate" text NOT NULL
);


ALTER TABLE "{1}"."RESalary1920" OWNER TO "{0}";

--
-- TOC entry 280 (class 1259 OID 1051174)
-- Name: RFM_SCORE; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."RFM_SCORE" (
    "R" integer NOT NULL,
    "F" integer NOT NULL,
    "M" integer NOT NULL,
    "Segment" text NOT NULL,
    "RFM" integer NOT NULL
);


ALTER TABLE "{1}"."RFM_SCORE" OWNER TO "{0}";

--
-- TOC entry 195 (class 1259 OID 16449)
-- Name: tblPLReportHeaderMapping; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblPLReportHeaderMapping" (
    "PLSequence" integer,
    "PLLevel" text,
    "PLReportHeader" text,
    "PLNote" text,
    "PLFormat" text,
    "PLGroup" text,
    "PLSign" text,
    "Version" text,
    "ValidFrom" text,
    "ValidTo" text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."tblPLReportHeaderMapping" OWNER TO "{0}";

--
-- TOC entry 195 (class 1259 OID 16449)
-- Name: "tblPostgresDbInfo"; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblPostgresDbInfo" (
   "DbName" text,
   "Host" text,
   "Port" text,
   "PostgresDB" text,
   "User" text,
   "Password" text
);

ALTER TABLE "{1}"."tblPostgresDbInfo" OWNER TO "{0}";


--
-- TOC entry 195 (class 1259 OID 16449)
-- Name: "tblGLRangeCategory"; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblGLRangeCategory" (
   "CategoryValue" text,
   "CategoryText" text
);

ALTER TABLE "{1}"."tblGLRangeCategory" OWNER TO "{0}";


--
-- TOC entry 195 (class 1259 OID 16449)
-- Name: "tblGLRangeCategory"; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblUploadFormat" (
   "Id" SERIAL PRIMARY KEY,
   "FormatName" text
);

ALTER TABLE "{1}"."tblUploadFormat" OWNER TO "{0}";


--
-- TOC entry 195 (class 1259 OID 16449)
-- Name: "tblPostgresDbInfo"; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblBackup" (
   "Id" SERIAL PRIMARY KEY,
   "Daily" boolean,
   "Weekly" boolean,
   "WeeklyDayOn" text,
   "Monthly" boolean,
   "MonthlyDay" text,
   "Time" text
);

ALTER TABLE "{1}"."tblBackup" OWNER TO "{0}";


--
-- TOC entry 196 (class 1259 OID 16455)
-- Name: SelectPL_CTE; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW "{1}"."SelectPL_CTE" AS
 WITH "CTE" AS (
         SELECT row_number() OVER (PARTITION BY "{1}"."tblPLReportHeaderMapping"."PLSequence" ORDER BY ( SELECT 1)) AS "Rn",
            "{1}"."tblPLReportHeaderMapping"."PLSequence",
            "{1}"."tblPLReportHeaderMapping"."EntityName",
            "{1}"."tblPLReportHeaderMapping"."DBName"
           FROM "{1}"."tblPLReportHeaderMapping"
          WHERE ("{1}"."tblPLReportHeaderMapping"."PLFormat" = 'F'::text)
        )
 SELECT "CTE"."Rn",
    "CTE"."PLSequence",
    "CTE"."EntityName",
    "CTE"."DBName"
   FROM "CTE"
  WHERE ("CTE"."Rn" > 1);


ALTER TABLE "{1}"."SelectPL_CTE" OWNER TO "{0}";

--
-- TOC entry 197 (class 1259 OID 16460)
-- Name: TargetCalender; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."TargetCalender" (
    "Quarters" text,
    "Months" text,
    "PeriodSort Label" text
);


ALTER TABLE "{1}"."TargetCalender" OWNER TO "{0}";

--
-- TOC entry 198 (class 1259 OID 16466)
-- Name: tempadjustmentbulkupload_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tempadjustmentbulkupload_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tempadjustmentbulkupload_id_seq OWNER TO "{0}";

--
-- TOC entry 199 (class 1259 OID 16468)
-- Name: TempAdjustmentBulkUpload; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."TempAdjustmentBulkUpload" (
    "RecordID" integer DEFAULT nextval('"{1}".tempadjustmentbulkupload_id_seq'::regclass) NOT NULL,
    "GL Account No" text,
    "Description" text,
    "PostingDate" text,
    "Amount" text,
    "DocumentNo" text,
    "Options" text,
    "EntryType" text,
    "Version" text,
    "AdjustmentType" text,
    "DBName" text,
    "EntityName" text,
    "IsActive" boolean
);


ALTER TABLE "{1}"."TempAdjustmentBulkUpload" OWNER TO "{0}";

--
-- TOC entry 200 (class 1259 OID 16475)
-- Name: TempBudgetName; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."TempBudgetName" (
    "BudgetName" text,
    "Description" text,
    "DBName" text,
    "EntityName" text,
    "UpdatedAt" timestamp
);


ALTER TABLE "{1}"."TempBudgetName" OWNER TO "{0}";

--
-- TOC entry 201 (class 1259 OID 16481)
-- Name: tempcompanynames_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tempcompanynames_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tempcompanynames_id_seq OWNER TO "{0}";

--
-- TOC entry 202 (class 1259 OID 16483)
-- Name: TempCompanyNames; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."TempCompanyNames" (
    id integer DEFAULT nextval('"{1}".tempcompanynames_id_seq'::regclass) NOT NULL,
    "Name" text NOT NULL,
    "DBName" text NOT NULL
);


ALTER TABLE "{1}"."TempCompanyNames" OWNER TO "{0}";

--
-- TOC entry 203 (class 1259 OID 16490)
-- Name: tempdimensionbudget_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tempdimensionbudget_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tempdimensionbudget_id_seq OWNER TO "{0}";

--
-- TOC entry 204 (class 1259 OID 16492)
-- Name: TempDimensionBudget; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."TempDimensionBudget" (
    "ID" integer DEFAULT nextval('"{1}".tempdimensionbudget_id_seq'::regclass) NOT NULL,
    "Budget Period" text,
    "Revenue Budget" text,
    "GM Budget" text,
    "DE Budget" text,
    "Status" text,
    "Budget Year" text,
    "Budget Name" text,
    "Output Period" text,
    "DBName" text,
    "EntityName" text,
    "Input Period" text,
    "AREA_Level2" text,
    "Sales Person" text,
    "Responsibility Centre" text,
    "Shelf No" text
);


ALTER TABLE "{1}"."TempDimensionBudget" OWNER TO "{0}";

--
-- TOC entry 205 (class 1259 OID 16499)
-- Name: TempDimensionValue; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."TempDimensionValue" (
    "Code" text,
    "Dimension Code" text,
    "Name" text,
    "Dimension Value Type" text,
    "Indentation Level" text,
    "DBName" text,
    "EntityName" text,
    "Blocked" text,
    "UpdatedAt" timestamp
);


ALTER TABLE "{1}"."TempDimensionValue" OWNER TO "{0}";

--
-- TOC entry 206 (class 1259 OID 16505)
-- Name: TempGLAccounts; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."TempGLAccounts" (
    "No_" text,
    "Name" text,
    "Search Name" text,
    "Account Type" text,
    "Indentation" text,
    "Income_Balance" text,
    "DBName" text,
    "EntityName" text,
    "UpdatedAt" timestamp
);


ALTER TABLE "{1}"."TempGLAccounts" OWNER TO "{0}";

--
-- TOC entry 276 (class 1259 OID 457836)
-- Name: TempGLAccountsBackup; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."TempGLAccountsBackup" (
    "No_" text,
    "Name" text,
    "Search Name" text,
    "Account Type" text,
    "Indentation" text,
    "Income_Balance" text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."TempGLAccountsBackup" OWNER TO "{0}";

--
-- TOC entry 207 (class 1259 OID 16511)
-- Name: TempGrossingUpParameter; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."TempGrossingUpParameter" (
    "ReportHeader" text,
    "Income Balance" text,
    "Sequence" text,
    "GL Account No" text,
    "Note" text,
    "Format" text,
    "Sign" text,
    "Posting Date" text,
    "Closing Balance" text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."TempGrossingUpParameter" OWNER TO "{0}";

--
-- TOC entry 208 (class 1259 OID 16517)
-- Name: TempItemMaster; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."TempItemMaster" (
    "No_" text,
    "Description" text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."TempItemMaster" OWNER TO "{0}";

--
-- TOC entry 209 (class 1259 OID 16523)
-- Name: TempLevelAlias; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."TempLevelAlias" (
    "Dimension Code" text,
    "Indentation Level" text,
    "Dimension Label" text,
    "Flag" text,
    "DBName" text,
    "EntityName" text,
    "UpdatedAt" timestamp
);


ALTER TABLE "{1}"."TempLevelAlias" OWNER TO "{0}";

--
-- TOC entry 210 (class 1259 OID 16529)
-- Name: TempMasterFieldName; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."TempMasterFieldName" (
    "Master" text,
    "FieldNames" text,
    "DBName" text,
    "EntityName" text,
    "UpdatedAt" timestamp
);


ALTER TABLE "{1}"."TempMasterFieldName" OWNER TO "{0}";

--
-- TOC entry 211 (class 1259 OID 16535)
-- Name: emailer_flag; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}".emailer_flag (
    no text NOT NULL,
    level integer,
    module text NOT NULL
);


ALTER TABLE "{1}".emailer_flag OWNER TO "{0}";

--
-- TOC entry 212 (class 1259 OID 16541)
-- Name: tblBSReportHeader; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblBSReportHeader" (
    "ID" integer,
    "BSLevel" text,
    "Note" text,
    "Version" text,
    "ValidFrom" text,
    "ValidTo" text,
    "EntityName" text,
    "DBName" text,
    "Preference" text,
    "BSFormat" text,
    "FormatName" text
);


ALTER TABLE "{1}"."tblBSReportHeader" OWNER TO "{0}";

--
-- TOC entry 213 (class 1259 OID 16547)
-- Name: tblBSReportHeaderMapping; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblBSReportHeaderMapping" (
    "BSSequence" integer,
    "BSLevel" text,
    "BSReportHeader" text,
    "BSNote" text,
    "BSFormat" text,
    "BSSign" text,
    "Version" text,
    "ValidFrom" text,
    "ValidTo" text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."tblBSReportHeaderMapping" OWNER TO "{0}";

--
-- TOC entry 214 (class 1259 OID 16553)
-- Name: select_bsheader; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW "{1}".select_bsheader AS
 SELECT a."ID",
        CASE
            WHEN (( SELECT "{1}"."tblBSReportHeaderMapping"."BSLevel"
               FROM "{1}"."tblBSReportHeaderMapping"
              WHERE (("{1}"."tblBSReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblBSReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblBSReportHeaderMapping"."BSSequence" = a."ID"))
              ORDER BY "{1}"."tblBSReportHeaderMapping"."BSLevel" DESC
             LIMIT 1) = ''::text) THEN a."BSLevel"
            ELSE ( SELECT "{1}"."tblBSReportHeaderMapping"."BSLevel"
               FROM "{1}"."tblBSReportHeaderMapping"
              WHERE (("{1}"."tblBSReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblBSReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblBSReportHeaderMapping"."BSSequence" = a."ID"))
              ORDER BY "{1}"."tblBSReportHeaderMapping"."BSLevel" DESC
             LIMIT 1)
        END AS "BSLevel",
        CASE
            WHEN (( SELECT "{1}"."tblBSReportHeaderMapping"."BSNote"
               FROM "{1}"."tblBSReportHeaderMapping"
              WHERE (("{1}"."tblBSReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblBSReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblBSReportHeaderMapping"."BSSequence" = a."ID"))
              ORDER BY "{1}"."tblBSReportHeaderMapping"."BSNote" DESC
             LIMIT 1) = ''::text) THEN a."Note"
            ELSE ( SELECT "{1}"."tblBSReportHeaderMapping"."BSNote"
               FROM "{1}"."tblBSReportHeaderMapping"
              WHERE (("{1}"."tblBSReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblBSReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblBSReportHeaderMapping"."BSSequence" = a."ID"))
              ORDER BY "{1}"."tblBSReportHeaderMapping"."BSNote" DESC
             LIMIT 1)
        END AS "BSNote",
        CASE
            WHEN (( SELECT "{1}"."tblBSReportHeaderMapping"."BSFormat"
               FROM "{1}"."tblBSReportHeaderMapping"
              WHERE (("{1}"."tblBSReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblBSReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblBSReportHeaderMapping"."BSSequence" = a."ID"))
              ORDER BY "{1}"."tblBSReportHeaderMapping"."BSFormat" DESC
             LIMIT 1) = ''::text) THEN a."BSFormat"
            ELSE ( SELECT "{1}"."tblBSReportHeaderMapping"."BSFormat"
               FROM "{1}"."tblBSReportHeaderMapping"
              WHERE (("{1}"."tblBSReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblBSReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblBSReportHeaderMapping"."BSSequence" = a."ID"))
              ORDER BY "{1}"."tblBSReportHeaderMapping"."BSFormat" DESC
             LIMIT 1)
        END AS "BSFormat",
        CASE
            WHEN (( SELECT "{1}"."tblBSReportHeaderMapping"."BSSign"
               FROM "{1}"."tblBSReportHeaderMapping"
              WHERE (("{1}"."tblBSReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblBSReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblBSReportHeaderMapping"."BSSequence" = a."ID"))
              ORDER BY "{1}"."tblBSReportHeaderMapping"."BSSign" DESC
             LIMIT 1) = ''::text) THEN ''::text
            ELSE ( SELECT "{1}"."tblBSReportHeaderMapping"."BSSign"
               FROM "{1}"."tblBSReportHeaderMapping"
              WHERE (("{1}"."tblBSReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblBSReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblBSReportHeaderMapping"."BSSequence" = a."ID"))
              ORDER BY "{1}"."tblBSReportHeaderMapping"."BSSign" DESC
             LIMIT 1)
        END AS "BSSign",
    a."DBName",
    a."EntityName"
   FROM "{1}"."tblBSReportHeader" a;


ALTER TABLE "{1}".select_bsheader OWNER TO "{0}";

--
-- TOC entry 215 (class 1259 OID 16558)
-- Name: tblCFReportHeader; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblCFReportHeader" (
    "ID" integer,
    "CFLevel" text,
    "Version" text,
    "ValidFrom" text,
    "ValidTo" text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."tblCFReportHeader" OWNER TO "{0}";

--
-- TOC entry 216 (class 1259 OID 16564)
-- Name: tblCFReportHeaderMapping; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblCFReportHeaderMapping" (
    "CFSequence" integer,
    "CFLevel" text,
    "CFReportHeader" text,
    "CFFormat" text,
    "CFFlag" text,
    "CFSign" text,
    "Version" text,
    "ValidFrom" text,
    "ValidTo" text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."tblCFReportHeaderMapping" OWNER TO "{0}";

--
-- TOC entry 217 (class 1259 OID 16570)
-- Name: select_cfheader; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW "{1}".select_cfheader AS
 SELECT a."ID",
        CASE
            WHEN (COALESCE(( SELECT "{1}"."tblCFReportHeaderMapping"."CFLevel"
               FROM "{1}"."tblCFReportHeaderMapping"
              WHERE (("{1}"."tblCFReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblCFReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblCFReportHeaderMapping"."CFSequence" = a."ID"))
              ORDER BY "{1}"."tblCFReportHeaderMapping"."CFLevel" DESC
             LIMIT 1), ''::text) = ''::text) THEN a."CFLevel"
            ELSE ( SELECT "{1}"."tblCFReportHeaderMapping"."CFLevel"
               FROM "{1}"."tblCFReportHeaderMapping"
              WHERE (("{1}"."tblCFReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblCFReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblCFReportHeaderMapping"."CFSequence" = a."ID"))
              ORDER BY "{1}"."tblCFReportHeaderMapping"."CFLevel" DESC
             LIMIT 1)
        END AS "CFLevel",
        CASE
            WHEN (COALESCE(( SELECT "{1}"."tblCFReportHeaderMapping"."CFFormat"
               FROM "{1}"."tblCFReportHeaderMapping"
              WHERE (("{1}"."tblCFReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblCFReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblCFReportHeaderMapping"."CFSequence" = a."ID"))
              ORDER BY "{1}"."tblCFReportHeaderMapping"."CFFormat" DESC
             LIMIT 1), ''::text) = ''::text) THEN ''::text
            ELSE ( SELECT "{1}"."tblCFReportHeaderMapping"."CFFormat"
               FROM "{1}"."tblCFReportHeaderMapping"
              WHERE (("{1}"."tblCFReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblCFReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblCFReportHeaderMapping"."CFSequence" = a."ID"))
              ORDER BY "{1}"."tblCFReportHeaderMapping"."CFFormat" DESC
             LIMIT 1)
        END AS "CFFormat",
        CASE
            WHEN (COALESCE(( SELECT "{1}"."tblCFReportHeaderMapping"."CFSign"
               FROM "{1}"."tblCFReportHeaderMapping"
              WHERE (("{1}"."tblCFReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblCFReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblCFReportHeaderMapping"."CFSequence" = a."ID"))
              ORDER BY "{1}"."tblCFReportHeaderMapping"."CFSign" DESC
             LIMIT 1), ''::text) = ''::text) THEN ''::text
            ELSE ( SELECT "{1}"."tblCFReportHeaderMapping"."CFSign"
               FROM "{1}"."tblCFReportHeaderMapping"
              WHERE (("{1}"."tblCFReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblCFReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblCFReportHeaderMapping"."CFSequence" = a."ID"))
              ORDER BY "{1}"."tblCFReportHeaderMapping"."CFSign" DESC
             LIMIT 1)
        END AS "CFSign",
        CASE
            WHEN (COALESCE(( SELECT "{1}"."tblCFReportHeaderMapping"."CFFlag"
               FROM "{1}"."tblCFReportHeaderMapping"
              WHERE (("{1}"."tblCFReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblCFReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblCFReportHeaderMapping"."CFSequence" = a."ID"))
              ORDER BY "{1}"."tblCFReportHeaderMapping"."CFFlag" DESC
             LIMIT 1), ''::text) = ''::text) THEN ''::text
            ELSE ( SELECT "{1}"."tblCFReportHeaderMapping"."CFFlag"
               FROM "{1}"."tblCFReportHeaderMapping"
              WHERE (("{1}"."tblCFReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblCFReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblCFReportHeaderMapping"."CFSequence" = a."ID"))
              ORDER BY "{1}"."tblCFReportHeaderMapping"."CFFlag" DESC
             LIMIT 1)
        END AS "CFFlag",
    a."DBName",
    a."EntityName"
   FROM "{1}"."tblCFReportHeader" a
  ORDER BY a."ID";


ALTER TABLE "{1}".select_cfheader OWNER TO "{0}";

--
-- TOC entry 218 (class 1259 OID 16575)
-- Name: tblPLReportHeader; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblPLReportHeader" (
    "ID" integer,
    "PLLevel" text,
    "Note" text,
    "Version" text,
    "ValidFrom" text,
    "ValidTo" text,
    "DBName" text,
    "EntityName" text,
    "Preference" text,
    "PLFormat" text,
    "PLGroup" text,
    "FormatName" text
);


ALTER TABLE "{1}"."tblPLReportHeader" OWNER TO "{0}";

--
-- TOC entry 219 (class 1259 OID 16581)
-- Name: select_plheader; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW "{1}".select_plheader AS
 SELECT a."ID",
        CASE
            WHEN (( SELECT "{1}"."tblPLReportHeaderMapping"."PLLevel"
               FROM "{1}"."tblPLReportHeaderMapping"
              WHERE (("{1}"."tblPLReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblPLReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblPLReportHeaderMapping"."PLSequence" = a."ID"))
              ORDER BY "{1}"."tblPLReportHeaderMapping"."PLLevel" DESC
             LIMIT 1) = ''::text) THEN a."PLLevel"
            ELSE ( SELECT "{1}"."tblPLReportHeaderMapping"."PLLevel"
               FROM "{1}"."tblPLReportHeaderMapping"
              WHERE (("{1}"."tblPLReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblPLReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblPLReportHeaderMapping"."PLSequence" = a."ID"))
              ORDER BY "{1}"."tblPLReportHeaderMapping"."PLLevel" DESC
             LIMIT 1)
        END AS "PLLevel",
        CASE
            WHEN (( SELECT "{1}"."tblPLReportHeaderMapping"."PLNote"
               FROM "{1}"."tblPLReportHeaderMapping"
              WHERE (("{1}"."tblPLReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblPLReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblPLReportHeaderMapping"."PLSequence" = a."ID"))
              ORDER BY "{1}"."tblPLReportHeaderMapping"."PLNote" DESC
             LIMIT 1) = ''::text) THEN a."Note"
            ELSE ( SELECT "{1}"."tblPLReportHeaderMapping"."PLNote"
               FROM "{1}"."tblPLReportHeaderMapping"
              WHERE (("{1}"."tblPLReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblPLReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblPLReportHeaderMapping"."PLSequence" = a."ID"))
              ORDER BY "{1}"."tblPLReportHeaderMapping"."PLNote" DESC
             LIMIT 1)
        END AS "PLNote",
        CASE
            WHEN (( SELECT "{1}"."tblPLReportHeaderMapping"."PLFormat"
               FROM "{1}"."tblPLReportHeaderMapping"
              WHERE (("{1}"."tblPLReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblPLReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblPLReportHeaderMapping"."PLSequence" = a."ID"))
              ORDER BY "{1}"."tblPLReportHeaderMapping"."PLFormat" DESC
             LIMIT 1) = ''::text) THEN a."PLFormat"
            ELSE ( SELECT "{1}"."tblPLReportHeaderMapping"."PLFormat"
               FROM "{1}"."tblPLReportHeaderMapping"
              WHERE (("{1}"."tblPLReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblPLReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblPLReportHeaderMapping"."PLSequence" = a."ID"))
              ORDER BY "{1}"."tblPLReportHeaderMapping"."PLFormat" DESC
             LIMIT 1)
        END AS "PLFormat",
        CASE
            WHEN (( SELECT "{1}"."tblPLReportHeaderMapping"."PLGroup"
               FROM "{1}"."tblPLReportHeaderMapping"
              WHERE (("{1}"."tblPLReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblPLReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblPLReportHeaderMapping"."PLSequence" = a."ID"))
              ORDER BY "{1}"."tblPLReportHeaderMapping"."PLGroup" DESC
             LIMIT 1) = ''::text) THEN a."PLGroup"
            ELSE ( SELECT "{1}"."tblPLReportHeaderMapping"."PLGroup"
               FROM "{1}"."tblPLReportHeaderMapping"
              WHERE (("{1}"."tblPLReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblPLReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblPLReportHeaderMapping"."PLSequence" = a."ID"))
              ORDER BY "{1}"."tblPLReportHeaderMapping"."PLGroup" DESC
             LIMIT 1)
        END AS "PLGroup",
        CASE
            WHEN (( SELECT "{1}"."tblPLReportHeaderMapping"."PLSign"
               FROM "{1}"."tblPLReportHeaderMapping"
              WHERE (("{1}"."tblPLReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblPLReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblPLReportHeaderMapping"."PLSequence" = a."ID"))
              ORDER BY "{1}"."tblPLReportHeaderMapping"."PLSign" DESC
             LIMIT 1) = ''::text) THEN ''::text
            ELSE ( SELECT "{1}"."tblPLReportHeaderMapping"."PLSign"
               FROM "{1}"."tblPLReportHeaderMapping"
              WHERE (("{1}"."tblPLReportHeaderMapping"."DBName" = a."DBName") 
			  AND ("{1}"."tblPLReportHeaderMapping"."EntityName" = a."EntityName") 
			  AND ("{1}"."tblPLReportHeaderMapping"."PLSequence" = a."ID"))
              ORDER BY "{1}"."tblPLReportHeaderMapping"."PLSign" DESC
             LIMIT 1)
        END AS "PLSign",
    a."DBName",
    a."EntityName"
   FROM "{1}"."tblPLReportHeader" a;


ALTER TABLE "{1}".select_plheader OWNER TO "{0}";

--
-- TOC entry 220 (class 1259 OID 16586)
-- Name: tblbudgetmapping_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tblbudgetmapping_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tblbudgetmapping_id_seq OWNER TO "{0}";

--
-- TOC entry 221 (class 1259 OID 16588)
-- Name: tblAPBucket; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblAPBucket" (
    "ID" integer DEFAULT nextval('"{1}".tblbudgetmapping_id_seq'::regclass) NOT NULL,
    "Lower Limit" text,
    "Upper Limit" text,
    "Bucket Name" text,
    "Bucket Sort" text,
    "Nod" text,
    "AP Lower Limit" text,
    "AP Upper Limit" text,
    "AP Ticket Name" text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."tblAPBucket" OWNER TO "{0}";

--
-- TOC entry 222 (class 1259 OID 16595)
-- Name: tblarbucket_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tblarbucket_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tblarbucket_id_seq OWNER TO "{0}";

--
-- TOC entry 223 (class 1259 OID 16597)
-- Name: tblARBucket; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblARBucket" (
    "ID" integer DEFAULT nextval('"{1}".tblarbucket_id_seq'::regclass) NOT NULL,
    "Lower Limit" text,
    "Upper Limit" text,
    "Bucket Name" text,
    "Bucket Sort" text,
    "Nod" text,
    "AR Lower Limit" text,
    "AR Upper Limit" text,
    "AR Ticket Name" text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."tblARBucket" OWNER TO "{0}";

--
-- TOC entry 224 (class 1259 OID 16604)
-- Name: tblaccessdetails_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tblaccessdetails_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tblaccessdetails_id_seq OWNER TO "{0}";

--
-- TOC entry 225 (class 1259 OID 16606)
-- Name: tblAccessDetails; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblAccessDetails" (
    id integer DEFAULT nextval('"{1}".tblaccessdetails_id_seq'::regclass) NOT NULL,
    serverip text,
    userid text,
    pwd text,
    databasename text,
    newdbname text,
    dbid text,
    navisionversion text,
    "Installation Drive" text,
    servertype text,
    linuxserver text,
    linuxuserId text,
    linuxpassword text
);


ALTER TABLE "{1}"."tblAccessDetails" OWNER TO "{0}";

--
-- TOC entry 226 (class 1259 OID 16613)
-- Name: tbladjustmententries_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tbladjustmententries_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tbladjustmententries_id_seq OWNER TO "{0}";

--
-- TOC entry 227 (class 1259 OID 16615)
-- Name: tblAdjustmentEntries; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblAdjustmentEntries" (
    "RecordID" integer DEFAULT nextval('"{1}".tbladjustmententries_id_seq'::regclass) NOT NULL,
    "GL Account No" text,
    "Description" text,
    "PostingDate" text,
    "Amount" text,
    "DocumentNo" text,
    "Options" text,
    "EntryType" text,
    "Version" text,
    "DBName" text,
    "EntityName" text,
    "IsActive" boolean,
    "AdjustmentType" text
);


ALTER TABLE "{1}"."tblAdjustmentEntries" OWNER TO "{0}";

--
-- TOC entry 228 (class 1259 OID 16622)
-- Name: tblanalysisgroup_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tblanalysisgroup_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tblanalysisgroup_id_seq OWNER TO "{0}";

--
-- TOC entry 229 (class 1259 OID 16624)
-- Name: tblAnalysisGroup; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblAnalysisGroup" (
    "ID" integer DEFAULT nextval('"{1}".tblanalysisgroup_id_seq'::regclass) NOT NULL,
    "AnalysisGroup" text
);


ALTER TABLE "{1}"."tblAnalysisGroup" OWNER TO "{0}";

--
-- TOC entry 230 (class 1259 OID 16631)
-- Name: tblanalysisgroupmapping_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tblanalysisgroupmapping_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tblanalysisgroupmapping_id_seq OWNER TO "{0}";

--
-- TOC entry 231 (class 1259 OID 16633)
-- Name: tblAnalysisGroupMapping; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblAnalysisGroupMapping" (
    "ID" integer DEFAULT nextval('"{1}".tblanalysisgroupmapping_id_seq'::regclass) NOT NULL,
    "Analysis Group" text,
    "Sequence" text,
    "Column Label" text,
    "AR Dimension" text,
    "Trend Dimension" text,
    "Order Dimension" text,
    "AP Dimension" text,
    "Purchase Dimension" text,
    "GroupID" text
);


ALTER TABLE "{1}"."tblAnalysisGroupMapping" OWNER TO "{0}";

--
-- TOC entry 232 (class 1259 OID 16640)
-- Name: tblbudgetdimension_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tblbudgetdimension_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tblbudgetdimension_id_seq OWNER TO "{0}";

--
-- TOC entry 233 (class 1259 OID 16642)
-- Name: tblBudgetDimension; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblBudgetDimension" (
    "ID" integer DEFAULT nextval('"{1}".tblbudgetdimension_id_seq'::regclass) NOT NULL,
    "Dimension Code" text,
    "Level Name" text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."tblBudgetDimension" OWNER TO "{0}";

--
-- TOC entry 234 (class 1259 OID 16649)
-- Name: tblbudgetfieldmapping_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tblbudgetfieldmapping_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tblbudgetfieldmapping_id_seq OWNER TO "{0}";

--
-- TOC entry 235 (class 1259 OID 16651)
-- Name: tblBudgetFieldMapping; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblBudgetFieldMapping" (
    "ID" integer DEFAULT nextval('"{1}".tblbudgetfieldmapping_id_seq'::regclass) NOT NULL,
    "Dimension Name" text,
    "Group Name" text,
    "Mapped From Values" text,
    "Mapped To Values" text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."tblBudgetFieldMapping" OWNER TO "{0}";

--
-- TOC entry 236 (class 1259 OID 16658)
-- Name: tblbudgetfieldnames_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tblbudgetfieldnames_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tblbudgetfieldnames_id_seq OWNER TO "{0}";

--
-- TOC entry 237 (class 1259 OID 16660)
-- Name: tblBudgetFieldNames; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblBudgetFieldNames" (
    "ID" integer DEFAULT nextval('"{1}".tblbudgetfieldnames_id_seq'::regclass) NOT NULL,
    "Table Name" text,
    "Column Name" text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."tblBudgetFieldNames" OWNER TO "{0}";

--
-- TOC entry 238 (class 1259 OID 16667)
-- Name: tblBudgetMapping; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblBudgetMapping" (
    "ID" integer DEFAULT nextval('"{1}".tblbudgetmapping_id_seq'::regclass) NOT NULL,
    "BudgetName" text,
    "Description" text,
    "IsActive" boolean,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."tblBudgetMapping" OWNER TO "{0}";

--
-- TOC entry 287 (class 1259 OID 1475312)
-- Name: tblCFReportHeaderMapping_0103; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblCFReportHeaderMapping_0103" (
    "CFSequence" integer,
    "CFLevel" text,
    "CFReportHeader" text,
    "CFFormat" text,
    "CFFlag" text,
    "CFSign" text,
    "Version" text,
    "ValidFrom" text,
    "ValidTo" text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."tblCFReportHeaderMapping_0103" OWNER TO "{0}";

--
-- TOC entry 285 (class 1259 OID 1455173)
-- Name: tblCFReportHeaderMapping_22022021; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblCFReportHeaderMapping_22022021" (
    "CFSequence" integer,
    "CFLevel" text,
    "CFReportHeader" text,
    "CFFormat" text,
    "CFFlag" text,
    "CFSign" text,
    "Version" text,
    "ValidFrom" text,
    "ValidTo" text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."tblCFReportHeaderMapping_22022021" OWNER TO "{0}";

--
-- TOC entry 284 (class 1259 OID 1377163)
-- Name: tblCFReportHeaderMapping_Backup; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblCFReportHeaderMapping_Backup" (
    "CFSequence" integer,
    "CFLevel" text,
    "CFReportHeader" text,
    "CFFormat" text,
    "CFFlag" text,
    "CFSign" text,
    "Version" text,
    "ValidFrom" text,
    "ValidTo" text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."tblCFReportHeaderMapping_Backup" OWNER TO "{0}";

--
-- TOC entry 286 (class 1259 OID 1475306)
-- Name: tblCFReportHeader_0103; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblCFReportHeader_0103" (
    "ID" integer,
    "CFLevel" text,
    "Version" text,
    "ValidFrom" text,
    "ValidTo" text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."tblCFReportHeader_0103" OWNER TO "{0}";

--
-- TOC entry 283 (class 1259 OID 1377154)
-- Name: tblCFReportHeader_backup; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblCFReportHeader_backup" (
    "ID" integer,
    "CFLevel" text,
    "Version" text,
    "ValidFrom" text,
    "ValidTo" text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."tblCFReportHeader_backup" OWNER TO "{0}";

--
-- TOC entry 239 (class 1259 OID 16674)
-- Name: tblcompanyname_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tblcompanyname_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tblcompanyname_id_seq OWNER TO "{0}";

--
-- TOC entry 240 (class 1259 OID 16676)
-- Name: tblCompanyName; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblCompanyName" (
    "RecordID" integer DEFAULT nextval('"{1}".tblcompanyname_id_seq'::regclass) NOT NULL,
    "CompanyName" text,
    "NewCompanyName" text,
    "Scomp" text,
    "StartDate" text,
    "EndDate" text,
    "Rtilldate" text,
    "DBName" text,
    "ActiveCompany" boolean
);


ALTER TABLE "{1}"."tblCompanyName" OWNER TO "{0}";

--
-- TOC entry 241 (class 1259 OID 16683)
-- Name: tbldefaultratioflag_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tbldefaultratioflag_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tbldefaultratioflag_id_seq OWNER TO "{0}";

--
-- TOC entry 242 (class 1259 OID 16685)
-- Name: tblDefaultRatioFlag; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblDefaultRatioFlag" (
    "ID" integer DEFAULT nextval('"{1}".tbldefaultratioflag_id_seq'::regclass) NOT NULL,
    "RatioFlag" text,
    "Income_Balance" text
);


ALTER TABLE "{1}"."tblDefaultRatioFlag" OWNER TO "{0}";

--
-- TOC entry 243 (class 1259 OID 16692)
-- Name: tbldimensioncode_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tbldimensioncode_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tbldimensioncode_id_seq OWNER TO "{0}";

--
-- TOC entry 244 (class 1259 OID 16694)
-- Name: tblDimensionCode; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblDimensionCode" (
    "ID" integer DEFAULT nextval('"{1}".tbldimensioncode_id_seq'::regclass) NOT NULL,
    "Name" text,
    "DBName" text,
    "EntityName" text,
    "IsActive" boolean,
    "IsActiveAdjustment" boolean,
    "IsActiveExpense" boolean,
    "IsActiveBudget" boolean
);


ALTER TABLE "{1}"."tblDimensionCode" OWNER TO "{0}";

--
-- TOC entry 245 (class 1259 OID 16701)
-- Name: tblDummyAccounts; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblDummyAccounts" (
    "GL Account No" text,
    "Income Balance" text,
    "Account Description" text,
    "GLAccountType" text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."tblDummyAccounts" OWNER TO "{0}";

--
-- TOC entry 246 (class 1259 OID 16707)
-- Name: tblfieldselection_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tblfieldselection_id_seq
    START WITH 6
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tblfieldselection_id_seq OWNER TO "{0}";

--
-- TOC entry 247 (class 1259 OID 16709)
-- Name: tblFieldSelection; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblFieldSelection" (
    "ID" integer DEFAULT nextval('"{1}".tblfieldselection_id_seq'::regclass) NOT NULL,
    "FieldType" text,
    "TableName" text,
    "Flag" text
);


ALTER TABLE "{1}"."tblFieldSelection" OWNER TO "{0}";

--
-- TOC entry 248 (class 1259 OID 16716)
-- Name: tblglaccountmapping_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tblglaccountmapping_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tblglaccountmapping_id_seq OWNER TO "{0}";

--
-- TOC entry 249 (class 1259 OID 16718)
-- Name: tblGLAccountMapping; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblGLAccountMapping" (
    "ID" integer DEFAULT nextval('"{1}".tblglaccountmapping_id_seq'::regclass) NOT NULL,
    "GL Range Category" text,
    "From GL Code" text,
    "To GL Code" text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."tblGLAccountMapping" OWNER TO "{0}";

--
-- TOC entry 250 (class 1259 OID 16725)
-- Name: tblglcodeexclusion_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tblglcodeexclusion_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tblglcodeexclusion_id_seq OWNER TO "{0}";

--
-- TOC entry 251 (class 1259 OID 16727)
-- Name: tblGLCodeExclusion; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblGLCodeExclusion" (
    "ID" integer DEFAULT nextval('"{1}".tblglcodeexclusion_id_seq'::regclass) NOT NULL,
    "GLCode" text,
    "IsExcluded" boolean,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."tblGLCodeExclusion" OWNER TO "{0}";

--
-- TOC entry 252 (class 1259 OID 16734)
-- Name: tblgrossingupparameter_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tblgrossingupparameter_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tblgrossingupparameter_id_seq OWNER TO "{0}";

--
-- TOC entry 253 (class 1259 OID 16736)
-- Name: tblGrossingUpParameter; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblGrossingUpParameter" (
    "ID" integer DEFAULT nextval('"{1}".tblgrossingupparameter_id_seq'::regclass) NOT NULL,
    "ReportHeader" text,
    "Income Balance" text,
    "Sequence" text,
    "GL Account No" text,
    "Note" text,
    "Format" text,
    "Sign" text,
    "Posting Date" text,
    "PostingYear" text,
    "PostingMonth" text,
    "Adjustment" text,
    "Revised" text,
    "Closing Balance" text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."tblGrossingUpParameter" OWNER TO "{0}";

--
-- TOC entry 254 (class 1259 OID 16743)
-- Name: tblinventorybucket_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tblinventorybucket_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tblinventorybucket_id_seq OWNER TO "{0}";

--
-- TOC entry 255 (class 1259 OID 16745)
-- Name: tblInventoryBucket; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblInventoryBucket" (
    "ID" integer DEFAULT nextval('"{1}".tblinventorybucket_id_seq'::regclass) NOT NULL,
    "Lower Limit" text,
    "Upper Limit" text,
    "Bucket Name" text,
    "Bucket Sort" text,
    "Nod" text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."tblInventoryBucket" OWNER TO "{0}";

--
-- TOC entry 256 (class 1259 OID 16752)
-- Name: tblitemlistexclusion_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tblitemlistexclusion_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tblitemlistexclusion_id_seq OWNER TO "{0}";

--
-- TOC entry 257 (class 1259 OID 16754)
-- Name: tblItemListExclusion; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblItemListExclusion" (
    "ID" integer DEFAULT nextval('"{1}".tblitemlistexclusion_id_seq'::regclass) NOT NULL,
    "ItemCode" text,
    "IsExcluded" boolean,
    flag text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."tblItemListExclusion" OWNER TO "{0}";

--
-- TOC entry 258 (class 1259 OID 16761)
-- Name: tbllevelalias_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tbllevelalias_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tbllevelalias_id_seq OWNER TO "{0}";

--
-- TOC entry 259 (class 1259 OID 16763)
-- Name: tblLevelAlias; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblLevelAlias" (
    "ID" integer DEFAULT nextval('"{1}".tbllevelalias_id_seq'::regclass) NOT NULL,
    "Dimension Code" text,
    "Indentation Level" text,
    "Dimension Label" text,
    "DBName" text,
    "EntityName" text,
    "Flag" text
);


ALTER TABLE "{1}"."tblLevelAlias" OWNER TO "{0}";

--
-- TOC entry 260 (class 1259 OID 16770)
-- Name: tblmasterfielddefault_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tblmasterfielddefault_id_seq
    START WITH 6
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tblmasterfielddefault_id_seq OWNER TO "{0}";

--
-- TOC entry 261 (class 1259 OID 16772)
-- Name: tblMasterFieldDefault; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblMasterFieldDefault" (
    "ID" integer DEFAULT nextval('"{1}".tblmasterfielddefault_id_seq'::regclass) NOT NULL,
    "TableName" text,
    "FieldName" text,
    "ColumnLabel" text
);


ALTER TABLE "{1}"."tblMasterFieldDefault" OWNER TO "{0}";

--
-- TOC entry 262 (class 1259 OID 16779)
-- Name: tblmasterfielddefaultalias_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tblmasterfielddefaultalias_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tblmasterfielddefaultalias_id_seq OWNER TO "{0}";

--
-- TOC entry 263 (class 1259 OID 16781)
-- Name: tblMasterFieldDefaultAlias; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblMasterFieldDefaultAlias" (
    "ID" integer DEFAULT nextval('"{1}".tblmasterfielddefaultalias_id_seq'::regclass) NOT NULL,
    "TableName" text,
    "FieldName" text,
    "ColumnLabel" text
);


ALTER TABLE "{1}"."tblMasterFieldDefaultAlias" OWNER TO "{0}";

--
-- TOC entry 264 (class 1259 OID 16788)
-- Name: tblnavmasterfieldaddition_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tblnavmasterfieldaddition_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tblnavmasterfieldaddition_id_seq OWNER TO "{0}";

--
-- TOC entry 265 (class 1259 OID 16790)
-- Name: tblNavMasterFieldAddition; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblNavMasterFieldAddition" (
    "ID" integer DEFAULT nextval('"{1}".tblnavmasterfieldaddition_id_seq'::regclass) NOT NULL,
    "Nav Table" text,
    "Nav Column" text,
    "Column Label" text,
    "Options Field" text,
    "Options String" text,
    "Fieldname" text,
    "Default Dimension" text,
    "Dimension Code" text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."tblNavMasterFieldAddition" OWNER TO "{0}";

--
-- TOC entry 266 (class 1259 OID 16797)
-- Name: tblpddbucket_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tblpddbucket_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tblpddbucket_id_seq OWNER TO "{0}";

--
-- TOC entry 267 (class 1259 OID 16799)
-- Name: tblPDDBucket; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblPDDBucket" (
    "ID" integer DEFAULT nextval('"{1}".tblpddbucket_id_seq'::regclass) NOT NULL,
    "Min Limit" text,
    "Max Limit" text,
    "Bucket Name" text,
    "Bucket Sort" text,
    "Nod" text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."tblPDDBucket" OWNER TO "{0}";

--
-- TOC entry 268 (class 1259 OID 16806)
-- Name: tblratioformula_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tblratioformula_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tblratioformula_id_seq OWNER TO "{0}";

--
-- TOC entry 269 (class 1259 OID 16808)
-- Name: tblRatioFormula; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblRatioFormula" (
    "ID" integer DEFAULT nextval('"{1}".tblratioformula_id_seq'::regclass) NOT NULL,
    "RatioName" text,
    "Denominator" text,
    "Numerator" text
);


ALTER TABLE "{1}"."tblRatioFormula" OWNER TO "{0}";

--
-- TOC entry 270 (class 1259 OID 16815)
-- Name: tblratioheadermapping_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tblratioheadermapping_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tblratioheadermapping_id_seq OWNER TO "{0}";

--
-- TOC entry 271 (class 1259 OID 16817)
-- Name: tblRatioHeaderMapping; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblRatioHeaderMapping" (
    "ID" integer DEFAULT nextval('"{1}".tblratioheadermapping_id_seq'::regclass) NOT NULL,
    "RatioFlag" text,
    "Income_Balance" text,
    "Component" text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."tblRatioHeaderMapping" OWNER TO "{0}";

--
-- TOC entry 272 (class 1259 OID 16824)
-- Name: tblratiovalues_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tblratiovalues_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tblratiovalues_id_seq OWNER TO "{0}";

--
-- TOC entry 273 (class 1259 OID 16826)
-- Name: tblRatioValues; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE "{1}"."tblRatioValues" (
    "ID" integer DEFAULT nextval('"{1}".tblratiovalues_id_seq'::regclass) NOT NULL,
    "Frequency" text,
    "MonthName" text,
    "RatioValue" text,
    "FY" text,
    "MonthID" text,
    "YearID" text,
    "RatioName" text,
    "CreationDate" text,
    "DBName" text,
    "EntityName" text
);


ALTER TABLE "{1}"."tblRatioValues" OWNER TO "{0}";

--
-- TOC entry 274 (class 1259 OID 16833)
-- Name: tblapbucket_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE "{1}".tblapbucket_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "{1}".tblapbucket_id_seq OWNER TO "{0}";

-------INSERT ENTERIES START-------


--
-- Data for Name: TargetCalender; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO "{1}"."TargetCalender" ("Quarters", "Months", "PeriodSort Label") VALUES
('Q1','Apr','1'),
('Q1','May','2'),
('Q1','Jun','3'),
('Q2','Jul','4'),
('Q2','Aug','5'),
('Q2','Sep','6'),
('Q3','Oct','7'),
('Q3','Nov','8'),
('Q3','Dec','9'),
('Q4','Jan','10'),
('Q4','Feb','11'),
('Q4','Mar','12');


--
-- Data for Name: TempGLAccounts; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO "{1}"."TempGLAccounts" ("No_", "Name", "Search Name", "Account Type", "Indentation", "Income_Balance", "DBName", "EntityName") VALUES
('10101','Test','Test','0','0','Balance Sheet','DB1','E1');


--
-- Data for Name: tblAPBucket; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO "{1}"."tblAPBucket" ("ID", "Lower Limit", "Upper Limit", "Bucket Name", "Bucket Sort", "Nod", "AP Lower Limit", "AP Upper Limit", "AP Ticket Name", "DBName", "EntityName") VALUES
('1','-999999','-1','Not Due','1','','0','10','0-10','DB1','E1'),
('3','31','60','31-60','3','','101','1000','101-1000','DB1','E1'),
('5','91','180','91-180','5','','','','','DB1','E1'),
('6','181','999999','180+','6','','','','','DB1','E1'),
('2','0','30','0-30','2','','11','100','11-100','DB1','E1'),
('4','61','90','61-90','4','','1001','10000','>1000','DB1','E1');



--
-- Data for Name: tblARBucket; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO "{1}"."tblARBucket" ("ID", "Lower Limit", "Upper Limit", "Bucket Name", "Bucket Sort", "Nod", "AR Lower Limit", "AR Upper Limit", "AR Ticket Name", "DBName", "EntityName") VALUES
('1','-999999','-1','Not Due','1','','0','10','0-10','DB1','E1'),
('2','0','30','0-30','2','','11','100','11-100','DB1','E1'),
('3','31','60','31-60','3','','101','1000','101-1000','DB1','E1'),
('4','61','90','61-90','4','','1001','10000','>1000','DB1','E1'),
('5','91','180','91-180','5','','','','','DB1','E1'),
('6','181','999999','180+','6','','','','','DB1','E1');



--
-- Data for Name: tblAnalysisGroup; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO "{1}"."tblAnalysisGroup" ("ID", "AnalysisGroup") VALUES
('1','Business Unit'),
('2','Customer'),
('3','Geography'),
('4','Item'),
('5','Salesperson'),
('6','Vendor'),
('7','Others'),
('8','Location');


--
-- Data for Name: tblDefaultRatioFlag; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO "{1}"."tblDefaultRatioFlag" ("ID", "RatioFlag", "Income_Balance") VALUES
('1','Total Revenue','Profit & Loss'),
('2','EBITA','Profit & Loss'),
('3','PAT','Profit & Loss'),
('3','Total Shareholders Fund','Balance Sheet'),
('5','Current Assets','Balance Sheet'),
('6','Current Liabilities','Balance Sheet'),
('7','Debt','Balance Sheet'),
('8','Equity','Balance Sheet'),
('9','Inventories','Balance Sheet'),
('10','Total Assets','Balance Sheet'),
('11','Total Liabilites','Balance Sheet');


--
-- Data for Name: tblFieldSelection; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO "{1}"."tblFieldSelection" ("ID", "FieldType", "TableName", "Flag") VALUES
('1','Item Ledger Entry Quantity ','Value Inventory','0'),
('2','Invoiced Quantity','Value Inventory','1'),
('3','Cost Posted to G_L','VE FieldSelection','1'),
('4','Cost Amount (Actual)','VE FieldSelection','0'),
('5','Cost Amount (Expected)','VE FieldSelection','0'),
('6','Sales Invoice Line','Sales Transaction','1'),
('7','Customer','Sales Transaction','0'),
('8','Posting Date','Order Date','1'),
('9','New Implementation','whats new','1'),
('10','Version Upgrade','whats new','0'),
('11','Order Date','Order Date','0'),
('12','Section Access','Section Access','0'),
('13','Promised Receipt Date','Purchase Order','1'),
('14','Requested Receipt Date','Purchase Order','0'),
('15','Promised Delivery Date','Sales Order','1'),
('16','Requested Delivery Date','Sales Order','0');



--
-- Data for Name: tblInventoryBucket; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO "{1}"."tblInventoryBucket" ("ID", "Lower Limit", "Upper Limit", "Bucket Name", "Bucket Sort", "Nod", "DBName", "EntityName") VALUES
('1','-999999','-1','Adv. Issued','1','','DB1','E1'),
('2','0','30','0-30','2','','DB1','E1'),
('3','31','60','31-60','3','','DB1','E1'),
('4','61','90','61-90','4','','DB1','E1'),
('5','91','180','91-180','5','','DB1','E1'),
('6','181','999999','180+','6','','DB1','E1');


--
-- Data for Name: tblMasterFieldDefault; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO "{1}"."tblMasterFieldDefault" ("ID", "TableName", "FieldName", "ColumnLabel") VALUES
('1','SalesPerson','Name','Sales Person'),
('2','SalesPerson','Job Title','Job Title'),
('3','Customer','Name','Customer Name'),
('4','Customer','Chain Name','Customer Group Name'),
('5','Customer','Country_Region Code','Country Region Code'),
('6','Customer','City','Customer City'),
('7','Item','Description','Item Description'),
('8','Item','Inventory Posting Group','Inventory Posting Group'),
('9','Item','Item Category Code','Item Category'),
('10','Item','Product Group Code','Product Group'),
('11','Vendor','Name','Vendor Name'),
('12','Vendor','Vendor Posting Group','Vendor Posting Group');



--
-- Data for Name: tblMasterFieldDefaultAlias; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO "{1}"."tblMasterFieldDefaultAlias" ("ID", "TableName", "FieldName", "ColumnLabel") VALUES
('1','Customer','Country Region Code','Country Region Code'),
('2','Customer','Customer','Customer'),
('2','Customer','Customer City','Customer City'),
('2','Customer','Customer Group Name','Customer Group Name'),
('2','Customer','Customer Post Code','Customer Post Code'),
('6','Customer','Customer State Code','Customer State Code'),
('7','Customer','Link Customer','Link Customer'),
('8','Customer','Service Zone Code','Service Zone Code'),
('9','Item','Inventory Posting Group','Inventory Posting Group'),
('10','Item','Item Category','Item Category'),
('11','Item','Item Description','Item Description'),
('12','Item','Item Manufacturer','Item Manufacturer'),
('13','Item','ItemCategory','ItemCategory'),
('14','Item','Link Item','Link Item'),
('15','Item','Product Group','Product Group'),
('16','Item','ProductGroup','ProductGroup'),
('17','Item','Serial Nos_','Serial Nos_'),
('18','SalesPerson','Job Title','Job Title'),
('19','SalesPerson','Link Sales Rep','Link Sales Rep'),
('20','SalesPerson','Sales Person','Sales Person'),
('21','Vendor','Link Vendor','Link Vendor'),
('22','Vendor','Vendor City','Vendor City'),
('23','Vendor','Vendor Name','Vendor Name'),
('24','Vendor','Vendor Post Code','Vendor Post Code'),
('25','Vendor','Vendor Posting Group','Vendor Posting Group'),
('26','Vendor','Vendor State Code','Vendor State Code'),
('27','Location','Location Name','Location Name');


--
-- Data for Name: tblPDDBucket; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO "{1}"."tblPDDBucket" ("ID", "Min Limit", "Max Limit", "Bucket Name", "Bucket Sort","Nod", "DBName", "EntityName") VALUES
('1','-999999','-61','60+ Past','1','','DB1','E1'),
('2','-60','-31','31-60 Past','2','','DB1','E1'),
('3','-30','-15','15-30 Past','3','','DB1','E1'),
('4','-14','-8','8-14 Past','4','','DB1','E1'),
('5','-7','-1','1-7 Past','5','','DB1','E1'),
('6','0','7','0-7','6','','DB1','E1'),
('7','8','14','8-14','7','','DB1','E1'),
('8','15','30','15-30','8','','DB1','E1'),
('9','31','60','31-60','9','','DB1','E1'),
('10','61','999999','60+','10','','DB1','E1');



--
-- Data for Name: tblRatioFormula; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO "{1}"."tblRatioFormula" ("ID", "RatioName", "Denominator", "Numerator") VALUES
('1','Current Ratio','Current tax+Deferred tax*Finance costs','Depreciation and amortisation+Employee benefit expenses*Finance costs'),
('2','Debt to Equity Ratio',' ','Deferred tax- Profit & Loss'),
('3','Quick Ratio','Current tax- Profit & Loss+Finance costs- Profit & Loss','Deferred tax- Profit & Loss'),
('4','Ret. on Cap. Employed','',''),
('5','Ret. on Equity','',''),
('6','Ret. on Total Assets','',''),
('7','W. Cap. to Sales','Current tax- Profit & Loss+Deferred tax- Profit & Loss','Finance costs- Profit & LossTax expense- Profit & Loss');


--
-- Data for Name: "tblGLRangeCategory"; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO "{1}"."tblGLRangeCategory" ("CategoryValue", "CategoryText") VALUES
('REVENUE','Revenue'),
('DE','Direct Expense'),
('INDE','Indirect Expense'),
('EBIDTA','EBIDTA'),
('PBT','Profit Before Tax'),
('PAT','Profit After Tax'),
('Realised Gain Loss','Realised Gain Loss'),
('Tangible Assets','Tangible Assets'),
('Intangible Assets','Intangible Assets'),
('Customer','Customer'),
('Vendor','Vendor'),
('Sales Revenue','Sales Revenue'),
('Service Revenue','Service Revenue'),
('Cash & Cash Equivalents','Cash & Cash Equivalents');

--
-- Name: tblaccessdetails_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tblaccessdetails_id_seq', 6, true);


--
-- Name: tbladjustmententries_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tbladjustmententries_id_seq', 6, true);


--
-- Name: tblanalysisgroup_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tblanalysisgroup_id_seq', 1, false);


--
-- Name: tblanalysisgroupmapping_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tblanalysisgroupmapping_id_seq', 1, false);


--
-- Name: tblapbucket_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tblapbucket_id_seq', 1, false);


--
-- Name: tblarbucket_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tblarbucket_id_seq', 1, false);


--
-- Name: tblbudgetdimension_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tblbudgetdimension_id_seq', 17, true);


--
-- Name: tblbudgetfieldmapping_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tblbudgetfieldmapping_id_seq', 14, true);


--
-- Name: tblbudgetfieldnames_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tblbudgetfieldnames_id_seq', 8, true);


--
-- Name: tblbudgetmapping_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tblbudgetmapping_id_seq', 1, false);


--
-- Name: tblcompanyname_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tblcompanyname_id_seq', 4, true);


--
-- Name: tbldefaultratioflag_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tbldefaultratioflag_id_seq', 1, false);


--
-- Name: tbldimensioncode_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tbldimensioncode_id_seq', 28, true);


--
-- Name: tblfieldselection_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tblfieldselection_id_seq', 6, false);


--
-- Name: tblglaccountmapping_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tblglaccountmapping_id_seq', 26, true);


--
-- Name: tblglcodeexclusion_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tblglcodeexclusion_id_seq', 1, true);


--
-- Name: tblgrossingupparameter_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tblgrossingupparameter_id_seq', 18, true);


--
-- Name: tblinventorybucket_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tblinventorybucket_id_seq', 1, false);


--
-- Name: tblitemlistexclusion_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tblitemlistexclusion_id_seq', 1, false);


--
-- Name: tbllevelalias_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tbllevelalias_id_seq', 43, true);


--
-- Name: tblmasterfielddefault_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tblmasterfielddefault_id_seq', 6, false);


--
-- Name: tblmasterfielddefaultalias_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tblmasterfielddefaultalias_id_seq', 1, false);


--
-- Name: tblnavmasterfieldaddition_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tblnavmasterfieldaddition_id_seq', 1, false);


--
-- Name: tblpddbucket_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tblpddbucket_id_seq', 1, false);


--
-- Name: tblratioformula_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tblratioformula_id_seq', 1, false);


--
-- Name: tblratioheadermapping_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tblratioheadermapping_id_seq', 16, true);


--
-- Name: tblratiovalues_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tblratiovalues_id_seq', 110, true);


--
-- Name: tempadjustmentbulkupload_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tempadjustmentbulkupload_id_seq', 1, false);


--
-- Name: tempcompanynames_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tempcompanynames_id_seq', 24, true);


--
-- Name: tempdimensionbudget_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('"{1}".tempdimensionbudget_id_seq', 9628, true);

-------INSERT ENTRIES END----------








--
-- TOC entry 2449 (class 2606 OID 1001083)
-- Name: ConditionalMapping ConditionalMapping_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY "{1}"."ConditionalMapping"
    ADD CONSTRAINT "ConditionalMapping_pkey" PRIMARY KEY ("GLAccount");


--
-- TOC entry 2445 (class 2606 OID 16836)
-- Name: Login Login_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY "{1}"."Login"
    ADD CONSTRAINT "Login_pkey" PRIMARY KEY (id);


--
-- TOC entry 2447 (class 2606 OID 16838)
-- Name: TempCompanyNames tempcompanynames_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY "{1}"."TempCompanyNames"
    ADD CONSTRAINT tempcompanynames_pkey PRIMARY KEY (id);


-- Completed on 2021-05-25 14:16:42

--
-- PostgreSQL database dump complete
--