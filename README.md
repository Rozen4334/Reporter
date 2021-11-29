# 🌲 Reporter
A Sandbox game reporting bot, specifically targetting Terraria, managed and hosted by me.

## 🔗 Hosting
I provide hosting for this application for free. 
*You can contact me at ` contact.rozen@one ` to have me host an instance of Reporter for you.*

> ⚠️ I can see much of what happens in servers by the command line.
> Abuse of this application can cause me to withdraw the service and block you from seeking any further contact.

#### Hosting yourself:
To host this bot yourself, you need good knowledge of how Discord app development works, specifically targetted at guilds. If you have some confidence here, and want to host this for yourself, feel free to. You can contact me at ` contact.rozen@one ` for questions, or send me a friend request in Discord: ` Rozen#0001 `

## Related:
- [Discord Developer Portal](https://discord.com/developers/applications) - Creating a token, setting up O2Auth & configuring gateway permissions.
- [Discord.NET-Labs](https://github.com/Discord-Net-Labs/Discord.Net-Labs) - Framework for this application.

### Code structure:
*If a feature does not belong to a class, something has not been written properly.*

> *If a feature belongs to multiple classes, apply SOLID to figure out the overhead interface.*
> *Stash commits, do not repeatedly commit.*
> *Only update a version if all notable bugs are gone.*
> *Only update version through assemblyversion.*

---

## 📇 Examples:

### 👁️‍🗨️ View all reports by offender.
![image](https://rozen.one/files/Discord_cL4RsZtPqY.png)
### 🔢 View a report by ID.
![image](https://rozen.one/files/Discord_eRakz6jX87.png)
### 📜 View all reports.
![image](https://rozen.one/files/Discord_yff3xm6Ic2.png)

> More functions like listing moderator actions are also included.

## 👩🏿‍💻 Commands:

` /reporterinfo ` 
- Shows your (or another staff members') report count. 

` /reportinfo ` 
- Displays the report by **ID** and gets all info on it.

` /reports ` 
- Gets a list of all reports.

` /playerinfo ` 
- Gets info on a username.

` /report ` 
- Reports a user based on provided info.

` @Reporter addimage ID (attachments) ` 
- Adds images to the report by ID. Buttons will show the images on a report through ` /reportinfo `.

` @Reporter addimage ID (image links) `
- Adds images to the report by links, i.e. FTP or DiscordApp links.
