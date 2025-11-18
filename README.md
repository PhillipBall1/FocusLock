# FocusLock
FocusLock is a lightweight, easy to use Windows application created to help users stay on task by blocking user selected distractions from opening while working on tasks. This application keeps you accountable while being out of the way.

### Features
- **Focus Mode**: Start a timed focus session or allow a task to start it for you. While active, the application will block anything selected as a distracting application.
- **Task Management**: Create tasks with titles, start times, and durations. Tasks will auto remove on completion unless set to recurring.  
- **Distraction Controls**: View all currently open applications and select the apps that are considered distracting. This will only display applications that are currently open.  
- **Distraction Tracking**: While FocusLock is open, any application that the user selected as distracting will be tracked with how much time the user has it open for. This tracking will only commence if the application is open and actively displayed to the user. 
- **Clean, SImple UI**: The UI is made to be as simple and easy to pick up as possible. 

### How it works
While FocusLock is running, it is actively checking processes that are running and comparing them to what the user has selected to be distracting. If the process tracking picks up that the user started something that is considered distracting during active focus, then the application gets terminated. 

Process tracking only does two things:
- Check if the application is open
- Check if that application is considered a distraction during focus

### Tech Stack
- Language: C#
- Framework: WPF (.NET 8)
- Storage: Local JSON
- UI: XAML

### Installation
1. Go to this repositories 'Releases' section
2. Download the 'FocusLock.zip' folder
3. Unpack it in the desired location
4. Open the 'publish' folder
5. Run the 'setup' file
6. Install the application

### Contributions
This project was created as a capstone for my college graduation. Any feedback or suggestions would be great, but further work on this project is still to be determined.
