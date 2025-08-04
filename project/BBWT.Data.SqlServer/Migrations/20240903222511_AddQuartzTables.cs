using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddQuartzTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Drop existing Quartz tables if they exist
                IF OBJECT_ID(N'[dbo].[QRTZ_FIRED_TRIGGERS]', N'U') IS NOT NULL DROP TABLE [dbo].[QRTZ_FIRED_TRIGGERS];
                IF OBJECT_ID(N'[dbo].[QRTZ_PAUSED_TRIGGER_GRPS]', N'U') IS NOT NULL DROP TABLE [dbo].[QRTZ_PAUSED_TRIGGER_GRPS];
                IF OBJECT_ID(N'[dbo].[QRTZ_SCHEDULER_STATE]', N'U') IS NOT NULL DROP TABLE [dbo].[QRTZ_SCHEDULER_STATE];
                IF OBJECT_ID(N'[dbo].[QRTZ_LOCKS]', N'U') IS NOT NULL DROP TABLE [dbo].[QRTZ_LOCKS];
                IF OBJECT_ID(N'[dbo].[QRTZ_SIMPLE_TRIGGERS]', N'U') IS NOT NULL DROP TABLE [dbo].[QRTZ_SIMPLE_TRIGGERS];
                IF OBJECT_ID(N'[dbo].[QRTZ_SIMPROP_TRIGGERS]', N'U') IS NOT NULL DROP TABLE [dbo].[QRTZ_SIMPROP_TRIGGERS];
                IF OBJECT_ID(N'[dbo].[QRTZ_CRON_TRIGGERS]', N'U') IS NOT NULL DROP TABLE [dbo].[QRTZ_CRON_TRIGGERS];
                IF OBJECT_ID(N'[dbo].[QRTZ_BLOB_TRIGGERS]', N'U') IS NOT NULL DROP TABLE [dbo].[QRTZ_BLOB_TRIGGERS];
                IF OBJECT_ID(N'[dbo].[QRTZ_TRIGGERS]', N'U') IS NOT NULL DROP TABLE [dbo].[QRTZ_TRIGGERS];
                IF OBJECT_ID(N'[dbo].[QRTZ_JOB_DETAILS]', N'U') IS NOT NULL DROP TABLE [dbo].[QRTZ_JOB_DETAILS];
                IF OBJECT_ID(N'[dbo].[QRTZ_CALENDARS]', N'U') IS NOT NULL DROP TABLE [dbo].[QRTZ_CALENDARS];

                -- Create Quartz tables for SQL Server
                CREATE TABLE [dbo].[QRTZ_JOB_DETAILS] (
                    SCHED_NAME NVARCHAR(120) NOT NULL,
                    JOB_NAME NVARCHAR(200) NOT NULL,
                    JOB_GROUP NVARCHAR(200) NOT NULL,
                    DESCRIPTION NVARCHAR(250) NULL,
                    JOB_CLASS_NAME NVARCHAR(250) NOT NULL,
                    IS_DURABLE BIT NOT NULL,
                    IS_NONCONCURRENT BIT NOT NULL,
                    IS_UPDATE_DATA BIT NOT NULL,
                    REQUESTS_RECOVERY BIT NOT NULL,
                    JOB_DATA VARBINARY(MAX) NULL,
                    PRIMARY KEY (SCHED_NAME, JOB_NAME, JOB_GROUP)
                );

                CREATE TABLE [dbo].[QRTZ_TRIGGERS] (
                    SCHED_NAME NVARCHAR(120) NOT NULL,
                    TRIGGER_NAME NVARCHAR(200) NOT NULL,
                    TRIGGER_GROUP NVARCHAR(200) NOT NULL,
                    JOB_NAME NVARCHAR(200) NOT NULL,
                    JOB_GROUP NVARCHAR(200) NOT NULL,
                    DESCRIPTION NVARCHAR(250) NULL,
                    NEXT_FIRE_TIME BIGINT NULL,
                    PREV_FIRE_TIME BIGINT NULL,
                    PRIORITY INT NULL,
                    TRIGGER_STATE NVARCHAR(16) NOT NULL,
                    TRIGGER_TYPE NVARCHAR(8) NOT NULL,
                    START_TIME BIGINT NOT NULL,
                    END_TIME BIGINT NULL,
                    CALENDAR_NAME NVARCHAR(200) NULL,
                    MISFIRE_INSTR SMALLINT NULL,
                    JOB_DATA VARBINARY(MAX) NULL,
                    PRIMARY KEY (SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP),
                    FOREIGN KEY (SCHED_NAME, JOB_NAME, JOB_GROUP)
                    REFERENCES [dbo].[QRTZ_JOB_DETAILS](SCHED_NAME, JOB_NAME, JOB_GROUP)
                );

                CREATE TABLE [dbo].[QRTZ_SIMPLE_TRIGGERS] (
                    SCHED_NAME NVARCHAR(120) NOT NULL,
                    TRIGGER_NAME NVARCHAR(200) NOT NULL,
                    TRIGGER_GROUP NVARCHAR(200) NOT NULL,
                    REPEAT_COUNT BIGINT NOT NULL,
                    REPEAT_INTERVAL BIGINT NOT NULL,
                    TIMES_TRIGGERED BIGINT NOT NULL,
                    PRIMARY KEY (SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP),
                    FOREIGN KEY (SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP)
                    REFERENCES [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP)
                );

                CREATE TABLE [dbo].[QRTZ_CRON_TRIGGERS] (
                    SCHED_NAME NVARCHAR(120) NOT NULL,
                    TRIGGER_NAME NVARCHAR(200) NOT NULL,
                    TRIGGER_GROUP NVARCHAR(200) NOT NULL,
                    CRON_EXPRESSION NVARCHAR(120) NOT NULL,
                    TIME_ZONE_ID NVARCHAR(80) NULL,
                    PRIMARY KEY (SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP),
                    FOREIGN KEY (SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP)
                    REFERENCES [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP)
                );

                CREATE TABLE [dbo].[QRTZ_SIMPROP_TRIGGERS] (
                    SCHED_NAME NVARCHAR(120) NOT NULL,
                    TRIGGER_NAME NVARCHAR(200) NOT NULL,
                    TRIGGER_GROUP NVARCHAR(200) NOT NULL,
                    STR_PROP_1 NVARCHAR(512) NULL,
                    STR_PROP_2 NVARCHAR(512) NULL,
                    STR_PROP_3 NVARCHAR(512) NULL,
                    INT_PROP_1 INT NULL,
                    INT_PROP_2 INT NULL,
                    LONG_PROP_1 BIGINT NULL,
                    LONG_PROP_2 BIGINT NULL,
                    DEC_PROP_1 DECIMAL(13,4) NULL,
                    DEC_PROP_2 DECIMAL(13,4) NULL,
                    BOOL_PROP_1 BIT NULL,
                    BOOL_PROP_2 BIT NULL,
                    TIME_ZONE_ID NVARCHAR(80) NULL,
                    PRIMARY KEY (SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP),
                    FOREIGN KEY (SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP)
                    REFERENCES [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP)
                );

                CREATE TABLE [dbo].[QRTZ_BLOB_TRIGGERS] (
                    SCHED_NAME NVARCHAR(120) NOT NULL,
                    TRIGGER_NAME NVARCHAR(200) NOT NULL,
                    TRIGGER_GROUP NVARCHAR(200) NOT NULL,
                    BLOB_DATA VARBINARY(MAX) NULL,
                    PRIMARY KEY (SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP),
                    FOREIGN KEY (SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP)
                    REFERENCES [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP)
                );

                CREATE TABLE [dbo].[QRTZ_CALENDARS] (
                    SCHED_NAME NVARCHAR(120) NOT NULL,
                    CALENDAR_NAME NVARCHAR(200) NOT NULL,
                    CALENDAR VARBINARY(MAX) NOT NULL,
                    PRIMARY KEY (SCHED_NAME, CALENDAR_NAME)
                );

                CREATE TABLE [dbo].[QRTZ_PAUSED_TRIGGER_GRPS] (
                    SCHED_NAME NVARCHAR(120) NOT NULL,
                    TRIGGER_GROUP NVARCHAR(200) NOT NULL,
                    PRIMARY KEY (SCHED_NAME, TRIGGER_GROUP)
                );

                CREATE TABLE [dbo].[QRTZ_FIRED_TRIGGERS] (
                    SCHED_NAME NVARCHAR(120) NOT NULL,
                    ENTRY_ID NVARCHAR(140) NOT NULL,
                    TRIGGER_NAME NVARCHAR(200) NOT NULL,
                    TRIGGER_GROUP NVARCHAR(200) NOT NULL,
                    INSTANCE_NAME NVARCHAR(200) NOT NULL,
                    FIRED_TIME BIGINT NOT NULL,
                    SCHED_TIME BIGINT NOT NULL,
                    PRIORITY INT NOT NULL,
                    STATE NVARCHAR(16) NOT NULL,
                    JOB_NAME NVARCHAR(200) NULL,
                    JOB_GROUP NVARCHAR(200) NULL,
                    IS_NONCONCURRENT BIT NULL,
                    REQUESTS_RECOVERY BIT NULL,
                    PRIMARY KEY (SCHED_NAME, ENTRY_ID)
                );

                CREATE TABLE [dbo].[QRTZ_SCHEDULER_STATE] (
                    SCHED_NAME NVARCHAR(120) NOT NULL,
                    INSTANCE_NAME NVARCHAR(200) NOT NULL,
                    LAST_CHECKIN_TIME BIGINT NOT NULL,
                    CHECKIN_INTERVAL BIGINT NOT NULL,
                    PRIMARY KEY (SCHED_NAME, INSTANCE_NAME)
                );

                CREATE TABLE [dbo].[QRTZ_LOCKS] (
                    SCHED_NAME NVARCHAR(120) NOT NULL,
                    LOCK_NAME NVARCHAR(40) NOT NULL,
                    PRIMARY KEY (SCHED_NAME, LOCK_NAME)
                );

                -- Create necessary indexes
                CREATE INDEX IDX_QRTZ_J_REQ_RECOVERY ON [dbo].[QRTZ_JOB_DETAILS](SCHED_NAME, REQUESTS_RECOVERY);
                CREATE INDEX IDX_QRTZ_J_GRP ON [dbo].[QRTZ_JOB_DETAILS](SCHED_NAME, JOB_GROUP);

                CREATE INDEX IDX_QRTZ_T_J ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, JOB_NAME, JOB_GROUP);
                CREATE INDEX IDX_QRTZ_T_JG ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, JOB_GROUP);
                CREATE INDEX IDX_QRTZ_T_C ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, CALENDAR_NAME);
                CREATE INDEX IDX_QRTZ_T_G ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_GROUP);
                CREATE INDEX IDX_QRTZ_T_STATE ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_STATE);
                CREATE INDEX IDX_QRTZ_T_N_STATE ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP, TRIGGER_STATE);
                CREATE INDEX IDX_QRTZ_T_N_G_STATE ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_GROUP, TRIGGER_STATE);
                CREATE INDEX IDX_QRTZ_T_NEXT_FIRE_TIME ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, NEXT_FIRE_TIME);
                CREATE INDEX IDX_QRTZ_T_NFT_ST ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_STATE, NEXT_FIRE_TIME);
                CREATE INDEX IDX_QRTZ_T_NFT_MISFIRE ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, MISFIRE_INSTR, NEXT_FIRE_TIME);
                CREATE INDEX IDX_QRTZ_T_NFT_ST_MISFIRE ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_STATE, MISFIRE_INSTR, NEXT_FIRE_TIME);

                CREATE INDEX IDX_QRTZ_FT_TRIG_INST_NAME ON [dbo].[QRTZ_FIRED_TRIGGERS](SCHED_NAME, INSTANCE_NAME);
                CREATE INDEX IDX_QRTZ_FT_INST_JOB_REQ_RCVRY ON [dbo].[QRTZ_FIRED_TRIGGERS](SCHED_NAME, INSTANCE_NAME, REQUESTS_RECOVERY);
                CREATE INDEX IDX_QRTZ_FT_J_G ON [dbo].[QRTZ_FIRED_TRIGGERS](SCHED_NAME, JOB_NAME, JOB_GROUP);
                CREATE INDEX IDX_QRTZ_FT_JG ON [dbo].[QRTZ_FIRED_TRIGGERS](SCHED_NAME, JOB_GROUP);
                CREATE INDEX IDX_QRTZ_FT_T_G ON [dbo].[QRTZ_FIRED_TRIGGERS](SCHED_NAME, TRIGGER_GROUP);

                CREATE INDEX IDX_QRTZ_S_STATE_INST_NAME ON [dbo].[QRTZ_SCHEDULER_STATE](SCHED_NAME, INSTANCE_NAME);

                -- End of SQL Script
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Drop Quartz tables for SQL Server
                DROP TABLE IF EXISTS [dbo].[QRTZ_FIRED_TRIGGERS];
                DROP TABLE IF EXISTS [dbo].[QRTZ_PAUSED_TRIGGER_GRPS];
                DROP TABLE IF EXISTS [dbo].[QRTZ_SCHEDULER_STATE];
                DROP TABLE IF EXISTS [dbo].[QRTZ_LOCKS];
                DROP TABLE IF EXISTS [dbo].[QRTZ_SIMPLE_TRIGGERS];
                DROP TABLE IF EXISTS [dbo].[QRTZ_SIMPROP_TRIGGERS];
                DROP TABLE IF EXISTS [dbo].[QRTZ_CRON_TRIGGERS];
                DROP TABLE IF EXISTS [dbo].[QRTZ_BLOB_TRIGGERS];
                DROP TABLE IF EXISTS [dbo].[QRTZ_TRIGGERS];
                DROP TABLE IF EXISTS [dbo].[QRTZ_JOB_DETAILS];
                DROP TABLE IF EXISTS [dbo].[QRTZ_CALENDARS];
            ");
        }
    }
}
