AggregatedLogs module allows to enable server, client and web server logging into MSSQL, MySQL or PostgreSQL database.

In order to enable server and client logging, please, set AggregatedLogsSettings Enabdled flag to true in appsettings.json
and reference this module in BBWT.Server. This logging is done in batches, their maximum size and timespan in seconds are also configurable.

Saving web server logs to database is done with AWS EventBridge job WebServerLogJob. If your project requires this functionality add a job
with preferable schedule in UI. Folder to get logs from, application name and source name (IIS, nginx) can be configured in WebServerLogsSettings.
Please, note that log files have to be in Common Log format (NCSA).

PostgreSQL was deemed to be the best choice because of high performance when inserting entities.


