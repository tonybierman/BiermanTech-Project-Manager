# BiermanTech Project Manager

BiermanTech Project Manager is a desktop application for managing projects using a Gantt chart interface. Built with Avalonia (https://avaloniaui.net/) and ReactiveUI (https://www.reactiveui.net/), it provides a cross-platform solution for creating, updating, and visualizing tasks with dependencies, start dates, durations, and progress tracking. The application includes business rules to ensure logical scheduling, such as preventing a dependent task from starting before the task it depends on.

## Features

- **Gantt Chart Visualization**:
  - View tasks on a timeline with a header showing months (on the first of each month) and day numbers.
  - Displays task dependencies with arrows connecting dependent tasks.
  - Highlights the current date with a dashed red line for better context.

- **Task Management**:
  - Add, update, and delete tasks via the "Task" menu or keyboard shortcuts.
  - Set task properties: name, start date, duration, percent complete, and dependencies.
  - Tasks are listed alongside the Gantt chart for easy selection and management.

- **Project Management**:
  - Create a new project, clearing all tasks and resetting metadata (File > New Project).
  - Load a project from a `.json` file, including metadata and tasks (File > Load Project).
  - Save the current project to a `.json` file, preserving metadata and tasks (File > Save Project).
  - Project metadata includes project name and author, displayed in the status bar.

- **Business Rules**:
  - A task cannot start earlier than the task it depends on.
  - Validation ensures required fields (e.g., task name, start date) are set, with errors displayed in the task dialog.

- **Undo/Redo**:
  - Support for undoing and redoing task operations (add, update, delete) and project operations (new, load, save).
  - Accessible via the "Edit" menu or keyboard shortcuts (Alt+E, U for Undo; Alt+E, R for Redo).

- **Status Bar Notifications**:
  - Displays project metadata (name and author) at the bottom of the window.
  - Shows temporary status messages for actions (e.g., "Task 'Planning' added", "Project saved to project.json").
  - Notifications clear automatically after 5 seconds.

- **Cross-Platform**:
  - Runs on Windows, macOS, and Linux thanks to Avalonia.

- **Reactive UI**:
  - Built with ReactiveUI for a responsive and maintainable user interface.
  - Ensures real-time updates to the task list and Gantt chart when tasks are modified.

- **Logging**:
  - Uses Serilog to log application events and errors to a file (`logs/log-.txt`).
  - Includes detailed logs for task operations, project actions, and errors for debugging.

Screenshots

[Gantt Chart View](screenshot.png)

Prerequisites

- .NET 8 SDK: Ensure you have the .NET 8 SDK installed. Download here: https://dotnet.microsoft.com/en-us/download/dotnet/8.0
- Git: To clone the repository.
- IDE: Visual Studio, Rider, or VS Code with C# extensions (optional but recommended).

Setup Instructions

1. Clone the Repository:
   git clone https://github.com/tonybierman/BiermanTech-Project-Manager.git
   cd BiermanTech-Project-Manager

2. Restore Dependencies:
   The project uses NuGet packages. Restore them by running:
   dotnet restore

3. Build the Project:
   Build the application to ensure everything is set up correctly:
   dotnet build

4. Run the Application:
   Launch the application:
   dotnet run --project BiermanTech.ProjectManager

   The application window should open, displaying the Gantt chart and task list.

## Usage

1. **View Tasks**:
   - The main window displays a Gantt chart with a timeline header.
   - The header shows month names (e.g., "April") on the first day of each month, with day numbers (e.g., "01", "02") below.
   - Tasks are listed on the left, with their timelines visualized on the right.

2. **Add a Task**:
   - Navigate to the "Task" menu and select "Add Task", or use the keyboard shortcut (Alt+T, A).
   - In the dialog, enter the task details:
     - **Task Name**: Required.
     - **Start Date**: Select a date using the date picker.
     - **Duration (days)**: Enter the number of days the task will take.
     - **Depends On**: Optionally select a task that this task depends on.
   - Click "Save" to add the task, or "Cancel" to discard.
   - A status message (e.g., "Task 'Planning' added.") will appear in the status bar at the bottom of the window.

3. **Update a Task**:
   - Select a task in the list or Gantt chart.
   - Navigate to the "Task" menu and select "Update Task", or use the keyboard shortcut (Alt+T, U).
   - Modify the task details in the dialog and click "Save".
   - A status message (e.g., "Task 'Planning' updated.") will appear in the status bar.

4. **Delete a Task**:
   - Select a task in the list or Gantt chart.
   - Navigate to the "Task" menu and select "Delete Task", or use the keyboard shortcut (Alt+T, D).
   - The task will be removed, and any dependent tasks will have their dependencies cleared.
   - A status message (e.g., "Task 'Planning' deleted.") will appear in the status bar.

5. **Undo/Redo**:
   - Navigate to the "Edit" menu and select "Undo" (Alt+E, U) or "Redo" (Alt+E, R) to revert or reapply changes.
   - The "Undo" and "Redo" options are enabled only when there are actions to undo or redo.
   - A status message (e.g., "Undo performed.") will appear in the status bar.

6. **Manage Projects**:
   - **New Project**:
     - Navigate to the "File" menu and select "New Project", or use the keyboard shortcut (Alt+F, N).
     - This clears the current project and tasks, setting the project name to "New Project" and the author to "Unknown".
     - A status message (e.g., "New project created.") will appear in the status bar.
   - **Load Project**:
     - Navigate to the "File" menu and select "Load Project", or use the keyboard shortcut (Alt+F, L).
     - A file picker dialog will open. Select a `.json` file containing a project (e.g., `project.json`).
     - The project (including metadata and tasks) will be loaded, updating the UI.
     - A status message (e.g., "Project loaded from project.json.") will appear in the status bar.
   - **Save Project**:
     - Navigate to the "File" menu and select "Save Project", or use the keyboard shortcut (Alt+F, S).
     - A save file dialog will open. Choose a location and file name (e.g., `project.json`).
     - The current project (metadata and tasks) will be saved to the specified file.
     - A status message (e.g., "Project saved to project.json.") will appear in the status bar.

7. **Business Rules**:
   - If a task depends on another task, its start date cannot be earlier than the depended task’s start date.
   - Validation errors are displayed in the task dialog, and the "Save" button is disabled until the errors are resolved.

Project Structure

- BiermanTech.ProjectManager:
  - Controls/: Contains the GanttChartControl for rendering the Gantt chart.
  - Models/: Defines the TaskItem model and ITaskRepository interface.
  - Services/: Includes services like InMemoryTaskRepository, DialogService, and MessageBus.
  - ViewModels/: Contains view models like MainWindowViewModel and TaskDialogViewModel.
  - Views/: Contains XAML files for the UI, such as MainWindow.axaml and TaskDialog.axaml.
  - Commands/: Implements the command pattern for task operations (e.g., AddTaskCommand, UpdateTaskCommand).
  - logs/: Directory where Serilog writes log files (log-.txt).

Dependencies

- Avalonia: Cross-platform UI framework.
- ReactiveUI: Reactive programming framework for MVVM.
- Serilog: Logging library for structured logging.
- Microsoft.Extensions.DependencyInjection: For dependency injection.

Contributing

Contributions are welcome! To contribute:

1. Fork the repository.
2. Create a new branch (git checkout -b feature/your-feature).
3. Make your changes and commit them (git commit -m "Add your feature").
4. Push to your branch (git push origin feature/your-feature).
5. Open a pull request.

Please ensure your code follows the existing style and includes tests where applicable.

Known Issues

- Performance: Rendering a large number of tasks may impact performance. Consider optimizing the Gantt chart rendering for large datasets.
- Validation UI: Validation errors are displayed as text; consider adding visual indicators (e.g., red borders) for invalid fields.

Future Enhancements

- Add more business rules (e.g., maximum duration, percent complete validation).
- Implement drag-and-drop to adjust task start dates and durations in the Gantt chart.
- Enhance the UI with themes and better styling.

License

This project is licensed under the MIT License. See the LICENSE file for details.

Contact

For questions or support, please contact Tony Bierman or open an issue on GitHub.