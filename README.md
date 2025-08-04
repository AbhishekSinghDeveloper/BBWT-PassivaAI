# Blueberry Web Template v3.10.0

**master:**   [![pipeline status](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/badges/master/pipeline.svg)](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/commits/master)
**test:**     [![pipeline status](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/badges/test/pipeline.svg)](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/commits/test)
**develop:**  [![pipeline status](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/badges/develop/pipeline.svg)](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/commits/develop)

This is the 3rd generation of the Blueberry Web Template web application framework, enabling powerful experiences across the web.

View deployed environments [here](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/environments).

This is a cross-platform **.NET 7.0** project, with an **Angular 17** front-end.

## Forking

If you are forking a new project based off of BBWT3, make sure that 3rd party modules are placed in a separate folder
and not placed within **./modules** folder. The recommended name of the folder for these modules is **./thirdparty**.
The reason we do this is because of code audits with tools such as SonarQube.

## Cleaning up Modules

If you need to remove the Demo module presented with [back-end part](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/-/tree/develop/modules%2FBBWM.Demo)
and [front-end part](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/-/tree/develop/project%2FBBWT.Client%2Fsrc%2Fapp%2Fmain%2Fdemo)
then find [remove_demo.bat](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/-/blob/develop/scripts/CleanupForDownstream/remove_demo.bat)/
[remove_demo.sh](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/-/blob/develop/scripts/CleanupForDownstream/remove_demo.sh) script and run it in your local repository.
The script removes files locally and also tracks these changes in git. Finally after running the script you just need to commit and push the changes to server.

**NOTE WELL: WHEN PROJECT CODE IS CONSIDERED TO BE USED FOR LIVE ENVIRONMENTS, IT'S STRONGLY RECOMMENDED TO REMOVE THE DEMO CODE COMPLETELY TO AVOID POTENTIAL ISSUES ABOUT REDUNDANT SOURCE LOADING AND POSSIBLE API SECURITY ISSUES.**


## Core Update History
| Release | Back-end Core | Front-end Core | PrimeNG | Updated On |
|-----------------|-------------|---------------|---------------|---------------|
||.NET8||| Scheduled: 2024 December |
| v3.10.0|| Angular 17.1.2 ([commit](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/-/commit/3dbc090e840c2157049d0b48109dfbd28e51c049))| PrimeNG 17.0.0 | 2024 Feb 15 |
| [v3.9.0](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/-/releases/stable-march24-v3.9.0)| .NET7 ([commit](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/-/commit/02398edd8851599e08d42f4b042f876908b65267))| Angular 15.2.8 ([commit](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/-/commit/02398edd8851599e08d42f4b042f876908b65267))| PrimeNG 15.4.1 | 2023 Jun 23 |
|[v3.8.0](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/-/releases/stable-february22-v3.8.0)|.NET6|Angular 13.2.0|PrimeNG 13.1.0| 2022 Feb 24 |
|[v3.7.0](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/-/releases/stable-december21-v3.7.0)|.NET5|Angular 12.2.7|PrimeNG 12.1.1| 2022 Jan 18 |
|[v3.6.0](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/-/releases/stable-august21-v3.6.0)||Angular 11.1.0|PrimeNG 11.2.0| 2021 Aug 12 |
|[v3.5.0](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/-/releases/stable-december20-v3.5.0)||Angular 10.1.6|PrimeNG 10.0.3| 2021 Jan 04 |
|[v3.4.0](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/-/releases/stable-august20-v3.4.0)||Angular 9.1.7 ([commit](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/-/commit/e8510f52fa7e0aa338f977aae7588fb34693f77c))|PrimeNG 9.1.3| 2020 Aug 26 |
|[v3.3.0](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/-/releases/stable-may20-v3.3.0)|Core 3.1 ([commit](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/-/commit/6063ed9d68a445e80a1ebd206dd0546cebcc7e41))||| 2020 May 06 |

## Wiki Documentation

The wiki documents everything from which editor to use, with links to download as well as a complete
[tutorial](https://wiki.bbconsult.co.uk/pages/viewpage.action?pageId=65077466) of how to set up your environment.

[Another](https://wiki.bbconsult.co.uk/pages/viewpage.action?pageId=65077468) tutorial is available to make sure you
get a working copy of the current state of BBWT3. The links provided will fill you in so that you can
start coding for a BBWT3 project.

### Useful Links in the Wiki:

##### Common
- [Getting Started](https://wiki.bbconsult.co.uk/pages/viewpage.action?pageId=65077464)
- [Setting up a Development Environment](https://wiki.bbconsult.co.uk/pages/viewpage.action?pageId=65077466)
- [Building, Running & Publishing](https://wiki.bbconsult.co.uk/pages/viewpage.action?pageId=65077468)
- [Databases and Migrations](https://wiki.bbconsult.co.uk/display/BLUEB/Databases+and+Migrations)
- [Useful Configuration Keys](https://wiki.bbconsult.co.uk/display/BLUEB/Useful+Configuration+Keys)
- [Angular CLI](https://wiki.bbconsult.co.uk/display/BLUEB/Angular+CLI)
- [Forking for a New Project](https://wiki.bbconsult.co.uk/display/BLUEB/Forking+for+a+New+Project)

##### Coding
- [BBWT3 Explained to Beginners](https://wiki.bbconsult.co.uk/display/BLUEB/BBWT3+Explained+to+Beginners)
- [BB Grid Component (front-end)](https://wiki.bbconsult.co.uk/display/BLUEB/BB+Grid+Component)
- [BB Filter Component (front-end)](https://wiki.bbconsult.co.uk/display/BLUEB/BB+Filter+Component)
- [Unit Testing](https://wiki.bbconsult.co.uk/pages/viewpage.action?pageId=67174474)

If you do not have an account set up to view the wiki,
you can find login credentials within [Bitwarden](https://bw.bbconsult.co.uk).

## Coding Tips
### Incomplete code
If you have any incomplete code parts committed to repository, do add a comment in code in format:
```
// TODO: #[Task ID] [short description of the problem to be completed]
```
This is a good rule for all the projects, though, in particular, this is important for BBWT3 R&D developers team. Following the rule you reduce code misunderstanding in downstream projects!
