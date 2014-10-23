## WIA - Web Install Assistant

WIA is a simple command line tool that helps you setup web projects created with ASP.NET and EPiServer. 

### Why?

Setting up an existing EPiServer site on your computer takes a couple of minutes of your precious time, and if you have a lot of customer sites then this job can easily get repetitive. This tool was created to automate this process and save you some time.

### What does it do?

Currently WIA helps you do all the following tasks in a matter of seconds:

- Figure out what kind of project you have (versions etc).
- Create a **new Site in IIS** with appropriate configuration.
- Adds a site mapping in **EPiServerFramework.config** matching your new IIS Site.
- **Updates the HOSTS file** with an entry for the web project's URL.
- **Copies a license file** for EPiServer CMS to the web directory.
- Builds the solution and displays any errors that occurred.
- Pings the site so that it starts up.

WIA will always check first if it needs to run the task. This means you can run the `install` command multiple times without worrying that something gets messed up.

### Install procedure

1. Download the [latest version of WIA](https://github.com/nansen/wia/releases "Download the latest version of WIA") (zip).
2. Unpack the wia.exe file to a folder where you keep command tools.   
*If you do not have such folder:*
	1. Create a new folder at e.g.`C:\Tools`.
	2. Search for PATH in start menu.
	3. Open "Edit the system environment variables".
	4. Click on "Environment Variables" button.
	5. Find PATH in the bottom pane and edit it.
	6. Append `;C:\Tools` (note the beginning semicolon) and save the changes.
3. Open a new Command Prompt with administrator rights and run `wia`. If everything is right a help text will be displayed.
4. *Optional*: Add a menu item for opening a elevated command prompt in shift-right-click menu. You can do that by running the "Add-admin-cmd-to-right-click.reg" file.

### Usage

![Install procedure for an EPiServer site](https://github.com/nansen/wia/blob/master/docs/wia-install-01.Gif?raw=true)

### Support

- EPiServer CMS 5-7.5
- IIS 7 and later


### Future

- Investigate how much work it is to support EPiServer Commerce projects.

### Change log

See [Releases](https://github.com/nansen/wia/releases) for change log.