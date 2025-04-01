# BiermanTech Project Manager

BiermanTech Project Manager is a desktop application for managing projects using a Gantt chart interface. Built with Avalonia (https://avaloniaui.net/) and ReactiveUI (https://www.reactiveui.net/), it provides a cross-platform solution for creating, updating, and visualizing tasks with dependencies, start dates, durations, and progress tracking. The application includes business rules to ensure logical scheduling, such as preventing a dependent task from starting before the task it depends on.

Features

- Gantt Chart Visualization: View tasks on a timeline with a header showing months (on the first of each month) and day numbers.
- Task Management:
  - Add, update, and delete tasks.
  - Set task properties: name, start date, duration, percent complete, and dependencies.
- Business Rules:
  - A task cannot start earlier than the task it depends on.
  - Validation ensures required fields (e.g., task name, start date) are set.
- Undo/Redo: Support for undoing and redoing task operations.
- Cross-Platform: Runs on Windows, macOS, and Linux thanks to Avalonia.
- Reactive UI: Built with ReactiveUI for a responsive and maintainable user interface.
- Logging: Uses Serilog to log application events and errors to a file (logs/log-.txt).

Screenshots

(You can add screenshots here by taking images of the application and placing them in a screenshots folder. For example:)
[Gantt Chart View](screenshots/gantt-chart.png)
[Task Dialog](screenshots/task-dialog.png)

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

Usage

1. View Tasks:
   - The main window displays a Gantt chart with a timeline header.
   - The header shows month names (e.g., "April") on the first day of each month, with day numbers (e.g., "01", "02") below.
   - Tasks are listed on the left, with their timelines visualized on the right.

2. Add a Task:
   - Click the "Add Task" button.
   - In the dialog, enter the task details:
     - Task Name: Required.
     - Start Date: Select a date using the date picker.
     - Duration (days): Enter the number of days the task will take.
     - Depends On: Optionally select a task that this task depends on.
   - Click "Save" to add the task, or "Cancel" to discard.

3. Update a Task:
   - Select a task in the list or Gantt chart.
   - Click the "Update Task" button.
   - Modify the task details in the dialog and click "Save".

4. Delete a Task:
   - Select a task and click the "Delete Task" button.
   - The task will be removed, and any dependent tasks will have their dependencies cleared.

5. Undo/Redo:
   - Use the "Undo" and "Redo" buttons to revert or reapply changes.

6. Business Rules:
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
- Add export/import functionality for project data.
- Enhance the UI with themes and better styling.

License

This project is licensed under the MIT License. See the LICENSE file for details.

Contact

For questions or support, please contact Tony Bierman or open an issue on GitHub.