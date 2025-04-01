# BiermanTech-Project-Manager

## TODO Items

- Introduce a Repository Pattern for Task Management: Separate data access into ITaskRepository.

- Implement the Command Pattern for Undo/Redo: Use ICommand and CommandManager for structured undo/redo.

- Use Dependency Injection (DI) for Services: Inject dependencies like ITaskRepository and IDialogService.

- Introduce a Dialog Service for TaskDialog: Abstract dialog interactions with IDialogService.

- Extract Gantt Chart Rendering Logic into a Service: Move rendering logic to GanttChartRenderer.

- Use a Factory Pattern for Command Creation: Centralize command creation with ICommandFactory.

- Add Input Validation in TaskDialogViewModel: Validate input and control SaveCommand’s CanExecute.

- Use MVVM Messaging for Cross-ViewModel Communication: Add IMessageBus for event-driven communication.

- Extract Constants and Configuration: Centralize hardcoded values in a config class.

- Add Unit Tests for ViewModels and Services: Ensure code quality with unit tests.